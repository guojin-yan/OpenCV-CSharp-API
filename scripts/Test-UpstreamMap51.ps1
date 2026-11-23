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
if ($summary.historicalCatalogMapCount -ne 73 -or $summary.refreshedParserSliceCount -ne 22 -or $summary.pendingCatalogMapCount -ne 51 -or
    (@($summary.slices.id) -join '|') -cne 'bgsegm|calib3d|core|dnn|face|features|highgui|img_hash|imgcodecs|imgproc|line_descriptor|ml|objdetect|optflow|photo|quality|stitching|tracking|video|videoio|ximgproc|xphoto' -or
    $summary.upstreamContribCommit -cne 'b67a855188a3d683813731010f6d39ac614733a4' -or $summary.repositoryWideUpstreamParityClaimed -or
    $summary.completeCppParityClaimed -or $summary.sharedGeneratorNegativeFixtureCount -ne 675 -or
    ($summary.slices.declarationCount | Measure-Object -Sum).Sum -ne 2825) { throw '5.1 refreshed parser slice inventory drifted.' }
Write-Host 'UPSTREAM_510_REFRESH_CONTRACT_OK slices=22 pending=51 declarations=2825 shared_generator_fixtures=675'
