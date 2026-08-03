[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$PackageRoot,
    [Parameter(Mandatory = $true)][string]$SbomRoot,
    [Parameter(Mandatory = $true)][string]$ChangeControlPath,
    [Parameter(Mandatory = $true)][ValidatePattern('^[0-9a-f]{40}$')][string]$SourceCommit,
    [Parameter(Mandatory = $true)][string]$PackageVersion,
    [Parameter(Mandatory = $true)][ValidatePattern('^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}Z$')][string]$Created,
    [Parameter(Mandatory = $true)][string]$PublicationManifestPath,
    [string]$ExpectedOwner = "GuojinYan",
    [string]$OutputPath = "",
    [switch]$Check
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
Add-Type -AssemblyName System.IO.Compression.FileSystem

$repositoryUrl = "https://github.com/guojin-yan/OpenCV-CSharp-API"
$serviceIndex = "https://api.nuget.org/v3/index.json"
$normalizedTimestamp = [DateTimeOffset]::new(2000, 1, 1, 0, 0, 0, [TimeSpan]::Zero)

function Get-BytesSha256 {
    param([byte[]]$Bytes)
    return [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($Bytes)).ToLowerInvariant()
}

function Read-ZipEntryBytes {
    param([IO.Compression.ZipArchiveEntry]$Entry)
    $stream = $Entry.Open()
    try {
        $memory = [IO.MemoryStream]::new()
        try { $stream.CopyTo($memory); return $memory.ToArray() }
        finally { $memory.Dispose() }
    }
    finally { $stream.Dispose() }
}

function Get-NuspecValue {
    param([xml]$Nuspec,[string]$Name)
    $node = $Nuspec.SelectSingleNode("/*[local-name()='package']/*[local-name()='metadata']/*[local-name()='$Name']")
    if ($null -eq $node) { return "" }
    return $node.InnerText.Trim()
}

function Get-PackageFact {
    param([string]$Path,[string]$ExpectedId,[string]$ExpectedHash)
    $file = Get-Item -LiteralPath $Path
    $hash = (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
    if ($hash -ne $ExpectedHash) { throw "Package hash mismatch for ${ExpectedId}: actual=$hash expected=$ExpectedHash" }

    $archive = [IO.Compression.ZipFile]::OpenRead($file.FullName)
    try {
        $entries = @($archive.Entries | Where-Object { -not $_.FullName.EndsWith('/') })
        $names = @($entries | ForEach-Object { $_.FullName.Replace('\', '/') })
        if (@($names | Where-Object { $_ -eq '.signature.p7s' }).Count -ne 0) { throw "Prepublication package must be unsigned: $($file.Name)" }
        if (@($names | Group-Object { $_.ToLowerInvariant() } | Where-Object Count -gt 1).Count -ne 0) { throw "Package contains duplicate or case-colliding entries: $($file.Name)" }
        if (@($archive.Entries | Where-Object { $_.LastWriteTime.DateTime -ne $normalizedTimestamp.DateTime }).Count -ne 0) { throw "Package entries are not deterministically timestamped: $($file.Name)" }

        $nuspecEntries = @($entries | Where-Object { $_.FullName.EndsWith('.nuspec', [StringComparison]::OrdinalIgnoreCase) })
        if ($nuspecEntries.Count -ne 1) { throw "Package must contain exactly one nuspec: $($file.Name)" }
        $nuspecStream = [IO.MemoryStream]::new((Read-ZipEntryBytes -Entry $nuspecEntries[0]), $false)
        try {
            [xml]$nuspec = [Xml.XmlDocument]::new()
            $nuspec.Load($nuspecStream)
        }
        finally { $nuspecStream.Dispose() }
        $id = Get-NuspecValue -Nuspec $nuspec -Name "id"
        $version = Get-NuspecValue -Nuspec $nuspec -Name "version"
        if ($id -cne $ExpectedId -or $version -cne $PackageVersion) { throw "Package identity mismatch: actual=$id/$version expected=$ExpectedId/$PackageVersion" }
        $repository = $nuspec.SelectSingleNode("/*[local-name()='package']/*[local-name()='metadata']/*[local-name()='repository']")
        if ($null -eq $repository -or $repository.GetAttribute('url') -cne $repositoryUrl -or $repository.GetAttribute('commit') -cne $SourceCommit) {
            throw "Package repository provenance mismatch: $($file.Name)"
        }

        $entryFacts = @($entries | ForEach-Object {
                $bytes = Read-ZipEntryBytes -Entry $_
                [ordered]@{ Path = $_.FullName.Replace('\', '/'); Bytes = $bytes.LongLength; Sha256 = Get-BytesSha256 -Bytes $bytes }
            } | Sort-Object Path)
        return [ordered]@{
            Id = $id
            Version = $version
            FileName = $file.Name
            Bytes = $file.Length
            Sha256 = $hash
            EntryCount = $entryFacts.Count
            PayloadCanonicalSha256 = Get-BytesSha256 -Bytes ([Text.UTF8Encoding]::new($false).GetBytes((($entryFacts | ForEach-Object { "$($_.Path)`0$($_.Bytes)`0$($_.Sha256)" }) -join "`n") + "`n"))
        }
    }
    finally { $archive.Dispose() }
}

function Get-SbomFact {
    param([string]$Path,[string]$ExpectedPackageId,[string]$ExpectedPackageSha256)
    $file = Get-Item -LiteralPath $Path
    $document = Get-Content -LiteralPath $file.FullName -Raw | ConvertFrom-Json
    $mainPackage = @($document.packages | Where-Object { $_.name -eq $ExpectedPackageId })
    if ($document.spdxVersion -ne 'SPDX-2.3' -or $mainPackage.Count -ne 1) { throw "SBOM identity mismatch: $($file.Name)" }
    $packageChecksum = @($mainPackage[0].checksums | Where-Object algorithm -eq 'SHA256')
    if ($packageChecksum.Count -ne 1 -or $packageChecksum[0].checksumValue -ne $ExpectedPackageSha256) { throw "SBOM package hash mismatch: $($file.Name)" }
    return [ordered]@{
        FileName = $file.Name
        Bytes = $file.Length
        Sha256 = (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
        PackageId = $ExpectedPackageId
        PackageSha256 = $ExpectedPackageSha256
        FileCount = @($document.files).Count
    }
}

$resolvedPackageRoot = (Resolve-Path -LiteralPath $PackageRoot).Path
$resolvedSbomRoot = (Resolve-Path -LiteralPath $SbomRoot).Path
$resolvedChangeControl = (Resolve-Path -LiteralPath $ChangeControlPath).Path
$resolvedManifest = (Resolve-Path -LiteralPath $PublicationManifestPath).Path
pwsh -NoProfile -File (Join-Path $PSScriptRoot 'Test-NuGetPublicationManifest.ps1') `
    -ManifestPath $resolvedManifest -SourceCommit $SourceCommit -PackageVersion $PackageVersion `
    -OutputPath $resolvedManifest -Check
if ($LASTEXITCODE -ne 0) { throw 'Publication manifest validation failed.' }
$manifest = Get-Content -LiteralPath $resolvedManifest -Raw | ConvertFrom-Json
$definitions = @($manifest.Packages | ForEach-Object {
        [pscustomobject]@{
            Id = [string]$_.PackageId
            File = [string]$_.PackageFile
            Hash = [string]$_.Sha256
            Sbom = [string]$_.SbomFile
            ArtifactName = [string]$_.ArtifactName
            RunId = [string]$_.RunId
            Rid = [string]$_.Rid
            RuntimeProfile = [string]$_.RuntimeProfile
            Kind = [string]$_.Kind
        }
    } | Sort-Object Id)
$actualPackages = @(Get-ChildItem -LiteralPath $resolvedPackageRoot -Filter *.nupkg -File)
if ($actualPackages.Count -ne $definitions.Count) { throw "Publication bundle package closure mismatch: expected=$($definitions.Count) actual=$($actualPackages.Count)" }
$expectedPackageFiles = @($definitions.File | Sort-Object)
$actualPackageFiles = @($actualPackages.Name | Sort-Object)
if (($actualPackageFiles -join "`n") -cne ($expectedPackageFiles -join "`n")) { throw 'Publication bundle package filenames do not match the manifest.' }
$actualSbomFiles = @(Get-ChildItem -LiteralPath $resolvedSbomRoot -Filter *.json -File)
$expectedSbomFiles = @($definitions.Sbom | Sort-Object)
if ($actualSbomFiles.Count -ne $definitions.Count -or (@($actualSbomFiles.Name | Sort-Object) -join "`n") -cne ($expectedSbomFiles -join "`n")) {
    throw 'Publication bundle SBOM filenames do not match the manifest.'
}

$packages = @($definitions | ForEach-Object {
        $fact = Get-PackageFact -Path (Join-Path $resolvedPackageRoot $_.File) -ExpectedId $_.Id -ExpectedHash $_.Hash
        [ordered]@{
            Id = $fact.Id
            Version = $fact.Version
            FileName = $fact.FileName
            Bytes = $fact.Bytes
            Sha256 = $fact.Sha256
            EntryCount = $fact.EntryCount
            PayloadCanonicalSha256 = $fact.PayloadCanonicalSha256
            Kind = $_.Kind
            Rid = $_.Rid
            RuntimeProfile = $_.RuntimeProfile
            ArtifactName = $_.ArtifactName
            PackRunId = $_.RunId
        }
    })
$sboms = @($definitions | ForEach-Object { Get-SbomFact -Path (Join-Path $resolvedSbomRoot $_.Sbom) -ExpectedPackageId $_.Id -ExpectedPackageSha256 $_.Hash })
$changeControlFile = Get-Item -LiteralPath $resolvedChangeControl
$changeControl = Get-Content -LiteralPath $changeControlFile.FullName -Raw | ConvertFrom-Json
if ($changeControl.SourceRevision -ne $SourceCommit -or $changeControl.SigningStatus -ne 'repository-signing-pending' -or $changeControl.SbomStatus -ne 'generated-unapproved' -or $changeControl.Approver.Status -ne 'not-approved' -or [bool]$changeControl.Publication.Allowed) {
    throw "Change-control state is not ready for NuGet.org repository-signing review."
}
$recordPackageHashes = @($changeControl.Packages | Sort-Object PackageId | ForEach-Object { "$($_.PackageId)|$($_.Sha256)" })
$expectedPackageHashes = @($packages | Sort-Object Id | ForEach-Object { "$($_.Id)|$($_.Sha256)" })
if (($recordPackageHashes -join "`n") -cne ($expectedPackageHashes -join "`n")) { throw "Change-control package closure does not match publication bundle." }

$canonical = @(
    "source=$SourceCommit",
    "created=$Created",
    "version=$PackageVersion",
    "owner=$ExpectedOwner",
    "service=$serviceIndex",
    "change-control=$((Get-FileHash -LiteralPath $resolvedChangeControl -Algorithm SHA256).Hash.ToLowerInvariant())",
    "manifest=$((Get-FileHash -LiteralPath $resolvedManifest -Algorithm SHA256).Hash.ToLowerInvariant())"
) + @($packages | Sort-Object Id | ForEach-Object { "package=$($_.Id)|$($_.PackRunId)|$($_.ArtifactName)|$($_.Bytes)|$($_.Sha256)|$($_.PayloadCanonicalSha256)" }) + @($sboms | Sort-Object PackageId | ForEach-Object { "sbom=$($_.PackageId)|$($_.Bytes)|$($_.Sha256)|$($_.PackageSha256)" })
$candidateHash = Get-BytesSha256 -Bytes ([Text.UTF8Encoding]::new($false).GetBytes(($canonical -join "`n") + "`n"))
$record = [ordered]@{
    SchemaVersion = 2
    RecordKind = 'nuget-publication-bundle'
    CandidateId = "nuget-publication/sha256/$candidateHash"
    AuthorizationToken = "publish-nuget:sha256:$candidateHash"
    Created = $Created
    SourceRevision = $SourceCommit
    PackageVersion = $PackageVersion
    Packages = @($packages | Sort-Object Id)
    Sboms = @($sboms | Sort-Object PackageId)
    PublicationManifest = [ordered]@{ FileName = (Split-Path -Leaf $resolvedManifest); Bytes = (Get-Item -LiteralPath $resolvedManifest).Length; Sha256 = (Get-FileHash -LiteralPath $resolvedManifest -Algorithm SHA256).Hash.ToLowerInvariant(); PackageCount = $definitions.Count }
    ChangeControl = [ordered]@{ FileName = $changeControlFile.Name; Bytes = $changeControlFile.Length; Sha256 = (Get-FileHash -LiteralPath $resolvedChangeControl -Algorithm SHA256).Hash.ToLowerInvariant() }
    RepositorySigning = [ordered]@{
        Strategy = 'nuget.org-repository-signing'
        Status = 'repository-signing-pending'
        ServiceIndex = $serviceIndex
        ExpectedSignatureType = 'Repository'
        ExpectedOwner = $ExpectedOwner
        VerificationScript = 'scripts/Test-NuGetRepositorySignedPackage.ps1'
        PrivateKeyRequired = $false
        AuthorCertificateRequired = $false
        PostPublishVerification = 'required'
    }
    Approval = [ordered]@{ Status = 'not-approved'; Publisher = 'unassigned'; IndependentApprover = 'unassigned' }
    Publication = [ordered]@{ Decision = 'do-not-publish'; Allowed = $false; UploadAttempted = $false; Environment = 'nuget-production'; AuthoritativeRepository = 'guojin-yan/OpenCV-CSharp-API' }
    SensitiveMaterialPresent = $false
    Deterministic = $true
}
$json = ((($record | ConvertTo-Json -Depth 12) -replace "`r`n", "`n") -replace "`r", "`n").TrimEnd() + "`n"
if (-not [string]::IsNullOrWhiteSpace($OutputPath)) {
    $fullOutput = [IO.Path]::GetFullPath($OutputPath)
    if ($Check) {
        if (-not (Test-Path -LiteralPath $fullOutput -PathType Leaf)) { throw "Publication bundle check output does not exist: $fullOutput" }
        $actual = ([IO.File]::ReadAllText($fullOutput) -replace "`r`n", "`n")
        if ($actual -cne $json) { throw "NuGet publication bundle record drifted: $fullOutput" }
    }
    else {
        New-Item -ItemType Directory -Force -Path (Split-Path -Parent $fullOutput) | Out-Null
        [IO.File]::WriteAllText($fullOutput, $json, [Text.UTF8Encoding]::new($false))
    }
}
elseif ($Check) { throw "-Check requires -OutputPath." }

Write-Host "NUGET_PUBLICATION_BUNDLE_OK candidate=$($record.CandidateId) packages=$($packages.Count) sboms=$($sboms.Count) signing=repository-signing-pending approval=not-approved publication=do-not-publish"
Write-Host "NUGET_PUBLICATION_AUTHORIZATION_TOKEN $($record.AuthorizationToken)"
if (-not [string]::IsNullOrWhiteSpace($OutputPath)) { Write-Host "Record: $([IO.Path]::GetFullPath($OutputPath))" }
