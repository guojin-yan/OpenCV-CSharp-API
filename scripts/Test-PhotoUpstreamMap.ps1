[CmdletBinding()]
param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path,
    [string]$DotNetPath = ""
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$arguments = @{ RepositoryRoot = $RepositoryRoot; Check = $true }
if (-not [string]::IsNullOrWhiteSpace($DotNetPath)) { $arguments.DotNetPath = $DotNetPath }
& (Join-Path $PSScriptRoot "Generate-PhotoUpstreamMap.ps1") @arguments
if (-not $?) { throw "Photo upstream-map freshness or negative-fixture validation failed." }

$summary = Get-Content -LiteralPath (Join-Path $RepositoryRoot "compatibility/photo-upstream-summary.json") -Raw | ConvertFrom-Json
if ($summary.declarationCount -ne 145 -or $summary.callableCount -ne 120 -or $summary.classCount -ne 15 -or $summary.enumCount -ne 10 -or
    $summary.compatibilityHeaderCount -ne 2 -or $summary.excludedPublicHeaderCount -ne 1 -or $summary.sourceHeaderCount -ne 3 -or
    $summary.classificationCounts.implemented -ne 120 -or $summary.classificationCounts.missing -ne 0 -or
    $summary.classificationCounts.'non-callable-metadata' -ne 25 -or $summary.classificationCounts.'intentionally-omitted' -ne 0 -or
    $summary.selectedFamilyCount -ne 3 -or $summary.selectedDeclarationCount -ne 83 -or $summary.negativeFixtureCount -ne 16 -or
    $summary.managedPublicTypeAdditionCount -ne 18 -or $summary.managedPublicMemberAdditionCount -ne 181 -or $summary.nativeEntrypointAdditionCount -ne 80) {
    throw "Photo upstream partition drifted: declarations=$($summary.declarationCount) callables=$($summary.callableCount) implemented=$($summary.classificationCounts.implemented) missing=$($summary.classificationCounts.missing)."
}
if ($summary.repositoryWideUpstreamParityClaimed -ne $false) { throw "Photo module map must not claim repository-wide upstream parity." }
Write-Host "PHOTO_UPSTREAM_MAP_CONTRACT_OK declarations=$($summary.declarationCount) callables=$($summary.callableCount) implemented=$($summary.classificationCounts.implemented) missing=$($summary.classificationCounts.missing) fixtures=$($summary.negativeFixtureCount) sha256=$($summary.mappingSha256)"
