param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,[switch]$Check,[string]$PythonPath = '')
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
if (-not $Check) {
    if ([string]::IsNullOrWhiteSpace($PythonPath)) { throw 'CUDA legacy raw extraction requires -PythonPath; use Test-CudaLegacyUpstreamMap.ps1 for checked-in evidence.' }
    $repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
    $workspace = (Resolve-Path -LiteralPath (Join-Path $repo '..')).Path
    $opencv = Join-Path $workspace 'opencv-source/opencv-5.0.0'
    & $PythonPath (Join-Path $repo 'tools/CudaLegacyUpstreamMap/extract_cudalegacy.py') --workspace $workspace --opencv-root $opencv --output (Join-Path $repo 'compatibility/cudalegacy-upstream-raw.json')
    if ($LASTEXITCODE -ne 0) { throw 'CUDA legacy raw extraction failed.' }
}
& (Join-Path $PSScriptRoot 'Generate-ContribUpstreamMap.ps1') -Module cudalegacy -DisplayName CudaLegacy -RepositoryRoot $RepositoryRoot -Check:$Check
