param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
& (Join-Path $PSScriptRoot 'Generate-DatasetsUpstreamMap.ps1') -RepositoryRoot $repo -Check

$summary = Get-Content (Join-Path $repo 'compatibility/datasets-upstream-summary.json') -Raw | ConvertFrom-Json
$raw = Get-Content (Join-Path $repo 'compatibility/datasets-upstream-raw.json') -Raw | ConvertFrom-Json
$map = Get-Content (Join-Path $repo 'compatibility/datasets-upstream-classifications.json') -Raw | ConvertFrom-Json
$manifest = @(Get-Content (Join-Path $repo 'src/OpenCvSharp.Native/generated/native_abi_manifest.txt') | ForEach-Object { ($_ -split '\|')[0] })
$managed = Get-Content (Join-Path $repo 'compatibility/managed-public-api.txt') -Raw
$primaryPath = Join-Path $repo '..\opencv-source\opencv_contrib-5.0.0\modules\datasets\include\opencv2\datasets\dataset.hpp'
$cmakePath = Join-Path $repo '..\opencv-source\opencv_contrib-5.0.0\modules\datasets\CMakeLists.txt'
$primary = 'opencv-source/opencv_contrib-5.0.0/modules/datasets/include/opencv2/datasets/dataset.hpp'
$buildFile = 'opencv-source/opencv_contrib-5.0.0/modules/datasets/CMakeLists.txt'
$cmake = Get-Content $cmakePath -Raw

if ([string]$raw.headerPath -cne $primary -or [int]$raw.declarationCount -ne 163 -or @($raw.sourceHeaders).Count -ne 32 -or [string]$raw.moduleBuildCondition -cne 'OPENCV_CORE_IMGCODECS_ML_FLANN_OPTIONAL_TEXT') { throw 'Datasets parser source closure drifted.' }
if (@($raw.compatibilityHeaders).Count -ne 1 -or [string]$raw.compatibilityHeaders[0].path -cne $buildFile -or [string]$raw.compatibilityHeaders[0].sha256 -cne (Get-FileHash $cmakePath -Algorithm SHA256).Hash.ToLowerInvariant()) { throw 'Datasets build dependency evidence drifted.' }
if ($cmake -notmatch '(?s)ocv_define_module\(datasets opencv_core opencv_imgcodecs opencv_ml opencv_flann OPTIONAL opencv_text WRAP python\)') { throw 'Datasets upstream CMake dependency boundary drifted.' }
if ([string]$raw.headerSha256 -cne (Get-FileHash $primaryPath -Algorithm SHA256).Hash.ToLowerInvariant()) { throw 'Datasets primary header hash drifted.' }
if ([int]$summary.declarationCount -ne 163 -or [int]$summary.callableCount -ne 83 -or [int]$summary.classCount -ne 72 -or [int]$summary.enumCount -ne 8 -or [int]$summary.classificationCounts.implemented -ne 0 -or [int]$summary.classificationCounts.'intentionally-omitted' -ne 83 -or [int]$summary.classificationCounts.'non-callable-metadata' -ne 80 -or [int]$summary.classificationCounts.missing -ne 0) { throw 'Datasets declaration partition drifted.' }
if (@($map.declarations).Count -ne 163 -or @($map.declarations | Where-Object { $_.classification -eq 'intentionally-omitted' -and (@($_.nativeEntrypoints).Count -ne 0 -or @($_.managedMembers).Count -ne 0 -or [string]$_.reason -notmatch 'no native or managed wrapper' -or [string]$_.reason -notmatch 'caller-provided dataset' -or [string]$_.reason -notmatch 'file paths') }).Count -ne 0 -or @($map.declarations | Where-Object classification -eq 'non-callable-metadata').Count -ne 80 -or @($map.declarations | Where-Object classification -eq 'missing').Count -ne 0) { throw 'Datasets callable classification closure drifted.' }
if ([int]$summary.nativeEvidenceCount -ne 0 -or [int]$summary.managedEvidenceCount -ne 0 -or @($manifest | Where-Object { $_ -match '^jyppx_ocv_datasets_' }).Count -ne 0 -or $managed -match 'JYPPX\.OpenCvSharp\.Datasets(?:\.|\|)') { throw 'Datasets map falsely claims a native or managed wrapper surface.' }
if (@($map.externalDataDependencies | Where-Object { [string]$_ -match 'caller-provided dataset' -and [string]$_ -match 'file paths' }).Count -ne 1) { throw 'Datasets external data boundary drifted.' }
if (@($map.declarations | Where-Object { $_.identity -match '^cv\.datasets\.Dataset\.(?:load|getTrain|getTest|getValidation)\(' }).Count -ne 4 -or @($map.declarations | Where-Object { $_.identity -match '^cv\.datasets\.split\(' }).Count -ne 1 -or @($map.declarations | Where-Object { $_.identity -match '^cv\.datasets\.createDirectory\(' }).Count -ne 1) { throw 'Datasets parser callable coverage drifted.' }
if ([bool]$summary.repositoryWideUpstreamParityClaimed) { throw 'Datasets map must not claim repository-wide parity.' }

Write-Host "DATASETS_UPSTREAM_MAP_CONTRACT_OK declarations=$($summary.declarationCount) callable=$($summary.callableCount) implemented=0 omitted=$($summary.classificationCounts.'intentionally-omitted') missing=0 metadata=$($summary.classificationCounts.'non-callable-metadata') native=0 managed=0 externalDependencies=$($summary.externalDataDependencyCount) sha256=$($summary.mappingSha256)"
