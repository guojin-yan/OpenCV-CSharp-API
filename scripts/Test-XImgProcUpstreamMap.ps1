param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$generator = Join-Path $repo 'scripts/Generate-XImgProcUpstreamMap.ps1'
& $generator -RepositoryRoot $repo -Check
if (-not $?) { throw 'XImgProc upstream map freshness validation failed.' }
$raw = Get-Content (Join-Path $repo 'compatibility/ximgproc-upstream-raw.json') -Raw | ConvertFrom-Json
$classes = Get-Content (Join-Path $repo 'compatibility/ximgproc-upstream-classifications.json') -Raw | ConvertFrom-Json
$summary = Get-Content (Join-Path $repo 'compatibility/ximgproc-upstream-summary.json') -Raw | ConvertFrom-Json
if ([int]$raw.declarationCount -ne 249 -or @($classes.declarations).Count -ne 249) { throw 'XImgProc declaration closure drifted.' }
if ([int]$summary.classificationCounts.missing -ne 0 -or [int]$summary.classificationCounts.implemented -ne 77 -or [int]$summary.classificationCounts.'intentionally-omitted' -ne 135) { throw 'XImgProc callable partition drifted.' }
$reviewedOrdinals = [int[]](@(41) + (72..95) + @(111,113,151,184) + (197..204))
$reviewedRows = @($classes.declarations | Where-Object { [int]$_.ordinal -in $reviewedOrdinals })
if ($reviewedRows.Count -ne 37 -or @($reviewedRows | Where-Object classification -ne 'implemented').Count -ne 0) { throw 'XImgProc existing property/factory evidence review drifted.' }
$quaternionRows = @($classes.declarations | Where-Object { [int]$_.ordinal -in @(0,1,2,3,4) })
if ($quaternionRows.Count -ne 5 -or @($quaternionRows | Where-Object { $_.classification -ne 'implemented' -or @($_.nativeEntrypoints).Count -ne 1 -or @($_.managedMembers).Count -lt 2 }).Count -ne 0) { throw 'XImgProc quaternion callable coverage drifted.' }
$methodAccessorEvidence = @(
    [pscustomobject]@{ Ordinal = 27; Native = 'jyppx_ocv_ximgproc_get_disparity_vis'; Managed = 'MEMBER|JYPPX.OpenCvSharp.XImgProc.XImgProcCv2|method|public;static|System.Void GetDisparityVis(JYPPX.OpenCvSharp.Core.Mat src,JYPPX.OpenCvSharp.Core.Mat dst,System.Double scale=1)' },
    [pscustomobject]@{ Ordinal = 35; Native = 'jyppx_ocv_ximgproc_edge_drawing_get_edge_image'; Managed = 'MEMBER|JYPPX.OpenCvSharp.XImgProc.EdgeDrawing|method|public;instance|System.Void GetEdgeImage(JYPPX.OpenCvSharp.Core.Mat dst)' },
    [pscustomobject]@{ Ordinal = 36; Native = 'jyppx_ocv_ximgproc_edge_drawing_get_gradient_image'; Managed = 'MEMBER|JYPPX.OpenCvSharp.XImgProc.EdgeDrawing|method|public;instance|System.Void GetGradientImage(JYPPX.OpenCvSharp.Core.Mat dst)' },
    [pscustomobject]@{ Ordinal = 134; Native = 'jyppx_ocv_ximgproc_scan_segment_get_labels'; Managed = 'MEMBER|JYPPX.OpenCvSharp.XImgProc.ScanSegment|method|public;instance|System.Void GetLabels(JYPPX.OpenCvSharp.Core.Mat labels)' },
    [pscustomobject]@{ Ordinal = 135; Native = 'jyppx_ocv_ximgproc_scan_segment_get_label_contour_mask'; Managed = 'MEMBER|JYPPX.OpenCvSharp.XImgProc.ScanSegment|method|public;instance|System.Void GetLabelContourMask(JYPPX.OpenCvSharp.Core.Mat image,System.Boolean thickLine=true)' },
    [pscustomobject]@{ Ordinal = 196; Native = 'jyppx_ocv_ximgproc_edge_aware_interpolator_set_cost_map'; Managed = 'MEMBER|JYPPX.OpenCvSharp.XImgProc.EdgeAwareInterpolator|method|public;instance|System.Void SetCostMap(JYPPX.OpenCvSharp.Core.Mat costMap)' }
)
foreach ($expected in $methodAccessorEvidence) {
    $row = @($classes.declarations | Where-Object { [int]$_.ordinal -eq $expected.Ordinal })
    if ($row.Count -ne 1 -or [string]$row[0].classification -ne 'implemented' -or @($row[0].nativeEntrypoints).Count -ne 1 -or [string]$row[0].nativeEntrypoints[0] -cne $expected.Native -or @($row[0].managedMembers) -cnotcontains $expected.Managed) {
        throw "XImgProc method/accessor evidence drifted at ordinal $($expected.Ordinal)."
    }
}
$native = @(Get-Content (Join-Path $repo 'src/OpenCvSharp.Native/generated/native_abi_manifest.txt') | Where-Object { $_ -match '^jyppx_ocv_ximgproc_' } | ForEach-Object { ($_ -split '\|')[0] })
$managed = @(Get-Content (Join-Path $repo 'compatibility/managed-public-api.txt') | Where-Object { $_ -match 'JYPPX\.OpenCvSharp\.XImgProc' })
foreach ($row in @($classes.declarations)) {
    if ([string]::IsNullOrWhiteSpace([string]$row.identity) -or [string]::IsNullOrWhiteSpace([string]$row.reason)) { throw "XImgProc classification row is incomplete: $($row.ordinal)" }
    foreach ($entry in @($row.nativeEntrypoints)) { if ($native -notcontains [string]$entry) { throw "XImgProc native evidence is not in manifest: $entry" } }
    foreach ($entry in @($row.managedMembers)) { if ($managed -notcontains [string]$entry) { throw 'XImgProc managed evidence is not in managed baseline.' } }
    if ([string]$row.classification -eq 'implemented' -and (@($row.nativeEntrypoints).Count -eq 0 -or @($row.managedMembers).Count -eq 0)) { throw "Implemented XImgProc row lacks evidence: $($row.ordinal)" }
}
Write-Host "XIMGPROC_UPSTREAM_MAP_CONTRACT_OK declarations=249 callables=$($summary.callableCount) implemented=$($summary.classificationCounts.implemented) omitted=$($summary.classificationCounts.'intentionally-omitted') missing=0 fixtures=$($summary.negativeFixtureCount) sha256=$($summary.mappingSha256)"
