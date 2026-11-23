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
if ($summary.historicalCatalogMapCount -ne 73 -or $summary.refreshedParserSliceCount -ne 55 -or $summary.pendingCatalogMapCount -ne 18 -or
    (@($summary.slices.id) -join '|') -cne 'alphamat|bgsegm|bioinspired|calib3d|ccalib|cnn_3dobj|core|cvv|datasets|dnn|dnn_objdetect|dnn_superres|dpm|face|features|freetype|fuzzy|hdf|hfs|highgui|img_hash|imgcodecs|imgproc|intensity_transform|julia|line_descriptor|ml|objdetect|optflow|phase_unwrapping|photo|plot|ptcloud|quality|rapid|reg|rgbd|saliency|shape|signal|stitching|structured_light|superres|surface_matching|text|tracking|video|videoio|videostab|wechat_qrcode|xfeatures2d|ximgproc|xobjdetect|xphoto|xstereo' -or
    $summary.upstreamContribCommit -cne 'b67a855188a3d683813731010f6d39ac614733a4' -or $summary.repositoryWideUpstreamParityClaimed -or
    $summary.completeCppParityClaimed -or $summary.sharedGeneratorNegativeFixtureCount -ne 1753 -or
    ($summary.slices.declarationCount | Measure-Object -Sum).Sum -ne 4051) { throw '5.1 refreshed parser slice inventory drifted.' }
Write-Host 'UPSTREAM_510_REFRESH_CONTRACT_OK slices=55 pending=18 declarations=4051 shared_generator_fixtures=1753'
