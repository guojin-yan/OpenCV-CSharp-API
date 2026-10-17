param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
& (Join-Path $PSScriptRoot 'Generate-RgbdUpstreamMap.ps1') -RepositoryRoot $repo -Check

$summary = Get-Content (Join-Path $repo 'compatibility/rgbd-upstream-summary.json') -Raw | ConvertFrom-Json
$raw = Get-Content (Join-Path $repo 'compatibility/rgbd-upstream-raw.json') -Raw | ConvertFrom-Json
$map = Get-Content (Join-Path $repo 'compatibility/rgbd-upstream-classifications.json') -Raw | ConvertFrom-Json
$manifest = @(Get-Content (Join-Path $repo 'src/OpenCvSharp.Native/generated/native_abi_manifest.txt') | ForEach-Object { ($_ -split '\|')[0] })
$managed = Get-Content (Join-Path $repo 'compatibility/managed-public-api.txt') -Raw
$headers = @(
    'opencv-source/opencv_contrib-5.0.0/modules/rgbd/include/opencv2/rgbd.hpp',
    'opencv-source/opencv_contrib-5.0.0/modules/rgbd/include/opencv2/rgbd/colored_kinfu.hpp',
    'opencv-source/opencv_contrib-5.0.0/modules/rgbd/include/opencv2/rgbd/dynafu.hpp',
    'opencv-source/opencv_contrib-5.0.0/modules/rgbd/include/opencv2/rgbd/kinfu.hpp',
    'opencv-source/opencv_contrib-5.0.0/modules/rgbd/include/opencv2/rgbd/large_kinfu.hpp',
    'opencv-source/opencv_contrib-5.0.0/modules/rgbd/include/opencv2/rgbd/linemod.hpp'
)

if ([string]$raw.headerPath -cne $headers[0] -or @($raw.sourceHeaders).Count -ne $headers.Count -or ((@($raw.sourceHeaders | ForEach-Object path) -join '|') -cne ($headers -join '|'))) { throw 'RGBD parser source closure drifted.' }
if ([int]$raw.declarationCount -ne 105 -or [int]$summary.declarationCount -ne 105 -or [int]$summary.callableCount -ne 88 -or [int]$summary.classCount -ne 17 -or [int]$summary.enumCount -ne 0 -or [int]$summary.classificationCounts.implemented -ne 0 -or [int]$summary.classificationCounts.'intentionally-omitted' -ne 88 -or [int]$summary.classificationCounts.'non-callable-metadata' -ne 17 -or [int]$summary.classificationCounts.missing -ne 0) { throw 'RGBD declaration partition drifted.' }
if (@($map.declarations | Where-Object { $_.classification -eq 'intentionally-omitted' -and [string]$_.identity -match '^cv\.(?:colored_kinfu|dynafu|kinfu|large_kinfu|linemod)\.' -and @($_.nativeEntrypoints).Count -eq 0 -and @($_.managedMembers).Count -eq 0 -and [string]$_.reason -match 'no native RGBD wrapper entrypoint' -and [string]$_.reason -match 'managed public owner' }).Count -ne 88) { throw 'RGBD callable classification closure drifted.' }
if ([int]$summary.nativeEvidenceCount -ne 0 -or [int]$summary.managedEvidenceCount -ne 0 -or [int]$summary.externalDataDependencyCount -ne 2 -or @($manifest | Where-Object { $_ -match '^jyppx_ocv_rgbd_(?!normals_)' }).Count -ne 0 -or $managed -match 'JYPPX\.OpenCvSharp\.Rgbd(?:\.|\|)') { throw 'RGBD map falsely claims a native or managed wrapper surface.' }
if ([bool]$summary.repositoryWideUpstreamParityClaimed) { throw 'RGBD map must not claim repository-wide parity.' }

Write-Host "RGBD_UPSTREAM_MAP_CONTRACT_OK declarations=$($summary.declarationCount) callables=$($summary.callableCount) implemented=0 omitted=$($summary.classificationCounts.'intentionally-omitted') missing=0 metadata=$($summary.classificationCounts.'non-callable-metadata') native=0 managed=0 external_dependencies=2 sha256=$($summary.mappingSha256)"
