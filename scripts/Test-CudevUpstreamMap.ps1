param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
& (Join-Path $PSScriptRoot 'Generate-CudevUpstreamMap.ps1') -RepositoryRoot $repo -Check
$raw = Get-Content (Join-Path $repo 'compatibility/cudev-upstream-raw.json') -Raw | ConvertFrom-Json
$summary = Get-Content (Join-Path $repo 'compatibility/cudev-upstream-summary.json') -Raw | ConvertFrom-Json
$map = Get-Content (Join-Path $repo 'compatibility/cudev-upstream-classifications.json') -Raw | ConvertFrom-Json
$extensionPath = Join-Path $repo 'compatibility/cudev-source-reviewed-extensions.json'
$extension = Get-Content $extensionPath -Raw | ConvertFrom-Json
$prefix = 'opencv-source/opencv_contrib-5.0.0/modules/cudev/'
$includePath = Join-Path $repo "../${prefix}include/opencv2"
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

# Independently reconstruct source discovery and branch context in .NET. The
# invariant suite can validate checked-in evidence without a Python install.
$sourceCache = @{}
$installed = @(Get-ChildItem -LiteralPath $includePath -Recurse -File -Filter '*.hpp' | ForEach-Object {
    $relative = [IO.Path]::GetRelativePath($includePath, $_.FullName).Replace('\', '/')
    $source = "${prefix}include/opencv2/$relative"
    $text = [IO.File]::ReadAllText($_.FullName).Replace("`r`n", "`n")
    $commentsRemoved = Mask-Text $text $false
    $code = Mask-Text $text
    $directives = @([regex]::Matches($commentsRemoved, $directivePattern))
    $stack = [Collections.Generic.List[object]]::new()
    $events = [Collections.Generic.List[object]]::new()
    $conditionRows = [Collections.Generic.List[string]]::new()
    $functionMacros = [Collections.Generic.List[int]]::new()
    foreach ($directive in $directives) {
        $logical = [regex]::Replace($directive.Value, '\\\n', ' ').Trim()
        $parts = [regex]::Match($logical, '^#\s*(\w+)\s*(.*)$')
        $command = $parts.Groups[1].Value
        $rest = $parts.Groups[2].Value
        if ($command -in @('if', 'ifdef', 'ifndef')) {
            $expression = if ($command -eq 'if') { $rest } elseif ($command -eq 'ifdef') { "defined($rest)" } else { "!defined($rest)" }
            $stack.Add(@{ Seen = @($expression); Current = $expression })
        } elseif ($command -in @('elif', 'else')) {
            if ($stack.Count -eq 0) { throw 'Cudev source has an unmatched conditional.' }
            $item = $stack[$stack.Count - 1]
            $prior = (@($item.Seen | ForEach-Object { "($_)" }) -join ' || ')
            $item.Current = "!($prior)" + $(if ($command -eq 'elif') { " && ($rest)" } else { '' })
            $item.Seen += $rest
        } elseif ($command -eq 'endif') {
            if ($stack.Count -eq 0) { throw 'Cudev source has an unmatched endif.' }
            $stack.RemoveAt($stack.Count - 1)
        }
        if ($command -in @('if', 'ifdef', 'ifndef', 'elif', 'else', 'endif')) {
            $line = [regex]::Matches($text.Substring(0, $directive.Index), '\n').Count + 1
            $conditionRows.Add("$line|$logical")
        }
        if ($command -eq 'define' -and $rest -match '^[A-Za-z_]\w*\(') { $functionMacros.Add($directive.Index) }
        $events.Add(@{ End = $directive.Index + $directive.Length; Conditions = @($stack | ForEach-Object { [string]$_.Current }) })
    }
    if ($stack.Count -ne 0) { throw 'Cudev source has unclosed conditionals.' }
    $code = [regex]::Replace($code, $directivePattern, [Text.RegularExpressions.MatchEvaluator]{ param($match) [regex]::Replace($match.Value, '[^\n]', ' ') })
    $sourceCache[$source] = @{ Text = $text; Code = $code; Events = $events.ToArray(); Conditions = $conditionRows.ToArray();
        MacroOffsets = $functionMacros.ToArray(); TemplateOffsets = @([regex]::Matches($code, '\btemplate\s*<') | ForEach-Object Index);
        Sha256 = (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash.ToLowerInvariant() }
    $source
})

function Get-Conditions([object]$Source, [int]$Offset) {
    $low, $high, $selected = 0, ($Source.Events.Count - 1), -1
    while ($low -le $high) {
        $middle = [int][Math]::Floor(($low + $high) / 2)
        if ([int]$Source.Events[$middle].End -le $Offset) { $selected = $middle; $low = $middle + 1 } else { $high = $middle - 1 }
    }
    if ($selected -ge 0) { @($Source.Events[$selected].Conditions) }
}
function Assert-SourceUnit([object]$Source, [object]$Unit) {
    $start, $end = [int]$Unit.startOffset, [int]$Unit.endOffset
    if ($start -lt 0 -or $end -le $start -or $end -gt $Source.Text.Length) { throw 'Cudev source unit extent is invalid.' }
    if ([string]$Unit.sourceSha256 -cne (Hash-Text $Source.Text.Substring($start, $end - $start)) -or [int]$Unit.line -ne ([regex]::Matches($Source.Text.Substring(0, $start), '\n').Count + 1)) { throw 'Cudev source unit hash/line drifted.' }
    if ((@($Unit.conditions) -join '|') -cne (@(Get-Conditions $Source $start) -join '|')) { throw 'Cudev source unit branch context drifted.' }
}

function Assert-CudevData {
    param([object]$Raw, [object]$Summary, [object]$Map, [object]$Extension)

    if ($installed.Count -ne 76 -or @($Raw.sourceHeaders).Count -ne 76 -or @($Extension.headers).Count -ne 76 -or [int]$Raw.declarationCount -ne 144 -or @($Raw.declarations).Count -ne 144) { throw 'Cudev installed-header/parser subset scope drifted.' }
    if ((@($Raw.sourceHeaders.path | Sort-Object) -join '|') -cne (@($installed | Sort-Object) -join '|') -or (@($Extension.headers.path) -join '|') -cne (@($Raw.sourceHeaders.path) -join '|')) { throw 'Cudev source inventory lost an installed header.' }
    if ([string]$Raw.headerPath -cne "${prefix}include/opencv2/cudev.hpp" -or [string]$Raw.headerSha256 -cne $sourceCache[$Raw.headerPath].Sha256 -or [string]$Raw.parserSha256 -cne '619cfc4e00987a905e06e11e780b963d90b3b0b86d41a8278a9b3a9d81780702' -or [string]$Raw.parserMode -cne 'non-wrapper-non-template-subset') { throw 'Cudev pinned parser/umbrella evidence drifted.' }
    $expectedDefines = @{ CV_VERSION_MAJOR = 5; OPENCV_ABI_COMPATIBILITY = 500; __CUDACC__ = 1; CUDART_VERSION = 12040; __CUDACC_VER_MAJOR__ = 12; __CUDACC_VER_MINOR__ = 4; __CUDA_ARCH__ = 750; CV_CUDEV_ARCH = 750 }
    if (@($Raw.preprocessorDefinitions.PSObject.Properties).Count -ne 8) { throw 'Cudev parser define scope drifted.' }
    foreach ($key in $expectedDefines.Keys) { if ([int]$Raw.preprocessorDefinitions.$key -ne $expectedDefines[$key]) { throw 'Cudev parser configuration drifted.' } }
    if ([bool]$Raw.completeCallableCoverageClaimed -or [bool]$Summary.completeCallableCoverageClaimed -or [bool]$Extension.completeCallableCoverageClaimed -or [bool]$Extension.macroExpansionClaimed -or [bool]$Extension.templateInstantiationCoverageClaimed -or [bool]$Raw.cudaExecutionVerified -or [bool]$Summary.cudaExecutionVerified -or [bool]$Summary.repositoryWideUpstreamParityClaimed) { throw 'Cudev evidence overstates callable, macro, template or CUDA coverage.' }
    if ([string]$Summary.coverageStatus -cne 'parser-subset-with-expanded-source-callable-inventory' -or [string]$Extension.reviewDisposition -cne 'intentionally-omitted-unwrapped-device-source' -or [string]$Map.limitation -cne [string]$Raw.sourceInventoryLimitation -or [string]$Extension.limitation -cne [string]$Raw.sourceInventoryLimitation -or [string]$Map.limitation -notmatch 'class-template members and the 44 finite local declaration generators' -or [string]$Map.claimedSlice -notmatch 'complete callable/instantiation coverage remains open') { throw 'Cudev partial source-map limitation drifted.' }
    $extensionHash = (Get-FileHash $extensionPath -Algorithm SHA256).Hash.ToLowerInvariant()
    if ([string]$Summary.sourceReviewedExtensionPath -cne 'compatibility/cudev-source-reviewed-extensions.json' -or [string]$Raw.sourceReviewedExtensionPath -cne [string]$Summary.sourceReviewedExtensionPath -or [string]$Summary.sourceReviewedExtensionSha256 -cne $extensionHash -or [string]$Raw.sourceReviewedExtensionSha256 -cne $extensionHash) { throw 'Cudev supplemental inventory hash binding drifted.' }
    $totals = @{ templatesCount = 0; templateParametersCount = 0; macroDefinitionsCount = 0; macroInvocationsCount = 0; conditionalDirectivesCount = 0; macroBodyTemplateKeywordCount = 0 }
    $definitionIds = @{}
    foreach ($header in $Extension.headers) { foreach ($definition in $header.macroDefinitions) { $definitionIds[[string]$definition.id] = [string]$definition.name } }
    if ($definitionIds.Count -ne 49) { throw 'Cudev macro definition IDs drifted.' }
    $nextOrdinal = 0
    for ($headerIndex = 0; $headerIndex -lt 76; $headerIndex++) {
        $header, $review = $Raw.sourceHeaders[$headerIndex], $Extension.headers[$headerIndex]
        $source = $sourceCache[[string]$header.path]
        if ([string]$header.sha256 -cne [string]$source.Sha256 -or [string]$review.sha256 -cne [string]$source.Sha256 -or [int]$header.startOrdinal -ne $nextOrdinal) { throw 'Cudev source hash or ordinal partition drifted.' }
        $members = @($Raw.declarations | Where-Object sourceHeader -CEQ ([string]$header.path))
        if ($members.Count -ne [int]$header.declarationCount) { throw 'Cudev per-header parser partition drifted.' }
        $nextOrdinal += $members.Count
        foreach ($field in @('templates','templateParameters','macroDefinitions','macroInvocations')) {
            $totals["${field}Count"] += @($review.$field).Count
            foreach ($unit in $review.$field) { Assert-SourceUnit $source $unit }
        }
        $templateOffsets = @(@($review.templates | ForEach-Object startOffset) + @($review.templateParameters | ForEach-Object startOffset) | Sort-Object)
        if (($templateOffsets -join '|') -cne (@($source.TemplateOffsets | Sort-Object) -join '|')) { throw 'Cudev source inventory lost a direct template token.' }
        foreach ($template in $review.templates) {
            $start, $headEnd = [int]$template.startOffset, [int]$template.headEndOffset
            if ($headEnd -le $start -or $headEnd -ge [int]$template.endOffset -or $source.Code[$headEnd] -notin @('{',';')) { throw 'Cudev template head extent drifted.' }
            $head = Compact-Text $source.Text.Substring($start, $headEnd - $start)
            if ($head -cne "template$($template.templateParameters) $($template.declarationHead)" -and $head -cne "template $($template.templateParameters) $($template.declarationHead)") { throw 'Cudev template parameter/head source evidence drifted.' }
            $expectedKind = if ([string]$template.declarationHead -match '^(class|struct)\s') { 'class-template' } else { 'function-template' }
            if ([string]$template.kind -cne $expectedKind) { throw 'Cudev template source kind drifted.' }
        }
        foreach ($parameter in $review.templateParameters) {
            $parents = @($review.templates | Where-Object { [int]$_.startOffset -eq [int]$parameter.enclosingTemplateOffset -and [int]$_.headEndOffset -gt [int]$parameter.endOffset })
            if ($parents.Count -ne 1) { throw 'Cudev nested template-template parameter lost its parent.' }
        }
        if ((@($review.macroDefinitions | ForEach-Object startOffset) -join '|') -cne (@($source.MacroOffsets) -join '|')) { throw 'Cudev function-like macro definition closure drifted.' }
        foreach ($definition in $review.macroDefinitions) {
            $body = $source.Text.Substring([int]$definition.startOffset, [int]$definition.endOffset - [int]$definition.startOffset)
            $logical = [regex]::Replace((Mask-Text $body $false), '\\\n', ' ').Trim()
            $match = [regex]::Match($logical, '^#\s*define\s+([A-Za-z_]\w*)\(([^)]*)\)([\s\S]*)$')
            if (-not $match.Success -or $match.Groups[1].Value -cne [string]$definition.name -or (Compact-Text $match.Groups[3].Value) -cne [string]$definition.replacement -or (@($definition.parameters) -join ',') -cne ([regex]::Replace($match.Groups[2].Value, '\s+', ''))) { throw 'Cudev macro replacement/parameter source evidence drifted.' }
            $keywords = [regex]::Matches((Mask-Text $body), '\btemplate\s*<').Count
            if ($keywords -ne [int]$definition.templateKeywordCount) { throw 'Cudev macro template-token evidence drifted.' }
            $totals.macroBodyTemplateKeywordCount += $keywords
        }
        $macroNames = @($definitionIds.Values | Sort-Object -Unique | ForEach-Object { [regex]::Escape($_) }) -join '|'
        $invocationOffsets = @([regex]::Matches($source.Code, "\b(?:$macroNames)\s*\(") | ForEach-Object Index)
        if ((@($review.macroInvocations | ForEach-Object startOffset) -join '|') -cne ($invocationOffsets -join '|')) { throw 'Cudev source inventory lost a macro invocation.' }
        foreach ($call in $review.macroInvocations) {
            $body = $source.Text.Substring([int]$call.startOffset, [int]$call.endOffset - [int]$call.startOffset)
            $open = $body.IndexOf('(')
            if ($open -lt 1 -or $body.Substring(0, $open).Trim() -cne [string]$call.name -or -not $body.EndsWith(')') -or (Compact-Text $body.Substring($open + 1, $body.Length - $open - 2)) -cne [string]$call.arguments -or @($call.sourceDefinitionIds).Count -eq 0) { throw 'Cudev macro invocation source evidence drifted.' }
            foreach ($id in $call.sourceDefinitionIds) { if (-not $definitionIds.ContainsKey([string]$id) -or $definitionIds[[string]$id] -cne [string]$call.name) { throw 'Cudev macro invocation references an unrelated source definition.' } }
        }
        $totals.conditionalDirectivesCount += @($review.conditionalDirectives).Count
        if ((@($review.conditionalDirectives | ForEach-Object { "$($_.line)|$($_.directive)" }) -join '|') -cne (@($source.Conditions) -join '|')) { throw 'Cudev source conditional directive inventory drifted.' }
    }
    $expectedCounts = @{ templatesCount = 1100; templateParametersCount = 11; macroDefinitionsCount = 49; macroInvocationsCount = 1206; conditionalDirectivesCount = 397; macroBodyTemplateKeywordCount = 92 }
    foreach ($key in $expectedCounts.Keys) { if ([int]$totals[$key] -ne $expectedCounts[$key] -or [int]$Extension.counts.$key -ne $expectedCounts[$key] -or [int]$Raw.sourceInventoryCounts.$key -ne $expectedCounts[$key] -or [int]$Summary.sourceInventoryCounts.$key -ne $expectedCounts[$key]) { throw "Cudev source inventory count drifted: $key" } }
    if (@($Raw.staticParserNameRepairs).Count -ne 13) { throw 'Cudev static parser name repair evidence drifted.' }
    foreach ($repair in $Raw.staticParserNameRepairs) {
        $rows = @($Raw.declarations | Where-Object { $_.sourceHeader -ceq $repair.sourceHeader -and $_.name -ceq $repair.correctedName -and @($_.modifiers) -ccontains '/S' })
        $matchingRepairs = @($rows | Where-Object { [string]$repair.parserName -ceq ([string]$repair.correctedName -creplace '\.([^.]+)$', ".$($_.returnType) `$1") })
        if ($matchingRepairs.Count -ne 1) { throw 'Cudev namespace-scope static parser name repair drifted.' }
    }
    if (@($Raw.parserConfigurationProbes).Count -ne 5) { throw 'Cudev source configuration probes drifted.' }
    for ($index = 0; $index -lt 5; $index++) {
        $probe = $Raw.parserConfigurationProbes[$index]
        $arch = @(0,120,200,300,750)[$index]
        if ([int]$probe.preprocessorDefinitions.CV_CUDEV_ARCH -ne $arch -or [int]$probe.headerDeclarationCounts.'warp/shuffle.hpp' -ne $(if ($arch -ge 300) { 25 } else { 0 }) -or [int]$probe.headerDeclarationCounts.'util/atomic.hpp' -ne 12 -or [int]$probe.headerDeclarationCounts.'util/simd_functions.hpp' -ne 38) { throw 'Cudev source architecture probe drifted.' }
    }
    if ([int]$Summary.declarationCount -ne 144 -or [int]$Summary.callableCount -ne 110 -or [int]$Summary.classCount -ne 24 -or [int]$Summary.enumCount -ne 10 -or @($Map.declarations).Count -ne 144 -or [int]$Summary.classificationCounts.'intentionally-omitted' -ne 110 -or [int]$Summary.classificationCounts.'non-callable-metadata' -ne 34 -or [int]$Summary.classificationCounts.implemented -ne 0 -or [int]$Summary.classificationCounts.missing -ne 0 -or [int]$Summary.classificationCounts.unsupported -ne 0 -or [int]$Summary.classificationCounts.'upstream-conditional' -ne 0) { throw 'Cudev parser-subset classification partition drifted.' }
    for ($index = 0; $index -lt 144; $index++) {
        $decl, $row = $Raw.declarations[$index], $Map.declarations[$index]
        $expectedClassification = if ($decl.kind -eq 'callable') { 'intentionally-omitted' } else { 'non-callable-metadata' }
        if ([int]$decl.ordinal -ne $index -or [int]$row.ordinal -ne $index -or [string]$row.identity -cne [string]$decl.identity -or [string]$row.classification -cne $expectedClassification -or [string]$row.buildCondition -cne 'HAVE_CUDA_AND_RECORDED_CUDA_SOURCE_CONFIGURATION' -or @($row.nativeEntrypoints).Count -ne 0 -or @($row.managedMembers).Count -ne 0) { throw 'Cudev parser identity/classification/wrapper/gate evidence drifted.' }
        if ($decl.kind -eq 'callable' -and ([string]$row.reason -notmatch 'no native or managed wrapper' -or [string]$row.reason -notmatch 'non-template parser subset')) { throw 'Cudev callable omission lost its partial scope.' }
    }
    $cmake = Get-Content (Join-Path $repo "../${prefix}CMakeLists.txt") -Raw
    if ([string]$Raw.moduleBuildCondition -cne 'HAVE_CUDA' -or $cmake -notmatch 'if\(NOT HAVE_CUDA\)\s+ocv_module_disable\(cudev\)' -or $cmake -notmatch 'file\(GLOB_RECURSE lib_hdrs' -or $cmake -notmatch 'ocv_module_include_directories\(opencv_core\)' -or @($Raw.compatibilityHeaders).Count -ne 1 -or [string]$Raw.compatibilityHeaders[0].sha256 -cne (Get-FileHash (Join-Path $repo "../${prefix}CMakeLists.txt") -Algorithm SHA256).Hash.ToLowerInvariant()) { throw 'Cudev HAVE_CUDA/recursive build scope drifted.' }
    if ([int]$Summary.nativeEvidenceCount -ne 0 -or [int]$Summary.managedEvidenceCount -ne 0 -or [int]$Summary.selectedDeclarationCount -ne 0 -or [int]$Summary.managedPublicTypeAdditionCount -ne 0 -or [int]$Summary.managedPublicMemberAdditionCount -ne 0 -or [int]$Summary.externalDataDependencyCount -ne 3 -or @($Map.externalDataDependencies).Count -ne 3) { throw 'Cudev package/dependency boundary drifted.' }
    if ((Get-Content (Join-Path $repo 'src/OpenCvSharp.Native/generated/native_abi_manifest.txt') -Raw) -match '(?m)^jyppx_ocv_cudev_' -or (Get-Content (Join-Path $repo 'compatibility/managed-public-api.txt') -Raw) -match 'JYPPX\.OpenCvSharp\.Cudev(?:\.|\|)') { throw 'Cudev native/managed wrapper evidence must remain empty.' }
}

Assert-CudevData $raw $summary $map $extension
$fixtures = @(
    @{ Name = 'lost detail header'; Mutate = { param($v) $v.raw.sourceHeaders = @($v.raw.sourceHeaders | Select-Object -Skip 1) } }
    @{ Name = 'lost direct template'; Mutate = { param($v) $v.extension.headers[1].templates = @($v.extension.headers[1].templates | Select-Object -Skip 1) } }
    @{ Name = 'damaged template span'; Mutate = { param($v) $v.extension.headers[1].templates[0].sourceSha256 = '0' * 64 } }
    @{ Name = 'false macro expansion'; Mutate = { param($v) $v.extension.macroExpansionClaimed = $true } }
    @{ Name = 'lost macro invocation'; Mutate = { param($v) $h = @($v.extension.headers | Where-Object { @($_.macroInvocations).Count -gt 0 })[0]; $h.macroInvocations = @($h.macroInvocations | Select-Object -Skip 1) } }
    @{ Name = 'false conditional context'; Mutate = { param($v) $v.extension.headers[1].templates[0].conditions = @('HAVE_CUDA') } }
    @{ Name = 'altered macro replacement'; Mutate = { param($v) $h = @($v.extension.headers | Where-Object { @($_.macroDefinitions).Count -gt 0 })[0]; $h.macroDefinitions[0].replacement = 'invalid' } }
    @{ Name = 'lost template-template parent'; Mutate = { param($v) $h = @($v.extension.headers | Where-Object { @($_.templateParameters).Count -gt 0 })[0]; $h.templateParameters[0].enclosingTemplateOffset = -1 } }
    @{ Name = 'false static parser repair'; Mutate = { param($v) $v.raw.staticParserNameRepairs[0].correctedName = 'cv.cudev.fake' } }
    @{ Name = 'lost source build gate'; Mutate = { param($v) $v.map.declarations[0].buildCondition = 'HAVE_CUDA' } }
    @{ Name = 'false host shuffle visibility'; Mutate = { param($v) $v.raw.parserConfigurationProbes[0].headerDeclarationCounts.'warp/shuffle.hpp' = 25 } }
    @{ Name = 'false implemented callable'; Mutate = { param($v) $v.map.declarations[1].classification = 'implemented' } }
    @{ Name = 'false native wrapper'; Mutate = { param($v) $v.map.declarations[0].nativeEntrypoints = @('jyppx_ocv_cudev_fake') } }
    @{ Name = 'false managed wrapper'; Mutate = { param($v) $v.map.declarations[0].managedMembers = @('JYPPX.OpenCvSharp.Cudev.Fake') } }
    @{ Name = 'false complete callable coverage'; Mutate = { param($v) $v.summary.completeCallableCoverageClaimed = $true } }
    @{ Name = 'false CUDA execution'; Mutate = { param($v) $v.raw.cudaExecutionVerified = $true } }
)
foreach ($fixture in $fixtures) {
    $value = @{ raw = $raw; summary = $summary; map = $map; extension = $extension } | ConvertTo-Json -Depth 35 | ConvertFrom-Json
    & $fixture.Mutate $value
    $rejected = $false
    try { Assert-CudevData $value.raw $value.summary $value.map $value.extension } catch { $rejected = $true }
    if (-not $rejected) { throw "Cudev contract accepted negative fixture: $($fixture.Name)" }
}
$sourceFixtureCount = & (Join-Path $PSScriptRoot 'Test-CudevSourceCallables.ps1') -RepositoryRoot $repo -SourceCache $sourceCache -Extension $extension -Raw $raw -Summary $summary
if ($fixtures.Count + $sourceFixtureCount -ne [int]$summary.negativeFixtureCount) { throw 'Cudev negative fixture count drifted.' }
Write-Host "CUDEV_UPSTREAM_MAP_CONTRACT_OK parserDeclarations=144 parserCallable=110 omitted=110 missing=0 headers=76 templates=1100 macroDefinitions=49 macroInvocations=1206 fixtures=$($fixtures.Count + $sourceFixtureCount) completeCallableCoverage=false cudaVerified=false"
