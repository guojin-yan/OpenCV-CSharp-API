[CmdletBinding()]
param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path,
    [string]$OutputPath = "",
    [switch]$Check
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$canonicalRecordRelativePath = "packaging/release/local-release-candidate-closeout.json"
if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $OutputPath = Join-Path $repo ($canonicalRecordRelativePath -replace '/', [IO.Path]::DirectorySeparatorChar)
}
$OutputPath = [IO.Path]::GetFullPath($OutputPath)

$textExtensions = @(
    ".c", ".cc", ".cpp", ".cs", ".cmake", ".csproj", ".h", ".hpp", ".json", ".md", ".props", ".ps1", ".slnx", ".targets", ".txt", ".yml", ".yaml", ".xml"
)
$textFileNames = @("CMakeLists.txt", ".gitignore", "global.json")

function Normalize-Text {
    param([Parameter(Mandatory)][AllowEmptyString()][string]$Text)

    return (($Text -replace "`r`n", "`n") -replace "`r", "`n").TrimEnd() + "`n"
}

function Get-Sha256Bytes {
    param([Parameter(Mandatory)][byte[]]$Bytes)

    return [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($Bytes)).ToLowerInvariant()
}

function Get-OrdinalSortedObjects {
    param(
        [Parameter(Mandatory)][AllowEmptyCollection()][object[]]$Values,
        [Parameter(Mandatory)][string]$Property
    )

    $copy = [object[]]$Values.Clone()
    $keys = [string[]]@($copy | ForEach-Object { [string]$_.$Property })
    [Array]::Sort[string, object]($keys, $copy, [StringComparer]::Ordinal)
    return $copy
}

function Get-LogicalFileRecord {
    param([Parameter(Mandatory)][string]$RelativePath)

    $normalizedPath = $RelativePath.Replace("\", "/")
    $absolutePath = Join-Path $repo ($normalizedPath -replace '/', [IO.Path]::DirectorySeparatorChar)
    if (-not (Test-Path -LiteralPath $absolutePath -PathType Leaf)) {
        throw "Source-set file is missing: $normalizedPath"
    }

    $extension = [IO.Path]::GetExtension($absolutePath).ToLowerInvariant()
    $name = [IO.Path]::GetFileName($absolutePath)
    if ($extension -in $textExtensions -or $name -in $textFileNames) {
        $logicalBytes = [Text.UTF8Encoding]::new($false).GetBytes(
            (Normalize-Text ([IO.File]::ReadAllText($absolutePath))))
    }
    else {
        $logicalBytes = [IO.File]::ReadAllBytes($absolutePath)
    }

    [pscustomobject]@{
        Path = $normalizedPath
        Length = $logicalBytes.Length
        Sha256 = Get-Sha256Bytes -Bytes $logicalBytes
    }
}

function Get-SourceSet {
    $paths = @(& git -C $repo ls-files --cached --others --exclude-standard)
    if ($LASTEXITCODE -ne 0) {
        throw "git ls-files failed while creating the release closeout source set."
    }

    $paths = @($paths | ForEach-Object { ([string]$_).Trim() } | Where-Object { $_ })
    $records = [System.Collections.Generic.List[object]]::new()
    foreach ($path in $paths) {
        $normalizedPath = $path.Replace("\", "/")
        if ($normalizedPath -eq $canonicalRecordRelativePath) {
            continue
        }
        $records.Add((Get-LogicalFileRecord -RelativePath $normalizedPath))
    }

    $sorted = @(Get-OrdinalSortedObjects -Values $records.ToArray() -Property "Path")
    $digestLines = @($sorted | ForEach-Object { "$($_.Path)|$($_.Length)|$($_.Sha256)" })
    $digest = Get-Sha256Bytes -Bytes ([Text.UTF8Encoding]::new($false).GetBytes((($digestLines -join "`n") + "`n")))
    [pscustomobject]@{
        FileCount = $sorted.Count
        Sha256 = $digest
        Files = $sorted
    }
}

function Get-FileEvidence {
    param([Parameter(Mandatory)][string]$RelativePath)

    $logicalRecord = Get-LogicalFileRecord -RelativePath $RelativePath
    return [pscustomobject]@{
        Path = $logicalRecord.Path
        Sha256 = $logicalRecord.Sha256
        Length = $logicalRecord.Length
    }
}

function Get-Record {
    $sourceSet = Get-SourceSet
    $support = Get-Content -LiteralPath (Join-Path $repo "packaging/runtime/runtime-support-contract.json") -Raw | ConvertFrom-Json
    $matrixPath = Join-Path $repo "packaging/runtime/runtime-package-matrix.json"
    $matrix = Get-Content -LiteralPath $matrixPath -Raw | ConvertFrom-Json
    $lifecycleRefreshMatrixPath = Join-Path $repo "packaging/runtime/runtime-lifecycle-refresh-matrix.json"
    $lifecycleRefreshMatrix = Get-Content -LiteralPath $lifecycleRefreshMatrixPath -Raw | ConvertFrom-Json
    $summary = Get-Content -LiteralPath (Join-Path $repo "compatibility/managed-public-api-summary.json") -Raw | ConvertFrom-Json
    $gapInventory = Get-Content -LiteralPath (Join-Path $repo "compatibility/api-gap-inventory.json") -Raw | ConvertFrom-Json
    $bindingSummary = Get-Content -LiteralPath (Join-Path $repo "compatibility/native-managed-binding-summary.json") -Raw | ConvertFrom-Json
    $spanFamily = Get-Content -LiteralPath (Join-Path $repo "compatibility/imgproc-point-set-span-family.json") -Raw | ConvertFrom-Json
    $imgProcSummary = Get-Content -LiteralPath (Join-Path $repo "compatibility/imgproc-upstream-summary.json") -Raw | ConvertFrom-Json
    $imgCodecsSummary = Get-Content -LiteralPath (Join-Path $repo "compatibility/imgcodecs-upstream-summary.json") -Raw | ConvertFrom-Json
    $videoIOSummary = Get-Content -LiteralPath (Join-Path $repo "compatibility/videoio-upstream-summary.json") -Raw | ConvertFrom-Json
    $videoIORegistry = Get-Content -LiteralPath (Join-Path $repo "compatibility/videoio-registry-surface.json") -Raw | ConvertFrom-Json
    $calib3DSummary = Get-Content -LiteralPath (Join-Path $repo "compatibility/calib3d-upstream-summary.json") -Raw | ConvertFrom-Json
    $coreSummary = Get-Content -LiteralPath (Join-Path $repo "compatibility/core-upstream-summary.json") -Raw | ConvertFrom-Json
    $dnnSummary = Get-Content -LiteralPath (Join-Path $repo "compatibility/dnn-upstream-summary.json") -Raw | ConvertFrom-Json
    $dnnSuperresRaw = Get-Content -LiteralPath (Join-Path $repo "compatibility/dnn_superres-upstream-raw.json") -Raw | ConvertFrom-Json
    $dnnSuperresSummary = Get-Content -LiteralPath (Join-Path $repo "compatibility/dnn_superres-upstream-summary.json") -Raw | ConvertFrom-Json
    $dnnObjDetectRaw = Get-Content -LiteralPath (Join-Path $repo "compatibility/dnn_objdetect-upstream-raw.json") -Raw | ConvertFrom-Json
    $dnnObjDetectSummary = Get-Content -LiteralPath (Join-Path $repo "compatibility/dnn_objdetect-upstream-summary.json") -Raw | ConvertFrom-Json
    $ccalibRaw = Get-Content -LiteralPath (Join-Path $repo "compatibility/ccalib-upstream-raw.json") -Raw | ConvertFrom-Json
    $ccalibSummary = Get-Content -LiteralPath (Join-Path $repo "compatibility/ccalib-upstream-summary.json") -Raw | ConvertFrom-Json
    $superresRaw = Get-Content -LiteralPath (Join-Path $repo "compatibility/superres-upstream-raw.json") -Raw | ConvertFrom-Json
    $superresSummary = Get-Content -LiteralPath (Join-Path $repo "compatibility/superres-upstream-summary.json") -Raw | ConvertFrom-Json
    $juliaRaw = Get-Content -LiteralPath (Join-Path $repo "compatibility/julia-upstream-raw.json") -Raw | ConvertFrom-Json
    $juliaSummary = Get-Content -LiteralPath (Join-Path $repo "compatibility/julia-upstream-summary.json") -Raw | ConvertFrom-Json
    $rgbdRaw = Get-Content -LiteralPath (Join-Path $repo "compatibility/rgbd-upstream-raw.json") -Raw | ConvertFrom-Json
    $rgbdSummary = Get-Content -LiteralPath (Join-Path $repo "compatibility/rgbd-upstream-summary.json") -Raw | ConvertFrom-Json
    $wechatQRCodeRaw = Get-Content -LiteralPath (Join-Path $repo "compatibility/wechat_qrcode-upstream-raw.json") -Raw | ConvertFrom-Json
    $wechatQRCodeSummary = Get-Content -LiteralPath (Join-Path $repo "compatibility/wechat_qrcode-upstream-summary.json") -Raw | ConvertFrom-Json
    $cnn3DObjRaw = Get-Content -LiteralPath (Join-Path $repo "compatibility/cnn_3dobj-upstream-raw.json") -Raw | ConvertFrom-Json
    $cnn3DObjSummary = Get-Content -LiteralPath (Join-Path $repo "compatibility/cnn_3dobj-upstream-summary.json") -Raw | ConvertFrom-Json
    $signalRaw = Get-Content -LiteralPath (Join-Path $repo "compatibility/signal-upstream-raw.json") -Raw | ConvertFrom-Json
    $signalSummary = Get-Content -LiteralPath (Join-Path $repo "compatibility/signal-upstream-summary.json") -Raw | ConvertFrom-Json
    $dpmRaw = Get-Content -LiteralPath (Join-Path $repo "compatibility/dpm-upstream-raw.json") -Raw | ConvertFrom-Json
    $dpmSummary = Get-Content -LiteralPath (Join-Path $repo "compatibility/dpm-upstream-summary.json") -Raw | ConvertFrom-Json
    $hdfRaw = Get-Content -LiteralPath (Join-Path $repo "compatibility/hdf-upstream-raw.json") -Raw | ConvertFrom-Json
    $hdfSummary = Get-Content -LiteralPath (Join-Path $repo "compatibility/hdf-upstream-summary.json") -Raw | ConvertFrom-Json
    $textRaw = Get-Content -LiteralPath (Join-Path $repo "compatibility/text-upstream-raw.json") -Raw | ConvertFrom-Json
    $textSummary = Get-Content -LiteralPath (Join-Path $repo "compatibility/text-upstream-summary.json") -Raw | ConvertFrom-Json
    $featuresSummary = Get-Content -LiteralPath (Join-Path $repo "compatibility/features-upstream-summary.json") -Raw | ConvertFrom-Json
    $highGuiSummary = Get-Content -LiteralPath (Join-Path $repo "compatibility/highgui-upstream-summary.json") -Raw | ConvertFrom-Json
    $objDetectSummary = Get-Content -LiteralPath (Join-Path $repo "compatibility/objdetect-upstream-summary.json") -Raw | ConvertFrom-Json
    $photoSummary = Get-Content -LiteralPath (Join-Path $repo "compatibility/photo-upstream-summary.json") -Raw | ConvertFrom-Json
    $mlSummary = Get-Content -LiteralPath (Join-Path $repo "compatibility/ml-upstream-summary.json") -Raw | ConvertFrom-Json
    $trackingSummary = Get-Content -LiteralPath (Join-Path $repo "compatibility/tracking-upstream-summary.json") -Raw | ConvertFrom-Json
    $stitchingSummary = Get-Content -LiteralPath (Join-Path $repo "compatibility/stitching-upstream-summary.json") -Raw | ConvertFrom-Json
    $videoSummary = Get-Content -LiteralPath (Join-Path $repo "compatibility/video-upstream-summary.json") -Raw | ConvertFrom-Json
    $surfaceMatchingSummary = Get-Content -LiteralPath (Join-Path $repo "compatibility/surface_matching-upstream-summary.json") -Raw | ConvertFrom-Json
    $regRaw = Get-Content -LiteralPath (Join-Path $repo "compatibility/reg-upstream-raw.json") -Raw | ConvertFrom-Json
    $regSummary = Get-Content -LiteralPath (Join-Path $repo "compatibility/reg-upstream-summary.json") -Raw | ConvertFrom-Json
    $xstereoRaw = Get-Content -LiteralPath (Join-Path $repo "compatibility/xstereo-upstream-raw.json") -Raw | ConvertFrom-Json
    $xstereoSummary = Get-Content -LiteralPath (Join-Path $repo "compatibility/xstereo-upstream-summary.json") -Raw | ConvertFrom-Json
    $ptcloudRaw = Get-Content -LiteralPath (Join-Path $repo "compatibility/ptcloud-upstream-raw.json") -Raw | ConvertFrom-Json
    $ptcloudSummary = Get-Content -LiteralPath (Join-Path $repo "compatibility/ptcloud-upstream-summary.json") -Raw | ConvertFrom-Json
    $xobjdetectRaw = Get-Content -LiteralPath (Join-Path $repo "compatibility/xobjdetect-upstream-raw.json") -Raw | ConvertFrom-Json
    $xobjdetectSummary = Get-Content -LiteralPath (Join-Path $repo "compatibility/xobjdetect-upstream-summary.json") -Raw | ConvertFrom-Json
    $xfeatures2dRaw = Get-Content -LiteralPath (Join-Path $repo "compatibility/xfeatures2d-upstream-raw.json") -Raw | ConvertFrom-Json
    $xfeatures2dSummary = Get-Content -LiteralPath (Join-Path $repo "compatibility/xfeatures2d-upstream-summary.json") -Raw | ConvertFrom-Json
    $evidencePaths = @(
        ".github/workflows/pack.yml",
        ".github/workflows/publish-nuget.yml",
        ".github/workflows/runtime-input.yml",
        "CHANGELOG.md",
        "CONTRIBUTING.md",
        "README.md",
        "README_cn.md",
        "compatibility/api-gap-inventory.json",
        "compatibility/upstream-map-catalog.json",
        "compatibility/upstream-map-catalog.schema.json",
        "compatibility/calib3d-implemented-families.json",
        "compatibility/calib3d-upstream-classifications.json",
        "compatibility/calib3d-upstream-map.txt",
        "compatibility/calib3d-upstream-raw.json",
        "compatibility/calib3d-upstream-summary.json",
        "compatibility/core-implemented-families.json",
        "compatibility/core-upstream-classifications.json",
        "compatibility/core-upstream-map.txt",
        "compatibility/core-upstream-raw.json",
        "compatibility/core-upstream-summary.json",
        "compatibility/dnn-implemented-families.json",
        "compatibility/dnn-upstream-classifications.json",
        "compatibility/dnn-upstream-map.txt",
        "compatibility/dnn-upstream-raw.json",
        "compatibility/dnn-upstream-summary.json",
        "compatibility/dnn_superres-implemented-families.json",
        "compatibility/dnn_superres-upstream-classifications.json",
        "compatibility/dnn_superres-upstream-map.txt",
        "compatibility/dnn_superres-upstream-raw.json",
        "compatibility/dnn_superres-upstream-summary.json",
        "compatibility/dnn_objdetect-implemented-families.json",
        "compatibility/dnn_objdetect-upstream-classifications.json",
        "compatibility/dnn_objdetect-upstream-map.txt",
        "compatibility/dnn_objdetect-upstream-raw.json",
        "compatibility/dnn_objdetect-upstream-summary.json",
        "compatibility/ccalib-implemented-families.json",
        "compatibility/ccalib-upstream-classifications.json",
        "compatibility/ccalib-upstream-map.txt",
        "compatibility/ccalib-upstream-raw.json",
        "compatibility/ccalib-upstream-summary.json",
        "compatibility/superres-implemented-families.json",
        "compatibility/superres-upstream-classifications.json",
        "compatibility/superres-upstream-map.txt",
        "compatibility/superres-upstream-raw.json",
        "compatibility/superres-upstream-summary.json",
        "compatibility/julia-implemented-families.json",
        "compatibility/julia-upstream-classifications.json",
        "compatibility/julia-upstream-map.txt",
        "compatibility/julia-upstream-raw.json",
        "compatibility/julia-upstream-summary.json",
        "compatibility/rgbd-implemented-families.json",
        "compatibility/rgbd-upstream-classifications.json",
        "compatibility/rgbd-upstream-map.txt",
        "compatibility/rgbd-upstream-raw.json",
        "compatibility/rgbd-upstream-summary.json",
        "compatibility/wechat_qrcode-implemented-families.json",
        "compatibility/wechat_qrcode-upstream-classifications.json",
        "compatibility/wechat_qrcode-upstream-map.txt",
        "compatibility/wechat_qrcode-upstream-raw.json",
        "compatibility/wechat_qrcode-upstream-summary.json",
        "compatibility/cnn_3dobj-implemented-families.json",
        "compatibility/cnn_3dobj-upstream-classifications.json",
        "compatibility/cnn_3dobj-upstream-map.txt",
        "compatibility/cnn_3dobj-upstream-raw.json",
        "compatibility/cnn_3dobj-upstream-summary.json",
        "compatibility/signal-implemented-families.json",
        "compatibility/signal-upstream-classifications.json",
        "compatibility/signal-upstream-map.txt",
        "compatibility/signal-upstream-raw.json",
        "compatibility/signal-upstream-summary.json",
        "compatibility/dpm-implemented-families.json",
        "compatibility/dpm-upstream-classifications.json",
        "compatibility/dpm-upstream-map.txt",
        "compatibility/dpm-upstream-raw.json",
        "compatibility/dpm-upstream-summary.json",
        "compatibility/hdf-implemented-families.json",
        "compatibility/hdf-upstream-classifications.json",
        "compatibility/hdf-upstream-map.txt",
        "compatibility/hdf-upstream-raw.json",
        "compatibility/hdf-upstream-summary.json",
        "compatibility/text-implemented-families.json",
        "compatibility/text-upstream-classifications.json",
        "compatibility/text-upstream-map.txt",
        "compatibility/text-upstream-raw.json",
        "compatibility/text-upstream-summary.json",
        "compatibility/features-implemented-families.json",
        "compatibility/features-source-reviewed-extensions.json",
        "compatibility/features-upstream-classifications.json",
        "compatibility/features-upstream-map.txt",
        "compatibility/features-upstream-raw.json",
        "compatibility/features-upstream-summary.json",
        "compatibility/highgui-implemented-families.json",
        "compatibility/highgui-upstream-classifications.json",
        "compatibility/highgui-upstream-map.txt",
        "compatibility/highgui-upstream-raw.json",
        "compatibility/highgui-upstream-summary.json",
        "compatibility/imgcodecs-implemented-families.json",
        "compatibility/imgcodecs-source-reviewed-extensions.json",
        "compatibility/imgcodecs-upstream-classifications.json",
        "compatibility/imgcodecs-upstream-map.txt",
        "compatibility/imgcodecs-upstream-raw.json",
        "compatibility/imgcodecs-upstream-summary.json",
        "compatibility/imgproc-implemented-families.json",
        "compatibility/imgproc-point-set-span-family.json",
        "compatibility/imgproc-upstream-classifications.json",
        "compatibility/imgproc-upstream-map.txt",
        "compatibility/imgproc-upstream-raw.json",
        "compatibility/imgproc-upstream-summary.json",
        "compatibility/managed-public-api-summary.json",
        "compatibility/managed-public-api.txt",
        "compatibility/ml-implemented-families.json",
        "compatibility/ml-upstream-classifications.json",
        "compatibility/ml-upstream-map.txt",
        "compatibility/ml-upstream-raw.json",
        "compatibility/ml-upstream-summary.json",
        "compatibility/tracking-implemented-families.json",
        "compatibility/tracking-upstream-classifications.json",
        "compatibility/tracking-upstream-map.txt",
        "compatibility/tracking-upstream-raw.json",
        "compatibility/tracking-upstream-summary.json",
        "compatibility/stitching-implemented-families.json",
        "compatibility/stitching-upstream-classifications.json",
        "compatibility/stitching-upstream-map.txt",
        "compatibility/stitching-upstream-raw.json",
        "compatibility/stitching-upstream-summary.json",
        "compatibility/native-managed-binding-map.txt",
        "compatibility/native-managed-binding-summary.json",
        "compatibility/objdetect-implemented-families.json",
        "compatibility/objdetect-upstream-classifications.json",
        "compatibility/objdetect-upstream-map.txt",
        "compatibility/objdetect-upstream-raw.json",
        "compatibility/objdetect-upstream-summary.json",
        "compatibility/photo-implemented-families.json",
        "compatibility/photo-upstream-classifications.json",
        "compatibility/photo-upstream-map.txt",
        "compatibility/photo-upstream-raw.json",
        "compatibility/photo-upstream-summary.json",
        "compatibility/video-implemented-families.json",
        "compatibility/video-upstream-classifications.json",
        "compatibility/video-upstream-map.txt",
        "compatibility/video-upstream-raw.json",
        "compatibility/video-upstream-summary.json",
        "compatibility/videoio-implemented-families.json",
        "compatibility/videoio-registry-surface.json",
        "compatibility/videoio-upstream-classifications.json",
        "compatibility/videoio-upstream-map.txt",
        "compatibility/videoio-upstream-raw.json",
        "compatibility/videoio-upstream-summary.json",
        "compatibility/xphoto-implemented-families.json",
        "compatibility/xphoto-upstream-classifications.json",
        "compatibility/xphoto-upstream-map.txt",
        "compatibility/xphoto-upstream-raw.json",
        "compatibility/xphoto-upstream-summary.json",
        "compatibility/ximgproc-implemented-families.json",
        "compatibility/ximgproc-upstream-classifications.json",
        "compatibility/ximgproc-upstream-map.txt",
        "compatibility/ximgproc-upstream-raw.json",
        "compatibility/ximgproc-upstream-summary.json",
        "compatibility/optflow-implemented-families.json",
        "compatibility/optflow-upstream-classifications.json",
        "compatibility/optflow-upstream-map.txt",
        "compatibility/optflow-upstream-raw.json",
        "compatibility/optflow-upstream-summary.json",
        "compatibility/bgsegm-implemented-families.json",
        "compatibility/bgsegm-upstream-classifications.json",
        "compatibility/bgsegm-upstream-map.txt",
        "compatibility/bgsegm-upstream-raw.json",
        "compatibility/bgsegm-upstream-summary.json",
        "compatibility/face-implemented-families.json",
        "compatibility/face-upstream-classifications.json",
        "compatibility/face-upstream-map.txt",
        "compatibility/face-upstream-raw.json",
        "compatibility/face-upstream-summary.json",
        "compatibility/quality-implemented-families.json",
        "compatibility/quality-upstream-classifications.json",
        "compatibility/quality-upstream-map.txt",
        "compatibility/quality-upstream-raw.json",
        "compatibility/quality-upstream-summary.json",
        "compatibility/img_hash-implemented-families.json",
        "compatibility/img_hash-upstream-classifications.json",
        "compatibility/img_hash-upstream-map.txt",
        "compatibility/img_hash-upstream-raw.json",
        "compatibility/img_hash-upstream-summary.json",
        "compatibility/line_descriptor-implemented-families.json",
        "compatibility/line_descriptor-upstream-classifications.json",
        "compatibility/line_descriptor-upstream-map.txt",
        "compatibility/line_descriptor-upstream-raw.json",
        "compatibility/line_descriptor-upstream-summary.json",
        "compatibility/freetype-implemented-families.json",
        "compatibility/freetype-upstream-classifications.json",
        "compatibility/freetype-upstream-map.txt",
        "compatibility/freetype-upstream-raw.json",
        "compatibility/freetype-upstream-summary.json",
        "compatibility/alphamat-implemented-families.json",
        "compatibility/alphamat-upstream-classifications.json",
        "compatibility/alphamat-upstream-map.txt",
        "compatibility/alphamat-upstream-raw.json",
        "compatibility/alphamat-upstream-summary.json",
        "compatibility/intensity_transform-implemented-families.json",
        "compatibility/intensity_transform-upstream-classifications.json",
        "compatibility/intensity_transform-upstream-map.txt",
        "compatibility/intensity_transform-upstream-raw.json",
        "compatibility/intensity_transform-upstream-summary.json",
        "compatibility/plot-implemented-families.json",
        "compatibility/plot-upstream-classifications.json",
        "compatibility/plot-upstream-map.txt",
        "compatibility/plot-upstream-raw.json",
        "compatibility/plot-upstream-summary.json",
        "compatibility/bioinspired-implemented-families.json",
        "compatibility/bioinspired-upstream-classifications.json",
        "compatibility/bioinspired-upstream-map.txt",
        "compatibility/bioinspired-upstream-raw.json",
        "compatibility/bioinspired-upstream-summary.json",
        "compatibility/phase_unwrapping-implemented-families.json",
        "compatibility/phase_unwrapping-upstream-classifications.json",
        "compatibility/phase_unwrapping-upstream-map.txt",
        "compatibility/phase_unwrapping-upstream-raw.json",
        "compatibility/phase_unwrapping-upstream-summary.json",
        "compatibility/hfs-implemented-families.json",
        "compatibility/hfs-upstream-classifications.json",
        "compatibility/hfs-upstream-map.txt",
        "compatibility/hfs-upstream-raw.json",
        "compatibility/hfs-upstream-summary.json",
        "compatibility/fuzzy-implemented-families.json",
        "compatibility/fuzzy-upstream-classifications.json",
        "compatibility/fuzzy-upstream-map.txt",
        "compatibility/fuzzy-upstream-raw.json",
        "compatibility/fuzzy-upstream-summary.json",
        "compatibility/rapid-implemented-families.json",
        "compatibility/rapid-upstream-classifications.json",
        "compatibility/rapid-upstream-map.txt",
        "compatibility/rapid-upstream-raw.json",
        "compatibility/rapid-upstream-summary.json",
        "compatibility/shape-implemented-families.json",
        "compatibility/shape-upstream-classifications.json",
        "compatibility/shape-upstream-map.txt",
        "compatibility/shape-upstream-raw.json",
        "compatibility/shape-upstream-summary.json",
        "compatibility/structured_light-implemented-families.json",
        "compatibility/structured_light-upstream-classifications.json",
        "compatibility/structured_light-upstream-map.txt",
        "compatibility/structured_light-upstream-raw.json",
        "compatibility/structured_light-upstream-summary.json",
        "compatibility/saliency-implemented-families.json",
        "compatibility/saliency-upstream-classifications.json",
        "compatibility/saliency-upstream-map.txt",
        "compatibility/saliency-upstream-raw.json",
        "compatibility/saliency-upstream-summary.json",
        "compatibility/surface_matching-implemented-families.json",
        "compatibility/surface_matching-upstream-classifications.json",
        "compatibility/surface_matching-upstream-map.txt",
        "compatibility/surface_matching-upstream-raw.json",
        "compatibility/surface_matching-upstream-summary.json",
        "compatibility/reg-implemented-families.json",
        "compatibility/reg-upstream-classifications.json",
        "compatibility/reg-upstream-map.txt",
        "compatibility/reg-upstream-raw.json",
        "compatibility/reg-upstream-summary.json",
        "compatibility/xstereo-implemented-families.json",
        "compatibility/xstereo-upstream-classifications.json",
        "compatibility/xstereo-upstream-map.txt",
        "compatibility/xstereo-upstream-raw.json",
        "compatibility/xstereo-upstream-summary.json",
        "compatibility/ptcloud-implemented-families.json",
        "compatibility/ptcloud-upstream-classifications.json",
        "compatibility/ptcloud-upstream-map.txt",
        "compatibility/ptcloud-upstream-raw.json",
        "compatibility/ptcloud-upstream-summary.json",
        "compatibility/xobjdetect-implemented-families.json",
        "compatibility/xobjdetect-upstream-classifications.json",
        "compatibility/xobjdetect-upstream-map.txt",
        "compatibility/xobjdetect-upstream-raw.json",
        "compatibility/xobjdetect-upstream-summary.json",
        "compatibility/xfeatures2d-implemented-families.json",
        "compatibility/xfeatures2d-upstream-classifications.json",
        "compatibility/xfeatures2d-upstream-map.txt",
        "compatibility/xfeatures2d-upstream-raw.json",
        "compatibility/xfeatures2d-upstream-summary.json",
        "compatibility/v5.0.1-change-ledger.json",
        "compatibility/v5.0.1-change-ledger.schema.json",
        "docs/articles/api-abi-compatibility-policy.md",
        "docs/articles/upstream-map-toolchain.md",
        "docs/articles/xfeatures2d-upstream-parity-guide.md",
        "docs/articles/dnn-objdetect-upstream-parity-guide.md",
        "docs/articles/ccalib-upstream-parity-guide.md",
        "docs/articles/julia-upstream-parity-guide.md",
        "docs/articles/rgbd-upstream-parity-guide.md",
        "docs/articles/signal-upstream-parity-guide.md",
        "docs/articles/dpm-upstream-parity-guide.md",
        "docs/articles/hdf-upstream-parity-guide.md",
        "docs/articles/text-upstream-parity-guide.md",
        "docs/articles/xphoto-upstream-parity-guide.md",
        "docs/articles/ximgproc-upstream-parity-guide.md",
        "docs/articles/optflow-bgsegm-upstream-parity-guide.md",
        "docs/articles/face-guide.md",
        "docs/articles/quality-guide.md",
        "docs/articles/img-hash-guide.md",
        "docs/articles/line-descriptor-guide.md",
        "docs/articles/intensity-transform-guide.md",
        "docs/articles/plot-guide.md",
        "docs/articles/bioinspired-guide.md",
        "docs/articles/phase-unwrapping-guide.md",
        "docs/articles/hfs-guide.md",
        "docs/articles/fuzzy-guide.md",
        "docs/articles/rapid-guide.md",
        "docs/articles/shape-guide.md",
        "docs/articles/structured-light-guide.md",
        "docs/articles/saliency-guide.md",
        "docs/articles/saliency-objectness-guide.md",
        "docs/articles/surface-matching-guide.md",
        "docs/articles/reg-guide.md",
        "docs/articles/xobjdetect-guide.md",
        "docs/articles/calib3d-upstream-parity-guide.md",
        "docs/articles/core-upstream-parity-guide.md",
        "docs/articles/dnn-structured-parity-guide.md",
        "docs/articles/dnn-named-io-dynamic-shape-adr.md",
        "docs/articles/dnn-model-wrapper-adr.md",
        "docs/articles/dnn-quantization-backend-diagnostics-adr.md",
        "docs/articles/dnn-tokenizer-text-adr.md",
        "docs/articles/features-upstream-parity-guide.md",
        "docs/articles/generic-linux-rid-adr.md",
        "docs/articles/headless-runtime-profile-adr.md",
        "docs/articles/generic-linux-musl-feasibility-adr.md",
        "docs/articles/domestic-platform-certification-adr.md",
        "docs/articles/opencl-tapi-umat-adr.md",
        "docs/articles/highgui-interaction-guide.md",
        "docs/articles/imgcodecs-upstream-parity-guide.md",
        "docs/articles/codec-buffer-lease-adr.md",
        "docs/articles/codec-buffer-and-owner-adr.md",
        "docs/articles/codec-benchmark-baseline.md",
        "docs/articles/image-adapter-selection-adr.md",
        "docs/articles/codec-preflight-and-limits.md",
        "docs/articles/imgproc-geometry-guide.md",
        "docs/articles/imgproc-upstream-parity-guide.md",
        "docs/articles/ml-guide.md",
        "docs/articles/tracking-guide.md",
        "docs/articles/stitching-structured-parity-guide.md",
        "docs/articles/objdetect-structured-parity-guide.md",
        "docs/articles/photo-ccm-guide.md",
        "docs/articles/photo-hdr-workflow-guide.md",
        "docs/articles/photo-intelligent-scissors-guide.md",
        "docs/articles/photo-tvl1-chromatic-aberration-guide.md",
        "docs/articles/point-set-marshalling-guide.md",
        "docs/articles/release-candidate-closeout.md",
        "docs/articles/sample-model-assets-guide.md",
        "docs/articles/support-lifecycle-policy.md",
        "docs/articles/tutorial-series.md",
        "docs/articles/tutorial-01-image-pipeline.md",
        "docs/articles/tutorial-02-chinese-puttext.md",
        "docs/articles/tutorial-03-contours.md",
        "docs/articles/tutorial-04-orb-features.md",
        "docs/articles/tutorial-05-template-matching.md",
        "docs/articles/tutorial-06-knn-classification.md",
        "docs/articles/tutorial-07-android-runtime.md",
        "docs/images/showcase/chinese-text.png",
        "docs/images/showcase/contours.png",
        "docs/images/showcase/image-pipeline.png",
        "docs/images/showcase/knn-classification.png",
        "docs/images/showcase/orb-features.png",
        "docs/images/showcase/showcase-overview.png",
        "docs/images/showcase/source.png",
        "docs/images/showcase/template-match.png",
        "docs/index.md",
        "docs/toc.yml",
        "docs/articles/video-upstream-parity-guide.md",
        "docs/articles/videoio-upstream-parity-guide.md",
        "docs/articles/async-video-owner-adr.md",
        "docs/articles/v501-modern-api-preview-guide.md",
        "docs/articles/video-dnn-benchmark-baseline.md",
        "docs/articles/perf-001-regression-baseline.md",
        "docs/articles/native-aot-trim-single-file-adr.md",
        "nuget/logo.jpg",
        "packaging/runtime/JYPPX.OpenCV.runtime/buildTransitive/JYPPX.OpenCV.runtime.targets",
        "packaging/runtime/android-runtime-evidence.json",
        "packaging/runtime/runtime-generic-linux-feasibility.json",
        "packaging/runtime/runtime-generic-linux-feasibility.schema.json",
        "packaging/runtime/runtime-generic-linux-preview-matrix.json",
        "packaging/runtime/runtime-generic-linux-preview-matrix.schema.json",
        "packaging/runtime/runtime-generic-linux-arm64-preview-matrix.json",
        "packaging/runtime/runtime-generic-linux-arm64-preview-matrix.schema.json",
        "packaging/runtime/runtime-headless-profile-contract.json",
        "packaging/runtime/runtime-headless-profile-contract.schema.json",
        "packaging/runtime/runtime-headless-candidate-evidence.json",
        "packaging/runtime/runtime-headless-candidate-evidence.schema.json",
        "packaging/runtime/runtime-generic-linux-musl-feasibility.json",
        "packaging/runtime/runtime-generic-linux-musl-feasibility.schema.json",
        "packaging/runtime/runtime-distro-rid-graph.json",
        "packaging/platform/domestic-platform-certification-matrix.json",
        "packaging/platform/domestic-platform-certification-matrix.schema.json",
        "packaging/runtime/runtime-opencl-tapi-umat-contract.json",
        "packaging/runtime/runtime-opencl-tapi-umat-contract.schema.json",
        "packaging/runtime/runtime-capabilities-json.schema.json",
        "packaging/runtime/capability-profile-evidence.json",
        "packaging/runtime/capability-profile-evidence.schema.json",
        "packaging/codec/image-preflight-mutation-corpus.json",
        "packaging/codec/image-preflight-mutation-corpus.schema.json",
        "packaging/codec/image-preflight-fuzz-evidence.json",
        "packaging/codec/image-preflight-fuzz-evidence.schema.json",
        "packaging/codec/codec-native-mutation-evidence.json",
        "packaging/codec/codec-native-mutation-evidence.schema.json",
        "packaging/performance/codec-typed-mat-benchmark-evidence.json",
        "packaging/performance/codec-typed-mat-benchmark-evidence.schema.json",
        "packaging/performance/codec-writer-native-evidence.json",
        "packaging/performance/codec-writer-native-evidence.schema.json",
        "packaging/performance/codec-writer-allocation-evidence.json",
        "packaging/performance/codec-writer-allocation-evidence.schema.json",
        "packaging/performance/video-dnn-benchmark-evidence.json",
        "packaging/performance/video-dnn-benchmark-evidence.schema.json",
        "packaging/performance/perf-001-regression-baseline.json",
        "packaging/performance/perf-001-regression-baseline.schema.json",
        "packaging/performance/native-aot-smoke-evidence.json",
        "packaging/performance/native-aot-smoke-evidence.schema.json",
        "packaging/performance/native-aot-trim-warning-ledger.json",
        "packaging/performance/native-aot-trim-warning-ledger.schema.json",
        "packaging/performance/typed-mat-native-lifetime-evidence.json",
        "packaging/performance/typed-mat-native-lifetime-evidence.schema.json",
        "packaging/performance/typed-mat-owner-lifetime-matrix.json",
        "packaging/performance/typed-mat-owner-lifetime-matrix.schema.json",
        "packaging/performance/typed-mat-target-framework-evidence.json",
        "packaging/performance/typed-mat-target-framework-evidence.schema.json",
        "packaging/video/runtime-async-video-contract.json",
        "packaging/video/runtime-async-video-contract.schema.json",
        "packaging/dnn/runtime-dnn-diagnostics-contract.json",
        "packaging/dnn/runtime-dnn-diagnostics-contract.schema.json",
        "packaging/codec/image-format-policy.json",
        "packaging/codec/image-format-policy.schema.json",
        "packaging/runtime/runtime-support-contract.schema.json",
        "packaging/runtime/runtime-support-contract.json",
        "samples/AndroidSmoke/AndroidSmoke.csproj",
        "samples/AndroidSmoke/MainActivity.cs",
        "samples/Common/ModelAssetSupport.cs",
        "samples/ConsoleSamples/Program.cs",
        "samples/ConsoleSamples/ShowcaseRunner.cs",
        "samples/assets/models/model-assets.json",
        "scripts/Generate-Calib3DUpstreamMap.ps1",
        "scripts/Generate-CoreUpstreamMap.ps1",
        "scripts/Generate-DnnUpstreamMap.ps1",
        "scripts/Generate-DnnSuperresUpstreamMap.ps1",
        "scripts/Generate-DnnObjDetectUpstreamMap.ps1",
        "scripts/Generate-CcalibUpstreamMap.ps1",
        "scripts/Generate-SuperresUpstreamMap.ps1",
        "scripts/Generate-JuliaUpstreamMap.ps1",
        "scripts/Generate-RgbdUpstreamMap.ps1",
        "scripts/Generate-WechatQRCodeUpstreamMap.ps1",
        "scripts/Generate-Cnn3DObjUpstreamMap.ps1",
        "scripts/Generate-SignalUpstreamMap.ps1",
        "scripts/Generate-DpmUpstreamMap.ps1",
        "scripts/Generate-HdfUpstreamMap.ps1",
        "scripts/Generate-TextUpstreamMap.ps1",
        "scripts/Generate-FeaturesUpstreamMap.ps1",
        "scripts/Generate-HighGuiUpstreamMap.ps1",
        "scripts/Generate-ImgProcUpstreamMap.ps1",
        "scripts/Generate-ImgCodecsUpstreamMap.ps1",
        "scripts/Generate-ManagedPublicApiBaseline.ps1",
        "scripts/Generate-MlUpstreamMap.ps1",
        "scripts/Generate-TrackingUpstreamMap.ps1",
        "scripts/Generate-StitchingUpstreamMap.ps1",
        "scripts/Generate-NativeAbiManifest.ps1",
        "scripts/Generate-NativeManagedBindingMap.ps1",
        "scripts/Test-NoUnpublishedCompatibilitySurface.ps1",
        "scripts/Generate-ObjDetectUpstreamMap.ps1",
        "scripts/Generate-PhotoUpstreamMap.ps1",
        "scripts/Generate-VideoUpstreamMap.ps1",
          "scripts/Build-AndroidRuntimeInput.ps1",
          "scripts/Build-OpenCV.ps1",
          "scripts/Apply-OpenCvSourcePatches.ps1",
          "scripts/Get-NativeRuntimeProfileEvidence.ps1",
          "scripts/Invoke-LifecycleRefreshContainerProducer.ps1",
          "scripts/Invoke-LifecycleRefreshContainerProducer.sh",
          "scripts/Test-LifecycleRefreshCandidateProducerSurface.ps1",
          "scripts/Test-LifecycleRefreshCandidatePackageSurface.ps1",
        "scripts/Test-LifecycleRefreshRuntimeInputArtifact.ps1",
        "scripts/Test-GenericLinuxFeasibilityContract.ps1",
        "scripts/Test-GenericLinuxPreviewMatrix.ps1",
        "scripts/New-GenericLinuxPreviewPackage.ps1",
        "scripts/Test-GenericLinuxPreviewPackageSurface.ps1",
        "scripts/Test-GenericLinuxPreviewConsumer.ps1",
        "scripts/Test-GenericLinuxArm64PreviewMatrix.ps1",
        "scripts/Test-GenericLinuxArm64PreviewPackageSurface.ps1",
        "scripts/Test-HeadlessRuntimeProfileContract.ps1",
        "scripts/Measure-HeadlessCandidateEvidence.ps1",
        "scripts/Test-HeadlessCandidateEvidence.ps1",
        "scripts/Test-GenericLinuxMuslFeasibilityContract.ps1",
        "scripts/Test-DomesticPlatformCertificationMatrix.ps1",
        "scripts/Test-OpenClTapiUmatContract.ps1",
        "scripts/Test-CapabilitiesJsonContract.ps1",
        "scripts/Measure-CapabilityProfileEvidence.ps1",
        "scripts/Test-CapabilityProfileEvidence.ps1",
        ".github/workflows/generic-linux-arm64-preview.yml",
        "scripts/Test-GitHubPackConsumerRestoreSurface.ps1",
          "packaging/runtime/patches/opencv-5.0.0-photo-ccm-instance-color-space.patch",
          "scripts/New-RuntimeInputArtifact.ps1",
        "scripts/Get-SampleModelAssets.ps1",
        "scripts/New-ReleaseCandidateFinalCloseout.ps1",
        "scripts/New-NuGetPublicationBundle.ps1",
        "scripts/Test-NuGetPublicationManifest.ps1",
        "scripts/New-ReleasePackageSbom.ps1",
        "scripts/Test-ApiAbiBaselineContract.ps1",
        "scripts/Test-Calib3DUpstreamMap.ps1",
        "scripts/Test-CoreUpstreamMap.ps1",
        "scripts/Test-DnnUpstreamMap.ps1",
        "scripts/Test-DnnSuperresUpstreamMap.ps1",
        "scripts/Test-DnnObjDetectUpstreamMap.ps1",
        "scripts/Test-CcalibUpstreamMap.ps1",
        "scripts/Test-SuperresUpstreamMap.ps1",
        "scripts/Test-JuliaUpstreamMap.ps1",
        "scripts/Test-RgbdUpstreamMap.ps1",
        "scripts/Test-WechatQRCodeUpstreamMap.ps1",
        "scripts/Test-Cnn3DObjUpstreamMap.ps1",
        "scripts/Test-SignalUpstreamMap.ps1",
        "scripts/Test-DpmUpstreamMap.ps1",
        "scripts/Test-HdfUpstreamMap.ps1",
        "scripts/Test-TextUpstreamMap.ps1",
        "scripts/Test-FeaturesUpstreamMap.ps1",
        "scripts/Test-HighGuiUpstreamMap.ps1",
        "scripts/Test-ImgProcUpstreamMap.ps1",
        "scripts/Test-LifecycleRefreshRuntimeMatrix.ps1",
        "scripts/Test-GitHubPackArtifactMatrixSurface.ps1",
        "scripts/Test-ManagedPackageIsolatedArtifactSurface.ps1",
        "scripts/Test-ManagedPackageStandaloneLocalConsumerCompile.ps1",
        "scripts/Test-MlUpstreamMap.ps1",
        "scripts/Test-TrackingUpstreamMap.ps1",
        "scripts/Test-StitchingUpstreamMap.ps1",
        "scripts/Test-ImgCodecsUpstreamMap.ps1",
        "scripts/Test-NativeManagedBindingMap.ps1",
        "scripts/Test-ObjDetectUpstreamMap.ps1",
        "scripts/Test-PhotoUpstreamMap.ps1",
        "scripts/Test-ReleaseCandidateFinalCloseout.ps1",
        "scripts/Test-ReleaseCandidateProvenance.ps1",
        "scripts/Test-ReleaseChangeControlRecord.ps1",
        "scripts/Test-ReleasePackageReproducibility.ps1",
        "scripts/Test-ReleasePackageSbom.ps1",
        "scripts/Test-ReleaseReadinessContract.ps1",
        "scripts/Test-ReleaseNotesContract.ps1",
        "scripts/Test-ReleaseSigningBoundary.ps1",
        "scripts/Test-SampleModelAssetContract.ps1",
        "scripts/Test-NuGetRepositorySignedPackage.ps1",
        "scripts/Test-NuGetRepositorySigningBoundary.ps1",
        "scripts/Test-ImageAdapterLicenseGate.ps1",
        "scripts/Test-PackageMetadataNeutrality.ps1",
        "scripts/Test-ReleaseSupportContract.ps1",
        "scripts/Test-VideoIORegistrySurface.ps1",
        "scripts/Test-VideoIOUpstreamMap.ps1",
        "scripts/Test-VideoUpstreamMap.ps1",
        "scripts/Test-WorkflowInvariantCoverage.ps1",
        "docs/releases/README.md",
        "docs/releases/5.0.0-preview.1.md",
        "docs/releases/5.0.0.md",
        "src/OpenCvSharp.Native/generated/native_abi_manifest.txt",
        "src/OpenCvSharp.Native/generated/native_abi_mini_manifest.txt",
        "src/OpenCvSharp.Native/include/open_cv_sharp/highgui/highgui.h",
        "src/OpenCvSharp.Native/src/highgui/highgui.cpp",
        "src/OpenCvSharp.Native/include/open_cv_sharp/stitching/stitching.h",
        "src/OpenCvSharp.Native/src/stitching/stitching.cpp",
        "src/OpenCvSharp.Native/src/stitching/stitching_handles.h",
        "src/OpenCvSharp.Native/src/imgproc.cpp",
        "src/OpenCvSharp.Native/tests/native_smoke.cpp",
        "src/OpenCvSharp/ImgProc/Cv2.RemainingParity.cs",
        "src/OpenCvSharp/ImgProc/FontFace.cs",
        "src/OpenCvSharp/Plot/Plot2d.cs",
        "src/OpenCvSharp/Plot/PlotCv2.cs",
        "src/OpenCvSharp/BioInspired/BioInspiredCv2.cs",
        "src/OpenCvSharp/BioInspired/BioInspiredStringConvert.cs",
        "src/OpenCvSharp/BioInspired/Retina.cs",
        "src/OpenCvSharp/BioInspired/RetinaColorSamplingMethod.cs",
        "src/OpenCvSharp/BioInspired/RetinaFastToneMapping.cs",
        "src/OpenCvSharp/BioInspired/RetinaMagnoParameters.cs",
        "src/OpenCvSharp/BioInspired/RetinaParameters.cs",
        "src/OpenCvSharp/BioInspired/RetinaParvoParameters.cs",
        "src/OpenCvSharp/BioInspired/SegmentationParameters.cs",
        "src/OpenCvSharp/BioInspired/TransientAreasSegmentationModule.cs",
        "src/OpenCvSharp.Native/src/bioinspired/bioinspired.cpp",
        "src/OpenCvSharp.Native/src/bioinspired/bioinspired_handles.h",
        "tests/OpenCvSharp.Tests/BioInspired/BioInspiredTests.cs",
        "src/OpenCvSharp/PhaseUnwrapping/HistogramPhaseUnwrapping.cs",
        "src/OpenCvSharp/PhaseUnwrapping/HistogramPhaseUnwrappingParams.cs",
        "src/OpenCvSharp/PhaseUnwrapping/PhaseUnwrappingCv2.cs",
        "src/OpenCvSharp/PhaseUnwrapping/PhaseUnwrappingObject.cs",
        "src/OpenCvSharp.Native/src/phase_unwrapping/phase_unwrapping.cpp",
        "src/OpenCvSharp.Native/src/phase_unwrapping/phase_unwrapping_handles.h",
        "tests/OpenCvSharp.Tests/PhaseUnwrapping/PhaseUnwrappingTests.cs",
        "src/OpenCvSharp/Hfs/HfsSegment.cs",
        "src/OpenCvSharp/Hfs/HfsSegmentParams.cs",
        "src/OpenCvSharp/Hfs/HfsCv2.cs",
        "src/OpenCvSharp/Internal/Interop/NativeHfsSegmentHandle.cs",
        "src/OpenCvSharp/Internal/Interop/NativeMethods.Hfs.DllImport.cs",
        "src/OpenCvSharp/Internal/Interop/NativeMethods.Hfs.LibraryImport.cs",
        "src/OpenCvSharp.Native/include/open_cv_sharp/hfs/hfs.h",
        "src/OpenCvSharp.Native/src/hfs/hfs.cpp",
        "src/OpenCvSharp.Native/src/hfs/hfs_handles.h",
        "tests/OpenCvSharp.Tests/Hfs/HfsTests.cs",
        "src/OpenCvSharp/Fuzzy/FuzzyCv2.cs",
        "src/OpenCvSharp/Fuzzy/FuzzyFunctionType.cs",
        "src/OpenCvSharp/Fuzzy/FuzzyInpaintAlgorithm.cs",
        "src/OpenCvSharp/Internal/Interop/NativeMethods.Fuzzy.DllImport.cs",
        "src/OpenCvSharp/Internal/Interop/NativeMethods.Fuzzy.LibraryImport.cs",
        "src/OpenCvSharp.Native/include/open_cv_sharp/fuzzy/fuzzy.h",
        "src/OpenCvSharp.Native/src/fuzzy/fuzzy.cpp",
        "tests/OpenCvSharp.Tests/Fuzzy/FuzzyTests.cs",
        "src/OpenCvSharp/Rapid/RapidCv2.cs",
        "src/OpenCvSharp/Rapid/RapidResult.cs",
        "src/OpenCvSharp/Rapid/RapidTracker.cs",
        "src/OpenCvSharp/Rapid/RapidSilhouetteTracker.cs",
        "src/OpenCvSharp/Rapid/OlsTracker.cs",
        "src/OpenCvSharp/Internal/Interop/NativeMethods.Rapid.DllImport.cs",
        "src/OpenCvSharp/Internal/Interop/NativeMethods.Rapid.LibraryImport.cs",
        "src/OpenCvSharp.Native/include/open_cv_sharp/rapid/rapid.h",
        "src/OpenCvSharp.Native/src/rapid/rapid.cpp",
        "src/OpenCvSharp.Native/src/rapid/rapid_handles.h",
        "tests/OpenCvSharp.Tests/Rapid/RapidTests.cs",
        "src/OpenCvSharp/Shape/ShapeCv2.cs",
        "src/OpenCvSharp/Shape/HistogramCostExtractor.cs",
        "src/OpenCvSharp/Shape/NormHistogramCostExtractor.cs",
        "src/OpenCvSharp/Shape/EMDHistogramCostExtractor.cs",
        "src/OpenCvSharp/Shape/ChiHistogramCostExtractor.cs",
        "src/OpenCvSharp/Shape/EMDL1HistogramCostExtractor.cs",
        "src/OpenCvSharp/Shape/ShapeDistanceExtractor.cs",
        "src/OpenCvSharp/Shape/ShapeContextDistanceExtractor.cs",
        "src/OpenCvSharp/Shape/HausdorffDistanceExtractor.cs",
        "src/OpenCvSharp/Internal/Interop/NativeMethods.Shape.DllImport.cs",
        "src/OpenCvSharp/Internal/Interop/NativeMethods.Shape.LibraryImport.cs",
        "src/OpenCvSharp.Native/include/open_cv_sharp/shape/shape.h",
        "src/OpenCvSharp.Native/src/shape/shape.cpp",
        "src/OpenCvSharp.Native/src/shape/shape_handles.h",
        "tests/OpenCvSharp.Tests/Shape/ShapeTests.cs",
        "src/OpenCvSharp/Internal/Interop/NativeBlenderHandle.cs",
        "src/OpenCvSharp/Internal/Interop/NativeFeaturesMatcherHandle.cs",
        "src/OpenCvSharp/Internal/Interop/NativeHighGuiCallbackRegistrationHandle.cs",
        "src/OpenCvSharp/Internal/CodecBufferLease.cs",
        "scripts/Test-CodecBufferLeasePrototype.ps1",
        "src/OpenCvSharp/Properties/InternalsVisibleTo.cs",
        "src/OpenCvSharp/Internal/Interop/NativeMethods.HighGui.DllImport.cs",
        "src/OpenCvSharp/Internal/Interop/NativeMethods.HighGui.LibraryImport.cs",
        "src/OpenCvSharp/Internal/Interop/NativeImageFeaturesHandle.cs",
        "src/OpenCvSharp/Internal/Interop/NativeMatchesInfoHandle.cs",
        "src/OpenCvSharp/Internal/Interop/NativeEstimatorHandle.cs",
        "src/OpenCvSharp/Internal/Interop/NativeMethods.Stitching.DllImport.cs",
        "src/OpenCvSharp/Internal/Interop/NativeMethods.Stitching.LibraryImport.cs",
        "src/OpenCvSharp/Stitching/AffineBestOf2NearestMatcher.cs",
        "src/OpenCvSharp/Stitching/BestOf2NearestMatcher.cs",
        "src/OpenCvSharp/Stitching/BestOf2NearestRangeMatcher.cs",
        "src/OpenCvSharp/Stitching/Blender.cs",
        "src/OpenCvSharp/Stitching/BlenderType.cs",
        "src/OpenCvSharp/Stitching/Blenders.cs",
        "src/OpenCvSharp/Stitching/BundleAdjusters.cs",
        "src/OpenCvSharp/Stitching/Estimator.cs",
        "src/OpenCvSharp/Stitching/FeaturesMatcher.cs",
        "src/OpenCvSharp/Stitching/ImageFeatures.cs",
        "src/OpenCvSharp/Stitching/MatchesInfo.cs",
        "src/OpenCvSharp/Stitching/StitcherCameraParams.cs",
        "src/OpenCvSharp/Stitching/StitchingMotion.cs",
        "src/OpenCvSharp/HighGui/Cv2.cs",
        "src/OpenCvSharp/HighGui/HighGuiStringConvert.cs",
        "src/OpenCvSharp/HighGui/HighGuiTrackbar.cs",
        "tests/OpenCvSharp.Tests/Stitching/BlenderTests.cs",
        "tests/OpenCvSharp.Tests/Stitching/FeaturesMatcherTests.cs",
        "tests/OpenCvSharp.Tests/Stitching/MotionEstimatorTests.cs",
        "tests/OpenCvSharp.Tests/HighGui/HighGuiInteractionTests.cs",
        "tests/OpenCvSharp.Tests/HighGui/HighGuiTests.cs",
        "tests/OpenCvSharp.Tests/ImgProc/ImgProcRemainingParityTests.cs",
        "tests/OpenCvSharp.Tests/Core/CodecBufferLeaseTests.cs",
        "tests/OpenCvSharp.Tests/ImgCodecs/Cv2InteropTests.cs",
        "tools/Calib3DUpstreamMap/Calib3DUpstreamMap.csproj",
        "tools/Calib3DUpstreamMap/Program.cs",
        "tools/Calib3DUpstreamMap/extract_calib3d.py",
        "tools/CoreUpstreamMap/CoreUpstreamMap.csproj",
        "tools/CoreUpstreamMap/Program.cs",
        "tools/CoreUpstreamMap/extract_core.py",
        "tools/DnnUpstreamMap/DnnUpstreamMap.csproj",
        "tools/DnnUpstreamMap/Program.cs",
        "tools/DnnUpstreamMap/extract_dnn.py",
        "tools/DnnSuperresUpstreamMap/DnnSuperresUpstreamMap.csproj",
        "tools/DnnSuperresUpstreamMap/Program.cs",
        "tools/DnnSuperresUpstreamMap/extract_dnn_superres.py",
        "tools/WechatQRCodeUpstreamMap/WechatQRCodeUpstreamMap.csproj",
        "tools/WechatQRCodeUpstreamMap/Program.cs",
        "tools/WechatQRCodeUpstreamMap/extract_wechat_qrcode.py",
        "tools/Cnn3DObjUpstreamMap/Cnn3DObjUpstreamMap.csproj",
        "tools/Cnn3DObjUpstreamMap/Program.cs",
        "tools/Cnn3DObjUpstreamMap/extract_cnn_3dobj.py",
        "tools/SignalUpstreamMap/SignalUpstreamMap.csproj",
        "tools/SignalUpstreamMap/Program.cs",
        "tools/SignalUpstreamMap/extract_signal.py",
        "tools/DpmUpstreamMap/DpmUpstreamMap.csproj",
        "tools/DpmUpstreamMap/Program.cs",
        "tools/DpmUpstreamMap/extract_dpm.py",
        "tools/HdfUpstreamMap/HdfUpstreamMap.csproj",
        "tools/HdfUpstreamMap/Program.cs",
        "tools/HdfUpstreamMap/extract_hdf.py",
        "tools/TextUpstreamMap/TextUpstreamMap.csproj",
        "tools/TextUpstreamMap/Program.cs",
        "tools/TextUpstreamMap/extract_text.py",
        "tools/FeaturesUpstreamMap/FeaturesUpstreamMap.csproj",
        "tools/FeaturesUpstreamMap/Program.cs",
        "tools/FeaturesUpstreamMap/extract_features.py",
        "tools/HighGuiUpstreamMap/HighGuiUpstreamMap.csproj",
        "tools/HighGuiUpstreamMap/Program.cs",
        "tools/HighGuiUpstreamMap/extract_highgui.py",
        "tools/ImgProcUpstreamMap/ImgProcUpstreamMap.csproj",
        "tools/ImgProcUpstreamMap/Program.cs",
        "tools/ImgProcUpstreamMap/extract_imgproc.py",
        "tools/MlUpstreamMap/MlUpstreamMap.csproj",
        "tools/MlUpstreamMap/Program.cs",
        "tools/MlUpstreamMap/extract_ml.py",
        "tools/TrackingUpstreamMap/TrackingUpstreamMap.csproj",
        "tools/TrackingUpstreamMap/Program.cs",
        "tools/TrackingUpstreamMap/extract_tracking.py",
        "tools/StitchingUpstreamMap/StitchingUpstreamMap.csproj",
        "tools/StitchingUpstreamMap/Program.cs",
        "tools/StitchingUpstreamMap/extract_stitching.py",
        "tools/ImgCodecsUpstreamMap/ImgCodecsUpstreamMap.csproj",
        "tools/ImgCodecsUpstreamMap/Program.cs",
        "tools/ImgCodecsUpstreamMap/extract_imgcodecs.py",
        "tools/NativeManagedBindingMap/NativeManagedBindingMap.csproj",
        "tools/NativeManagedBindingMap/Program.cs",
        "tools/ObjDetectUpstreamMap/ObjDetectUpstreamMap.csproj",
        "tools/ObjDetectUpstreamMap/Program.cs",
        "tools/ObjDetectUpstreamMap/extract_objdetect.py",
        "tools/PhotoUpstreamMap/PhotoUpstreamMap.csproj",
        "tools/PhotoUpstreamMap/Program.cs",
        "tools/PhotoUpstreamMap/extract_photo.py",
        "tools/VideoUpstreamMap/VideoUpstreamMap.csproj",
        "tools/NuGetRepositorySignatureVerifier/NuGetRepositorySignatureVerifier.csproj",
        "tools/NuGetRepositorySignatureVerifier/Program.cs",
        "tools/VideoUpstreamMap/Program.cs",
        "tools/VideoUpstreamMap/extract_video.py",
        "tools/XPhotoUpstreamMap/XPhotoUpstreamMap.csproj",
        "tools/XPhotoUpstreamMap/Program.cs",
        "tools/XPhotoUpstreamMap/extract_xphoto.py",
        "tools/XImgProcUpstreamMap/XImgProcUpstreamMap.csproj",
        "tools/XImgProcUpstreamMap/Program.cs",
        "tools/XImgProcUpstreamMap/extract_ximgproc.py",
        "tools/OptFlowUpstreamMap/OptFlowUpstreamMap.csproj",
        "tools/OptFlowUpstreamMap/Program.cs",
        "tools/OptFlowUpstreamMap/extract_optflow.py",
        "tools/BgSegmUpstreamMap/BgSegmUpstreamMap.csproj",
        "tools/BgSegmUpstreamMap/Program.cs",
        "tools/BgSegmUpstreamMap/extract_bgsegm.py",
        "tools/FaceUpstreamMap/FaceUpstreamMap.csproj",
        "tools/FaceUpstreamMap/Program.cs",
        "tools/FaceUpstreamMap/extract_face.py",
        "tools/QualityUpstreamMap/QualityUpstreamMap.csproj",
        "tools/QualityUpstreamMap/Program.cs",
        "tools/QualityUpstreamMap/extract_quality.py",
        "tools/ImgHashUpstreamMap/ImgHashUpstreamMap.csproj",
        "tools/ImgHashUpstreamMap/Program.cs",
        "tools/ImgHashUpstreamMap/extract_img_hash.py",
        "tools/LineDescriptorUpstreamMap/LineDescriptorUpstreamMap.csproj",
        "tools/LineDescriptorUpstreamMap/Program.cs",
        "tools/LineDescriptorUpstreamMap/extract_line_descriptor.py",
        "tools/FreetypeUpstreamMap/FreetypeUpstreamMap.csproj",
        "tools/FreetypeUpstreamMap/Program.cs",
        "tools/FreetypeUpstreamMap/extract_freetype.py",
        "tools/AlphaMatUpstreamMap/AlphaMatUpstreamMap.csproj",
        "tools/AlphaMatUpstreamMap/Program.cs",
        "tools/AlphaMatUpstreamMap/extract_alphamat.py",
        "tools/IntensityTransformUpstreamMap/IntensityTransformUpstreamMap.csproj",
        "tools/IntensityTransformUpstreamMap/Program.cs",
        "tools/IntensityTransformUpstreamMap/extract_intensity_transform.py",
        "tools/PlotUpstreamMap/PlotUpstreamMap.csproj",
        "tools/PlotUpstreamMap/Program.cs",
        "tools/PlotUpstreamMap/extract_plot.py",
        "tools/BioInspiredUpstreamMap/BioInspiredUpstreamMap.csproj",
        "tools/BioInspiredUpstreamMap/Program.cs",
        "tools/BioInspiredUpstreamMap/extract_bioinspired.py",
        "tools/PhaseUnwrappingUpstreamMap/PhaseUnwrappingUpstreamMap.csproj",
        "tools/PhaseUnwrappingUpstreamMap/Program.cs",
        "tools/PhaseUnwrappingUpstreamMap/extract_phase_unwrapping.py",
        "tools/HfsUpstreamMap/HfsUpstreamMap.csproj",
        "tools/HfsUpstreamMap/Program.cs",
        "tools/HfsUpstreamMap/extract_hfs.py",
        "tools/FuzzyUpstreamMap/FuzzyUpstreamMap.csproj",
        "tools/FuzzyUpstreamMap/Program.cs",
        "tools/FuzzyUpstreamMap/extract_fuzzy.py",
        "tools/RapidUpstreamMap/RapidUpstreamMap.csproj",
        "tools/RapidUpstreamMap/Program.cs",
        "tools/RapidUpstreamMap/extract_rapid.py",
        "tools/ShapeUpstreamMap/ShapeUpstreamMap.csproj",
        "tools/ShapeUpstreamMap/Program.cs",
        "tools/ShapeUpstreamMap/extract_shape.py",
        "tools/StructuredLightUpstreamMap/StructuredLightUpstreamMap.csproj",
        "tools/StructuredLightUpstreamMap/Program.cs",
        "tools/StructuredLightUpstreamMap/extract_structured_light.py",
        "tools/SaliencyUpstreamMap/SaliencyUpstreamMap.csproj",
        "tools/SaliencyUpstreamMap/Program.cs",
        "tools/SaliencyUpstreamMap/extract_saliency.py",
        "tools/SurfaceMatchingUpstreamMap/SurfaceMatchingUpstreamMap.csproj",
        "tools/SurfaceMatchingUpstreamMap/Program.cs",
        "tools/SurfaceMatchingUpstreamMap/extract_surface_matching.py",
        "tools/RegUpstreamMap/RegUpstreamMap.csproj",
        "tools/RegUpstreamMap/Program.cs",
        "tools/RegUpstreamMap/extract_reg.py",
        "tools/XStereoUpstreamMap/XStereoUpstreamMap.csproj",
        "tools/XStereoUpstreamMap/Program.cs",
        "tools/XStereoUpstreamMap/extract_xstereo.py",
        "tools/PtCloudUpstreamMap/PtCloudUpstreamMap.csproj",
        "tools/PtCloudUpstreamMap/Program.cs",
        "tools/PtCloudUpstreamMap/extract_ptcloud.py",
        "tools/XObjDetectUpstreamMap/XObjDetectUpstreamMap.csproj",
        "tools/XObjDetectUpstreamMap/Program.cs",
        "tools/XObjDetectUpstreamMap/extract_xobjdetect.py",
        "tools/XFeatures2DUpstreamMap/XFeatures2DUpstreamMap.csproj",
        "tools/XFeatures2DUpstreamMap/Program.cs",
        "tools/XFeatures2DUpstreamMap/extract_xfeatures2d.py",
        "tools/DnnObjDetectUpstreamMap/DnnObjDetectUpstreamMap.csproj",
        "tools/DnnObjDetectUpstreamMap/Program.cs",
        "tools/DnnObjDetectUpstreamMap/extract_dnn_objdetect.py",
        "tools/CcalibUpstreamMap/CcalibUpstreamMap.csproj",
        "tools/CcalibUpstreamMap/Program.cs",
        "tools/CcalibUpstreamMap/extract_ccalib.py",
        "tools/SuperresUpstreamMap/SuperresUpstreamMap.csproj",
        "tools/SuperresUpstreamMap/Program.cs",
        "tools/SuperresUpstreamMap/extract_superres.py",
        "tools/JuliaUpstreamMap/JuliaUpstreamMap.csproj",
        "tools/JuliaUpstreamMap/Program.cs",
        "tools/JuliaUpstreamMap/extract_julia.py",
        "tools/RgbdUpstreamMap/RgbdUpstreamMap.csproj",
        "tools/RgbdUpstreamMap/Program.cs",
        "tools/RgbdUpstreamMap/extract_rgbd.py",
        "docs/articles/dnn-superres-upstream-parity-guide.md",
        "docs/articles/wechat-qrcode-upstream-parity-guide.md",
        "docs/articles/cnn-3dobj-upstream-parity-guide.md",
        "docs/articles/superres-upstream-parity-guide.md",
        "tools/CodecBenchmark/CodecBenchmark.csproj",
        "tools/CodecBenchmark/Program.cs",
        "scripts/Measure-CodecTypedMatBenchmark.ps1",
        "scripts/Test-CodecBenchmarkHarness.ps1",
        "scripts/Measure-CodecWriterNativeEvidence.ps1",
        "scripts/Test-CodecWriterNativeEvidence.ps1",
        "tools/ScenarioBenchmark/ScenarioBenchmark.csproj",
        "tools/ScenarioBenchmark/Program.cs",
        "scripts/Test-ScenarioBenchmarkHarness.ps1",
        "scripts/Measure-VideoDnnBenchmark.ps1",
        "scripts/Test-ImagePreflightMutationCorpus.ps1",
        "scripts/Measure-ImagePreflightFuzz.ps1",
        "scripts/Test-ImagePreflightFuzzEvidence.ps1",
        "scripts/Measure-CodecNativeMutationEvidence.ps1",
        "scripts/Test-CodecNativeMutationEvidence.ps1",
        "scripts/Test-NativeAotSmoke.ps1",
        "scripts/Measure-NativeAotSmokeEvidence.ps1",
        "scripts/Test-NativeAotSmokeEvidence.ps1",
        "scripts/Test-NativeAotTrimWarningLedger.ps1",
        "scripts/Test-PerformanceBenchmarkEvidence.ps1",
        "scripts/Test-VideoDnnBenchmarkEvidence.ps1",
        "scripts/New-PerformanceRegressionBaseline.ps1",
        "scripts/Test-PerformanceRegressionBaseline.ps1",
        "scripts/Test-TypedMatTargetFrameworks.ps1",
        "scripts/Measure-TypedMatNativeLifetime.ps1",
        "scripts/Test-TypedMatNativeLifetimeEvidence.ps1",
        "scripts/Measure-TypedMatOwnerLifetimeMatrix.ps1",
        "scripts/Test-TypedMatOwnerLifetimeMatrix.ps1",
        "scripts/Test-AsyncVideoOwnerContract.ps1",
        "scripts/Test-DnnDiagnosticsContract.ps1",
        "scripts/Test-ImageFormatPolicy.ps1",
        "scripts/Test-ModernApiPreviewSamples.ps1",
        "scripts/Test-PlatformProbeSample.ps1",
        "scripts/Test-V501ChangeLedger.ps1",
        "scripts/New-UpstreamMapCatalog.ps1",
        "scripts/Test-UpstreamMapCatalog.ps1",
        "scripts/Generate-XPhotoUpstreamMap.ps1",
        "scripts/Test-XPhotoUpstreamMap.ps1",
        "scripts/Generate-XImgProcUpstreamMap.ps1",
        "scripts/Test-XImgProcUpstreamMap.ps1",
        "scripts/Generate-ContribUpstreamMap.ps1",
        "scripts/Generate-OptFlowUpstreamMap.ps1",
        "scripts/Test-OptFlowUpstreamMap.ps1",
        "scripts/Generate-BgSegmUpstreamMap.ps1",
        "scripts/Test-BgSegmUpstreamMap.ps1",
        "scripts/Generate-FaceUpstreamMap.ps1",
        "scripts/Test-FaceUpstreamMap.ps1",
        "scripts/Generate-QualityUpstreamMap.ps1",
        "scripts/Test-QualityUpstreamMap.ps1",
        "scripts/Generate-ImgHashUpstreamMap.ps1",
        "scripts/Test-ImgHashUpstreamMap.ps1",
        "scripts/Generate-LineDescriptorUpstreamMap.ps1",
        "scripts/Test-LineDescriptorUpstreamMap.ps1",
        "scripts/Generate-FreetypeUpstreamMap.ps1",
        "scripts/Test-FreetypeUpstreamMap.ps1",
        "scripts/Generate-AlphaMatUpstreamMap.ps1",
        "scripts/Test-AlphaMatUpstreamMap.ps1",
        "scripts/Generate-IntensityTransformUpstreamMap.ps1",
        "scripts/Test-IntensityTransformUpstreamMap.ps1",
        "scripts/Generate-PlotUpstreamMap.ps1",
        "scripts/Test-PlotUpstreamMap.ps1",
        "scripts/Generate-BioInspiredUpstreamMap.ps1",
        "scripts/Test-BioInspiredUpstreamMap.ps1",
        "scripts/Generate-PhaseUnwrappingUpstreamMap.ps1",
        "scripts/Test-PhaseUnwrappingUpstreamMap.ps1",
        "scripts/Generate-HfsUpstreamMap.ps1",
        "scripts/Test-HfsUpstreamMap.ps1",
        "scripts/Generate-FuzzyUpstreamMap.ps1",
        "scripts/Test-FuzzyUpstreamMap.ps1",
        "scripts/Generate-RapidUpstreamMap.ps1",
        "scripts/Test-RapidUpstreamMap.ps1",
        "scripts/Generate-ShapeUpstreamMap.ps1",
        "scripts/Test-ShapeUpstreamMap.ps1",
        "scripts/Generate-StructuredLightUpstreamMap.ps1",
        "scripts/Test-StructuredLightUpstreamMap.ps1",
        "scripts/Generate-SaliencyUpstreamMap.ps1",
        "scripts/Test-SaliencyUpstreamMap.ps1",
        "scripts/Generate-SurfaceMatchingUpstreamMap.ps1",
        "scripts/Test-SurfaceMatchingUpstreamMap.ps1",
        "scripts/Generate-RegUpstreamMap.ps1",
        "scripts/Test-RegUpstreamMap.ps1",
        "scripts/Generate-XStereoUpstreamMap.ps1",
        "scripts/Test-XStereoUpstreamMap.ps1",
        "scripts/Generate-PtCloudUpstreamMap.ps1",
        "scripts/Test-PtCloudUpstreamMap.ps1",
        "scripts/Generate-XObjDetectUpstreamMap.ps1",
        "scripts/Test-XObjDetectUpstreamMap.ps1",
        "scripts/Generate-XFeatures2DUpstreamMap.ps1",
        "scripts/Test-XFeatures2DUpstreamMap.ps1",
        "scripts/Initialize-UpstreamMapSourceEvidence.ps1",
        "tools/TypedMatBenchmark/TypedMatBenchmark.csproj",
        "tools/TypedMatBenchmark/Program.cs",
        "scripts/Test-TypedMatBenchmarkHarness.ps1",
        "tools/AotSmoke/AotSmoke.csproj",
        "tools/AotSmoke/Program.cs"
    )
    $evidence = @(Get-OrdinalSortedObjects -Values @($evidencePaths | ForEach-Object { Get-FileEvidence -RelativePath $_ }) -Property "Path")
    $winX86FullStatus = if (@($support.realSupport) -contains 'win-x86/full') { 'real-supported' } else { [string](@($support.pending | Where-Object target -eq 'win-x86/full')[0].status) }

    $blockers = @(
        [ordered]@{ Id = "android-arm-device-evidence"; Status = "android-evidence-pending"; Evidence = "Android x64/x86 Full and Mini have authoritative single-loader emulator loading evidence. Android ARM/ARM64 NDK, ELF, same-run package, and APK evidence has passed; matching device loading evidence remains required before promotion." },
        [ordered]@{ Id = "api-gap-implementation"; Status = "open-local-follow-up"; Evidence = "The structured ImgProc, ImgCodecs, VideoIO, Calib3D, Core, DNN, Features, ObjDetect, main CPU Photo, and main Video slices are closed at zero missing callable declarations. Repository-wide upstream parity and prioritized ownership/marshalling work remain open." },
        [ordered]@{ Id = "macos-support-decision"; Status = "decision-deferred"; Evidence = "macOS is outside the declared matrix until an explicit decision and native/consumer evidence exist." },
        [ordered]@{ Id = "publication-authorization"; Status = "not-authorized"; Evidence = "No publish, tag, release, or mutable feed operation is authorized in the current quota state." },
        [ordered]@{ Id = "release-approval"; Status = "candidate-token-required"; Evidence = "The owner accepted the version-bounded stable 5.0.0 single-maintainer risk exception; exact candidate bytes still require the dry-run authorization token." },
        [ordered]@{ Id = "repository-signing-verification"; Status = "post-publication-required"; Evidence = "NuGet.org must add a Repository primary signature and pass exact payload comparison; GitHub Packages must be public, repository-linked, and byte-identical to the reviewed candidate." },
        [ordered]@{ Id = "sbom-inputs"; Status = "candidate-refresh-required"; Evidence = "The deterministic SPDX-2.3 generator and guard are provisioned; final package-bound documents must be regenerated from the final source commit and approved." }
    )

    [ordered]@{
        SchemaVersion = 2
        RecordKind = "local-release-candidate-closeout"
        CandidateId = "local-closeout/sha256/$($sourceSet.Sha256.Substring(0, 16))"
        SourceIdentity = "sha256:$($sourceSet.Sha256)"
        OpenCvRevision = "5.0.0"
        SourceSet = [ordered]@{
            FileCount = $sourceSet.FileCount
            Sha256 = $sourceSet.Sha256
            HashPolicy = "UTF-8 text normalized to LF; non-text bytes hashed raw; closeout record excluded"
        }
        PackageMatrix = [ordered]@{
            Path = "packaging/runtime/runtime-package-matrix.json"
            Sha256 = (Get-FileHash -LiteralPath $matrixPath -Algorithm SHA256).Hash.ToLowerInvariant()
            RidCount = @($matrix.rids).Count
            ProfileCount = @($matrix.profiles).Count
            EntryCount = @($matrix.rids).Count * @($matrix.profiles).Count
        }
        LifecycleRefreshMatrix = [ordered]@{
            Path = "packaging/runtime/runtime-lifecycle-refresh-matrix.json"
            Sha256 = (Get-FileHash -LiteralPath $lifecycleRefreshMatrixPath -Algorithm SHA256).Hash.ToLowerInvariant()
            Status = [string]$lifecycleRefreshMatrix.status
            RidCount = @($lifecycleRefreshMatrix.rids).Count
            ProfileCount = @($lifecycleRefreshMatrix.profiles).Count
            TargetCount = @($lifecycleRefreshMatrix.rids | ForEach-Object { @($_.producer.profiles).Count } | Measure-Object -Sum).Sum
            PublicationAllowed = [bool]$lifecycleRefreshMatrix.publicationAllowed
        }
        SupportContract = [ordered]@{
            Path = "packaging/runtime/runtime-support-contract.json"
            Sha256 = (Get-FileHash -LiteralPath (Join-Path $repo "packaging/runtime/runtime-support-contract.json") -Algorithm SHA256).Hash.ToLowerInvariant()
            MatrixEntryCount = @($support.realSupport).Count + @($support.compatibilityOnly).Count + @($support.pending).Count + @($support.excluded).Count
            RealSupportCount = @($support.realSupport).Count
            CompatibilityOnlyCount = @($support.compatibilityOnly).Count
            PendingSupportCount = @($support.pending).Count
            ExcludedSupportCount = @($support.excluded).Count
            OutsideMatrixCount = @($support.outsideMatrix).Count
            LifecycleRefreshCandidateCount = @($support.lifecycleRefreshCandidates).Count
            LifecycleRefreshCandidates = @($support.lifecycleRefreshCandidates | Sort-Object rid)
            WinX86FullStatus = $winX86FullStatus
            WinX86MiniStatus = ($support.excluded | Where-Object { $_.target -eq "win-x86/mini" }).status
            PackageSurfaceDefinesSupport = [bool]$support.policy.packageSurfaceIsSupport
            HostedPromotionEvidence = $support.hostedPromotionEvidence
        }
        ApiAbiBaseline = [ordered]@{
            Managed = [ordered]@{
                Path = "compatibility/managed-public-api.txt"
                SummaryPath = "compatibility/managed-public-api-summary.json"
                Sha256 = $summary.baselineSha256
                TypeCount = [int]$summary.typeCount
                MemberCount = [int]$summary.memberCount
                NamespaceCount = [int]$summary.namespaceCount
                TargetFramework = $summary.targetFramework
            }
            NativeFull = [ordered]@{
                Path = "src/OpenCvSharp.Native/generated/native_abi_manifest.txt"
                Sha256 = (Get-FileHash -LiteralPath (Join-Path $repo "src/OpenCvSharp.Native/generated/native_abi_manifest.txt") -Algorithm SHA256).Hash.ToLowerInvariant()
                FunctionCount = 2669
            }
            NativeMini = [ordered]@{
                Path = "src/OpenCvSharp.Native/generated/native_abi_mini_manifest.txt"
                Sha256 = (Get-FileHash -LiteralPath (Join-Path $repo "src/OpenCvSharp.Native/generated/native_abi_mini_manifest.txt") -Algorithm SHA256).Hash.ToLowerInvariant()
                FunctionCount = 527
            }
            NativeManagedBindingMap = [ordered]@{
                Path = "compatibility/native-managed-binding-map.txt"
                SummaryPath = "compatibility/native-managed-binding-summary.json"
                Sha256 = $bindingSummary.mappingSha256
                NativeFunctionCount = [int]$bindingSummary.nativeFunctionCount
                ManagedBoundCount = [int]$bindingSummary.managedBoundCount
                UnboundCount = [int]$bindingSummary.unboundCount
                ManagedOnlyCount = [int]$bindingSummary.managedOnlyCount
            }
            ImgProcUpstreamMap = [ordered]@{
                Path = "compatibility/imgproc-upstream-map.txt"
                SummaryPath = "compatibility/imgproc-upstream-summary.json"
                FamilyInventoryPath = "compatibility/imgproc-implemented-families.json"
                Sha256 = $imgProcSummary.mappingSha256
                DeclarationCount = [int]$imgProcSummary.declarationCount
                CallableCount = [int]$imgProcSummary.callableCount
                ImplementedCount = [int]$imgProcSummary.classificationCounts.implemented
                MissingCount = [int]$imgProcSummary.classificationCounts.missing
                IntentionallyOmittedCount = [int]$imgProcSummary.classificationCounts.'intentionally-omitted'
                SelectedFamilyCount = [int]$imgProcSummary.selectedFamilyCount
                SelectedDeclarationCount = [int]$imgProcSummary.selectedDeclarationCount
                ManagedPublicMemberAdditionCount = [int]$imgProcSummary.managedPublicMemberAdditionCount
                RepositoryWideParityClaimed = [bool]$imgProcSummary.repositoryWideUpstreamParityClaimed
            }
            ImgCodecsUpstreamMap = [ordered]@{
                Path = "compatibility/imgcodecs-upstream-map.txt"
                SummaryPath = "compatibility/imgcodecs-upstream-summary.json"
                FamilyInventoryPath = "compatibility/imgcodecs-implemented-families.json"
                SourceReviewedExtensionsPath = "compatibility/imgcodecs-source-reviewed-extensions.json"
                Sha256 = $imgCodecsSummary.mappingSha256
                DeclarationCount = [int]$imgCodecsSummary.declarationCount
                CallableCount = [int]$imgCodecsSummary.callableCount
                ImplementedCount = [int]$imgCodecsSummary.classificationCounts.implemented
                MissingCount = [int]$imgCodecsSummary.classificationCounts.missing
                IntentionallyOmittedCount = [int]$imgCodecsSummary.classificationCounts.'intentionally-omitted'
                SelectedFamilyCount = [int]$imgCodecsSummary.selectedFamilyCount
                SelectedDeclarationCount = [int]$imgCodecsSummary.selectedDeclarationCount
                ManagedPublicTypeAdditionCount = [int]$imgCodecsSummary.managedPublicTypeAdditionCount
                ManagedPublicMemberAdditionCount = [int]$imgCodecsSummary.managedPublicMemberAdditionCount
                RepositoryWideParityClaimed = [bool]$imgCodecsSummary.repositoryWideUpstreamParityClaimed
            }
            VideoIOUpstreamMap = [ordered]@{
                Path = "compatibility/videoio-upstream-map.txt"
                SummaryPath = "compatibility/videoio-upstream-summary.json"
                FamilyInventoryPath = "compatibility/videoio-implemented-families.json"
                RegistrySurfacePath = "compatibility/videoio-registry-surface.json"
                Sha256 = $videoIOSummary.mappingSha256
                RegistrySurfaceSha256 = (Get-FileHash -LiteralPath (Join-Path $repo "compatibility/videoio-registry-surface.json") -Algorithm SHA256).Hash.ToLowerInvariant()
                DeclarationCount = [int]$videoIOSummary.declarationCount
                CallableCount = [int]$videoIOSummary.callableCount
                ImplementedCount = [int]$videoIOSummary.classificationCounts.implemented
                MissingCount = [int]$videoIOSummary.classificationCounts.missing
                IntentionallyOmittedCount = [int]$videoIOSummary.classificationCounts.'intentionally-omitted'
                RegistryOperationCount = @($videoIORegistry.operations).Count
                RepositoryWideParityClaimed = [bool]$videoIOSummary.repositoryWideUpstreamParityClaimed
            }
            Calib3DUpstreamMap = [ordered]@{
                Path = "compatibility/calib3d-upstream-map.txt"
                SummaryPath = "compatibility/calib3d-upstream-summary.json"
                FamilyInventoryPath = "compatibility/calib3d-implemented-families.json"
                Sha256 = $calib3DSummary.mappingSha256
                DeclarationCount = [int]$calib3DSummary.declarationCount
                CallableCount = [int]$calib3DSummary.callableCount
                ImplementedCount = [int]$calib3DSummary.classificationCounts.implemented
                MissingCount = [int]$calib3DSummary.classificationCounts.missing
                IntentionallyOmittedCount = [int]$calib3DSummary.classificationCounts.'intentionally-omitted'
                SourceHeaderCount = [int]$calib3DSummary.sourceHeaderCount
                SelectedFamilyCount = [int]$calib3DSummary.selectedFamilyCount
                SelectedDeclarationCount = [int]$calib3DSummary.selectedDeclarationCount
                ManagedPublicTypeAdditionCount = [int]$calib3DSummary.managedPublicTypeAdditionCount
                ManagedPublicMemberAdditionCount = [int]$calib3DSummary.managedPublicMemberAdditionCount
                RepositoryWideParityClaimed = [bool]$calib3DSummary.repositoryWideUpstreamParityClaimed
            }
            CoreUpstreamMap = [ordered]@{
                Path = "compatibility/core-upstream-map.txt"
                SummaryPath = "compatibility/core-upstream-summary.json"
                ClassificationPath = "compatibility/core-upstream-classifications.json"
                FamilyInventoryPath = "compatibility/core-implemented-families.json"
                Sha256 = $coreSummary.mappingSha256
                DeclarationCount = [int]$coreSummary.declarationCount
                CallableCount = [int]$coreSummary.callableCount
                ImplementedCount = [int]$coreSummary.classificationCounts.implemented
                MissingCount = [int]$coreSummary.classificationCounts.missing
                IntentionallyOmittedCount = [int]$coreSummary.classificationCounts.'intentionally-omitted'
                UnsupportedCount = [int]$coreSummary.classificationCounts.unsupported
                UpstreamConditionalCount = [int]$coreSummary.classificationCounts.'upstream-conditional'
                SourceHeaderCount = [int]$coreSummary.sourceHeaderCount
                SelectedFamilyCount = [int]$coreSummary.selectedFamilyCount
                SelectedDeclarationCount = [int]$coreSummary.selectedDeclarationCount
                ManagedPublicTypeAdditionCount = [int]$coreSummary.managedPublicTypeAdditionCount
                ManagedPublicMemberAdditionCount = [int]$coreSummary.managedPublicMemberAdditionCount
                RepositoryWideParityClaimed = [bool]$coreSummary.repositoryWideUpstreamParityClaimed
            }
            DnnUpstreamMap = [ordered]@{
                Path = "compatibility/dnn-upstream-map.txt"
                SummaryPath = "compatibility/dnn-upstream-summary.json"
                ClassificationPath = "compatibility/dnn-upstream-classifications.json"
                FamilyInventoryPath = "compatibility/dnn-implemented-families.json"
                Sha256 = $dnnSummary.mappingSha256
                DeclarationCount = [int]$dnnSummary.declarationCount
                CallableCount = [int]$dnnSummary.callableCount
                ImplementedCount = [int]$dnnSummary.classificationCounts.implemented
                MissingCount = [int]$dnnSummary.classificationCounts.missing
                IntentionallyOmittedCount = [int]$dnnSummary.classificationCounts.'intentionally-omitted'
                UnsupportedCount = [int]$dnnSummary.classificationCounts.unsupported
                UpstreamConditionalCount = [int]$dnnSummary.classificationCounts.'upstream-conditional'
                SourceHeaderCount = [int]$dnnSummary.sourceHeaderCount
                SelectedFamilyCount = [int]$dnnSummary.selectedFamilyCount
                SelectedDeclarationCount = [int]$dnnSummary.selectedDeclarationCount
                ManagedPublicTypeAdditionCount = [int]$dnnSummary.managedPublicTypeAdditionCount
                ManagedPublicMemberAdditionCount = [int]$dnnSummary.managedPublicMemberAdditionCount
                RepositoryWideParityClaimed = [bool]$dnnSummary.repositoryWideUpstreamParityClaimed
            }
            DnnSuperresUpstreamMap = [ordered]@{
                Path = "compatibility/dnn_superres-upstream-map.txt"
                SummaryPath = "compatibility/dnn_superres-upstream-summary.json"
                ClassificationPath = "compatibility/dnn_superres-upstream-classifications.json"
                FamilyInventoryPath = "compatibility/dnn_superres-implemented-families.json"
                RawExtractionPath = "compatibility/dnn_superres-upstream-raw.json"
                Sha256 = $dnnSuperresSummary.mappingSha256
                RawSha256 = (Get-FileHash -LiteralPath (Join-Path $repo "compatibility/dnn_superres-upstream-raw.json") -Algorithm SHA256).Hash.ToLowerInvariant()
                DeclarationCount = [int]$dnnSuperresSummary.declarationCount
                CallableCount = [int]$dnnSuperresSummary.callableCount
                ImplementedCount = [int]$dnnSuperresSummary.classificationCounts.implemented
                MissingCount = [int]$dnnSuperresSummary.classificationCounts.missing
                IntentionallyOmittedCount = [int]$dnnSuperresSummary.classificationCounts.'intentionally-omitted'
                UnsupportedCount = [int]$dnnSuperresSummary.classificationCounts.unsupported
                UpstreamConditionalCount = [int]$dnnSuperresSummary.classificationCounts.'upstream-conditional'
                SourceHeaderCount = @($dnnSuperresRaw.sourceHeaders).Count
                SelectedFamilyCount = [int]$dnnSuperresSummary.selectedFamilyCount
                SelectedDeclarationCount = [int]$dnnSuperresSummary.selectedDeclarationCount
                ManagedPublicTypeAdditionCount = [int]$dnnSuperresSummary.managedPublicTypeAdditionCount
                ManagedPublicMemberAdditionCount = [int]$dnnSuperresSummary.managedPublicMemberAdditionCount
                NativeEntrypointAdditionCount = [int]$dnnSuperresSummary.nativeEvidenceCount
                RepositoryWideParityClaimed = [bool]$dnnSuperresSummary.repositoryWideUpstreamParityClaimed
            }
            SuperresUpstreamMap = [ordered]@{
                Path = "compatibility/superres-upstream-map.txt"
                SummaryPath = "compatibility/superres-upstream-summary.json"
                ClassificationPath = "compatibility/superres-upstream-classifications.json"
                FamilyInventoryPath = "compatibility/superres-implemented-families.json"
                RawExtractionPath = "compatibility/superres-upstream-raw.json"
                Sha256 = $superresSummary.mappingSha256
                RawSha256 = (Get-FileHash -LiteralPath (Join-Path $repo "compatibility/superres-upstream-raw.json") -Algorithm SHA256).Hash.ToLowerInvariant()
                DeclarationCount = [int]$superresSummary.declarationCount
                CallableCount = [int]$superresSummary.callableCount
                ImplementedCount = [int]$superresSummary.classificationCounts.implemented
                MissingCount = [int]$superresSummary.classificationCounts.missing
                IntentionallyOmittedCount = [int]$superresSummary.classificationCounts.'intentionally-omitted'
                UnsupportedCount = [int]$superresSummary.classificationCounts.unsupported
                UpstreamConditionalCount = [int]$superresSummary.classificationCounts.'upstream-conditional'
                SourceHeaderCount = @($superresRaw.sourceHeaders).Count
                SelectedFamilyCount = [int]$superresSummary.selectedFamilyCount
                SelectedDeclarationCount = [int]$superresSummary.selectedDeclarationCount
                ManagedPublicTypeAdditionCount = [int]$superresSummary.managedPublicTypeAdditionCount
                ManagedPublicMemberAdditionCount = [int]$superresSummary.managedPublicMemberAdditionCount
                NativeEntrypointAdditionCount = [int]$superresSummary.nativeEvidenceCount
                ExternalDataDependencyCount = [int]$superresSummary.externalDataDependencyCount
                RepositoryWideParityClaimed = [bool]$superresSummary.repositoryWideUpstreamParityClaimed
            }
            JuliaUpstreamMap = [ordered]@{
                Path = "compatibility/julia-upstream-map.txt"
                SummaryPath = "compatibility/julia-upstream-summary.json"
                ClassificationPath = "compatibility/julia-upstream-classifications.json"
                FamilyInventoryPath = "compatibility/julia-implemented-families.json"
                RawExtractionPath = "compatibility/julia-upstream-raw.json"
                Sha256 = $juliaSummary.mappingSha256
                RawSha256 = (Get-FileHash -LiteralPath (Join-Path $repo "compatibility/julia-upstream-raw.json") -Algorithm SHA256).Hash.ToLowerInvariant()
                DeclarationCount = [int]$juliaSummary.declarationCount
                CallableCount = [int]$juliaSummary.callableCount
                ImplementedCount = [int]$juliaSummary.classificationCounts.implemented
                MissingCount = [int]$juliaSummary.classificationCounts.missing
                IntentionallyOmittedCount = [int]$juliaSummary.classificationCounts.'intentionally-omitted'
                UnsupportedCount = [int]$juliaSummary.classificationCounts.unsupported
                UpstreamConditionalCount = [int]$juliaSummary.classificationCounts.'upstream-conditional'
                SourceHeaderCount = @($juliaRaw.sourceHeaders).Count
                SelectedFamilyCount = [int]$juliaSummary.selectedFamilyCount
                SelectedDeclarationCount = [int]$juliaSummary.selectedDeclarationCount
                ManagedPublicTypeAdditionCount = [int]$juliaSummary.managedPublicTypeAdditionCount
                ManagedPublicMemberAdditionCount = [int]$juliaSummary.managedPublicMemberAdditionCount
                NativeEntrypointAdditionCount = [int]$juliaSummary.nativeEvidenceCount
                ExternalDataDependencyCount = [int]$juliaSummary.externalDataDependencyCount
                RepositoryWideParityClaimed = [bool]$juliaSummary.repositoryWideUpstreamParityClaimed
            }
            RgbdUpstreamMap = [ordered]@{
                Path = "compatibility/rgbd-upstream-map.txt"
                SummaryPath = "compatibility/rgbd-upstream-summary.json"
                ClassificationPath = "compatibility/rgbd-upstream-classifications.json"
                FamilyInventoryPath = "compatibility/rgbd-implemented-families.json"
                RawExtractionPath = "compatibility/rgbd-upstream-raw.json"
                Sha256 = $rgbdSummary.mappingSha256
                RawSha256 = (Get-FileHash -LiteralPath (Join-Path $repo "compatibility/rgbd-upstream-raw.json") -Algorithm SHA256).Hash.ToLowerInvariant()
                DeclarationCount = [int]$rgbdSummary.declarationCount
                CallableCount = [int]$rgbdSummary.callableCount
                ImplementedCount = [int]$rgbdSummary.classificationCounts.implemented
                MissingCount = [int]$rgbdSummary.classificationCounts.missing
                IntentionallyOmittedCount = [int]$rgbdSummary.classificationCounts.'intentionally-omitted'
                UnsupportedCount = [int]$rgbdSummary.classificationCounts.unsupported
                UpstreamConditionalCount = [int]$rgbdSummary.classificationCounts.'upstream-conditional'
                SourceHeaderCount = @($rgbdRaw.sourceHeaders).Count
                SelectedFamilyCount = [int]$rgbdSummary.selectedFamilyCount
                SelectedDeclarationCount = [int]$rgbdSummary.selectedDeclarationCount
                ManagedPublicTypeAdditionCount = [int]$rgbdSummary.managedPublicTypeAdditionCount
                ManagedPublicMemberAdditionCount = [int]$rgbdSummary.managedPublicMemberAdditionCount
                NativeEntrypointAdditionCount = [int]$rgbdSummary.nativeEvidenceCount
                ExternalDataDependencyCount = [int]$rgbdSummary.externalDataDependencyCount
                RepositoryWideParityClaimed = [bool]$rgbdSummary.repositoryWideUpstreamParityClaimed
            }
            WechatQRCodeUpstreamMap = [ordered]@{
                Path = "compatibility/wechat_qrcode-upstream-map.txt"
                SummaryPath = "compatibility/wechat_qrcode-upstream-summary.json"
                ClassificationPath = "compatibility/wechat_qrcode-upstream-classifications.json"
                FamilyInventoryPath = "compatibility/wechat_qrcode-implemented-families.json"
                RawExtractionPath = "compatibility/wechat_qrcode-upstream-raw.json"
                Sha256 = $wechatQRCodeSummary.mappingSha256
                RawSha256 = (Get-FileHash -LiteralPath (Join-Path $repo "compatibility/wechat_qrcode-upstream-raw.json") -Algorithm SHA256).Hash.ToLowerInvariant()
                DeclarationCount = [int]$wechatQRCodeSummary.declarationCount
                CallableCount = [int]$wechatQRCodeSummary.callableCount
                ImplementedCount = [int]$wechatQRCodeSummary.classificationCounts.implemented
                MissingCount = [int]$wechatQRCodeSummary.classificationCounts.missing
                IntentionallyOmittedCount = [int]$wechatQRCodeSummary.classificationCounts.'intentionally-omitted'
                UnsupportedCount = [int]$wechatQRCodeSummary.classificationCounts.unsupported
                UpstreamConditionalCount = [int]$wechatQRCodeSummary.classificationCounts.'upstream-conditional'
                SourceHeaderCount = @($wechatQRCodeRaw.sourceHeaders).Count
                SelectedFamilyCount = [int]$wechatQRCodeSummary.selectedFamilyCount
                SelectedDeclarationCount = [int]$wechatQRCodeSummary.selectedDeclarationCount
                ManagedPublicTypeAdditionCount = [int]$wechatQRCodeSummary.managedPublicTypeAdditionCount
                ManagedPublicMemberAdditionCount = [int]$wechatQRCodeSummary.managedPublicMemberAdditionCount
                NativeEntrypointAdditionCount = [int]$wechatQRCodeSummary.nativeEvidenceCount
                RepositoryWideParityClaimed = [bool]$wechatQRCodeSummary.repositoryWideUpstreamParityClaimed
            }
            Cnn3DObjUpstreamMap = [ordered]@{
                Path = "compatibility/cnn_3dobj-upstream-map.txt"
                SummaryPath = "compatibility/cnn_3dobj-upstream-summary.json"
                ClassificationPath = "compatibility/cnn_3dobj-upstream-classifications.json"
                FamilyInventoryPath = "compatibility/cnn_3dobj-implemented-families.json"
                RawExtractionPath = "compatibility/cnn_3dobj-upstream-raw.json"
                Sha256 = $cnn3DObjSummary.mappingSha256
                RawSha256 = (Get-FileHash -LiteralPath (Join-Path $repo "compatibility/cnn_3dobj-upstream-raw.json") -Algorithm SHA256).Hash.ToLowerInvariant()
                DeclarationCount = [int]$cnn3DObjSummary.declarationCount
                CallableCount = [int]$cnn3DObjSummary.callableCount
                ImplementedCount = [int]$cnn3DObjSummary.classificationCounts.implemented
                MissingCount = [int]$cnn3DObjSummary.classificationCounts.missing
                IntentionallyOmittedCount = [int]$cnn3DObjSummary.classificationCounts.'intentionally-omitted'
                UnsupportedCount = [int]$cnn3DObjSummary.classificationCounts.unsupported
                UpstreamConditionalCount = [int]$cnn3DObjSummary.classificationCounts.'upstream-conditional'
                SourceHeaderCount = @($cnn3DObjRaw.sourceHeaders).Count
                SelectedFamilyCount = [int]$cnn3DObjSummary.selectedFamilyCount
                SelectedDeclarationCount = [int]$cnn3DObjSummary.selectedDeclarationCount
                ManagedPublicTypeAdditionCount = [int]$cnn3DObjSummary.managedPublicTypeAdditionCount
                ManagedPublicMemberAdditionCount = [int]$cnn3DObjSummary.managedPublicMemberAdditionCount
                NativeEntrypointAdditionCount = [int]$cnn3DObjSummary.nativeEvidenceCount
                RepositoryWideParityClaimed = [bool]$cnn3DObjSummary.repositoryWideUpstreamParityClaimed
            }
            SignalUpstreamMap = [ordered]@{
                Path = "compatibility/signal-upstream-map.txt"
                SummaryPath = "compatibility/signal-upstream-summary.json"
                ClassificationPath = "compatibility/signal-upstream-classifications.json"
                FamilyInventoryPath = "compatibility/signal-implemented-families.json"
                RawExtractionPath = "compatibility/signal-upstream-raw.json"
                Sha256 = $signalSummary.mappingSha256
                RawSha256 = (Get-FileHash -LiteralPath (Join-Path $repo "compatibility/signal-upstream-raw.json") -Algorithm SHA256).Hash.ToLowerInvariant()
                DeclarationCount = [int]$signalSummary.declarationCount
                CallableCount = [int]$signalSummary.callableCount
                ImplementedCount = [int]$signalSummary.classificationCounts.implemented
                MissingCount = [int]$signalSummary.classificationCounts.missing
                IntentionallyOmittedCount = [int]$signalSummary.classificationCounts.'intentionally-omitted'
                UnsupportedCount = [int]$signalSummary.classificationCounts.unsupported
                UpstreamConditionalCount = [int]$signalSummary.classificationCounts.'upstream-conditional'
                SourceHeaderCount = @($signalRaw.sourceHeaders).Count
                SelectedFamilyCount = [int]$signalSummary.selectedFamilyCount
                SelectedDeclarationCount = [int]$signalSummary.selectedDeclarationCount
                ManagedPublicTypeAdditionCount = [int]$signalSummary.managedPublicTypeAdditionCount
                ManagedPublicMemberAdditionCount = [int]$signalSummary.managedPublicMemberAdditionCount
                NativeEntrypointAdditionCount = [int]$signalSummary.nativeEvidenceCount
                RepositoryWideParityClaimed = [bool]$signalSummary.repositoryWideUpstreamParityClaimed
            }
             DpmUpstreamMap = [ordered]@{
                 Path = "compatibility/dpm-upstream-map.txt"
                 SummaryPath = "compatibility/dpm-upstream-summary.json"
                 ClassificationPath = "compatibility/dpm-upstream-classifications.json"
                 FamilyInventoryPath = "compatibility/dpm-implemented-families.json"
                 RawExtractionPath = "compatibility/dpm-upstream-raw.json"
                 Sha256 = $dpmSummary.mappingSha256
                 RawSha256 = (Get-FileHash -LiteralPath (Join-Path $repo "compatibility/dpm-upstream-raw.json") -Algorithm SHA256).Hash.ToLowerInvariant()
                 DeclarationCount = [int]$dpmSummary.declarationCount
                 CallableCount = [int]$dpmSummary.callableCount
                 ImplementedCount = [int]$dpmSummary.classificationCounts.implemented
                 MissingCount = [int]$dpmSummary.classificationCounts.missing
                 IntentionallyOmittedCount = [int]$dpmSummary.classificationCounts.'intentionally-omitted'
                 UnsupportedCount = [int]$dpmSummary.classificationCounts.unsupported
                 UpstreamConditionalCount = [int]$dpmSummary.classificationCounts.'upstream-conditional'
                 SourceHeaderCount = @($dpmRaw.sourceHeaders).Count
                 SelectedFamilyCount = [int]$dpmSummary.selectedFamilyCount
                 SelectedDeclarationCount = [int]$dpmSummary.selectedDeclarationCount
                 ManagedPublicTypeAdditionCount = [int]$dpmSummary.managedPublicTypeAdditionCount
                 ManagedPublicMemberAdditionCount = [int]$dpmSummary.managedPublicMemberAdditionCount
                 NativeEntrypointAdditionCount = [int]$dpmSummary.nativeEvidenceCount
                 RepositoryWideParityClaimed = [bool]$dpmSummary.repositoryWideUpstreamParityClaimed
             }
             HdfUpstreamMap = [ordered]@{
                 Path = "compatibility/hdf-upstream-map.txt"
                 SummaryPath = "compatibility/hdf-upstream-summary.json"
                 ClassificationPath = "compatibility/hdf-upstream-classifications.json"
                 FamilyInventoryPath = "compatibility/hdf-implemented-families.json"
                 RawExtractionPath = "compatibility/hdf-upstream-raw.json"
                 Sha256 = $hdfSummary.mappingSha256
                 RawSha256 = (Get-FileHash -LiteralPath (Join-Path $repo "compatibility/hdf-upstream-raw.json") -Algorithm SHA256).Hash.ToLowerInvariant()
                 DeclarationCount = [int]$hdfSummary.declarationCount
                 CallableCount = [int]$hdfSummary.callableCount
                 ImplementedCount = [int]$hdfSummary.classificationCounts.implemented
                 MissingCount = [int]$hdfSummary.classificationCounts.missing
                 IntentionallyOmittedCount = [int]$hdfSummary.classificationCounts.'intentionally-omitted'
                 UnsupportedCount = [int]$hdfSummary.classificationCounts.unsupported
                 UpstreamConditionalCount = [int]$hdfSummary.classificationCounts.'upstream-conditional'
                 SourceHeaderCount = @($hdfRaw.sourceHeaders).Count
                 SelectedFamilyCount = [int]$hdfSummary.selectedFamilyCount
                 SelectedDeclarationCount = [int]$hdfSummary.selectedDeclarationCount
                 ManagedPublicTypeAdditionCount = [int]$hdfSummary.managedPublicTypeAdditionCount
                 ManagedPublicMemberAdditionCount = [int]$hdfSummary.managedPublicMemberAdditionCount
                 NativeEntrypointAdditionCount = [int]$hdfSummary.nativeEvidenceCount
                 ExternalDataDependencyCount = [int]$hdfSummary.externalDataDependencyCount
                 RepositoryWideParityClaimed = [bool]$hdfSummary.repositoryWideUpstreamParityClaimed
             }
             TextUpstreamMap = [ordered]@{
                 Path = "compatibility/text-upstream-map.txt"
                 SummaryPath = "compatibility/text-upstream-summary.json"
                 ClassificationPath = "compatibility/text-upstream-classifications.json"
                 FamilyInventoryPath = "compatibility/text-implemented-families.json"
                 RawExtractionPath = "compatibility/text-upstream-raw.json"
                 Sha256 = $textSummary.mappingSha256
                 RawSha256 = (Get-FileHash -LiteralPath (Join-Path $repo "compatibility/text-upstream-raw.json") -Algorithm SHA256).Hash.ToLowerInvariant()
                 DeclarationCount = [int]$textSummary.declarationCount
                 CallableCount = [int]$textSummary.callableCount
                 ImplementedCount = [int]$textSummary.classificationCounts.implemented
                 MissingCount = [int]$textSummary.classificationCounts.missing
                 IntentionallyOmittedCount = [int]$textSummary.classificationCounts.'intentionally-omitted'
                 UnsupportedCount = [int]$textSummary.classificationCounts.unsupported
                 UpstreamConditionalCount = [int]$textSummary.classificationCounts.'upstream-conditional'
                 SourceHeaderCount = @($textRaw.sourceHeaders).Count
                 SelectedFamilyCount = [int]$textSummary.selectedFamilyCount
                 SelectedDeclarationCount = [int]$textSummary.selectedDeclarationCount
                 ManagedPublicTypeAdditionCount = [int]$textSummary.managedPublicTypeAdditionCount
                 ManagedPublicMemberAdditionCount = [int]$textSummary.managedPublicMemberAdditionCount
                 NativeEntrypointAdditionCount = [int]$textSummary.nativeEvidenceCount
                 ExternalDataDependencyCount = [int]$textSummary.externalDataDependencyCount
                 RepositoryWideParityClaimed = [bool]$textSummary.repositoryWideUpstreamParityClaimed
             }
            FeaturesUpstreamMap = [ordered]@{
                Path = "compatibility/features-upstream-map.txt"
                SummaryPath = "compatibility/features-upstream-summary.json"
                ClassificationPath = "compatibility/features-upstream-classifications.json"
                FamilyInventoryPath = "compatibility/features-implemented-families.json"
                SourceReviewedExtensionsPath = "compatibility/features-source-reviewed-extensions.json"
                Sha256 = $featuresSummary.mappingSha256
                DeclarationCount = [int]$featuresSummary.declarationCount
                CallableCount = [int]$featuresSummary.callableCount
                ImplementedCount = [int]$featuresSummary.classificationCounts.implemented
                MissingCount = [int]$featuresSummary.classificationCounts.missing
                IntentionallyOmittedCount = [int]$featuresSummary.classificationCounts.'intentionally-omitted'
                UnsupportedCount = [int]$featuresSummary.classificationCounts.unsupported
                UpstreamConditionalCount = [int]$featuresSummary.classificationCounts.'upstream-conditional'
                CompatibilityHeaderCount = [int]$featuresSummary.compatibilityHeaderCount
                SourceHeaderCount = [int]$featuresSummary.sourceHeaderCount
                SourceReviewedExtensionCount = [int]$featuresSummary.sourceReviewedExtensionDeclarationCount
                SelectedFamilyCount = [int]$featuresSummary.selectedFamilyCount
                SelectedDeclarationCount = [int]$featuresSummary.selectedDeclarationCount
                ManagedPublicTypeAdditionCount = [int]$featuresSummary.managedPublicTypeAdditionCount
                ManagedPublicMemberAdditionCount = [int]$featuresSummary.managedPublicMemberAdditionCount
                RepositoryWideParityClaimed = [bool]$featuresSummary.repositoryWideUpstreamParityClaimed
            }
            HighGuiUpstreamMap = [ordered]@{
                Path = "compatibility/highgui-upstream-map.txt"
                SummaryPath = "compatibility/highgui-upstream-summary.json"
                ClassificationPath = "compatibility/highgui-upstream-classifications.json"
                FamilyInventoryPath = "compatibility/highgui-implemented-families.json"
                RawExtractionPath = "compatibility/highgui-upstream-raw.json"
                Sha256 = $highGuiSummary.mappingSha256
                RawSha256 = (Get-FileHash -LiteralPath (Join-Path $repo "compatibility/highgui-upstream-raw.json") -Algorithm SHA256).Hash.ToLowerInvariant()
                DeclarationCount = [int]$highGuiSummary.declarationCount
                CallableCount = [int]$highGuiSummary.callableCount
                ImplementedCount = [int]$highGuiSummary.classificationCounts.implemented
                MissingCount = [int]$highGuiSummary.classificationCounts.missing
                IntentionallyOmittedCount = [int]$highGuiSummary.classificationCounts.'intentionally-omitted'
                UnsupportedCount = [int]$highGuiSummary.classificationCounts.unsupported
                UpstreamConditionalCount = [int]$highGuiSummary.classificationCounts.'upstream-conditional'
                CompatibilityHeaderCount = [int]$highGuiSummary.compatibilityHeaderCount
                ExcludedPublicHeaderCount = [int]$highGuiSummary.excludedPublicHeaderCount
                SourceHeaderCount = [int]$highGuiSummary.sourceHeaderCount
                SourceReviewedExtensionCount = [int]$highGuiSummary.sourceReviewedExtensionCount
                SelectedFamilyCount = [int]$highGuiSummary.selectedFamilyCount
                SelectedDeclarationCount = [int]$highGuiSummary.selectedDeclarationCount
                ManagedPublicTypeAdditionCount = [int]$highGuiSummary.managedPublicTypeAdditionCount
                ManagedPublicMemberAdditionCount = [int]$highGuiSummary.managedPublicMemberAdditionCount
                NativeEntrypointAdditionCount = [int]$highGuiSummary.nativeEntrypointAdditionCount
                RepositoryWideParityClaimed = [bool]$highGuiSummary.repositoryWideUpstreamParityClaimed
            }
            ObjDetectUpstreamMap = [ordered]@{
                Path = "compatibility/objdetect-upstream-map.txt"
                SummaryPath = "compatibility/objdetect-upstream-summary.json"
                ClassificationPath = "compatibility/objdetect-upstream-classifications.json"
                FamilyInventoryPath = "compatibility/objdetect-implemented-families.json"
                RawExtractionPath = "compatibility/objdetect-upstream-raw.json"
                Sha256 = $objDetectSummary.mappingSha256
                RawSha256 = (Get-FileHash -LiteralPath (Join-Path $repo "compatibility/objdetect-upstream-raw.json") -Algorithm SHA256).Hash.ToLowerInvariant()
                DeclarationCount = [int]$objDetectSummary.declarationCount
                CallableCount = [int]$objDetectSummary.callableCount
                ImplementedCount = [int]$objDetectSummary.classificationCounts.implemented
                MissingCount = [int]$objDetectSummary.classificationCounts.missing
                IntentionallyOmittedCount = [int]$objDetectSummary.classificationCounts.'intentionally-omitted'
                UnsupportedCount = [int]$objDetectSummary.classificationCounts.unsupported
                UpstreamConditionalCount = [int]$objDetectSummary.classificationCounts.'upstream-conditional'
                CompatibilityHeaderCount = [int]$objDetectSummary.compatibilityHeaderCount
                SourceHeaderCount = [int]$objDetectSummary.sourceHeaderCount
                SelectedFamilyCount = [int]$objDetectSummary.selectedFamilyCount
                SelectedDeclarationCount = [int]$objDetectSummary.selectedDeclarationCount
                ManagedPublicTypeAdditionCount = [int]$objDetectSummary.managedPublicTypeAdditionCount
                ManagedPublicMemberAdditionCount = [int]$objDetectSummary.managedPublicMemberAdditionCount
                NativeEntrypointAdditionCount = [int]$objDetectSummary.nativeEntrypointAdditionCount
                RepositoryWideParityClaimed = [bool]$objDetectSummary.repositoryWideUpstreamParityClaimed
            }
            PhotoUpstreamMap = [ordered]@{
                Path = "compatibility/photo-upstream-map.txt"
                SummaryPath = "compatibility/photo-upstream-summary.json"
                ClassificationPath = "compatibility/photo-upstream-classifications.json"
                FamilyInventoryPath = "compatibility/photo-implemented-families.json"
                RawExtractionPath = "compatibility/photo-upstream-raw.json"
                Sha256 = $photoSummary.mappingSha256
                RawSha256 = (Get-FileHash -LiteralPath (Join-Path $repo "compatibility/photo-upstream-raw.json") -Algorithm SHA256).Hash.ToLowerInvariant()
                DeclarationCount = [int]$photoSummary.declarationCount
                CallableCount = [int]$photoSummary.callableCount
                ImplementedCount = [int]$photoSummary.classificationCounts.implemented
                MissingCount = [int]$photoSummary.classificationCounts.missing
                IntentionallyOmittedCount = [int]$photoSummary.classificationCounts.'intentionally-omitted'
                UnsupportedCount = [int]$photoSummary.classificationCounts.unsupported
                UpstreamConditionalCount = [int]$photoSummary.classificationCounts.'upstream-conditional'
                CompatibilityHeaderCount = [int]$photoSummary.compatibilityHeaderCount
                ExcludedPublicHeaderCount = [int]$photoSummary.excludedPublicHeaderCount
                SourceHeaderCount = [int]$photoSummary.sourceHeaderCount
                SelectedFamilyCount = [int]$photoSummary.selectedFamilyCount
                SelectedDeclarationCount = [int]$photoSummary.selectedDeclarationCount
                ManagedPublicTypeAdditionCount = [int]$photoSummary.managedPublicTypeAdditionCount
                ManagedPublicMemberAdditionCount = [int]$photoSummary.managedPublicMemberAdditionCount
                NativeEntrypointAdditionCount = [int]$photoSummary.nativeEntrypointAdditionCount
                RepositoryWideParityClaimed = [bool]$photoSummary.repositoryWideUpstreamParityClaimed
            }
            MlUpstreamMap = [ordered]@{
                Path = "compatibility/ml-upstream-map.txt"
                SummaryPath = "compatibility/ml-upstream-summary.json"
                ClassificationPath = "compatibility/ml-upstream-classifications.json"
                FamilyInventoryPath = "compatibility/ml-implemented-families.json"
                RawExtractionPath = "compatibility/ml-upstream-raw.json"
                Sha256 = $mlSummary.mappingSha256
                RawSha256 = (Get-FileHash -LiteralPath (Join-Path $repo "compatibility/ml-upstream-raw.json") -Algorithm SHA256).Hash.ToLowerInvariant()
                DeclarationCount = [int]$mlSummary.declarationCount
                CallableCount = [int]$mlSummary.callableCount
                ImplementedCount = [int]$mlSummary.classificationCounts.implemented
                MissingCount = [int]$mlSummary.classificationCounts.missing
                IntentionallyOmittedCount = [int]$mlSummary.classificationCounts.'intentionally-omitted'
                UnsupportedCount = [int]$mlSummary.classificationCounts.unsupported
                UpstreamConditionalCount = [int]$mlSummary.classificationCounts.'upstream-conditional'
                CompatibilityHeaderCount = [int]$mlSummary.compatibilityHeaderCount
                ExcludedPublicHeaderCount = [int]$mlSummary.excludedPublicHeaderCount
                SourceHeaderCount = [int]$mlSummary.sourceHeaderCount
                SourceReviewedExtensionCount = [int]$mlSummary.sourceReviewedExtensionCount
                SelectedFamilyCount = [int]$mlSummary.selectedFamilyCount
                SelectedDeclarationCount = [int]$mlSummary.selectedDeclarationCount
                ManagedPublicTypeAdditionCount = [int]$mlSummary.managedPublicTypeAdditionCount
                ManagedPublicMemberAdditionCount = [int]$mlSummary.managedPublicMemberAdditionCount
                NativeEntrypointAdditionCount = [int]$mlSummary.nativeEntrypointAdditionCount
                RepositoryWideParityClaimed = [bool]$mlSummary.repositoryWideUpstreamParityClaimed
            }
            TrackingUpstreamMap = [ordered]@{
                Path = "compatibility/tracking-upstream-map.txt"
                SummaryPath = "compatibility/tracking-upstream-summary.json"
                ClassificationPath = "compatibility/tracking-upstream-classifications.json"
                FamilyInventoryPath = "compatibility/tracking-implemented-families.json"
                RawExtractionPath = "compatibility/tracking-upstream-raw.json"
                Sha256 = $trackingSummary.mappingSha256
                RawSha256 = (Get-FileHash -LiteralPath (Join-Path $repo "compatibility/tracking-upstream-raw.json") -Algorithm SHA256).Hash.ToLowerInvariant()
                DeclarationCount = [int]$trackingSummary.declarationCount
                CallableCount = [int]$trackingSummary.callableCount
                ImplementedCount = [int]$trackingSummary.classificationCounts.implemented
                MissingCount = [int]$trackingSummary.classificationCounts.missing
                IntentionallyOmittedCount = [int]$trackingSummary.classificationCounts.'intentionally-omitted'
                UnsupportedCount = [int]$trackingSummary.classificationCounts.unsupported
                UpstreamConditionalCount = [int]$trackingSummary.classificationCounts.'upstream-conditional'
                PrimaryDeclarationCount = [int]$trackingSummary.primaryDeclarationCount
                PrimaryCallableCount = [int]$trackingSummary.primaryCallableCount
                LegacyDeclarationCount = [int]$trackingSummary.legacyDeclarationCount
                LegacyCallableCount = [int]$trackingSummary.legacyCallableCount
                CompatibilityHeaderCount = [int]$trackingSummary.compatibilityHeaderCount
                ExcludedPublicHeaderCount = [int]$trackingSummary.excludedPublicHeaderCount
                SourceHeaderCount = [int]$trackingSummary.sourceHeaderCount
                SourceReviewedExtensionCount = [int]$trackingSummary.sourceReviewedExtensionCount
                SelectedFamilyCount = [int]$trackingSummary.selectedFamilyCount
                SelectedDeclarationCount = [int]$trackingSummary.selectedDeclarationCount
                ManagedPublicTypeAdditionCount = [int]$trackingSummary.managedPublicTypeAdditionCount
                ManagedPublicMemberAdditionCount = [int]$trackingSummary.managedPublicMemberAdditionCount
                NativeEntrypointAdditionCount = [int]$trackingSummary.nativeEntrypointAdditionCount
                MainVideoRowsDoubleCounted = [bool]$trackingSummary.mainVideoRowsDoubleCounted
                LegacyRowsMixedIntoPrimary = [bool]$trackingSummary.legacyRowsMixedIntoPrimary
                RepositoryWideParityClaimed = [bool]$trackingSummary.repositoryWideUpstreamParityClaimed
            }
            StitchingUpstreamMap = [ordered]@{
                Path = "compatibility/stitching-upstream-map.txt"
                SummaryPath = "compatibility/stitching-upstream-summary.json"
                ClassificationPath = "compatibility/stitching-upstream-classifications.json"
                FamilyInventoryPath = "compatibility/stitching-implemented-families.json"
                RawExtractionPath = "compatibility/stitching-upstream-raw.json"
                Sha256 = $stitchingSummary.mappingSha256
                RawSha256 = (Get-FileHash -LiteralPath (Join-Path $repo "compatibility/stitching-upstream-raw.json") -Algorithm SHA256).Hash.ToLowerInvariant()
                DeclarationCount = [int]$stitchingSummary.declarationCount
                CallableCount = [int]$stitchingSummary.callableCount
                ImplementedCount = [int]$stitchingSummary.classificationCounts.implemented
                MissingCount = [int]$stitchingSummary.classificationCounts.missing
                IntentionallyOmittedCount = [int]$stitchingSummary.classificationCounts.'intentionally-omitted'
                UnsupportedCount = [int]$stitchingSummary.classificationCounts.unsupported
                UpstreamConditionalCount = [int]$stitchingSummary.classificationCounts.'upstream-conditional'
                HighLevelDeclarationCount = [int]$stitchingSummary.surfaceCounts.primary.declarations
                HighLevelCallableCount = [int]$stitchingSummary.surfaceCounts.primary.callables
                HighLevelImplementedCount = [int]$stitchingSummary.surfaceCounts.primary.implemented
                PublicWarperDeclarationCount = [int]$stitchingSummary.surfaceCounts.'public-warpers'.declarations
                PublicWarperCallableCount = [int]$stitchingSummary.surfaceCounts.'public-warpers'.callables
                PublicWarperImplementedCount = [int]$stitchingSummary.surfaceCounts.'public-warpers'.implemented
                BlenderDeclarationCount = [int]$stitchingSummary.surfaceCounts.'detail-blenders'.declarations
                BlenderCallableCount = [int]$stitchingSummary.surfaceCounts.'detail-blenders'.callables
                BlenderImplementedCount = [int]$stitchingSummary.surfaceCounts.'detail-blenders'.implemented
                ExposureDeclarationCount = [int]$stitchingSummary.surfaceCounts.'detail-exposure'.declarations
                ExposureCallableCount = [int]$stitchingSummary.surfaceCounts.'detail-exposure'.callables
                ExposureImplementedCount = [int]$stitchingSummary.surfaceCounts.'detail-exposure'.implemented
                MatcherDeclarationCount = [int]$stitchingSummary.surfaceCounts.'detail-matchers'.declarations
                MatcherCallableCount = [int]$stitchingSummary.surfaceCounts.'detail-matchers'.callables
                MatcherImplementedCount = [int]$stitchingSummary.surfaceCounts.'detail-matchers'.implemented
                SeamFinderDeclarationCount = [int]$stitchingSummary.surfaceCounts.'detail-seam-finders'.declarations
                SeamFinderCallableCount = [int]$stitchingSummary.surfaceCounts.'detail-seam-finders'.callables
                SeamFinderImplementedCount = [int]$stitchingSummary.surfaceCounts.'detail-seam-finders'.implemented
                TimelapserDeclarationCount = [int]$stitchingSummary.surfaceCounts.'detail-timelapsers'.declarations
                TimelapserCallableCount = [int]$stitchingSummary.surfaceCounts.'detail-timelapsers'.callables
                TimelapserImplementedCount = [int]$stitchingSummary.surfaceCounts.'detail-timelapsers'.implemented
                UtilityDeclarationCount = [int]$stitchingSummary.surfaceCounts.'detail-util'.declarations
                UtilityCallableCount = [int]$stitchingSummary.surfaceCounts.'detail-util'.callables
                UtilityImplementedCount = [int]$stitchingSummary.surfaceCounts.'detail-util'.implemented
                DetailWarperDeclarationCount = [int]$stitchingSummary.surfaceCounts.'detail-warpers'.declarations
                DetailWarperCallableCount = [int]$stitchingSummary.surfaceCounts.'detail-warpers'.callables
                DetailWarperImplementedCount = [int]$stitchingSummary.surfaceCounts.'detail-warpers'.implemented
                CompatibilityHeaderCount = [int]$stitchingSummary.compatibilityHeaderCount
                SourceHeaderCount = [int]$stitchingSummary.sourceHeaderCount
                SourceReviewedExtensionCount = [int]$stitchingSummary.sourceReviewedExtensionCount
                SelectedFamilyCount = [int]$stitchingSummary.selectedFamilyCount
                SelectedDeclarationCount = [int]$stitchingSummary.selectedDeclarationCount
                ManagedPublicTypeAdditionCount = [int]$stitchingSummary.managedPublicTypeAdditionCount
                ManagedPublicMemberAdditionCount = [int]$stitchingSummary.managedPublicMemberAdditionCount
                NativeEntrypointAdditionCount = [int]$stitchingSummary.nativeEntrypointAdditionCount
                UMatExecutionClaimed = [bool]$stitchingSummary.uMatExecutionClaimed
                DetailRowsMixedIntoHighLevel = [bool]$stitchingSummary.detailRowsMixedIntoHighLevel
                RepositoryWideParityClaimed = [bool]$stitchingSummary.repositoryWideUpstreamParityClaimed
            }
            VideoUpstreamMap = [ordered]@{
                Path = "compatibility/video-upstream-map.txt"
                SummaryPath = "compatibility/video-upstream-summary.json"
                ClassificationPath = "compatibility/video-upstream-classifications.json"
                FamilyInventoryPath = "compatibility/video-implemented-families.json"
                RawExtractionPath = "compatibility/video-upstream-raw.json"
                Sha256 = $videoSummary.mappingSha256
                RawSha256 = (Get-FileHash -LiteralPath (Join-Path $repo "compatibility/video-upstream-raw.json") -Algorithm SHA256).Hash.ToLowerInvariant()
                DeclarationCount = [int]$videoSummary.declarationCount
                CallableCount = [int]$videoSummary.callableCount
                ImplementedCount = [int]$videoSummary.classificationCounts.implemented
                MissingCount = [int]$videoSummary.classificationCounts.missing
                IntentionallyOmittedCount = [int]$videoSummary.classificationCounts.'intentionally-omitted'
                UnsupportedCount = [int]$videoSummary.classificationCounts.unsupported
                UpstreamConditionalCount = [int]$videoSummary.classificationCounts.'upstream-conditional'
                CompatibilityHeaderCount = [int]$videoSummary.compatibilityHeaderCount
                ExcludedPublicHeaderCount = [int]$videoSummary.excludedPublicHeaderCount
                SourceHeaderCount = [int]$videoSummary.sourceHeaderCount
                SelectedFamilyCount = [int]$videoSummary.selectedFamilyCount
                SelectedDeclarationCount = [int]$videoSummary.selectedDeclarationCount
                ManagedPublicTypeAdditionCount = [int]$videoSummary.managedPublicTypeAdditionCount
                ManagedPublicMemberAdditionCount = [int]$videoSummary.managedPublicMemberAdditionCount
                NativeEntrypointAdditionCount = [int]$videoSummary.nativeEntrypointAdditionCount
                RepositoryWideParityClaimed = [bool]$videoSummary.repositoryWideUpstreamParityClaimed
            }
            SurfaceMatchingUpstreamMap = [ordered]@{
                Path = "compatibility/surface_matching-upstream-map.txt"
                SummaryPath = "compatibility/surface_matching-upstream-summary.json"
                ClassificationPath = "compatibility/surface_matching-upstream-classifications.json"
                FamilyInventoryPath = "compatibility/surface_matching-implemented-families.json"
                RawExtractionPath = "compatibility/surface_matching-upstream-raw.json"
                Sha256 = $surfaceMatchingSummary.mappingSha256
                RawSha256 = (Get-FileHash -LiteralPath (Join-Path $repo "compatibility/surface_matching-upstream-raw.json") -Algorithm SHA256).Hash.ToLowerInvariant()
                DeclarationCount = [int]$surfaceMatchingSummary.declarationCount
                CallableCount = [int]$surfaceMatchingSummary.callableCount
                ImplementedCount = [int]$surfaceMatchingSummary.classificationCounts.implemented
                MissingCount = [int]$surfaceMatchingSummary.classificationCounts.missing
                IntentionallyOmittedCount = [int]$surfaceMatchingSummary.classificationCounts.'intentionally-omitted'
                UnsupportedCount = [int]$surfaceMatchingSummary.classificationCounts.unsupported
                UpstreamConditionalCount = [int]$surfaceMatchingSummary.classificationCounts.'upstream-conditional'
                SelectedFamilyCount = [int]$surfaceMatchingSummary.selectedFamilyCount
                SelectedDeclarationCount = [int]$surfaceMatchingSummary.selectedDeclarationCount
                ManagedPublicTypeAdditionCount = [int]$surfaceMatchingSummary.managedPublicTypeAdditionCount
                ManagedPublicMemberAdditionCount = [int]$surfaceMatchingSummary.managedPublicMemberAdditionCount
                NativeEntrypointAdditionCount = [int]$surfaceMatchingSummary.nativeEvidenceCount
                RepositoryWideParityClaimed = [bool]$surfaceMatchingSummary.repositoryWideUpstreamParityClaimed
            }
            RegUpstreamMap = [ordered]@{
                Path = "compatibility/reg-upstream-map.txt"
                SummaryPath = "compatibility/reg-upstream-summary.json"
                ClassificationPath = "compatibility/reg-upstream-classifications.json"
                FamilyInventoryPath = "compatibility/reg-implemented-families.json"
                RawExtractionPath = "compatibility/reg-upstream-raw.json"
                Sha256 = $regSummary.mappingSha256
                RawSha256 = (Get-FileHash -LiteralPath (Join-Path $repo "compatibility/reg-upstream-raw.json") -Algorithm SHA256).Hash.ToLowerInvariant()
                DeclarationCount = [int]$regSummary.declarationCount
                CallableCount = [int]$regSummary.callableCount
                ImplementedCount = [int]$regSummary.classificationCounts.implemented
                MissingCount = [int]$regSummary.classificationCounts.missing
                IntentionallyOmittedCount = [int]$regSummary.classificationCounts.'intentionally-omitted'
                UnsupportedCount = [int]$regSummary.classificationCounts.unsupported
                UpstreamConditionalCount = [int]$regSummary.classificationCounts.'upstream-conditional'
                SourceHeaderCount = @($regRaw.sourceHeaders).Count
                SelectedFamilyCount = [int]$regSummary.selectedFamilyCount
                SelectedDeclarationCount = [int]$regSummary.selectedDeclarationCount
                ManagedPublicTypeAdditionCount = [int]$regSummary.managedPublicTypeAdditionCount
                ManagedPublicMemberAdditionCount = [int]$regSummary.managedPublicMemberAdditionCount
                NativeEntrypointAdditionCount = [int]$regSummary.nativeEvidenceCount
                RepositoryWideParityClaimed = [bool]$regSummary.repositoryWideUpstreamParityClaimed
            }
            XStereoUpstreamMap = [ordered]@{
                Path = "compatibility/xstereo-upstream-map.txt"
                SummaryPath = "compatibility/xstereo-upstream-summary.json"
                ClassificationPath = "compatibility/xstereo-upstream-classifications.json"
                FamilyInventoryPath = "compatibility/xstereo-implemented-families.json"
                RawExtractionPath = "compatibility/xstereo-upstream-raw.json"
                Sha256 = $xstereoSummary.mappingSha256
                RawSha256 = (Get-FileHash -LiteralPath (Join-Path $repo "compatibility/xstereo-upstream-raw.json") -Algorithm SHA256).Hash.ToLowerInvariant()
                DeclarationCount = [int]$xstereoSummary.declarationCount
                CallableCount = [int]$xstereoSummary.callableCount
                ImplementedCount = [int]$xstereoSummary.classificationCounts.implemented
                MissingCount = [int]$xstereoSummary.classificationCounts.missing
                IntentionallyOmittedCount = [int]$xstereoSummary.classificationCounts.'intentionally-omitted'
                UnsupportedCount = [int]$xstereoSummary.classificationCounts.unsupported
                UpstreamConditionalCount = [int]$xstereoSummary.classificationCounts.'upstream-conditional'
                SourceHeaderCount = @($xstereoRaw.sourceHeaders).Count
                SelectedFamilyCount = [int]$xstereoSummary.selectedFamilyCount
                SelectedDeclarationCount = [int]$xstereoSummary.selectedDeclarationCount
                ManagedPublicTypeAdditionCount = [int]$xstereoSummary.managedPublicTypeAdditionCount
                ManagedPublicMemberAdditionCount = [int]$xstereoSummary.managedPublicMemberAdditionCount
                NativeEntrypointAdditionCount = [int]$xstereoSummary.nativeEvidenceCount
                RepositoryWideParityClaimed = [bool]$xstereoSummary.repositoryWideUpstreamParityClaimed
            }
            PtCloudUpstreamMap = [ordered]@{
                Path = "compatibility/ptcloud-upstream-map.txt"
                SummaryPath = "compatibility/ptcloud-upstream-summary.json"
                ClassificationPath = "compatibility/ptcloud-upstream-classifications.json"
                FamilyInventoryPath = "compatibility/ptcloud-implemented-families.json"
                RawExtractionPath = "compatibility/ptcloud-upstream-raw.json"
                Sha256 = $ptcloudSummary.mappingSha256
                RawSha256 = (Get-FileHash -LiteralPath (Join-Path $repo "compatibility/ptcloud-upstream-raw.json") -Algorithm SHA256).Hash.ToLowerInvariant()
                DeclarationCount = [int]$ptcloudSummary.declarationCount
                CallableCount = [int]$ptcloudSummary.callableCount
                ImplementedCount = [int]$ptcloudSummary.classificationCounts.implemented
                MissingCount = [int]$ptcloudSummary.classificationCounts.missing
                IntentionallyOmittedCount = [int]$ptcloudSummary.classificationCounts.'intentionally-omitted'
                UnsupportedCount = [int]$ptcloudSummary.classificationCounts.unsupported
                UpstreamConditionalCount = [int]$ptcloudSummary.classificationCounts.'upstream-conditional'
                SourceHeaderCount = @($ptcloudRaw.sourceHeaders).Count
                ExcludedPublicHeaderCount = @($ptcloudRaw.excludedPublicHeaders).Count
                SelectedFamilyCount = [int]$ptcloudSummary.selectedFamilyCount
                SelectedDeclarationCount = [int]$ptcloudSummary.selectedDeclarationCount
                ManagedPublicTypeAdditionCount = [int]$ptcloudSummary.managedPublicTypeAdditionCount
                ManagedPublicMemberAdditionCount = [int]$ptcloudSummary.managedPublicMemberAdditionCount
                NativeEntrypointAdditionCount = [int]$ptcloudSummary.nativeEvidenceCount
                RepositoryWideParityClaimed = [bool]$ptcloudSummary.repositoryWideUpstreamParityClaimed
            }
            XObjDetectUpstreamMap = [ordered]@{
                Path = "compatibility/xobjdetect-upstream-map.txt"
                SummaryPath = "compatibility/xobjdetect-upstream-summary.json"
                ClassificationPath = "compatibility/xobjdetect-upstream-classifications.json"
                FamilyInventoryPath = "compatibility/xobjdetect-implemented-families.json"
                RawExtractionPath = "compatibility/xobjdetect-upstream-raw.json"
                Sha256 = $xobjdetectSummary.mappingSha256
                RawSha256 = (Get-FileHash -LiteralPath (Join-Path $repo "compatibility/xobjdetect-upstream-raw.json") -Algorithm SHA256).Hash.ToLowerInvariant()
                DeclarationCount = [int]$xobjdetectSummary.declarationCount
                CallableCount = [int]$xobjdetectSummary.callableCount
                ImplementedCount = [int]$xobjdetectSummary.classificationCounts.implemented
                MissingCount = [int]$xobjdetectSummary.classificationCounts.missing
                IntentionallyOmittedCount = [int]$xobjdetectSummary.classificationCounts.'intentionally-omitted'
                UnsupportedCount = [int]$xobjdetectSummary.classificationCounts.unsupported
                UpstreamConditionalCount = [int]$xobjdetectSummary.classificationCounts.'upstream-conditional'
                SourceHeaderCount = @($xobjdetectRaw.sourceHeaders).Count
                SelectedFamilyCount = [int]$xobjdetectSummary.selectedFamilyCount
                SelectedDeclarationCount = [int]$xobjdetectSummary.selectedDeclarationCount
                ManagedPublicTypeAdditionCount = [int]$xobjdetectSummary.managedPublicTypeAdditionCount
                ManagedPublicMemberAdditionCount = [int]$xobjdetectSummary.managedPublicMemberAdditionCount
                NativeEntrypointAdditionCount = [int]$xobjdetectSummary.nativeEvidenceCount
                RepositoryWideParityClaimed = [bool]$xobjdetectSummary.repositoryWideUpstreamParityClaimed
            }
            XFeatures2DUpstreamMap = [ordered]@{
                Path = "compatibility/xfeatures2d-upstream-map.txt"
                SummaryPath = "compatibility/xfeatures2d-upstream-summary.json"
                ClassificationPath = "compatibility/xfeatures2d-upstream-classifications.json"
                FamilyInventoryPath = "compatibility/xfeatures2d-implemented-families.json"
                RawExtractionPath = "compatibility/xfeatures2d-upstream-raw.json"
                Sha256 = $xfeatures2dSummary.mappingSha256
                RawSha256 = (Get-FileHash -LiteralPath (Join-Path $repo "compatibility/xfeatures2d-upstream-raw.json") -Algorithm SHA256).Hash.ToLowerInvariant()
                DeclarationCount = [int]$xfeatures2dSummary.declarationCount
                CallableCount = [int]$xfeatures2dSummary.callableCount
                ImplementedCount = [int]$xfeatures2dSummary.classificationCounts.implemented
                MissingCount = [int]$xfeatures2dSummary.classificationCounts.missing
                IntentionallyOmittedCount = [int]$xfeatures2dSummary.classificationCounts.'intentionally-omitted'
                UnsupportedCount = [int]$xfeatures2dSummary.classificationCounts.unsupported
                UpstreamConditionalCount = [int]$xfeatures2dSummary.classificationCounts.'upstream-conditional'
                SourceHeaderCount = @($xfeatures2dRaw.sourceHeaders).Count
                SelectedFamilyCount = [int]$xfeatures2dSummary.selectedFamilyCount
                SelectedDeclarationCount = [int]$xfeatures2dSummary.selectedDeclarationCount
                ManagedPublicTypeAdditionCount = [int]$xfeatures2dSummary.managedPublicTypeAdditionCount
                ManagedPublicMemberAdditionCount = [int]$xfeatures2dSummary.managedPublicMemberAdditionCount
                NativeEntrypointAdditionCount = [int]$xfeatures2dSummary.nativeEvidenceCount
                ExternalDataDependencyCount = [int]$xfeatures2dSummary.externalDataDependencyCount
                RepositoryWideParityClaimed = [bool]$xfeatures2dSummary.repositoryWideUpstreamParityClaimed
            }
            DnnObjDetectUpstreamMap = [ordered]@{
                Path = "compatibility/dnn_objdetect-upstream-map.txt"
                SummaryPath = "compatibility/dnn_objdetect-upstream-summary.json"
                ClassificationPath = "compatibility/dnn_objdetect-upstream-classifications.json"
                FamilyInventoryPath = "compatibility/dnn_objdetect-implemented-families.json"
                RawExtractionPath = "compatibility/dnn_objdetect-upstream-raw.json"
                Sha256 = $dnnObjDetectSummary.mappingSha256
                RawSha256 = (Get-FileHash -LiteralPath (Join-Path $repo "compatibility/dnn_objdetect-upstream-raw.json") -Algorithm SHA256).Hash.ToLowerInvariant()
                DeclarationCount = [int]$dnnObjDetectSummary.declarationCount
                CallableCount = [int]$dnnObjDetectSummary.callableCount
                ImplementedCount = [int]$dnnObjDetectSummary.classificationCounts.implemented
                MissingCount = [int]$dnnObjDetectSummary.classificationCounts.missing
                IntentionallyOmittedCount = [int]$dnnObjDetectSummary.classificationCounts.'intentionally-omitted'
                UnsupportedCount = [int]$dnnObjDetectSummary.classificationCounts.unsupported
                UpstreamConditionalCount = [int]$dnnObjDetectSummary.classificationCounts.'upstream-conditional'
                SourceHeaderCount = @($dnnObjDetectRaw.sourceHeaders).Count
                SelectedFamilyCount = [int]$dnnObjDetectSummary.selectedFamilyCount
                SelectedDeclarationCount = [int]$dnnObjDetectSummary.selectedDeclarationCount
                ManagedPublicTypeAdditionCount = [int]$dnnObjDetectSummary.managedPublicTypeAdditionCount
                ManagedPublicMemberAdditionCount = [int]$dnnObjDetectSummary.managedPublicMemberAdditionCount
                NativeEntrypointAdditionCount = [int]$dnnObjDetectSummary.nativeEvidenceCount
                ExternalDataDependencyCount = [int]$dnnObjDetectSummary.externalDataDependencyCount
                RepositoryWideParityClaimed = [bool]$dnnObjDetectSummary.repositoryWideUpstreamParityClaimed
            }
            CcalibUpstreamMap = [ordered]@{
                Path = "compatibility/ccalib-upstream-map.txt"
                SummaryPath = "compatibility/ccalib-upstream-summary.json"
                ClassificationPath = "compatibility/ccalib-upstream-classifications.json"
                FamilyInventoryPath = "compatibility/ccalib-implemented-families.json"
                RawExtractionPath = "compatibility/ccalib-upstream-raw.json"
                Sha256 = $ccalibSummary.mappingSha256
                RawSha256 = (Get-FileHash -LiteralPath (Join-Path $repo "compatibility/ccalib-upstream-raw.json") -Algorithm SHA256).Hash.ToLowerInvariant()
                DeclarationCount = [int]$ccalibSummary.declarationCount
                CallableCount = [int]$ccalibSummary.callableCount
                ImplementedCount = [int]$ccalibSummary.classificationCounts.implemented
                MissingCount = [int]$ccalibSummary.classificationCounts.missing
                IntentionallyOmittedCount = [int]$ccalibSummary.classificationCounts.'intentionally-omitted'
                UnsupportedCount = [int]$ccalibSummary.classificationCounts.unsupported
                UpstreamConditionalCount = [int]$ccalibSummary.classificationCounts.'upstream-conditional'
                SourceHeaderCount = @($ccalibRaw.sourceHeaders).Count
                SelectedFamilyCount = [int]$ccalibSummary.selectedFamilyCount
                SelectedDeclarationCount = [int]$ccalibSummary.selectedDeclarationCount
                ManagedPublicTypeAdditionCount = [int]$ccalibSummary.managedPublicTypeAdditionCount
                ManagedPublicMemberAdditionCount = [int]$ccalibSummary.managedPublicMemberAdditionCount
                NativeEntrypointAdditionCount = [int]$ccalibSummary.nativeEvidenceCount
                ExternalDataDependencyCount = [int]$ccalibSummary.externalDataDependencyCount
                RepositoryWideParityClaimed = [bool]$ccalibSummary.repositoryWideUpstreamParityClaimed
            }
            GapInventoryPath = "compatibility/api-gap-inventory.json"
            GapInventoryStatus = $gapInventory.status
            UpstreamParityMeasured = [bool]$gapInventory.measurements.upstreamCppParityMeasured
            NativeToManagedParityMeasured = [bool]$gapInventory.measurements.nativeToManagedParityMeasured
            ImplementedSpanFamilyPath = "compatibility/imgproc-point-set-span-family.json"
            ImplementedSpanFamilyStatus = $spanFamily.status
        }
        EvidenceReferences = $evidence
        LocalValidation = [ordered]@{
            Status = "locally-validated"
            InvariantGuardCount = 105
            RequiredChecks = @("actionlint-1.7.12", "api-abi-baseline", "docfx-2.78.5", "git-diff-check", "repository-powershell-ast", "workflow-bash-syntax", "workflow-powershell-syntax")
            SdkPolicy = ".NET 10 (any installed feature band)"
            PublicationAllowed = $false
        }
        Signing = [ordered]@{
            Status = "repository-signing-pending"
            Strategy = "nuget.org-repository-signing"
            NormalizedInputRequired = $true
            AuthorCertificateRequired = $false
            PrivateKeyRequired = $false
            PrivateKeyMaterialPresent = $false
            ServiceIndex = "https://api.nuget.org/v3/index.json"
            ExpectedSignatureType = "Repository"
            ExpectedOwner = "GuojinYan"
            VerificationScript = "scripts/Test-NuGetRepositorySignedPackage.ps1"
            Verification = "post-publication-required"
        }
        Sbom = [ordered]@{
            Status = "not-ready"
            Format = "SPDX-2.3"
            Generator = "scripts/New-ReleasePackageSbom.ps1"
            Guard = "scripts/Test-ReleasePackageSbom.ps1"
            Deterministic = $true
            FinalCandidateDocumentGenerated = $false
            Verification = "generator-verified-final-candidate-not-generated"
        }
        Approval = [ordered]@{
            Status = "not-approved"
            Reviewer = "automated-local-preflight"
            Approver = "unassigned"
            EvidenceKind = "local-source-and-offline-fixture"
            RemoteMutationAllowed = $false
        }
        PublicFeed = [ordered]@{
            Mode = "read-only"
            ServiceIndex = "https://api.nuget.org/v3/index.json"
            GitHubPackagesServiceIndex = "https://nuget.pkg.github.com/guojin-yan/index.json"
            GitHubPackagesRepository = "guojin-yan/OpenCV-CSharp-API"
            RequiredPublicVisibility = "public"
            RequiredFeedCount = 2
            CandidatePackage = "https://api.nuget.org/v3-flatcontainer/jyppx.opencv.csharp.api/5.0.0/jyppx.opencv.csharp.api.5.0.0.nupkg"
            Methods = @("GET", "HEAD")
            Mutable = $false
            CandidateStatus = "not-published"
            UploadAttempted = $false
        }
        Rollback = [ordered]@{
            Status = "not-published"
            PriorKnownGood = ""
            PackageRemovalRequired = $false
        }
        ReleaseApproval = [ordered]@{
            Status = "not-approved"
            ReleaseReady = $false
            PublicationDecision = "do-not-publish"
        }
        ExternalBlockers = @(Get-OrdinalSortedObjects -Values $blockers -Property "Id")
        PrivateKeyMaterialPresent = $false
        SecretMaterialPresent = $false
        Deterministic = $true
    }
}

$record = Get-Record
$json = Normalize-Text ($record | ConvertTo-Json -Depth 30)
if ($Check) {
    if (-not (Test-Path -LiteralPath $OutputPath -PathType Leaf)) {
        throw "Final release closeout record is missing: $OutputPath"
    }
    $current = Normalize-Text ([IO.File]::ReadAllText($OutputPath))
    if ($current -cne $json) {
        throw "Final release closeout record is out of date: $OutputPath"
    }
}
else {
    [IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($OutputPath)) | Out-Null
    [IO.File]::WriteAllText($OutputPath, $json, [Text.UTF8Encoding]::new($false))
}

Write-Host "RELEASE_CANDIDATE_CLOSEOUT_RECORD_OK candidate=$($record.CandidateId) files=$($record.SourceSet.FileCount) source_sha256=$($record.SourceSet.Sha256) mode=$(if($Check){'check'}else{'write'})"
