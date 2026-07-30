[CmdletBinding()]
param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path,
    [string]$DotNetPath = ""
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$arguments = @{ RepositoryRoot = $RepositoryRoot; Check = $true }
if (-not [string]::IsNullOrWhiteSpace($DotNetPath)) { $arguments.DotNetPath = $DotNetPath }
& (Join-Path $PSScriptRoot "Generate-MlUpstreamMap.ps1") @arguments
if (-not $?) { throw "ML upstream-map freshness or negative-fixture validation failed." }

$summary = Get-Content -LiteralPath (Join-Path $RepositoryRoot "compatibility/ml-upstream-summary.json") -Raw | ConvertFrom-Json
if ($summary.declarationCount -ne 241 -or $summary.callableCount -ne 208 -or $summary.classCount -ne 13 -or $summary.enumCount -ne 20 -or
    $summary.compatibilityHeaderCount -ne 1 -or $summary.excludedPublicHeaderCount -ne 1 -or $summary.sourceHeaderCount -ne 1 -or
    $summary.classificationCounts.implemented -ne 208 -or $summary.classificationCounts.missing -ne 0 -or
    $summary.classificationCounts.'non-callable-metadata' -ne 33 -or $summary.classificationCounts.'intentionally-omitted' -ne 0 -or
    $summary.selectedFamilyCount -ne 6 -or $summary.selectedDeclarationCount -ne 121 -or $summary.sourceReviewedExtensionCount -ne 2 -or
    $summary.negativeFixtureCount -ne 17 -or $summary.managedPublicTypeAdditionCount -ne 18 -or
    $summary.managedPublicMemberAdditionCount -ne 147 -or $summary.nativeEntrypointAdditionCount -ne 75) {
    throw "ML upstream partition drifted: declarations=$($summary.declarationCount) callables=$($summary.callableCount) implemented=$($summary.classificationCounts.implemented) missing=$($summary.classificationCounts.missing)."
}
if ($summary.repositoryWideUpstreamParityClaimed -ne $false) { throw "ML module map must not claim repository-wide upstream parity." }
Write-Host "ML_UPSTREAM_MAP_CONTRACT_OK declarations=$($summary.declarationCount) callables=$($summary.callableCount) implemented=$($summary.classificationCounts.implemented) missing=$($summary.classificationCounts.missing) fixtures=$($summary.negativeFixtureCount) sha256=$($summary.mappingSha256)"
