param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')))
Set-StrictMode -Version Latest;$ErrorActionPreference='Stop'
$repo=(Resolve-Path -LiteralPath $RepositoryRoot).Path
& (Join-Path $PSScriptRoot 'Generate-AlphaMatUpstreamMap.ps1') -RepositoryRoot $repo -Check
$summary=Get-Content (Join-Path $repo 'compatibility/alphamat-upstream-summary.json') -Raw|ConvertFrom-Json
$map=Get-Content (Join-Path $repo 'compatibility/alphamat-upstream-classifications.json') -Raw|ConvertFrom-Json
$raw=Get-Content (Join-Path $repo 'compatibility/alphamat-upstream-raw.json') -Raw|ConvertFrom-Json
$manifest=@(Get-Content (Join-Path $repo 'src/OpenCvSharp.Native/generated/native_abi_manifest.txt')|ForEach-Object {($_ -split '\|')[0]})
$row=@($map.declarations|Where-Object {$_.identity -like 'cv.alphamat.infoFlow(*'})
if([int]$summary.declarationCount -ne 1 -or [int]$summary.callableCount -ne 1 -or [int]$summary.classificationCounts.implemented -ne 1 -or [int]$summary.classificationCounts.'intentionally-omitted' -ne 0 -or [int]$summary.classificationCounts.missing -ne 0){throw 'AlphaMat upstream map partition drifted.'}
if(@($raw.sourceHeaders|Where-Object {$_.path -eq 'opencv-source/opencv_contrib-5.0.0/modules/alphamat/include/opencv2/alphamat.hpp'}).Count -ne 1){throw 'AlphaMat map must cover the pinned public umbrella header.'}
if($row.Count -ne 1 -or @($row[0].nativeEntrypoints) -notcontains 'jyppx_ocv_alphamat_info_flow' -or $manifest -notcontains 'jyppx_ocv_alphamat_info_flow'){throw 'AlphaMat parser declaration-to-native ABI mapping drifted.'}
if(@($row[0].managedMembers|Where-Object {$_ -match '\.AlphaMatCv2\|method\|.*\bInfoFlow\('}).Count -ne 2){throw 'AlphaMat managed InfoFlow overload evidence drifted.'}
Write-Host "ALPHAMAT_UPSTREAM_MAP_CONTRACT_OK declarations=1 callables=1 implemented=1 omitted=0 missing=0 managed_overloads=2 sha256=$($summary.mappingSha256)"
