[CmdletBinding()]
param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,
    [string]$DotNetPath = 'dotnet'
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
& (Join-Path $PSScriptRoot 'Generate-UpstreamMap51.ps1') -RepositoryRoot $repo -DotNetPath $DotNetPath -Check
$summary = Get-Content -LiteralPath (Join-Path $repo 'compatibility/opencv-5.1.0/upstream-map-refresh-summary.json') -Raw | ConvertFrom-Json
if ($summary.historicalCatalogMapCount -ne 73 -or $summary.refreshedParserSliceCount -ne 5 -or $summary.pendingCatalogMapCount -ne 68 -or
    (@($summary.slices.id) -join '|') -cne 'calib3d|core|dnn|imgcodecs|imgproc' -or $summary.repositoryWideUpstreamParityClaimed -or
    $summary.completeCppParityClaimed -or $summary.sharedGeneratorNegativeFixtureCount -ne 127 -or
    ($summary.slices.declarationCount | Measure-Object -Sum).Sum -ne 914) { throw '5.1 refreshed parser slice inventory drifted.' }
Write-Host 'UPSTREAM_510_REFRESH_CONTRACT_OK slices=5 pending=68 declarations=914 shared_generator_fixtures=127'
