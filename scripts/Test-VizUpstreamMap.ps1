param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
& (Join-Path $PSScriptRoot 'Generate-VizUpstreamMap.ps1') -RepositoryRoot $repo -Check

$summary = Get-Content (Join-Path $repo 'compatibility/viz-upstream-summary.json') -Raw | ConvertFrom-Json
$raw = Get-Content (Join-Path $repo 'compatibility/viz-upstream-raw.json') -Raw | ConvertFrom-Json
$map = Get-Content (Join-Path $repo 'compatibility/viz-upstream-classifications.json') -Raw | ConvertFrom-Json
$manifest = @(Get-Content (Join-Path $repo 'src/OpenCvSharp.Native/generated/native_abi_manifest.txt') | ForEach-Object { ($_ -split '\|')[0] })
$managed = Get-Content (Join-Path $repo 'compatibility/managed-public-api.txt') -Raw
$headerPath = Join-Path $repo '..\opencv-source\opencv_contrib-5.0.0\modules\viz\include\opencv2\viz.hpp'
$cmakePath = Join-Path $repo '..\opencv-source\opencv_contrib-5.0.0\modules\viz\CMakeLists.txt'
$header = 'opencv-source/opencv_contrib-5.0.0/modules/viz/include/opencv2/viz.hpp'
$buildFile = 'opencv-source/opencv_contrib-5.0.0/modules/viz/CMakeLists.txt'
$cmake = Get-Content $cmakePath -Raw

if ([string]$raw.headerPath -cne $header -or [int]$raw.declarationCount -ne 269 -or @($raw.sourceHeaders).Count -ne 5 -or [string]$raw.moduleBuildCondition -cne 'HAVE_VTK_AND_NO_STATIC_VTK_BUILTIN_CONFLICT') { throw 'Viz parser source closure drifted.' }
if (@($raw.parserDuplicateIdentities).Count -ne 2 -or @($raw.parserDuplicateIdentities | Where-Object { $_ -notin @('cv.viz.Color.Color()->', 'cv.viz.Color.Color(const Scalar& color)->') }).Count -ne 0) { throw 'Viz parser normalization duplicate set drifted.' }
if (@($raw.compatibilityHeaders).Count -ne 1 -or [string]$raw.compatibilityHeaders[0].path -cne $buildFile -or [string]$raw.compatibilityHeaders[0].sha256 -cne (Get-FileHash $cmakePath -Algorithm SHA256).Hash.ToLowerInvariant()) { throw 'Viz build dependency evidence drifted.' }
if ($cmake -notmatch '(?s)if\(NOT HAVE_VTK\).*?ocv_module_disable\(viz\).*?ocv_add_module\(viz opencv_core WRAP python\)') { throw 'Viz upstream CMake no longer enforces the VTK boundary.' }
if ($cmake -notmatch '(?s)if\(NOT BUILD_SHARED_LIBS\).*?ocv_module_disable\(viz\)') { throw 'Viz static VTK conflict boundary drifted.' }
if ([string]$raw.headerSha256 -cne (Get-FileHash $headerPath -Algorithm SHA256).Hash.ToLowerInvariant()) { throw 'Viz umbrella header hash drifted.' }
if ([int]$summary.declarationCount -ne 269 -or [int]$summary.callableCount -ne 225 -or [int]$summary.classCount -ne 35 -or [int]$summary.enumCount -ne 9 -or [int]$summary.classificationCounts.implemented -ne 0 -or [int]$summary.classificationCounts.'intentionally-omitted' -ne 225 -or [int]$summary.classificationCounts.'non-callable-metadata' -ne 44 -or [int]$summary.classificationCounts.missing -ne 0) { throw 'Viz declaration partition drifted.' }
if (@($map.declarations).Count -ne 269 -or @($map.declarations | Where-Object { $_.classification -eq 'intentionally-omitted' -and (@($_.nativeEntrypoints).Count -ne 0 -or @($_.managedMembers).Count -ne 0 -or [string]$_.reason -notmatch 'no native or managed wrapper' -or [string]$_.reason -notmatch 'external VTK' -or [string]$_.reason -notmatch 'caller-owned') }).Count -ne 0 -or @($map.declarations | Where-Object classification -eq 'non-callable-metadata').Count -ne 44 -or @($map.declarations | Where-Object classification -eq 'missing').Count -ne 0) { throw 'Viz callable classification closure drifted.' }
if ([int]$summary.nativeEvidenceCount -ne 0 -or [int]$summary.managedEvidenceCount -ne 0 -or @($manifest | Where-Object { $_ -match '^jyppx_ocv_viz_' }).Count -ne 0 -or $managed -match 'JYPPX\.OpenCvSharp\.Viz(?:\.|\|)') { throw 'Viz map falsely claims a native or managed wrapper surface.' }
if (@($map.externalDataDependencies | Where-Object { [string]$_ -match 'external VTK' }).Count -ne 1 -or @($map.externalDataDependencies | Where-Object { [string]$_ -match 'caller-selected GUI' -and [string]$_ -match 'mesh/cloud/trajectory' }).Count -ne 1) { throw 'Viz external dependency boundary drifted.' }
if (@($map.declarations | Where-Object { $_.identity -match '^cv\.viz\.(?:getWindowByName|imshow|writeCloud|readCloud|readMesh|computeNormals)\(' }).Count -ne 6 -or @($map.declarations | Where-Object { $_.identity -match '^cv\.viz\.Viz3d\.(?:Viz3d|showWidget)' }).Count -lt 2 -or @($map.declarations | Where-Object { $_.identity -match '^cv\.Vec3b\(' }).Count -ne 1) { throw 'Viz parser callable coverage drifted.' }
if ([bool]$summary.repositoryWideUpstreamParityClaimed) { throw 'Viz map must not claim repository-wide parity.' }

Write-Host "VIZ_UPSTREAM_MAP_CONTRACT_OK declarations=$($summary.declarationCount) callable=$($summary.callableCount) implemented=0 omitted=$($summary.classificationCounts.'intentionally-omitted') missing=0 metadata=$($summary.classificationCounts.'non-callable-metadata') native=0 managed=0 externalDependencies=$($summary.externalDataDependencyCount) sha256=$($summary.mappingSha256)"
