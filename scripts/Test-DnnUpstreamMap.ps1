[CmdletBinding()]
param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path,
    [string]$DotNetPath = ""
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$arguments = @{ RepositoryRoot = $RepositoryRoot; Check = $true }
if (-not [string]::IsNullOrWhiteSpace($DotNetPath)) {
    $arguments.DotNetPath = $DotNetPath
}
& (Join-Path $PSScriptRoot "Generate-DnnUpstreamMap.ps1") @arguments
if (-not $?) {
    throw "DNN upstream-map freshness or negative-fixture validation failed."
}

$summary = Get-Content -LiteralPath (Join-Path $RepositoryRoot "compatibility/dnn-upstream-summary.json") -Raw | ConvertFrom-Json
if ($summary.declarationCount -ne 182 -or $summary.callableCount -ne 159 -or $summary.classificationCounts.missing -ne 0) {
    throw "DNN upstream partition drifted: declarations=$($summary.declarationCount) callables=$($summary.callableCount) implemented=$($summary.classificationCounts.implemented) missing=$($summary.classificationCounts.missing)."
}
if ($summary.repositoryWideUpstreamParityClaimed -ne $false) {
    throw "DNN module map must not claim repository-wide upstream parity."
}
Write-Host "DNN_UPSTREAM_MAP_CONTRACT_OK declarations=$($summary.declarationCount) callables=$($summary.callableCount) implemented=$($summary.classificationCounts.implemented) missing=$($summary.classificationCounts.missing) fixtures=$($summary.negativeFixtureCount) sha256=$($summary.mappingSha256)"
