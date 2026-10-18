param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
& (Join-Path $PSScriptRoot 'Generate-CvvUpstreamMap.ps1') -RepositoryRoot $repo -Check

$summary = Get-Content (Join-Path $repo 'compatibility/cvv-upstream-summary.json') -Raw | ConvertFrom-Json
$raw = Get-Content (Join-Path $repo 'compatibility/cvv-upstream-raw.json') -Raw | ConvertFrom-Json
$map = Get-Content (Join-Path $repo 'compatibility/cvv-upstream-classifications.json') -Raw | ConvertFrom-Json
$manifest = @(Get-Content (Join-Path $repo 'src/OpenCvSharp.Native/generated/native_abi_manifest.txt') | ForEach-Object { ($_ -split '\|')[0] })
$managed = Get-Content (Join-Path $repo 'compatibility/managed-public-api.txt') -Raw
$headerPath = Join-Path $repo '..\opencv-source\opencv_contrib-5.0.0\modules\cvv\include\opencv2\cvv.hpp'
$cmakePath = Join-Path $repo '..\opencv-source\opencv_contrib-5.0.0\modules\cvv\CMakeLists.txt'
$header = 'opencv-source/opencv_contrib-5.0.0/modules/cvv/include/opencv2/cvv.hpp'
$buildFile = 'opencv-source/opencv_contrib-5.0.0/modules/cvv/CMakeLists.txt'
$cmake = Get-Content $cmakePath -Raw

if ([string]$raw.headerPath -cne $header -or [int]$raw.declarationCount -ne 20 -or @($raw.sourceHeaders).Count -ne 7 -or [string]$raw.moduleBuildCondition -cne 'HAVE_QT_GE_5_AND_CXX11') { throw 'CVV parser source closure drifted.' }
if (@($raw.compatibilityHeaders).Count -ne 1 -or [string]$raw.compatibilityHeaders[0].path -cne $buildFile -or [string]$raw.compatibilityHeaders[0].sha256 -cne (Get-FileHash $cmakePath -Algorithm SHA256).Hash.ToLowerInvariant()) { throw 'CVV build dependency evidence drifted.' }
if ($cmake -notmatch '(?s)NOT HAVE_QT OR NOT HAVE_CXX11.*?ocv_module_disable\(cvv\).*?ocv_add_module\(cvv opencv_core opencv_imgproc opencv_features WRAP python\)') { throw 'CVV upstream CMake no longer enforces the recorded Qt/C++11 boundary.' }
if ([string]$raw.headerSha256 -cne (Get-FileHash $headerPath -Algorithm SHA256).Hash.ToLowerInvariant()) { throw 'CVV umbrella header hash drifted.' }
if ([int]$summary.declarationCount -ne 20 -or [int]$summary.callableCount -ne 18 -or [int]$summary.classCount -ne 2 -or [int]$summary.enumCount -ne 0 -or [int]$summary.classificationCounts.implemented -ne 0 -or [int]$summary.classificationCounts.'intentionally-omitted' -ne 18 -or [int]$summary.classificationCounts.'non-callable-metadata' -ne 2 -or [int]$summary.classificationCounts.missing -ne 0) { throw 'CVV declaration partition drifted.' }
if (@($map.declarations).Count -ne 20 -or @($map.declarations | Where-Object { $_.classification -eq 'intentionally-omitted' -and (@($_.nativeEntrypoints).Count -ne 0 -or @($_.managedMembers).Count -ne 0 -or [string]$_.reason -notmatch 'no native or managed wrapper' -or [string]$_.reason -notmatch 'Qt 5' -or [string]$_.reason -notmatch 'interactive GUI') }).Count -ne 0 -or @($map.declarations | Where-Object classification -eq 'non-callable-metadata').Count -ne 2 -or @($map.declarations | Where-Object classification -eq 'missing').Count -ne 0) { throw 'CVV callable classification closure drifted.' }
if ([int]$summary.nativeEvidenceCount -ne 0 -or [int]$summary.managedEvidenceCount -ne 0 -or @($manifest | Where-Object { $_ -match '^jyppx_ocv_cvv_' }).Count -ne 0 -or $managed -match 'JYPPX\.OpenCvSharp\.Cvv(?:\.|\|)') { throw 'CVV map falsely claims a native or managed wrapper surface.' }
if (@($map.externalDataDependencies | Where-Object { [string]$_ -match 'Qt 5' -and [string]$_ -match 'C\+\+11' }).Count -ne 1) { throw 'CVV external Qt/C++11 boundary drifted.' }
if (@($map.declarations | Where-Object { $_.identity -match 'debugDMatch\(' }).Count -ne 3 -or @($map.declarations | Where-Object { $_.identity -match 'debugFilter\(' }).Count -ne 3 -or @($map.declarations | Where-Object { $_.identity -match 'showImage\(' }).Count -ne 3) { throw 'CVV overload parser coverage drifted.' }
if ([bool]$summary.repositoryWideUpstreamParityClaimed) { throw 'CVV map must not claim repository-wide parity.' }

Write-Host "CVV_UPSTREAM_MAP_CONTRACT_OK declarations=$($summary.declarationCount) callable=$($summary.callableCount) implemented=0 omitted=$($summary.classificationCounts.'intentionally-omitted') missing=0 metadata=$($summary.classificationCounts.'non-callable-metadata') native=0 managed=0 externalDependencies=$($summary.externalDataDependencyCount) sha256=$($summary.mappingSha256)"
