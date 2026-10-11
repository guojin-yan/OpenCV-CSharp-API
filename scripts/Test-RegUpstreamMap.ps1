param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
& (Join-Path $PSScriptRoot 'Generate-RegUpstreamMap.ps1') -RepositoryRoot $repo -Check
$raw = Get-Content (Join-Path $repo 'compatibility/reg-upstream-raw.json') -Raw | ConvertFrom-Json
$summary = Get-Content (Join-Path $repo 'compatibility/reg-upstream-summary.json') -Raw | ConvertFrom-Json
$map = Get-Content (Join-Path $repo 'compatibility/reg-upstream-classifications.json') -Raw | ConvertFrom-Json
$manifest = @(Get-Content (Join-Path $repo 'src/OpenCvSharp.Native/generated/native_abi_manifest.txt') | Where-Object { $_ -match '^jyppx_ocv_reg_' } | ForEach-Object { ($_ -split '\|')[0] })
$managedBaseline = @(Get-Content (Join-Path $repo 'compatibility/managed-public-api.txt'))
$expectedCounts = '6|9|3|4|4|4|4|4|8|9|8'
$sourceCounts = @($raw.sourceHeaders | ForEach-Object { [int]$_.declarationCount }) -join '|'
if ([string]$raw.headerPath -cne 'opencv-source/opencv_contrib-5.0.0/modules/reg/include/opencv2/reg/map.hpp' -or @($raw.sourceHeaders).Count -ne 11 -or $sourceCounts -cne $expectedCounts) { throw 'Reg parser source closure drifted.' }
if ([int]$raw.declarationCount -ne 63 -or [int]$summary.callableCount -ne 51 -or [int]$summary.classCount -ne 12 -or [int]$summary.classificationCounts.implemented -ne 48 -or [int]$summary.classificationCounts.'intentionally-omitted' -ne 3 -or [int]$summary.classificationCounts.'non-callable-metadata' -ne 12 -or [int]$summary.classificationCounts.missing -ne 0) { throw 'Reg declaration partition drifted.' }
$implemented = @($map.declarations | Where-Object { $_.classification -eq 'implemented' })
if ($implemented.Count -ne 48) { throw 'Reg implemented callable count drifted.' }
if (@($implemented | Where-Object { @($_.nativeEntrypoints).Count -eq 0 -or @($_.managedMembers).Count -eq 0 -or @($_.nativeEntrypoints | Where-Object { $manifest -notcontains [string]$_ }).Count -ne 0 -or @($_.managedMembers | Where-Object { $managedBaseline -notcontains [string]$_ }).Count -ne 0 }).Count -ne 0) { throw 'Reg implemented rows must retain exact native and managed evidence.' }
$omitted = @($map.declarations | Where-Object { $_.classification -eq 'intentionally-omitted' })
if (@($omitted | Where-Object { @($_.nativeEntrypoints).Count -ne 0 -or @($_.managedMembers).Count -ne 0 -or [string]$_.identity -notmatch '^cv\.reg\.MapTypeCaster\.' }).Count -ne 0) { throw 'Reg omission boundary drifted.' }
if (@($manifest).Count -ne 27 -or [int]$summary.nativeEvidenceCount -ne 20 -or [int]$summary.managedEvidenceCount -ne 22) { throw 'Reg ABI evidence counts drifted.' }
Write-Host "REG_UPSTREAM_MAP_CONTRACT_OK declarations=63 callables=51 implemented=48 omitted=3 missing=0 metadata=12 native=20 managed=22 sha256=$($summary.mappingSha256)"
