param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,[switch]$Check,[string]$PythonPath = '')
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
if (-not $Check) {
    if ([string]::IsNullOrWhiteSpace($PythonPath)) { throw 'CUDA features2d raw extraction requires -PythonPath; use Test-CudaFeatures2DUpstreamMap.ps1 for checked-in evidence.' }
    $repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
    $workspace = (Resolve-Path -LiteralPath (Join-Path $repo '..')).Path
    $opencv = Join-Path $workspace 'opencv-source/opencv-5.0.0'
    & $PythonPath (Join-Path $repo 'tools/CudaFeatures2DUpstreamMap/extract_cudafeatures2d.py') --workspace $workspace --opencv-root $opencv --output (Join-Path $repo 'compatibility/cudafeatures2d-upstream-raw.json')
    if ($LASTEXITCODE -ne 0) { throw 'CUDA features2d raw extraction failed.' }
}
& (Join-Path $PSScriptRoot 'Generate-ContribUpstreamMap.ps1') -Module cudafeatures2d -DisplayName CudaFeatures2D -RepositoryRoot $RepositoryRoot -Check:$Check
