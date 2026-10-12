param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
& (Join-Path $PSScriptRoot 'Generate-SignalUpstreamMap.ps1') -RepositoryRoot $repo -Check

$summary = Get-Content (Join-Path $repo 'compatibility/signal-upstream-summary.json') -Raw | ConvertFrom-Json
$raw = Get-Content (Join-Path $repo 'compatibility/signal-upstream-raw.json') -Raw | ConvertFrom-Json
$map = Get-Content (Join-Path $repo 'compatibility/signal-upstream-classifications.json') -Raw | ConvertFrom-Json
$manifest = @(Get-Content (Join-Path $repo 'src/OpenCvSharp.Native/generated/native_abi_manifest.txt') | ForEach-Object { ($_ -split '\|')[0] })
$managed = Get-Content (Join-Path $repo 'compatibility/managed-public-api.txt') -Raw
$header = 'opencv-source/opencv_contrib-5.0.0/modules/signal/include/opencv2/signal.hpp'
$sourceHeader = 'opencv-source/opencv_contrib-5.0.0/modules/signal/include/opencv2/signal/signal_resample.hpp'

if ([string]$raw.headerPath -cne $header -or @($raw.sourceHeaders).Count -ne 1 -or [string]$raw.sourceHeaders[0].path -cne $sourceHeader -or [int]$raw.sourceHeaders[0].startOrdinal -ne 0 -or [int]$raw.sourceHeaders[0].declarationCount -ne 1) { throw 'Signal parser source closure drifted.' }
if ([int]$raw.declarationCount -ne 1 -or [int]$summary.declarationCount -ne 1 -or [int]$summary.callableCount -ne 1 -or [int]$summary.classCount -ne 0 -or [int]$summary.enumCount -ne 0 -or [int]$summary.classificationCounts.implemented -ne 0 -or [int]$summary.classificationCounts.'intentionally-omitted' -ne 1 -or [int]$summary.classificationCounts.'non-callable-metadata' -ne 0 -or [int]$summary.classificationCounts.missing -ne 0) { throw 'Signal declaration partition drifted.' }
if (@($map.declarations).Count -ne 1 -or @($map.declarations | Where-Object { $_.classification -eq 'intentionally-omitted' -and [string]$_.identity -match '^cv\.signal\.resampleSignal\(' -and @($_.nativeEntrypoints).Count -eq 0 -and @($_.managedMembers).Count -eq 0 -and [string]$_.reason -match 'no native Signal wrapper entrypoint' -and [string]$_.reason -match 'managed public owner' }).Count -ne 1) { throw 'Signal classification closure drifted.' }
if ([int]$summary.nativeEvidenceCount -ne 0 -or [int]$summary.managedEvidenceCount -ne 0 -or [int]$summary.externalDataDependencyCount -ne 0 -or @($manifest | Where-Object { $_ -match '^jyppx_ocv_signal_' }).Count -ne 0 -or $managed -match 'JYPPX\.OpenCvSharp\.Signal') { throw 'Signal map falsely claims a native or managed wrapper surface.' }
if ([bool]$summary.repositoryWideUpstreamParityClaimed) { throw 'Signal map must not claim repository-wide parity.' }

Write-Host "SIGNAL_UPSTREAM_MAP_CONTRACT_OK declarations=$($summary.declarationCount) callables=$($summary.callableCount) implemented=0 omitted=$($summary.classificationCounts.'intentionally-omitted') missing=0 metadata=0 native=0 managed=0 external_dependencies=0 sha256=$($summary.mappingSha256)"
