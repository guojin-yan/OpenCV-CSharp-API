param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,
    [switch]$Check,
    [string]$PythonPath = ''
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$workspace = (Resolve-Path -LiteralPath (Join-Path $repo '..')).Path
if (-not $Check) {
    if ([string]::IsNullOrWhiteSpace($PythonPath)) { throw 'CannOps raw extraction requires -PythonPath; use Test-CannOpsUpstreamMap.ps1 for checked-in evidence.' }
    & $PythonPath (Join-Path $repo 'tools/CannOpsUpstreamMap/extract_cannops.py') --workspace $workspace --opencv-root (Join-Path $workspace 'opencv-source/opencv-5.0.0') --output (Join-Path $repo 'compatibility/cannops-upstream-raw.json')
    if ($LASTEXITCODE -ne 0) { throw 'CannOps raw extraction failed.' }
}
& (Join-Path $PSScriptRoot 'Generate-ContribUpstreamMap.ps1') -Module cannops -DisplayName CannOps -RepositoryRoot $repo -Check:$Check
