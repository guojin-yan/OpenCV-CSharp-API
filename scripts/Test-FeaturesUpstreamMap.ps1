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
& (Join-Path $PSScriptRoot "Generate-FeaturesUpstreamMap.ps1") @arguments
if (-not $?) {
    throw "Features upstream-map freshness or negative-fixture validation failed."
}

$summary = Get-Content -LiteralPath (Join-Path $RepositoryRoot "compatibility/features-upstream-summary.json") -Raw | ConvertFrom-Json
if ($summary.declarationCount -ne 183 -or
    $summary.callableCount -ne 160 -or
    $summary.classCount -ne 17 -or
    $summary.enumCount -ne 6 -or
    $summary.compatibilityHeaderCount -ne 2 -or
    $summary.classificationCounts.implemented -ne 134 -or
    $summary.classificationCounts.missing -ne 0 -or
    $summary.classificationCounts.'intentionally-omitted' -ne 26 -or
    $summary.selectedDeclarationCount -ne 12 -or
    $summary.sourceReviewedExtensionDeclarationCount -ne 9 -or
    $summary.negativeFixtureCount -ne 15) {
    throw "Features upstream partition drifted: declarations=$($summary.declarationCount) callables=$($summary.callableCount) implemented=$($summary.classificationCounts.implemented) missing=$($summary.classificationCounts.missing)."
}
if ($summary.repositoryWideUpstreamParityClaimed -ne $false) {
    throw "Features module map must not claim repository-wide upstream parity."
}
Write-Host "FEATURES_UPSTREAM_MAP_CONTRACT_OK declarations=$($summary.declarationCount) callables=$($summary.callableCount) implemented=$($summary.classificationCounts.implemented) missing=$($summary.classificationCounts.missing) fixtures=$($summary.negativeFixtureCount) sha256=$($summary.mappingSha256)"
