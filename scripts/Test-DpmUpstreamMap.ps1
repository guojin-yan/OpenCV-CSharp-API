param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
& (Join-Path $PSScriptRoot 'Generate-DpmUpstreamMap.ps1') -RepositoryRoot $repo -Check

$summary = Get-Content (Join-Path $repo 'compatibility/dpm-upstream-summary.json') -Raw | ConvertFrom-Json
$raw = Get-Content (Join-Path $repo 'compatibility/dpm-upstream-raw.json') -Raw | ConvertFrom-Json
$map = Get-Content (Join-Path $repo 'compatibility/dpm-upstream-classifications.json') -Raw | ConvertFrom-Json
$header = 'opencv-source/opencv_contrib-5.0.0/modules/dpm/include/opencv2/dpm.hpp'

if ([string]$raw.headerPath -cne $header -or @($raw.sourceHeaders).Count -ne 1 -or [string]$raw.sourceHeaders[0].path -cne $header -or [int]$raw.sourceHeaders[0].startOrdinal -ne 0 -or [int]$raw.sourceHeaders[0].declarationCount -ne 2) { throw 'DPM parser source closure drifted.' }
if ([int]$raw.declarationCount -ne 2 -or [int]$summary.declarationCount -ne 2 -or [int]$summary.callableCount -ne 0 -or [int]$summary.classCount -ne 2 -or [int]$summary.enumCount -ne 0 -or [int]$summary.classificationCounts.implemented -ne 0 -or [int]$summary.classificationCounts.'intentionally-omitted' -ne 0 -or [int]$summary.classificationCounts.'non-callable-metadata' -ne 2 -or [int]$summary.classificationCounts.missing -ne 0) { throw 'DPM declaration partition drifted.' }
if (@($map.declarations).Count -ne 2 -or @($map.declarations | Where-Object { $_.classification -eq 'non-callable-metadata' -and @($_.nativeEntrypoints).Count -eq 0 -and @($_.managedMembers).Count -eq 0 }).Count -ne 2) { throw 'DPM metadata classification closure drifted.' }
if ([int]$summary.nativeEvidenceCount -ne 0 -or [int]$summary.managedEvidenceCount -ne 0 -or [int]$summary.externalDataDependencyCount -ne 0 -or [bool]$summary.repositoryWideUpstreamParityClaimed) { throw 'DPM map falsely claims a wrapper surface.' }

Write-Host "DPM_UPSTREAM_MAP_CONTRACT_OK declarations=$($summary.declarationCount) callables=0 implemented=0 omitted=0 missing=0 metadata=$($summary.classificationCounts.'non-callable-metadata') native=0 managed=0 sha256=$($summary.mappingSha256)"
