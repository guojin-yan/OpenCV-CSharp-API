param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
& (Join-Path $PSScriptRoot 'Generate-SuperresUpstreamMap.ps1') -RepositoryRoot $repo -Check

$summary = Get-Content (Join-Path $repo 'compatibility/superres-upstream-summary.json') -Raw | ConvertFrom-Json
$raw = Get-Content (Join-Path $repo 'compatibility/superres-upstream-raw.json') -Raw | ConvertFrom-Json
$map = Get-Content (Join-Path $repo 'compatibility/superres-upstream-classifications.json') -Raw | ConvertFrom-Json
$manifest = @(Get-Content (Join-Path $repo 'src/OpenCvSharp.Native/generated/native_abi_manifest.txt') | ForEach-Object { ($_ -split '\|')[0] })
$managed = Get-Content (Join-Path $repo 'compatibility/managed-public-api.txt') -Raw
$umbrella = 'opencv-source/opencv_contrib-5.0.0/modules/superres/include/opencv2/superres.hpp'
$opticalFlow = 'opencv-source/opencv_contrib-5.0.0/modules/superres/include/opencv2/superres/optical_flow.hpp'

if ([string]$raw.headerPath -cne $umbrella -or [string]$raw.parserMode -cne 'non-wrapper-public-declarations' -or [string]$raw.parserNormalization -notmatch 'CV_EXPORTS without CV_WRAP' -or [string]$raw.parserNormalization -notmatch 'three protected SuperResolution constructor/initImpl/processImpl') { throw 'Superres parser mode/normalization drifted.' }
if (@($raw.sourceHeaders).Count -ne 2 -or [string]$raw.sourceHeaders[0].path -cne $umbrella -or [int]$raw.sourceHeaders[0].startOrdinal -ne 0 -or [int]$raw.sourceHeaders[0].declarationCount -ne 35 -or [string]$raw.sourceHeaders[1].path -cne $opticalFlow -or [int]$raw.sourceHeaders[1].startOrdinal -ne 35 -or [int]$raw.sourceHeaders[1].declarationCount -ne 61) { throw 'Superres parser source closure drifted.' }
if ([int]$raw.declarationCount -ne 96 -or [int]$summary.declarationCount -ne 96 -or [int]$summary.callableCount -ne 89 -or [int]$summary.classCount -ne 7 -or [int]$summary.enumCount -ne 0 -or [int]$summary.classificationCounts.implemented -ne 0 -or [int]$summary.classificationCounts.'intentionally-omitted' -ne 89 -or [int]$summary.classificationCounts.'non-callable-metadata' -ne 7 -or [int]$summary.classificationCounts.missing -ne 0) { throw 'Superres declaration partition drifted.' }
if (@($raw.declarations | Where-Object { [string]$_.identity -in @('cv.superres.SuperResolution.SuperResolution','cv.superres.SuperResolution.initImpl','cv.superres.SuperResolution.processImpl') }).Count -ne 0) { throw 'Superres protected implementation hooks leaked into public scope.' }
if (@($map.declarations).Count -ne 96 -or @($map.declarations | Where-Object { $_.classification -eq 'intentionally-omitted' }).Count -ne 89 -or @($map.declarations | Where-Object { $_.classification -eq 'missing' }).Count -ne 0) { throw 'Superres classification closure drifted.' }
if (@($map.declarations | Where-Object { $_.classification -eq 'intentionally-omitted' -and (@($_.nativeEntrypoints).Count -ne 0 -or @($_.managedMembers).Count -ne 0 -or [string]$_.reason -notmatch 'optional VideoIO/camera and CUDA' -or [string]$_.reason -notmatch 'input/output frame ownership' -or [string]$_.reason -notmatch 'no native Superres wrapper entrypoint') }).Count -ne 0) { throw 'Superres omissions must retain the reviewed wrapper/runtime boundary.' }
if ([int]$summary.nativeEvidenceCount -ne 0 -or [int]$summary.managedEvidenceCount -ne 0 -or @($manifest | Where-Object { $_ -match '^jyppx_ocv_superres_' }).Count -ne 0 -or $managed -match 'JYPPX\.OpenCvSharp\.Superres') { throw 'Superres map falsely claims a native or managed wrapper surface.' }
if (@($map.externalDataDependencies | Where-Object { [string]$_ -match 'optional' -and [string]$_ -match 'VideoIO' -and [string]$_ -match 'CUDA' -and [string]$_ -match 'no managed/native Superres wrapper' }).Count -ne 1) { throw 'Superres optional dependency boundary drifted.' }
if ([bool]$summary.repositoryWideUpstreamParityClaimed) { throw 'Superres map must not claim repository-wide parity.' }

Write-Host "SUPERRES_UPSTREAM_MAP_CONTRACT_OK declarations=$($summary.declarationCount) callable=$($summary.callableCount) implemented=0 omitted=$($summary.classificationCounts.'intentionally-omitted') missing=0 metadata=$($summary.classificationCounts.'non-callable-metadata') native=0 managed=0 parser_mode=non-wrapper-public-declarations sha256=$($summary.mappingSha256)"
