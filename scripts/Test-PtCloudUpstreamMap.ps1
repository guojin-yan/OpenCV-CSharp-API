param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
& (Join-Path $repo 'scripts/Generate-PtCloudUpstreamMap.ps1') -RepositoryRoot $repo -Check
$raw = Get-Content (Join-Path $repo 'compatibility/ptcloud-upstream-raw.json') -Raw | ConvertFrom-Json
$summary = Get-Content (Join-Path $repo 'compatibility/ptcloud-upstream-summary.json') -Raw | ConvertFrom-Json
$map = Get-Content (Join-Path $repo 'compatibility/ptcloud-upstream-classifications.json') -Raw | ConvertFrom-Json
$manifest = @(Get-Content (Join-Path $repo 'src/OpenCvSharp.Native/generated/native_abi_manifest.txt') | Where-Object { $_ -match '^jyppx_ocv_(?:ptcloud|rgbd_normals)_' } | ForEach-Object { ($_ -split '\|')[0] })
$managed = @(Get-Content (Join-Path $repo 'compatibility/managed-public-api.txt') | Where-Object { $_ -match 'JYPPX\.OpenCvSharp\.PtCloud' })
if ([int]$raw.declarationCount -ne 22 -or @($raw.declarations).Count -ne 22 -or @($raw.sourceHeaders).Count -ne 1) { throw 'PtCloud parser source closure drifted.' }
if ([string]$raw.sourceHeaders[0].path -cne 'opencv-source/opencv-5.0.0/modules/ptcloud/include/opencv2/ptcloud/depth.hpp') { throw 'PtCloud selected source header drifted.' }
if (@($raw.excludedPublicHeaders).Count -ne 1 -or [string]$raw.excludedPublicHeaders[0].path -cne 'opencv-source/opencv-5.0.0/modules/ptcloud/include/opencv2/ptcloud.hpp') { throw 'PtCloud umbrella exclusion drifted.' }
if ([int]$summary.callableCount -ne 19 -or [int]$summary.classCount -ne 1 -or [int]$summary.enumCount -ne 2 -or [int]$summary.classificationCounts.implemented -ne 19 -or [int]$summary.classificationCounts.'intentionally-omitted' -ne 0 -or [int]$summary.classificationCounts.'non-callable-metadata' -ne 3 -or [int]$summary.classificationCounts.missing -ne 0) { throw 'PtCloud declaration partition drifted.' }
$implemented = @($map.declarations | Where-Object { $_.classification -eq 'implemented' })
if ($implemented.Count -ne 19) { throw 'PtCloud implemented callable count drifted.' }
if (@($implemented | Where-Object { @($_.nativeEntrypoints).Count -eq 0 -or @($_.managedMembers).Count -eq 0 -or @($_.nativeEntrypoints | Where-Object { $manifest -notcontains [string]$_ }).Count -ne 0 -or @($_.managedMembers | Where-Object { $managed -notcontains [string]$_ }).Count -ne 0 }).Count -ne 0) { throw 'PtCloud implemented rows must retain exact native and managed evidence.' }
if (@($manifest).Count -ne 14 -or [int]$summary.nativeEvidenceCount -ne 13 -or [int]$summary.managedEvidenceCount -ne 21) { throw 'PtCloud ABI evidence counts drifted.' }
if (@($map.declarations | Where-Object { [string]$_.classification -eq 'missing' }).Count -ne 0) { throw 'PtCloud map contains unexplained missing declarations.' }
Write-Host "PTCLOUD_UPSTREAM_MAP_CONTRACT_OK declarations=22 callables=19 implemented=19 omitted=0 missing=0 metadata=3 native=13 managed=21 sha256=$($summary.mappingSha256)"
