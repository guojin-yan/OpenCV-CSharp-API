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
    if ([string]::IsNullOrWhiteSpace($PythonPath)) { throw 'SFM raw extraction requires -PythonPath; use Test-SfmUpstreamMap.ps1 for checked-in evidence.' }
    & $PythonPath (Join-Path $repo 'tools/SfmUpstreamMap/extract_sfm.py') --workspace $workspace --opencv-root (Join-Path $workspace 'opencv-source/opencv-5.0.0') --output (Join-Path $repo 'compatibility/sfm-upstream-raw.json')
    if ($LASTEXITCODE -ne 0) { throw 'SFM raw extraction failed.' }
}
& (Join-Path $PSScriptRoot 'Generate-ContribUpstreamMap.ps1') -Module sfm -DisplayName Sfm -RepositoryRoot $repo -Check:$Check
