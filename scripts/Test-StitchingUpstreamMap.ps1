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

& (Join-Path $repo "scripts/Generate-StitchingUpstreamMap.ps1") -RepositoryRoot $repo -DotNetPath $dotnet -Check
if ($LASTEXITCODE -ne 0) { throw "Stitching map check failed with exit code $LASTEXITCODE." }

$summary = Get-Content -LiteralPath (Join-Path $repo "compatibility/stitching-upstream-summary.json") -Raw | ConvertFrom-Json
if ($summary.declarationCount -ne 207 -or $summary.callableCount -ne 158 -or $summary.enumCount -ne 9 -or $summary.classCount -ne 40) { throw "Stitching parser count contract drifted." }
if ($summary.compatibilityHeaderCount -ne 14 -or $summary.sourceHeaderCount -ne 14) { throw "Stitching public header closure drifted." }
if ($summary.surfaceCounts.primary.declarations -ne 24 -or $summary.surfaceCounts.primary.callables -ne 21 -or $summary.surfaceCounts.primary.implemented -ne 21) { throw "Stitching high-level partition drifted." }
if ($summary.surfaceCounts.'detail-exposure'.declarations -ne 53 -or $summary.surfaceCounts.'detail-exposure'.callables -ne 45 -or $summary.surfaceCounts.'detail-exposure'.implemented -ne 45) { throw "Stitching Exposure partition drifted." }
if ($summary.classificationCounts.implemented -ne 66 -or $summary.classificationCounts.missing -ne 92 -or
    $summary.classificationCounts.'non-callable-metadata' -ne 49 -or $summary.classificationCounts.'intentionally-omitted' -ne 0 -or
    $summary.classificationCounts.unsupported -ne 0 -or $summary.classificationCounts.'upstream-conditional' -ne 0) { throw "Stitching classification contract drifted." }
if ($summary.negativeFixtureCount -ne 17 -or $summary.selectedFamilyCount -ne 1 -or $summary.selectedDeclarationCount -ne 45 -or $summary.highLevelImplementedCallableCount -ne 21) { throw "Stitching fixture/family contract drifted." }
if ($summary.managedPublicTypeAdditionCount -ne 8 -or $summary.managedPublicMemberAdditionCount -ne 27 -or $summary.nativeEntrypointAdditionCount -ne 22) { throw "Stitching addition counts drifted." }
if ($summary.umatExecutionClaimed -or $summary.detailRowsMixedIntoHighLevel -or $summary.repositoryWideUpstreamParityClaimed) { throw "Stitching ownership or parity boundary was weakened." }

Write-Output "STITCHING_UPSTREAM_MAP_GUARD_OK declarations=207 high-level=24/21/21 exposure=53/45/45 implemented=66 missing=92 fixtures=17"
