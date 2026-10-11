param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')))

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
& (Join-Path $PSScriptRoot 'Generate-DnnSuperresUpstreamMap.ps1') -RepositoryRoot $repo -Check

$summary = Get-Content (Join-Path $repo 'compatibility/dnn_superres-upstream-summary.json') -Raw | ConvertFrom-Json
$raw = Get-Content (Join-Path $repo 'compatibility/dnn_superres-upstream-raw.json') -Raw | ConvertFrom-Json
$map = Get-Content (Join-Path $repo 'compatibility/dnn_superres-upstream-classifications.json') -Raw | ConvertFrom-Json
$manifest = @(Get-Content (Join-Path $repo 'src/OpenCvSharp.Native/generated/native_abi_manifest.txt') | ForEach-Object { ($_ -split '\|')[0] })
$managed = Get-Content (Join-Path $repo 'compatibility/managed-public-api.txt') -Raw
$header = 'opencv-source/opencv_contrib-5.0.0/modules/dnn_superres/include/opencv2/dnn_superres.hpp'

if ([string]$raw.headerPath -cne $header -or @($raw.sourceHeaders).Count -ne 1 -or [int]$raw.sourceHeaders[0].startOrdinal -ne 0 -or [int]$raw.sourceHeaders[0].declarationCount -ne 10) { throw 'DnnSuperres parser source closure drifted.' }
if ([int]$raw.declarationCount -ne 10 -or [int]$summary.declarationCount -ne 10 -or [int]$summary.callableCount -ne 9 -or [int]$summary.classCount -ne 1 -or [int]$summary.enumCount -ne 0 -or [int]$summary.classificationCounts.implemented -ne 0 -or [int]$summary.classificationCounts.'intentionally-omitted' -ne 9 -or [int]$summary.classificationCounts.'non-callable-metadata' -ne 1 -or [int]$summary.classificationCounts.missing -ne 0) { throw 'DnnSuperres declaration partition drifted.' }
if (@($map.declarations).Count -ne 10 -or @($map.declarations | Where-Object { $_.classification -eq 'intentionally-omitted' }).Count -ne 9 -or @($map.declarations | Where-Object { $_.classification -eq 'missing' }).Count -ne 0) { throw 'DnnSuperres classification closure drifted.' }
if (@($map.declarations | Where-Object { $_.classification -eq 'intentionally-omitted' -and (@($_.nativeEntrypoints).Count -ne 0 -or @($_.managedMembers).Count -ne 0 -or [string]$_.reason -notmatch 'model-driven' -or [string]$_.reason -notmatch 'no native DnnSuperres wrapper entrypoint' -or [string]$_.reason -notmatch 'backend/target policy' -or [string]$_.reason -notmatch 'output ownership') }).Count -ne 0) { throw 'DnnSuperres omissions must retain the reviewed model/wrapper boundary.' }
if (@($map.declarations | Where-Object { $_.classification -eq 'non-callable-metadata' -and [string]$_.identity -ceq 'class cv.dnn_superres.DnnSuperResImpl' }).Count -ne 1) { throw 'DnnSuperres class metadata closure drifted.' }
if ([int]$summary.nativeEvidenceCount -ne 0 -or [int]$summary.managedEvidenceCount -ne 0 -or @($manifest | Where-Object { $_ -match '^jyppx_ocv_dnn_superres_' }).Count -ne 0 -or $managed -match 'JYPPX\.OpenCvSharp\.DnnSuperres') { throw 'DnnSuperres map falsely claims a native or managed wrapper surface.' }
if (@($map.externalDataDependencies | Where-Object { [string]$_ -match 'caller-selected compatible model data' -and [string]$_ -match 'backend/target policy' -and [string]$_ -match 'no model asset' }).Count -ne 1) { throw 'DnnSuperres model dependency boundary drifted.' }
if ([bool]$summary.repositoryWideUpstreamParityClaimed) { throw 'DnnSuperres map must not claim repository-wide parity.' }

Write-Host "DNN_SUPERRES_UPSTREAM_MAP_CONTRACT_OK declarations=$($summary.declarationCount) callables=$($summary.callableCount) implemented=0 omitted=$($summary.classificationCounts.'intentionally-omitted') missing=0 metadata=$($summary.classificationCounts.'non-callable-metadata') native=0 managed=0 model_dependency=true sha256=$($summary.mappingSha256)"
