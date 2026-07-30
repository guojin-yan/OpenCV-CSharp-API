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

& (Join-Path $PSScriptRoot "Generate-VideoIOUpstreamMap.ps1") @arguments
if (-not $?) {
    throw "VideoIO upstream-map freshness or negative-fixture validation failed."
}

$summary = Get-Content -LiteralPath (Join-Path $RepositoryRoot "compatibility/videoio-upstream-summary.json") -Raw | ConvertFrom-Json
if ($summary.declarationCount -ne 71 -or $summary.enumCount -ne 28 -or $summary.classCount -ne 3 -or $summary.callableCount -ne 40) {
    throw "VideoIO upstream declaration partition drifted: declarations=$($summary.declarationCount) enums=$($summary.enumCount) classes=$($summary.classCount) callables=$($summary.callableCount)."
}
if ($summary.classificationCounts.missing -ne 0 -or $summary.classificationCounts.implemented -ne 40) {
    throw "VideoIO upstream callable partition drifted: implemented=$($summary.classificationCounts.implemented) missing=$($summary.classificationCounts.missing)."
}
if ($summary.selectedFamilyCount -ne 4 -or $summary.selectedDeclarationCount -ne 71 -or $summary.managedPublicTypeAdditionCount -ne 3 -or $summary.managedPublicMemberAdditionCount -ne 31) {
    throw "VideoIO family inventory drifted: families=$($summary.selectedFamilyCount) declarations=$($summary.selectedDeclarationCount) types=$($summary.managedPublicTypeAdditionCount) members=$($summary.managedPublicMemberAdditionCount)."
}
Write-Host "VIDEOIO_UPSTREAM_MAP_CONTRACT_OK declarations=$($summary.declarationCount) callables=$($summary.callableCount) implemented=$($summary.classificationCounts.implemented) missing=$($summary.classificationCounts.missing) fixtures=$($summary.negativeFixtureCount) sha256=$($summary.mappingSha256)"
