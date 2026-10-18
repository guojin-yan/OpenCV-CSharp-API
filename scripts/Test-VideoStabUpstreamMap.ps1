param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
& (Join-Path $PSScriptRoot 'Generate-VideoStabUpstreamMap.ps1') -RepositoryRoot $repo -Check

$summary = Get-Content (Join-Path $repo 'compatibility/videostab-upstream-summary.json') -Raw | ConvertFrom-Json
$raw = Get-Content (Join-Path $repo 'compatibility/videostab-upstream-raw.json') -Raw | ConvertFrom-Json
$map = Get-Content (Join-Path $repo 'compatibility/videostab-upstream-classifications.json') -Raw | ConvertFrom-Json
$manifest = @(Get-Content (Join-Path $repo 'src/OpenCvSharp.Native/generated/native_abi_manifest.txt') | ForEach-Object { ($_ -split '\|')[0] })
$managed = Get-Content (Join-Path $repo 'compatibility/managed-public-api.txt') -Raw
$umbrella = 'opencv-source/opencv_contrib-5.0.0/modules/videostab/include/opencv2/videostab.hpp'
$buildFile = 'opencv-source/opencv_contrib-5.0.0/modules/videostab/CMakeLists.txt'
$cmakePath = Join-Path $repo '..\opencv-source\opencv_contrib-5.0.0\modules\videostab\CMakeLists.txt'
$cmake = Get-Content $cmakePath -Raw
$expectedBuildCondition = 'ocv_define_module(videostab opencv_imgproc opencv_features opencv_video opencv_photo opencv_geometry; OPTIONAL opencv_cudafeatures2d opencv_cudawarping opencv_cudaoptflow opencv_videoio)'

if ([string]$raw.headerPath -cne $umbrella -or [string]$raw.parserMode -cne 'non-wrapper-public-declarations' -or [string]$raw.parserNormalization -notmatch 'protected sections changed to private' -or @($raw.sourceHeaders).Count -ne 14) { throw 'VideoStab parser source scope/mode drifted.' }
if (@($raw.compatibilityHeaders).Count -ne 1 -or [string]$raw.compatibilityHeaders[0].path -cne $buildFile -or [string]$raw.compatibilityHeaders[0].sha256 -cne (Get-FileHash $cmakePath -Algorithm SHA256).Hash.ToLowerInvariant() -or [string]$raw.moduleBuildCondition -cne $expectedBuildCondition) { throw 'VideoStab CMake dependency evidence drifted.' }
if ($cmake -notmatch '(?s)ocv_define_module\(videostab\s+opencv_imgproc\s+opencv_features\s+opencv_video\s+opencv_photo\s+opencv_geometry\s+OPTIONAL\s+opencv_cudafeatures2d\s+opencv_cudawarping\s+opencv_cudaoptflow\s+opencv_videoio') { throw 'VideoStab upstream required/optional module dependencies drifted.' }
$protectedExclusions = @(
    'cv.videostab.MotionEstimatorBase.MotionEstimatorBase(MotionModel model)->',
    'cv.videostab.ImageMotionEstimatorBase.ImageMotionEstimatorBase(MotionModel model)->',
    'cv.videostab.StabilizerBase.StabilizerBase()->',
    'cv.videostab.StabilizerBase.reset()->void',
    'cv.videostab.StabilizerBase.nextStabilizedFrame()->Mat',
    'cv.videostab.StabilizerBase.doOneIteration()->bool',
    'cv.videostab.StabilizerBase.setUp(const Mat & firstFrame)->void',
    'cv.videostab.StabilizerBase.estimateMotion()->Mat',
    'cv.videostab.StabilizerBase.estimateStabilizationMotion()->Mat',
    'cv.videostab.StabilizerBase.stabilizeFrame()->void',
    'cv.videostab.StabilizerBase.postProcessFrame(const Mat & frame)->Mat',
    'cv.videostab.StabilizerBase.logProcessingTime()->void',
    'cv.videostab.OnePassStabilizer.setUp(const Mat & firstFrame)->void',
    'cv.videostab.OnePassStabilizer.estimateMotion()->Mat',
    'cv.videostab.OnePassStabilizer.estimateStabilizationMotion()->Mat',
    'cv.videostab.OnePassStabilizer.postProcessFrame(const Mat & frame)->Mat',
    'cv.videostab.TwoPassStabilizer.runPrePassIfNecessary()->void',
    'cv.videostab.TwoPassStabilizer.setUp(const Mat & firstFrame)->void',
    'cv.videostab.TwoPassStabilizer.estimateMotion()->Mat',
    'cv.videostab.TwoPassStabilizer.estimateStabilizationMotion()->Mat',
    'cv.videostab.TwoPassStabilizer.postProcessFrame(const Mat & frame)->Mat',
    'cv.videostab.MoreAccurateMotionWobbleSuppressorBase.MoreAccurateMotionWobbleSuppressorBase()->'
)
if ($protectedExclusions.Count -ne 22 -or @($raw.declarations | Where-Object { $protectedExclusions -ccontains [string]$_.identity }).Count -ne 0) { throw 'VideoStab protected implementation-hook exclusion boundary drifted.' }
if ([int]$raw.declarationCount -ne 278 -or [int]$summary.declarationCount -ne 278 -or [int]$summary.callableCount -ne 232 -or [int]$summary.classCount -ne 45 -or [int]$summary.enumCount -ne 1 -or [int]$summary.classificationCounts.implemented -ne 0 -or [int]$summary.classificationCounts.'intentionally-omitted' -ne 232 -or [int]$summary.classificationCounts.'non-callable-metadata' -ne 46 -or [int]$summary.classificationCounts.missing -ne 0) { throw 'VideoStab parser declaration partition drifted.' }
if (@($map.declarations).Count -ne 278 -or @($map.declarations | Where-Object { $_.classification -eq 'intentionally-omitted' -and (@($_.nativeEntrypoints).Count -ne 0 -or @($_.managedMembers).Count -ne 0 -or [string]$_.reason -notmatch 'no native VideoStab wrapper entrypoint' -or [string]$_.reason -notmatch 'callback, state, and Mat ownership') }).Count -ne 232 -or @($map.declarations | Where-Object classification -eq 'missing').Count -ne 0) { throw 'VideoStab callable classification closure drifted.' }
if ([int]$summary.nativeEvidenceCount -ne 0 -or [int]$summary.managedEvidenceCount -ne 0 -or @($manifest | Where-Object { $_ -match '^jyppx_ocv_videostab_' }).Count -ne 0 -or $managed -match 'JYPPX\.OpenCvSharp\.VideoStab(?:\.|\|)') { throw 'VideoStab map falsely claims a native or managed wrapper surface.' }
if (@($map.externalDataDependencies | Where-Object { [string]$_ -match 'caller-selected frame' -and [string]$_ -match 'optional build-time integrations' }).Count -ne 1) { throw 'VideoStab caller-source/optional-backend boundary drifted.' }
if ([bool]$summary.repositoryWideUpstreamParityClaimed) { throw 'VideoStab map must not claim repository-wide parity.' }

Write-Host "VIDEOSTAB_UPSTREAM_MAP_CONTRACT_OK declarations=$($summary.declarationCount) callable=$($summary.callableCount) implemented=0 omitted=$($summary.classificationCounts.'intentionally-omitted') missing=0 metadata=$($summary.classificationCounts.'non-callable-metadata') protected_excluded=$($protectedExclusions.Count) native=0 managed=0 parser_mode=non-wrapper-public-declarations sha256=$($summary.mappingSha256)"
