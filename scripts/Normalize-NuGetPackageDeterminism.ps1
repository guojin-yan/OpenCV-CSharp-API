param(
    [Parameter(Mandatory = $true)]
    [string]$PackagePath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.IO.Compression.FileSystem

$resolvedPackagePath = (Resolve-Path -LiteralPath $PackagePath).Path
if ([IO.Path]::GetExtension($resolvedPackagePath) -ne '.nupkg') {
    throw "Package path must end in .nupkg: $resolvedPackagePath"
}

$temporaryPackagePath = "$resolvedPackagePath.$([guid]::NewGuid().ToString('N')).tmp"
$fixedTimestamp = [DateTimeOffset]::new(2000, 1, 1, 0, 0, 0, [TimeSpan]::Zero)

function Read-EntryBytes {
    param([Parameter(Mandatory = $true)][IO.Compression.ZipArchiveEntry]$Entry)

    $inputStream = $Entry.Open()
    $memoryStream = [IO.MemoryStream]::new()
    try {
        $inputStream.CopyTo($memoryStream)
        return $memoryStream.ToArray()
    }
    finally {
        $memoryStream.Dispose()
        $inputStream.Dispose()
    }
}

function Convert-RootRelationships {
    param([Parameter(Mandatory = $true)][byte[]]$Bytes)

    $text = [Text.UTF8Encoding]::new($false).GetString($Bytes)
    [xml]$xml = $text
    $namespaceManager = [Xml.XmlNamespaceManager]::new($xml.NameTable)
    $namespaceManager.AddNamespace('r', 'http://schemas.openxmlformats.org/package/2006/relationships')
    $relationships = @($xml.SelectNodes('//r:Relationship', $namespaceManager) | Sort-Object Type, Target)
    $index = 1
    foreach ($relationship in $relationships) {
        $relationship.Id = "R$index"
        if ([string]$relationship.Target -match '^/package/services/metadata/core-properties/[^/]+\.psmdcp$') {
            $relationship.Target = '/package/services/metadata/core-properties/core-properties.psmdcp'
        }
        $index++
    }

    $settings = [Xml.XmlWriterSettings]::new()
    $settings.Encoding = [Text.UTF8Encoding]::new($false)
    $settings.Indent = $true
    $settings.NewLineChars = "`n"
    $settings.NewLineHandling = [Xml.NewLineHandling]::Replace
    $stringWriter = [IO.StringWriter]::new([Globalization.CultureInfo]::InvariantCulture)
    try {
        $xmlWriter = [Xml.XmlWriter]::Create($stringWriter, $settings)
        try { $xml.Save($xmlWriter) }
        finally { $xmlWriter.Dispose() }
        return [Text.UTF8Encoding]::new($false).GetBytes($stringWriter.ToString())
    }
    finally { $stringWriter.Dispose() }
}

$sourceArchive = $null
$destinationArchive = $null
try {
    $sourceArchive = [IO.Compression.ZipFile]::OpenRead($resolvedPackagePath)
    $entries = [Collections.Generic.Dictionary[string, byte[]]]::new([StringComparer]::Ordinal)
    foreach ($sourceEntry in $sourceArchive.Entries) {
        if ($sourceEntry.FullName -match '(?i)(^|/)\.signature\.p7s$') {
            throw "Package normalization must run before signing and refuses signed package input: $($sourceEntry.FullName)"
        }

        $normalizedName = $sourceEntry.FullName
        if ($normalizedName -match '^package/services/metadata/core-properties/[^/]+\.psmdcp$') {
            $normalizedName = 'package/services/metadata/core-properties/core-properties.psmdcp'
        }
        if ($entries.ContainsKey($normalizedName)) {
            throw "Package contains duplicate entries after deterministic normalization: $normalizedName"
        }

        $bytes = Read-EntryBytes -Entry $sourceEntry
        if ($normalizedName -eq '_rels/.rels') {
            $bytes = Convert-RootRelationships -Bytes $bytes
        }
        $entries.Add($normalizedName, $bytes)
    }

    $destinationArchive = [IO.Compression.ZipFile]::Open($temporaryPackagePath, [IO.Compression.ZipArchiveMode]::Create)
    $names = [Collections.Generic.List[string]]::new()
    foreach ($name in $entries.Keys) { $names.Add($name) }
    $names.Sort([StringComparer]::Ordinal)
    foreach ($name in $names) {
        $destinationEntry = $destinationArchive.CreateEntry($name, [IO.Compression.CompressionLevel]::Optimal)
        $destinationEntry.LastWriteTime = $fixedTimestamp
        $bytes = $entries[$name]
        if ($bytes.Length -gt 0) {
            $outputStream = $destinationEntry.Open()
            try { $outputStream.Write($bytes, 0, $bytes.Length) }
            finally { $outputStream.Dispose() }
        }
    }
}
finally {
    if ($null -ne $destinationArchive) { $destinationArchive.Dispose() }
    if ($null -ne $sourceArchive) { $sourceArchive.Dispose() }
}

try {
    [IO.File]::Move($temporaryPackagePath, $resolvedPackagePath, $true)
}
catch {
    if ([IO.File]::Exists($temporaryPackagePath)) { [IO.File]::Delete($temporaryPackagePath) }
    throw
}

$packageInfo = Get-Item -LiteralPath $resolvedPackagePath
$packageHash = (Get-FileHash -LiteralPath $resolvedPackagePath -Algorithm SHA256).Hash.ToLowerInvariant()
Write-Host "NUGET_PACKAGE_NORMALIZED package=$($packageInfo.Name) sha256=$packageHash bytes=$($packageInfo.Length) entries=$($names.Count) timestamp=2000-01-01T00:00:00Z"
