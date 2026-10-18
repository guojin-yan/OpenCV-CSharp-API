param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,[switch]$Check,[string]$PythonPath = '')
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
if (-not $Check) {
    if ([string]::IsNullOrWhiteSpace($PythonPath)) { throw 'CUDA stereo raw extraction requires -PythonPath; use Test-CudaStereoUpstreamMap.ps1 for checked-in evidence.' }
    $repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
    $workspace = (Resolve-Path -LiteralPath (Join-Path $repo '..')).Path
    $opencv = Join-Path $workspace 'opencv-source/opencv-5.0.0'
    & $PythonPath (Join-Path $repo 'tools/CudaStereoUpstreamMap/extract_cudastereo.py') --workspace $workspace --opencv-root $opencv --output (Join-Path $repo 'compatibility/cudastereo-upstream-raw.json')
    if ($LASTEXITCODE -ne 0) { throw 'CUDA stereo raw extraction failed.' }
}
& (Join-Path $PSScriptRoot 'Generate-ContribUpstreamMap.ps1') -Module cudastereo -DisplayName CudaStereo -RepositoryRoot $RepositoryRoot -Check:$Check
