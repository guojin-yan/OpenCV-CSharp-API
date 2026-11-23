[CmdletBinding()]
param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,
    [string]$DotNetPath = 'dotnet'
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
& (Join-Path $PSScriptRoot 'Generate-Dnn51UpstreamMap.ps1') -RepositoryRoot $RepositoryRoot -DotNetPath $DotNetPath -Check
$summary = Get-Content -LiteralPath (Join-Path $RepositoryRoot 'compatibility/opencv-5.1.0/dnn-upstream-summary.json') -Raw | ConvertFrom-Json
if ($summary.classificationCounts.missing -ne 0 -or $summary.completeDnnCppParityClaimed -or $summary.repositoryWideUpstreamParityClaimed -or $summary.ortRuntimeValidated) {
    throw 'DNN 5.1 map must retain its parser-slice and optional-runtime boundaries.'
}
Write-Host "DNN_510_UPSTREAM_CONTRACT_OK declarations=$($summary.declarationCount) added=$($summary.addedIdentityCount) removed=$($summary.removedIdentityCount) fixtures=$($summary.negativeFixtureCount)"
