param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
Add-Type -AssemblyName System.IO.Compression.FileSystem

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$violations = [System.Collections.Generic.List[object]]::new()

function Add-Violation {
    param(
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][System.Collections.Generic.List[object]]$List,
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$Issue,
        [string]$Text = ""
    )

    $List.Add([pscustomobject]@{ Path = $Path; Issue = $Issue; Text = $Text.Trim() })
}

function Assert-True {
    param(
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][System.Collections.Generic.List[object]]$List,
        [Parameter(Mandatory = $true)][bool]$Condition,
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$Issue,
        [string]$Text = ""
    )

    if (-not $Condition) { Add-Violation -List $List -Path $Path -Issue $Issue -Text $Text }
}

function Get-Sha256 {
    param([Parameter(Mandatory = $true)][byte[]]$Bytes)
    return ([System.BitConverter]::ToString(([System.Security.Cryptography.SHA256]::HashData($Bytes))).Replace('-', '')).ToLowerInvariant()
}

function Write-Utf8NoBom {
    param([Parameter(Mandatory = $true)][string]$Path, [Parameter(Mandatory = $true)][string]$Text)
    [System.IO.File]::WriteAllText($Path, $Text, [System.Text.UTF8Encoding]::new($false))
}

function New-FixturePackage {
    param([Parameter(Mandatory = $true)][string]$Path)

    New-Item -ItemType Directory -Force -Path (Split-Path -Parent $Path) | Out-Null
    $archive = [System.IO.Compression.ZipFile]::Open($Path, [System.IO.Compression.ZipArchiveMode]::Create)
    try {
        foreach ($item in @(
                [pscustomobject]@{ Name = 'JYPPX.OpenCV.runtime.win-x64.nuspec'; Text = '<package><metadata><id>JYPPX.OpenCV.runtime.win-x64</id><version>5.0.0.0</version></metadata></package>' },
                [pscustomobject]@{ Name = 'README.md'; Text = '# release readiness fixture' },
                [pscustomobject]@{ Name = 'runtimes/win-x64/native/JYPPX.OpenCV.Native.dll'; Text = 'native-fixture' }
            )) {
            $entry = $archive.CreateEntry($item.Name)
            $stream = $entry.Open()
            try {
                $bytes = [System.Text.UTF8Encoding]::new($false).GetBytes($item.Text)
                $stream.Write($bytes, 0, $bytes.Length)
            }
            finally { $stream.Dispose() }
        }
    }
    finally { $archive.Dispose() }
}

function Get-PackageFacts {
    param([Parameter(Mandatory = $true)][string]$Path)

    $archive = [System.IO.Compression.ZipFile]::OpenRead($Path)
    try {
        $entries = @($archive.Entries | Where-Object { -not $_.FullName.EndsWith('/') } | ForEach-Object {
                $stream = $_.Open()
                try {
                    $memory = [System.IO.MemoryStream]::new()
                    try { $stream.CopyTo($memory); $bytes = $memory.ToArray() }
                    finally { $memory.Dispose() }
                }
                finally { $stream.Dispose() }
                [pscustomobject]@{ Path = $_.FullName.Replace('\', '/'); Length = $bytes.Length; Sha256 = Get-Sha256 -Bytes $bytes }
            } | Sort-Object Path)
        return [pscustomobject]@{
            PackageId = 'JYPPX.OpenCV.runtime.win-x64'
            PackageVersion = '5.0.0.0'
            PackageFile = [IO.Path]::GetFileName($Path)
            PackageSha256 = (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant()
            Entries = $entries
        }
    }
    finally { $archive.Dispose() }
}

function New-ReadinessManifest {
    param([Parameter(Mandatory = $true)][pscustomobject]$Package)

    [ordered]@{
        SchemaVersion = 2
        PackageId = $Package.PackageId
        PackageVersion = $Package.PackageVersion
        PackageFile = $Package.PackageFile
        PackageSha256 = $Package.PackageSha256
        Normalization = [ordered]@{
            Status = 'verified'
            Tool = 'scripts/Normalize-NuGetPackageDeterminism.ps1'
            PackageSha256 = $Package.PackageSha256
            EntryCount = @($Package.Entries).Count
            Deterministic = $true
        }
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
            VerificationScript = 'scripts/Test-NuGetRepositorySignedPackage.ps1'
            AuthorCertificateRequired = $false
            PrivateKeyRequired = $false
            VerificationResult = 'post-publication-required'
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
            Components = @()
        }
        FeedVerification = [ordered]@{
            Status = 'not-run'
            Mode = 'read-only-fixture'
            FeedReference = ''
            UploadAttempted = $false
        }
        HostedPromotion = [ordered]@{
            Target = 'win-x86/full'
            Status = 'pending-hosted-evidence'
            ProducerRunId = ''
            PackRunId = ''
            ConsumerRunId = ''
            ProducerArtifactName = 'runtime-input-win-x86-full'
            PackageArtifactName = 'nupkg-win-x86-full'
            HostArchitecture = 'AMD64'
            TargetArchitecture = 'X86'
            PeMachine = 'I386'
            Wow64Probe = 'required'
            ConsumerProcessArchitecture = 'X86'
            ProducerPathOverrides = 'forbidden'
            LoaderOverrides = 'forbidden'
            PromotionRequires = @('hosted-producer', 'independent-artifact-audit', 'same-run-pack', 'x86-consumer')
        }
    }
}

function Test-ReadinessManifest {
    param(
        [Parameter(Mandatory = $true)][pscustomobject]$Package,
        [Parameter(Mandatory = $true)][object]$Manifest,
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][System.Collections.Generic.List[object]]$List,
        [Parameter(Mandatory = $true)][string]$Path
    )

    foreach ($field in @('SchemaVersion', 'PackageId', 'PackageVersion', 'PackageFile', 'PackageSha256', 'Normalization', 'Signature', 'Sbom', 'FeedVerification', 'HostedPromotion')) {
        Assert-True -List $List -Condition ($null -ne $Manifest.PSObject.Properties[$field]) -Path $Path -Issue 'Readiness manifest is missing required field' -Text $field
    }
    if ($null -eq $Manifest.Normalization -or $null -eq $Manifest.Signature -or $null -eq $Manifest.Sbom -or $null -eq $Manifest.FeedVerification -or $null -eq $Manifest.HostedPromotion) { return }

    Assert-True -List $List -Condition ([int]$Manifest.SchemaVersion -eq 2) -Path $Path -Issue 'Readiness schema version must be 2'
    Assert-True -List $List -Condition ($Manifest.PackageSha256 -eq $Package.PackageSha256) -Path $Path -Issue 'Readiness package hash does not match package'
    Assert-True -List $List -Condition ($Manifest.Normalization.Status -eq 'verified' -and $Manifest.Normalization.Tool -eq 'scripts/Normalize-NuGetPackageDeterminism.ps1') -Path $Path -Issue 'Readiness must carry verified package normalization provenance'
    Assert-True -List $List -Condition ($Manifest.Normalization.PackageSha256 -eq $Package.PackageSha256 -and [int]$Manifest.Normalization.EntryCount -eq @($Package.Entries).Count -and [bool]$Manifest.Normalization.Deterministic) -Path $Path -Issue 'Normalization provenance does not match package contents'
    Assert-True -List $List -Condition ($Manifest.Signature.PackageSha256 -eq $Package.PackageSha256) -Path $Path -Issue 'Signature input hash does not match package'
    Assert-True -List $List -Condition ($Manifest.Signature.InputPackageSha256 -eq $Manifest.Normalization.PackageSha256 -and [bool]$Manifest.Signature.NormalizationRequired) -Path $Path -Issue 'Signing input must bind the normalized package hash'
    Assert-True -List $List -Condition ($Manifest.Signature.Status -in @('repository-signing-pending', 'repository-signed', 'verified')) -Path $Path -Issue 'Repository-signature status is outside the approved state machine'
    Assert-True -List $List -Condition (-not [bool]$Manifest.Signature.PrivateKeyMaterialPresent) -Path $Path -Issue 'Private key material must never be present in readiness metadata'
    Assert-True -List $List -Condition ($Manifest.Signature.Strategy -eq 'nuget.org-repository-signing' -and $Manifest.Signature.TimestampPolicy -eq 'NuGet.org-repository-timestamp-required' -and $Manifest.Signature.ServiceIndex -eq 'https://api.nuget.org/v3/index.json' -and $Manifest.Signature.ExpectedSignatureType -eq 'Repository' -and $Manifest.Signature.ExpectedOwner -eq 'GuojinYan' -and $Manifest.Signature.VerificationScript -eq 'scripts/Test-NuGetRepositorySignedPackage.ps1' -and -not [bool]$Manifest.Signature.AuthorCertificateRequired -and -not [bool]$Manifest.Signature.PrivateKeyRequired) -Path $Path -Issue 'NuGet.org repository-signing policy drifted'
    if ($Manifest.Signature.Status -eq 'repository-signing-pending') {
        Assert-True -List $List -Condition ([string]::IsNullOrWhiteSpace([string]$Manifest.Signature.CertificateReference)) -Path $Path -Issue 'Repository-signing pending state must not claim an author certificate'
        Assert-True -List $List -Condition ($Manifest.Signature.VerificationResult -eq 'post-publication-required') -Path $Path -Issue 'Repository-signing pending state must require post-publication verification'
        Assert-True -List $List -Condition ([string]::IsNullOrWhiteSpace([string]$Manifest.Signature.PostSigningPackageSha256)) -Path $Path -Issue 'Repository-signing pending state must not claim post-publication package bytes'
    }
    else {
        Assert-True -List $List -Condition ($Manifest.Signature.PostSigningPackageSha256 -match '^[0-9a-f]{64}$' -and $Manifest.Signature.PostSigningPackageSha256 -ne $Manifest.Signature.InputPackageSha256) -Path $Path -Issue 'Repository-signed state must bind distinct public package bytes'
    }
    if ($Manifest.Signature.Status -eq 'verified') {
        Assert-True -List $List -Condition ($Manifest.Signature.VerificationResult -eq 'passed') -Path $Path -Issue 'Verified signature must have a passed verification result'
    }

    Assert-True -List $List -Condition ($Manifest.Sbom.Status -in @('not-ready', 'ready', 'verified')) -Path $Path -Issue 'SBOM status is outside the approved state machine'
    Assert-True -List $List -Condition ($Manifest.Sbom.PackageSha256 -eq $Package.PackageSha256) -Path $Path -Issue 'SBOM input hash does not match package'
    Assert-True -List $List -Condition ([bool]$Manifest.Sbom.Deterministic) -Path $Path -Issue 'SBOM must declare deterministic serialization'
    if ($Manifest.Sbom.Status -eq 'not-ready') {
        Assert-True -List $List -Condition ([string]::IsNullOrWhiteSpace([string]$Manifest.Sbom.Generator)) -Path $Path -Issue 'Not-ready SBOM must not claim a generator'
        Assert-True -List $List -Condition ([string]::IsNullOrWhiteSpace([string]$Manifest.Sbom.DocumentSha256)) -Path $Path -Issue 'Not-ready SBOM must not claim a document hash'
    }
    else {
        Assert-True -List $List -Condition (-not [string]::IsNullOrWhiteSpace([string]$Manifest.Sbom.Generator)) -Path $Path -Issue 'Ready SBOM must identify its generator'
        Assert-True -List $List -Condition (-not [string]::IsNullOrWhiteSpace([string]$Manifest.Sbom.DocumentSha256)) -Path $Path -Issue 'Ready SBOM must identify its document hash'
        Assert-True -List $List -Condition ([int]$Manifest.Sbom.ComponentCount -gt 0) -Path $Path -Issue 'Ready SBOM must contain components'
    }

    Assert-True -List $List -Condition ($Manifest.FeedVerification.Status -eq 'not-run') -Path $Path -Issue 'Local readiness must not claim public-feed verification'
    Assert-True -List $List -Condition ($Manifest.FeedVerification.Mode -eq 'read-only-fixture') -Path $Path -Issue 'Feed verification must be explicitly read-only'
    Assert-True -List $List -Condition ([string]::IsNullOrWhiteSpace([string]$Manifest.FeedVerification.FeedReference)) -Path $Path -Issue 'Local readiness must not carry a mutable feed URL'
    Assert-True -List $List -Condition (-not [bool]$Manifest.FeedVerification.UploadAttempted) -Path $Path -Issue 'Readiness must reject feed upload attempts'

    $promotion = $Manifest.HostedPromotion
    Assert-True -List $List -Condition ($promotion.Target -eq 'win-x86/full' -and $promotion.Status -eq 'pending-hosted-evidence') -Path $Path -Issue 'win-x86/full must remain hosted-evidence-pending'
    Assert-True -List $List -Condition ($promotion.HostArchitecture -eq 'AMD64' -and $promotion.TargetArchitecture -eq 'X86' -and $promotion.PeMachine -eq 'I386') -Path $Path -Issue 'Hosted x86 architecture checklist is incomplete'
    Assert-True -List $List -Condition ($promotion.Wow64Probe -eq 'required' -and $promotion.ConsumerProcessArchitecture -eq 'X86') -Path $Path -Issue 'Hosted x86 WoW64/consumer checklist is incomplete'
    Assert-True -List $List -Condition ($promotion.ProducerPathOverrides -eq 'forbidden' -and $promotion.LoaderOverrides -eq 'forbidden') -Path $Path -Issue 'Hosted x86 package consumer must forbid producer path overrides'
    Assert-True -List $List -Condition ([string]::IsNullOrWhiteSpace([string]$promotion.ProducerRunId) -and [string]::IsNullOrWhiteSpace([string]$promotion.PackRunId) -and [string]::IsNullOrWhiteSpace([string]$promotion.ConsumerRunId)) -Path $Path -Issue 'Pending hosted promotion must not claim run IDs'
    Assert-True -List $List -Condition (@($promotion.PromotionRequires | Sort-Object) -join ',' -eq 'hosted-producer,independent-artifact-audit,same-run-pack,x86-consumer') -Path $Path -Issue 'Hosted x86 promotion criteria changed'
}

$temporaryRoot = Join-Path ([System.IO.Path]::GetTempPath()) ('opencv-csharp-release-readiness-' + [guid]::NewGuid().ToString('N'))
try {
    $packagePath = Join-Path $temporaryRoot 'packages/JYPPX.OpenCV.runtime.win-x64.5.0.0.nupkg'
    New-FixturePackage -Path $packagePath
    $package = Get-PackageFacts -Path $packagePath
    $manifest = New-ReadinessManifest -Package $package
    $manifestPath = Join-Path $temporaryRoot 'readiness.json'
    $json = $manifest | ConvertTo-Json -Depth 12
    Write-Utf8NoBom -Path $manifestPath -Text $json
    Test-ReadinessManifest -Package $package -Manifest ($json | ConvertFrom-Json) -List $violations -Path $manifestPath

    $negativeCases = @(
        'missing repository signing policy',
        'changed package hash',
        'unsigned marked signed',
        'SBOM component drift',
        'nondeterministic ordering',
        'mutable feed',
        'attempted publish',
        'private key material'
    )
    foreach ($case in $negativeCases) {
        $copy = $json | ConvertFrom-Json
        switch ($case) {
            'missing repository signing policy' { $copy.Signature.Strategy = '' }
            'changed package hash' { $copy.Signature.PackageSha256 = ('0' * 64) }
            'unsigned marked signed' { $copy.Signature.Status = 'verified' }
            'SBOM component drift' { $copy.Sbom.Status = 'ready'; $copy.Sbom.Generator = 'fixture'; $copy.Sbom.GeneratorVersion = '1'; $copy.Sbom.DocumentSha256 = 'a'; $copy.Sbom.ComponentCount = 1; $copy.Sbom.PackageSha256 = ('0' * 64) }
            'nondeterministic ordering' { $copy.Sbom.Deterministic = $false }
            'mutable feed' { $copy.FeedVerification.FeedReference = 'https://example.invalid/latest' }
            'attempted publish' { $copy.FeedVerification.UploadAttempted = $true }
            'private key material' { $copy.Signature.PrivateKeyMaterialPresent = $true }
        }
        $caseViolations = [System.Collections.Generic.List[object]]::new()
        Test-ReadinessManifest -Package $package -Manifest $copy -List $caseViolations -Path "$manifestPath/$case"
        Assert-True -List $violations -Condition ($caseViolations.Count -gt 0) -Path $case -Issue 'Readiness negative fixture was accepted'
    }

    $sourceFiles = Get-ChildItem -LiteralPath $repo -Recurse -File -Include *.ps1,*.json,*.yml,*.yaml,*.props,*.targets -Force
    foreach ($sourceFile in $sourceFiles) {
        $text = [System.IO.File]::ReadAllText($sourceFile.FullName)
        if ($text -match '-----BEGIN [A-Z ]*PRIVATE KEY-----') {
            Add-Violation -List $violations -Path $sourceFile.FullName -Issue 'Private key material must not exist in repository source files'
        }
    }
}
catch {
    Add-Violation -List $violations -Path $temporaryRoot -Issue 'Release readiness contract execution failed' -Text $_.Exception.Message
}
finally {
    if (Test-Path -LiteralPath $temporaryRoot -PathType Container) {
        [System.IO.Directory]::Delete((Resolve-Path -LiteralPath $temporaryRoot).Path, $true)
    }
}

if ($violations.Count -gt 0) {
    Write-Host "Release readiness contract failed with $($violations.Count) violation(s)."
    $violations | Format-List Path, Issue, Text
    exit 1
}

Write-Host 'Release readiness contract passed.'
Write-Host 'NuGet.org repository-signature/SBOM state machine, package hash binding, private-key exclusion, read-only feed policy, and hosted win-x86 promotion checklist validated.'
Write-Host 'Negative fixtures rejected: missing repository policy, package hash drift, unsigned marked signed, SBOM drift, nondeterminism, mutable feed, publication, private key.'
