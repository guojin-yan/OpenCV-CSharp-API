param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')),[switch]$Check,[string]$PythonPath = '')
Set-StrictMode -Version Latest;$ErrorActionPreference='Stop'
if (-not $Check) {
    if ([string]::IsNullOrWhiteSpace($PythonPath)) { throw 'CUDA image-filter raw extraction requires -PythonPath; use Test-CudaFiltersUpstreamMap.ps1 for checked-in evidence.' }
    $repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
    $workspace = (Resolve-Path -LiteralPath (Join-Path $repo '..')).Path
    $opencv = Join-Path $workspace 'opencv-source/opencv-5.0.0'
    & $PythonPath (Join-Path $repo 'tools/CudaFiltersUpstreamMap/extract_cudafilters.py') --workspace $workspace --opencv-root $opencv --output (Join-Path $repo 'compatibility/cudafilters-upstream-raw.json')
    if ($LASTEXITCODE -ne 0) { throw 'CUDA image-filter raw extraction failed.' }
}
& (Join-Path $PSScriptRoot 'Generate-ContribUpstreamMap.ps1') -Module cudafilters -DisplayName CudaFilters -RepositoryRoot $RepositoryRoot -Check:$Check
