param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,[switch]$Check,[string]$PythonPath = '')
Set-StrictMode -Version Latest;$ErrorActionPreference='Stop'
if (-not $Check) {
    if ([string]::IsNullOrWhiteSpace($PythonPath)) { throw 'CUDA codec raw extraction requires -PythonPath; use Test-CudaCodecUpstreamMap.ps1 for checked-in evidence.' }
    $repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
    $workspace = (Resolve-Path -LiteralPath (Join-Path $repo '..')).Path
    $opencv = Join-Path $workspace 'opencv-source/opencv-5.0.0'
    & $PythonPath (Join-Path $repo 'tools/CudaCodecUpstreamMap/extract_cudacodec.py') --workspace $workspace --opencv-root $opencv --output (Join-Path $repo 'compatibility/cudacodec-upstream-raw.json')
    if ($LASTEXITCODE -ne 0) { throw 'CUDA codec raw extraction failed.' }
}
& (Join-Path $PSScriptRoot 'Generate-ContribUpstreamMap.ps1') -Module cudacodec -DisplayName CudaCodec -RepositoryRoot $RepositoryRoot -Check:$Check
