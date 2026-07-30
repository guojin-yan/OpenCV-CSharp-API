[CmdletBinding()]
param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path,
    [string]$DotNetPath = ""
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$arguments = @{ RepositoryRoot = $RepositoryRoot; Check = $true }
if (-not [string]::IsNullOrWhiteSpace($DotNetPath)) { $arguments.DotNetPath = $DotNetPath }
& (Join-Path $PSScriptRoot "Generate-VideoUpstreamMap.ps1") @arguments
if (-not $?) { throw "Video upstream-map freshness or negative-fixture validation failed." }

$summary = Get-Content -LiteralPath (Join-Path $RepositoryRoot "compatibility/video-upstream-summary.json") -Raw | ConvertFrom-Json
if ($summary.declarationCount -ne 168 -or $summary.callableCount -ne 145 -or $summary.classCount -ne 20 -or $summary.enumCount -ne 3 -or
    $summary.compatibilityHeaderCount -ne 2 -or $summary.excludedPublicHeaderCount -ne 2 -or $summary.sourceHeaderCount -ne 2 -or
    $summary.classificationCounts.implemented -ne 138 -or $summary.classificationCounts.missing -ne 0 -or
    $summary.classificationCounts.'non-callable-metadata' -ne 23 -or $summary.classificationCounts.'intentionally-omitted' -ne 7 -or
    $summary.selectedFamilyCount -ne 3 -or $summary.selectedDeclarationCount -ne 83 -or $summary.negativeFixtureCount -ne 17 -or
    $summary.managedPublicTypeAdditionCount -ne 13 -or $summary.managedPublicMemberAdditionCount -ne 110 -or $summary.nativeEntrypointAdditionCount -ne 45) {
    throw "Video upstream partition drifted: declarations=$($summary.declarationCount) callables=$($summary.callableCount) implemented=$($summary.classificationCounts.implemented) missing=$($summary.classificationCounts.missing)."
}
if ($summary.repositoryWideUpstreamParityClaimed -ne $false) { throw "Video module map must not claim repository-wide upstream parity." }
Write-Host "VIDEO_UPSTREAM_MAP_CONTRACT_OK declarations=$($summary.declarationCount) callables=$($summary.callableCount) implemented=$($summary.classificationCounts.implemented) missing=$($summary.classificationCounts.missing) fixtures=$($summary.negativeFixtureCount) sha256=$($summary.mappingSha256)"
