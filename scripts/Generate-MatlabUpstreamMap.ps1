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
    if ([string]::IsNullOrWhiteSpace($PythonPath)) { throw 'MATLAB extraction requires -PythonPath; use Test-MatlabUpstreamMap.ps1 for checked-in evidence.' }
    & $PythonPath (Join-Path $repo 'tools/MatlabUpstreamMap/extract_matlab.py') --workspace $workspace --opencv-root (Join-Path $workspace 'opencv-source/opencv-5.0.0') --output (Join-Path $repo 'compatibility/matlab-upstream-raw.json')
    if ($LASTEXITCODE -ne 0) { throw 'MATLAB extraction failed.' }
    if ([string]::IsNullOrWhiteSpace($SyntaxParserRoot)) {
        $SyntaxParserRoot = & (Join-Path $PSScriptRoot 'Initialize-CudevSyntaxParser.ps1') -RepositoryRoot $repo -PythonPath $PythonPath
    }
    & $PythonPath (Join-Path $repo 'tools/MatlabUpstreamMap/extract_matlab_callables.py') --workspace $workspace --output (Join-Path $repo 'compatibility/matlab-source-callables.json') --syntax-parser-root $SyntaxParserRoot
    if ($LASTEXITCODE -ne 0) { throw 'MATLAB source callable extraction failed.' }
}
& (Join-Path $PSScriptRoot 'Generate-ContribUpstreamMap.ps1') -Module matlab -DisplayName Matlab -RepositoryRoot $repo -Check:$Check
