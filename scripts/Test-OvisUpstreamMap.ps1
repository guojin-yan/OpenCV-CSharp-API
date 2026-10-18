param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
& (Join-Path $PSScriptRoot 'Generate-OvisUpstreamMap.ps1') -RepositoryRoot $repo -Check

$summary = Get-Content (Join-Path $repo 'compatibility/ovis-upstream-summary.json') -Raw | ConvertFrom-Json
$raw = Get-Content (Join-Path $repo 'compatibility/ovis-upstream-raw.json') -Raw | ConvertFrom-Json
$map = Get-Content (Join-Path $repo 'compatibility/ovis-upstream-classifications.json') -Raw | ConvertFrom-Json
$manifest = @(Get-Content (Join-Path $repo 'src/OpenCvSharp.Native/generated/native_abi_manifest.txt') | ForEach-Object { ($_ -split '\|')[0] })
$managed = Get-Content (Join-Path $repo 'compatibility/managed-public-api.txt') -Raw
$headerPath = Join-Path $repo '..\opencv-source\opencv_contrib-5.0.0\modules\ovis\include\opencv2\ovis.hpp'
$cmakePath = Join-Path $repo '..\opencv-source\opencv_contrib-5.0.0\modules\ovis\CMakeLists.txt'
$header = 'opencv-source/opencv_contrib-5.0.0/modules/ovis/include/opencv2/ovis.hpp'
$buildFile = 'opencv-source/opencv_contrib-5.0.0/modules/ovis/CMakeLists.txt'
$cmake = Get-Content $cmakePath -Raw

if ([string]$raw.headerPath -cne $header -or @($raw.sourceHeaders).Count -ne 1 -or [string]$raw.sourceHeaders[0].path -cne $header -or [int]$raw.sourceHeaders[0].startOrdinal -ne 0 -or [int]$raw.sourceHeaders[0].declarationCount -ne 42) { throw 'OVIS parser source closure drifted.' }
if (@($raw.compatibilityHeaders).Count -ne 1 -or [string]$raw.compatibilityHeaders[0].path -cne $buildFile -or [string]$raw.compatibilityHeaders[0].sha256 -cne (Get-FileHash $cmakePath -Algorithm SHA256).Hash.ToLowerInvariant() -or [string]$raw.moduleBuildCondition -cne 'HAVE_OGRE_GE_1_11_5_AND_REQUIRED_OPENCV_MODULES_PRESENT') { throw 'OVIS build dependency evidence drifted.' }
if ($cmake -notmatch '(?s)find_package\(OGRE QUIET CONFIG\).*?if\(NOT OGRE_FOUND\).*?ocv_module_disable\(ovis\).*?elseif\(OGRE_VERSION VERSION_LESS 1\.11\.5\).*?ocv_module_disable\(ovis\).*?ocv_add_module\(ovis opencv_core opencv_imgproc opencv_geometry WRAP python\).*?ocv_target_link_libraries\(\$\{the_module\} \$\{OGRE_LIBRARIES\}\)') { throw 'OVIS upstream CMake no longer enforces the recorded OGRE and OpenCV module boundary.' }
if ([string]$raw.sourceHeaders[0].sha256 -cne (Get-FileHash $headerPath -Algorithm SHA256).Hash.ToLowerInvariant()) { throw 'OVIS upstream public header hash drifted.' }
if ([int]$raw.declarationCount -ne 42 -or [int]$summary.declarationCount -ne 42 -or [int]$summary.callableCount -ne 38 -or [int]$summary.classCount -ne 1 -or [int]$summary.enumCount -ne 3 -or [int]$summary.classificationCounts.implemented -ne 0 -or [int]$summary.classificationCounts.'intentionally-omitted' -ne 38 -or [int]$summary.classificationCounts.'non-callable-metadata' -ne 4 -or [int]$summary.classificationCounts.missing -ne 0) { throw 'OVIS parser declaration partition drifted.' }
if (@($map.declarations).Count -ne 42 -or @($map.declarations | Where-Object { $_.classification -eq 'intentionally-omitted' -and (@($_.nativeEntrypoints).Count -ne 0 -or @($_.managedMembers).Count -ne 0 -or [string]$_.reason -notmatch 'no native or managed wrapper' -or [string]$_.reason -notmatch 'external OGRE' -or [string]$_.reason -notmatch 'scene resources') }).Count -ne 0 -or @($map.declarations | Where-Object { $_.classification -eq 'non-callable-metadata' }).Count -ne 4 -or @($map.declarations | Where-Object classification -eq 'missing').Count -ne 0) { throw 'OVIS callable classification closure drifted.' }
if ([int]$summary.nativeEvidenceCount -ne 0 -or [int]$summary.managedEvidenceCount -ne 0 -or @($manifest | Where-Object { $_ -match '^jyppx_ocv_ovis_' }).Count -ne 0 -or $managed -match 'JYPPX\.OpenCvSharp\.Ovis(?:\.|\|)') { throw 'OVIS map falsely claims a native or managed wrapper surface.' }
if (@($map.externalDataDependencies | Where-Object { [string]$_ -match 'OGRE3D' }).Count -ne 1 -or @($map.externalDataDependencies | Where-Object { [string]$_ -match 'scene meshes' -and [string]$_ -match 'Mat input/output ownership' }).Count -ne 1) { throw 'OVIS external renderer/resource boundary drifted.' }
if (@($map.declarations | Where-Object { $_.identity -match '^cv\.ovis\.WindowScene\.setBackground\(' }).Count -ne 2 -or @($map.declarations | Where-Object { $_.identity -match '^cv\.ovis\.setMaterialProperty\(' }).Count -ne 4 -or @($map.declarations | Where-Object { $_.identity -match '^cv\.ovis\.createWindow\(' }).Count -ne 1) { throw 'OVIS overload/factory parser coverage drifted.' }
if ([bool]$summary.repositoryWideUpstreamParityClaimed) { throw 'OVIS map must not claim repository-wide parity.' }

Write-Host "OVIS_UPSTREAM_MAP_CONTRACT_OK declarations=$($summary.declarationCount) callable=$($summary.callableCount) implemented=0 omitted=$($summary.classificationCounts.'intentionally-omitted') missing=0 metadata=$($summary.classificationCounts.'non-callable-metadata') native=0 managed=0 externalDependencies=$($summary.externalDataDependencyCount) sha256=$($summary.mappingSha256)"
