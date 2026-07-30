[CmdletBinding()]
param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path,
    [string]$DotNetPath = ""
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$arguments = @{
    RepositoryRoot = $RepositoryRoot
    Check = $true
}
if (-not [string]::IsNullOrWhiteSpace($DotNetPath)) {
    $arguments.DotNetPath = $DotNetPath
}

& (Join-Path $PSScriptRoot "Generate-ImgProcUpstreamMap.ps1") @arguments
if (-not $?) {
    throw "ImgProc upstream-map freshness or negative-fixture validation failed."
}

$summary = Get-Content -LiteralPath (Join-Path $RepositoryRoot "compatibility/imgproc-upstream-summary.json") -Raw | ConvertFrom-Json
if ($summary.classificationCounts.missing -ne 0 -or $summary.classificationCounts.implemented -ne 161) {
    throw "ImgProc upstream callable partition drifted: implemented=$($summary.classificationCounts.implemented) missing=$($summary.classificationCounts.missing)."
}
Write-Host "IMGPROC_UPSTREAM_MAP_CONTRACT_OK declarations=$($summary.declarationCount) callables=$($summary.callableCount) implemented=$($summary.classificationCounts.implemented) missing=$($summary.classificationCounts.missing) fixtures=$($summary.negativeFixtureCount) sha256=$($summary.mappingSha256)"
