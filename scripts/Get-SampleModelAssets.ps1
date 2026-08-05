[CmdletBinding()]
param(
    [string[]]$Bundle = @("all"),
    [string]$OutputRoot = "",
    [switch]$Refresh
)

$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

$repo = Split-Path $PSScriptRoot -Parent
$manifestPath = Join-Path $repo "samples/assets/models/model-assets.json"
if (-not (Test-Path -LiteralPath $manifestPath -PathType Leaf)) {
    throw "Sample model asset manifest was not found: $manifestPath"
}

$manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
if ([int]$manifest.schemaVersion -ne 1) {
    throw "Unsupported sample model asset manifest schema: $($manifest.schemaVersion)"
}

$cacheRoot = if ([string]::IsNullOrWhiteSpace($OutputRoot)) {
    Join-Path $repo ([string]$manifest.defaultCacheDirectory)
}
else {
    if ([IO.Path]::IsPathRooted($OutputRoot)) { $OutputRoot } else { Join-Path $repo $OutputRoot }
}
$cacheRoot = [IO.Path]::GetFullPath($cacheRoot)
[IO.Directory]::CreateDirectory($cacheRoot) | Out-Null
$cachePrefix = $cacheRoot.TrimEnd([IO.Path]::DirectorySeparatorChar, [IO.Path]::AltDirectorySeparatorChar) + [IO.Path]::DirectorySeparatorChar

$fileById = @{}
foreach ($file in @($manifest.files)) {
    $id = [string]$file.id
    if ([string]::IsNullOrWhiteSpace($id) -or $fileById.ContainsKey($id)) {
        throw "Sample model asset file ids must be non-empty and unique: '$id'"
    }
    $fileById[$id] = $file
}

$selectedBundleIds = @($Bundle | ForEach-Object { [string]$_ } | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
if ($selectedBundleIds.Count -eq 0 -or $selectedBundleIds -contains "all") {
    $selectedBundleIds = @($manifest.bundles | ForEach-Object { [string]$_.id })
}

$selectedFileIds = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
foreach ($bundleId in $selectedBundleIds) {
    $matches = @($manifest.bundles | Where-Object { [string]$_.id -ceq $bundleId })
    if ($matches.Count -ne 1) {
        throw "Unknown or duplicate sample model asset bundle: $bundleId"
    }
    foreach ($fileId in @($matches[0].fileIds)) {
        if (-not $fileById.ContainsKey([string]$fileId)) {
            throw "Bundle '$bundleId' references an unknown file id: $fileId"
        }
        [void]$selectedFileIds.Add([string]$fileId)
    }
}

foreach ($fileId in @($selectedFileIds | Sort-Object)) {
    $file = $fileById[$fileId]
    $relativePath = ([string]$file.relativePath).Replace('/', [IO.Path]::DirectorySeparatorChar)
    if ([string]::IsNullOrWhiteSpace($relativePath) -or [IO.Path]::IsPathRooted($relativePath)) {
        throw "Asset '$fileId' has an invalid relative path."
    }

    $targetPath = [IO.Path]::GetFullPath((Join-Path $cacheRoot $relativePath))
    if (-not $targetPath.StartsWith($cachePrefix, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Asset '$fileId' escapes the cache root."
    }

    $expectedSize = [long]$file.sizeBytes
    $expectedHash = ([string]$file.sha256).ToLowerInvariant()
    if ($expectedSize -le 0 -or $expectedHash -notmatch '^[0-9a-f]{64}$') {
        throw "Asset '$fileId' has invalid size or SHA256 metadata."
    }

    if (Test-Path -LiteralPath $targetPath -PathType Leaf) {
        $actualSize = (Get-Item -LiteralPath $targetPath).Length
        $actualHash = (Get-FileHash -LiteralPath $targetPath -Algorithm SHA256).Hash.ToLowerInvariant()
        if ($actualSize -eq $expectedSize -and $actualHash -ceq $expectedHash) {
            Write-Host "SAMPLE_ASSET_OK id=$fileId bytes=$actualSize sha256=$actualHash path=$targetPath"
            continue
        }
        if (-not $Refresh) {
            throw "Cached asset '$fileId' failed validation. Re-run with -Refresh to replace it: $targetPath"
        }
    }

    [IO.Directory]::CreateDirectory((Split-Path $targetPath -Parent)) | Out-Null
    $temporaryPath = "$targetPath.download-$([Guid]::NewGuid().ToString('N'))"
    try {
        Invoke-WebRequest -Uri ([string]$file.downloadUrl) -OutFile $temporaryPath -MaximumRedirection 10
        $actualSize = (Get-Item -LiteralPath $temporaryPath).Length
        $actualHash = (Get-FileHash -LiteralPath $temporaryPath -Algorithm SHA256).Hash.ToLowerInvariant()
        if ($actualSize -ne $expectedSize -or $actualHash -cne $expectedHash) {
            throw "Downloaded asset '$fileId' failed validation. Expected $expectedSize/$expectedHash, got $actualSize/$actualHash."
        }
        Move-Item -LiteralPath $temporaryPath -Destination $targetPath -Force
        Write-Host "SAMPLE_ASSET_DOWNLOADED id=$fileId bytes=$actualSize sha256=$actualHash path=$targetPath"
    }
    finally {
        if (Test-Path -LiteralPath $temporaryPath -PathType Leaf) {
            Remove-Item -LiteralPath $temporaryPath -Force
        }
    }
}

Write-Host "SAMPLE_MODEL_ASSET_BUNDLES_OK bundles=$($selectedBundleIds -join ',') files=$($selectedFileIds.Count) root=$cacheRoot"
