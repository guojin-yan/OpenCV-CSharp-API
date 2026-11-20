param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
& (Join-Path $PSScriptRoot 'Generate-FastCVUpstreamMap.ps1') -RepositoryRoot $repo -Check

$raw = Get-Content (Join-Path $repo 'compatibility/fastcv-upstream-raw.json') -Raw | ConvertFrom-Json
$summary = Get-Content (Join-Path $repo 'compatibility/fastcv-upstream-summary.json') -Raw | ConvertFrom-Json
$map = Get-Content (Join-Path $repo 'compatibility/fastcv-upstream-classifications.json') -Raw | ConvertFrom-Json
$cmakePath = Join-Path $repo '..\opencv-source\opencv_contrib-5.0.0\modules\fastcv\CMakeLists.txt'
$readmePath = Join-Path $repo '..\opencv-source\opencv_contrib-5.0.0\modules\fastcv\README.md'
$umbrellaPath = Join-Path $repo '..\opencv-source\opencv_contrib-5.0.0\modules\fastcv\include\opencv2\fastcv.hpp'
$cmake = Get-Content $cmakePath -Raw
$readme = Get-Content $readmePath -Raw

function Assert-FastCVData {
    param([object]$Raw, [object]$Summary, [object]$Map)

    if ([string]$raw.headerPath -cne 'opencv-source/opencv_contrib-5.0.0/modules/fastcv/include/opencv2/fastcv.hpp' -or @($raw.sourceHeaders).Count -ne 30 -or @($raw.umbrellaIncludes).Count -ne 30 -or [int]$raw.parserDeclarationCountBeforeExactDeduplication -ne 71 -or [int]$raw.declarationCount -ne 71) { throw 'FastCV parser source closure/count drifted.' }
    if (@($raw.parserDuplicateIdentities).Count -ne 0) { throw 'FastCV parser unexpectedly emitted duplicate exact declaration identities.' }
    if (@($raw.compatibilityHeaders).Count -ne 2 -or [string]$raw.compatibilityHeaders[0].path -cne 'opencv-source/opencv_contrib-5.0.0/modules/fastcv/CMakeLists.txt' -or [string]$raw.compatibilityHeaders[1].path -cne 'opencv-source/opencv_contrib-5.0.0/modules/fastcv/README.md') { throw 'FastCV build/runtime boundary evidence drifted.' }
    if ([string]$raw.headerSha256 -cne (Get-FileHash $umbrellaPath -Algorithm SHA256).Hash.ToLowerInvariant() -or [string]$raw.compatibilityHeaders[0].sha256 -cne (Get-FileHash $cmakePath -Algorithm SHA256).Hash.ToLowerInvariant() -or [string]$raw.compatibilityHeaders[1].sha256 -cne (Get-FileHash $readmePath -Algorithm SHA256).Hash.ToLowerInvariant()) { throw 'FastCV pinned source hash drifted.' }
    if ($cmake -notmatch '(?s)if\(HAVE_FASTCV\).*?ocv_define_module\(fastcv opencv_core opencv_geometry opencv_imgproc opencv_features opencv_video WRAP python java\).*?\$\{FASTCV_LIBRARY\}.*?else\(\).*?ocv_module_disable\(fastcv\)') { throw 'FastCV upstream CMake gate, dependencies, bindings, or external library contract drifted.' }
    if ($readme -notmatch '(?is)ARM architecture only.*?x86 platform') { throw 'FastCV upstream ARM-only support boundary drifted.' }
    if (@($raw.moduleDependencies) -join '|' -cne 'opencv_core|opencv_geometry|opencv_imgproc|opencv_features|opencv_video' -or [string]$raw.externalLibrary -cne 'FASTCV_LIBRARY' -or [string]$raw.moduleBuildCondition -cne 'HAVE_FASTCV_AND_SUPPORTED_ARM_ARCHITECTURE_AND_EXTERNAL_FASTCV_LIBRARY') { throw 'FastCV build dependency metadata drifted.' }

    $sourcePaths = @($raw.sourceHeaders | ForEach-Object { [string]$_.path })
    [string[]]$umbrellaPaths = @($raw.umbrellaIncludes)
    [Array]::Sort($umbrellaPaths, [StringComparer]::Ordinal)
    if (($sourcePaths -join '|') -cne ($umbrellaPaths -join '|') -or @($sourcePaths | Sort-Object -Unique).Count -ne 30) { throw 'FastCV umbrella includes and parser source headers do not match.' }
    $nextOrdinal = 0
    foreach ($header in $raw.sourceHeaders) {
        if ([string]$header.path -notmatch '^opencv-source/opencv_contrib-5\.0\.0/modules/fastcv/include/opencv2/fastcv/[A-Za-z0-9_]+\.hpp$') { throw 'FastCV source header escaped the pinned closure.' }
        if ([int]$header.startOrdinal -ne $nextOrdinal) { throw 'FastCV source header ordinal partition drifted.' }
        if ([int]$header.declarationCount -ne @($raw.declarations | Where-Object sourceHeader -CEQ ([string]$header.path)).Count) { throw 'FastCV source header declaration membership drifted.' }
        $nextOrdinal += [int]$header.declarationCount
        $path = Join-Path $repo (Join-Path '..' ([string]$header.path))
        if ([string]$header.sha256 -cne (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant()) { throw 'FastCV source header hash drifted.' }
    }
    if ($nextOrdinal -ne [int]$raw.declarationCount) { throw 'FastCV source header declaration partition drifted.' }
    $allIdentities = @($raw.declarations | ForEach-Object { [string]$_.identity })
    if ($allIdentities.Count -ne (@($allIdentities | Sort-Object -Unique)).Count -or @($raw.declarations | Where-Object { [string]$_.identity -notmatch '^(?:enum |class |struct )?cv\.fastcv\.' }).Count -ne 0) { throw 'FastCV identity normalization is incomplete or duplicated.' }
    $callables = @($raw.declarations | Where-Object kind -eq 'callable')
    $annotatedCallables = @($callables | Where-Object upstreamWrapperAnnotated)
    if ($annotatedCallables.Count -ne 46 -or $annotatedCallables.Count -ne [int]$raw.upstreamWrapperAnnotatedCallableCount -or [int]$raw.upstreamWrapperAnnotatedDeclarationCount -ne 48 -or $annotatedCallables.Count -ge $callables.Count) { throw 'FastCV upstream wrapper-annotation parser pass drifted.' }
    foreach ($decl in $raw.declarations) {
        $expectedAnnotation = [string]$decl.name -eq 'cv.fastcv.FCVMSER' -or [string]$decl.kind -eq 'enum'
        if ([string]$decl.kind -eq 'callable') {
            $expectedAnnotation = [string]$decl.name -notmatch '^cv\.fastcv\.(?:QcResourceManager\.|QcAllocator\.|getQcAllocator$|dsp\.|moments$|FCVMSER\.~FCVMSER$)' -and -not ([string]$decl.name -eq 'cv.fastcv.FCVMSER.detect' -and @($decl.arguments).Count -eq 4)
        }
        if ([bool]$decl.upstreamWrapperAnnotated -ne $expectedAnnotation) { throw 'FastCV upstream wrapper annotations are assigned to the wrong declarations.' }
    }
    if ([int]$summary.declarationCount -ne 71 -or [int]$summary.callableCount -ne 66 -or [int]$summary.classCount -ne 4 -or [int]$summary.enumCount -ne 1 -or [int]$summary.classificationCounts.implemented -ne 0 -or [int]$summary.classificationCounts.'intentionally-omitted' -ne 66 -or [int]$summary.classificationCounts.'non-callable-metadata' -ne 5 -or [int]$summary.classificationCounts.missing -ne 0) { throw 'FastCV declaration partition drifted.' }
    if (@($map.declarations).Count -ne 71 -or @($map.declarations | Where-Object { $_.classification -eq 'intentionally-omitted' -and (@($_.nativeEntrypoints).Count -ne 0 -or @($_.managedMembers).Count -ne 0 -or [string]$_.reason -notmatch 'no native or managed wrapper' -or [string]$_.reason -notmatch 'Qualcomm FastCV' -or [string]$_.reason -notmatch 'ARM') }).Count -ne 0 -or @($map.declarations | Where-Object classification -eq 'missing').Count -ne 0) { throw 'FastCV callable classification closure drifted.' }
    if (@($raw.declarations).Count -ne 71) { throw 'FastCV raw declaration row count drifted.' }
    for ($index = 0; $index -lt 71; $index++) {
        $decl = $raw.declarations[$index]
        $row = $map.declarations[$index]
        $expectedClassification = if ([string]$decl.kind -eq 'callable') { 'intentionally-omitted' } else { 'non-callable-metadata' }
        if ([int]$decl.ordinal -ne $index -or [int]$row.ordinal -ne $index -or [string]$decl.identity -cne [string]$row.identity -or [string]$row.classification -cne $expectedClassification -or [string]$row.buildCondition -cne [string]$raw.moduleBuildCondition) { throw 'FastCV declaration identity, classification, ordinal, or build gate drifted.' }
    }
    if ([int]$summary.nativeEvidenceCount -ne 0 -or [int]$summary.managedEvidenceCount -ne 0 -or [bool]$summary.repositoryWideUpstreamParityClaimed) { throw 'FastCV map overstates package wrapper or parity evidence.' }
    $manifest = @(Get-Content (Join-Path $repo 'src/OpenCvSharp.Native/generated/native_abi_manifest.txt') | ForEach-Object { ($_ -split '\|')[0] })
    $managed = Get-Content (Join-Path $repo 'compatibility/managed-public-api.txt') -Raw
    if (@($manifest | Where-Object { $_ -match '^jyppx_ocv_fastcv_' }).Count -ne 0 -or $managed -match 'JYPPX\.OpenCvSharp\.FastCV(?:\.|\|)') { throw 'FastCV package native/managed evidence must remain empty.' }
    if (@($map.externalDataDependencies | Where-Object { [string]$_ -match 'external Qualcomm FastCV' }).Count -ne 1 -or @($map.externalDataDependencies | Where-Object { [string]$_ -match 'ARM only' }).Count -ne 1) { throw 'FastCV external runtime and architecture dependencies drifted.' }
}

Assert-FastCVData -Raw $raw -Summary $summary -Map $map
$fixtures = @(
    @{ Name = 'removed parser header'; Mutate = { param($value) $value.raw.sourceHeaders = @($value.raw.sourceHeaders | Select-Object -Skip 1) } }
    @{ Name = 'foreign primary header'; Mutate = { param($value) $value.raw.headerPath = 'opencv-source/opencv-5.0.0/modules/core/include/opencv2/core.hpp' } }
    @{ Name = 'removed declaration'; Mutate = { param($value) $value.raw.declarations = @($value.raw.declarations | Select-Object -Skip 1) } }
    @{ Name = 'duplicated identity'; Mutate = { param($value) $value.raw.declarations[1].identity = $value.raw.declarations[0].identity } }
    @{ Name = 'reordered ordinal'; Mutate = { param($value) $value.raw.declarations[1].ordinal = 0 } }
    @{ Name = 'dropped wrapper annotation'; Mutate = { param($value) ($value.raw.declarations | Where-Object name -eq 'cv.fastcv.matmuls8s32').upstreamWrapperAnnotated = $false } }
    @{ Name = 'reassigned wrapper annotation'; Mutate = { param($value) ($value.raw.declarations | Where-Object name -eq 'cv.fastcv.matmuls8s32').upstreamWrapperAnnotated = $false; ($value.raw.declarations | Where-Object name -eq 'cv.fastcv.moments').upstreamWrapperAnnotated = $true } }
    @{ Name = 'false implemented callable'; Mutate = { param($value) $value.map.declarations[1].classification = 'implemented' } }
    @{ Name = 'false native evidence'; Mutate = { param($value) $value.map.declarations[1].nativeEntrypoints = @('jyppx_ocv_fastcv_fake') } }
    @{ Name = 'false managed evidence'; Mutate = { param($value) $value.map.declarations[1].managedMembers = @('JYPPX.OpenCvSharp.FastCV.Fake') } }
    @{ Name = 'lost upstream gate'; Mutate = { param($value) $value.map.declarations[1].buildCondition = 'ALWAYS_AVAILABLE' } }
    @{ Name = 'false repository parity'; Mutate = { param($value) $value.summary.repositoryWideUpstreamParityClaimed = $true } }
)
foreach ($fixture in $fixtures) {
    $value = @{ raw = $raw; summary = $summary; map = $map } | ConvertTo-Json -Depth 30 | ConvertFrom-Json
    & $fixture.Mutate $value
    $rejected = $false
    try { Assert-FastCVData -Raw $value.raw -Summary $value.summary -Map $value.map }
    catch { $rejected = $true }
    if (-not $rejected) { throw "FastCV contract accepted negative fixture: $($fixture.Name)" }
}
if ($fixtures.Count -ne [int]$summary.negativeFixtureCount) { throw 'FastCV negative fixture evidence count drifted.' }

Write-Host "FASTCV_UPSTREAM_MAP_CONTRACT_OK declarations=$($summary.declarationCount) callable=$($summary.callableCount) upstreamWrapperAnnotated=$($raw.upstreamWrapperAnnotatedCallableCount) implemented=0 omitted=$($summary.classificationCounts.'intentionally-omitted') missing=0 native=0 managed=0 fixtures=$($fixtures.Count) externalDependencies=$($summary.externalDataDependencyCount) sha256=$($summary.mappingSha256)"
