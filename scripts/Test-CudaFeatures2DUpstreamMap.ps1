param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
& (Join-Path $PSScriptRoot 'Generate-CudaFeatures2DUpstreamMap.ps1') -RepositoryRoot $repo -Check

$summary = Get-Content (Join-Path $repo 'compatibility/cudafeatures2d-upstream-summary.json') -Raw | ConvertFrom-Json
$raw = Get-Content (Join-Path $repo 'compatibility/cudafeatures2d-upstream-raw.json') -Raw | ConvertFrom-Json
$map = Get-Content (Join-Path $repo 'compatibility/cudafeatures2d-upstream-classifications.json') -Raw | ConvertFrom-Json
$manifest = Get-Content (Join-Path $repo 'src/OpenCvSharp.Native/generated/native_abi_manifest.txt') | ForEach-Object { ($_ -split '\|')[0] }
$managed = Get-Content (Join-Path $repo 'compatibility/managed-public-api.txt') -Raw
$header = 'opencv-source/opencv_contrib-5.0.0/modules/cudafeatures2d/include/opencv2/cudafeatures2d.hpp'
$buildFile = 'opencv-source/opencv_contrib-5.0.0/modules/cudafeatures2d/CMakeLists.txt'
$cmakePath = Join-Path $repo '..\opencv-source\opencv_contrib-5.0.0\modules\cudafeatures2d\CMakeLists.txt'
$cmake = Get-Content $cmakePath -Raw
$expectedCondition = 'NOT_IOS_AND_NOT_WINRT_AND_(HAVE_CUDA_OR_BUILD_CUDA_STUBS)'

if ([string]$raw.headerPath -cne $header -or @($raw.sourceHeaders).Count -ne 1 -or [string]$raw.sourceHeaders[0].path -cne $header) { throw 'CUDA features2d parser source closure drifted.' }
if (@($raw.compatibilityHeaders).Count -ne 1 -or [string]$raw.compatibilityHeaders[0].path -cne $buildFile -or [string]$raw.compatibilityHeaders[0].sha256 -cne (Get-FileHash $cmakePath -Algorithm SHA256).Hash.ToLowerInvariant() -or [string]$raw.moduleBuildCondition -cne $expectedCondition) { throw 'CUDA features2d CMake build evidence drifted.' }
if ($cmake -notmatch '(?s)if\(IOS OR WINRT OR \(NOT HAVE_CUDA AND NOT BUILD_CUDA_STUBS\)\).*?ocv_module_disable\(cudafeatures2d\)' -or $cmake -notmatch 'ocv_add_module\(cudafeatures2d opencv_features opencv_cudafilters opencv_cudawarping WRAP python\)' -or $cmake -notmatch 'CUDA::cudart') { throw 'CUDA features2d upstream build prerequisites drifted.' }
if ([int]$raw.declarationCount -ne 64 -or [int]$summary.declarationCount -ne 64 -or [int]$summary.callableCount -ne 58 -or [int]$summary.classCount -ne 6 -or [int]$summary.enumCount -ne 0 -or [int]$summary.classificationCounts.implemented -ne 0 -or [int]$summary.classificationCounts.'intentionally-omitted' -ne 58 -or [int]$summary.classificationCounts.'non-callable-metadata' -ne 6 -or [int]$summary.classificationCounts.missing -ne 0) { throw 'CUDA features2d declaration partition drifted.' }
if ((@($raw.declarations | Where-Object { $_.kind -eq 'class' } | ForEach-Object identity | Sort-Object) -join '|') -cne 'class cv.cuda.CornernessCriteria|class cv.cuda.CornersDetector|class cv.cuda.DescriptorMatcher|class cv.cuda.FastFeatureDetector|class cv.cuda.Feature2DAsync|class cv.cuda.ORB') { throw 'CUDA features2d public class metadata drifted.' }
if (@($raw.declarations | Where-Object { $_.kind -eq 'callable' -and [string]$_.identity -notmatch '^cv\.cuda\.' }).Count -ne 0 -or @($map.declarations | Where-Object { $_.classification -eq 'intentionally-omitted' -and [string]$_.identity -match '^cv\.cuda\.' -and @($_.nativeEntrypoints).Count -eq 0 -and @($_.managedMembers).Count -eq 0 -and [string]$_.reason -match 'no native CudaFeatures2D wrapper entrypoint' -and [string]$_.reason -match 'managed public owner' -and [string]$_.buildCondition -ceq $expectedCondition }).Count -ne 58) { throw 'CUDA features2d callable classification closure drifted.' }
if ([int]$summary.nativeEvidenceCount -ne 0 -or [int]$summary.managedEvidenceCount -ne 0 -or [int]$summary.externalDataDependencyCount -ne 2 -or @($manifest | Where-Object { $_ -match '^jyppx_ocv_cudafeatures2d_' }).Count -ne 0 -or $managed -match 'JYPPX\.OpenCvSharp\.CudaFeatures2D(?:\.|\|)') { throw 'CUDA features2d map falsely claims a native or managed wrapper surface.' }
if ([bool]$summary.repositoryWideUpstreamParityClaimed) { throw 'CUDA features2d map must not claim repository-wide parity.' }
Write-Host "CUDAFEATURES2D_UPSTREAM_MAP_CONTRACT_OK declarations=$($summary.declarationCount) callables=$($summary.callableCount) implemented=0 omitted=$($summary.classificationCounts.'intentionally-omitted') missing=0 metadata=$($summary.classificationCounts.'non-callable-metadata') native=0 managed=0 external_dependencies=2 build_condition=$expectedCondition sha256=$($summary.mappingSha256)"
