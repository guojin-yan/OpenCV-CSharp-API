param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$generator = Join-Path $repo 'scripts/Generate-XImgProcUpstreamMap.ps1'
& $generator -RepositoryRoot $repo -Check
if (-not $?) { throw 'XImgProc upstream map freshness validation failed.' }
$raw = Get-Content (Join-Path $repo 'compatibility/ximgproc-upstream-raw.json') -Raw | ConvertFrom-Json
$classes = Get-Content (Join-Path $repo 'compatibility/ximgproc-upstream-classifications.json') -Raw | ConvertFrom-Json
$summary = Get-Content (Join-Path $repo 'compatibility/ximgproc-upstream-summary.json') -Raw | ConvertFrom-Json
if ([int]$raw.declarationCount -ne 249 -or @($classes.declarations).Count -ne 249) { throw 'XImgProc declaration closure drifted.' }
if ([int]$summary.classificationCounts.missing -ne 0 -or [int]$summary.classificationCounts.implemented -le 0) { throw 'XImgProc callable partition drifted.' }
$native = @(Get-Content (Join-Path $repo 'src/OpenCvSharp.Native/generated/native_abi_manifest.txt') | Where-Object { $_ -match '^jyppx_ocv_ximgproc_' } | ForEach-Object { ($_ -split '\|')[0] })
$managed = @(Get-Content (Join-Path $repo 'compatibility/managed-public-api.txt') | Where-Object { $_ -match 'JYPPX\.OpenCvSharp\.XImgProc' })
foreach ($row in @($classes.declarations)) {
    if ([string]::IsNullOrWhiteSpace([string]$row.identity) -or [string]::IsNullOrWhiteSpace([string]$row.reason)) { throw "XImgProc classification row is incomplete: $($row.ordinal)" }
    foreach ($entry in @($row.nativeEntrypoints)) { if ($native -notcontains [string]$entry) { throw "XImgProc native evidence is not in manifest: $entry" } }
    foreach ($entry in @($row.managedMembers)) { if ($managed -notcontains [string]$entry) { throw 'XImgProc managed evidence is not in managed baseline.' } }
    if ([string]$row.classification -eq 'implemented' -and (@($row.nativeEntrypoints).Count -eq 0 -or @($row.managedMembers).Count -eq 0)) { throw "Implemented XImgProc row lacks evidence: $($row.ordinal)" }
}
Write-Host "XIMGPROC_UPSTREAM_MAP_CONTRACT_OK declarations=249 callables=$($summary.callableCount) implemented=$($summary.classificationCounts.implemented) omitted=$($summary.classificationCounts.'intentionally-omitted') missing=0 fixtures=$($summary.negativeFixtureCount) sha256=$($summary.mappingSha256)"
