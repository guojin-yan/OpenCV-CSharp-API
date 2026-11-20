param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
& (Join-Path $PSScriptRoot 'Generate-MatlabUpstreamMap.ps1') -RepositoryRoot $repo -Check
$raw = Get-Content (Join-Path $repo 'compatibility/matlab-upstream-raw.json') -Raw | ConvertFrom-Json
$summary = Get-Content (Join-Path $repo 'compatibility/matlab-upstream-summary.json') -Raw | ConvertFrom-Json
$map = Get-Content (Join-Path $repo 'compatibility/matlab-upstream-classifications.json') -Raw | ConvertFrom-Json
$extensionPath = Join-Path $repo 'compatibility/matlab-source-reviewed-extensions.json'
$extension = Get-Content $extensionPath -Raw | ConvertFrom-Json
$prefix = 'opencv-source/opencv_contrib-5.0.0/modules/matlab/'
$modulePath = Join-Path $repo "../$prefix"
$includePath = Join-Path $modulePath 'include/opencv2/matlab'
$cmake = Get-Content (Join-Path $modulePath 'CMakeLists.txt') -Raw
$findMatlab = Get-Content (Join-Path $modulePath 'cmake/OpenCVFindMatlab.cmake') -Raw
$compile = Get-Content (Join-Path $modulePath 'compile.cmake') -Raw
$bridge = Get-Content (Join-Path $includePath 'bridge.hpp') -Raw
$mxarray = Get-Content (Join-Path $includePath 'mxarray.hpp') -Raw
$buildCondition = 'MATLAB_FOUND_AND_PYTHON_AND_JINJA2_AND_MATCHING_BITNESS_AND_MEX_PROBES_AND_NOT_ANDROID_APPLE_FRAMEWORK'
$names = @('bridge.hpp','map.hpp','mxarray.hpp','transpose.hpp')
$lexemePattern = @'
//[^\n]*|/\*[\s\S]*?\*/|"(?:\\[\s\S]|[^"\\])*"|'(?:\\[\s\S]|[^'\\])*'
'@
$directivePattern = '(?m)^[ \t]*#(?:[^\n]*\\\n)*[^\n]*(?:\n|$)'

function Hash-Text([string]$Text) {
    [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData([Text.Encoding]::UTF8.GetBytes($Text))).ToLowerInvariant()
}
function Mask-Text([string]$Text, [bool]$Literals = $true) {
    [regex]::Replace($Text, $lexemePattern.Trim(), [Text.RegularExpressions.MatchEvaluator]{ param($match)
        if ($Literals -or $match.Value.StartsWith('//') -or $match.Value.StartsWith('/*')) { return [regex]::Replace($match.Value, '[^\n]', ' ') }
        return $match.Value
    })
}
function Compact-Text([string]$Text) { [regex]::Replace((Mask-Text $Text $false), '\s+', ' ').Trim() }

# Independent .NET discovery keeps ordinary verification free of Python, MEX
# and downloads. Template units retain every branch, not an active build view.
$sourceCache = @{}
foreach ($name in $names) {
    $path = Join-Path $includePath $name
    $text = [IO.File]::ReadAllText($path).Replace("`r`n", "`n")
    $directives = @([regex]::Matches((Mask-Text $text $false), $directivePattern))
    $stack = [Collections.Generic.List[object]]::new()
    $events = [Collections.Generic.List[object]]::new()
    $conditionRows = [Collections.Generic.List[string]]::new()
    foreach ($directive in $directives) {
        $logical = [regex]::Replace($directive.Value, '\\\n', ' ').Trim()
        $parts = [regex]::Match($logical, '^#\s*(\w+)\s*(.*)$')
        $command, $rest = $parts.Groups[1].Value, $parts.Groups[2].Value
        if ($command -in @('if','ifdef','ifndef')) {
            $expression = if ($command -eq 'if') { $rest } elseif ($command -eq 'ifdef') { "defined($rest)" } else { "!defined($rest)" }
            $stack.Add(@{ Seen = @($expression); Current = $expression })
        } elseif ($command -in @('elif','else')) {
            if ($stack.Count -eq 0) { throw 'MATLAB source has unmatched conditional.' }
            $item = $stack[$stack.Count - 1]
            $prior = @($item.Seen | ForEach-Object { "($_)" }) -join ' || '
            $item.Current = "!($prior)" + $(if ($command -eq 'elif') { " && ($rest)" } else { '' })
            $item.Seen += $rest
        } elseif ($command -eq 'endif') {
            if ($stack.Count -eq 0) { throw 'MATLAB source has unmatched endif.' }
            $stack.RemoveAt($stack.Count - 1)
        }
        if ($command -in @('if','ifdef','ifndef','elif','else','endif')) {
            $line = [regex]::Matches($text.Substring(0, $directive.Index), '\n').Count + 1
            $conditionRows.Add("$line|$logical")
        }
        if ($command -eq 'define' -and $rest -match '^[A-Za-z_]\w*\(') { throw 'MATLAB local function-like macro scope changed.' }
        $events.Add(@{ End = $directive.Index + $directive.Length; Conditions = @($stack | ForEach-Object { [string]$_.Current }) })
    }
    if ($stack.Count -ne 0) { throw 'MATLAB source has unclosed conditionals.' }
    $code = [regex]::Replace((Mask-Text $text), $directivePattern, [Text.RegularExpressions.MatchEvaluator]{ param($match) [regex]::Replace($match.Value, '[^\n]', ' ') })
    $sourceCache["${prefix}include/opencv2/matlab/$name"] = @{ Text = $text; Code = $code; Events = $events.ToArray(); Conditions = $conditionRows.ToArray();
        TemplateOffsets = @([regex]::Matches($code, '\btemplate\s*<') | ForEach-Object Index) }
}

function Assert-MatlabData {
    param([object]$Raw, [object]$Summary, [object]$Map, [object]$Extension)

    $installed = @(Get-ChildItem -LiteralPath $includePath -Recurse -File -Filter '*.hpp' | ForEach-Object Name | Sort-Object)
    if (($installed -join '|') -cne ($names -join '|') -or @($Raw.sourceHeaders).Count -ne 4 -or @($Extension.headers).Count -ne 4 -or [int]$Raw.declarationCount -ne 190 -or @($Raw.declarations).Count -ne 190 -or @($Map.declarations).Count -ne 190) { throw 'MATLAB installed-header/parser subset scope drifted.' }
    if ([string]$Raw.headerPath -cne "${prefix}include/opencv2/matlab/bridge.hpp" -or [string]$Raw.parserMode -cne 'non-wrapper-non-template-subset' -or [string]$Raw.parserPath -cne 'opencv-source/opencv-5.0.0/modules/python/src2/hdr_parser.py' -or [string]$Raw.parserSha256 -cne '619cfc4e00987a905e06e11e780b963d90b3b0b86d41a8278a9b3a9d81780702') { throw 'MATLAB pinned parser identity drifted.' }
    if (@($Raw.preprocessorDefinitions.PSObject.Properties).Count -ne 3 -or [int]$Raw.preprocessorDefinitions.MATLAB_SOURCE_CPLUSPLUS -ne 201103 -or [int]$Raw.preprocessorDefinitions.CV_VERSION_MAJOR -ne 5 -or [int]$Raw.preprocessorDefinitions.OPENCV_ABI_COMPATIBILITY -ne 500 -or [string]$Raw.parserNormalization -notmatch 'L integer suffix') { throw 'MATLAB temporary C++ source configuration drifted.' }
    $counts, $callableCounts, $templateCounts, $conditionCounts = @(154,0,36,0), @(153,0,32,0), @(9,2,23,4), @(2,5,11,4)
    $ordinal = 0
    for ($i = 0; $i -lt 4; $i++) {
        $source = "${prefix}include/opencv2/matlab/$($names[$i])"
        $header, $review = $Raw.sourceHeaders[$i], $Extension.headers[$i]
        if ([string]$header.path -cne $source -or [string]$review.path -cne $source -or [string]$header.sha256 -cne [string]$review.sha256 -or [int]$header.startOrdinal -ne $ordinal -or [int]$header.declarationCount -ne $counts[$i]) { throw 'MATLAB header ordinal partition drifted.' }
        $members = @($Raw.declarations | Where-Object sourceHeader -CEQ $source)
        if ($members.Count -ne $counts[$i] -or @($members | Where-Object kind -eq 'callable').Count -ne $callableCounts[$i]) { throw 'MATLAB per-header parser membership drifted.' }
        $ordinal += $counts[$i]
        $cache = $sourceCache[$source]
        if (@($review.templates).Count -ne $templateCounts[$i] -or @($review.conditionalDirectives).Count -ne $conditionCounts[$i] -or @($review.templateParameters).Count -ne 0 -or @($review.macroDefinitions).Count -ne 0) { throw 'MATLAB lexical source unit count drifted.' }
        if ((@($review.templates | ForEach-Object startOffset) -join '|') -cne (@($cache.TemplateOffsets) -join '|') -or (@($review.conditionalDirectives | ForEach-Object { "$($_.line)|$($_.directive)" }) -join "`n") -cne (@($cache.Conditions) -join "`n")) { throw 'MATLAB independent template/conditional discovery drifted.' }
        foreach ($unit in @($review.templates)) {
            $start, $end, $headEnd = [int]$unit.startOffset, [int]$unit.endOffset, [int]$unit.headEndOffset
            if ($start -lt 0 -or $end -le $start -or $end -gt $cache.Text.Length -or $headEnd -le $start -or $headEnd -ge $end) { throw 'MATLAB source span is invalid.' }
            $snippet = $cache.Text.Substring($start, $end - $start)
            $head = Compact-Text $cache.Text.Substring($start, $headEnd - $start)
            $parts = [regex]::Match($head, '^template\s*(<[^>]*>)\s*(.*)$')
            $kind = if ($parts.Groups[2].Value -match '^(class|struct)\s') { 'class-template' } elseif ($parts.Groups[2].Value -match '^using\s') { 'alias-template' } else { 'function-template' }
            if (-not $parts.Success -or [string]$unit.kind -cne $kind -or [string]$unit.templateParameters -cne $parts.Groups[1].Value -or [string]$unit.declarationHead -cne $parts.Groups[2].Value -or [string]$unit.sourceSha256 -cne (Hash-Text $snippet) -or [int]$unit.line -ne ([regex]::Matches($cache.Text.Substring(0, $start), '\n').Count + 1)) { throw 'MATLAB source template head/kind/hash/line drifted.' }
            $conditions = @()
            foreach ($event in $cache.Events) { if ([int]$event.End -le $start) { $conditions = @($event.Conditions) } else { break } }
            if ((@($unit.conditions) -join '|') -cne ($conditions -join '|')) { throw 'MATLAB source branch provenance drifted.' }
        }
    }
    $compatibilityNames = @('CMakeLists.txt','cmake/init.cmake','cmake/OpenCVFindMatlab.cmake','compile.cmake','README.md','generator/gen_matlab.py','generator/parse_tree.py','generator/templates/functional.cpp','generator/templates/template_class_base.cpp','generator/templates/template_function_base.cpp','test/test_compiler.cpp','test/test_generator.hpp')
    if ((@($Raw.compatibilityHeaders | ForEach-Object path) -join '|') -cne (@($compatibilityNames | ForEach-Object { "$prefix$_" }) -join '|')) { throw 'MATLAB build/generator compatibility source closure drifted.' }
    foreach ($source in @(@{ path = $Raw.headerPath; sha256 = $Raw.headerSha256 }, @{ path = $Raw.parserPath; sha256 = $Raw.parserSha256 }) + @($Raw.sourceHeaders) + @($Raw.compatibilityHeaders)) {
        if ([string]$source.sha256 -cne (Get-FileHash -LiteralPath (Join-Path $repo "../$($source.path)") -Algorithm SHA256).Hash.ToLowerInvariant()) { throw 'MATLAB upstream source hash drifted.' }
    }
    foreach ($countObject in @($Raw.sourceInventoryCounts, $Summary.sourceInventoryCounts, $Extension.counts)) {
        if ([int]$countObject.templatesCount -ne 38 -or [int]$countObject.templateParametersCount -ne 0 -or [int]$countObject.macroDefinitionsCount -ne 0 -or [int]$countObject.conditionalDirectivesCount -ne 22 -or [int]$countObject.templateKindCounts.'class-template' -ne 15 -or [int]$countObject.templateKindCounts.'function-template' -ne 22 -or [int]$countObject.templateKindCounts.'alias-template' -ne 1) { throw 'MATLAB source inventory totals drifted.' }
    }
    if ([string]$Raw.sourceReviewedExtensionPath -cne 'compatibility/matlab-source-reviewed-extensions.json' -or [string]$Summary.sourceReviewedExtensionPath -cne [string]$Raw.sourceReviewedExtensionPath -or [string]$Raw.sourceReviewedExtensionSha256 -cne (Get-FileHash $extensionPath -Algorithm SHA256).Hash.ToLowerInvariant() -or [string]$Summary.sourceReviewedExtensionSha256 -cne [string]$Raw.sourceReviewedExtensionSha256) { throw 'MATLAB lexical hash binding drifted.' }
    $targets = @('cv::Mat','std::string','cv::Point','cv::Point2f','cv::Point2d','cv::Size','cv::Moments','cv::Scalar','cv::Rect','cv::RotatedRect','cv::TermCriteria','cv::RNG')
    if (@($Raw.parserNameRepairs).Count -ne 14) { throw 'MATLAB parser name repair count drifted.' }
    for ($i = 0; $i -lt 12; $i++) {
        $repair = $Raw.parserNameRepairs[$i]
        if ([string]$repair.parserName -cne ('cv.bridge.operator ' + $targets[$i].Replace('::','.')) -or [string]$repair.correctedName -cne "cv.bridge.Bridge.operator $($targets[$i])" -or [string]$repair.sourceHeader -cne [string]$Raw.headerPath -or $bridge -notmatch ([regex]::Escape("operator $($targets[$i])()"))) { throw 'MATLAB conversion-operator owner/target repair drifted.' }
    }
    for ($i = 12; $i -lt 14; $i++) {
        $name = @('conditionalError','error')[$i - 12]
        if ([string]$Raw.parserNameRepairs[$i].parserName -cne "matlab.void $name" -or [string]$Raw.parserNameRepairs[$i].correctedName -cne "matlab.$name" -or (@($Raw.declarations[154 + $i - 12].modifiers) -join '|') -cne '/S') { throw 'MATLAB static parser repair/metadata drifted.' }
    }
    if (@($Raw.reconciledInlineDefinitions).Count -ne 2 -or (@($Raw.reconciledInlineDefinitions | ForEach-Object retainedOrdinal) -join '|') -cne '9|10') { throw 'MATLAB duplicate inline reconciliation drifted.' }
    foreach ($duplicate in $Raw.reconciledInlineDefinitions) {
        if ([string]$duplicate.identity -cne [string]$Raw.declarations[[int]$duplicate.retainedOrdinal].identity -or [string]$duplicate.sourceHeader -cne [string]$Raw.headerPath) { throw 'MATLAB inline declaration identity drifted.' }
    }
    if (@($Raw.parserConfigurationProbes).Count -ne 3 -or (@($Raw.parserConfigurationProbes | ForEach-Object cplusplus) -join '|') -cne '199711|201103|201703' -or (@($Raw.parserConfigurationProbes | ForEach-Object originalMxarrayDeclarationCount) -join '|') -cne '36|36|36' -or (@($Raw.parserConfigurationProbes | ForEach-Object normalizedMxarrayDeclarationCount) -join '|') -cne '35|36|36' -or (@($Raw.parserConfigurationProbes | ForEach-Object normalizedMapDeclarationCount) -join '|') -cne '0|0|0') { throw 'MATLAB source-language parser probes drifted.' }
    if ([string]$Raw.moduleBuildCondition -cne $buildCondition -or $cmake -notmatch 'if\(APPLE_FRAMEWORK OR ANDROID OR NOT MATLAB_FOUND\)' -or $cmake -notmatch 'elseif\(NOT PYTHON_DEFAULT_AVAILABLE\)' -or $cmake -notmatch 'if\(NOT HAVE_PYTHON_JINJA2\)' -or $cmake -notmatch 'ocv_add_module\(matlab\s+BINDINGS' -or $cmake -notmatch 'if \(\$\{ARCH\} EQUAL 32 AND \$\{MATLAB_ARCH\} MATCHES "64"\)' -or $cmake -notmatch 'if \(GEN_ERROR\)' -or $cmake -notmatch 'if \(MEX_ERROR\)' -or $compile -notmatch 'if \("\$\{CONFIGURATION\}" MATCHES "Debug"\)' -or $findMatlab -notmatch 'find_path\(MATLAB_INCLUDE_DIRS_ mex.h' -or $findMatlab -notmatch 'find_library\(MATLAB_LIB_MX_' -or $findMatlab -notmatch 'find_library\(MATLAB_LIB_MEX_' -or $findMatlab -notmatch 'find_library\(MATLAB_LIB_MAT_') { throw 'MATLAB platform/dependency/MEX build contract drifted.' }
    $deps = @('core','imgproc','ml','imgcodecs','videoio','highgui','objdetect','flann','features','photo','video','videostab','calib','geometry','stitching','superres','xfeatures2d','optflow','xphoto') | ForEach-Object { "opencv_$_" }
    if ((@($Raw.optionalGeneratorModuleDependencies) -join '|') -cne ($deps -join '|') -or @($Raw.ownershipBoundaries).Count -ne 4 -or $mxarray -notmatch 'MxArray\(const mxArray\* ptr\).*owns_\(false\)' -or $mxarray -notmatch 'mxDuplicateArray\(other.ptr_\)' -or $mxarray -notmatch 'if \(owns_ && ptr_\).*mxDestroyArray' -or $mxarray -notmatch 'releaseOwnership\(\)\s*\{\s*owns_ = false;\s*return ptr_;' -or $bridge -notmatch 'void deepCopyAndTranspose') { throw 'MATLAB optional generator dependencies/ownership boundary drifted.' }
    if ([int]$Raw.upstreamPlaceholderBoundaries.noOpAssignmentDefinitionCount -ne 48 -or [regex]::Matches($bridge, 'Bridge& operator=\([^\n]*\) \{ return \*this; \}').Count -ne 48 -or -not [bool]$Raw.upstreamPlaceholderBoundaries.boolConversionReturnsZero -or -not $bridge.Contains('bool toBool() { return 0; }')) { throw 'MATLAB upstream placeholder conversion limits drifted.' }
    $identities = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    for ($i = 0; $i -lt 190; $i++) {
        $decl, $row = $Raw.declarations[$i], $Map.declarations[$i]
        $classification = if ([string]$decl.kind -eq 'callable') { 'intentionally-omitted' } else { 'non-callable-metadata' }
        $gate = $buildCondition + $(if ($i -eq 166) { '_AND_SOURCE_CPLUSPLUS_GE_201103' } else { '' })
        if (-not $identities.Add([string]$decl.identity) -or [int]$decl.ordinal -ne $i -or [int]$row.ordinal -ne $i -or [string]$row.identity -cne [string]$decl.identity -or [string]$row.classification -cne $classification -or [string]$row.buildCondition -cne $gate -or [bool]$decl.requiresCpp11 -ne ($i -eq 166) -or @($row.nativeEntrypoints).Count -ne 0 -or @($row.managedMembers).Count -ne 0) { throw 'MATLAB ordinal/identity/classification/Cpp11/build/evidence closure drifted.' }
        if ($classification -eq 'intentionally-omitted' -and ([string]$row.reason -notmatch 'no native or managed wrapper' -or [string]$row.reason -notmatch 'mex.h' -or [string]$row.reason -notmatch 'releaseOwnership' -or [string]$row.reason -notmatch 'non-template parser subset')) { throw 'MATLAB callable omission boundary drifted.' }
        if ([string]$decl.name -match '^cv\.bridge\.operator ' -or [string]$decl.name -match '^matlab\.void ') { throw 'MATLAB unrepaired parser identity remains.' }
    }
    foreach ($target in $targets) {
        if (@($Raw.declarations | Where-Object name -CEQ "cv.bridge.Bridge.operator $target").Count -ne 1) { throw 'MATLAB repaired conversion identity is missing.' }
    }
    if ([string]$Raw.declarations[166].identity -cne 'matlab.MxArray.MxArray(MxArray&& other)->' -or [string]$Raw.declarations[186].arguments[3].type -cne '...' -or (@($Raw.declarations[10].modifiers) -join '|') -cne '/C') { throw 'MATLAB move/variadic/const signature metadata drifted.' }
    if ([int]$Summary.declarationCount -ne 190 -or [int]$Summary.callableCount -ne 185 -or [int]$Summary.classCount -ne 5 -or [int]$Summary.enumCount -ne 0 -or [int]$Summary.classificationCounts.'intentionally-omitted' -ne 185 -or [int]$Summary.classificationCounts.'non-callable-metadata' -ne 5 -or [int]$Summary.classificationCounts.missing -ne 0 -or [int]$Summary.nativeEvidenceCount -ne 0 -or [int]$Summary.managedEvidenceCount -ne 0 -or [int]$Summary.selectedDeclarationCount -ne 0 -or [int]$Summary.managedPublicTypeAdditionCount -ne 0 -or [int]$Summary.managedPublicMemberAdditionCount -ne 0 -or [bool]$Summary.repositoryWideUpstreamParityClaimed) { throw 'MATLAB parser subset summary overstates wrapper/parity evidence.' }
    if ([bool]$Raw.completeCallableCoverageClaimed -or [bool]$Summary.completeCallableCoverageClaimed -or [bool]$Extension.completeCallableCoverageClaimed -or [bool]$Extension.macroExpansionClaimed -or [bool]$Extension.templateInstantiationCoverageClaimed -or [bool]$Raw.matlabExecutionVerified -or [bool]$Summary.matlabExecutionVerified -or [bool]$Raw.generatedBindingSurfaceIncluded -or [bool]$Summary.generatedBindingSurfaceIncluded -or [string]$Summary.coverageStatus -cne 'parser-subset-with-expanded-source-callable-inventory') { throw 'MATLAB source inventory overstates complete/generated/runtime coverage.' }
    foreach ($limitation in @($Raw.sourceInventoryLimitation, $Extension.limitation, $Map.limitation)) { if ([string]$limitation -notmatch 'not complete MATLAB callable coverage' -or [string]$limitation -notmatch 'MATLAB/MEX build/link/runtime') { throw 'MATLAB partial-source limitation disappeared.' } }
    if ([int]$Summary.externalDataDependencyCount -ne 3 -or @($Map.externalDataDependencies).Count -ne 3 -or [string]$Map.externalDataDependencies[0] -notmatch 'external MATLAB' -or [string]$Map.externalDataDependencies[1] -notmatch 'Python and Jinja2' -or [string]$Map.externalDataDependencies[2] -notmatch 'releaseOwnership') { throw 'MATLAB external dependency boundaries drifted.' }
    if ((Get-Content (Join-Path $repo 'src/OpenCvSharp.Native/generated/native_abi_manifest.txt') -Raw) -match '(?m)^jyppx_ocv_matlab_' -or (Get-Content (Join-Path $repo 'compatibility/managed-public-api.txt') -Raw) -match 'JYPPX\.OpenCvSharp\.Matlab(?:\.|\|)') { throw 'MATLAB native/managed wrapper evidence must remain empty.' }
}

Assert-MatlabData $raw $summary $map $extension
$fixtures = @(
    @{ Name = 'lost installed header'; Mutate = { param($v) $v.raw.sourceHeaders = @($v.raw.sourceHeaders | Select-Object -Skip 1) } }
    @{ Name = 'lost template'; Mutate = { param($v) $v.extension.headers[0].templates = @($v.extension.headers[0].templates | Select-Object -Skip 1) } }
    @{ Name = 'damaged source span'; Mutate = { param($v) $v.extension.headers[0].templates[0].sourceSha256 = '0' * 64 } }
    @{ Name = 'false branch provenance'; Mutate = { param($v) $v.extension.headers[1].templates[0].conditions = @('__cplusplus < 201103L') } }
    @{ Name = 'alias incorrectly counted as function'; Mutate = { param($v) $v.extension.headers[1].templates[0].kind = 'function-template' } }
    @{ Name = 'damaged static name repair'; Mutate = { param($v) $v.raw.parserNameRepairs[12].correctedName = 'matlab.void conditionalError' } }
    @{ Name = 'lost conversion owner'; Mutate = { param($v) $v.raw.declarations[11].name = 'cv.bridge.operator cv.Mat' } }
    @{ Name = 'damaged inline reconciliation'; Mutate = { param($v) $v.raw.reconciledInlineDefinitions[0].retainedOrdinal = 8 } }
    @{ Name = 'lost Cpp11 move gate'; Mutate = { param($v) $v.raw.declarations[166].requiresCpp11 = $false } }
    @{ Name = 'false original parser language gate'; Mutate = { param($v) $v.raw.parserConfigurationProbes[0].originalMxarrayDeclarationCount = 35 } }
    @{ Name = 'lost MEX build boundary'; Mutate = { param($v) $v.map.declarations[0].buildCondition = 'MATLAB_FOUND' } }
    @{ Name = 'false implemented callable'; Mutate = { param($v) $v.map.declarations[1].classification = 'implemented' } }
    @{ Name = 'false native evidence'; Mutate = { param($v) $v.map.declarations[1].nativeEntrypoints = @('jyppx_ocv_matlab_fake') } }
    @{ Name = 'false managed evidence'; Mutate = { param($v) $v.map.declarations[1].managedMembers = @('JYPPX.OpenCvSharp.Matlab.Fake') } }
    @{ Name = 'false complete callable coverage'; Mutate = { param($v) $v.summary.completeCallableCoverageClaimed = $true } }
    @{ Name = 'false MATLAB execution'; Mutate = { param($v) $v.raw.matlabExecutionVerified = $true } }
    @{ Name = 'false generated gateway coverage'; Mutate = { param($v) $v.summary.generatedBindingSurfaceIncluded = $true } }
    @{ Name = 'erased upstream placeholder limits'; Mutate = { param($v) $v.raw.upstreamPlaceholderBoundaries.noOpAssignmentDefinitionCount = 0 } }
)
foreach ($fixture in $fixtures) {
    $value = @{ raw = $raw; summary = $summary; map = $map; extension = $extension } | ConvertTo-Json -Depth 35 | ConvertFrom-Json
    & $fixture.Mutate $value
    $rejected = $false
    try { Assert-MatlabData $value.raw $value.summary $value.map $value.extension } catch { $rejected = $true }
    if (-not $rejected) { throw "MATLAB guard accepted negative fixture: $($fixture.Name)" }
}
$sourceFixtureCount = & (Join-Path $PSScriptRoot 'Test-MatlabSourceCallables.ps1') -RepositoryRoot $repo -SourceCache $sourceCache -Extension $extension -Raw $raw -Summary $summary
if ($fixtures.Count + $sourceFixtureCount -ne [int]$summary.negativeFixtureCount) { throw 'MATLAB negative fixture evidence count drifted.' }
Write-Host "MATLAB_UPSTREAM_MAP_CONTRACT_OK parserDeclarations=190 parserCallable=185 omitted=185 missing=0 headers=4 templates=38 conditionals=22 fixtures=$($fixtures.Count + $sourceFixtureCount) completeCallableCoverage=false matlabVerified=false"
