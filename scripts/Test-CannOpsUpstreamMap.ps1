param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
& (Join-Path $PSScriptRoot 'Generate-CannOpsUpstreamMap.ps1') -RepositoryRoot $repo -Check

$raw = Get-Content (Join-Path $repo 'compatibility/cannops-upstream-raw.json') -Raw | ConvertFrom-Json
$summary = Get-Content (Join-Path $repo 'compatibility/cannops-upstream-summary.json') -Raw | ConvertFrom-Json
$map = Get-Content (Join-Path $repo 'compatibility/cannops-upstream-classifications.json') -Raw | ConvertFrom-Json
$sourcePrefix = 'opencv-source/opencv_contrib-5.0.0/modules/cannops/'
$modulePath = Join-Path $repo "../$sourcePrefix"
$cmake = Get-Content (Join-Path $modulePath 'CMakeLists.txt') -Raw
$kernelCmake = Get-Content (Join-Path $modulePath 'ascendc_kernels/CMakeLists.txt') -Raw
$finder = Get-Content (Join-Path $repo '../opencv-source/opencv-5.0.0/cmake/OpenCVFindCANN.cmake') -Raw
$interface = Get-Content (Join-Path $modulePath 'include/opencv2/cann_interface.hpp') -Raw
$buildCondition = 'HAVE_CANN_AND_SUPPORTED_UNIX_X86_64_OR_AARCH64_AND_NOT_IOS_WINRT_ANDROID_APPLE_WIN32_AND_ASCENDC_TOOLCHAIN'

function Assert-CannOpsData {
    param([object]$Raw, [object]$Summary, [object]$Map)

    if ([string]$Raw.headerPath -cne "${sourcePrefix}include/opencv2/cann.hpp" -or @($Raw.sourceHeaders).Count -ne 8 -or [int]$Raw.declarationCount -ne 201 -or @($Raw.declarations).Count -ne 201) { throw 'CannOps installed header closure/count drifted.' }
    if ([string]$Raw.parserPath -cne 'opencv-source/opencv-5.0.0/modules/python/src2/hdr_parser.py' -or [string]$Raw.parserSha256 -cne '619cfc4e00987a905e06e11e780b963d90b3b0b86d41a8278a9b3a9d81780702' -or [string]$Raw.parserMode -cne 'non-wrapper-public-declarations') { throw 'CannOps pinned parser contract drifted.' }
    if (@($Raw.preprocessorDefinitions.PSObject.Properties).Count -ne 3 -or [int]$Raw.preprocessorDefinitions.CV_VERSION_MAJOR -ne 5 -or [int]$Raw.preprocessorDefinitions.OPENCV_ABI_COMPATIBILITY -ne 500 -or [int]$Raw.preprocessorDefinitions.HAVE_CANN -ne 1) { throw 'CannOps parser definitions enable an unsupported configuration.' }
    foreach ($source in @(@{ path = $Raw.headerPath; sha256 = $Raw.headerSha256 }, @{ path = $Raw.parserPath; sha256 = $Raw.parserSha256 }) + @($Raw.sourceHeaders) + @($Raw.compatibilityHeaders)) {
        $path = Join-Path $repo "../$($source.path)"
        if ([string]$source.sha256 -cne (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant()) { throw 'CannOps pinned source hash drifted.' }
    }
    $names = @('ascendc_kernels.hpp','cann.hpp','cann.inl.hpp','cann_call.hpp','cann_interface.hpp','cann_private.hpp','dvpp_call.hpp','stream_accessor.hpp')
    $counts = @(0,61,0,41,61,10,22,6)
    $parserCounts = @(0,61,18,41,61,10,22,6)
    $callableCounts = @(0,57,0,37,60,10,20,4)
    $nextOrdinal = 0
    for ($index = 0; $index -lt $names.Count; $index++) {
        $header = $Raw.sourceHeaders[$index]
        if ([string]$header.path -cne "${sourcePrefix}include/opencv2/$($names[$index])" -or [int]$header.startOrdinal -ne $nextOrdinal -or [int]$header.declarationCount -ne $counts[$index] -or [int]$header.parserDeclarationCount -ne $parserCounts[$index]) { throw 'CannOps source inventory/ordinal partition drifted.' }
        $members = @($Raw.declarations | Where-Object sourceHeader -CEQ ([string]$header.path))
        if ($members.Count -ne $counts[$index] -or @($members | Where-Object kind -eq 'callable').Count -ne $callableCounts[$index]) { throw 'CannOps per-header declaration membership drifted.' }
        foreach ($decl in $members) {
            if ([int]$decl.ordinal -lt $nextOrdinal -or [int]$decl.ordinal -ge ($nextOrdinal + $counts[$index])) { throw 'CannOps source declaration escaped its header partition.' }
        }
        $nextOrdinal += $counts[$index]
    }
    if ($nextOrdinal -ne 201) { throw 'CannOps header partitions do not close.' }
    $compatibilityPaths = @("${sourcePrefix}CMakeLists.txt", "${sourcePrefix}ascendc_kernels/CMakeLists.txt", 'opencv-source/opencv-5.0.0/cmake/OpenCVFindCANN.cmake', "${sourcePrefix}tutorials/ascend_npu_image_processing.markdown")
    if (@($Raw.compatibilityHeaders).Count -ne 4 -or (@($Raw.compatibilityHeaders | ForEach-Object path) -join '|') -cne ($compatibilityPaths -join '|')) { throw 'CannOps build/runtime evidence closure drifted.' }
    if ($cmake -notmatch '(?s)if\(IOS OR WINRT OR ANDROID OR APPLE OR WIN32 OR \(NOT HAVE_CANN\)\).*?ocv_module_disable\(cannops\)' -or $cmake -notmatch 'ocv_add_module\(cannops opencv_core WRAP python\)' -or $cmake -notmatch 'ocv_create_module\(\$\{CANN_LIBRARIES\}\)' -or $cmake -notmatch 'add_subdirectory\(ascendc_kernels\)') { throw 'CannOps upstream module gates and dependencies drifted.' }
    if ($finder -notmatch 'if\(NOT UNIX\)' -or $finder -notmatch 'CMAKE_SYSTEM_PROCESSOR STREQUAL "aarch64"' -or $finder -notmatch 'CMAKE_SYSTEM_PROCESSOR STREQUAL "x86_64" OR CMAKE_SYSTEM_PROCESSOR STREQUAL "amd64"' -or $finder -notmatch 'try_compile\(VALID_ASCENDCL' -or $finder -notmatch 'if\(NOT \$\{VALID_ASCENDCL\}\)') { throw 'CannOps supported platform/SDK probe boundary drifted.' }
    foreach ($library in @('ascendcl','acl_op_compiler','acl_dvpp_mpi','graph','ge_compiler','opsproto')) {
        if ($finder -notmatch ("NAMES " + $library + ' PATHS')) { throw 'CannOps external SDK library closure drifted.' }
    }
    if ($kernelCmake -notmatch 'set\(SOC_VERSION "ascend310p3"' -or $kernelCmake -notmatch 'set\(RUN_MODE "npu"' -or $kernelCmake -notmatch 'set\(ASCEND_CANN_PACKAGE_PATH "/usr/local/Ascend/ascend-toolkit/latest"' -or $kernelCmake -notmatch 'compiler/tikcpp/ascendc_kernel_cmake' -or $kernelCmake -notmatch 'ascendc_devkit/tikcpp/samples/cmake' -or $kernelCmake -notmatch 'message\(FATAL_ERROR' -or $kernelCmake -notmatch 'threshold_opencv_kernel\.cpp') { throw 'CannOps external AscendC toolchain/SOC contract drifted.' }
    if ((@($Raw.moduleDependencies) -join '|') -cne 'opencv_core' -or [string]$Raw.externalLibraries -cne 'CANN_LIBRARIES' -or [string]$Raw.moduleBuildCondition -cne $buildCondition) { throw 'CannOps build metadata drifted.' }

    $allIdentities = @($Raw.declarations | ForEach-Object { [string]$_.identity })
    if (@($Raw.parserDuplicateIdentities).Count -ne 0 -or $allIdentities.Count -ne @($allIdentities | Sort-Object -Unique).Count -or @($Raw.declarations | Where-Object { [string]$_.identity -notmatch '^(?:enum |class )?cv\.cann\.' -or [string]$_.identity -match 'Ascend(?:Stream|Event)_Impl' }).Count -ne 0) { throw 'CannOps normalized identity closure drifted.' }
    if ((@($Raw.qualifiedImplementationClasses) -join '|') -cne 'cv.cann.AscendStream.Impl|cv.cann.AscendEvent.Impl' -or [string]$Raw.parserNormalization -notmatch 'temporary parser copy') { throw 'CannOps temporary parser normalization evidence drifted.' }
    foreach ($identity in @('class cv.cann.AscendStream.Impl', 'class cv.cann.AscendEvent.Impl', 'cv.cann.AscendStream.Impl.Impl()->', 'cv.cann.AscendEvent.Impl.~Impl()->', 'cv.cann.AscendMat.operator=(const AscendMat& m)->AscendMat&')) {
        if ($allIdentities -cnotcontains $identity) { throw 'CannOps nested implementation/operator identities were not restored.' }
    }
    $inlineIdentities = @($Raw.inlineDefinitionIdentities)
    if ($inlineIdentities.Count -ne 16 -or @($inlineIdentities | Sort-Object -Unique).Count -ne 16) { throw 'CannOps inline definition reconciliation drifted.' }
    foreach ($identity in $inlineIdentities) {
        if (@($Raw.declarations | Where-Object { [string]$_.identity -ceq $identity -and [string]$_.sourceHeader -ceq "${sourcePrefix}include/opencv2/cann.hpp" }).Count -ne 1) { throw 'CannOps inline definition has no unique public declaration.' }
    }
    $privateIdentities = @('cv.cann.AscendStream.AscendStream(const Ptr<AscendStream::Impl>& impl)->', 'cv.cann.AscendEvent.AscendEvent(const Ptr<AscendEvent::Impl>& impl)->')
    if ((@($Raw.excludedPrivateInlineDefinitions) -join '|') -cne ($privateIdentities -join '|')) { throw 'CannOps private inline constructor exclusion drifted.' }
    foreach ($identity in $privateIdentities) {
        if ($allIdentities -ccontains $identity) { throw 'CannOps private inline constructor leaked into the public map.' }
    }
    if ([int]$Raw.bindingOnlyPlaceholderBlockCount -ne 7 -or [regex]::Matches($interface, '(?m)^#ifdef NEVER_DEFINED').Count -ne 7) { throw 'CannOps inactive binding-only placeholder inventory drifted.' }
    foreach ($name in @('add','subtract','multiply','divide','bitwise_and','bitwise_or','bitwise_xor')) {
        $overloads = @($Raw.declarations | Where-Object name -CEQ "cv.cann.$name")
        if ($overloads.Count -ne 4 -or @($overloads | Where-Object { [string]$_.identity -match '(?:const InputArray src1;const Scalar& src2|const Scalar& src1;const InputArray src2)' }).Count -ne 0) { throw 'CannOps inactive binding-only overload entered the declaration map.' }
    }

    if ([int]$Summary.declarationCount -ne 201 -or [int]$Summary.callableCount -ne 188 -or [int]$Summary.classCount -ne 12 -or [int]$Summary.enumCount -ne 1 -or [int]$Summary.classificationCounts.implemented -ne 0 -or [int]$Summary.classificationCounts.'intentionally-omitted' -ne 188 -or [int]$Summary.classificationCounts.'non-callable-metadata' -ne 13 -or [int]$Summary.classificationCounts.missing -ne 0 -or [int]$Summary.classificationCounts.unsupported -ne 0 -or [int]$Summary.classificationCounts.'upstream-conditional' -ne 0 -or @($Map.declarations).Count -ne 201) { throw 'CannOps declaration classification partition drifted.' }
    for ($index = 0; $index -lt 201; $index++) {
        $decl = $Raw.declarations[$index]
        $row = $Map.declarations[$index]
        $expectedClassification = if ([string]$decl.kind -eq 'callable') { 'intentionally-omitted' } else { 'non-callable-metadata' }
        if ([int]$decl.ordinal -ne $index -or [int]$row.ordinal -ne $index -or [string]$decl.identity -cne [string]$row.identity -or [string]$row.classification -cne $expectedClassification -or [string]$row.buildCondition -cne $buildCondition -or @($row.nativeEntrypoints).Count -ne 0 -or @($row.managedMembers).Count -ne 0) { throw 'CannOps ordinal, identity, classification, evidence, or build gate drifted.' }
        if ($expectedClassification -eq 'intentionally-omitted' -and ([string]$row.reason -notmatch 'no native or managed wrapper' -or [string]$row.reason -notmatch 'Huawei CANN' -or [string]$row.reason -notmatch 'AscendC')) { throw 'CannOps callable omission lost its package/runtime boundary.' }
    }
    if ([int]$Summary.nativeEvidenceCount -ne 0 -or [int]$Summary.managedEvidenceCount -ne 0 -or [int]$Summary.selectedDeclarationCount -ne 0 -or [int]$Summary.managedPublicTypeAdditionCount -ne 0 -or [int]$Summary.managedPublicMemberAdditionCount -ne 0 -or [bool]$Summary.repositoryWideUpstreamParityClaimed) { throw 'CannOps map overstates package wrapper or parity evidence.' }
    $manifest = Get-Content (Join-Path $repo 'src/OpenCvSharp.Native/generated/native_abi_manifest.txt') -Raw
    $managed = Get-Content (Join-Path $repo 'compatibility/managed-public-api.txt') -Raw
    if ($manifest -match '(?m)^jyppx_ocv_cann(?:ops)?_' -or $managed -match 'JYPPX\.OpenCvSharp\.Cann(?:Ops)?(?:\.|\|)') { throw 'CannOps package native/managed evidence must remain empty.' }
    if ([int]$Summary.externalDataDependencyCount -ne 3 -or @($Map.externalDataDependencies).Count -ne 3 -or [string]$Map.externalDataDependencies[0] -notmatch 'external Huawei CANN SDK' -or [string]$Map.externalDataDependencies[1] -notmatch 'AscendC.*SOC_VERSION.*RUN_MODE' -or [string]$Map.externalDataDependencies[2] -notmatch 'Ascend device/runtime.*stream/event/context') { throw 'CannOps external dependency boundary drifted.' }
}

Assert-CannOpsData -Raw $raw -Summary $summary -Map $map
$fixtures = @(
    @{ Name = 'removed installed helper header'; Mutate = { param($value) $value.raw.sourceHeaders = @($value.raw.sourceHeaders | Select-Object -Skip 1) } }
    @{ Name = 'damaged source hash'; Mutate = { param($value) $value.raw.sourceHeaders[0].sha256 = '0' * 64 } }
    @{ Name = 'removed declaration'; Mutate = { param($value) $value.raw.declarations = @($value.raw.declarations | Select-Object -Skip 1) } }
    @{ Name = 'leaked temporary Impl spelling'; Mutate = { param($value) ($value.raw.declarations | Where-Object identity -CEQ 'class cv.cann.AscendStream.Impl').identity = 'class cv.cann.AscendStream_Impl' } }
    @{ Name = 'duplicated inline reconciliation'; Mutate = { param($value) $value.raw.inlineDefinitionIdentities[1] = $value.raw.inlineDefinitionIdentities[0] } }
    @{ Name = 'leaked private inline constructor'; Mutate = { param($value) $value.raw.declarations[1].identity = $value.raw.excludedPrivateInlineDefinitions[0] } }
    @{ Name = 'enabled binding-only placeholders'; Mutate = { param($value) $value.raw.preprocessorDefinitions | Add-Member -NotePropertyName NEVER_DEFINED -NotePropertyValue 1 } }
    @{ Name = 'false implemented callable'; Mutate = { param($value) $value.map.declarations[1].classification = 'implemented' } }
    @{ Name = 'false native evidence'; Mutate = { param($value) $value.map.declarations[1].nativeEntrypoints = @('jyppx_ocv_cannops_fake') } }
    @{ Name = 'false managed evidence'; Mutate = { param($value) $value.map.declarations[1].managedMembers = @('JYPPX.OpenCvSharp.CannOps.Fake') } }
    @{ Name = 'lost upstream build gate'; Mutate = { param($value) $value.map.declarations[1].buildCondition = 'ALWAYS_AVAILABLE' } }
    @{ Name = 'false repository parity'; Mutate = { param($value) $value.summary.repositoryWideUpstreamParityClaimed = $true } }
)
foreach ($fixture in $fixtures) {
    $value = @{ raw = $raw; summary = $summary; map = $map } | ConvertTo-Json -Depth 30 | ConvertFrom-Json
    & $fixture.Mutate $value
    $rejected = $false
    try { Assert-CannOpsData -Raw $value.raw -Summary $value.summary -Map $value.map }
    catch { $rejected = $true }
    if (-not $rejected) { throw "CannOps contract accepted negative fixture: $($fixture.Name)" }
}
if ($fixtures.Count -ne [int]$summary.negativeFixtureCount) { throw 'CannOps negative fixture evidence count drifted.' }

Write-Host "CANNOPS_UPSTREAM_MAP_CONTRACT_OK declarations=201 callable=188 implemented=0 omitted=188 missing=0 headers=8 inlineReconciled=16 privateInlineExcluded=2 native=0 managed=0 fixtures=$($fixtures.Count) externalDependencies=3 sha256=$($summary.mappingSha256)"
