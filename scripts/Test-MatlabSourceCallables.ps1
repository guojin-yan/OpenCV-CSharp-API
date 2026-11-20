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
    & (Join-Path $PSScriptRoot 'Test-MatlabUpstreamMap.ps1') -RepositoryRoot $RepositoryRoot
    return
}
# The parent supplies independently checked lexical/source/branch witnesses.
# Checked-in CST evidence needs neither Python nor a MATLAB installation.
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$syntaxPath = Join-Path $repo 'compatibility/matlab-source-callables.json'
$syntax = Get-Content -LiteralPath $syntaxPath -Raw | ConvertFrom-Json
$expectedCounts = [ordered]@{
    callableDeclarationCount = 242; directCallableCount = 242; templateContextCallableCount = 39
    definitionCount = 238; declarationOnlyCount = 4; friendFunctionCount = 1; constructorInitializerCount = 10; conditionalScopeBoundaryCount = 1
    reconciledParserCallableCount = 185; declarationErrorCount = 0; bodyParseDiagnosticCount = 1
}
$syntaxHash = (Get-FileHash -LiteralPath $syntaxPath -Algorithm SHA256).Hash.ToLowerInvariant()
foreach ($link in @($Extension.sourceCallableInventory, $Summary.sourceCallableInventory)) {
    if ([string]$link.path -cne 'compatibility/matlab-source-callables.json' -or [string]$link.sha256 -cne $syntaxHash) { throw 'MATLAB source callable hash chain drifted.' }
    foreach ($key in $expectedCounts.Keys) { if ([int]$link.counts.$key -ne $expectedCounts[$key]) { throw "MATLAB linked source callable count drifted: $key" } }
}
if ([string]$Summary.coverageStatus -cne 'parser-subset-with-expanded-source-callable-inventory') { throw 'MATLAB source callable coverage status drifted.' }
$syntaxBytes = [IO.File]::ReadAllBytes($syntaxPath)
if ($syntaxBytes[-1] -ne 10 -or $syntaxBytes -contains 13 -or ($syntaxBytes[0] -eq 239 -and $syntaxBytes[1] -eq 187 -and $syntaxBytes[2] -eq 191)) { throw 'MATLAB syntax evidence must use canonical UTF-8/LF bytes.' }
[void][Text.UTF8Encoding]::new($false, $true).GetString($syntaxBytes)

function Get-MatlabBytesText([byte[]]$Bytes, [int]$Start, [int]$End) {
    if ($Start -lt 0 -or $End -lt $Start -or $End -gt $Bytes.Length) { throw 'MATLAB syntax byte extent is invalid.' }
    [Text.UTF8Encoding]::new($false, $true).GetString($Bytes, $Start, $End - $Start)
}
function Get-MatlabConditions([object]$Source, [int]$Offset) {
    $conditions = @()
    foreach ($event in $Source.Events) { if ([int]$event.End -le $Offset) { $conditions = @($event.Conditions) } else { break } }
    $conditions
}
function Get-MatlabParameters([string]$Text) {
    if (-not $Text.StartsWith('(') -or -not $Text.EndsWith(')')) { throw 'MATLAB parameter witness lost parentheses.' }
    $body = $Text.Substring(1, $Text.Length - 2).Trim()
    if ($body -eq '' -or $body -eq 'void') { return }
    $code = Mask-Text $body
    $groups = [Collections.Generic.List[char]]::new()
    $start = 0
    for ($i = 0; $i -lt $code.Length; $i++) {
        $char = $code[$i]
        if ($char -in @('(', '[', '{', '<')) { $groups.Add(@{ '(' = ')'; '[' = ']'; '{' = '}'; '<' = '>' }[[string]$char]) }
        elseif ($groups.Count -gt 0 -and $char -eq $groups[$groups.Count - 1]) { $groups.RemoveAt($groups.Count - 1) }
        elseif ($char -eq ',' -and $groups.Count -eq 0) { $body.Substring($start, $i - $start).Trim(); $start = $i + 1 }
    }
    if ($groups.Count -ne 0) { throw 'MATLAB parameter witness has unbalanced groups.' }
    $body.Substring($start).Trim()
}

$prepared = @{}
foreach ($header in $Extension.headers) {
    $source = $SourceCache[[string]$header.path]
    $normalized = $source.Text.ToCharArray()
    $directives = @([regex]::Matches((Mask-Text $source.Text $false), $directivePattern))
    foreach ($directive in $directives) {
        for ($i = $directive.Index; $i -lt $directive.Index + $directive.Length; $i++) { if ($normalized[$i] -ne "`n") { $normalized[$i] = ' ' } }
    }
    $prepared[[string]$header.path] = @{ Source = $source; Bytes = [Text.Encoding]::UTF8.GetBytes($source.Text)
        Sha256 = (Hash-Text $source.Text); NormalizedSha256 = (Hash-Text (-join $normalized)); DirectiveCount = $directives.Count }
}

function Assert-MatlabSyntax([object]$Data) {
    if ([int]$Data.schemaVersion -ne 1 -or [string]$Data.upstreamOpenCvVersion -cne '5.0.0' -or [string]$Data.generator -cne 'tools/MatlabUpstreamMap/extract_matlab_callables.py' -or [string]$Data.syntaxParser.treeSitterVersion -cne '0.25.2' -or [string]$Data.syntaxParser.cppGrammarVersion -cne '0.23.4' -or [int]$Data.syntaxParser.languageAbi -ne 14) { throw 'MATLAB source syntax parser identity drifted.' }
    foreach ($flag in @('completeCallableCoverageClaimed','concreteTemplateInstantiationsClaimed','externalDeclarationClosureClaimed','generatedBindingSurfaceIncluded','matlabExecutionVerified')) { if ([bool]$Data.$flag) { throw "MATLAB syntax evidence overstates coverage: $flag" } }
    if ([string]$Data.reviewDisposition -cne 'intentionally-omitted-unwrapped-matlab-source' -or [string]$Data.sourceInventoryPath -cne 'compatibility/matlab-source-reviewed-extensions.json' -or [string]$Data.limitation -notmatch 'uninstantiated source function declaration occurrences' -or [string]$Data.limitation -notmatch 'C\+\+ type checking' -or [string]$Data.limitation -notmatch 'MATLAB/MEX execution remain outside') { throw 'MATLAB syntax inventory lost its scope limitations.' }
    foreach ($key in $expectedCounts.Keys) { if ([int]$Data.counts.$key -ne $expectedCounts[$key]) { throw "MATLAB source callable count drifted: $key" } }
    if ((@($Data.headers.path) -join '|') -cne (@($Extension.headers.path) -join '|')) { throw 'MATLAB source callable inventory lost/reordered a header.' }
    $counts, $byOrdinal, $parameterCache = @{}, @{}, @{}
    foreach ($key in $expectedCounts.Keys) { $counts[$key] = 0 }
    $ordinal = 0
    $headerCounts = @(164,3,71,4)
    for ($headerIndex = 0; $headerIndex -lt 4; $headerIndex++) {
        $header = $Data.headers[$headerIndex]
        $cache = $prepared[[string]$header.path]
        if ([string]$header.sha256 -cne $cache.Sha256 -or [string]$header.normalizedInputSha256 -cne $cache.NormalizedSha256 -or [int]$header.directiveCount -ne $cache.DirectiveCount -or [string]$header.normalization -cne 'blank-preprocessor-directives-retain-all-branches' -or [int]$header.declarationErrorCount -ne 0 -or @($header.callables).Count -ne $headerCounts[$headerIndex]) { throw 'MATLAB source/normalized syntax input or per-header partition drifted.' }
        $contextInfo = [Collections.Generic.List[object]]::new()
        $conditionalScopes = [Collections.Generic.List[object]]::new()
        foreach ($context in $header.contexts) {
            $value = Compact-Text (Get-MatlabBytesText $cache.Bytes ([int]$context.valueStartByte) ([int]$context.valueEndByte))
            $before = Compact-Text (Get-MatlabBytesText $cache.Bytes ([int]$context.startByte) ([int]$context.valueStartByte))
            if ($value -cne [string]$context.value -or [int]$context.valueEndByte -gt [int]$context.bodyStartByte -or [int]$context.bodyStartByte -ge [int]$context.endByte) { throw 'MATLAB namespace/class/template source witness drifted.' }
            $access = [Collections.Generic.List[object]]::new()
            if ([string]$context.kind -ceq 'template_declaration') {
                if ($before -cne 'template' -or -not $value.StartsWith('<') -or -not $value.EndsWith('>') -or [int]$context.bodyStartByte -ne [int]$context.valueEndByte) { throw 'MATLAB template context source witness drifted.' }
            } elseif ([string]$context.kind -cin @('namespace_definition','class_specifier','struct_specifier')) {
                $keyword = @{ namespace_definition = 'namespace'; class_specifier = 'class'; struct_specifier = 'struct' }[[string]$context.kind]
                if ($before -cne $keyword -or (Get-MatlabBytesText $cache.Bytes ([int]$context.bodyStartByte) ([int]$context.bodyStartByte + 1)) -cne '{' -or (Get-MatlabBytesText $cache.Bytes ([int]$context.endByte - 1) ([int]$context.endByte)) -cne '}') { throw 'MATLAB scope body witness drifted.' }
                $startChar = (Get-MatlabBytesText $cache.Bytes 0 ([int]$context.bodyStartByte)).Length
                $endChar = (Get-MatlabBytesText $cache.Bytes 0 ([int]$context.endByte)).Length
                $depth = 0
                foreach ($token in [regex]::Matches($cache.Source.Code.Substring($startChar, $endChar - $startChar), '[{}]|\b(?:public|private|protected)\s*:')) {
                    if ($token.Value -ceq '{') { $depth++ } elseif ($token.Value -ceq '}') { $depth-- } elseif ($depth -eq 1) { $access.Add(@{ Offset = $startChar + $token.Index; Value = $token.Value.TrimEnd(':').Trim() }) }
                    if ($depth -eq 0 -and $token.Index -ne $endChar - $startChar - 1) { throw 'MATLAB scope crosses its closing brace.' }
                }
                if ($depth -ne 0) { throw 'MATLAB source scope has unbalanced braces.' }
                $openChar = (Get-MatlabBytesText $cache.Bytes 0 ([int]$context.startByte)).Length
                $openConditions = @(Get-MatlabConditions $cache.Source $openChar)
                $closeConditions = @(Get-MatlabConditions $cache.Source ($endChar - 1))
                if (($openConditions -join '|') -cne ($closeConditions -join '|')) {
                    $conditionalScopes.Add(@{ Index = $contextInfo.Count; Opening = $openConditions; Closing = $closeConditions })
                }
            } else { throw 'Unknown MATLAB source context kind.' }
            $contextInfo.Add(@{ Row = $context; Access = $access.ToArray() })
        }
        if (@($header.conditionalScopeBoundaries).Count -ne $conditionalScopes.Count) { throw 'MATLAB conditional namespace/class boundary observation disappeared.' }
        for ($i = 0; $i -lt $conditionalScopes.Count; $i++) {
            $actual, $expected = $header.conditionalScopeBoundaries[$i], $conditionalScopes[$i]
            if ([int]$actual.contextIndex -ne $expected.Index -or (@($actual.openingConditions) -join '|') -cne ($expected.Opening -join '|') -or (@($actual.closingConditions) -join '|') -cne ($expected.Closing -join '|')) { throw 'MATLAB conditional scope boundary source conditions drifted.' }
        }
        $counts.conditionalScopeBoundaryCount += $conditionalScopes.Count
        $previousStart, $definitions, $conversions = -1, 0, 0
        $bodies = [Collections.Generic.List[object]]::new()
        foreach ($row in $header.callables) {
            $start, $end = [int]$row.expandedStartByte, [int]$row.expandedEndByte
            if ([int]$row.ordinal -ne $ordinal -or $start -le $previousStart -or $end -le $start) { throw 'MATLAB callable source order/ordinal drifted.' }
            $previousStart = $start
            $head = Get-MatlabBytesText $cache.Bytes $start $end
            if ((Compact-Text $head) -cne [string]$row.head -or (Hash-Text $head) -cne [string]$row.headSha256 -or [int]$row.declaratorStartByte -lt $start -or [int]$row.declaratorEndByte -gt $end -or [int]$row.parametersStartByte -lt [int]$row.declaratorStartByte -or [int]$row.parametersEndByte -gt [int]$row.declaratorEndByte) { throw 'MATLAB callable head/declarator source witness drifted.' }
            $startChar = (Get-MatlabBytesText $cache.Bytes 0 $start).Length
            if ([string]$row.origin.kind -cne 'direct' -or [int]$row.origin.startOffset -ne $startChar -or [int]$row.origin.endOffset -ne $startChar + $head.Length -or [int]$row.origin.line -ne ([regex]::Matches($cache.Source.Text.Substring(0,$startChar), '\n').Count + 1) -or (@($row.conditions) -join '|') -cne (@(Get-MatlabConditions $cache.Source $startChar) -join '|')) { throw 'MATLAB callable source/line/branch provenance drifted.' }
            $declarator = Compact-Text (Get-MatlabBytesText $cache.Bytes ([int]$row.declaratorStartByte) ([int]$row.declaratorEndByte))
            $parameters = @(Get-MatlabParameters (Compact-Text (Get-MatlabBytesText $cache.Bytes ([int]$row.parametersStartByte) ([int]$row.parametersEndByte))))
            $variadic = @($parameters | Where-Object { (Mask-Text ($_ -split '=',2)[0]) -match '\.\.\.' }).Count -gt 0
            if ($declarator -cne [string]$row.declarator -or $parameters.Count -ne [int]$row.parameterCount -or [bool]$row.variadic -ne $variadic) { throw 'MATLAB callable parameter/declarator metadata drifted.' }
            $parameterCache[$ordinal] = $parameters
            $scopes, $namespaces, $templates = [Collections.Generic.List[string]]::new(), [Collections.Generic.List[string]]::new(), [Collections.Generic.List[string]]::new()
            $containing = [Collections.Generic.List[int]]::new()
            $memberAccess = 'namespace'
            for ($i = 0; $i -lt $contextInfo.Count; $i++) {
                $info = $contextInfo[$i]; $context = $info.Row
                if ([int]$context.startByte -gt $start -or [int]$context.endByte -lt $end) { continue }
                $containing.Add($i)
                if ([string]$context.kind -ceq 'template_declaration') { $templates.Add([string]$context.value) } else {
                    $scopes.Add([string]$context.value)
                    if ([string]$context.kind -ceq 'namespace_definition') { $namespaces.Add([string]$context.value) }
                    $memberAccess = if ([string]$context.kind -ceq 'class_specifier') { 'private' } elseif ([string]$context.kind -ceq 'struct_specifier') { 'public' } else { 'namespace' }
                    foreach ($event in $info.Access) { if ($event.Offset -lt $startChar) { $memberAccess = $event.Value } }
                }
            }
            $name = Compact-Text (Get-MatlabBytesText $cache.Bytes ([int]$row.declaratorStartByte) ([int]$row.parametersStartByte))
            $name = $name -replace '^[*&\s]+', '' -replace '(^|::)operator\s*', '${1}operator '
            $prefix = Get-MatlabBytesText $cache.Bytes $start ([int]$row.declaratorStartByte)
            $storage = @([regex]::Matches($prefix,'\b(?:friend|static|inline|virtual|explicit)\b') | ForEach-Object Value)
            $friend = $storage -ccontains 'friend'
            $qualified = ((@($(if ($friend) { $namespaces } else { $scopes })) + @($name)) -join '::')
            if ([string]$row.lexicalQualifiedName -cne ((@($scopes) + @($name)) -join '::') -or [string]$row.qualifiedName -cne $qualified -or [bool]$row.friendDeclared -ne $friend -or (@($row.contextIndices) -join '|') -cne (@($containing) -join '|') -or (@($row.templateContext) -join '|') -cne (@($templates) -join '|') -or [string]$row.access -cne $memberAccess -or (@($row.storageSpecifiers) -join '|') -cne ($storage -join '|') -or [bool]$row.methodConst -ne ($declarator -match '\)\s*const\s*$')) { throw "MATLAB callable owner/template/access/qualifier metadata drifted: $($row.qualifiedName)" }
            if ($friend) { $counts.friendFunctionCount++ }
            if ($templates.Count -gt 0) { $counts.templateContextCallableCount++ }
            if ($name -match '(?:^|::)operator (?:const\b|[A-Za-z_]\w*)') { $conversions++ }
            if ([string]$row.kind -ceq 'definition') {
                if ($null -eq $row.definitionBody) { throw 'MATLAB definition lost its function body witness.' }
                $body = $row.definitionBody
                $bodyText = Get-MatlabBytesText $cache.Bytes ([int]$body.startByte) ([int]$body.endByte)
                if ([int]$body.startByte -lt $end -or -not $bodyText.StartsWith('{') -or -not $bodyText.EndsWith('}') -or [string]$body.sha256 -cne (Hash-Text $bodyText)) { throw 'MATLAB definition body source witness drifted.' }
                $tail = Get-MatlabBytesText $cache.Bytes $end ([int]$body.startByte)
                if ($tail.Trim() -cne '') {
                    $init = $row.constructorInitializer
                    if ($null -eq $init -or -not $tail.TrimStart().StartsWith(':') -or [int]$init.startByte -ne $end -or [int]$init.endByte -ne [int]$body.startByte -or [string]$init.text -cne (Compact-Text $tail) -or [string]$init.sha256 -cne (Hash-Text $tail)) { throw 'MATLAB constructor initializer source witness drifted.' }
                    $counts.constructorInitializerCount++
                } elseif ($null -ne $row.constructorInitializer) { throw 'MATLAB callable invented a constructor initializer.' }
                $bodies.Add($body)
                $definitions++; $counts.definitionCount++
            } elseif ([string]$row.kind -cne 'declaration' -or $null -ne $row.definitionBody -or $null -ne $row.constructorInitializer -or -not $head.TrimEnd().EndsWith(';')) { throw 'MATLAB function declaration/definition partition drifted.' }
            else { $counts.declarationOnlyCount++ }
            $byOrdinal[$ordinal] = @{ Row = $row; Header = [string]$header.path; Cache = $cache; Prefix = $prefix; Name = $name }
            $ordinal++
        }
        if ([int]$header.syntaxNodeCounts.surfaceFunctionDefinitionCount -ne $definitions -or [int]$header.syntaxNodeCounts.surfaceConversionDeclaratorCount -ne $conversions -or [int]$header.syntaxNodeCounts.surfaceFunctionDeclaratorCount + $conversions -ne @($header.callables).Count -or [int]$header.syntaxNodeCounts.excludedTypeDeclaratorCount -ne 0 -or [int]$header.syntaxNodeCounts.bodyFunctionDeclaratorCount -ne @(1,0,0,0)[$headerIndex] -or [int]$header.syntaxNodeCounts.bodyAbstractFunctionDeclaratorCount -ne 0) { throw 'MATLAB source function syntax role partition drifted.' }
        foreach ($diagnostic in $header.bodyParseDiagnostics) {
            $text = Get-MatlabBytesText $cache.Bytes ([int]$diagnostic.expandedStartByte) ([int]$diagnostic.expandedEndByte)
            $context = Compact-Text (Get-MatlabBytesText $cache.Bytes ([int]$diagnostic.contextStartByte) ([int]$diagnostic.contextEndByte))
            if ([string]$diagnostic.kind -cne 'ERROR' -or [bool]$diagnostic.missing -or $text -cne 'char*' -or -not ([string]$header.path).EndsWith('/mxarray.hpp') -or $context -notmatch 'va_arg\(opt, const char\*\)' -or [string]$diagnostic.sourceSha256 -cne (Hash-Text $text) -or [string]$diagnostic.text -cne (Compact-Text $text) -or [string]$diagnostic.context -cne $context -or @($bodies | Where-Object { [int]$_.startByte -le [int]$diagnostic.expandedStartByte -and [int]$_.endByte -ge [int]$diagnostic.expandedEndByte }).Count -ne 1) { throw 'MATLAB function body diagnostic source/containment drifted.' }
        }
        $counts.bodyParseDiagnosticCount += @($header.bodyParseDiagnostics).Count
    }
    $parserRows = @($Raw.declarations | Where-Object kind -CEQ callable)
    if (@($Data.parserCallableReconciliation).Count -ne $parserRows.Count) { throw 'MATLAB source reconciliation lost a parser callable.' }
    for ($i = 0; $i -lt $parserRows.Count; $i++) {
        $decl, $reconciliation = $parserRows[$i], $Data.parserCallableReconciliation[$i]
        $expectedOccurrences = if ([int]$decl.ordinal -in @(9,10)) { 2 } else { 1 }
        if (@($reconciliation.sourceOrdinals).Count -ne $expectedOccurrences) { throw 'MATLAB parser/source declaration and inline definition closure drifted.' }
        if ([int]$reconciliation.parserOrdinal -ne [int]$decl.ordinal -or [string]$reconciliation.identity -cne [string]$decl.identity -or @($reconciliation.sourceOrdinals).Count -lt 1 -or @($reconciliation.sourceOrdinals | Sort-Object -Unique).Count -ne @($reconciliation.sourceOrdinals).Count) { throw 'MATLAB parser/source reconciliation identity drifted.' }
        foreach ($sourceOrdinal in $reconciliation.sourceOrdinals) {
            if (-not $byOrdinal.ContainsKey([int]$sourceOrdinal)) { throw 'MATLAB parser reconciliation references no source callable.' }
            $value = $byOrdinal[[int]$sourceOrdinal]; $row = $value.Row
            $name = ([string]$row.qualifiedName -replace 'operator\s+([=\[\]()+*/!<>&|^-])','operator$1')
            if ($name.Contains('::operator ')) { $parts = $name -split '::operator ',2; $name = $parts[0].Replace('::','.') + '.operator ' + $parts[1] } else { $name = $name.Replace('::','.') }
            if ($value.Header -cne [string]$decl.sourceHeader -or $name -cne [string]$decl.name -or @($row.templateContext).Count -ne 0 -or [bool]$row.friendDeclared -or [int]$row.parameterCount -ne @($decl.arguments).Count) { throw 'MATLAB parser/source owner or arity drifted.' }
            for ($a = 0; $a -lt @($decl.arguments).Count; $a++) {
                $arg = $decl.arguments[$a]; $expected = [string]$arg.type + ' ' + [string]$arg.name
                if ([string]$arg.default -cne '') { $expected += '=' + [string]$arg.default }
                if (([string]$parameterCache[[int]$sourceOrdinal][$a] -replace '\s+','') -cne ($expected -replace '\s+','') -or @($arg.modifiers).Count -ne 0) { throw 'MATLAB parser/source parameter signature drifted.' }
            }
            $return = $value.Prefix -replace '\b(?:inline|static|explicit|virtual)\b',''
            $leading = [regex]::Match([string]$row.declarator,'^[*&\s]+').Value
            $return += $leading
            if (($return -replace '\s+','') -cne ([string]$decl.returnType -replace '\s+','') -or (@($decl.modifiers) -ccontains '/S') -ne (@($row.storageSpecifiers) -ccontains 'static') -or (@($decl.modifiers) -ccontains '/C') -ne [bool]$row.methodConst) { throw "MATLAB parser/source return/static/const signature drifted: $($decl.identity)" }
        }
    }
    $counts.callableDeclarationCount = $ordinal; $counts.directCallableCount = $ordinal; $counts.reconciledParserCallableCount = $parserRows.Count
    foreach ($key in $expectedCounts.Keys) { if ($counts[$key] -ne $expectedCounts[$key]) { throw "MATLAB source callable measured partition drifted: $key" } }
}

Assert-MatlabSyntax $syntax
$fixtures = @(
    @{ Name = 'lost source callable'; Mutate = { param($v) $v.headers[1].callables = @($v.headers[1].callables | Select-Object -Skip 1) } }
    @{ Name = 'false callable head'; Mutate = { param($v) $v.headers[0].callables[0].head = 'int fake()' } }
    @{ Name = 'shifted source origin'; Mutate = { param($v) $v.headers[0].callables[0].origin.startOffset++ } }
    @{ Name = 'lost inactive fallback condition'; Mutate = { param($v) $v.headers[1].callables[0].conditions = @() } }
    @{ Name = 'lost class template context'; Mutate = { param($v) $v.headers[1].callables[0].templateContext = @() } }
    @{ Name = 'friend mistaken for member'; Mutate = { param($v) $r = @($v.headers[2].callables | Where-Object friendDeclared)[0]; $r.qualifiedName = $r.lexicalQualifiedName } }
    @{ Name = 'lost friend qualifier'; Mutate = { param($v) $r = @($v.headers[2].callables | Where-Object friendDeclared)[0]; $r.storageSpecifiers = @() } }
    @{ Name = 'damaged constructor initializer'; Mutate = { param($v) $r = @($v.headers[2].callables | Where-Object { $null -ne $_.constructorInitializer })[0]; $r.constructorInitializer.sha256 = '0' * 64 } }
    @{ Name = 'lost method const'; Mutate = { param($v) $v.headers[1].callables[2].methodConst = $false } }
    @{ Name = 'false parameter count'; Mutate = { param($v) $v.headers[1].callables[0].parameterCount++ } }
    @{ Name = 'same-arity wrong overload reconciliation'; Mutate = { param($v) $a = @($v.parserCallableReconciliation | Where-Object parserOrdinal -EQ 9)[0]; $b = @($v.parserCallableReconciliation | Where-Object parserOrdinal -EQ 15)[0]; $a.sourceOrdinals = @($b.sourceOrdinals) } }
    @{ Name = 'hidden body diagnostic'; Mutate = { param($v) $v.headers[2].bodyParseDiagnostics = @() } }
    @{ Name = 'lost reconciled inline definition'; Mutate = { param($v) $r = @($v.parserCallableReconciliation | Where-Object parserOrdinal -EQ 9)[0]; $r.sourceOrdinals = @($r.sourceOrdinals[0]) } }
    @{ Name = 'erased conditional namespace closure'; Mutate = { param($v) $v.headers[1].conditionalScopeBoundaries = @() } }
    @{ Name = 'false concrete instantiation'; Mutate = { param($v) $v.concreteTemplateInstantiationsClaimed = $true } }
    @{ Name = 'false external include closure'; Mutate = { param($v) $v.externalDeclarationClosureClaimed = $true } }
    @{ Name = 'false generated gateway coverage'; Mutate = { param($v) $v.generatedBindingSurfaceIncluded = $true } }
    @{ Name = 'false MATLAB runtime verification'; Mutate = { param($v) $v.matlabExecutionVerified = $true } }
)
$syntaxJson = $syntax | ConvertTo-Json -Depth 40 -Compress
foreach ($fixture in $fixtures) {
    $value = $syntaxJson | ConvertFrom-Json
    & $fixture.Mutate $value
    $rejected = $false
    try { Assert-MatlabSyntax $value } catch { $rejected = $true }
    if (-not $rejected) { throw "MATLAB source callable guard accepted negative fixture: $($fixture.Name)" }
}
Write-Host "MATLAB_SOURCE_CALLABLE_CONTRACT_OK declarations=242 templates=39 definitions=238 reconciled=185 friendFunctions=1 initializers=10 declarationErrors=0 bodyDiagnostics=1 fixtures=$($fixtures.Count)"
return $fixtures.Count
