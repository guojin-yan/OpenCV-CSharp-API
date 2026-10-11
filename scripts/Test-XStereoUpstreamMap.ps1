param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
& (Join-Path $repo 'scripts/Generate-XStereoUpstreamMap.ps1') -RepositoryRoot $repo -Check
$raw = Get-Content (Join-Path $repo 'compatibility/xstereo-upstream-raw.json') -Raw | ConvertFrom-Json
$summary = Get-Content (Join-Path $repo 'compatibility/xstereo-upstream-summary.json') -Raw | ConvertFrom-Json
$map = Get-Content (Join-Path $repo 'compatibility/xstereo-upstream-classifications.json') -Raw | ConvertFrom-Json
$manifest = @(Get-Content (Join-Path $repo 'src/OpenCvSharp.Native/generated/native_abi_manifest.txt') | Where-Object { $_ -match '^jyppx_ocv_xstereo_' } | ForEach-Object { ($_ -split '\|')[0] })
$managed = @(Get-Content (Join-Path $repo 'compatibility/managed-public-api.txt') | Where-Object { $_ -match 'JYPPX\.OpenCvSharp\.XStereo' })
if ([int]$raw.declarationCount -ne 14 -or [int]$summary.callableCount -ne 10 -or [int]$summary.classCount -ne 3 -or [int]$summary.classificationCounts.implemented -ne 8 -or [int]$summary.classificationCounts.'intentionally-omitted' -ne 2 -or [int]$summary.classificationCounts.'non-callable-metadata' -ne 4 -or [int]$summary.classificationCounts.missing -ne 0) { throw 'XStereo declaration partition drifted.' }
if (@($raw.sourceHeaders).Count -ne 2 -or @($raw.sourceHeaders | Where-Object { $_.path -match '/xstereo/(descriptor|quasi_dense_stereo)\.hpp$' }).Count -ne 2) { throw 'XStereo parser source closure drifted.' }
$implemented = @($map.declarations | Where-Object { $_.classification -eq 'implemented' })
if ($implemented.Count -ne 8 -or @($implemented | Where-Object { @($_.nativeEntrypoints).Count -eq 0 -or @($_.managedMembers).Count -eq 0 -or @($_.nativeEntrypoints | Where-Object { $manifest -notcontains [string]$_ }).Count -ne 0 -or @($_.managedMembers | Where-Object { $managed -notcontains [string]$_ }).Count -ne 0 }).Count -ne 0) { throw 'XStereo implemented rows must retain exact native and managed evidence.' }
$omitted = @($map.declarations | Where-Object { $_.classification -eq 'intentionally-omitted' })
if ($omitted.Count -ne 2 -or @($omitted | Where-Object { @($_.nativeEntrypoints).Count -ne 0 -or @($_.managedMembers).Count -ne 0 }).Count -ne 0) { throw 'XStereo omission boundary drifted.' }
if ([int]$summary.nativeEvidenceCount -ne 10 -or [int]$summary.managedEvidenceCount -ne 9) { throw 'XStereo evidence counts drifted.' }
Write-Host "XSTEREO_UPSTREAM_MAP_CONTRACT_OK declarations=14 callables=10 implemented=8 omitted=2 missing=0 metadata=4 native=10 managed=9 sha256=$($summary.mappingSha256)"
