param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
& (Join-Path $PSScriptRoot 'Generate-TextUpstreamMap.ps1') -RepositoryRoot $repo -Check

$summary = Get-Content (Join-Path $repo 'compatibility/text-upstream-summary.json') -Raw | ConvertFrom-Json
$raw = Get-Content (Join-Path $repo 'compatibility/text-upstream-raw.json') -Raw | ConvertFrom-Json
$map = Get-Content (Join-Path $repo 'compatibility/text-upstream-classifications.json') -Raw | ConvertFrom-Json
$header = 'opencv-source/opencv_contrib-5.0.0/modules/text/include/opencv2/text.hpp'
$expectedHeaders = @(
    'opencv-source/opencv_contrib-5.0.0/modules/text/include/opencv2/text/erfilter.hpp',
    'opencv-source/opencv_contrib-5.0.0/modules/text/include/opencv2/text/ocr.hpp',
    'opencv-source/opencv_contrib-5.0.0/modules/text/include/opencv2/text/swt_text_detection.hpp',
    'opencv-source/opencv_contrib-5.0.0/modules/text/include/opencv2/text/textDetector.hpp'
)

if ([string]$raw.headerPath -cne $header -or @($raw.sourceHeaders).Count -ne $expectedHeaders.Count) { throw 'Text parser source closure drifted.' }
for ($index = 0; $index -lt $expectedHeaders.Count; $index++) {
    if ([string]$raw.sourceHeaders[$index].path -cne $expectedHeaders[$index]) { throw "Text parser header order drifted at $index." }
}
if ([int]$raw.declarationCount -ne 47 -or [int]$summary.declarationCount -ne 47 -or [int]$summary.callableCount -ne 30 -or [int]$summary.classCount -ne 10 -or [int]$summary.enumCount -ne 7 -or [int]$summary.classificationCounts.implemented -ne 0 -or [int]$summary.classificationCounts.'intentionally-omitted' -ne 30 -or [int]$summary.classificationCounts.'non-callable-metadata' -ne 17 -or [int]$summary.classificationCounts.missing -ne 0) { throw 'Text declaration partition drifted.' }
if (@($map.declarations).Count -ne 47 -or @($map.declarations | Where-Object { $_.classification -eq 'intentionally-omitted' -and @($_.nativeEntrypoints).Count -eq 0 -and @($_.managedMembers).Count -eq 0 -and [string]$_.reason -match 'Tesseract' }).Count -ne 30) { throw 'Text omission classification closure drifted.' }
if ([int]$summary.nativeEvidenceCount -ne 0 -or [int]$summary.managedEvidenceCount -ne 0 -or [int]$summary.externalDataDependencyCount -ne 2 -or [bool]$summary.repositoryWideUpstreamParityClaimed) { throw 'Text map falsely claims a wrapper surface.' }

Write-Host "TEXT_UPSTREAM_MAP_CONTRACT_OK declarations=$($summary.declarationCount) callables=$($summary.callableCount) implemented=0 omitted=$($summary.classificationCounts.'intentionally-omitted') missing=0 metadata=$($summary.classificationCounts.'non-callable-metadata') native=0 managed=0 external_dependencies=$($summary.externalDataDependencyCount) sha256=$($summary.mappingSha256)"
