param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
& (Join-Path $PSScriptRoot 'Generate-CudaOptFlowUpstreamMap.ps1') -RepositoryRoot $repo -Check

$summary = Get-Content (Join-Path $repo 'compatibility/cudaoptflow-upstream-summary.json') -Raw | ConvertFrom-Json
$raw = Get-Content (Join-Path $repo 'compatibility/cudaoptflow-upstream-raw.json') -Raw | ConvertFrom-Json
$map = Get-Content (Join-Path $repo 'compatibility/cudaoptflow-upstream-classifications.json') -Raw | ConvertFrom-Json
$manifest = Get-Content (Join-Path $repo 'src/OpenCvSharp.Native/generated/native_abi_manifest.txt') | ForEach-Object { ($_ -split '\|')[0] }
$managed = Get-Content (Join-Path $repo 'compatibility/managed-public-api.txt') -Raw
$header = 'opencv-source/opencv_contrib-5.0.0/modules/cudaoptflow/include/opencv2/cudaoptflow.hpp'
$buildFile = 'opencv-source/opencv_contrib-5.0.0/modules/cudaoptflow/CMakeLists.txt'
$cmakePath = Join-Path $repo '..\opencv-source\opencv_contrib-5.0.0\modules\cudaoptflow\CMakeLists.txt'
$cmake = Get-Content $cmakePath -Raw
$expectedCondition = 'NOT_IOS_AND_NOT_WINRT_AND_(HAVE_CUDA_OR_BUILD_CUDA_STUBS)'

if ([string]$raw.headerPath -cne $header -or @($raw.sourceHeaders).Count -ne 1 -or [string]$raw.sourceHeaders[0].path -cne $header) { throw 'CUDA optical-flow parser source closure drifted.' }
if (@($raw.compatibilityHeaders).Count -ne 1 -or [string]$raw.compatibilityHeaders[0].path -cne $buildFile -or [string]$raw.compatibilityHeaders[0].sha256 -cne (Get-FileHash $cmakePath -Algorithm SHA256).Hash.ToLowerInvariant() -or [string]$raw.moduleBuildCondition -ne $expectedCondition) { throw 'CUDA optical-flow CMake build evidence drifted.' }
if ($cmake -notmatch '(?s)if\(IOS OR WINRT OR \(NOT HAVE_CUDA AND NOT BUILD_CUDA_STUBS\)\).*?ocv_module_disable\(cudaoptflow\)' -or $cmake -notmatch 'ocv_define_module\(cudaoptflow\s+opencv_video\s+opencv_optflow\s+opencv_cudaarithm\s+opencv_cudawarping\s+opencv_cudaimgproc\s+opencv_cudafeatures2d\s+OPTIONAL opencv_cudalegacy\s+WRAP python\)' -or $cmake -notmatch 'CUDA::cudart' -or $cmake -notmatch 'NVIDIA_OPTICAL_FLOW_2_0_HEADERS|NVIDIAOpticalFlowSDK') { throw 'CUDA optical-flow upstream build prerequisites drifted.' }
if ([int]$summary.externalDataDependencyCount -ne 2) { throw 'CUDA optical-flow external dependency partition drifted.' }
if ([int]$raw.declarationCount -ne 93 -or [int]$summary.declarationCount -ne 93 -or [int]$summary.callableCount -ne 79 -or [int]$summary.classCount -ne 10 -or [int]$summary.enumCount -ne 4 -or [int]$summary.classificationCounts.implemented -ne 0 -or [int]$summary.classificationCounts.'intentionally-omitted' -ne 79 -or [int]$summary.classificationCounts.'non-callable-metadata' -ne 14 -or [int]$summary.classificationCounts.missing -ne 0) { throw 'CUDA optical-flow declaration partition drifted.' }
if ((@($raw.declarations | Where-Object { $_.kind -eq 'class' } | ForEach-Object identity | Sort-Object) -join '|') -cne 'class cv.cuda.BroxOpticalFlow|class cv.cuda.DenseOpticalFlow|class cv.cuda.DensePyrLKOpticalFlow|class cv.cuda.FarnebackOpticalFlow|class cv.cuda.NvidiaHWOpticalFlow|class cv.cuda.NvidiaOpticalFlow_1_0|class cv.cuda.NvidiaOpticalFlow_2_0|class cv.cuda.OpticalFlowDual_TVL1|class cv.cuda.SparseOpticalFlow|class cv.cuda.SparsePyrLKOpticalFlow') { throw 'CUDA optical-flow public class metadata drifted.' }
if ((@($raw.declarations | Where-Object { $_.kind -eq 'enum' } | ForEach-Object { ([string]$_.identity -split '\[')[0] } | Sort-Object) -join '|') -cne 'enum cv.cuda.NvidiaOpticalFlow_1_0.NVIDIA_OF_PERF_LEVEL|enum cv.cuda.NvidiaOpticalFlow_2_0.NVIDIA_OF_HINT_VECTOR_GRID_SIZE|enum cv.cuda.NvidiaOpticalFlow_2_0.NVIDIA_OF_OUTPUT_VECTOR_GRID_SIZE|enum cv.cuda.NvidiaOpticalFlow_2_0.NVIDIA_OF_PERF_LEVEL') { throw 'CUDA optical-flow public enum metadata drifted.' }
if (@($raw.declarations | Where-Object { $_.kind -eq 'callable' -and [string]$_.identity -notmatch '^cv\.cuda\.' }).Count -ne 0 -or @($map.declarations | Where-Object { $_.classification -eq 'intentionally-omitted' -and [string]$_.identity -match '^cv\.cuda\.' -and @($_.nativeEntrypoints).Count -eq 0 -and @($_.managedMembers).Count -eq 0 -and [string]$_.reason -match 'no native CudaOptFlow wrapper entrypoint' -and [string]$_.reason -match 'managed public owner' -and [string]$_.buildCondition -ceq $expectedCondition }).Count -ne [int]$summary.callableCount) { throw 'CUDA optical-flow callable classification closure drifted.' }
if ([int]$summary.nativeEvidenceCount -ne 0 -or [int]$summary.managedEvidenceCount -ne 0 -or @($manifest | Where-Object { $_ -match '^jyppx_ocv_cudaoptflow_' }).Count -ne 0 -or $managed -match 'JYPPX\.OpenCvSharp\.CudaOptFlow(?:\.|\|)') { throw 'CUDA optical-flow map falsely claims a native or managed wrapper surface.' }
if ([bool]$summary.repositoryWideUpstreamParityClaimed) { throw 'CUDA optical-flow map must not claim repository-wide parity.' }
Write-Host "CUDAOPTFLOW_UPSTREAM_MAP_CONTRACT_OK declarations=$($summary.declarationCount) callables=$($summary.callableCount) implemented=0 omitted=$($summary.classificationCounts.'intentionally-omitted') missing=0 metadata=$($summary.classificationCounts.'non-callable-metadata') native=0 managed=0 external_dependencies=$($summary.externalDataDependencyCount) build_condition=$expectedCondition sha256=$($summary.mappingSha256)"
