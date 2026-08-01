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

& (Join-Path $repo "scripts/Generate-HighGuiUpstreamMap.ps1") -RepositoryRoot $repo -DotNetPath $dotnet -Check
if ($LASTEXITCODE -ne 0) { throw "HighGui map check failed with exit code $LASTEXITCODE." }

$summary = Get-Content -LiteralPath (Join-Path $repo "compatibility/highgui-upstream-summary.json") -Raw | ConvertFrom-Json
if ($summary.declarationCount -ne 33 -or $summary.callableCount -ne 26 -or $summary.enumCount -ne 7 -or $summary.classCount -ne 0) {
    throw "HighGui declaration partition drifted."
}
if ($summary.classificationCounts.implemented -ne 20 -or $summary.classificationCounts.missing -ne 0 -or
    $summary.classificationCounts.'intentionally-omitted' -ne 3 -or $summary.classificationCounts.'upstream-conditional' -ne 3 -or
    $summary.classificationCounts.unsupported -ne 0 -or $summary.classificationCounts.'non-callable-metadata' -ne 7) {
    throw "HighGui classification contract drifted."
}
if ($summary.compatibilityHeaderCount -ne 3 -or $summary.sourceHeaderCount -ne 1 -or $summary.excludedPublicHeaderCount -ne 0 -or
    $summary.selectedFamilyCount -ne 3 -or $summary.selectedDeclarationCount -ne 20 -or $summary.sourceReviewedExtensionCount -ne 4 -or
    $summary.negativeFixtureCount -ne 17) {
    throw "HighGui header/family/fixture contract drifted."
}
if ($summary.managedPublicTypeAdditionCount -ne 0 -or $summary.managedPublicMemberAdditionCount -ne 6 -or
    $summary.nativeEntrypointAdditionCount -ne 10 -or $summary.repositoryWideUpstreamParityClaimed) {
    throw "HighGui addition count or repository-wide parity boundary drifted."
}

Write-Output "HIGHGUI_UPSTREAM_MAP_GUARD_OK declarations=33 callables=26 implemented=20 omitted=3 conditional=3 missing=0 extensions=4 fixtures=17"
