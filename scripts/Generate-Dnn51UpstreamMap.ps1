[CmdletBinding()]
param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,
    [string]$DotNetPath = 'dotnet',
    [string]$PythonPath = '',
    [switch]$RegenerateRaw,
    [switch]$InitializeClassification,
    [switch]$Check
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$workspace = (Resolve-Path -LiteralPath (Join-Path $repo '..')).Path
$output = Join-Path $repo 'compatibility/opencv-5.1.0'
$raw = Join-Path $output 'dnn-upstream-raw.json'
if ($RegenerateRaw) {
    if ([string]::IsNullOrWhiteSpace($PythonPath)) { throw 'Raw extraction requires an explicit -PythonPath.' }
    & $PythonPath (Join-Path $repo 'tools/DnnUpstreamMap/extract_dnn.py') --workspace $workspace --opencv-root (Join-Path $workspace 'opencv-source/opencv-5.1.0') --upstream-version 5.1.0 --output $raw
    if ($LASTEXITCODE -ne 0) { throw 'DNN 5.1 raw extraction failed.' }
}
$arguments = @('run', '--project', (Join-Path $repo 'tools/DnnUpstreamMap/DnnUpstreamMap.csproj'), '-c', 'Release', '--',
    '--refresh-5.1', '--repository', $repo, '--workspace', $workspace, '--raw', $raw,
    '--classification', (Join-Path $output 'dnn-upstream-classifications.json'),
    '--native-manifest', (Join-Path $repo 'src/OpenCvSharp.Native/generated/native_abi_manifest.txt'),
    '--managed-baseline', (Join-Path $repo 'compatibility/managed-public-api.txt'),
    '--output', (Join-Path $output 'dnn-upstream-map.txt'), '--summary', (Join-Path $output 'dnn-upstream-summary.json'),
    '--family-output', (Join-Path $output 'dnn-implemented-families.json'))
if ($InitializeClassification) { $arguments += '--initialize-classification' }
if ($Check) { $arguments += '--check' }
& $DotNetPath @arguments
if ($LASTEXITCODE -ne 0) { throw 'DNN 5.1 generation or semantic check failed.' }
