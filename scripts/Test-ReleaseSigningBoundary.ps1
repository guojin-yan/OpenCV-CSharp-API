param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
Add-Type -AssemblyName System.IO.Compression.FileSystem

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$violations = [System.Collections.Generic.List[object]]::new()
$hashPattern = '^[0-9a-f]{64}$'

function Add-Violation {
    param(
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][System.Collections.Generic.List[object]]$List,
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$Issue,
        [string]$Text = ''
    )

    $List.Add([pscustomobject]@{ Path = $Path; Issue = $Issue; Text = $Text.Trim() })
}

function Assert-True {
    param(
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][System.Collections.Generic.List[object]]$List,
        [Parameter(Mandatory = $true)][bool]$Condition,
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$Issue,
        [string]$Text = ''
    )

    if (-not $Condition) { Add-Violation -List $List -Path $Path -Issue $Issue -Text $Text }
}

function Has-Property {
    param([object]$Object, [Parameter(Mandatory = $true)][string]$Name)
    return $null -ne $Object -and $null -ne $Object.PSObject.Properties[$Name]
}

function Get-BytesSha256 {
    param([Parameter(Mandatory = $true)][byte[]]$Bytes)
    return ([System.BitConverter]::ToString(([System.Security.Cryptography.SHA256]::HashData($Bytes))).Replace('-', '')).ToLowerInvariant()
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
        finally { $memory.Dispose() }
    }
    finally { $stream.Dispose() }
}

function New-UnsignedFixture {
    param([Parameter(Mandatory = $true)][string]$Path)

    New-Item -ItemType Directory -Force -Path (Split-Path -Parent $Path) | Out-Null
    $coreName = "package/services/metadata/core-properties/$([guid]::NewGuid().ToString('N')).psmdcp"
    $coreBytes = [Text.UTF8Encoding]::new($false).GetBytes('<coreProperties xmlns="http://schemas.openxmlformats.org/package/2006/metadata/core-properties"><dc:creator xmlns:dc="http://purl.org/dc/elements/1.1/">fixture</dc:creator></coreProperties>')
    $relsText = @"
<?xml version="1.0" encoding="utf-8"?>
<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
  <Relationship Type="http://schemas.microsoft.com/packaging/2010/07/manifest" Target="/JYPPX.OpenCV.runtime.win-x64.nuspec" Id="R$([guid]::NewGuid().ToString('N'))" />
  <Relationship Type="http://schemas.openxmlformats.org/package/2006/relationships/metadata/core-properties" Target="/$coreName" Id="R$([guid]::NewGuid().ToString('N'))" />
</Relationships>
"@.TrimStart()
    $items = @(
        [pscustomobject]@{ Name = 'JYPPX.OpenCV.runtime.win-x64.nuspec'; Bytes = [Text.UTF8Encoding]::new($false).GetBytes('<package><metadata><id>JYPPX.OpenCV.runtime.win-x64</id><version>5.0.0.0</version></metadata></package>') },
        [pscustomobject]@{ Name = '_rels/.rels'; Bytes = [Text.UTF8Encoding]::new($false).GetBytes($relsText) },
        [pscustomobject]@{ Name = $coreName; Bytes = $coreBytes },
        [pscustomobject]@{ Name = 'README.md'; Bytes = [Text.UTF8Encoding]::new($false).GetBytes('# signing boundary fixture') }
    )
    $archive = [IO.Compression.ZipFile]::Open($Path, [IO.Compression.ZipArchiveMode]::Create)
    try {
        foreach ($item in $items) {
            $entry = $archive.CreateEntry($item.Name)
            $stream = $entry.Open()
            try { $stream.Write($item.Bytes, 0, $item.Bytes.Length) }
            finally { $stream.Dispose() }
        }
    }
    finally { $archive.Dispose() }
}

function Add-SignatureFixtureEntry {
    param([Parameter(Mandatory = $true)][string]$Path)

    $archive = [IO.Compression.ZipFile]::Open($Path, [IO.Compression.ZipArchiveMode]::Update)
    try {
        $entry = $archive.CreateEntry('package/services/metadata/signatures/.signature.p7s')
        $stream = $entry.Open()
        try {
            $bytes = [byte[]](1, 2, 3, 4)
            $stream.Write($bytes, 0, $bytes.Length)
        }
        finally { $stream.Dispose() }
    }
    finally { $archive.Dispose() }
}

function Get-PackageFacts {
    param([Parameter(Mandatory = $true)][string]$Path)

    $archive = [IO.Compression.ZipFile]::OpenRead($Path)
    try {
        $entries = @($archive.Entries | Where-Object { -not $_.FullName.EndsWith('/') })
        return [pscustomobject]@{
            Hash = (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant()
            EntryCount = $entries.Count
        }
    }
    finally { $archive.Dispose() }
}

function New-BoundaryRecord {
    param(
        [Parameter(Mandatory = $true)][string]$PackageHash,
        [Parameter(Mandatory = $true)][string]$NormalizedHash,
        [Parameter(Mandatory = $true)][string]$PreNormalizationHash,
        [Parameter(Mandatory = $true)][int]$EntryCount,
        [ValidateSet('not-ready', 'ready-for-signing', 'signed', 'verified')]
        [string]$SigningStatus = 'not-ready',
        [string]$PostSigningHash = ''
    )

    return [ordered]@{
        SchemaVersion = 1
        PackageId = 'JYPPX.OpenCV.runtime.win-x64'
        PackageVersion = '5.0.0.0'
        PackageSha256 = $PackageHash
        Normalization = [ordered]@{
            Status = 'verified'
            Tool = 'scripts/Normalize-NuGetPackageDeterminism.ps1'
            InputPackageSha256 = $PreNormalizationHash
            PackageSha256 = $NormalizedHash
            EntryCount = $EntryCount
            Deterministic = $true
        }
        Signing = [ordered]@{
            Status = $SigningStatus
            NormalizationRequired = $true
            InputPackageSha256 = $NormalizedHash
            PostSigningPackageSha256 = $PostSigningHash
            PackageIdentity = 'JYPPX.OpenCV.runtime.win-x64/5.0.0.0'
            PublicKeyReference = ''
            CertificateReference = ''
            TimestampPolicy = 'RFC3161-required'
            TimestampAuthorityReference = ''
            CustodyOwner = ''
            KeyNotBefore = ''
            KeyNotAfter = ''
            RotationPolicy = '90-day-review'
            VerificationResult = if ($SigningStatus -eq 'not-ready') { 'not-run' } else { 'pending' }
            PrivateKeyMaterialPresent = $false
        }
        Sbom = [ordered]@{
            Status = 'not-ready'
            Format = 'SPDX-2.3'
            PackageSha256 = $NormalizedHash
            SourceCommit = 'fixture-source-commit'
            OpenCvSourceRevision = 'opencv-5.0.0'
            NativeModules = @('core', 'imgproc')
            LicenseEvidence = @('Apache-2.0:opencv', 'BSD-3-Clause:wrapper')
            Generator = ''
            GeneratorVersion = ''
            ComponentCount = 0
            DocumentSha256 = ''
            Deterministic = $true
        }
        Approval = [ordered]@{
            Status = 'not-approved'
            Reviewer = 'automated-local-preflight'
            Approver = 'unassigned'
            NormalizedPackageSha256 = $NormalizedHash
            SourceCommit = 'fixture-source-commit'
            OpenCvSourceRevision = 'opencv-5.0.0'
            SigningStatus = $SigningStatus
            SbomStatus = 'not-ready'
            EvidenceKind = 'local-preflight'
            RemoteMutationAllowed = $false
        }
        FeedReference = ''
        PublicationAttempted = $false
        PublicationAllowed = $false
        PrivateKeyMaterialPresent = $false
        SecretMaterialPresent = $false
    }
}

function Set-SignedRecordInputs {
    param([Parameter(Mandatory = $true)][object]$Record)

    $Record.Signing.PublicKeyReference = 'spki-sha256:' + ('b' * 64)
    $Record.Signing.CertificateReference = 'sha256:' + ('a' * 64)
    $Record.Signing.TimestampAuthorityReference = 'rfc3161-policy-sha256:' + ('c' * 64)
    $Record.Signing.CustodyOwner = 'release-security'
    $Record.Signing.KeyNotBefore = '2026-01-01T00:00:00Z'
    $Record.Signing.KeyNotAfter = '2026-04-01T00:00:00Z'
    $Record.Signing.VerificationResult = 'passed'
    $Record.Sbom.Status = 'verified'
    $Record.Sbom.Generator = 'fixture-spdx-generator'
    $Record.Sbom.GeneratorVersion = '1.0.0'
    $Record.Sbom.ComponentCount = 3
    $Record.Sbom.DocumentSha256 = 'd' * 64
    $Record.Approval.Status = 'approved'
    $Record.Approval.Approver = 'release-approver'
    $Record.Approval.SigningStatus = 'verified'
    $Record.Approval.SbomStatus = 'verified'
    $Record.Approval.EvidenceKind = 'externally-approved-public-inputs'
}

function Assert-ImmutableReference {
    param(
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][System.Collections.Generic.List[object]]$List,
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$Name,
        [string]$Value = ''
    )

    if ([string]::IsNullOrWhiteSpace($Value)) { return }
    $pattern = switch ($Name) {
        'TimestampAuthorityReference' { '^rfc3161-policy-sha256:[0-9a-f]{64}$'; break }
        'CertificateReference' { '^sha256:[0-9a-f]{64}$'; break }
        'PublicKeyReference' { '^spki-sha256:[0-9a-f]{64}$'; break }
        default { '^$' }
    }
    Assert-True -List $List -Condition ($Value -match $pattern) -Path $Path -Issue "$Name must be an immutable public digest reference" -Text $Value
}

function Test-BoundaryRecord {
    param(
        [Parameter(Mandatory = $true)][object]$Record,
        [Parameter(Mandatory = $true)][string]$PackagePath,
        [Parameter(Mandatory = $true)][string]$ExpectedPackageHash,
        [Parameter(Mandatory = $true)][string]$ExpectedNormalizedHash,
        [Parameter(Mandatory = $true)][string]$ExpectedPreNormalizationHash,
        [Parameter(Mandatory = $true)][int]$ExpectedEntryCount,
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][System.Collections.Generic.List[object]]$List,
        [Parameter(Mandatory = $true)][string]$Path
    )

    foreach ($field in @('SchemaVersion', 'PackageId', 'PackageVersion', 'PackageSha256', 'Normalization', 'Signing', 'Sbom', 'Approval', 'FeedReference', 'PublicationAttempted', 'PublicationAllowed', 'PrivateKeyMaterialPresent', 'SecretMaterialPresent')) {
        Assert-True -List $List -Condition (Has-Property -Object $Record -Name $field) -Path $Path -Issue 'Signing boundary record is missing required field' -Text $field
    }
    if (-not (Has-Property -Object $Record -Name 'Normalization') -or -not (Has-Property -Object $Record -Name 'Signing') -or -not (Has-Property -Object $Record -Name 'Sbom') -or -not (Has-Property -Object $Record -Name 'Approval')) { return }

    Assert-True -List $List -Condition ([int]$Record.SchemaVersion -eq 1) -Path $Path -Issue 'Signing boundary schema version must be 1'
    Assert-True -List $List -Condition ($Record.PackageId -eq 'JYPPX.OpenCV.runtime.win-x64' -and $Record.PackageVersion -eq '5.0.0.0') -Path $Path -Issue 'Package identity/version drifted'
    Assert-True -List $List -Condition ($Record.PackageSha256 -eq $ExpectedPackageHash) -Path $Path -Issue 'Final package hash does not match the inspected package'
    Assert-True -List $List -Condition ((Get-FileHash -LiteralPath $PackagePath -Algorithm SHA256).Hash.ToLowerInvariant() -eq $ExpectedPackageHash) -Path $Path -Issue 'Inspected package hash changed during boundary verification'

    foreach ($field in @('Status', 'Tool', 'InputPackageSha256', 'PackageSha256', 'EntryCount', 'Deterministic')) {
        Assert-True -List $List -Condition (Has-Property -Object $Record.Normalization -Name $field) -Path $Path -Issue 'Normalization provenance is incomplete' -Text $field
    }
    if (@('Status', 'Tool', 'InputPackageSha256', 'PackageSha256', 'EntryCount', 'Deterministic' | Where-Object { -not (Has-Property -Object $Record.Normalization -Name $_) }).Count -gt 0) { return }
    Assert-True -List $List -Condition ($Record.Normalization.Status -eq 'verified') -Path $Path -Issue 'Package normalization must be verified before signing'
    Assert-True -List $List -Condition ($Record.Normalization.Tool -eq 'scripts/Normalize-NuGetPackageDeterminism.ps1') -Path $Path -Issue 'Normalization tool reference is not canonical'
    Assert-True -List $List -Condition ($Record.Normalization.InputPackageSha256 -eq $ExpectedPreNormalizationHash) -Path $Path -Issue 'Pre-normalization provenance hash drifted'
    Assert-True -List $List -Condition ($Record.Normalization.PackageSha256 -eq $ExpectedNormalizedHash -and $Record.Normalization.PackageSha256 -match $hashPattern) -Path $Path -Issue 'Normalized package hash is missing or incorrect'
    Assert-True -List $List -Condition ([int]$Record.Normalization.EntryCount -eq $ExpectedEntryCount -and [int]$Record.Normalization.EntryCount -gt 0) -Path $Path -Issue 'Normalized package entry count is missing or incorrect'
    Assert-True -List $List -Condition ([bool]$Record.Normalization.Deterministic) -Path $Path -Issue 'Normalization must declare deterministic output'

    foreach ($field in @('Status', 'NormalizationRequired', 'InputPackageSha256', 'PostSigningPackageSha256', 'PackageIdentity', 'PublicKeyReference', 'CertificateReference', 'TimestampPolicy', 'TimestampAuthorityReference', 'CustodyOwner', 'KeyNotBefore', 'KeyNotAfter', 'RotationPolicy', 'VerificationResult', 'PrivateKeyMaterialPresent')) {
        Assert-True -List $List -Condition (Has-Property -Object $Record.Signing -Name $field) -Path $Path -Issue 'Signing provenance is incomplete' -Text $field
    }
    if (@('Status', 'NormalizationRequired', 'InputPackageSha256', 'PostSigningPackageSha256', 'PackageIdentity', 'PublicKeyReference', 'CertificateReference', 'TimestampPolicy', 'TimestampAuthorityReference', 'CustodyOwner', 'KeyNotBefore', 'KeyNotAfter', 'RotationPolicy', 'VerificationResult', 'PrivateKeyMaterialPresent' | Where-Object { -not (Has-Property -Object $Record.Signing -Name $_) }).Count -gt 0) { return }
    Assert-True -List $List -Condition ($Record.Signing.NormalizationRequired -eq $true) -Path $Path -Issue 'Signing must require the normalized package boundary'
    Assert-True -List $List -Condition ($Record.Signing.InputPackageSha256 -eq $Record.Normalization.PackageSha256) -Path $Path -Issue 'Signing input must equal the final normalized package hash'
    Assert-True -List $List -Condition ($Record.Signing.PackageIdentity -eq "$($Record.PackageId)/$($Record.PackageVersion)") -Path $Path -Issue 'Signing package identity drifted'
    Assert-True -List $List -Condition ($Record.Signing.TimestampPolicy -eq 'RFC3161-required' -and $Record.Signing.RotationPolicy -eq '90-day-review') -Path $Path -Issue 'Signing timestamp or rotation policy drifted'
    Assert-True -List $List -Condition (-not [bool]$Record.Signing.PrivateKeyMaterialPresent) -Path $Path -Issue 'Signing record must never contain private key material'
    Assert-ImmutableReference -List $List -Path $Path -Name 'CertificateReference' -Value ([string]$Record.Signing.CertificateReference)
    Assert-ImmutableReference -List $List -Path $Path -Name 'PublicKeyReference' -Value ([string]$Record.Signing.PublicKeyReference)
    Assert-ImmutableReference -List $List -Path $Path -Name 'TimestampAuthorityReference' -Value ([string]$Record.Signing.TimestampAuthorityReference)

    switch ([string]$Record.Signing.Status) {
        'not-ready' {
            Assert-True -List $List -Condition ([string]::IsNullOrWhiteSpace([string]$Record.Signing.PostSigningPackageSha256)) -Path $Path -Issue 'Not-ready signing must not claim a post-signing package hash'
            Assert-True -List $List -Condition ($Record.Signing.VerificationResult -eq 'not-run') -Path $Path -Issue 'Not-ready signing must not claim verification'
            Assert-True -List $List -Condition ([string]::IsNullOrWhiteSpace([string]$Record.Signing.CertificateReference) -and [string]::IsNullOrWhiteSpace([string]$Record.Signing.PublicKeyReference)) -Path $Path -Issue 'Not-ready signing must not claim certificate/public-key inputs'
        }
        'ready-for-signing' {
            Assert-True -List $List -Condition (-not [string]::IsNullOrWhiteSpace([string]$Record.Signing.CertificateReference) -and -not [string]::IsNullOrWhiteSpace([string]$Record.Signing.PublicKeyReference) -and -not [string]::IsNullOrWhiteSpace([string]$Record.Signing.CustodyOwner)) -Path $Path -Issue 'Ready-for-signing state lacks public inputs or custody owner'
            Assert-True -List $List -Condition ([string]::IsNullOrWhiteSpace([string]$Record.Signing.PostSigningPackageSha256)) -Path $Path -Issue 'Ready-for-signing must not claim post-signing bytes'
        }
        'signed' { Assert-True -List $List -Condition ($Record.Signing.PostSigningPackageSha256 -match $hashPattern -and $Record.Signing.PostSigningPackageSha256 -eq $ExpectedPackageHash) -Path $Path -Issue 'Signed package hash must bind the post-signing bytes' }
        'verified' {
            Assert-True -List $List -Condition ($Record.Signing.PostSigningPackageSha256 -match $hashPattern -and $Record.Signing.PostSigningPackageSha256 -eq $ExpectedPackageHash) -Path $Path -Issue 'Verified package hash must bind the post-signing bytes'
            Assert-True -List $List -Condition ($Record.Signing.VerificationResult -eq 'passed') -Path $Path -Issue 'Verified signing must have a passed verification result'
            Assert-True -List $List -Condition (-not [string]::IsNullOrWhiteSpace([string]$Record.Signing.KeyNotBefore) -and -not [string]::IsNullOrWhiteSpace([string]$Record.Signing.KeyNotAfter) -and -not [string]::IsNullOrWhiteSpace([string]$Record.Signing.CustodyOwner)) -Path $Path -Issue 'Verified signing is missing validity/custody evidence'
        }
        default { Add-Violation -List $List -Path $Path -Issue 'Signing status is outside the approved state machine' }
    }

    foreach ($field in @('Status', 'Format', 'PackageSha256', 'SourceCommit', 'OpenCvSourceRevision', 'NativeModules', 'LicenseEvidence', 'Generator', 'GeneratorVersion', 'ComponentCount', 'DocumentSha256', 'Deterministic')) {
        Assert-True -List $List -Condition (Has-Property -Object $Record.Sbom -Name $field) -Path $Path -Issue 'SBOM provenance is incomplete' -Text $field
    }
    if (@('Status', 'Format', 'PackageSha256', 'SourceCommit', 'OpenCvSourceRevision', 'NativeModules', 'LicenseEvidence', 'Generator', 'GeneratorVersion', 'ComponentCount', 'DocumentSha256', 'Deterministic' | Where-Object { -not (Has-Property -Object $Record.Sbom -Name $_) }).Count -gt 0) { return }
    Assert-True -List $List -Condition ($Record.Sbom.Format -eq 'SPDX-2.3') -Path $Path -Issue 'SBOM format must be SPDX-2.3'
    Assert-True -List $List -Condition ($Record.Sbom.PackageSha256 -eq $Record.Normalization.PackageSha256) -Path $Path -Issue 'SBOM hash must bind the final normalized package input'
    Assert-True -List $List -Condition (-not [string]::IsNullOrWhiteSpace([string]$Record.Sbom.SourceCommit) -and -not [string]::IsNullOrWhiteSpace([string]$Record.Sbom.OpenCvSourceRevision)) -Path $Path -Issue 'SBOM source/OpenCV revision provenance is incomplete'
    Assert-True -List $List -Condition (@($Record.Sbom.NativeModules).Count -gt 0 -and @($Record.Sbom.LicenseEvidence).Count -gt 0) -Path $Path -Issue 'SBOM native-module or license evidence is incomplete'
    Assert-True -List $List -Condition ([bool]$Record.Sbom.Deterministic) -Path $Path -Issue 'SBOM must declare deterministic ordering'
    if ($Record.Sbom.Status -eq 'not-ready') {
        Assert-True -List $List -Condition ([string]::IsNullOrWhiteSpace([string]$Record.Sbom.Generator) -and [string]::IsNullOrWhiteSpace([string]$Record.Sbom.DocumentSha256) -and [int]$Record.Sbom.ComponentCount -eq 0) -Path $Path -Issue 'Not-ready SBOM must not claim generated document evidence'
    }
    else {
        Assert-True -List $List -Condition ($Record.Sbom.Status -in @('ready', 'verified') -and -not [string]::IsNullOrWhiteSpace([string]$Record.Sbom.Generator) -and -not [string]::IsNullOrWhiteSpace([string]$Record.Sbom.GeneratorVersion) -and $Record.Sbom.DocumentSha256 -match $hashPattern -and [int]$Record.Sbom.ComponentCount -gt 0) -Path $Path -Issue 'Ready SBOM lacks generator, document hash, or component count'
    }

    foreach ($field in @('Status', 'Reviewer', 'Approver', 'NormalizedPackageSha256', 'SourceCommit', 'OpenCvSourceRevision', 'SigningStatus', 'SbomStatus', 'EvidenceKind', 'RemoteMutationAllowed')) {
        Assert-True -List $List -Condition (Has-Property -Object $Record.Approval -Name $field) -Path $Path -Issue 'Release-input approval provenance is incomplete' -Text $field
    }
    if (@('Status', 'Reviewer', 'Approver', 'NormalizedPackageSha256', 'SourceCommit', 'OpenCvSourceRevision', 'SigningStatus', 'SbomStatus', 'EvidenceKind', 'RemoteMutationAllowed' | Where-Object { -not (Has-Property -Object $Record.Approval -Name $_) }).Count -gt 0) { return }
    Assert-True -List $List -Condition ($Record.Approval.NormalizedPackageSha256 -eq $Record.Normalization.PackageSha256) -Path $Path -Issue 'Approval must cover the exact normalized package hash'
    Assert-True -List $List -Condition ($Record.Approval.SourceCommit -eq $Record.Sbom.SourceCommit -and $Record.Approval.OpenCvSourceRevision -eq $Record.Sbom.OpenCvSourceRevision) -Path $Path -Issue 'Approval source provenance does not match SBOM provenance'
    Assert-True -List $List -Condition ($Record.Approval.SigningStatus -eq $Record.Signing.Status -and $Record.Approval.SbomStatus -eq $Record.Sbom.Status) -Path $Path -Issue 'Approval status does not match signing/SBOM state'
    Assert-True -List $List -Condition (-not [bool]$Record.Approval.RemoteMutationAllowed) -Path $Path -Issue 'Release-input approval must not authorize remote mutation'
    Assert-True -List $List -Condition ($Record.Approval.Reviewer -eq 'automated-local-preflight') -Path $Path -Issue 'Approval reviewer identity drifted'
    switch ([string]$Record.Approval.Status) {
        'not-approved' {
            Assert-True -List $List -Condition ($Record.Approval.Approver -eq 'unassigned' -and $Record.Signing.Status -eq 'not-ready' -and $Record.Sbom.Status -eq 'not-ready' -and $Record.Approval.EvidenceKind -eq 'local-preflight') -Path $Path -Issue 'Unapproved release inputs must remain local and not-ready'
        }
        'approved' {
            Assert-True -List $List -Condition ($Record.Approval.Approver -ne 'unassigned' -and $Record.Signing.Status -eq 'verified' -and $Record.Sbom.Status -eq 'verified' -and $Record.Signing.VerificationResult -eq 'passed') -Path $Path -Issue 'Approved release inputs require verified signing and SBOM evidence'
            Assert-True -List $List -Condition ($Record.Approval.EvidenceKind -eq 'externally-approved-public-inputs') -Path $Path -Issue 'Approved release inputs must identify their evidence kind'
        }
        default { Add-Violation -List $List -Path $Path -Issue 'Approval status is outside the approved state machine' }
    }

    Assert-True -List $List -Condition ([string]::IsNullOrWhiteSpace([string]$Record.FeedReference) -or [string]$Record.FeedReference -eq 'https://api.nuget.org/v3/index.json') -Path $Path -Issue 'Feed reference must be absent or the immutable read-only service index'
    Assert-True -List $List -Condition (-not [bool]$Record.PublicationAttempted -and -not [bool]$Record.PublicationAllowed) -Path $Path -Issue 'Signing boundary must remain non-publishing'
    Assert-True -List $List -Condition (-not [bool]$Record.PrivateKeyMaterialPresent -and -not [bool]$Record.SecretMaterialPresent) -Path $Path -Issue 'Signing boundary must reject private-key and secret material'
}

function Test-PackScriptContract {
    param([Parameter(Mandatory = $true)][string]$Path,[Parameter(Mandatory = $true)][string]$Text,[Parameter(Mandatory = $true)][AllowEmptyCollection()][System.Collections.Generic.List[object]]$List)

    $normalizerMatches = [regex]::Matches($Text, 'Normalize-NuGetPackageDeterminism\.ps1')
    Assert-True -List $List -Condition ($normalizerMatches.Count -eq 1) -Path $Path -Issue 'Pack script must invoke deterministic normalization exactly once'
    Assert-True -List $List -Condition (-not $Text.Contains('.signature.p7s')) -Path $Path -Issue 'Pack script must not carry signed package material'
    $packIndex = $Text.IndexOf('& dotnet @arguments', [StringComparison]::Ordinal)
    $normalizeIndex = $Text.IndexOf('Normalize-NuGetPackageDeterminism.ps1', [StringComparison]::Ordinal)
    Assert-True -List $List -Condition ($packIndex -ge 0 -and $normalizeIndex -gt $packIndex) -Path $Path -Issue 'Package normalization must occur after packing and before any signing boundary'
}

function Test-WorkflowContract {
    param([Parameter(Mandatory = $true)][string]$Path,[Parameter(Mandatory = $true)][string]$Text,[Parameter(Mandatory = $true)][AllowEmptyCollection()][System.Collections.Generic.List[object]]$List)

    foreach ($token in @('dotnet nuget sign', 'nuget sign', 'signtool sign', 'gpg --detach-sign', 'cosign sign', 'minisign -S')) {
        Assert-True -List $List -Condition (-not $Text.Contains($token)) -Path $Path -Issue 'Pack workflow must not sign packages before an approved future boundary' -Text $token
    }
    foreach ($job in @(
            [pscustomobject]@{ Name = 'pack-managed'; PackScript = 'Pack-Managed.ps1' },
            [pscustomobject]@{ Name = 'pack-runtime'; PackScript = 'Pack-Runtime.ps1' }
        )) {
        $match = [regex]::Match($Text, "(?ms)^  $($job.Name):\r?\n.*?(?=^  [A-Za-z0-9_-]+:|\z)")
        Assert-True -List $List -Condition $match.Success -Path $Path -Issue 'Pack workflow job could not be isolated' -Text $job.Name
        if (-not $match.Success) { continue }
        $jobText = $match.Value
        $packIndex = $jobText.IndexOf($job.PackScript, [StringComparison]::Ordinal)
        $pushIndex = $jobText.IndexOf('dotnet nuget push', [StringComparison]::Ordinal)
        if ($pushIndex -ge 0) {
            Assert-True -List $List -Condition ($packIndex -ge 0 -and $packIndex -lt $pushIndex) -Path $Path -Issue 'Publishing must consume output from the pack script after normalization' -Text $job.Name
        }
    }
}

$temporaryRoot = Join-Path ([IO.Path]::GetTempPath()) ('opencv-csharp-signing-boundary-' + [guid]::NewGuid().ToString('N'))
$normalizerPath = Join-Path $repo 'scripts/Normalize-NuGetPackageDeterminism.ps1'
try {
    New-Item -ItemType Directory -Force -Path $temporaryRoot | Out-Null
    $unsignedPath = Join-Path $temporaryRoot 'JYPPX.OpenCV.runtime.win-x64.5.0.0.nupkg'
    New-UnsignedFixture -Path $unsignedPath
    $preNormalizationHash = (Get-FileHash -LiteralPath $unsignedPath -Algorithm SHA256).Hash.ToLowerInvariant()
    & pwsh -NoProfile -File $normalizerPath -PackagePath $unsignedPath | Out-Host
    Assert-True -List $violations -Condition ($LASTEXITCODE -eq 0) -Path $unsignedPath -Issue 'Unsigned package normalization failed'
    $normalizedFacts = Get-PackageFacts -Path $unsignedPath
    Assert-True -List $violations -Condition ($preNormalizationHash -ne $normalizedFacts.Hash) -Path $unsignedPath -Issue 'Fixture did not produce a distinct pre-normalization hash'

    $unsignedRecord = New-BoundaryRecord -PackageHash $normalizedFacts.Hash -NormalizedHash $normalizedFacts.Hash -PreNormalizationHash $preNormalizationHash -EntryCount $normalizedFacts.EntryCount
    $unsignedJson = $unsignedRecord | ConvertTo-Json -Depth 12
    Test-BoundaryRecord -Record ($unsignedJson | ConvertFrom-Json) -PackagePath $unsignedPath -ExpectedPackageHash $normalizedFacts.Hash -ExpectedNormalizedHash $normalizedFacts.Hash -ExpectedPreNormalizationHash $preNormalizationHash -ExpectedEntryCount $normalizedFacts.EntryCount -List $violations -Path "$temporaryRoot/unsigned-record.json"

    $signedPath = Join-Path $temporaryRoot 'signed.nupkg'
    [IO.File]::Copy($unsignedPath, $signedPath, $true)
    Add-SignatureFixtureEntry -Path $signedPath
    $signedFacts = Get-PackageFacts -Path $signedPath
    $signedRecord = New-BoundaryRecord -PackageHash $signedFacts.Hash -NormalizedHash $normalizedFacts.Hash -PreNormalizationHash $preNormalizationHash -EntryCount $normalizedFacts.EntryCount -SigningStatus 'verified' -PostSigningHash $signedFacts.Hash
    Set-SignedRecordInputs -Record $signedRecord
    $signedJson = $signedRecord | ConvertTo-Json -Depth 12
    Test-BoundaryRecord -Record ($signedJson | ConvertFrom-Json) -PackagePath $signedPath -ExpectedPackageHash $signedFacts.Hash -ExpectedNormalizedHash $normalizedFacts.Hash -ExpectedPreNormalizationHash $preNormalizationHash -ExpectedEntryCount $normalizedFacts.EntryCount -List $violations -Path "$temporaryRoot/signed-record.json"
    $null = & pwsh -NoProfile -File $normalizerPath -PackagePath $signedPath 2>&1
    Assert-True -List $violations -Condition ($LASTEXITCODE -ne 0) -Path $signedPath -Issue 'Normalizer accepted a package after signing'

    foreach ($case in @('pre-normalization hash drift', 'post-signing byte drift', 'missing normalization evidence', 'signed state with private key', 'mutable certificate/timestamp URL', 'SBOM hash drift', 'missing provenance', 'approval hash drift', 'approval without verification', 'approval with remote mutation', 'signed without approval')) {
        $copy = $signedJson | ConvertFrom-Json
        switch ($case) {
            'pre-normalization hash drift' { $copy.Signing.InputPackageSha256 = $preNormalizationHash }
            'post-signing byte drift' { $copy.Signing.PostSigningPackageSha256 = '0' * 64 }
            'missing normalization evidence' { $copy.Normalization = $null }
            'signed state with private key' { $copy.Signing.PrivateKeyMaterialPresent = $true }
            'mutable certificate/timestamp URL' { $copy.Signing.CertificateReference = 'https://example.invalid/latest.pem'; $copy.Signing.TimestampAuthorityReference = 'https://tsa.example.invalid/timestamp/latest' }
            'SBOM hash drift' { $copy.Sbom.PackageSha256 = '0' * 64 }
            'missing provenance' { $copy.Sbom.NativeModules = @(); $copy.Sbom.LicenseEvidence = @() }
            'approval hash drift' { $copy.Approval.NormalizedPackageSha256 = '0' * 64 }
            'approval without verification' { $copy.Approval.SigningStatus = 'verified'; $copy.Approval.SbomStatus = 'not-ready' }
            'approval with remote mutation' { $copy.Approval.RemoteMutationAllowed = $true }
            'signed without approval' { $copy.Approval.Status = 'not-approved'; $copy.Approval.Approver = 'unassigned'; $copy.Approval.SigningStatus = 'verified'; $copy.Approval.SbomStatus = 'verified' }
        }
        $caseViolations = [System.Collections.Generic.List[object]]::new()
        Test-BoundaryRecord -Record $copy -PackagePath $signedPath -ExpectedPackageHash $signedFacts.Hash -ExpectedNormalizedHash $normalizedFacts.Hash -ExpectedPreNormalizationHash $preNormalizationHash -ExpectedEntryCount $normalizedFacts.EntryCount -List $caseViolations -Path "$signedPath/$case"
        Assert-True -List $violations -Condition ($caseViolations.Count -gt 0) -Path $case -Issue 'Signing boundary negative fixture was accepted'
    }

    foreach ($packScriptName in @('Pack-Managed.ps1', 'Pack-Runtime.ps1')) {
        $packPath = Join-Path $repo "scripts/$packScriptName"
        $packText = [IO.File]::ReadAllText($packPath)
        Test-PackScriptContract -Path $packPath -Text $packText -List $violations
        $badPackViolations = [System.Collections.Generic.List[object]]::new()
        Test-PackScriptContract -Path "$packPath/missing-normalizer" -Text ($packText -replace 'Normalize-NuGetPackageDeterminism\.ps1', '') -List $badPackViolations
        Assert-True -List $violations -Condition ($badPackViolations.Count -gt 0) -Path "$packPath/missing-normalizer" -Issue 'Pack script missing-normalizer fixture was accepted'
    }

    $packWorkflowPath = Join-Path $repo '.github/workflows/pack.yml'
    $packWorkflowText = [IO.File]::ReadAllText($packWorkflowPath)
    Test-WorkflowContract -Path $packWorkflowPath -Text $packWorkflowText -List $violations
    $badWorkflowViolations = [System.Collections.Generic.List[object]]::new()
    Test-WorkflowContract -Path "$packWorkflowPath/direct-signing" -Text ($packWorkflowText + "`n dotnet nuget sign package.nupkg") -List $badWorkflowViolations
    Assert-True -List $violations -Condition ($badWorkflowViolations.Count -gt 0) -Path "$packWorkflowPath/direct-signing" -Issue 'Workflow direct-signing fixture was accepted'

    Write-Host "RELEASE_SIGNING_BOUNDARY_OK normalized_sha256=$($normalizedFacts.Hash) signed_sha256=$($signedFacts.Hash) entries=$($normalizedFacts.EntryCount) sbom_format=SPDX-2.3 approval=not-approved-by-default publication=blocked"
}
catch {
    Add-Violation -List $violations -Path $temporaryRoot -Issue 'Release signing boundary execution failed' -Text $_.Exception.Message
}
finally {
    if (Test-Path -LiteralPath $temporaryRoot -PathType Container) { [IO.Directory]::Delete((Resolve-Path -LiteralPath $temporaryRoot).Path, $true) }
}

if ($violations.Count -gt 0) {
    Write-Host "Release signing boundary failed with $($violations.Count) violation(s)."
    $violations | Format-List Path, Issue, Text
    exit 1
}

Write-Host 'Release signing boundary passed.'
Write-Host 'Validated normalized-input signing, post-signing hash binding, immutable public references, SPDX-2.3 provenance, explicit approval scope, workflow ordering, private-key exclusion, and publication blocking.'
Write-Host 'Negative fixtures rejected: pre-normalization hash drift, post-signing byte drift, missing normalization, private key, mutable references, SBOM drift, missing provenance, approval hash/status/mutation drift, missing pack normalization, direct workflow signing.'
