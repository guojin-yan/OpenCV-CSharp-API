param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
& (Join-Path $PSScriptRoot 'Generate-DnnObjDetectUpstreamMap.ps1') -RepositoryRoot $repo -Check

$summary = Get-Content (Join-Path $repo 'compatibility/dnn_objdetect-upstream-summary.json') -Raw | ConvertFrom-Json
$raw = Get-Content (Join-Path $repo 'compatibility/dnn_objdetect-upstream-raw.json') -Raw | ConvertFrom-Json
$map = Get-Content (Join-Path $repo 'compatibility/dnn_objdetect-upstream-classifications.json') -Raw | ConvertFrom-Json
$header = 'opencv-source/opencv_contrib-5.0.0/modules/dnn_objdetect/include/opencv2/core_detect.hpp'

if ([string]$raw.headerPath -cne $header -or @($raw.sourceHeaders).Count -ne 1 -or [int]$raw.sourceHeaders[0].startOrdinal -ne 0 -or [string]$raw.parserMode -cne 'non-wrapper-public-declarations' -or [string]$raw.parserNormalization -notmatch 'protected -> private') { throw 'DnnObjDetect parser source closure or parser normalization drifted.' }
if ([int]$raw.declarationCount -ne 4 -or [int]$summary.declarationCount -ne 4 -or [int]$summary.classCount -ne 2 -or [int]$summary.callableCount -ne 2 -or [int]$summary.classificationCounts.implemented -ne 0 -or [int]$summary.classificationCounts.missing -ne 0 -or [int]$summary.nativeEvidenceCount -ne 0 -or [int]$summary.managedEvidenceCount -ne 0 -or [int]$summary.externalDataDependencyCount -ne 1) { throw 'DnnObjDetect declaration partition drifted.' }
if (@($map.declarations).Count -ne 4 -or @($raw.declarations | Where-Object { $_.kind -eq 'callable' -and $_.name -notin @('cv.dnn_objdetect.InferBbox.InferBbox','cv.dnn_objdetect.InferBbox.filter') }).Count -ne 0 -or @($raw.declarations | Where-Object { $_.kind -eq 'callable' } | Where-Object { $_.name -match 'transform_bboxes|final_probability|assert_predictions|filter_top_n|nms_wrapper|non_maximal|intersection_over_union|comparator' }).Count -ne 0) { throw 'DnnObjDetect public callable inventory drifted or leaked protected helpers.' }
if (@($raw.declarations | Where-Object { $_.kind -eq 'class' -and $_.name -eq 'cv.dnn_objdetect.object' } | ForEach-Object { $_.arguments.name } | Sort-Object -Unique) -join ',' -cne 'class_idx,class_prob,label_name,xmax,xmin,ymax,ymin' -or @($raw.declarations | Where-Object { $_.kind -eq 'class' -and $_.name -eq 'cv.dnn_objdetect.InferBbox' } | ForEach-Object { $_.arguments.name }) -join ',' -cne 'detections') { throw 'DnnObjDetect public data-member inventory drifted.' }
if (@($map.declarations | Where-Object { $_.classification -eq 'intentionally-omitted' -and [string]$_.reason -match 'model-driven DNN post-processing helper' }).Count -ne [int]$summary.callableCount) { throw 'DnnObjDetect omission classification closure drifted.' }
if ([bool]$summary.repositoryWideUpstreamParityClaimed) { throw 'DnnObjDetect map must not claim repository-wide parity.' }

Write-Host "DNN_OBJDETECT_UPSTREAM_MAP_CONTRACT_OK declarations=$($summary.declarationCount) callables=$($summary.callableCount) public_fields=8 protected_helpers=excluded implemented=$($summary.classificationCounts.implemented) omitted=$($summary.classificationCounts.'intentionally-omitted') missing=$($summary.classificationCounts.missing) metadata=$($summary.classificationCounts.'non-callable-metadata') native=$($summary.nativeEvidenceCount) managed=$($summary.managedEvidenceCount) model_dependency=true sha256=$($summary.mappingSha256)"
