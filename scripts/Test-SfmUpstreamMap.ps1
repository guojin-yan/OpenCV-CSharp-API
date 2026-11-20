param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
& (Join-Path $PSScriptRoot 'Generate-SfmUpstreamMap.ps1') -RepositoryRoot $repo -Check
$raw = Get-Content (Join-Path $repo 'compatibility/sfm-upstream-raw.json') -Raw | ConvertFrom-Json
$summary = Get-Content (Join-Path $repo 'compatibility/sfm-upstream-summary.json') -Raw | ConvertFrom-Json
$map = Get-Content (Join-Path $repo 'compatibility/sfm-upstream-classifications.json') -Raw | ConvertFrom-Json
$prefix = 'opencv-source/opencv_contrib-5.0.0/modules/sfm/'
$modulePath = Join-Path $repo "../$prefix"
$cmake = Get-Content (Join-Path $modulePath 'CMakeLists.txt') -Raw
$libmvCmake = Get-Content (Join-Path $modulePath 'src/libmv_light/libmv/CMakeLists.txt') -Raw
$pipelineCmake = Get-Content (Join-Path $modulePath 'src/libmv_light/libmv/simple_pipeline/CMakeLists.txt') -Raw
$umbrella = Get-Content (Join-Path $modulePath 'include/opencv2/sfm.hpp') -Raw
$reconstructHeader = Get-Content (Join-Path $modulePath 'include/opencv2/sfm/reconstruct.hpp') -Raw
$reconstructSource = Get-Content (Join-Path $modulePath 'src/reconstruct.cpp') -Raw
$pipelineSource = Get-Content (Join-Path $modulePath 'src/simple_pipeline.cpp') -Raw
$fundamentalSource = Get-Content (Join-Path $modulePath 'src/fundamental.cpp') -Raw
$readme = Get-Content (Join-Path $modulePath 'README.md') -Raw
$buildCondition = 'HAVE_EIGEN_AND_SFM_DEPS_OK'

function Assert-SfmData {
    param([object]$Raw, [object]$Summary, [object]$Map)

    if ([string]$Raw.headerPath -cne "${prefix}include/opencv2/sfm.hpp" -or @($Raw.sourceHeaders).Count -ne 10 -or [int]$Raw.declarationCount -ne 62 -or @($Raw.declarations).Count -ne 62) { throw 'SFM installed header closure/count drifted.' }
    if ([string]$Raw.parserPath -cne 'opencv-source/opencv-5.0.0/modules/python/src2/hdr_parser.py' -or [string]$Raw.parserSha256 -cne '619cfc4e00987a905e06e11e780b963d90b3b0b86d41a8278a9b3a9d81780702' -or [string]$Raw.parserMode -cne 'non-wrapper-public-declarations') { throw 'SFM pinned parser contract drifted.' }
    if (@($Raw.preprocessorDefinitions.PSObject.Properties).Count -ne 3 -or [int]$Raw.preprocessorDefinitions.CV_VERSION_MAJOR -ne 5 -or [int]$Raw.preprocessorDefinitions.OPENCV_ABI_COMPATIBILITY -ne 500 -or [int]$Raw.preprocessorDefinitions.CERES_FOUND -ne 1) { throw 'SFM parser configuration drifted.' }
    $names = @('sfm.hpp','sfm/conditioning.hpp','sfm/fundamental.hpp','sfm/io.hpp','sfm/numeric.hpp','sfm/projection.hpp','sfm/reconstruct.hpp','sfm/robust.hpp','sfm/simple_pipeline.hpp','sfm/triangulation.hpp')
    $counts = @(0,5,11,2,2,5,4,2,30,1)
    $callableCounts = @(0,5,11,1,2,5,4,2,24,1)
    $nextOrdinal = 0
    for ($index = 0; $index -lt $names.Count; $index++) {
        $header = $Raw.sourceHeaders[$index]
        $requiresCeres = $names[$index] -in @('sfm/reconstruct.hpp','sfm/simple_pipeline.hpp')
        if ([string]$header.path -cne "${prefix}include/opencv2/$($names[$index])" -or [int]$header.startOrdinal -ne $nextOrdinal -or [int]$header.declarationCount -ne $counts[$index] -or [bool]$header.requiresCeres -ne $requiresCeres) { throw 'SFM source inventory/Ceres/ordinal partition drifted.' }
        $members = @($Raw.declarations | Where-Object sourceHeader -CEQ ([string]$header.path))
        if ($members.Count -ne $counts[$index] -or @($members | Where-Object kind -eq 'callable').Count -ne $callableCounts[$index]) { throw 'SFM per-header declaration membership drifted.' }
        foreach ($decl in $members) {
            if ([int]$decl.ordinal -lt $nextOrdinal -or [int]$decl.ordinal -ge ($nextOrdinal + $counts[$index]) -or [bool]$decl.requiresCeres -ne $requiresCeres) { throw 'SFM declaration escaped its header/Ceres partition.' }
        }
        $nextOrdinal += $counts[$index]
    }
    $compatibilityNames = @('CMakeLists.txt','src/libmv_light/libmv/CMakeLists.txt','src/libmv_light/libmv/simple_pipeline/CMakeLists.txt','src/reconstruct.cpp','src/simple_pipeline.cpp','src/fundamental.cpp','README.md','tutorials/sfm_installation/sfm_installation.markdown')
    if (@($Raw.compatibilityHeaders).Count -ne 8 -or (@($Raw.compatibilityHeaders | ForEach-Object path) -join '|') -cne (@($compatibilityNames | ForEach-Object { "$prefix$_" }) -join '|')) { throw 'SFM build/runtime evidence closure drifted.' }
    foreach ($source in @(@{ path = $Raw.headerPath; sha256 = $Raw.headerSha256 }, @{ path = $Raw.parserPath; sha256 = $Raw.parserSha256 }) + @($Raw.sourceHeaders) + @($Raw.compatibilityHeaders)) {
        if ([string]$source.sha256 -cne (Get-FileHash -LiteralPath (Join-Path $repo "../$($source.path)") -Algorithm SHA256).Hash.ToLowerInvariant()) { throw 'SFM pinned source hash drifted.' }
    }
    $expectedIncludes = @('conditioning.hpp','fundamental.hpp','io.hpp','numeric.hpp','projection.hpp','triangulation.hpp','reconstruct.hpp','simple_pipeline.hpp') | ForEach-Object { "${prefix}include/opencv2/sfm/$_" }
    if ((@($Raw.umbrellaIncludes) -join '|') -cne ($expectedIncludes -join '|') -or (@($Raw.standaloneInstalledHeaders) -join '|') -cne "${prefix}include/opencv2/sfm/robust.hpp" -or $umbrella -match '#include <opencv2/sfm/robust.hpp>') { throw 'SFM standalone robust/umbrella include distinction drifted.' }
    if ((@($Raw.ceresConditionalHeaders) -join '|') -cne "${prefix}include/opencv2/sfm/reconstruct.hpp|${prefix}include/opencv2/sfm/simple_pipeline.hpp" -or [int]$Raw.baseDeclarationCount -ne 28 -or [int]$Raw.ceresDeclarationCount -ne 34 -or [int]$Raw.ceresCallableCount -ne 28 -or @($Raw.declarations | Where-Object requiresCeres).Count -ne 34) { throw 'SFM Ceres declaration partition drifted.' }
    if (@($Raw.ceresHeaderGuardProbes).Count -ne 3 -or (@($Raw.ceresHeaderGuardProbes | ForEach-Object configuration) -join '|') -cne 'undefined|defined-zero|defined-one' -or (@($Raw.ceresHeaderGuardProbes | ForEach-Object reconstructHeaderDeclarationCount) -join '|') -cne '0|4|4' -or @($Raw.ceresHeaderGuardProbes[0].preprocessorDefinitions.PSObject.Properties).Count -ne 2 -or [int]$Raw.ceresHeaderGuardProbes[1].preprocessorDefinitions.CERES_FOUND -ne 0 -or [int]$Raw.ceresHeaderGuardProbes[2].preprocessorDefinitions.CERES_FOUND -ne 1) { throw 'SFM defined-zero Ceres header probe evidence drifted.' }
    if ($reconstructHeader -notmatch '#if defined\(CV_DOXYGEN\) \|\| defined\(CERES_FOUND\)' -or $umbrella -notmatch '(?s)#if CERES_FOUND\s*#include <opencv2/sfm/reconstruct.hpp>\s*#include <opencv2/sfm/simple_pipeline.hpp>' -or $reconstructSource -notmatch '(?m)^#if CERES_FOUND' -or $pipelineSource -notmatch '(?m)^#if CERES_FOUND' -or [string]$Raw.ceresHeaderGuardLimitation -notmatch 'direct header visibility is not linkable Ceres support') { throw 'SFM header visibility and linkable Ceres implementation boundary drifted.' }

    if ($cmake -notmatch 'if\(NOT HAVE_EIGEN OR NOT SFM_DEPS_OK\)' -or $cmake -notmatch '(?s)try_compile\(\s*SFM_GLOG_GFLAGS_TEST' -or $cmake -notmatch 'set\(SFM_DEPS_OK "\$\{SFM_GLOG_GFLAGS_TEST\}"\)' -or $cmake -notmatch '(?s)if\(Ceres_FOUND\).*?-DCERES_FOUND=1.*?else\(\).*?-DCERES_FOUND=0' -or $libmvCmake -notmatch '(?s)if \( Ceres_FOUND \)\s*add_subdirectory\(simple_pipeline\)' -or $pipelineCmake -notmatch 'TARGET_LINK_LIBRARIES\(opencv.sfm.simple_pipeline LINK_PRIVATE opencv.sfm.multiview \$\{CERES_LIBRARIES\}\)') { throw 'SFM external dependency compilation/linkage gates drifted.' }
    $dependencies = 'opencv_core|opencv_geometry|opencv_features|opencv_xfeatures2d|opencv_imgcodecs'
    if ((@($Raw.moduleDependencies) -join '|') -cne $dependencies -or $cmake -notmatch '(?s)ocv_add_module\(sfm\s+opencv_core\s+opencv_geometry\s+opencv_features\s+opencv_xfeatures2d\s+opencv_imgcodecs\s+WRAP python\s*\)' -or [string]$Raw.moduleBuildCondition -cne $buildCondition -or [string]$Raw.ceresBuildCondition -cne "${buildCondition}_AND_CERES_FOUND") { throw 'SFM OpenCV dependencies or build metadata drifted.' }
    if ($readme -notmatch 'only available for Linux/GNU systems' -or $cmake -notmatch 'if\(WIN32\)' -or [string]$Raw.readmePlatformAdvice -notmatch 'CMake has no corresponding unconditional platform disable') { throw 'SFM documentary platform advice was confused with enforced build gates.' }
    if ([int]$Raw.affineReconstructionNotImplementedBranchCount -ne 4 -or [regex]::Matches($reconstructSource, 'CV_Error\(Error::StsNotImplemented, "Affine reconstruction not yet implemented"\)').Count -ne 4 -or $fundamentalSource -notmatch '(?s)computeOrientation\( const Mat_<T>.*?TODO: finish implementation' -or @($Raw.upstreamRuntimeLimitations).Count -ne 2 -or [string]$Raw.upstreamRuntimeLimitations[0] -notmatch 'computeOrientation.*unfinished' -or [string]$Raw.upstreamRuntimeLimitations[1] -notmatch 'four reconstruct overloads.*StsNotImplemented') { throw 'SFM pinned upstream incomplete-runtime boundaries drifted.' }

    $allIdentities = @($Raw.declarations | ForEach-Object { [string]$_.identity })
    if (@($Raw.parserDuplicateIdentities).Count -ne 0 -or $allIdentities.Count -ne @($allIdentities | Sort-Object -Unique).Count -or @($Raw.declarations | Where-Object { [string]$_.identity -notmatch '^(?:enum |class )?cv\.sfm\.' }).Count -ne 0) { throw 'SFM exact identity closure drifted.' }
    $enumSignatures = @('const cv.sfm.SFM_IO_BUNDLER=0;const cv.sfm.SFM_IO_VISUALSFM=1;const cv.sfm.SFM_IO_OPENSFM=2;const cv.sfm.SFM_IO_OPENMVG=3;const cv.sfm.SFM_IO_THEIASFM=4', 'const cv.sfm.SFM_DISTORTION_MODEL_POLYNOMIAL=0;const cv.sfm.SFM_DISTORTION_MODEL_DIVISION=1', 'const cv.sfm.SFM_REFINE_FOCAL_LENGTH=(1 << 0);const cv.sfm.SFM_REFINE_PRINCIPAL_POINT=(1 << 1);const cv.sfm.SFM_REFINE_RADIAL_DISTORTION_K1=(1 << 2);const cv.sfm.SFM_REFINE_RADIAL_DISTORTION_K2=(1 << 4)')
    $enums = @($Raw.declarations | Where-Object kind -eq 'enum')
    if ($enums.Count -ne 3) { throw 'SFM anonymous enum metadata count drifted.' }
    for ($index = 0; $index -lt 3; $index++) {
        $values = @($enums[$index].enumValues | ForEach-Object { "$($_.name)=$($_.value)" }) -join ';'
        if ($values -cne $enumSignatures[$index] -or [string]$enums[$index].identity -cne "enum cv.sfm.<unnamed>[$values]") { throw 'SFM anonymous enums lost their distinct value identities.' }
    }
    foreach ($option in @(@{ Name = 'libmv_CameraIntrinsicsOptions'; Properties = 14 }, @{ Name = 'libmv_ReconstructionOptions'; Properties = 5 })) {
        $classes = @($Raw.declarations | Where-Object { $_.kind -eq 'class' -and $_.name -ceq "cv.sfm.$($option.Name)" })
        if ($classes.Count -ne 1 -or @($classes[0].arguments).Count -ne $option.Properties -or @($classes[0].arguments | Where-Object { @($_.modifiers) -cnotcontains '/RW' }).Count -ne 0) { throw 'SFM option property metadata drifted.' }
    }
    foreach ($owner in @('BaseSFM','SFMLibmvEuclideanReconstruction')) {
        $runs = @($Raw.declarations | Where-Object name -CEQ "cv.sfm.$owner.run")
        if ($runs.Count -ne 4 -or @($runs | Where-Object { [string]$_.identity -match 'const std::vector<String> & images' }).Count -ne 2) { throw 'SFM non-wrapper image/track pipeline overload closure drifted.' }
    }
    if (@($Raw.declarations | Where-Object name -CEQ 'cv.sfm.reconstruct').Count -ne 4 -or $allIdentities -cnotcontains 'cv.sfm.BaseSFM.~BaseSFM()->' -or @($Raw.declarations | Where-Object name -CEQ 'cv.sfm.SFMLibmvEuclideanReconstruction.create').Count -ne 1) { throw 'SFM non-wrapper reconstruction/factory/destructor closure drifted.' }
    if ([int]$Summary.declarationCount -ne 62 -or [int]$Summary.callableCount -ne 55 -or [int]$Summary.classCount -ne 4 -or [int]$Summary.enumCount -ne 3 -or [int]$Summary.classificationCounts.implemented -ne 0 -or [int]$Summary.classificationCounts.'intentionally-omitted' -ne 55 -or [int]$Summary.classificationCounts.'non-callable-metadata' -ne 7 -or [int]$Summary.classificationCounts.missing -ne 0 -or [int]$Summary.classificationCounts.unsupported -ne 0 -or [int]$Summary.classificationCounts.'upstream-conditional' -ne 0 -or @($Map.declarations).Count -ne 62) { throw 'SFM declaration classification partition drifted.' }
    for ($index = 0; $index -lt 62; $index++) {
        $decl = $Raw.declarations[$index]
        $row = $Map.declarations[$index]
        $expectedClassification = if ([string]$decl.kind -eq 'callable') { 'intentionally-omitted' } else { 'non-callable-metadata' }
        $expectedBuildCondition = if ([bool]$decl.requiresCeres) { "${buildCondition}_AND_CERES_FOUND" } else { $buildCondition }
        if ([int]$decl.ordinal -ne $index -or [int]$row.ordinal -ne $index -or [string]$decl.identity -cne [string]$row.identity -or [string]$row.classification -cne $expectedClassification -or [string]$row.buildCondition -cne $expectedBuildCondition -or @($row.nativeEntrypoints).Count -ne 0 -or @($row.managedMembers).Count -ne 0) { throw 'SFM ordinal, identity, classification, wrapper evidence, or per-row build gate drifted.' }
        if ($expectedClassification -eq 'intentionally-omitted' -and ([string]$row.reason -notmatch 'no native or managed wrapper' -or [string]$row.reason -notmatch 'Eigen.*Glog/Gflags' -or ([bool]$decl.requiresCeres -and [string]$row.reason -notmatch 'external Ceres Solver'))) { throw 'SFM callable omission lost its dependency boundary.' }
        if ([string]$decl.name -eq 'cv.sfm.reconstruct' -and [string]$row.reason -notmatch 'StsNotImplemented') { throw 'SFM reconstruct omission lost its upstream affine limitation.' }
        if ([string]$decl.name -eq 'cv.sfm.computeOrientation' -and [string]$row.reason -notmatch 'TODO: finish implementation') { throw 'SFM orientation omission lost its upstream unfinished implementation.' }
    }
    if ([int]$Summary.nativeEvidenceCount -ne 0 -or [int]$Summary.managedEvidenceCount -ne 0 -or [int]$Summary.selectedDeclarationCount -ne 0 -or [int]$Summary.managedPublicTypeAdditionCount -ne 0 -or [int]$Summary.managedPublicMemberAdditionCount -ne 0 -or [bool]$Summary.repositoryWideUpstreamParityClaimed) { throw 'SFM map overstates package wrapper or parity evidence.' }
    if ((Get-Content (Join-Path $repo 'src/OpenCvSharp.Native/generated/native_abi_manifest.txt') -Raw) -match '(?m)^jyppx_ocv_sfm_' -or (Get-Content (Join-Path $repo 'compatibility/managed-public-api.txt') -Raw) -match 'JYPPX\.OpenCvSharp\.Sfm(?:\.|\|)') { throw 'SFM package native/managed evidence must remain empty.' }
    if ([int]$Summary.externalDataDependencyCount -ne 3 -or @($Map.externalDataDependencies).Count -ne 3 -or [string]$Map.externalDataDependencies[0] -notmatch 'Eigen, Glog and Gflags' -or [string]$Map.externalDataDependencies[1] -notmatch 'external Ceres Solver' -or [string]$Map.externalDataDependencies[2] -notmatch 'caller-provided') { throw 'SFM external dependency evidence drifted.' }
}

Assert-SfmData -Raw $raw -Summary $summary -Map $map
$fixtures = @(
    @{ Name = 'removed standalone robust header'; Mutate = { param($value) $value.raw.sourceHeaders = @($value.raw.sourceHeaders | Where-Object { $_.path -notlike '*/robust.hpp' }) } }
    @{ Name = 'lost Ceres declaration flag'; Mutate = { param($value) $value.raw.declarations[25].requiresCeres = $false } }
    @{ Name = 'lost Ceres build gate'; Mutate = { param($value) $value.map.declarations[25].buildCondition = 'HAVE_EIGEN_AND_SFM_DEPS_OK' } }
    @{ Name = 'false defined-zero macro behavior'; Mutate = { param($value) $value.raw.ceresHeaderGuardProbes[1].reconstructHeaderDeclarationCount = 0 } }
    @{ Name = 'dropped unannotated image overload'; Mutate = { param($value) $value.raw.declarations = @($value.raw.declarations | Where-Object ordinal -ne 41) } }
    @{ Name = 'altered anonymous enum value'; Mutate = { param($value) $value.raw.declarations[16].enumValues[0].value = '99' } }
    @{ Name = 'damaged source hash'; Mutate = { param($value) $value.raw.sourceHeaders[0].sha256 = '0' * 64 } }
    @{ Name = 'false implemented callable'; Mutate = { param($value) $value.map.declarations[0].classification = 'implemented' } }
    @{ Name = 'false native evidence'; Mutate = { param($value) $value.map.declarations[0].nativeEntrypoints = @('jyppx_ocv_sfm_fake') } }
    @{ Name = 'false managed evidence'; Mutate = { param($value) $value.map.declarations[0].managedMembers = @('JYPPX.OpenCvSharp.Sfm.Fake') } }
    @{ Name = 'erased upstream affine limitation'; Mutate = { param($value) $value.raw.affineReconstructionNotImplementedBranchCount = 0 } }
    @{ Name = 'false repository parity'; Mutate = { param($value) $value.summary.repositoryWideUpstreamParityClaimed = $true } }
)
foreach ($fixture in $fixtures) {
    $value = @{ raw = $raw; summary = $summary; map = $map } | ConvertTo-Json -Depth 30 | ConvertFrom-Json
    & $fixture.Mutate $value
    $rejected = $false
    try { Assert-SfmData -Raw $value.raw -Summary $value.summary -Map $value.map }
    catch { $rejected = $true }
    if (-not $rejected) { throw "SFM contract accepted negative fixture: $($fixture.Name)" }
}
if ($fixtures.Count -ne [int]$summary.negativeFixtureCount) { throw 'SFM negative fixture evidence count drifted.' }
Write-Host "SFM_UPSTREAM_MAP_CONTRACT_OK declarations=62 callable=55 implemented=0 omitted=55 missing=0 headers=10 base=28 ceres=34 ceresCallable=28 native=0 managed=0 fixtures=$($fixtures.Count) externalDependencies=3 sha256=$($summary.mappingSha256)"
