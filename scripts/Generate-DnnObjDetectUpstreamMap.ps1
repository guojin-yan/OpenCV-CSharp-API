param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,
    [switch]$Check,
    [string]$PythonPath = ''
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$workspace = (Resolve-Path -LiteralPath (Join-Path $repo '..')).Path
$raw = Join-Path $repo 'compatibility/dnn_objdetect-upstream-raw.json'

if (-not $Check) {
    if ([string]::IsNullOrWhiteSpace($PythonPath)) {
        throw 'DnnObjDetect raw extraction requires -PythonPath; use Test-DnnObjDetectUpstreamMap.ps1 for checked-in evidence.'
    }
    $opencv = Join-Path $workspace 'opencv-source/opencv-5.0.0'
    & $PythonPath (Join-Path $repo 'tools/DnnObjDetectUpstreamMap/extract_dnn_objdetect.py') --workspace $workspace --opencv-root $opencv --output $raw
    if ($LASTEXITCODE -ne 0) { throw 'DnnObjDetect raw extraction failed.' }
}

& (Join-Path $PSScriptRoot 'Generate-ContribUpstreamMap.ps1') -Module dnn_objdetect -DisplayName DnnObjDetect -RepositoryRoot $repo -Check:$Check
