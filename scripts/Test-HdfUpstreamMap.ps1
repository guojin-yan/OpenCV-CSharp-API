param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
& (Join-Path $PSScriptRoot 'Generate-HdfUpstreamMap.ps1') -RepositoryRoot $repo -Check

$summary = Get-Content (Join-Path $repo 'compatibility/hdf-upstream-summary.json') -Raw | ConvertFrom-Json
$raw = Get-Content (Join-Path $repo 'compatibility/hdf-upstream-raw.json') -Raw | ConvertFrom-Json
$map = Get-Content (Join-Path $repo 'compatibility/hdf-upstream-classifications.json') -Raw | ConvertFrom-Json
$header = 'opencv-source/opencv_contrib-5.0.0/modules/hdf/include/opencv2/hdf.hpp'
$sourceHeader = 'opencv-source/opencv_contrib-5.0.0/modules/hdf/include/opencv2/hdf/hdf5.hpp'

if ([string]$raw.headerPath -cne $header -or @($raw.sourceHeaders).Count -ne 1 -or [string]$raw.sourceHeaders[0].path -cne $sourceHeader -or [int]$raw.sourceHeaders[0].startOrdinal -ne 0 -or [int]$raw.sourceHeaders[0].declarationCount -ne 38) { throw 'HDF parser source closure drifted.' }
if ([int]$raw.declarationCount -ne 38 -or [int]$summary.declarationCount -ne 38 -or [int]$summary.callableCount -ne 37 -or [int]$summary.classCount -ne 1 -or [int]$summary.enumCount -ne 0 -or [int]$summary.classificationCounts.implemented -ne 0 -or [int]$summary.classificationCounts.'intentionally-omitted' -ne 37 -or [int]$summary.classificationCounts.'non-callable-metadata' -ne 1 -or [int]$summary.classificationCounts.missing -ne 0) { throw 'HDF declaration partition drifted.' }
if (@($map.declarations).Count -ne 38 -or @($map.declarations | Where-Object { $_.classification -eq 'intentionally-omitted' -and @($_.nativeEntrypoints).Count -eq 0 -and @($_.managedMembers).Count -eq 0 -and [string]$_.reason -match 'external HDF5' }).Count -ne 37) { throw 'HDF omission classification closure drifted.' }
if ([int]$summary.nativeEvidenceCount -ne 0 -or [int]$summary.managedEvidenceCount -ne 0 -or [int]$summary.externalDataDependencyCount -ne 1 -or [bool]$summary.repositoryWideUpstreamParityClaimed) { throw 'HDF map falsely claims a wrapper surface.' }

Write-Host "HDF_UPSTREAM_MAP_CONTRACT_OK declarations=$($summary.declarationCount) callables=$($summary.callableCount) implemented=0 omitted=$($summary.classificationCounts.'intentionally-omitted') missing=0 metadata=$($summary.classificationCounts.'non-callable-metadata') native=0 managed=0 external_dependencies=$($summary.externalDataDependencyCount) sha256=$($summary.mappingSha256)"
