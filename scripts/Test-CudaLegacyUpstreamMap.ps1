param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
& (Join-Path $PSScriptRoot 'Generate-CudaLegacyUpstreamMap.ps1') -RepositoryRoot $repo -Check

$summary = Get-Content (Join-Path $repo 'compatibility/cudalegacy-upstream-summary.json') -Raw | ConvertFrom-Json
$raw = Get-Content (Join-Path $repo 'compatibility/cudalegacy-upstream-raw.json') -Raw | ConvertFrom-Json
$map = Get-Content (Join-Path $repo 'compatibility/cudalegacy-upstream-classifications.json') -Raw | ConvertFrom-Json
$manifest = Get-Content (Join-Path $repo 'src/OpenCvSharp.Native/generated/native_abi_manifest.txt') | ForEach-Object { ($_ -split '\|')[0] }
$managed = Get-Content (Join-Path $repo 'compatibility/managed-public-api.txt') -Raw
$header = 'opencv-source/opencv_contrib-5.0.0/modules/cudalegacy/include/opencv2/cudalegacy.hpp'
$buildFile = 'opencv-source/opencv_contrib-5.0.0/modules/cudalegacy/CMakeLists.txt'
$cmakePath = Join-Path $repo '..\opencv-source\opencv_contrib-5.0.0\modules\cudalegacy\CMakeLists.txt'
$cmake = Get-Content $cmakePath -Raw
$expectedCondition = 'HAVE_CUDA'

if ([string]$raw.headerPath -cne $header -or @($raw.sourceHeaders).Count -ne 7 -or [string]$raw.sourceHeaders[0].path -cne $header) { throw 'CUDA legacy parser source closure drifted.' }
if (@($raw.compatibilityHeaders).Count -ne 1 -or [string]$raw.compatibilityHeaders[0].path -cne $buildFile -or [string]$raw.compatibilityHeaders[0].sha256 -cne (Get-FileHash $cmakePath -Algorithm SHA256).Hash.ToLowerInvariant() -or [string]$raw.moduleBuildCondition -ne $expectedCondition) { throw 'CUDA legacy CMake build evidence drifted.' }
if ($cmake -notmatch '(?s)if\(NOT HAVE_CUDA\).*?ocv_module_disable\(cudalegacy\)' -or $cmake -notmatch 'ocv_add_module\(cudalegacy\s+opencv_core\s+opencv_geometry\s+opencv_video' -or $cmake -notmatch 'CUDA::cudart') { throw 'CUDA legacy upstream build prerequisites drifted.' }
if ([int]$summary.externalDataDependencyCount -ne 2) { throw 'CUDA legacy external dependency partition drifted.' }
if ([int]$raw.declarationCount -ne 221 -or [int]$summary.declarationCount -ne 221 -or [int]$summary.callableCount -ne 191 -or [int]$summary.classCount -ne 25 -or [int]$summary.enumCount -ne 5 -or [int]$summary.classificationCounts.implemented -ne 0 -or [int]$summary.classificationCounts.'intentionally-omitted' -ne 191 -or [int]$summary.classificationCounts.'non-callable-metadata' -ne 30 -or [int]$summary.classificationCounts.missing -ne 0) { throw 'CUDA legacy declaration partition drifted.' }
if (@($raw.declarations | Where-Object { $_.kind -eq 'callable' -and @($_.identity) -contains '' }).Count -ne 0) { throw 'CUDA legacy callable identity closure drifted.' }
if (@($map.declarations | Where-Object { $_.classification -eq 'intentionally-omitted' -and @($_.nativeEntrypoints).Count -eq 0 -and @($_.managedMembers).Count -eq 0 -and [string]$_.reason -match 'no native CudaLegacy wrapper entrypoint' -and [string]$_.reason -match 'managed public owner' -and [string]$_.buildCondition -ceq $expectedCondition }).Count -ne [int]$summary.callableCount) { throw 'CUDA legacy callable classification closure drifted.' }
if ([int]$summary.nativeEvidenceCount -ne 0 -or [int]$summary.managedEvidenceCount -ne 0 -or @($manifest | Where-Object { $_ -match '^jyppx_ocv_cudalegacy_' }).Count -ne 0 -or $managed -match 'JYPPX\.OpenCvSharp\.CudaLegacy(?:\.|\|)') { throw 'CUDA legacy map falsely claims a native or managed wrapper surface.' }
if ([bool]$summary.repositoryWideUpstreamParityClaimed) { throw 'CUDA legacy map must not claim repository-wide parity.' }
Write-Host "CUDALEGACY_UPSTREAM_MAP_CONTRACT_OK declarations=$($summary.declarationCount) callables=$($summary.callableCount) implemented=0 omitted=$($summary.classificationCounts.'intentionally-omitted') missing=0 metadata=$($summary.classificationCounts.'non-callable-metadata') native=0 managed=0 external_dependencies=$($summary.externalDataDependencyCount) build_condition=$expectedCondition sha256=$($summary.mappingSha256)"
