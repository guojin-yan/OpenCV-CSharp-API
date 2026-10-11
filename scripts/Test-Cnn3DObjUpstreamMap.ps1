param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
& (Join-Path $PSScriptRoot 'Generate-Cnn3DObjUpstreamMap.ps1') -RepositoryRoot $repo -Check

$summary = Get-Content (Join-Path $repo 'compatibility/cnn_3dobj-upstream-summary.json') -Raw | ConvertFrom-Json
$raw = Get-Content (Join-Path $repo 'compatibility/cnn_3dobj-upstream-raw.json') -Raw | ConvertFrom-Json
$map = Get-Content (Join-Path $repo 'compatibility/cnn_3dobj-upstream-classifications.json') -Raw | ConvertFrom-Json
$manifest = @(Get-Content (Join-Path $repo 'src/OpenCvSharp.Native/generated/native_abi_manifest.txt') | ForEach-Object { ($_ -split '\|')[0] })
$managed = Get-Content (Join-Path $repo 'compatibility/managed-public-api.txt') -Raw
$header = 'opencv-source/opencv_contrib-5.0.0/modules/cnn_3dobj/include/opencv2/cnn_3dobj.hpp'

if ([string]$raw.headerPath -cne $header -or @($raw.sourceHeaders).Count -ne 1 -or [int]$raw.sourceHeaders[0].startOrdinal -ne 0 -or [int]$raw.sourceHeaders[0].declarationCount -ne 7) { throw 'Cnn3DObj parser source closure drifted.' }
if ([int]$raw.declarationCount -ne 7 -or [int]$summary.declarationCount -ne 7 -or [int]$summary.callableCount -ne 5 -or [int]$summary.classCount -ne 2 -or [int]$summary.enumCount -ne 0 -or [int]$summary.classificationCounts.implemented -ne 0 -or [int]$summary.classificationCounts.'intentionally-omitted' -ne 5 -or [int]$summary.classificationCounts.'non-callable-metadata' -ne 2 -or [int]$summary.classificationCounts.missing -ne 0) { throw 'Cnn3DObj declaration partition drifted.' }
if (@($map.declarations).Count -ne 7 -or @($map.declarations | Where-Object { $_.classification -eq 'intentionally-omitted' }).Count -ne 5 -or @($map.declarations | Where-Object { $_.classification -eq 'missing' }).Count -ne 0) { throw 'Cnn3DObj classification closure drifted.' }
if (@($map.declarations | Where-Object { $_.classification -eq 'intentionally-omitted' -and (@($_.nativeEntrypoints).Count -ne 0 -or @($_.managedMembers).Count -ne 0 -or [string]$_.reason -notmatch 'Caffe' -or [string]$_.reason -notmatch 'model' -or [string]$_.reason -notmatch 'no native Cnn3DObj wrapper entrypoint') }).Count -ne 0) { throw 'Cnn3DObj omissions must retain the Caffe/model/wrapper boundary.' }
if (@($map.declarations | Where-Object { $_.classification -eq 'non-callable-metadata' -and [string]$_.identity -in @('class cv.cnn_3dobj.icoSphere','class cv.cnn_3dobj.descriptorExtractor') }).Count -ne 2) { throw 'Cnn3DObj class metadata closure drifted.' }
if ([int]$summary.nativeEvidenceCount -ne 0 -or [int]$summary.managedEvidenceCount -ne 0 -or @($manifest | Where-Object { $_ -match '^jyppx_ocv_cnn_3dobj_' }).Count -ne 0 -or $managed -match 'JYPPX\.OpenCvSharp\.Cnn3DObj') { throw 'Cnn3DObj map falsely claims a native or managed wrapper surface.' }
if (@($map.externalDataDependencies | Where-Object { [string]$_ -match 'Caffe' -and [string]$_ -match 'triplet' -and [string]$_ -match 'model assets' -and [string]$_ -match 'managed/native wrapper' }).Count -ne 1) { throw 'Cnn3DObj external Caffe/model dependency boundary drifted.' }
if ([bool]$summary.repositoryWideUpstreamParityClaimed) { throw 'Cnn3DObj map must not claim repository-wide parity.' }

Write-Host "CNN_3DOBJ_UPSTREAM_MAP_CONTRACT_OK declarations=$($summary.declarationCount) callables=$($summary.callableCount) implemented=0 omitted=$($summary.classificationCounts.'intentionally-omitted') missing=0 metadata=$($summary.classificationCounts.'non-callable-metadata') native=0 managed=0 external_caffe=true sha256=$($summary.mappingSha256)"
