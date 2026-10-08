param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$generator = Join-Path $repo 'scripts/Generate-XPhotoUpstreamMap.ps1'
& $generator -RepositoryRoot $repo -Check
if (-not $?) { throw 'XPhoto upstream map freshness or negative-fixture validation failed.' }
$rawPath = Join-Path $repo 'compatibility/xphoto-upstream-raw.json'
$classPath = Join-Path $repo 'compatibility/xphoto-upstream-classifications.json'
$summaryPath = Join-Path $repo 'compatibility/xphoto-upstream-summary.json'
$familyPath = Join-Path $repo 'compatibility/xphoto-implemented-families.json'
$raw = Get-Content -LiteralPath $rawPath -Raw | ConvertFrom-Json
$classes = Get-Content -LiteralPath $classPath -Raw | ConvertFrom-Json
$summary = Get-Content -LiteralPath $summaryPath -Raw | ConvertFrom-Json
if ([int]$raw.declarationCount -ne 47 -or @($raw.declarations).Count -ne 47 -or @($classes.declarations).Count -ne 47) { throw 'XPhoto declaration count or classification closure drifted.' }
if ([int]$summary.classificationCounts.missing -ne 0 -or [int]$summary.classificationCounts.implemented -ne 30 -or [int]$summary.classificationCounts.'intentionally-omitted' -ne 9) { throw 'XPhoto callable partition drifted.' }
$inpaint = @($classes.declarations | Where-Object { [string]$_.identity -like 'cv.xphoto.inpaint(*' })
if ($inpaint.Count -ne 1 -or [string]$inpaint[0].classification -cne 'implemented' -or
    @($inpaint[0].nativeEntrypoints).Count -ne 1 -or [string]$inpaint[0].nativeEntrypoints[0] -cne 'jyppx_ocv_xphoto_inpaint' -or
    @($inpaint[0].managedMembers).Count -ne 2 -or @($inpaint[0].managedMembers | Where-Object { $_ -match '\|method\|public;static\|.* Inpaint\(' }).Count -ne 2) {
    throw 'XPhoto inpaint must remain bound to its exact native entrypoint and both managed overloads.'
}
$durandRows = @($classes.declarations | Where-Object { [int]$_.ordinal -ge 10 -and [int]$_.ordinal -le 18 })
$expectedDurandIdentities = @(
    'cv.xphoto.TonemapDurand.getSaturation()->float',
    'cv.xphoto.TonemapDurand.setSaturation(float saturation)->void',
    'cv.xphoto.TonemapDurand.getContrast()->float',
    'cv.xphoto.TonemapDurand.setContrast(float contrast)->void',
    'cv.xphoto.TonemapDurand.getSigmaSpace()->float',
    'cv.xphoto.TonemapDurand.setSigmaSpace(float sigma_space)->void',
    'cv.xphoto.TonemapDurand.getSigmaColor()->float',
    'cv.xphoto.TonemapDurand.setSigmaColor(float sigma_color)->void',
    'cv.xphoto.createTonemapDurand(float gamma=1.0f;float contrast=4.0f;float saturation=1.0f;float sigma_color=2.0f;float sigma_space=2.0f)->Ptr<TonemapDurand>'
)
$actualDurandIdentities = @($durandRows | ForEach-Object { [string]$_.identity })
$durandRawFactory = @($raw.declarations | Where-Object { [int]$_.ordinal -eq 18 })
if ($durandRows.Count -ne 9 -or $actualDurandIdentities.Count -ne $expectedDurandIdentities.Count -or
    @($actualDurandIdentities | Sort-Object -Unique).Count -ne 9 -or
    @($actualDurandIdentities | Where-Object { $expectedDurandIdentities -cnotcontains $_ }).Count -ne 0 -or
    @($expectedDurandIdentities | Where-Object { $actualDurandIdentities -cnotcontains $_ }).Count -ne 0 -or
    @($durandRows | Where-Object { $_.classification -cne 'intentionally-omitted' -or @($_.nativeEntrypoints).Count -ne 0 -or @($_.managedMembers).Count -ne 0 }).Count -ne 0 -or
    @($durandRows | Where-Object { -not ([string]$_.reason -match 'OPENCV_ENABLE_NONFREE') }).Count -ne 0 -or
    $durandRawFactory.Count -ne 1 -or [string]$durandRawFactory[0].documentation -notmatch 'OPENCV_ENABLE_NONFREE') {
    throw 'XPhoto TonemapDurand nonfree-gated omission review drifted.'
}
$nativeManifest = @(Get-Content (Join-Path $repo 'src/OpenCvSharp.Native/generated/native_abi_manifest.txt') | Where-Object { $_ -match '^jyppx_ocv_xphoto_' } | ForEach-Object { ($_ -split '\|')[0] })
$managed = @(Get-Content (Join-Path $repo 'compatibility/managed-public-api.txt') | Where-Object { $_ -match 'JYPPX\.OpenCvSharp\.XPhoto' })
foreach ($row in @($classes.declarations)) {
    if ([int]$row.ordinal -lt 0 -or [string]::IsNullOrWhiteSpace([string]$row.identity) -or [string]::IsNullOrWhiteSpace([string]$row.reason)) { throw "XPhoto classification row is incomplete: $($row.ordinal)" }
    foreach ($entry in @($row.nativeEntrypoints)) { if ($nativeManifest -notcontains [string]$entry) { throw "XPhoto native evidence is not in the manifest: $entry" } }
    foreach ($entry in @($row.managedMembers)) { if ($managed -notcontains [string]$entry) { throw 'XPhoto managed evidence is not in the managed baseline.' } }
    if ([string]$row.classification -eq 'implemented' -and (@($row.nativeEntrypoints).Count -eq 0 -or @($row.managedMembers).Count -eq 0)) { throw "Implemented XPhoto row lacks evidence: $($row.ordinal)" }
}
if (@($summary.negativeFixtureCount) -lt 12 -or @((Get-Content $familyPath -Raw | ConvertFrom-Json).families).Count -ne 1) { throw 'XPhoto negative fixture or family evidence drifted.' }
Write-Host "XPHOTO_UPSTREAM_MAP_CONTRACT_OK declarations=47 callables=39 implemented=30 omitted=9 missing=0 fixtures=$($summary.negativeFixtureCount) sha256=$($summary.mappingSha256)"
