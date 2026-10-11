param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
& (Join-Path $PSScriptRoot 'Generate-WechatQRCodeUpstreamMap.ps1') -RepositoryRoot $repo -Check

$summary = Get-Content (Join-Path $repo 'compatibility/wechat_qrcode-upstream-summary.json') -Raw | ConvertFrom-Json
$raw = Get-Content (Join-Path $repo 'compatibility/wechat_qrcode-upstream-raw.json') -Raw | ConvertFrom-Json
$map = Get-Content (Join-Path $repo 'compatibility/wechat_qrcode-upstream-classifications.json') -Raw | ConvertFrom-Json
$manifest = @(Get-Content (Join-Path $repo 'src/OpenCvSharp.Native/generated/native_abi_manifest.txt') | ForEach-Object { ($_ -split '\|')[0] })
$managed = Get-Content (Join-Path $repo 'compatibility/managed-public-api.txt') -Raw
$header = 'opencv-source/opencv_contrib-5.0.0/modules/wechat_qrcode/include/opencv2/wechat_qrcode.hpp'

if ([string]$raw.headerPath -cne $header -or @($raw.sourceHeaders).Count -ne 1 -or [int]$raw.sourceHeaders[0].startOrdinal -ne 0 -or [int]$raw.sourceHeaders[0].declarationCount -ne 5) { throw 'WechatQRCode parser source closure drifted.' }
if ([int]$raw.declarationCount -ne 5 -or [int]$summary.declarationCount -ne 5 -or [int]$summary.callableCount -ne 4 -or [int]$summary.classCount -ne 1 -or [int]$summary.enumCount -ne 0 -or [int]$summary.classificationCounts.implemented -ne 0 -or [int]$summary.classificationCounts.'intentionally-omitted' -ne 4 -or [int]$summary.classificationCounts.'non-callable-metadata' -ne 1 -or [int]$summary.classificationCounts.missing -ne 0) { throw 'WechatQRCode declaration partition drifted.' }
if (@($map.declarations).Count -ne 5 -or @($map.declarations | Where-Object { $_.classification -eq 'intentionally-omitted' }).Count -ne 4 -or @($map.declarations | Where-Object { $_.classification -eq 'missing' }).Count -ne 0) { throw 'WechatQRCode classification closure drifted.' }
if (@($map.declarations | Where-Object { $_.classification -eq 'intentionally-omitted' -and (@($_.nativeEntrypoints).Count -ne 0 -or @($_.managedMembers).Count -ne 0 -or [string]$_.reason -notmatch 'model-driven' -or [string]$_.reason -notmatch 'no native WeChatQRCode wrapper entrypoint' -or [string]$_.reason -notmatch 'caller-supplied' -or [string]$_.reason -notmatch 'decoded-string ownership') }).Count -ne 0) { throw 'WechatQRCode omissions must retain the reviewed model/ownership boundary.' }
if (@($map.declarations | Where-Object { $_.classification -eq 'non-callable-metadata' -and [string]$_.identity -ceq 'class cv.wechat_qrcode.WeChatQRCode' }).Count -ne 1) { throw 'WechatQRCode class metadata closure drifted.' }
if ([int]$summary.nativeEvidenceCount -ne 0 -or [int]$summary.managedEvidenceCount -ne 0 -or @($manifest | Where-Object { $_ -match '^jyppx_ocv_wechat_qrcode_' }).Count -ne 0 -or $managed -match 'JYPPX\.OpenCvSharp\.WechatQRCode') { throw 'WechatQRCode map falsely claims a native or managed wrapper surface.' }
if (@($map.externalDataDependencies | Where-Object { [string]$_ -match 'detector and super-resolution ONNX model files' -and [string]$_ -match 'no model assets' }).Count -ne 1) { throw 'WechatQRCode model dependency boundary drifted.' }
if ([bool]$summary.repositoryWideUpstreamParityClaimed) { throw 'WechatQRCode map must not claim repository-wide parity.' }

Write-Host "WECHAT_QRCODE_UPSTREAM_MAP_CONTRACT_OK declarations=$($summary.declarationCount) callables=$($summary.callableCount) implemented=0 omitted=$($summary.classificationCounts.'intentionally-omitted') missing=0 metadata=$($summary.classificationCounts.'non-callable-metadata') native=0 managed=0 model_dependency=true sha256=$($summary.mappingSha256)"
