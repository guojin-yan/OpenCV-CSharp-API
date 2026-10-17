param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
& (Join-Path $PSScriptRoot 'Generate-JuliaUpstreamMap.ps1') -RepositoryRoot $repo -Check

$summary = Get-Content (Join-Path $repo 'compatibility/julia-upstream-summary.json') -Raw | ConvertFrom-Json
$raw = Get-Content (Join-Path $repo 'compatibility/julia-upstream-raw.json') -Raw | ConvertFrom-Json
$map = Get-Content (Join-Path $repo 'compatibility/julia-upstream-classifications.json') -Raw | ConvertFrom-Json
$header = 'opencv-source/opencv_contrib-5.0.0/modules/julia/include/opencv2/julia.hpp'

if ([string]$raw.headerPath -cne $header -or @($raw.sourceHeaders).Count -ne 1 -or [string]$raw.sourceHeaders[0].path -cne $header -or [int]$raw.sourceHeaders[0].startOrdinal -ne 0 -or [int]$raw.sourceHeaders[0].declarationCount -ne 1) { throw 'Julia parser source closure drifted.' }
if ([int]$raw.declarationCount -ne 1 -or [int]$summary.declarationCount -ne 1 -or [int]$summary.callableCount -ne 1 -or [int]$summary.classCount -ne 0 -or [int]$summary.enumCount -ne 0 -or [int]$summary.classificationCounts.implemented -ne 0 -or [int]$summary.classificationCounts.'intentionally-omitted' -ne 1 -or [int]$summary.classificationCounts.'non-callable-metadata' -ne 0 -or [int]$summary.classificationCounts.missing -ne 0) { throw 'Julia declaration partition drifted.' }
if (@($map.declarations).Count -ne 1 -or [string]$map.declarations[0].identity -notmatch '^cv\.julia\.initJulia\(' -or [string]$map.declarations[0].classification -cne 'intentionally-omitted' -or @($map.declarations[0].nativeEntrypoints).Count -ne 0 -or @($map.declarations[0].managedMembers).Count -ne 0) { throw 'Julia callable classification closure drifted.' }
if ([int]$summary.nativeEvidenceCount -ne 0 -or [int]$summary.managedEvidenceCount -ne 0 -or [int]$summary.externalDataDependencyCount -ne 1 -or [bool]$summary.repositoryWideUpstreamParityClaimed) { throw 'Julia map falsely claims a wrapper surface.' }

Write-Host "JULIA_UPSTREAM_MAP_CONTRACT_OK declarations=$($summary.declarationCount) callables=1 implemented=0 omitted=1 missing=0 metadata=0 native=0 managed=0 external_dependencies=1 sha256=$($summary.mappingSha256)"
