[CmdletBinding()]
param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,
    [string]$DotNetPath = 'dotnet'
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
& (Join-Path $PSScriptRoot 'Generate-UpstreamMap51.ps1') -RepositoryRoot $repo -DotNetPath $DotNetPath -Check
$summary = Get-Content -LiteralPath (Join-Path $repo 'compatibility/opencv-5.1.0/upstream-map-refresh-summary.json') -Raw | ConvertFrom-Json
if ($summary.historicalCatalogMapCount -ne 73 -or $summary.refreshedParserSliceCount -ne 73 -or $summary.pendingCatalogMapCount -ne 0 -or
    (@($summary.slices.id) -join '|') -cne 'alphamat|bgsegm|bioinspired|calib3d|cannops|ccalib|cnn_3dobj|core|cudaarithm|cudabgsegm|cudacodec|cudafeatures2d|cudafilters|cudaimgproc|cudalegacy|cudaobjdetect|cudaoptflow|cudastereo|cudawarping|cudev|cvv|datasets|dnn|dnn_objdetect|dnn_superres|dpm|face|fastcv|features|freetype|fuzzy|hdf|hfs|highgui|img_hash|imgcodecs|imgproc|intensity_transform|julia|line_descriptor|matlab|ml|objdetect|optflow|ovis|phase_unwrapping|photo|plot|ptcloud|quality|rapid|reg|rgbd|saliency|sfm|shape|signal|stitching|structured_light|superres|surface_matching|text|tracking|video|videoio|videostab|viz|wechat_qrcode|xfeatures2d|ximgproc|xobjdetect|xphoto|xstereo' -or
    $summary.upstreamContribCommit -cne 'b67a855188a3d683813731010f6d39ac614733a4' -or $summary.repositoryWideUpstreamParityClaimed -or
    $summary.completeCppParityClaimed -or $summary.sharedGeneratorNegativeFixtureCount -ne 2420 -or
    ($summary.slices.declarationCount | Measure-Object -Sum).Sum -ne 5778) { throw '5.1 refreshed parser slice inventory drifted.' }
& (Join-Path $PSScriptRoot 'Test-CudevUpstreamMap.ps1') -RepositoryRoot $repo -UpstreamVersion '5.1.0'
& (Join-Path $PSScriptRoot 'Test-MatlabUpstreamMap.ps1') -RepositoryRoot $repo -UpstreamVersion '5.1.0'
Write-Host 'UPSTREAM_510_REFRESH_CONTRACT_OK slices=73 pending=0 declarations=5778 shared_generator_fixtures=2420 source_semantic_fixtures=64'
