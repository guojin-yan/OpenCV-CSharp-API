[CmdletBinding()]
param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path,
    [string]$DotNetPath = ""
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$arguments = @{ RepositoryRoot = $RepositoryRoot; Check = $true }
if (-not [string]::IsNullOrWhiteSpace($DotNetPath)) { $arguments.DotNetPath = $DotNetPath }
& (Join-Path $PSScriptRoot "Generate-ObjDetectUpstreamMap.ps1") @arguments
if (-not $?) { throw "ObjDetect upstream-map freshness or negative-fixture validation failed." }

$summary = Get-Content -LiteralPath (Join-Path $RepositoryRoot "compatibility/objdetect-upstream-summary.json") -Raw | ConvertFrom-Json
if ($summary.declarationCount -ne 195 -or $summary.callableCount -ne 163 -or $summary.classCount -ne 22 -or $summary.enumCount -ne 10 -or
    $summary.compatibilityHeaderCount -ne 2 -or $summary.sourceHeaderCount -ne 9 -or $summary.classificationCounts.implemented -ne 153 -or
    $summary.classificationCounts.missing -ne 0 -or $summary.classificationCounts.'intentionally-omitted' -ne 10 -or
    $summary.selectedDeclarationCount -ne 33 -or $summary.negativeFixtureCount -ne 15 -or
    $summary.managedPublicTypeAdditionCount -ne 4 -or $summary.managedPublicMemberAdditionCount -ne 55 -or $summary.nativeEntrypointAdditionCount -ne 35) {
    throw "ObjDetect upstream partition drifted: declarations=$($summary.declarationCount) callables=$($summary.callableCount) implemented=$($summary.classificationCounts.implemented) missing=$($summary.classificationCounts.missing)."
}
if ($summary.repositoryWideUpstreamParityClaimed -ne $false) { throw "ObjDetect module map must not claim repository-wide upstream parity." }
Write-Host "OBJDETECT_UPSTREAM_MAP_CONTRACT_OK declarations=$($summary.declarationCount) callables=$($summary.callableCount) implemented=$($summary.classificationCounts.implemented) missing=$($summary.classificationCounts.missing) fixtures=$($summary.negativeFixtureCount) sha256=$($summary.mappingSha256)"
