param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path)
Set-StrictMode -Version Latest;$ErrorActionPreference='Stop'
$repo=(Resolve-Path -LiteralPath $RepositoryRoot).Path
& (Join-Path $repo 'scripts/Generate-XObjDetectUpstreamMap.ps1') -RepositoryRoot $repo -Check
if(-not $?){throw 'XObjDetect generated-file freshness check failed.'}
$raw=Get-Content (Join-Path $repo 'compatibility/xobjdetect-upstream-raw.json') -Raw|ConvertFrom-Json
$summary=Get-Content (Join-Path $repo 'compatibility/xobjdetect-upstream-summary.json') -Raw|ConvertFrom-Json
$map=Get-Content (Join-Path $repo 'compatibility/xobjdetect-upstream-classifications.json') -Raw|ConvertFrom-Json
$manifest=@(Get-Content (Join-Path $repo 'src/OpenCvSharp.Native/generated/native_abi_manifest.txt')|Where-Object{$_ -match '^jyppx_ocv_(cascade_classifier|hog_descriptor)_'}|ForEach-Object{($_ -split '\|')[0]})
$managed=@(Get-Content (Join-Path $repo 'compatibility/managed-public-api.txt')|Where-Object{$_ -match 'JYPPX\.OpenCvSharp\.XObjDetect'})
$implemented=@($map.declarations|Where-Object{$_.classification -eq 'implemented'})
if(@($raw.sourceHeaders).Count -ne 1 -or [int]$raw.declarationCount -ne [int]$summary.declarationCount){throw 'XObjDetect parser closure drifted.'}
if([int]$raw.declarationCount -ne 34 -or [int]$summary.callableCount -ne 27 -or [int]$summary.classCount -ne 3 -or [int]$summary.enumCount -ne 4 -or [int]$summary.classificationCounts.implemented -ne 20 -or [int]$summary.classificationCounts.'intentionally-omitted' -ne 7 -or [int]$summary.classificationCounts.'non-callable-metadata' -ne 7){throw 'XObjDetect declaration partition drifted.'}
if([int]$summary.classificationCounts.missing -ne 0 -or @($map.declarations).Count -ne [int]$raw.declarationCount){throw 'XObjDetect classification completeness drifted.'}
if(@($implemented|Where-Object{@($_.nativeEntrypoints).Count -eq 0 -or @($_.managedMembers).Count -eq 0 -or @($_.nativeEntrypoints|Where-Object{$manifest -notcontains [string]$_}).Count -ne 0 -or @($_.managedMembers|Where-Object{$managed -notcontains [string]$_}).Count -ne 0}).Count -ne 0){throw 'XObjDetect implemented rows must retain exact native and managed evidence.'}
if([int]$summary.nativeEvidenceCount -ne 27 -or [int]$summary.managedEvidenceCount -ne 21){throw 'XObjDetect evidence counts drifted.'}
Write-Host "XOBJDETECT_UPSTREAM_MAP_CONTRACT_OK declarations=$($summary.declarationCount) callable=$($summary.callableCount) implemented=$($summary.classificationCounts.implemented) missing=$($summary.classificationCounts.missing) omitted=$($summary.classificationCounts.'intentionally-omitted') metadata=$($summary.classificationCounts.'non-callable-metadata') native=$($summary.nativeEvidenceCount) managed=$($summary.managedEvidenceCount) sha256=$($summary.mappingSha256)"
