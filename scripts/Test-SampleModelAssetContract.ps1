[CmdletBinding()]
param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path,
    [switch]$VerifyCache
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$manifestRelativePath = "samples/assets/models/model-assets.json"
$manifestPath = Join-Path $repo $manifestRelativePath
$downloaderPath = Join-Path $repo "scripts/Get-SampleModelAssets.ps1"
$runtimeSupportPath = Join-Path $repo "samples/Common/ModelAssetSupport.cs"

function Assert-Contract {
    param(
        [Parameter(Mandatory = $true)]
        [bool]$Condition,
        [Parameter(Mandatory = $true)]
        [string]$Message
    )

    if (-not $Condition) {
        throw $Message
    }
}

foreach ($path in @($manifestPath, $downloaderPath, $runtimeSupportPath)) {
    Assert-Contract -Condition (Test-Path -LiteralPath $path -PathType Leaf) -Message "Required sample model asset contract file is missing: $path"
}

$manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
Assert-Contract -Condition ([int]$manifest.schemaVersion -eq 1) -Message "Sample model asset manifest schema must be 1."
Assert-Contract -Condition ([string]$manifest.defaultCacheDirectory -ceq "samples/assets/models/cache") -Message "Sample model asset cache location drifted."

$files = @($manifest.files)
$bundles = @($manifest.bundles)
Assert-Contract -Condition ($files.Count -gt 0) -Message "Sample model asset manifest has no files."
Assert-Contract -Condition ($bundles.Count -gt 0) -Message "Sample model asset manifest has no bundles."

$fileById = @{}
$relativePaths = [Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
$allowedSourceRepositories = @("opencv/opencv", "opencv/opencv_zoo")
$commitPattern = '^[0-9a-f]{40}$'
$shaPattern = '^[0-9a-f]{64}$'

foreach ($file in $files) {
    $id = [string]$file.id
    $relativePath = ([string]$file.relativePath).Replace('\', '/')
    $segments = @($relativePath.Split('/', [StringSplitOptions]::RemoveEmptyEntries))
    $downloadUrl = [string]$file.downloadUrl
    $sourceCommit = [string]$file.sourceCommit
    $transportRevision = if ($file.PSObject.Properties.Name -contains "transportRevision") { [string]$file.transportRevision } else { "" }

    Assert-Contract -Condition (-not [string]::IsNullOrWhiteSpace($id) -and -not $fileById.ContainsKey($id)) -Message "Sample model asset file ids must be non-empty and unique: $id"
    Assert-Contract -Condition (-not [string]::IsNullOrWhiteSpace($relativePath) -and -not [IO.Path]::IsPathRooted($relativePath)) -Message "Asset '$id' must use a relative cache path."
    Assert-Contract -Condition ($segments.Count -gt 0 -and $segments -notcontains ".." -and $segments -notcontains ".") -Message "Asset '$id' has a path traversal segment."
    Assert-Contract -Condition ($relativePaths.Add($relativePath)) -Message "Sample model asset relative paths must be unique: $relativePath"

    $uri = $null
    Assert-Contract -Condition ([Uri]::TryCreate($downloadUrl, [UriKind]::Absolute, [ref]$uri) -and $uri.Scheme -ceq "https") -Message "Asset '$id' must use an absolute HTTPS download URL."
    Assert-Contract -Condition ($downloadUrl -notmatch '(?i)(^|[/_-])(main|master|latest)([/_.?=-]|$)') -Message "Asset '$id' uses a floating download URL."
    Assert-Contract -Condition ($allowedSourceRepositories -ccontains [string]$file.sourceRepository) -Message "Asset '$id' uses an unapproved source repository."
    Assert-Contract -Condition ($sourceCommit -cmatch $commitPattern) -Message "Asset '$id' must pin a lowercase 40-character source commit."
    Assert-Contract -Condition ([long]$file.sizeBytes -gt 0 -and [string]$file.sha256 -cmatch $shaPattern) -Message "Asset '$id' must pin a positive size and lowercase SHA256."
    Assert-Contract -Condition ([string]$file.license -ceq "Apache-2.0") -Message "Asset '$id' must declare Apache-2.0."
    Assert-Contract -Condition (-not [string]::IsNullOrWhiteSpace([string]$file.licenseFileId)) -Message "Asset '$id' must reference a license file."

    if ([string]::IsNullOrWhiteSpace($transportRevision)) {
        Assert-Contract -Condition ($downloadUrl.Contains($sourceCommit, [StringComparison]::Ordinal)) -Message "Asset '$id' URL must embed its source commit."
    }
    else {
        Assert-Contract -Condition ($transportRevision -cmatch $commitPattern) -Message "Asset '$id' must pin a lowercase 40-character transport revision."
        Assert-Contract -Condition ([string]$file.transportRepository -cmatch '^opencv/[a-z0-9_-]+$') -Message "Asset '$id' must use an official OpenCV transport repository."
        Assert-Contract -Condition ($downloadUrl.Contains($transportRevision, [StringComparison]::Ordinal)) -Message "Asset '$id' URL must embed its transport revision."
    }

    $fileById[$id] = $file
}

foreach ($file in $files) {
    $licenseId = [string]$file.licenseFileId
    Assert-Contract -Condition ($fileById.ContainsKey($licenseId)) -Message "Asset '$($file.id)' references unknown license '$licenseId'."
    Assert-Contract -Condition ([string]$fileById[$licenseId].kind -ceq "license") -Message "Asset '$($file.id)' license reference is not a license asset."
}

$requiredBundleIds = @("classification-mobilenet-v2", "detection-nanodet", "segmentation-pphumanseg")
$bundleIds = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
foreach ($bundle in $bundles) {
    $bundleId = [string]$bundle.id
    Assert-Contract -Condition (-not [string]::IsNullOrWhiteSpace($bundleId) -and $bundleIds.Add($bundleId)) -Message "Sample model asset bundle ids must be non-empty and unique: $bundleId"

    $bundleFiles = @($bundle.fileIds | ForEach-Object {
        $fileId = [string]$_
        Assert-Contract -Condition ($fileById.ContainsKey($fileId)) -Message "Bundle '$bundleId' references unknown asset '$fileId'."
        $fileById[$fileId]
    })
    $bundleFileIds = @($bundle.fileIds | ForEach-Object { [string]$_ })
    Assert-Contract -Condition ($bundleFileIds.Count -eq @($bundleFileIds | Sort-Object -Unique).Count) -Message "Bundle '$bundleId' contains duplicate file ids."
    Assert-Contract -Condition (@($bundleFiles | Where-Object { [string]$_.kind -ceq "model" }).Count -ge 1) -Message "Bundle '$bundleId' must contain a model."
    Assert-Contract -Condition (@($bundleFiles | Where-Object { [string]$_.kind -ceq "image" }).Count -ge 1) -Message "Bundle '$bundleId' must contain an input image."
    Assert-Contract -Condition (@($bundleFiles | Where-Object { [string]$_.kind -ceq "license" }).Count -ge 1) -Message "Bundle '$bundleId' must contain a license."
}
foreach ($bundleId in $requiredBundleIds) {
    Assert-Contract -Condition ($bundleIds.Contains($bundleId)) -Message "Required sample model asset bundle is missing: $bundleId"
}

$downloader = Get-Content -LiteralPath $downloaderPath -Raw
foreach ($token in @("ConvertFrom-Json", "Get-FileHash", "SHA256", "sizeBytes", "GetFullPath", "StartsWith", "MaximumRedirection", "download-", "finally")) {
    Assert-Contract -Condition ($downloader.Contains($token, [StringComparison]::Ordinal)) -Message "Sample model asset downloader lost required validation token: $token"
}
Assert-Contract -Condition ($downloader -notmatch '(?i)(api[_-]?key|authorization|bearer|password|secret)') -Message "Sample model asset downloader must not contain credential handling."

$runtimeSupport = Get-Content -LiteralPath $runtimeSupportPath -Raw
foreach ($token in @("OPENCV_CSHARP_SAMPLE_ASSET_ROOT", "SHA256.Create", "SizeBytes", "Path.GetFullPath", "StartsWith", "StringComparison.Ordinal")) {
    Assert-Contract -Condition ($runtimeSupport.Contains($token, [StringComparison]::Ordinal)) -Message "Runtime sample asset validation lost required token: $token"
}

$trackedCacheFiles = @(& git -C $repo ls-files -- "samples/assets/models/cache/**")
Assert-Contract -Condition ($LASTEXITCODE -eq 0 -and $trackedCacheFiles.Count -eq 0) -Message "Downloaded sample model assets must not be tracked by Git."
& git -C $repo check-ignore --quiet -- "samples/assets/models/cache/.contract-probe"
Assert-Contract -Condition ($LASTEXITCODE -eq 0) -Message "Sample model asset cache directory must remain ignored by Git."

if ($VerifyCache) {
    $cacheRoot = Join-Path $repo ([string]$manifest.defaultCacheDirectory)
    foreach ($file in $files) {
        $path = Join-Path $cacheRoot (([string]$file.relativePath).Replace('/', [IO.Path]::DirectorySeparatorChar))
        Assert-Contract -Condition (Test-Path -LiteralPath $path -PathType Leaf) -Message "Verified cache asset is missing: $($file.id)"
        Assert-Contract -Condition ((Get-Item -LiteralPath $path).Length -eq [long]$file.sizeBytes) -Message "Cached asset size drifted: $($file.id)"
        Assert-Contract -Condition ((Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant() -ceq [string]$file.sha256) -Message "Cached asset SHA256 drifted: $($file.id)"
    }
}

Write-Host "SAMPLE_MODEL_ASSET_CONTRACT_OK files=$($files.Count) bundles=$($bundles.Count) cache_verified=$([bool]$VerifyCache)"
