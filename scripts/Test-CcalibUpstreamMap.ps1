param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
& (Join-Path $PSScriptRoot 'Generate-CcalibUpstreamMap.ps1') -RepositoryRoot $repo -Check

$summary = Get-Content (Join-Path $repo 'compatibility/ccalib-upstream-summary.json') -Raw | ConvertFrom-Json
$raw = Get-Content (Join-Path $repo 'compatibility/ccalib-upstream-raw.json') -Raw | ConvertFrom-Json
$map = Get-Content (Join-Path $repo 'compatibility/ccalib-upstream-classifications.json') -Raw | ConvertFrom-Json
$header = 'opencv-source/opencv_contrib-5.0.0/modules/ccalib/include/opencv2/ccalib.hpp'

if ([string]$raw.headerPath -cne $header -or @($raw.sourceHeaders).Count -ne 3 -or [int]$raw.sourceHeaders[0].startOrdinal -ne 0 -or [int]$raw.sourceHeaders[0].declarationCount -ne 1 -or [int]$raw.sourceHeaders[1].startOrdinal -ne 1 -or [int]$raw.sourceHeaders[1].declarationCount -ne 11 -or [int]$raw.sourceHeaders[2].startOrdinal -ne 12 -or [int]$raw.sourceHeaders[2].declarationCount -ne 0) { throw 'Ccalib parser source closure drifted.' }
if ([int]$raw.declarationCount -ne 12 -or [int]$summary.declarationCount -ne 12 -or [int]$summary.enumCount -ne 4 -or [int]$summary.callableCount -ne 8 -or [int]$summary.classificationCounts.missing -ne 0 -or [int]$summary.classificationCounts.implemented -ne 0 -or [int]$summary.classificationCounts.'intentionally-omitted' -ne 8 -or [int]$summary.classificationCounts.'non-callable-metadata' -ne 4) { throw 'Ccalib declaration partition drifted.' }
if (@($map.declarations).Count -ne [int]$summary.declarationCount -or @($map.declarations | Where-Object { $_.classification -eq 'intentionally-omitted' -and @($_.nativeEntrypoints).Count -eq 0 -and @($_.managedMembers).Count -eq 0 }).Count -ne [int]$summary.classificationCounts.'intentionally-omitted') { throw 'Ccalib omission closure drifted.' }
if ([int]$summary.nativeEvidenceCount -ne 0 -or [int]$summary.managedEvidenceCount -ne 0 -or [int]$summary.externalDataDependencyCount -ne 1 -or [bool]$summary.repositoryWideUpstreamParityClaimed) { throw 'Ccalib map falsely claims a wrapper surface.' }

Write-Host "CCALIB_UPSTREAM_MAP_CONTRACT_OK declarations=$($summary.declarationCount) callable=$($summary.callableCount) implemented=0 omitted=$($summary.classificationCounts.'intentionally-omitted') missing=0 metadata=$($summary.classificationCounts.'non-callable-metadata') native=0 managed=0 sha256=$($summary.mappingSha256)"
