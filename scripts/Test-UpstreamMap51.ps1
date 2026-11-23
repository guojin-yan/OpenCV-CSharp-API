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
if ($summary.historicalCatalogMapCount -ne 73 -or $summary.refreshedParserSliceCount -ne 4 -or $summary.pendingCatalogMapCount -ne 69 -or
    (@($summary.slices.id) -join '|') -cne 'core|dnn|imgcodecs|imgproc' -or $summary.repositoryWideUpstreamParityClaimed -or
    $summary.completeCppParityClaimed -or $summary.sharedGeneratorNegativeFixtureCount -ne 90) { throw '5.1 refreshed parser slice inventory drifted.' }
Write-Host 'UPSTREAM_510_REFRESH_CONTRACT_OK slices=4 pending=69 declarations=706 shared_generator_fixtures=90'
