param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,
    [switch]$Check,
    [string]$PythonPath = '',
    [string]$SyntaxParserRoot = ''
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$workspace = (Resolve-Path -LiteralPath (Join-Path $repo '..')).Path
if (-not $Check) {
    if ([string]::IsNullOrWhiteSpace($PythonPath)) { throw 'Cudev raw/source inventory extraction requires -PythonPath; use Test-CudevUpstreamMap.ps1 for checked-in evidence.' }
    & $PythonPath (Join-Path $repo 'tools/CudevUpstreamMap/extract_cudev.py') --workspace $workspace --opencv-root (Join-Path $workspace 'opencv-source/opencv-5.0.0') --output (Join-Path $repo 'compatibility/cudev-upstream-raw.json')
    if ($LASTEXITCODE -ne 0) { throw 'Cudev raw/source inventory extraction failed.' }
    if ([string]::IsNullOrWhiteSpace($SyntaxParserRoot)) {
        $SyntaxParserRoot = & (Join-Path $PSScriptRoot 'Initialize-CudevSyntaxParser.ps1') -RepositoryRoot $repo -PythonPath $PythonPath
    }
    & $PythonPath (Join-Path $repo 'tools/CudevUpstreamMap/extract_source_callables.py') --workspace $workspace --output (Join-Path $repo 'compatibility/cudev-source-callables.json') --syntax-parser-root $SyntaxParserRoot
    if ($LASTEXITCODE -ne 0) { throw 'Cudev source callable extraction failed.' }
}
& (Join-Path $PSScriptRoot 'Generate-ContribUpstreamMap.ps1') -Module cudev -DisplayName Cudev -RepositoryRoot $repo -Check:$Check
