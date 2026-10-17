param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
& (Join-Path $PSScriptRoot 'Generate-CudaCodecUpstreamMap.ps1') -RepositoryRoot $repo -Check

$summary = Get-Content (Join-Path $repo 'compatibility/cudacodec-upstream-summary.json') -Raw | ConvertFrom-Json
$raw = Get-Content (Join-Path $repo 'compatibility/cudacodec-upstream-raw.json') -Raw | ConvertFrom-Json
$map = Get-Content (Join-Path $repo 'compatibility/cudacodec-upstream-classifications.json') -Raw | ConvertFrom-Json
$manifest = @(Get-Content (Join-Path $repo 'src/OpenCvSharp.Native/generated/native_abi_manifest.txt') | ForEach-Object { ($_ -split '\|')[0] })
$managed = Get-Content (Join-Path $repo 'compatibility/managed-public-api.txt') -Raw
$header = 'opencv-source/opencv_contrib-5.0.0/modules/cudacodec/include/opencv2/cudacodec.hpp'
$buildFile = 'opencv-source/opencv_contrib-5.0.0/modules/cudacodec/CMakeLists.txt'
$cmakePath = Join-Path $repo '..\opencv-source\opencv_contrib-5.0.0\modules\cudacodec\CMakeLists.txt'
$cmake = Get-Content $cmakePath -Raw
$expectedCondition = 'NOT_IOS_AND_NOT_APPLE_AND_NOT_WINRT_AND_(HAVE_CUDA_OR_BUILD_CUDA_STUBS)'

if ([string]$raw.headerPath -cne $header -or @($raw.sourceHeaders).Count -ne 1 -or [string]$raw.sourceHeaders[0].path -cne $header) { throw 'CUDA codec parser source closure drifted.' }
if (@($raw.compatibilityHeaders).Count -ne 1 -or [string]$raw.compatibilityHeaders[0].path -cne $buildFile -or [string]$raw.compatibilityHeaders[0].sha256 -cne (Get-FileHash $cmakePath -Algorithm SHA256).Hash.ToLowerInvariant() -or [string]$raw.moduleBuildCondition -cne $expectedCondition) { throw 'CUDA codec CMake build evidence drifted.' }
if ($cmake -notmatch '(?s)if\(IOS OR APPLE OR WINRT OR \(NOT HAVE_CUDA AND NOT BUILD_CUDA_STUBS\)\).*?ocv_module_disable\(cudacodec\)' -or $cmake -notmatch 'set\(required_dependencies opencv_core opencv_videoio opencv_cudaarithm opencv_cudawarping\)' -or $cmake -notmatch 'ocv_add_module\(cudacodec .*opencv_cudev.*WRAP python\)' -or $cmake -notmatch 'CUDA::cuda_driver.*CUDA::cudart' -or $cmake -notmatch 'nvcuvid' -or $cmake -notmatch 'nvencodeapi|nvidia-encode') { throw 'CUDA codec upstream build prerequisites drifted.' }
if ([int]$raw.declarationCount -ne 45 -or [int]$summary.declarationCount -ne 45 -or [int]$summary.callableCount -ne 23 -or [int]$summary.classCount -ne 9 -or [int]$summary.enumCount -ne 13 -or [int]$summary.classificationCounts.implemented -ne 0 -or [int]$summary.classificationCounts.'intentionally-omitted' -ne 23 -or [int]$summary.classificationCounts.'non-callable-metadata' -ne 22 -or [int]$summary.classificationCounts.missing -ne 0) { throw 'CUDA codec declaration partition drifted.' }
if (@($raw.declarations | Where-Object { $_.kind -eq 'callable' -and [string]$_.identity -notmatch '^cv\.cudacodec\.' }).Count -ne 0 -or @($map.declarations | Where-Object { $_.classification -eq 'intentionally-omitted' -and [string]$_.identity -match '^cv\.cudacodec\.' -and @($_.nativeEntrypoints).Count -eq 0 -and @($_.managedMembers).Count -eq 0 -and [string]$_.reason -match 'no native CudaCodec wrapper entrypoint' -and [string]$_.reason -match 'managed public owner' -and [string]$_.buildCondition -ceq $expectedCondition }).Count -ne 23) { throw 'CUDA codec callable classification closure drifted.' }
if ((@($raw.declarations | Where-Object { $_.kind -eq 'class' } | ForEach-Object identity | Sort-Object) -join '|') -cne 'class cv.cudacodec.EncodeQp|class cv.cudacodec.EncoderCallback|class cv.cudacodec.EncoderParams|class cv.cudacodec.FormatInfo|class cv.cudacodec.NVSurfaceToColorConverter|class cv.cudacodec.RawVideoSource|class cv.cudacodec.VideoReader|class cv.cudacodec.VideoReaderInitParams|class cv.cudacodec.VideoWriter') { throw 'CUDA codec public class metadata drifted.' }
if ([int]$summary.nativeEvidenceCount -ne 0 -or [int]$summary.managedEvidenceCount -ne 0 -or [int]$summary.externalDataDependencyCount -ne 2 -or @($manifest | Where-Object { $_ -match '^jyppx_ocv_cudacodec_' }).Count -ne 0 -or $managed -match 'JYPPX\.OpenCvSharp\.CudaCodec(?:\.|\|)') { throw 'CUDA codec map falsely claims a native or managed wrapper surface.' }
if ([bool]$summary.repositoryWideUpstreamParityClaimed) { throw 'CUDA codec map must not claim repository-wide parity.' }

Write-Host "CUDACODEC_UPSTREAM_MAP_CONTRACT_OK declarations=$($summary.declarationCount) callables=$($summary.callableCount) implemented=0 omitted=$($summary.classificationCounts.'intentionally-omitted') missing=0 metadata=$($summary.classificationCounts.'non-callable-metadata') native=0 managed=0 external_dependencies=2 build_condition=$expectedCondition sha256=$($summary.mappingSha256)"
