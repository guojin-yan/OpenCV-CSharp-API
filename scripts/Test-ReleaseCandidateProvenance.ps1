param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
Add-Type -AssemblyName System.IO.Compression.FileSystem

function Add-Violation {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [System.Collections.Generic.List[object]]$Violations,
        [Parameter(Mandatory = $true)]
        [string]$Path,
        [Parameter(Mandatory = $true)]
        [string]$Issue,
        [string]$Text = ""
    )

    $Violations.Add([pscustomobject]@{ Path = $Path; Issue = $Issue; Text = $Text.Trim() })
}

function Assert-True {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [System.Collections.Generic.List[object]]$Violations,
        [Parameter(Mandatory = $true)]
        [bool]$Condition,
        [Parameter(Mandatory = $true)]
        [string]$Path,
        [Parameter(Mandatory = $true)]
        [string]$Issue,
        [string]$Text = ""
    )

    if (-not $Condition) {
        Add-Violation -Violations $Violations -Path $Path -Issue $Issue -Text $Text
    }
}

function Test-NormalizedNuGetPackageVersion {
    param([Parameter(Mandatory = $true)][string]$Version)

    $match = [regex]::Match(
        $Version,
        '^(?<major>0|[1-9][0-9]*)\.(?<minor>0|[1-9][0-9]*)\.(?<patch>0|[1-9][0-9]*)(?:\.(?<revision>[1-9][0-9]*))?(?:-(?<prerelease>[0-9a-z-]+(?:\.[0-9a-z-]+)*))?$')
    if (-not $match.Success) {
        return $false
    }

    foreach ($identifier in $match.Groups['prerelease'].Value.Split('.', [System.StringSplitOptions]::RemoveEmptyEntries)) {
        if ($identifier -match '^[0-9]+$' -and $identifier.Length -gt 1 -and $identifier[0] -eq '0') {
            return $false
        }
    }

    return $true
}

function Get-EntryBytes {
    param([Parameter(Mandatory = $true)][System.IO.Compression.ZipArchiveEntry]$Entry)

    $stream = $Entry.Open()
    try {
        $memory = [System.IO.MemoryStream]::new()
        try {
            $stream.CopyTo($memory)
            return $memory.ToArray()
        }
        finally {
            $memory.Dispose()
        }
    }
    finally {
        $stream.Dispose()
    }
}

function Get-BytesSha256 {
    param([Parameter(Mandatory = $true)][byte[]]$Bytes)

    return ([System.BitConverter]::ToString(([System.Security.Cryptography.SHA256]::HashData($Bytes))).Replace('-', '')).ToLowerInvariant()
}

function Get-TextBytes {
    param([Parameter(Mandatory = $true)][string]$Text)

    return [System.Text.UTF8Encoding]::new($false).GetBytes($Text)
}

function Get-NuspecValue {
    param(
        [Parameter(Mandatory = $true)][xml]$Nuspec,
        [Parameter(Mandatory = $true)][string]$Name
    )

    $node = $Nuspec.SelectSingleNode("/*[local-name()='package']/*[local-name()='metadata']/*[local-name()='$Name']")
    if ($null -eq $node) { return "" }
    return $node.InnerText.Trim()
}

function Get-PackageRecord {
    param(
        [Parameter(Mandatory = $true)][string]$PackagePath,
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][System.Collections.Generic.List[object]]$Violations
    )

    if (-not (Test-Path -LiteralPath $PackagePath -PathType Leaf)) {
        Add-Violation -Violations $Violations -Path $PackagePath -Issue "Release candidate package is missing"
        return $null
    }

    $archive = $null
    try {
        $archive = [System.IO.Compression.ZipFile]::OpenRead($PackagePath)
        $entries = [System.Collections.Generic.List[object]]::new()
        $seen = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
        foreach ($entry in $archive.Entries) {
            $name = $entry.FullName.Replace('\', '/')
            if ([string]::IsNullOrWhiteSpace($name) -or $name.EndsWith('/')) { continue }
            if ($name.Contains('..') -or $name.StartsWith('/') -or $name.Contains(':')) {
                Add-Violation -Violations $Violations -Path $PackagePath -Issue "Package entry path is unsafe" -Text $name
            }
            if (-not $seen.Add($name)) {
                Add-Violation -Violations $Violations -Path $PackagePath -Issue "Package contains duplicate entry names" -Text $name
            }

            $bytes = Get-EntryBytes -Entry $entry
            $entries.Add([pscustomobject]@{
                    Path = $name
                    Length = [int64]$bytes.Length
                    Sha256 = Get-BytesSha256 -Bytes $bytes
                    Bytes = $bytes
                })
        }

        $nuspecEntry = @($entries | Where-Object { $_.Path -match '\.nuspec$' })
        Assert-True -Violations $Violations -Condition ($nuspecEntry.Count -eq 1) -Path $PackagePath -Issue "Release candidate package must contain exactly one nuspec"
        if ($nuspecEntry.Count -ne 1) { return $null }

        [xml]$nuspec = [System.Text.UTF8Encoding]::new($false).GetString($nuspecEntry[0].Bytes)
        $packageId = Get-NuspecValue -Nuspec $nuspec -Name 'id'
        $packageVersion = Get-NuspecValue -Nuspec $nuspec -Name 'version'
        $packageFileName = [System.IO.Path]::GetFileName($PackagePath)
        $expectedName = "$packageId.$packageVersion.nupkg"

        Assert-True -Violations $Violations -Condition ($packageFileName -eq $expectedName) -Path $PackagePath -Issue "Package filename must match nuspec identity and normalized version" -Text "Expected $expectedName"
        Assert-True -Violations $Violations -Condition ($packageId -match '^JYPPX\.OpenCV(?:\.CSharp\.API|\.runtime\.[a-z0-9.-]+(?:\.mini)?)$') -Path $PackagePath -Issue "Package ID must remain version-neutral and target-bound" -Text $packageId
        Assert-True -Violations $Violations -Condition (Test-NormalizedNuGetPackageVersion -Version $packageVersion) -Path $PackagePath -Issue "Package version must use canonical NuGet numeric and optional lowercase prerelease metadata" -Text $packageVersion

        return [pscustomobject]@{
            Path = [System.IO.Path]::GetFullPath($PackagePath)
            FileName = $packageFileName
            PackageId = $packageId
            PackageVersion = $packageVersion
            PackageSha256 = (Get-FileHash -LiteralPath $PackagePath -Algorithm SHA256).Hash.ToLowerInvariant()
            Entries = @($entries | Sort-Object Path | ForEach-Object {
                    [ordered]@{ Path = $_.Path; Length = $_.Length; Sha256 = $_.Sha256 }
                })
        }
    }
    catch {
        Add-Violation -Violations $Violations -Path $PackagePath -Issue "Release candidate package could not be inspected" -Text $_.Exception.Message
        return $null
    }
    finally {
        if ($null -ne $archive) { $archive.Dispose() }
    }
}

function New-DeterministicManifest {
    param(
        [Parameter(Mandatory = $true)][pscustomobject]$Package,
        [Parameter(Mandatory = $true)][string]$Rid,
        [Parameter(Mandatory = $true)][string]$RuntimeProfile,
        [Parameter(Mandatory = $true)][string]$Commit
    )

    return [ordered]@{
        SchemaVersion = 2
        PackageId = $Package.PackageId
        PackageVersion = $Package.PackageVersion
        PackageFile = $Package.FileName
        PackageSha256 = $Package.PackageSha256
        Normalization = [ordered]@{
            Status = 'verified'
            Tool = 'scripts/Normalize-NuGetPackageDeterminism.ps1'
            PackageSha256 = $Package.PackageSha256
            EntryCount = @($Package.Entries).Count
            Deterministic = $true
        }
        Rid = $Rid
        RuntimeProfile = $RuntimeProfile
        SourceCommit = $Commit
        Entries = @($Package.Entries)
        SignatureStatus = 'repository-signing-pending'
        SbomStatus = 'not-ready'
        SigningKeyReference = ''
        PublicFeedVerification = 'not-run'
        FeedReference = ''
        PublicationAttempted = $false
        PublicationAllowed = $false
        Signature = [ordered]@{
            Status = 'repository-signing-pending'
            Strategy = 'nuget.org-repository-signing'
            PackageSha256 = $Package.PackageSha256
            InputPackageSha256 = $Package.PackageSha256
            PostSigningPackageSha256 = ''
            NormalizationRequired = $true
            PackageIdentity = "$($Package.PackageId)/$($Package.PackageVersion)"
            CertificateReference = ''
            TimestampPolicy = 'NuGet.org-repository-timestamp-required'
            ServiceIndex = 'https://api.nuget.org/v3/index.json'
            ExpectedSignatureType = 'Repository'
            ExpectedOwner = 'GuojinYan'
            AuthorCertificateRequired = $false
            PrivateKeyRequired = $false
            VerificationResult = 'post-publication-required'
            PrivateKeyMaterialPresent = $false
        }
        SigningHandoff = [ordered]@{
            Status = 'repository-signing-pending'
            Strategy = 'nuget.org-repository-signing'
            PackageSha256 = $Package.PackageSha256
            InputPackageSha256 = $Package.PackageSha256
            PostSigningPackageSha256 = ''
            NormalizationRequired = $true
            PackageIdentity = "$($Package.PackageId)/$($Package.PackageVersion)"
            PublicKeyReference = ''
            CertificateReference = ''
            TimestampPolicy = 'NuGet.org-repository-timestamp-required'
            ServiceIndex = 'https://api.nuget.org/v3/index.json'
            ExpectedSignatureType = 'Repository'
            ExpectedOwner = 'GuojinYan'
            VerificationScript = 'scripts/Test-NuGetRepositorySignedPackage.ps1'
            AuthorCertificateRequired = $false
            PrivateKeyRequired = $false
            CustodyOwner = ''
            KeyNotBefore = ''
            KeyNotAfter = ''
            RotationPolicy = '90-day-review'
            PrivateKeyMaterialPresent = $false
        }
        Sbom = [ordered]@{
            Status = 'not-ready'
            Format = 'SPDX-2.3'
            PackageSha256 = $Package.PackageSha256
            Generator = ''
            GeneratorVersion = ''
            DocumentSha256 = ''
            ComponentCount = 0
            Deterministic = $true
        }
        SbomHandoff = [ordered]@{
            Status = 'not-ready'
            Format = 'SPDX-2.3'
            PackageSha256 = $Package.PackageSha256
            SourceCommit = $Commit
            OpenCvSourceRevision = ''
            NativeModules = @()
            LicenseEvidence = @()
            Generator = ''
            GeneratorVersion = ''
            ComponentCount = 0
            Deterministic = $true
            DocumentSha256 = ''
        }
        HostedPromotion = [ordered]@{
            Target = 'win-x86/full'
            Status = 'pending-hosted-evidence'
            ProducerRunId = ''
            PackRunId = ''
            ConsumerRunId = ''
            ProducerArtifactName = 'runtime-input-win-x86-full'
            PackageArtifactName = 'nupkg-win-x86-full'
            ConsumerProcessArchitecture = 'X86'
        }
        Rollback = [ordered]@{
            CandidateId = "$($Package.PackageId)/$($Package.PackageVersion)/$Rid/$RuntimeProfile"
            PriorKnownGood = ''
            AbortRecord = 'local-preflight-only; no remote mutation'
        }
    }
}

function Test-Manifest {
    param(
        [Parameter(Mandatory = $true)][pscustomobject]$Package,
        [Parameter(Mandatory = $true)][object]$Manifest,
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][System.Collections.Generic.List[object]]$Violations,
        [Parameter(Mandatory = $true)][string]$ManifestPath
    )

    foreach ($name in @('SchemaVersion', 'PackageId', 'PackageVersion', 'PackageFile', 'PackageSha256', 'Normalization', 'Rid', 'RuntimeProfile', 'SourceCommit', 'Entries', 'SignatureStatus', 'SbomStatus', 'PublicFeedVerification', 'PublicationAttempted', 'PublicationAllowed', 'Signature', 'SigningHandoff', 'Sbom', 'SbomHandoff', 'HostedPromotion', 'Rollback')) {
        Assert-True -Violations $Violations -Condition ($null -ne $Manifest.PSObject.Properties[$name]) -Path $ManifestPath -Issue "Release manifest is missing required field" -Text $name
    }

    Assert-True -Violations $Violations -Condition ([int]$Manifest.SchemaVersion -eq 2) -Path $ManifestPath -Issue "Release manifest schema version is unsupported"
    Assert-True -Violations $Violations -Condition ($Manifest.PackageId -eq $Package.PackageId) -Path $ManifestPath -Issue "Release manifest package ID does not match package"
    Assert-True -Violations $Violations -Condition ($Manifest.PackageVersion -eq $Package.PackageVersion) -Path $ManifestPath -Issue "Release manifest package version does not match package"
    Assert-True -Violations $Violations -Condition ($Manifest.PackageFile -eq $Package.FileName) -Path $ManifestPath -Issue "Release manifest package filename does not match package"
    Assert-True -Violations $Violations -Condition ($Manifest.PackageSha256 -eq $Package.PackageSha256) -Path $ManifestPath -Issue "Release manifest package hash does not match package"
    Assert-True -Violations $Violations -Condition ($Manifest.Normalization.Status -eq 'verified' -and $Manifest.Normalization.Tool -eq 'scripts/Normalize-NuGetPackageDeterminism.ps1') -Path $ManifestPath -Issue "Release manifest must carry verified package normalization provenance"
    Assert-True -Violations $Violations -Condition ($Manifest.Normalization.PackageSha256 -eq $Package.PackageSha256 -and [int]$Manifest.Normalization.EntryCount -eq @($Package.Entries).Count -and [bool]$Manifest.Normalization.Deterministic) -Path $ManifestPath -Issue "Release manifest normalization provenance does not match package contents"
    if ($Package.PackageId -match '^JYPPX\.OpenCV\.runtime\.(?<rid>[^.]+(?:\.[^.]+)*?)(?<mini>\.mini)?$') {
        $expectedRid = $Matches['rid']
        $expectedProfile = if ($Matches['mini']) { 'mini' } else { 'full' }
        Assert-True -Violations $Violations -Condition ($Manifest.Rid -eq $expectedRid) -Path $ManifestPath -Issue "Release manifest RID does not match runtime package ID" -Text "Expected $expectedRid"
        Assert-True -Violations $Violations -Condition ($Manifest.RuntimeProfile -eq $expectedProfile) -Path $ManifestPath -Issue "Release manifest profile does not match runtime package ID" -Text "Expected $expectedProfile"
    }
    Assert-True -Violations $Violations -Condition ($Manifest.SignatureStatus -eq 'repository-signing-pending') -Path $ManifestPath -Issue "Unsigned local preflight must state NuGet.org repository-signing readiness explicitly"
    Assert-True -Violations $Violations -Condition ($Manifest.SbomStatus -eq 'not-ready') -Path $ManifestPath -Issue "Local preflight must state SBOM readiness explicitly"
    Assert-True -Violations $Violations -Condition ($Manifest.PublicFeedVerification -eq 'not-run') -Path $ManifestPath -Issue "Local preflight must not claim public-feed verification"
    Assert-True -Violations $Violations -Condition ([string]::IsNullOrWhiteSpace([string]$Manifest.FeedReference)) -Path $ManifestPath -Issue "Local preflight must not carry a mutable feed reference"
    Assert-True -Violations $Violations -Condition (-not [bool]$Manifest.PublicationAttempted) -Path $ManifestPath -Issue "Release preflight must reject publication attempts"
    Assert-True -Violations $Violations -Condition (-not [bool]$Manifest.PublicationAllowed) -Path $ManifestPath -Issue "Local preflight must not authorize publication"
    Assert-True -Violations $Violations -Condition ($null -ne $Manifest.Rollback -and $Manifest.Rollback.AbortRecord -eq 'local-preflight-only; no remote mutation') -Path $ManifestPath -Issue "Rollback metadata must record the non-publishing abort state"
    Assert-True -Violations $Violations -Condition ($null -ne $Manifest.Signature -and $Manifest.Signature.Status -eq 'repository-signing-pending' -and $Manifest.Signature.Strategy -eq 'nuget.org-repository-signing') -Path $ManifestPath -Issue "Repository-signing readiness must be explicit"
    Assert-True -Violations $Violations -Condition ($Manifest.Signature.PackageSha256 -eq $Package.PackageSha256) -Path $ManifestPath -Issue "Signature input hash must match package"
    Assert-True -Violations $Violations -Condition ($Manifest.Signature.InputPackageSha256 -eq $Manifest.Normalization.PackageSha256 -and [bool]$Manifest.Signature.NormalizationRequired) -Path $ManifestPath -Issue "Signature input must bind the normalized package hash"
    Assert-True -Violations $Violations -Condition ([string]::IsNullOrWhiteSpace([string]$Manifest.Signature.CertificateReference) -and $Manifest.Signature.TimestampPolicy -eq 'NuGet.org-repository-timestamp-required' -and $Manifest.Signature.ServiceIndex -eq 'https://api.nuget.org/v3/index.json' -and $Manifest.Signature.ExpectedSignatureType -eq 'Repository' -and $Manifest.Signature.ExpectedOwner -eq 'GuojinYan' -and -not [bool]$Manifest.Signature.AuthorCertificateRequired -and -not [bool]$Manifest.Signature.PrivateKeyRequired -and $Manifest.Signature.VerificationResult -eq 'post-publication-required') -Path $ManifestPath -Issue "Repository-signing policy drifted or claimed author-signing inputs"
    Assert-True -Violations $Violations -Condition (-not [bool]$Manifest.Signature.PrivateKeyMaterialPresent) -Path $ManifestPath -Issue "Private key material must never be present in provenance"
    Assert-True -Violations $Violations -Condition ($null -ne $Manifest.SigningHandoff -and $Manifest.SigningHandoff.Status -eq 'repository-signing-pending' -and $Manifest.SigningHandoff.Strategy -eq 'nuget.org-repository-signing') -Path $ManifestPath -Issue "Signing handoff must remain repository-signing-pending before publication"
    Assert-True -Violations $Violations -Condition ($Manifest.SigningHandoff.PackageSha256 -eq $Package.PackageSha256) -Path $ManifestPath -Issue "Signing handoff package hash must match package"
    Assert-True -Violations $Violations -Condition ($Manifest.SigningHandoff.InputPackageSha256 -eq $Manifest.Normalization.PackageSha256 -and [bool]$Manifest.SigningHandoff.NormalizationRequired) -Path $ManifestPath -Issue "Signing handoff must bind the normalized package hash"
    Assert-True -Violations $Violations -Condition ([string]::IsNullOrWhiteSpace([string]$Manifest.SigningHandoff.PostSigningPackageSha256)) -Path $ManifestPath -Issue "Prepublication signing handoff must not claim repository-signed package bytes"
    Assert-True -Violations $Violations -Condition ([string]::IsNullOrWhiteSpace([string]$Manifest.SigningHandoff.PublicKeyReference) -and [string]::IsNullOrWhiteSpace([string]$Manifest.SigningHandoff.CertificateReference) -and $Manifest.SigningHandoff.ServiceIndex -eq 'https://api.nuget.org/v3/index.json' -and $Manifest.SigningHandoff.ExpectedSignatureType -eq 'Repository' -and $Manifest.SigningHandoff.ExpectedOwner -eq 'GuojinYan' -and $Manifest.SigningHandoff.VerificationScript -eq 'scripts/Test-NuGetRepositorySignedPackage.ps1' -and -not [bool]$Manifest.SigningHandoff.AuthorCertificateRequired -and -not [bool]$Manifest.SigningHandoff.PrivateKeyRequired) -Path $ManifestPath -Issue "Repository-signing handoff drifted or claimed author-signing inputs"
    Assert-True -Violations $Violations -Condition (-not [bool]$Manifest.SigningHandoff.PrivateKeyMaterialPresent) -Path $ManifestPath -Issue "Signing handoff must reject private key material"
    Assert-True -Violations $Violations -Condition ($null -ne $Manifest.Sbom -and $Manifest.Sbom.Status -eq 'not-ready') -Path $ManifestPath -Issue "SBOM readiness must be explicit"
    Assert-True -Violations $Violations -Condition ($Manifest.Sbom.PackageSha256 -eq $Package.PackageSha256) -Path $ManifestPath -Issue "SBOM input hash must match package"
    Assert-True -Violations $Violations -Condition ([string]::IsNullOrWhiteSpace([string]$Manifest.Sbom.DocumentSha256)) -Path $ManifestPath -Issue "Not-ready SBOM must not claim a document hash"
    Assert-True -Violations $Violations -Condition ([bool]$Manifest.Sbom.Deterministic) -Path $ManifestPath -Issue "SBOM serialization must declare deterministic ordering"
    Assert-True -Violations $Violations -Condition ($null -ne $Manifest.SbomHandoff -and $Manifest.SbomHandoff.Status -eq 'not-ready') -Path $ManifestPath -Issue "SBOM handoff must remain not-ready without generator inputs"
    Assert-True -Violations $Violations -Condition ($Manifest.SbomHandoff.PackageSha256 -eq $Package.PackageSha256) -Path $ManifestPath -Issue "SBOM handoff package hash must match package"
    Assert-True -Violations $Violations -Condition ([string]::IsNullOrWhiteSpace([string]$Manifest.SbomHandoff.DocumentSha256)) -Path $ManifestPath -Issue "Not-ready SBOM handoff must not claim a document hash"
    Assert-True -Violations $Violations -Condition ($null -ne $Manifest.HostedPromotion -and $Manifest.HostedPromotion.Target -eq 'win-x86/full' -and $Manifest.HostedPromotion.Status -eq 'pending-hosted-evidence') -Path $ManifestPath -Issue "Windows x86 promotion must remain hosted-evidence-pending"
    Assert-True -Violations $Violations -Condition ([string]::IsNullOrWhiteSpace([string]$Manifest.HostedPromotion.ProducerRunId) -and [string]::IsNullOrWhiteSpace([string]$Manifest.HostedPromotion.PackRunId) -and [string]::IsNullOrWhiteSpace([string]$Manifest.HostedPromotion.ConsumerRunId)) -Path $ManifestPath -Issue "Pending Windows x86 promotion must not claim hosted run IDs"

    $actualEntries = @($Package.Entries | ForEach-Object { "$($_.Path)|$($_.Length)|$($_.Sha256)" })
    $manifestEntries = @($Manifest.Entries | ForEach-Object { "$($_.Path)|$($_.Length)|$($_.Sha256)" })
    Assert-True -Violations $Violations -Condition ([string]::Join("`n", $actualEntries) -eq [string]::Join("`n", $manifestEntries)) -Path $ManifestPath -Issue "Release manifest entry list does not match package contents"
}

function New-TestPackage {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$PackageId,
        [Parameter(Mandatory = $true)][string]$PackageVersion,
        [Parameter(Mandatory = $true)][string]$Rid,
        [Parameter(Mandatory = $true)][string]$RuntimeProfile
    )

    $directory = Split-Path -Parent $Path
    New-Item -ItemType Directory -Force -Path $directory | Out-Null
    $archive = [System.IO.Compression.ZipFile]::Open($Path, [System.IO.Compression.ZipArchiveMode]::Create)
    try {
        $nuspec = ('<?xml version="1.0" encoding="utf-8"?><package><metadata><id>' + $PackageId + '</id><version>' + $PackageVersion + '</version></metadata></package>')
        foreach ($item in @(
                [pscustomobject]@{ Path = "$PackageId.nuspec"; Text = $nuspec },
                [pscustomobject]@{ Path = 'README.md'; Text = '# local release candidate' },
                [pscustomobject]@{ Path = "runtimes/$Rid/native/JYPPX.OpenCV.Native.dll"; Text = 'native-fixture' },
                [pscustomobject]@{ Path = 'build/JYPPX.OpenCV.runtime.provenance.json'; Text = (@{ PackageId = $PackageId; PackageVersion = $PackageVersion; Rid = $Rid; RuntimeProfile = $RuntimeProfile; SyntheticRuntimeInputs = $false } | ConvertTo-Json -Compress) }
            )) {
            $entry = $archive.CreateEntry($item.Path)
            $stream = $entry.Open()
            try { $bytes = Get-TextBytes -Text $item.Text; $stream.Write($bytes, 0, $bytes.Length) }
            finally { $stream.Dispose() }
        }
    }
    finally { $archive.Dispose() }
}

$violations = [System.Collections.Generic.List[object]]::new()
$temporaryRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("opencv-csharp-release-provenance-" + [guid]::NewGuid().ToString('N'))

try {
    $packageRoot = Join-Path $temporaryRoot 'packages'
    $packageId = 'JYPPX.OpenCV.runtime.win-x64'
    $packageVersion = '5.0.0-preview.1'
    $rid = 'win-x64'
    $profile = 'full'
    $packagePath = Join-Path $packageRoot "$packageId.$packageVersion.nupkg"
    New-TestPackage -Path $packagePath -PackageId $packageId -PackageVersion $packageVersion -Rid $rid -RuntimeProfile $profile

    $packageViolations = [System.Collections.Generic.List[object]]::new()
    $package = Get-PackageRecord -PackagePath $packagePath -Violations $packageViolations
    $violations.AddRange($packageViolations)
    if ($null -eq $package) { throw 'Positive release package fixture could not be inspected.' }

    $manifest = New-DeterministicManifest -Package $package -Rid $rid -RuntimeProfile $profile -Commit 'fixture-commit'
    $manifestPath = Join-Path $temporaryRoot 'release-candidate.provenance.json'
    $manifestJson = $manifest | ConvertTo-Json -Depth 8
    [System.IO.File]::WriteAllText($manifestPath, $manifestJson, [System.Text.UTF8Encoding]::new($false))
    $parsedManifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
    Test-Manifest -Package $package -Manifest $parsedManifest -Violations $violations -ManifestPath $manifestPath

    $baselineManifest = $manifest | ConvertTo-Json -Depth 8 | ConvertFrom-Json
    $negative = @(
        'changed package hash',
        'extra package file',
        'mismatched RID/profile',
        'mutable feed URL',
        'malformed manifest',
        'publish-command invocation'
    )
    foreach ($case in $negative) {
        $copy = $manifest | ConvertTo-Json -Depth 8 | ConvertFrom-Json
        switch ($case) {
            'changed package hash' { $copy.PackageSha256 = ('0' * 64) }
            'extra package file' { $copy.Entries = @($copy.Entries) + [pscustomobject]@{ Path = 'unexpected.txt'; Length = 1; Sha256 = ('0' * 64) } }
            'mismatched RID/profile' { $copy.Rid = 'linux-x64' }
            'mutable feed URL' { $copy.FeedReference = 'https://example.invalid/latest' }
            'malformed manifest' { $copy.Rollback = $null }
            'publish-command invocation' { $copy.PublicationAttempted = $true }
        }
        $caseViolations = [System.Collections.Generic.List[object]]::new()
        Test-Manifest -Package $package -Manifest $copy -Violations $caseViolations -ManifestPath "$manifestPath/$case"
        Assert-True -Violations $violations -Condition ($caseViolations.Count -gt 0) -Path $case -Issue 'Negative provenance fixture was accepted'
    }

    $manifestAgain = $manifest | ConvertTo-Json -Depth 8
    Assert-True -Violations $violations -Condition ($manifestJson -eq $manifestAgain) -Path $manifestPath -Issue 'Release manifest serialization is not deterministic'
}
catch {
    Add-Violation -Violations $violations -Path $temporaryRoot -Issue 'Release candidate provenance guard execution failed' -Text $_.Exception.Message
}
finally {
    if (Test-Path -LiteralPath $temporaryRoot -PathType Container) {
        [System.IO.Directory]::Delete((Resolve-Path -LiteralPath $temporaryRoot).Path, $true)
    }
}

if ($violations.Count -gt 0) {
    Write-Host "Release candidate provenance guard failed with $($violations.Count) violation(s)."
    $violations | Format-List Path, Issue, Text
    exit 1
}

Write-Host 'Release candidate provenance guard passed.'
Write-Host 'Deterministic package manifest, NuGet.org repository-signing handoff, explicit SBOM readiness, rollback metadata, and non-publishing policy validated.'
Write-Host 'Negative fixtures rejected: changed hash, extra entry, RID drift, mutable feed, malformed manifest, publication attempt.'
