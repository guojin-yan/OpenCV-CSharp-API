param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
& (Join-Path $PSScriptRoot 'Generate-XFeatures2DUpstreamMap.ps1') -RepositoryRoot $repo -Check

$summary = Get-Content (Join-Path $repo 'compatibility/xfeatures2d-upstream-summary.json') -Raw | ConvertFrom-Json
$raw = Get-Content (Join-Path $repo 'compatibility/xfeatures2d-upstream-raw.json') -Raw | ConvertFrom-Json
$map = Get-Content (Join-Path $repo 'compatibility/xfeatures2d-upstream-classifications.json') -Raw | ConvertFrom-Json
$header = 'opencv-source/opencv_contrib-5.0.0/modules/xfeatures2d/include/opencv2/xfeatures2d.hpp'
$expectedHeaders = @(
    'opencv-source/opencv_contrib-5.0.0/modules/xfeatures2d/include/opencv2/xfeatures2d/cuda.hpp',
    'opencv-source/opencv_contrib-5.0.0/modules/xfeatures2d/include/opencv2/xfeatures2d/nonfree.hpp'
)

if ([string]$raw.headerPath -cne $header -or @($raw.sourceHeaders).Count -ne $expectedHeaders.Count) { throw 'XFeatures2D parser source closure drifted.' }
for ($index = 0; $index -lt $expectedHeaders.Count; $index++) {
    if ([string]$raw.sourceHeaders[$index].path -cne $expectedHeaders[$index]) { throw "XFeatures2D parser header order drifted at $index." }
}
if ([int]$raw.declarationCount -ne 21 -or [int]$summary.declarationCount -ne 21 -or [int]$summary.callableCount -ne 18 -or [int]$summary.classificationCounts.implemented -ne 0 -or [int]$summary.classificationCounts.'intentionally-omitted' -ne 18 -or [int]$summary.classificationCounts.'non-callable-metadata' -ne 3 -or [int]$summary.classificationCounts.missing -ne 0) { throw 'XFeatures2D declaration partition drifted.' }
if (@($map.declarations).Count -ne 21 -or @($map.declarations | Where-Object { $_.classification -eq 'intentionally-omitted' -and @($_.nativeEntrypoints).Count -eq 0 -and @($_.managedMembers).Count -eq 0 -and [string]$_.reason -match 'CUDA/nonfree' }).Count -ne 18) { throw 'XFeatures2D omission classification closure drifted.' }
if ([int]$summary.nativeEvidenceCount -ne 0 -or [int]$summary.managedEvidenceCount -ne 0 -or [int]$summary.externalDataDependencyCount -ne 2 -or [bool]$summary.repositoryWideUpstreamParityClaimed) { throw 'XFeatures2D map falsely claims a wrapper surface.' }

Write-Host "XFEATURES2D_UPSTREAM_MAP_CONTRACT_OK declarations=$($summary.declarationCount) callables=$($summary.callableCount) implemented=0 omitted=$($summary.classificationCounts.'intentionally-omitted') missing=0 metadata=$($summary.classificationCounts.'non-callable-metadata') native=0 managed=0 external_dependencies=$($summary.externalDataDependencyCount) sha256=$($summary.mappingSha256)"
