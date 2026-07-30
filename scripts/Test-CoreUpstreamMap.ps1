[CmdletBinding()]
param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path, [string]$DotNetPath = "")
Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
$arguments = @{ RepositoryRoot = $RepositoryRoot; Check = $true }
if (-not [string]::IsNullOrWhiteSpace($DotNetPath)) { $arguments.DotNetPath = $DotNetPath }
& (Join-Path $PSScriptRoot "Generate-CoreUpstreamMap.ps1") @arguments
if (-not $?) { throw "Core upstream map contract failed." }
$summary = Get-Content (Join-Path $RepositoryRoot "compatibility/core-upstream-summary.json") -Raw | ConvertFrom-Json
if ($summary.classificationCounts.missing -ne 0) { throw "Core map contains unexplained missing callables." }
Write-Host "CORE_UPSTREAM_MAP_CONTRACT_OK declarations=$($summary.declarationCount) callables=$($summary.callableCount) implemented=$($summary.classificationCounts.implemented) missing=$($summary.classificationCounts.missing) fixtures=$($summary.negativeFixtureCount) sha256=$($summary.mappingSha256)"
