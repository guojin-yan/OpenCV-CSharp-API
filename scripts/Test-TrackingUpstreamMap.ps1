[CmdletBinding()]
param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path,
    [string]$DotNetPath = ""
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
if ([string]::IsNullOrWhiteSpace($DotNetPath)) { $DotNetPath = (Get-Command dotnet -ErrorAction Stop).Source }
$dotnet = (Resolve-Path -LiteralPath $DotNetPath).Path

& (Join-Path $repo "scripts/Generate-TrackingUpstreamMap.ps1") -RepositoryRoot $repo -DotNetPath $dotnet -Check
if ($LASTEXITCODE -ne 0) { throw "Tracking map check failed with exit code $LASTEXITCODE." }

$summary = Get-Content -LiteralPath (Join-Path $repo "compatibility/tracking-upstream-summary.json") -Raw | ConvertFrom-Json
if ($summary.declarationCount -ne 35 -or $summary.callableCount -ne 21) { throw "Tracking declaration/callable contract drifted." }
if ($summary.primaryDeclarationCount -ne 10 -or $summary.primaryCallableCount -ne 5) { throw "Tracking primary partition drifted." }
if ($summary.legacyDeclarationCount -ne 25 -or $summary.legacyCallableCount -ne 16) { throw "Tracking legacy partition drifted." }
if ($summary.classificationCounts.implemented -ne 21 -or $summary.classificationCounts.missing -ne 0 -or
    $summary.classificationCounts.'intentionally-omitted' -ne 0 -or $summary.classificationCounts.unsupported -ne 0 -or
    $summary.classificationCounts.'upstream-conditional' -ne 0 -or $summary.classificationCounts.'non-callable-metadata' -ne 14) {
    throw "Tracking classification contract drifted."
}
if ($summary.negativeFixtureCount -ne 17 -or $summary.selectedFamilyCount -ne 1 -or $summary.selectedDeclarationCount -ne 6) {
    throw "Tracking fixture/family contract drifted."
}
if ($summary.managedPublicTypeAdditionCount -ne 5 -or $summary.managedPublicMemberAdditionCount -ne 23 -or $summary.nativeEntrypointAdditionCount -ne 10) {
    throw "Tracking addition counts drifted."
}
if ($summary.mainVideoRowsDoubleCounted -or $summary.legacyRowsMixedIntoPrimary -or $summary.repositoryWideUpstreamParityClaimed) {
    throw "Tracking ownership or repository-wide parity boundary was weakened."
}

Write-Output "TRACKING_UPSTREAM_MAP_GUARD_OK declarations=35 primary=10/5 legacy=25/16 implemented=21 fixtures=17"
