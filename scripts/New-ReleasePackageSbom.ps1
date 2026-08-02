[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$PackagePath,
    [Parameter(Mandatory = $true)]
    [ValidatePattern('^[0-9a-f]{40}$')]
    [string]$SourceCommit,
    [Parameter(Mandatory = $true)]
    [ValidatePattern('^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}Z$')]
    [string]$Created,
    [string]$OpenCvVersion = "5.0.0",
    [string]$RepositoryUrl = "https://github.com/guojin-yan/OpenCV-CSharp-API",
    [string]$OutputPath = "",
    [switch]$Check
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

Add-Type -AssemblyName System.IO.Compression.FileSystem
. (Join-Path $PSScriptRoot "PackageVersion.ps1")

$generatorIdentity = "OpenCV-CSharp-API/scripts/New-ReleasePackageSbom.ps1@1.0.0"
$normalizedTimestamp = [DateTimeOffset]::new(2000, 1, 1, 0, 0, 0, [TimeSpan]::Zero)
$maxEntryBytes = 512MB
$maxPackageContentBytes = 2GB

function Get-HashHex {
    param(
        [Parameter(Mandatory = $true)]
        [byte[]]$Bytes,
        [Parameter(Mandatory = $true)]
        [ValidateSet("SHA1", "SHA256")]
        [string]$Algorithm
    )

    $hash = if ($Algorithm -eq "SHA1") {
        [Security.Cryptography.SHA1]::HashData($Bytes)
    }
    else {
        [Security.Cryptography.SHA256]::HashData($Bytes)
    }

    return [Convert]::ToHexString($hash).ToLowerInvariant()
}

function Get-EntryBytes {
    param(
        [Parameter(Mandatory = $true)]
        [IO.Compression.ZipArchiveEntry]$Entry
    )

    $stream = $Entry.Open()
    try {
        $memory = [IO.MemoryStream]::new()
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

function Read-SecureXml {
    param(
        [Parameter(Mandatory = $true)]
        [byte[]]$Bytes
    )

    $settings = [Xml.XmlReaderSettings]::new()
    $settings.DtdProcessing = [Xml.DtdProcessing]::Prohibit
    $settings.XmlResolver = $null

    $memory = [IO.MemoryStream]::new($Bytes, $false)
    try {
        $reader = [Xml.XmlReader]::Create($memory, $settings)
        try {
            $document = [Xml.XmlDocument]::new()
            $document.XmlResolver = $null
            $document.Load($reader)
            return $document
        }
        finally {
            $reader.Dispose()
        }
    }
    finally {
        $memory.Dispose()
    }
}

function Get-NuspecValue {
    param(
        [Parameter(Mandatory = $true)]
        [Xml.XmlDocument]$Nuspec,
        [Parameter(Mandatory = $true)]
        [string]$Name
    )

    $node = $Nuspec.SelectSingleNode("/*[local-name()='package']/*[local-name()='metadata']/*[local-name()='$Name']")
    if ($null -eq $node) {
        return ""
    }

    return $node.InnerText.Trim()
}

function Test-NormalizedNuGetVersion {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Version
    )

    $match = [regex]::Match(
        $Version,
        '^(?<major>0|[1-9][0-9]*)\.(?<minor>0|[1-9][0-9]*)\.(?<patch>0|[1-9][0-9]*)(?:\.(?<revision>[1-9][0-9]*))?(?:-(?<prerelease>[0-9a-z-]+(?:\.[0-9a-z-]+)*))?$')
    if (-not $match.Success) {
        return $false
    }

    foreach ($name in @("major", "minor", "patch", "revision")) {
        if (-not $match.Groups[$name].Success) {
            continue
        }

        $number = 0
        if (-not [int]::TryParse(
                $match.Groups[$name].Value,
                [Globalization.NumberStyles]::None,
                [Globalization.CultureInfo]::InvariantCulture,
                [ref]$number)) {
            return $false
        }
    }

    foreach ($identifier in $match.Groups["prerelease"].Value.Split('.', [StringSplitOptions]::RemoveEmptyEntries)) {
        if ($identifier -match '^[0-9]+$' -and $identifier.Length -gt 1 -and $identifier[0] -eq '0') {
            return $false
        }
    }

    return $true
}

function Get-PackageEntries {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    $archive = [IO.Compression.ZipFile]::OpenRead($Path)
    try {
        $records = [Collections.Generic.Dictionary[string, object]]::new([StringComparer]::Ordinal)
        $caseInsensitiveNames = [Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
        [long]$totalLength = 0

        foreach ($entry in $archive.Entries) {
            $name = $entry.FullName.Replace('\', '/')
            if ([string]::IsNullOrWhiteSpace($name) -or $name.EndsWith('/')) {
                continue
            }

            $segments = @($name.Split('/'))
            if ($name.StartsWith('/') -or
                $name.Contains(':') -or
                $segments -contains '..' -or
                $segments -contains '.' -or
                @($segments | Where-Object { [string]::IsNullOrEmpty($_) }).Count -gt 0) {
                throw "Package contains an unsafe entry path: $name"
            }

            if (-not $caseInsensitiveNames.Add($name)) {
                throw "Package contains duplicate or case-colliding entry paths: $name"
            }

            if ($name.Equals('.signature.p7s', [StringComparison]::OrdinalIgnoreCase)) {
                throw "SBOM generation requires the normalized unsigned package before signing: $Path"
            }

            if ($entry.Length -lt 0 -or $entry.Length -gt $maxEntryBytes) {
                throw "Package entry exceeds the supported SBOM inspection size: $name ($($entry.Length) bytes)"
            }

            if ($entry.Length -gt ($maxPackageContentBytes - $totalLength)) {
                throw "Package uncompressed content exceeds the supported SBOM inspection size: $totalLength bytes"
            }
            $totalLength += $entry.Length

            if ($entry.LastWriteTime.DateTime -ne $normalizedTimestamp.DateTime) {
                throw "Package entry timestamp is not deterministically normalized to $($normalizedTimestamp.ToString('O')): $name"
            }

            $bytes = Get-EntryBytes -Entry $entry
            if ($bytes.LongLength -ne $entry.Length) {
                throw "Package entry length changed while reading: $name"
            }

            $records.Add($name, [pscustomobject]@{
                    Path = $name
                    Length = [long]$bytes.LongLength
                    Sha1 = Get-HashHex -Bytes $bytes -Algorithm SHA1
                    Sha256 = Get-HashHex -Bytes $bytes -Algorithm SHA256
                    Bytes = $bytes
                })
        }

        if ($records.Count -eq 0) {
            throw "Package contains no file entries: $Path"
        }

        [string[]]$paths = @($records.Keys)
        [Array]::Sort($paths, [StringComparer]::Ordinal)
        return @($paths | ForEach-Object { $records[$_] })
    }
    finally {
        $archive.Dispose()
    }
}

function Get-SpdxFileType {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    $extension = [IO.Path]::GetExtension($Path).ToLowerInvariant()
    if ($extension -in @('.dll', '.so', '.dylib', '.exe')) {
        return "BINARY"
    }

    if ($extension -in @('.md', '.txt', '.xml', '.json', '.nuspec', '.psmdcp', '.ijg')) {
        return "TEXT"
    }

    return "OTHER"
}

function Get-PackageVerificationCode {
    param(
        [Parameter(Mandatory = $true)]
        [object[]]$Entries
    )

    [string[]]$hashes = @($Entries | ForEach-Object { [string]$_.Sha1 })
    [Array]::Sort($hashes, [StringComparer]::Ordinal)
    $joined = [Text.Encoding]::ASCII.GetBytes([string]::Concat($hashes))
    return Get-HashHex -Bytes $joined -Algorithm SHA1
}

$packageFullPath = [IO.Path]::GetFullPath($PackagePath)
if (-not (Test-Path -LiteralPath $packageFullPath -PathType Leaf)) {
    throw "Release package was not found: $packageFullPath"
}

if (-not $packageFullPath.EndsWith('.nupkg', [StringComparison]::OrdinalIgnoreCase)) {
    throw "SBOM input must be a .nupkg file: $packageFullPath"
}

if ($RepositoryUrl -ne 'https://github.com/guojin-yan/OpenCV-CSharp-API') {
    throw "RepositoryUrl must identify the authoritative release repository. Actual: $RepositoryUrl"
}

if ($OpenCvVersion -notmatch '^(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)$') {
    throw "OpenCvVersion must use three numeric parts. Actual: $OpenCvVersion"
}

$createdTimestamp = [DateTimeOffset]::MinValue
if (-not [DateTimeOffset]::TryParseExact(
        $Created,
        "yyyy-MM-dd'T'HH:mm:ss'Z'",
        [Globalization.CultureInfo]::InvariantCulture,
        [Globalization.DateTimeStyles]::AssumeUniversal -bor [Globalization.DateTimeStyles]::AdjustToUniversal,
        [ref]$createdTimestamp)) {
    throw "Created must be a factual UTC RFC3339 timestamp with whole seconds. Actual: $Created"
}

if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $OutputPath = [IO.Path]::ChangeExtension($packageFullPath, '.spdx.json')
}
$outputFullPath = [IO.Path]::GetFullPath($OutputPath)
if ($outputFullPath.Equals($packageFullPath, [StringComparison]::OrdinalIgnoreCase)) {
    throw "SBOM output path must differ from the package input path."
}

$entries = @(Get-PackageEntries -Path $packageFullPath)
$nuspecEntries = @($entries | Where-Object { $_.Path.EndsWith('.nuspec', [StringComparison]::OrdinalIgnoreCase) })
if ($nuspecEntries.Count -ne 1) {
    throw "Package must contain exactly one nuspec entry. Found: $($nuspecEntries.Count)"
}

$nuspec = Read-SecureXml -Bytes $nuspecEntries[0].Bytes
$packageId = Get-NuspecValue -Nuspec $nuspec -Name 'id'
$packageVersion = Get-NuspecValue -Nuspec $nuspec -Name 'version'
$licenseExpression = Get-NuspecValue -Nuspec $nuspec -Name 'license'
$authors = Get-NuspecValue -Nuspec $nuspec -Name 'authors'
$copyright = Get-NuspecValue -Nuspec $nuspec -Name 'copyright'

if ($packageId -notmatch '^JYPPX\.OpenCV(?:\.CSharp\.API|\.runtime\.[a-z0-9.-]+(?:\.mini)?)$') {
    throw "Package ID is outside the version-neutral release identity contract: $packageId"
}

if (-not (Test-NormalizedNuGetVersion -Version $packageVersion)) {
    throw "Nuspec version is not a canonical NuGet version: $packageVersion"
}

$expectedFileName = "$packageId.$packageVersion.nupkg"
$actualFileName = [IO.Path]::GetFileName($packageFullPath)
if ($actualFileName -cne $expectedFileName) {
    throw "Package filename does not match the exact nuspec identity. Expected: $expectedFileName; actual: $actualFileName"
}

if ([string]::IsNullOrWhiteSpace($licenseExpression)) {
    throw "Nuspec must declare a package license expression."
}

if ([string]::IsNullOrWhiteSpace($authors)) {
    throw "Nuspec must declare package authors."
}

$repositoryNode = $nuspec.SelectSingleNode("/*[local-name()='package']/*[local-name()='metadata']/*[local-name()='repository']")
if ($null -eq $repositoryNode) {
    throw "Nuspec must contain repository provenance."
}

$nuspecRepositoryUrl = $repositoryNode.Attributes['url'].Value
$nuspecRepositoryType = $repositoryNode.Attributes['type'].Value
$nuspecRepositoryCommit = $repositoryNode.Attributes['commit'].Value
if ($nuspecRepositoryUrl -ne $RepositoryUrl -or $nuspecRepositoryType -ne 'git' -or $nuspecRepositoryCommit -ne $SourceCommit) {
    throw "Nuspec repository provenance does not match the authoritative repository and source commit."
}

$runtimeProvenanceEntries = @($entries | Where-Object { $_.Path -ceq 'build/JYPPX.OpenCV.runtime.provenance.json' })
$isRuntimePackage = $packageId.StartsWith('JYPPX.OpenCV.runtime.', [StringComparison]::Ordinal)
$runtimeModules = @()
$runtimeProfile = ''
$runtimeRid = ''
$sourcePackageVersion = ''
if ($isRuntimePackage) {
    if ($runtimeProvenanceEntries.Count -ne 1) {
        throw "Runtime packages must contain exactly one runtime provenance manifest."
    }

    $runtimeProvenance = [Text.UTF8Encoding]::new($false, $true).GetString($runtimeProvenanceEntries[0].Bytes) | ConvertFrom-Json
    foreach ($field in @('PackageId', 'PackageVersion', 'OpenCvVersion', 'Rid', 'RuntimeProfile', 'SyntheticRuntimeInputs', 'RequiredModules', 'OptionalModulesStaged')) {
        if ($null -eq $runtimeProvenance.PSObject.Properties[$field]) {
            throw "Runtime provenance is missing required field: $field"
        }
    }

    if ($runtimeProvenance.PackageId -ne $packageId -or
        $runtimeProvenance.OpenCvVersion -ne $OpenCvVersion -or
        [bool]$runtimeProvenance.SyntheticRuntimeInputs) {
        throw "Runtime provenance identity, OpenCV version, or real-input status is not release eligible."
    }

    $sourceVersionRecord = ConvertTo-OpenCvCSharpPackageVersion -Version ([string]$runtimeProvenance.PackageVersion)
    if ($sourceVersionRecord.NuGetVersion -ne $packageVersion -or $sourceVersionRecord.OpenCvVersion -ne $OpenCvVersion) {
        throw "Runtime provenance source package version does not normalize to the nuspec/OpenCV identity."
    }

    $runtimeRid = [string]$runtimeProvenance.Rid
    $runtimeProfile = [string]$runtimeProvenance.RuntimeProfile
    $sourcePackageVersion = [string]$runtimeProvenance.PackageVersion
    if ([string]::IsNullOrWhiteSpace($runtimeRid) -or $runtimeProfile -notin @('full', 'mini')) {
        throw "Runtime provenance RID/profile is incomplete."
    }

    $runtimeModules = @(
        @($runtimeProvenance.RequiredModules) + @($runtimeProvenance.OptionalModulesStaged) |
            ForEach-Object { [string]$_ } |
            Where-Object { -not [string]::IsNullOrWhiteSpace($_) } |
            Sort-Object -Unique
    )
    if ($runtimeModules.Count -eq 0) {
        throw "Runtime provenance must identify at least one OpenCV module."
    }

    $licenseEntries = @($entries | Where-Object { $_.Path.StartsWith('licenses/', [StringComparison]::Ordinal) })
    if ($licenseEntries.Count -eq 0) {
        throw "Runtime package SBOM generation requires packaged license evidence."
    }
}
elseif ($runtimeProvenanceEntries.Count -ne 0) {
    throw "Managed package must not contain runtime provenance."
}

$packageBytes = [IO.File]::ReadAllBytes($packageFullPath)
$packageSha256 = Get-HashHex -Bytes $packageBytes -Algorithm SHA256
$packageVerificationCode = Get-PackageVerificationCode -Entries $entries
$packageSpdxId = 'SPDXRef-Package-' + ($packageId -replace '[^A-Za-z0-9.-]', '-')
$openCvSpdxId = 'SPDXRef-Package-OpenCV'

$spdxFiles = [Collections.Generic.List[object]]::new()
$relationships = [Collections.Generic.List[object]]::new()
$relationships.Add([ordered]@{
        spdxElementId = 'SPDXRef-DOCUMENT'
        relationshipType = 'DESCRIBES'
        relatedSpdxElement = $packageSpdxId
    })
$relationships.Add([ordered]@{
        spdxElementId = $packageSpdxId
        relationshipType = 'DEPENDS_ON'
        relatedSpdxElement = $openCvSpdxId
    })

for ($index = 0; $index -lt $entries.Count; $index++) {
    $entry = $entries[$index]
    $fileSpdxId = 'SPDXRef-File-{0:D4}-{1}' -f ($index + 1), $entry.Sha256.Substring(0, 12)
    $spdxFiles.Add([ordered]@{
            SPDXID = $fileSpdxId
            fileName = './' + $entry.Path
            fileTypes = @((Get-SpdxFileType -Path $entry.Path))
            checksums = @(
                [ordered]@{ algorithm = 'SHA1'; checksumValue = $entry.Sha1 },
                [ordered]@{ algorithm = 'SHA256'; checksumValue = $entry.Sha256 }
            )
            licenseConcluded = 'NOASSERTION'
            copyrightText = 'NOASSERTION'
        })
    $relationships.Add([ordered]@{
            spdxElementId = $packageSpdxId
            relationshipType = 'CONTAINS'
            relatedSpdxElement = $fileSpdxId
        })
}

$packageComment = if ($isRuntimePackage) {
    "Runtime RID/profile: $runtimeRid/$runtimeProfile; strict source package version: $sourcePackageVersion; OpenCV modules: $($runtimeModules -join ','); synthetic inputs: false."
}
else {
    "Managed OpenCV CSharp API binding; a matching RID/profile runtime package supplies the native OpenCV $OpenCvVersion implementation."
}

$supplier = if ([string]::IsNullOrWhiteSpace($authors)) { 'NOASSERTION' } else { "Person: $authors" }
$copyrightText = if ([string]::IsNullOrWhiteSpace($copyright)) { 'NOASSERTION' } else { $copyright }
$documentNamespace = "$RepositoryUrl/sbom/$([Uri]::EscapeDataString($packageId))/$([Uri]::EscapeDataString($packageVersion))/$packageSha256"

$document = [ordered]@{
    spdxVersion = 'SPDX-2.3'
    dataLicense = 'CC0-1.0'
    SPDXID = 'SPDXRef-DOCUMENT'
    name = "$packageId-$packageVersion-sbom"
    documentNamespace = $documentNamespace
    creationInfo = [ordered]@{
        created = $Created
        creators = @("Tool: $generatorIdentity")
        comment = "Deterministic release SBOM for normalized package SHA256 $packageSha256 at source commit $SourceCommit."
    }
    packages = @(
        [ordered]@{
            SPDXID = $packageSpdxId
            name = $packageId
            versionInfo = $packageVersion
            packageFileName = $actualFileName
            supplier = $supplier
            downloadLocation = 'NOASSERTION'
            filesAnalyzed = $true
            packageVerificationCode = [ordered]@{
                packageVerificationCodeValue = $packageVerificationCode
            }
            checksums = @(
                [ordered]@{ algorithm = 'SHA256'; checksumValue = $packageSha256 }
            )
            homepage = $RepositoryUrl
            sourceInfo = "$RepositoryUrl commit $SourceCommit; OpenCV $OpenCvVersion."
            licenseConcluded = $licenseExpression
            licenseDeclared = $licenseExpression
            copyrightText = $copyrightText
            primaryPackagePurpose = 'LIBRARY'
            externalRefs = @(
                [ordered]@{
                    referenceCategory = 'PACKAGE-MANAGER'
                    referenceType = 'purl'
                    referenceLocator = "pkg:nuget/$packageId@$packageVersion"
                }
            )
            comment = $packageComment
        },
        [ordered]@{
            SPDXID = $openCvSpdxId
            name = 'OpenCV'
            versionInfo = $OpenCvVersion
            supplier = 'Organization: OpenCV'
            downloadLocation = "https://github.com/opencv/opencv/archive/refs/tags/$OpenCvVersion.tar.gz"
            filesAnalyzed = $false
            licenseConcluded = 'Apache-2.0'
            licenseDeclared = 'Apache-2.0'
            copyrightText = 'NOASSERTION'
            primaryPackagePurpose = 'LIBRARY'
            externalRefs = @(
                [ordered]@{
                    referenceCategory = 'PACKAGE-MANAGER'
                    referenceType = 'purl'
                    referenceLocator = "pkg:github/opencv/opencv@$OpenCvVersion"
                }
            )
        }
    )
    files = @($spdxFiles)
    relationships = @($relationships)
}

$json = $document | ConvertTo-Json -Depth 12
$normalizedJson = (($json -replace "`r`n", "`n") -replace "`r", "`n").TrimEnd() + "`n"
$outputBytes = [Text.UTF8Encoding]::new($false).GetBytes($normalizedJson)
$documentSha256 = Get-HashHex -Bytes $outputBytes -Algorithm SHA256

if ($Check) {
    if (-not (Test-Path -LiteralPath $outputFullPath -PathType Leaf)) {
        throw "SPDX document was not found for check mode: $outputFullPath"
    }

    $actualBytes = [IO.File]::ReadAllBytes($outputFullPath)
    if (-not [Linq.Enumerable]::SequenceEqual($outputBytes, $actualBytes)) {
        throw "SPDX document is stale or does not match the exact package/source inputs: $outputFullPath"
    }
}
else {
    $outputDirectory = Split-Path -Parent $outputFullPath
    New-Item -ItemType Directory -Force -Path $outputDirectory | Out-Null
    $temporaryPath = "$outputFullPath.tmp-$PID-$([guid]::NewGuid().ToString('N'))"
    try {
        [IO.File]::WriteAllBytes($temporaryPath, $outputBytes)
        [IO.File]::Move($temporaryPath, $outputFullPath, $true)
    }
    finally {
        if (Test-Path -LiteralPath $temporaryPath -PathType Leaf) {
            [IO.File]::Delete($temporaryPath)
        }
    }
}

$mode = if ($Check) { 'check' } else { 'write' }
Write-Host "SPDX_PACKAGE_SBOM_OK package=$packageId/$packageVersion package_sha256=$packageSha256 document_sha256=$documentSha256 files=$($entries.Count) modules=$($runtimeModules.Count) mode=$mode"
Write-Host "SPDX document: $outputFullPath"
