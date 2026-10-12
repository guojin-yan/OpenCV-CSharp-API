param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,
    [switch]$Check,
    [string]$PythonPath = ''
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$workspace = (Resolve-Path -LiteralPath (Join-Path $repo '..')).Path
$raw = Join-Path $repo 'compatibility/hdf-upstream-raw.json'

if (-not $Check) {
    if ([string]::IsNullOrWhiteSpace($PythonPath)) {
        throw 'HDF raw extraction requires -PythonPath; use Test-HdfUpstreamMap.ps1 for the checked-in evidence.'
    }
    $opencv = Join-Path $workspace 'opencv-source/opencv-5.0.0'
    & $PythonPath (Join-Path $repo 'tools/HdfUpstreamMap/extract_hdf.py') --workspace $workspace --opencv-root $opencv --output $raw
    if ($LASTEXITCODE -ne 0) { throw 'HDF raw extraction failed.' }
}

& (Join-Path $PSScriptRoot 'Generate-ContribUpstreamMap.ps1') -Module hdf -DisplayName Hdf -RepositoryRoot $repo -Check:$Check
