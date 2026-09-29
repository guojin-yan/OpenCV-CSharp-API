param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')))
Set-StrictMode -Version Latest;$ErrorActionPreference='Stop'
$repo=(Resolve-Path -LiteralPath $RepositoryRoot).Path
& (Join-Path $PSScriptRoot 'Generate-OptFlowUpstreamMap.ps1') -RepositoryRoot $repo -Check
if(-not $?){throw 'OptFlow upstream map freshness validation failed.'}
$s=Get-Content (Join-Path $repo 'compatibility/optflow-upstream-summary.json') -Raw|ConvertFrom-Json
if([int]$s.classificationCounts.missing -ne 0 -or [int]$s.declarationCount -ne 85){throw 'OptFlow upstream map partition drifted.'}
Write-Host "OPTFLOW_UPSTREAM_MAP_CONTRACT_OK declarations=$($s.declarationCount) callables=$($s.callableCount) implemented=$($s.classificationCounts.implemented) omitted=$($s.classificationCounts.'intentionally-omitted') missing=0 fixtures=$($s.negativeFixtureCount) sha256=$($s.mappingSha256)"
