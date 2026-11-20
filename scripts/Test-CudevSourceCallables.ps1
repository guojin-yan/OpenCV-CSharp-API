param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,
    [hashtable]$SourceCache,
    [object]$Extension,
    [object]$Raw,
    [object]$Summary
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
if ($null -eq $SourceCache) {
    & (Join-Path $PSScriptRoot 'Test-CudevUpstreamMap.ps1') -RepositoryRoot $RepositoryRoot
    return
}
# The parent independently validates the lexical inventory and supplies its
# source/branch cache. No Python, tree-sitter, CUDA SDK or device is used here.
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$syntaxPath = Join-Path $repo 'compatibility/cudev-source-callables.json'
$syntax = Get-Content -LiteralPath $syntaxPath -Raw | ConvertFrom-Json
$syntaxHash = (Get-FileHash -LiteralPath $syntaxPath -Algorithm SHA256).Hash.ToLowerInvariant()
$expectedCounts = [ordered]@{
    callableDeclarationCount = 5433; directCallableCount = 1096; macroGeneratedCallableCount = 4337
    templateContextCallableCount = 1979; definitionCount = 5382; declarationOnlyCount = 51
    declarationMacroDefinitionCount = 44; expandedMacroInvocationCount = 1088
    nonCallableExpansionCount = 213
    reconciledParserCallableCount = 110; declarationErrorCount = 0; bodyParseDiagnosticCount = 28
}
foreach ($link in @($Extension.sourceCallableInventory, $Summary.sourceCallableInventory)) {
    if ([string]$link.path -cne 'compatibility/cudev-source-callables.json' -or [string]$link.sha256 -cne $syntaxHash) { throw 'Cudev source callable hash chain drifted.' }
    foreach ($key in $expectedCounts.Keys) { if ([int]$link.counts.$key -ne $expectedCounts[$key]) { throw "Cudev source callable linked count drifted: $key" } }
}
$syntaxBytes = [IO.File]::ReadAllBytes($syntaxPath)
if ($syntaxBytes[-1] -ne 10 -or $syntaxBytes -contains 13 -or ($syntaxBytes[0] -eq 239 -and $syntaxBytes[1] -eq 187 -and $syntaxBytes[2] -eq 191)) { throw 'Cudev syntax evidence must use canonical UTF-8/LF bytes.' }
[void][Text.UTF8Encoding]::new($false, $true).GetString($syntaxBytes)

function Get-CudevBytesText([byte[]]$Bytes, [int]$Start, [int]$End) {
    if ($Start -lt 0 -or $End -lt $Start -or $End -gt $Bytes.Length) { throw 'Cudev syntax byte extent is invalid.' }
    [Text.UTF8Encoding]::new($false, $true).GetString($Bytes, $Start, $End - $Start)
}
function Get-CudevParameters([string]$Text) {
    if (-not $Text.StartsWith('(') -or -not $Text.EndsWith(')')) { throw 'Cudev parameter witness lost parentheses.' }
    $body = $Text.Substring(1, $Text.Length - 2).Trim()
    if ($body -eq '' -or $body -eq 'void') { return }
    $groups = [Collections.Generic.List[char]]::new()
    $start = 0
    for ($i = 0; $i -lt $body.Length; $i++) {
        $char = $body[$i]
        if ($char -in @('(', '[', '{', '<')) {
            $groups.Add(@{ '(' = ')'; '[' = ']'; '{' = '}'; '<' = '>' }[[string]$char])
        } elseif ($groups.Count -gt 0 -and $char -eq $groups[$groups.Count - 1]) {
            $groups.RemoveAt($groups.Count - 1)
        } elseif ($char -eq ',' -and $groups.Count -eq 0) {
            $body.Substring($start, $i - $start).Trim()
            $start = $i + 1
        }
    }
    if ($groups.Count -ne 0) { throw 'Cudev parameter witness has an unbalanced group.' }
    $body.Substring($start).Trim()
}
function Test-CudevVariadic([string[]]$Parameters) {
    foreach ($parameter in $Parameters) {
        $groups = [Collections.Generic.List[char]]::new()
        for ($i = 0; $i -lt $parameter.Length; $i++) {
            $char = $parameter[$i]
            if ($groups.Count -eq 0 -and $char -eq '=') { break }
            if ($groups.Count -eq 0 -and $i + 2 -lt $parameter.Length -and $parameter.Substring($i, 3) -ceq '...') { return $true }
            if ($char -in @('(', '[', '{', '<')) { $groups.Add(@{ '(' = ')'; '[' = ']'; '{' = '}'; '<' = '>' }[[string]$char]) }
            elseif ($groups.Count -gt 0 -and $char -eq $groups[$groups.Count - 1]) { $groups.RemoveAt($groups.Count - 1) }
        }
    }
    return $false
}

$runtimeMacros = @('CV_CUDEV_SAFE_CALL','CV_CUDEV_DESCALE','__shfl','__shfl_xor','__shfl_down')
$definitions = @{}
foreach ($header in $Extension.headers) { foreach ($definition in $header.macroDefinitions) { $definitions[[string]$definition.id] = @{ Row = $definition; Header = [string]$header.path } } }
if (@($definitions.Values | Where-Object { $_.Row.name -cnotin $runtimeMacros }).Count -ne 44) { throw 'Cudev declaration-generator scope drifted.' }
$prepared = @{}
foreach ($header in $Extension.headers) {
    $source = $SourceCache[[string]$header.path]
    $builder = [Text.StringBuilder]::new()
    $segments, $expansions = [Collections.Generic.List[object]]::new(), [Collections.Generic.List[object]]::new()
    $cursor, $byteCursor = 0, 0
    foreach ($call in $header.macroInvocations) {
        if ([string]$call.name -cin $runtimeMacros) { continue }
        if (@($call.sourceDefinitionIds).Count -ne 1 -or -not $definitions.ContainsKey([string]$call.sourceDefinitionIds[0])) { throw 'Cudev declaration macro definition is ambiguous.' }
        $definition = $definitions[[string]$call.sourceDefinitionIds[0]]
        if ($definition.Header -cne [string]$header.path -or [string]$definition.Row.name -cne [string]$call.name) { throw 'Cudev declaration macro crossed a header/definition boundary.' }
        $arguments = @(([string]$call.arguments).Split(',') | ForEach-Object Trim)
        if ($arguments.Count -ne @($definition.Row.parameters).Count -or @($arguments | Where-Object { $_ -match '[()"'']' }).Count -gt 0) { throw 'Cudev declaration macro argument contract drifted.' }
        $lookup = [Collections.Generic.Dictionary[string,string]]::new([StringComparer]::Ordinal)
        for ($i = 0; $i -lt $arguments.Count; $i++) { $lookup[[string]$definition.Row.parameters[$i]] = $arguments[$i] }
        $replacement = [regex]::Replace([string]$definition.Row.replacement, '\b[A-Za-z_]\w*\b', [Text.RegularExpressions.MatchEvaluator]{ param($match)
            if ($lookup.ContainsKey($match.Value)) { return $lookup[$match.Value] }
            $match.Value
        })
        while ($replacement.Contains('##')) {
            $next = [regex]::Replace($replacement, '(\w+)\s*##\s*(\w+)', '$1$2')
            if ($next -ceq $replacement) { throw 'Unsupported Cudev declaration macro paste.' }
            $replacement = $next
        }
        $direct = $source.Text.Substring($cursor, [int]$call.startOffset - $cursor)
        $length = [Text.Encoding]::UTF8.GetByteCount($direct)
        $segments.Add(@{ Start = $byteCursor; End = $byteCursor + $length; SourceStart = $cursor; Expansion = -1 })
        [void]$builder.Append($direct)
        $byteCursor += $length
        $length = [Text.Encoding]::UTF8.GetByteCount($replacement)
        $segments.Add(@{ Start = $byteCursor; End = $byteCursor + $length; SourceStart = [int]$call.startOffset; Expansion = $expansions.Count })
        $expansions.Add(@{ Call = $call; Text = $replacement; Sha256 = (Hash-Text $replacement); DefinitionId = [string]$definition.Row.id })
        [void]$builder.Append($replacement)
        $byteCursor += $length
        $cursor = [int]$call.endOffset
    }
    $direct = $source.Text.Substring($cursor)
    $segments.Add(@{ Start = $byteCursor; End = $byteCursor + [Text.Encoding]::UTF8.GetByteCount($direct); SourceStart = $cursor; Expansion = -1 })
    [void]$builder.Append($direct)
    $expanded = $builder.ToString()
    $bytes = [Text.Encoding]::UTF8.GetBytes($expanded)
    $normalizations = [Collections.Generic.List[object]]::new()
    $normalized = $expanded
    foreach ($pair in @(
        @('sizeof(A) >= sizeof(B)', '(sizeof(A)>=sizeof(B))'),
        @('unsigned(VecTraits<A>::cn) != unsigned(VecTraits<B>::cn)', '((unsigned)VecTraits<A>::cn!=(unsigned)VecTraits<B>::cn)')
    )) {
        $count = [regex]::Matches($normalized, [regex]::Escape($pair[0])).Count
        if ($count -gt 0) {
            $normalizations.Add([ordered]@{ kind = 'equivalent-non-callable-trait-expression'; source = $pair[0]; replacement = $pair[1]; count = $count })
            $normalized = $normalized.Replace($pair[0], $pair[1])
        }
    }
    $markers = '\b(?:_CCCL_BEGIN_NAMESPACE_CUDA_STD|_CCCL_END_NAMESPACE_CUDA_STD|_LIBCUDACXX_BEGIN_NAMESPACE_STD|_LIBCUDACXX_END_NAMESPACE_STD)\b'
    $markerCount = [regex]::Matches($normalized, $markers).Count
    if ($markerCount -gt 0) {
        if (-not ([string]$header.path).EndsWith('/ptr2d/zip.hpp') -or $markerCount -ne 4) { throw 'Cudev external namespace normalization scope drifted.' }
        $normalizations.Add([ordered]@{ kind = 'external-namespace-markers-around-non-callable-tuple-traits'; count = $markerCount })
        $normalized = [regex]::Replace($normalized, $markers, [Text.RegularExpressions.MatchEvaluator]{ param($match) ' ' * $match.Length })
    }
    $normalized = [regex]::Replace($normalized, '\b(?:__host__|__device__|__global__|__forceinline__|__shared__|__constant__)\b', [Text.RegularExpressions.MatchEvaluator]{ param($match) ' ' * $match.Length })
    if ([Text.Encoding]::UTF8.GetByteCount($normalized) -ne $bytes.Length) { throw 'Cudev normalization moved byte offsets.' }
    $code = [regex]::Replace((Mask-Text $expanded), $directivePattern, [Text.RegularExpressions.MatchEvaluator]{ param($match) [regex]::Replace($match.Value, '[^\n]', ' ') })
    $prepared[[string]$header.path] = @{ Source = $source; Bytes = $bytes; Expanded = $expanded; Code = $code; Segments = $segments.ToArray();
        Expansions = $expansions.ToArray(); Normalizations = $normalizations.ToArray(); NormalizedSha256 = (Hash-Text $normalized) }
}

function Assert-CudevSyntax([object]$Data) {
    if ([int]$Data.schemaVersion -ne 1 -or [string]$Data.upstreamOpenCvVersion -cne '5.0.0' -or [string]$Data.generator -cne 'tools/CudevUpstreamMap/extract_source_callables.py' -or [string]$Data.syntaxParser.treeSitterVersion -cne '0.25.2' -or [string]$Data.syntaxParser.cppGrammarVersion -cne '0.23.4' -or [int]$Data.syntaxParser.languageAbi -ne 14) { throw 'Cudev syntax parser identity drifted.' }
    if (-not [bool]$Data.localDeclarationMacroExpansionClaimed -or [bool]$Data.concreteTemplateInstantiationsClaimed -or [bool]$Data.externalDeclarationClosureClaimed -or [bool]$Data.cudaExecutionVerified -or [string]$Data.reviewDisposition -cne 'intentionally-omitted-unwrapped-device-source' -or [string]$Data.sourceInventoryPath -cne 'compatibility/cudev-source-reviewed-extensions.json' -or [string]$Data.limitation -notmatch 'uninstantiated source declaration occurrences' -or [string]$Data.limitation -notmatch 'C\+\+ type checking, linking and CUDA execution remain outside') { throw 'Cudev syntax evidence overstates its source scope.' }
    foreach ($key in $expectedCounts.Keys) { if ([int]$Data.counts.$key -ne $expectedCounts[$key]) { throw "Cudev source callable count drifted: $key" } }
    if ((@($Data.headers.path) -join '|') -cne (@($Extension.headers.path) -join '|')) { throw 'Cudev syntax inventory lost/reordered an installed header.' }
    $counts = @{}
    foreach ($key in $expectedCounts.Keys) { $counts[$key] = 0 }
    $ordinal, $conversions, $bodyDeclarators, $bodyAbstract = 0, 0, 0, 0
    $byOrdinal, $parameterCache = @{}, @{}
    foreach ($header in $Data.headers) {
        $cache = $prepared[[string]$header.path]
        if ([string]$header.sha256 -cne [string]$cache.Source.Sha256 -or [string]$header.normalizedInputSha256 -cne [string]$cache.NormalizedSha256 -or [int]$header.declarationErrorCount -ne 0) { throw 'Cudev syntax input/source hash or declaration diagnostics drifted.' }
        if (($header.normalizations | ConvertTo-Json -Depth 8 -Compress) -cne ($cache.Normalizations | ConvertTo-Json -Depth 8 -Compress)) { throw 'Cudev byte-preserving normalizations drifted.' }
        if (@($header.macroExpansions).Count -ne $cache.Expansions.Count) { throw 'Cudev syntax inventory lost a declaration expansion.' }
        $expansionCounts = [int[]]::new($cache.Expansions.Count)
        for ($i = 0; $i -lt $cache.Expansions.Count; $i++) {
            $actual, $expected = $header.macroExpansions[$i], $cache.Expansions[$i]
            if ([string]$actual.name -cne [string]$expected.Call.name -or [string]$actual.definitionId -cne $expected.DefinitionId -or [int]$actual.startOffset -ne [int]$expected.Call.startOffset -or [int]$actual.endOffset -ne [int]$expected.Call.endOffset -or [string]$actual.sourceSha256 -cne [string]$expected.Call.sourceSha256 -or [string]$actual.expandedSha256 -cne $expected.Sha256 -or (@($actual.conditions) -join '|') -cne (@($expected.Call.conditions) -join '|')) { throw 'Cudev independent local declaration expansion drifted.' }
        }
        $contextInfo = [Collections.Generic.List[object]]::new()
        foreach ($context in $header.contexts) {
            $value = Compact-Text (Get-CudevBytesText $cache.Bytes ([int]$context.valueStartByte) ([int]$context.valueEndByte))
            $before = Compact-Text (Get-CudevBytesText $cache.Bytes ([int]$context.startByte) ([int]$context.valueStartByte))
            if ($value -cne [string]$context.value -or [int]$context.valueEndByte -gt [int]$context.bodyStartByte -or [int]$context.bodyStartByte -ge [int]$context.endByte) { throw 'Cudev namespace/class/template source witness drifted.' }
            $access = [Collections.Generic.List[object]]::new()
            if ([string]$context.kind -ceq 'template_declaration') {
                if ($before -cne 'template' -or -not $value.StartsWith('<') -or -not $value.EndsWith('>') -or [int]$context.bodyStartByte -ne [int]$context.valueEndByte) { throw 'Cudev template context witness drifted.' }
            } elseif ([string]$context.kind -cin @('namespace_definition','class_specifier','struct_specifier')) {
                $keyword = @{ namespace_definition = 'namespace'; class_specifier = 'class'; struct_specifier = 'struct' }[[string]$context.kind]
                if ($before -cne $keyword -or (Get-CudevBytesText $cache.Bytes ([int]$context.bodyStartByte) ([int]$context.bodyStartByte + 1)) -cne '{' -or (Get-CudevBytesText $cache.Bytes ([int]$context.endByte - 1) ([int]$context.endByte)) -cne '}') { throw 'Cudev scope body witness drifted.' }
                $startChar = (Get-CudevBytesText $cache.Bytes 0 ([int]$context.bodyStartByte)).Length
                $endChar = (Get-CudevBytesText $cache.Bytes 0 ([int]$context.endByte)).Length
                $depth = 0
                foreach ($token in [regex]::Matches($cache.Code.Substring($startChar, $endChar - $startChar), '[{}]|\b(?:public|private|protected)\s*:')) {
                    if ($token.Value -ceq '{') { $depth++ } elseif ($token.Value -ceq '}') { $depth-- } elseif ($depth -eq 1) { $access.Add(@{ Offset = $startChar + $token.Index; Value = $token.Value.TrimEnd(':').Trim() }) }
                    if ($depth -eq 0 -and $token.Index -ne $endChar - $startChar - 1) { throw 'Cudev scope extent crosses its closing brace.' }
                }
                if ($depth -ne 0) { throw 'Cudev scope brace witness is unbalanced.' }
            } else { throw 'Unknown Cudev source context kind.' }
            $contextInfo.Add(@{ Row = $context; Access = $access.ToArray() })
        }
        $previousStart, $headerDefinitions, $headerConversions = -1, 0, 0
        foreach ($row in $header.callables) {
            $start, $end = [int]$row.expandedStartByte, [int]$row.expandedEndByte
            if ([int]$row.ordinal -ne $ordinal -or $start -le $previousStart -or $end -le $start) { throw 'Cudev callable ordering/ordinal partition drifted.' }
            $previousStart = $start
            $head = Get-CudevBytesText $cache.Bytes $start $end
            if ((Compact-Text $head) -cne [string]$row.head -or (Hash-Text $head) -cne [string]$row.headSha256 -or [int]$row.declaratorStartByte -lt $start -or [int]$row.declaratorEndByte -gt $end -or [int]$row.parametersStartByte -lt [int]$row.declaratorStartByte -or [int]$row.parametersEndByte -gt [int]$row.declaratorEndByte) { throw 'Cudev callable head/declarator source evidence drifted.' }
            $declarator = Compact-Text (Get-CudevBytesText $cache.Bytes ([int]$row.declaratorStartByte) ([int]$row.declaratorEndByte))
            $parameters = @(Get-CudevParameters (Compact-Text (Get-CudevBytesText $cache.Bytes ([int]$row.parametersStartByte) ([int]$row.parametersEndByte))))
            if ($declarator -cne [string]$row.declarator -or $parameters.Count -ne [int]$row.parameterCount -or [bool]$row.variadic -ne (Test-CudevVariadic $parameters)) { throw "Cudev callable declarator/parameter metadata drifted: $($row.qualifiedName), parameters=$($parameters.Count)/$($row.parameterCount), declarator=$declarator" }
            $parameterCache[$ordinal] = $parameters
            $attrs = @([regex]::Matches($head, '\b(?:__host__|__device__|__global__|__forceinline__|__shared__|__constant__)\b') | ForEach-Object Value | Select-Object -Unique)
            if (($attrs -join '|') -cne (@($row.cudaAttributes) -join '|')) { throw 'Cudev callable CUDA attribute metadata drifted.' }
            $segment = $null
            foreach ($candidate in $cache.Segments) { if ($candidate.Start -le $start -and $start -lt $candidate.End) { $segment = $candidate; break } }
            if ($null -eq $segment -or $end -gt $segment.End) { throw 'Cudev callable crosses a declaration expansion boundary.' }
            if ($segment.Expansion -ge 0) {
                if ([string]$row.origin.kind -cne 'macro-expansion' -or [int]$row.origin.expansionIndex -ne $segment.Expansion -or [int]$row.origin.startOffset -ne $start - $segment.Start -or [int]$row.origin.endOffset -ne $end - $segment.Start) { throw 'Cudev expanded callable source provenance drifted.' }
                $conditions = @($cache.Expansions[$segment.Expansion].Call.conditions)
                $expansionCounts[$segment.Expansion]++
                $counts.macroGeneratedCallableCount++
            } else {
                $sourceStart = $segment.SourceStart + (Get-CudevBytesText $cache.Bytes $segment.Start $start).Length
                if ([string]$row.origin.kind -cne 'direct' -or [int]$row.origin.startOffset -ne $sourceStart -or [int]$row.origin.endOffset -ne $sourceStart + $head.Length -or $cache.Source.Text.Substring($sourceStart, $head.Length) -cne $head) { throw 'Cudev direct callable source provenance drifted.' }
                $conditions = @(Get-Conditions $cache.Source $sourceStart)
                $counts.directCallableCount++
            }
            if (($conditions -join '|') -cne (@($row.conditions) -join '|')) { throw 'Cudev callable source branch context drifted.' }
            $scopes, $templates = [Collections.Generic.List[string]]::new(), [Collections.Generic.List[string]]::new()
            $containing = [Collections.Generic.List[int]]::new()
            $memberAccess = 'namespace'
            $startChar = (Get-CudevBytesText $cache.Bytes 0 $start).Length
            for ($i = 0; $i -lt $contextInfo.Count; $i++) {
                $info = $contextInfo[$i]
                $context = $info.Row
                if ([int]$context.startByte -gt $start -or [int]$context.endByte -lt $end) { continue }
                $containing.Add($i)
                if ([string]$context.kind -ceq 'template_declaration') { $templates.Add([string]$context.value) } else {
                    $scopes.Add([string]$context.value)
                    $memberAccess = if ([string]$context.kind -ceq 'class_specifier') { 'private' } elseif ([string]$context.kind -ceq 'struct_specifier') { 'public' } else { 'namespace' }
                    foreach ($event in $info.Access) { if ($event.Offset -lt $startChar) { $memberAccess = $event.Value } }
                }
            }
            $name = Compact-Text (Get-CudevBytesText $cache.Bytes ([int]$row.declaratorStartByte) ([int]$row.parametersStartByte))
            $name = $name -replace '^[*&\s]+', '' -replace '(^|::)operator\s*', '${1}operator '
            if ([string]$row.qualifiedName -cne ((@($scopes) + @($name)) -join '::') -or (@($row.contextIndices) -join '|') -cne (@($containing) -join '|') -or (@($row.templateContext) -join '|') -cne (@($templates) -join '|') -or [string]$row.access -cne $memberAccess) { throw "Cudev callable name/template/access metadata drifted: $($row.qualifiedName)" }
            $kind = if ($head.TrimEnd().EndsWith(';') -and $head -notmatch '=\s*(?:default|delete)\s*;\s*$') { 'declaration' } else { 'definition' }
            if ([string]$row.kind -cne $kind) { throw 'Cudev definition/declaration distinction drifted.' }
            if ($kind -ceq 'definition') { $headerDefinitions++; $counts.definitionCount++ } else { $counts.declarationOnlyCount++ }
            if ($name -match '(?:^|::)operator (?:const\b|[A-Za-z_]\w*)') { $headerConversions++ }
            if ($templates.Count -gt 0) { $counts.templateContextCallableCount++ }
            $byOrdinal[$ordinal] = @{ Row = $row; Header = [string]$header.path; Cache = $cache }
            $ordinal++
        }
        for ($i = 0; $i -lt $expansionCounts.Count; $i++) { if ([int]$header.macroExpansions[$i].callableCount -ne $expansionCounts[$i]) { throw 'Cudev declaration expansion callable partition drifted.' } }
        if ([int]$header.syntaxNodeCounts.surfaceFunctionDefinitionCount -ne $headerDefinitions -or [int]$header.syntaxNodeCounts.surfaceConversionDeclaratorCount -ne $headerConversions -or [int]$header.syntaxNodeCounts.surfaceFunctionDeclaratorCount + $headerConversions -ne @($header.callables).Count -or [int]$header.syntaxNodeCounts.excludedTypeDeclaratorCount -ne 0) { throw 'Cudev surface function syntax partition drifted.' }
        $conversions += $headerConversions
        $bodyDeclarators += [int]$header.syntaxNodeCounts.bodyFunctionDeclaratorCount
        $bodyAbstract += [int]$header.syntaxNodeCounts.bodyAbstractFunctionDeclaratorCount
        foreach ($diagnostic in $header.bodyParseDiagnostics) {
            $text = Get-CudevBytesText $cache.Bytes ([int]$diagnostic.expandedStartByte) ([int]$diagnostic.expandedEndByte)
            $context = Compact-Text (Get-CudevBytesText $cache.Bytes ([int]$diagnostic.contextStartByte) ([int]$diagnostic.contextEndByte))
            if ((Hash-Text $text) -cne [string]$diagnostic.sourceSha256 -or (Compact-Text $text) -cne [string]$diagnostic.text -or $context -cne [string]$diagnostic.context) { throw 'Cudev function-body diagnostic source witness drifted.' }
            if ([bool]$diagnostic.missing) {
                if ([string]$diagnostic.kind -cne 'identifier' -or $text -cne '' -or $context -notmatch '<<<' -or $context -notmatch '>>>') { throw 'Cudev CUDA launch body diagnostic drifted.' }
            } elseif ([string]$diagnostic.kind -cne 'ERROR' -or $text -cne 'operator' -or -not ([string]$header.path).EndsWith('/ptr2d/texture.hpp') -or $context -notmatch 'texture->operator bool\(\)') { throw 'Cudev explicit conversion call body diagnostic drifted.' }
        }
        $counts.expandedMacroInvocationCount += @($header.macroExpansions).Count
        $counts.nonCallableExpansionCount += @($header.macroExpansions | Where-Object callableCount -EQ 0).Count
        $counts.bodyParseDiagnosticCount += @($header.bodyParseDiagnostics).Count
    }
    $counts.callableDeclarationCount = $ordinal
    $counts.declarationMacroDefinitionCount = 44
    $parserRows = @($Raw.declarations | Where-Object kind -CEQ callable)
    if (@($Data.parserCallableReconciliation).Count -ne $parserRows.Count) { throw 'Cudev source callable reconciliation lost a parser identity.' }
    for ($i = 0; $i -lt $parserRows.Count; $i++) {
        $decl, $reconciliation = $parserRows[$i], $Data.parserCallableReconciliation[$i]
        if ([int]$reconciliation.parserOrdinal -ne [int]$decl.ordinal -or [string]$reconciliation.identity -cne [string]$decl.identity -or @($reconciliation.sourceOrdinals).Count -lt 1 -or @($reconciliation.sourceOrdinals | Sort-Object -Unique).Count -ne @($reconciliation.sourceOrdinals).Count) { throw 'Cudev parser/source callable reconciliation identity drifted.' }
        foreach ($sourceOrdinal in $reconciliation.sourceOrdinals) {
            if (-not $byOrdinal.ContainsKey([int]$sourceOrdinal)) { throw 'Cudev parser reconciliation references no source callable.' }
            $value = $byOrdinal[[int]$sourceOrdinal]
            $row = $value.Row
            if ($value.Header -cne [string]$decl.sourceHeader -or ([string]$row.qualifiedName).Replace('::','.') -cne [string]$decl.name -or @($row.templateContext).Count -ne 0 -or [int]$row.parameterCount -ne @($decl.arguments).Count) { throw 'Cudev parser/source callable owner or arity drifted.' }
            for ($a = 0; $a -lt @($decl.arguments).Count; $a++) {
                $argument = $decl.arguments[$a]
                $expected = [string]$argument.type + ' ' + [string]$argument.name
                if ([string]$argument.default -cne '') { $expected += '=' + [string]$argument.default }
                if (([string]$parameterCache[[int]$sourceOrdinal][$a] -replace '\s+', '') -cne ($expected -replace '\s+', '') -or @($argument.modifiers).Count -ne 0) { throw 'Cudev parser/source callable parameter signature drifted.' }
            }
            $return = Get-CudevBytesText $value.Cache.Bytes ([int]$row.expandedStartByte) ([int]$row.declaratorStartByte)
            $return = $return -replace '\b(?:__host__|__device__|__global__|__forceinline__|__shared__|__constant__|inline|static|explicit)\b', ''
            if (($return -replace '\s+', '') -cne ([string]$decl.returnType -replace '\s+', '') -or (@($decl.modifiers) -ccontains '/S') -ne ([string]$row.head -match '\bstatic\b') -or (@($decl.modifiers) -ccontains '/C') -ne ([string]$row.declarator -match '\)\s*const\s*$')) { throw 'Cudev parser/source return type or static/const signature drifted.' }
        }
    }
    $counts.reconciledParserCallableCount = $parserRows.Count
    foreach ($key in $expectedCounts.Keys) { if ($counts[$key] -ne $expectedCounts[$key]) { throw "Cudev source callable partition drifted: $key" } }
    if ($conversions -ne 16 -or $bodyDeclarators -ne 27 -or $bodyAbstract -ne 12) { throw 'Cudev function syntax role inventory drifted.' }
}

Assert-CudevSyntax $syntax
$fixtures = @(
    @{ Name = 'lost source callable'; Mutate = { param($v) $v.headers[0].callables = @($v.headers[0].callables | Select-Object -Skip 1) } }
    @{ Name = 'false head'; Mutate = { param($v) $v.headers[0].callables[0].head = 'int fake()' } }
    @{ Name = 'shifted direct origin'; Mutate = { param($v) $v.headers[0].callables[0].origin.startOffset++ } }
    @{ Name = 'damaged local macro expansion'; Mutate = { param($v) $h = @($v.headers | Where-Object { $_.macroExpansions.Count -gt 0 })[0]; $h.macroExpansions[0].expandedSha256 = '0' * 64 } }
    @{ Name = 'lost declaration generator call'; Mutate = { param($v) $h = @($v.headers | Where-Object { $_.macroExpansions.Count -gt 0 })[0]; $h.macroExpansions = @($h.macroExpansions | Select-Object -Skip 1) } }
    @{ Name = 'false callable owner'; Mutate = { param($v) $v.headers[0].callables[0].qualifiedName = 'cv::fake::blockId' } }
    @{ Name = 'lost class template context'; Mutate = { param($v) $h = @($v.headers | Where-Object { @($_.callables | Where-Object { $_.templateContext.Count -gt 0 }).Count -gt 0 })[0]; $r = @($h.callables | Where-Object { $_.templateContext.Count -gt 0 })[0]; $r.templateContext = @() } }
    @{ Name = 'false access'; Mutate = { param($v) $v.headers[0].callables[0].access = 'private' } }
    @{ Name = 'false parameter count'; Mutate = { param($v) $v.headers[0].callables[0].parameterCount++ } }
    @{ Name = 'false parser reconciliation'; Mutate = { param($v) $v.parserCallableReconciliation[0].sourceOrdinals = @(1) } }
    @{ Name = 'hidden body parse diagnostic'; Mutate = { param($v) $h = @($v.headers | Where-Object { $_.bodyParseDiagnostics.Count -gt 0 })[0]; $h.bodyParseDiagnostics = @($h.bodyParseDiagnostics | Select-Object -Skip 1) } }
    @{ Name = 'false concrete instantiation'; Mutate = { param($v) $v.concreteTemplateInstantiationsClaimed = $true } }
)
$syntaxJson = $syntax | ConvertTo-Json -Depth 40 -Compress
foreach ($fixture in $fixtures) {
    $value = $syntaxJson | ConvertFrom-Json
    & $fixture.Mutate $value
    $rejected = $false
    try { Assert-CudevSyntax $value } catch { $rejected = $true }
    if (-not $rejected) { throw "Cudev source callable guard accepted negative fixture: $($fixture.Name)" }
}
Write-Host "CUDEV_SOURCE_CALLABLE_CONTRACT_OK declarations=5433 direct=1096 macroGenerated=4337 templates=1979 expansions=1088 reconciled=110 declarationErrors=0 bodyDiagnostics=28 fixtures=$($fixtures.Count)"
return $fixtures.Count
