[CmdletBinding()]
param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,
    [string]$DotNetPath = 'dotnet',
    [string]$PythonPath = '',
    [string]$OutputDirectory = '',
    [switch]$RegenerateRaw,
    [switch]$Check
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$workspace = (Resolve-Path -LiteralPath (Join-Path $repo '..')).Path
if ([string]::IsNullOrWhiteSpace($OutputDirectory)) { $OutputDirectory = Join-Path $repo 'compatibility/opencv-5.1.0' }
$OutputDirectory = [IO.Path]::GetFullPath($OutputDirectory)
if ($RegenerateRaw) {
    if ([string]::IsNullOrWhiteSpace($PythonPath)) { throw 'Raw extraction requires an explicit -PythonPath.' }
    & $PythonPath (Join-Path $repo 'tools/UpstreamMapRefresh/extract.py') --repository $repo --workspace $workspace --catalog (Join-Path $repo 'compatibility/opencv-5.1.0/upstream-map-refresh-catalog.json') --output-directory $OutputDirectory
    if ($LASTEXITCODE -ne 0) { throw 'OpenCV 5.1 raw extraction failed.' }
}
$arguments = @('run', '--project', (Join-Path $repo 'tools/UpstreamMapRefresh/UpstreamMapRefresh.csproj'), '-c', 'Release', '--',
    '--repository', $repo, '--workspace', $workspace, '--output-directory', $OutputDirectory)
if ($Check) { $arguments += '--check' }
& $DotNetPath @arguments
if ($LASTEXITCODE -ne 0) { throw 'OpenCV 5.1 map generation or semantic verification failed.' }
