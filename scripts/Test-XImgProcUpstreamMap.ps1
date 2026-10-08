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
if ([int]$summary.classificationCounts.missing -ne 0 -or [int]$summary.classificationCounts.implemented -ne 189 -or [int]$summary.classificationCounts.'intentionally-omitted' -ne 23) { throw 'XImgProc callable partition drifted.' }
$reviewedOrdinals = [int[]](@(41) + (72..95) + @(111,113,151,184) + (197..204))
$reviewedRows = @($classes.declarations | Where-Object { [int]$_.ordinal -in $reviewedOrdinals })
if ($reviewedRows.Count -ne 37 -or @($reviewedRows | Where-Object classification -ne 'implemented').Count -ne 0) { throw 'XImgProc existing property/factory evidence review drifted.' }
$quaternionRows = @($classes.declarations | Where-Object { [int]$_.ordinal -in @(0,1,2,3,4) })
if ($quaternionRows.Count -ne 5 -or @($quaternionRows | Where-Object { $_.classification -ne 'implemented' -or @($_.nativeEntrypoints).Count -ne 1 -or @($_.managedMembers).Count -lt 1 }).Count -ne 0) { throw 'XImgProc quaternion callable coverage drifted.' }
$unwrappedUtilityRows = @(
    [pscustomobject]@{ Ordinal = 5; Reason = 'three-channel color-template/DFT|CV_64F result|image-format validation|numerical smoke' },
    [pscustomobject]@{ Ordinal = 24; Reason = 'filesystem path|file-IO helper|ground-truth|provenance' },
    [pscustomobject]@{ Ordinal = 108; Reason = 'variable-length ellipse|six-float|output-buffer|deterministic smoke' },
    [pscustomobject]@{ Ordinal = 127; Reason = 'type- and geometry-dependent|CV_32S|CV_64F|output contract' }
)
foreach ($expected in $unwrappedUtilityRows) {
    $row = @($classes.declarations | Where-Object { [int]$_.ordinal -eq $expected.Ordinal })
    if ($row.Count -ne 1 -or $row[0].classification -ne 'intentionally-omitted' -or @($row[0].nativeEntrypoints).Count -ne 0 -or @($row[0].managedMembers).Count -ne 0 -or ([string]$row[0].reason -notmatch $expected.Reason)) {
        throw "XImgProc unwrapped utility omission review drifted at ordinal $($expected.Ordinal)."
    }
}
$methodAccessorEvidence = @(
    [pscustomobject]@{ Ordinal = 27; Native = @('jyppx_ocv_ximgproc_get_disparity_vis'); Managed = @('MEMBER|JYPPX.OpenCvSharp.XImgProc.XImgProcCv2|method|public;static|System.Void GetDisparityVis(JYPPX.OpenCvSharp.Core.Mat src,JYPPX.OpenCvSharp.Core.Mat dst,System.Double scale=1)') },
    [pscustomobject]@{ Ordinal = 15; Native = @('jyppx_ocv_ximgproc_disparity_wls_filter_get_lrc_thresh'); Managed = @('MEMBER|JYPPX.OpenCvSharp.XImgProc.DisparityWLSFilter|property|instance;get:public;set:public|System.Int32 LrcThreshold') },
    [pscustomobject]@{ Ordinal = 16; Native = @('jyppx_ocv_ximgproc_disparity_wls_filter_set_lrc_thresh'); Managed = @('MEMBER|JYPPX.OpenCvSharp.XImgProc.DisparityWLSFilter|property|instance;get:public;set:public|System.Int32 LrcThreshold') },
    [pscustomobject]@{ Ordinal = 21; Native = @('jyppx_ocv_ximgproc_disparity_wls_filter_create_from_stereo_bm','jyppx_ocv_ximgproc_disparity_wls_filter_create_from_stereo_sgbm','jyppx_ocv_ximgproc_disparity_wls_filter_create_from_stereo_matcher'); Managed = @('MEMBER|JYPPX.OpenCvSharp.XImgProc.XImgProcCv2|method|public;static|JYPPX.OpenCvSharp.XImgProc.DisparityWLSFilter CreateDisparityWLSFilter(JYPPX.OpenCvSharp.Calib3D.StereoBM matcherLeft)','MEMBER|JYPPX.OpenCvSharp.XImgProc.XImgProcCv2|method|public;static|JYPPX.OpenCvSharp.XImgProc.DisparityWLSFilter CreateDisparityWLSFilter(JYPPX.OpenCvSharp.Calib3D.StereoSGBM matcherLeft)','MEMBER|JYPPX.OpenCvSharp.XImgProc.XImgProcCv2|method|public;static|JYPPX.OpenCvSharp.XImgProc.DisparityWLSFilter CreateDisparityWLSFilter(JYPPX.OpenCvSharp.Calib3D.StereoMatcher matcherLeft)') },
    [pscustomobject]@{ Ordinal = 22; Native = @('jyppx_ocv_ximgproc_create_right_matcher_from_stereo_bm','jyppx_ocv_ximgproc_create_right_matcher_from_stereo_sgbm','jyppx_ocv_ximgproc_create_right_matcher_from_stereo_matcher'); Managed = @('MEMBER|JYPPX.OpenCvSharp.XImgProc.XImgProcCv2|method|public;static|JYPPX.OpenCvSharp.Calib3D.StereoMatcher CreateRightMatcher(JYPPX.OpenCvSharp.Calib3D.StereoBM matcherLeft)','MEMBER|JYPPX.OpenCvSharp.XImgProc.XImgProcCv2|method|public;static|JYPPX.OpenCvSharp.Calib3D.StereoMatcher CreateRightMatcher(JYPPX.OpenCvSharp.Calib3D.StereoSGBM matcherLeft)','MEMBER|JYPPX.OpenCvSharp.XImgProc.XImgProcCv2|method|public;static|JYPPX.OpenCvSharp.Calib3D.StereoMatcher CreateRightMatcher(JYPPX.OpenCvSharp.Calib3D.StereoMatcher matcherLeft)') },
    [pscustomobject]@{ Ordinal = 23; Native = @('jyppx_ocv_ximgproc_disparity_wls_filter_create_generic'); Managed = @('MEMBER|JYPPX.OpenCvSharp.XImgProc.XImgProcCv2|method|public;static|JYPPX.OpenCvSharp.XImgProc.DisparityWLSFilter CreateDisparityWLSFilterGeneric(System.Boolean useConfidence=false)') },
    [pscustomobject]@{ Ordinal = 35; Native = @('jyppx_ocv_ximgproc_edge_drawing_get_edge_image'); Managed = @('MEMBER|JYPPX.OpenCvSharp.XImgProc.EdgeDrawing|method|public;instance|System.Void GetEdgeImage(JYPPX.OpenCvSharp.Core.Mat dst)') },
    [pscustomobject]@{ Ordinal = 36; Native = @('jyppx_ocv_ximgproc_edge_drawing_get_gradient_image'); Managed = @('MEMBER|JYPPX.OpenCvSharp.XImgProc.EdgeDrawing|method|public;instance|System.Void GetGradientImage(JYPPX.OpenCvSharp.Core.Mat dst)') },
    [pscustomobject]@{ Ordinal = 37; Native = @('jyppx_ocv_ximgproc_edge_drawing_get_segments_count','jyppx_ocv_ximgproc_edge_drawing_get_segments_fill'); Managed = @('MEMBER|JYPPX.OpenCvSharp.XImgProc.EdgeDrawing|method|public;instance|JYPPX.OpenCvSharp.Core.Point[][] GetSegments()') },
    [pscustomobject]@{ Ordinal = 38; Native = @('jyppx_ocv_ximgproc_edge_drawing_get_segment_indices_of_lines_count','jyppx_ocv_ximgproc_edge_drawing_get_segment_indices_of_lines_fill'); Managed = @('MEMBER|JYPPX.OpenCvSharp.XImgProc.EdgeDrawing|method|public;instance|System.Int32[] GetSegmentIndicesOfLines()') },
    [pscustomobject]@{ Ordinal = 51; Native = @('jyppx_ocv_ximgproc_guided_filter_run'); Managed = @('MEMBER|JYPPX.OpenCvSharp.XImgProc.XImgProcCv2|method|public;static|System.Void GuidedFilter(JYPPX.OpenCvSharp.Core.Mat guide,JYPPX.OpenCvSharp.Core.Mat src,JYPPX.OpenCvSharp.Core.Mat dst,System.Int32 radius,System.Double eps,System.Int32 dDepth=-1,System.Double scale=1)') },
    [pscustomobject]@{ Ordinal = 64; Native = @('jyppx_ocv_ximgproc_fast_bilateral_solver_filter_run'); Managed = @('MEMBER|JYPPX.OpenCvSharp.XImgProc.XImgProcCv2|method|public;static|System.Void FastBilateralSolverFilter(JYPPX.OpenCvSharp.Core.Mat guide,JYPPX.OpenCvSharp.Core.Mat src,JYPPX.OpenCvSharp.Core.Mat confidence,JYPPX.OpenCvSharp.Core.Mat dst,System.Double sigmaSpatial=8,System.Double sigmaLuma=8,System.Double sigmaChroma=8,System.Double lambda=128,System.Int32 numIter=25,System.Double maxTol=1E-05)') },
    [pscustomobject]@{ Ordinal = 68; Native = @('jyppx_ocv_ximgproc_fast_global_smoother_filter_run'); Managed = @('MEMBER|JYPPX.OpenCvSharp.XImgProc.XImgProcCv2|method|public;static|System.Void FastGlobalSmootherFilter(JYPPX.OpenCvSharp.Core.Mat guide,JYPPX.OpenCvSharp.Core.Mat src,JYPPX.OpenCvSharp.Core.Mat dst,System.Double lambda,System.Double sigmaColor,System.Double lambdaAttenuation=0.25,System.Int32 numIter=3)') },
    [pscustomobject]@{ Ordinal = 71; Native = @('jyppx_ocv_ximgproc_edge_boxes_get_bounding_boxes_count','jyppx_ocv_ximgproc_edge_boxes_get_bounding_boxes_fill'); Managed = @('MEMBER|JYPPX.OpenCvSharp.XImgProc.EdgeBoxes|method|public;instance|JYPPX.OpenCvSharp.XImgProc.EdgeBox[] GetBoundingBoxes(JYPPX.OpenCvSharp.Core.Mat edgeMap,JYPPX.OpenCvSharp.Core.Mat orientationMap)') },
    [pscustomobject]@{ Ordinal = 103; Native = @('jyppx_ocv_ximgproc_hough_point_to_line'); Managed = @('MEMBER|JYPPX.OpenCvSharp.XImgProc.XImgProcCv2|method|public;static|JYPPX.OpenCvSharp.ImgProc.LineSegment HoughPointToLine(System.Int32 houghX,System.Int32 houghY,JYPPX.OpenCvSharp.Core.Mat srcImgInfo,JYPPX.OpenCvSharp.XImgProc.AngleRangeOption angleRange=Aro315To135,JYPPX.OpenCvSharp.XImgProc.HoughDeskewOption makeSkew=Deskew,JYPPX.OpenCvSharp.XImgProc.RulesOption rules=IgnoreBorders)') },
    [pscustomobject]@{ Ordinal = 120; Native = @('jyppx_ocv_ximgproc_superpixel_lsc_get_number'); Managed = @('MEMBER|JYPPX.OpenCvSharp.XImgProc.SuperpixelLSC|property|instance;get:public|System.Int32 NumberOfSuperpixels') },
    [pscustomobject]@{ Ordinal = 122; Native = @('jyppx_ocv_ximgproc_superpixel_lsc_get_labels'); Managed = @('MEMBER|JYPPX.OpenCvSharp.XImgProc.SuperpixelLSC|method|public;instance|System.Void GetLabels(JYPPX.OpenCvSharp.Core.Mat labels)') },
    [pscustomobject]@{ Ordinal = 123; Native = @('jyppx_ocv_ximgproc_superpixel_lsc_get_label_contour_mask'); Managed = @('MEMBER|JYPPX.OpenCvSharp.XImgProc.SuperpixelLSC|method|public;instance|System.Void GetLabelContourMask(JYPPX.OpenCvSharp.Core.Mat image,System.Boolean thickLine=true)') },
    [pscustomobject]@{ Ordinal = 124; Native = @('jyppx_ocv_ximgproc_superpixel_lsc_enforce_label_connectivity'); Managed = @('MEMBER|JYPPX.OpenCvSharp.XImgProc.SuperpixelLSC|method|public;instance|System.Void EnforceLabelConnectivity(System.Int32 minElementSize=25)') },
    [pscustomobject]@{ Ordinal = 132; Native = @('jyppx_ocv_ximgproc_scan_segment_get_number'); Managed = @('MEMBER|JYPPX.OpenCvSharp.XImgProc.ScanSegment|property|instance;get:public|System.Int32 NumberOfSuperpixels') },
    [pscustomobject]@{ Ordinal = 134; Native = @('jyppx_ocv_ximgproc_scan_segment_get_labels'); Managed = @('MEMBER|JYPPX.OpenCvSharp.XImgProc.ScanSegment|method|public;instance|System.Void GetLabels(JYPPX.OpenCvSharp.Core.Mat labels)') },
    [pscustomobject]@{ Ordinal = 135; Native = @('jyppx_ocv_ximgproc_scan_segment_get_label_contour_mask'); Managed = @('MEMBER|JYPPX.OpenCvSharp.XImgProc.ScanSegment|method|public;instance|System.Void GetLabelContourMask(JYPPX.OpenCvSharp.Core.Mat image,System.Boolean thickLine=true)') },
    [pscustomobject]@{ Ordinal = 138; Native = @('jyppx_ocv_ximgproc_superpixel_seeds_get_number'); Managed = @('MEMBER|JYPPX.OpenCvSharp.XImgProc.SuperpixelSEEDS|property|instance;get:public|System.Int32 NumberOfSuperpixels') },
    [pscustomobject]@{ Ordinal = 140; Native = @('jyppx_ocv_ximgproc_superpixel_seeds_get_labels'); Managed = @('MEMBER|JYPPX.OpenCvSharp.XImgProc.SuperpixelSEEDS|method|public;instance|System.Void GetLabels(JYPPX.OpenCvSharp.Core.Mat labels)') },
    [pscustomobject]@{ Ordinal = 141; Native = @('jyppx_ocv_ximgproc_superpixel_seeds_get_label_contour_mask'); Managed = @('MEMBER|JYPPX.OpenCvSharp.XImgProc.SuperpixelSEEDS|method|public;instance|System.Void GetLabelContourMask(JYPPX.OpenCvSharp.Core.Mat image,System.Boolean thickLine=false)') },
    [pscustomobject]@{ Ordinal = 187; Native = @('jyppx_ocv_ximgproc_superpixel_slic_get_number'); Managed = @('MEMBER|JYPPX.OpenCvSharp.XImgProc.SuperpixelSLIC|property|instance;get:public|System.Int32 NumberOfSuperpixels') },
    [pscustomobject]@{ Ordinal = 189; Native = @('jyppx_ocv_ximgproc_superpixel_slic_get_labels'); Managed = @('MEMBER|JYPPX.OpenCvSharp.XImgProc.SuperpixelSLIC|method|public;instance|System.Void GetLabels(JYPPX.OpenCvSharp.Core.Mat labels)') },
    [pscustomobject]@{ Ordinal = 190; Native = @('jyppx_ocv_ximgproc_superpixel_slic_get_label_contour_mask'); Managed = @('MEMBER|JYPPX.OpenCvSharp.XImgProc.SuperpixelSLIC|method|public;instance|System.Void GetLabelContourMask(JYPPX.OpenCvSharp.Core.Mat image,System.Boolean thickLine=true)') },
    [pscustomobject]@{ Ordinal = 191; Native = @('jyppx_ocv_ximgproc_superpixel_slic_enforce_label_connectivity'); Managed = @('MEMBER|JYPPX.OpenCvSharp.XImgProc.SuperpixelSLIC|method|public;instance|System.Void EnforceLabelConnectivity(System.Int32 minElementSize=25)') },
    [pscustomobject]@{ Ordinal = 196; Native = @('jyppx_ocv_ximgproc_edge_aware_interpolator_set_cost_map'); Managed = @('MEMBER|JYPPX.OpenCvSharp.XImgProc.EdgeAwareInterpolator|method|public;instance|System.Void SetCostMap(JYPPX.OpenCvSharp.Core.Mat costMap)') },
    [pscustomobject]@{ Ordinal = 216; Native = @('jyppx_ocv_ximgproc_ric_interpolator_set_superpixel_nn_count'); Managed = @('MEMBER|JYPPX.OpenCvSharp.XImgProc.RICInterpolator|property|instance;get:public;set:public|System.Int32 SuperpixelNNCount') },
    [pscustomobject]@{ Ordinal = 217; Native = @('jyppx_ocv_ximgproc_ric_interpolator_get_superpixel_nn_count'); Managed = @('MEMBER|JYPPX.OpenCvSharp.XImgProc.RICInterpolator|property|instance;get:public;set:public|System.Int32 SuperpixelNNCount') }
    ,[pscustomobject]@{ Ordinal = 130; Native = @('jyppx_ocv_ximgproc_ridge_detection_filter_get_image'); Managed = @('MEMBER|JYPPX.OpenCvSharp.XImgProc.RidgeDetectionFilter|method|public;instance|System.Void GetRidgeFilteredImage(JYPPX.OpenCvSharp.Core.Mat src,JYPPX.OpenCvSharp.Core.Mat dst)') }
)
foreach ($expected in $methodAccessorEvidence) {
    $row = @($classes.declarations | Where-Object { [int]$_.ordinal -eq $expected.Ordinal })
    $actualNative = @()
    if ($row.Count -eq 1) { $actualNative = @($row[0].nativeEntrypoints | Sort-Object -Unique) }
    $expectedNative = @($expected.Native | Sort-Object -Unique)
    $nativeMatches = @($actualNative).Count -eq @($expectedNative).Count -and @($actualNative | Where-Object { $expectedNative -cnotcontains [string]$_ }).Count -eq 0
    if ($row.Count -ne 1 -or [string]$row[0].classification -ne 'implemented' -or -not $nativeMatches -or @($row[0].managedMembers | Where-Object { $expected.Managed -ccontains [string]$_ }).Count -ne $expected.Managed.Count) {
        throw "XImgProc method/accessor evidence drifted at ordinal $($expected.Ordinal)."
    }
}
$segmentationBindings = @(
    [pscustomobject]@{ Ordinal = 144; Native = @('jyppx_ocv_ximgproc_graph_segmentation_process_image'); ManagedMarker = 'MEMBER|JYPPX.OpenCvSharp.XImgProc.GraphSegmentation|method|public;instance|System.Void ProcessImage(' },
    [pscustomobject]@{ Ordinal = 145; Native = @('jyppx_ocv_ximgproc_graph_segmentation_set_sigma'); ManagedMarker = 'MEMBER|JYPPX.OpenCvSharp.XImgProc.GraphSegmentation|property|instance;get:public;set:public|System.Double Sigma' },
    [pscustomobject]@{ Ordinal = 146; Native = @('jyppx_ocv_ximgproc_graph_segmentation_get_sigma'); ManagedMarker = 'MEMBER|JYPPX.OpenCvSharp.XImgProc.GraphSegmentation|property|instance;get:public;set:public|System.Double Sigma' },
    [pscustomobject]@{ Ordinal = 147; Native = @('jyppx_ocv_ximgproc_graph_segmentation_set_k'); ManagedMarker = 'MEMBER|JYPPX.OpenCvSharp.XImgProc.GraphSegmentation|property|instance;get:public;set:public|System.Single K' },
    [pscustomobject]@{ Ordinal = 148; Native = @('jyppx_ocv_ximgproc_graph_segmentation_get_k'); ManagedMarker = 'MEMBER|JYPPX.OpenCvSharp.XImgProc.GraphSegmentation|property|instance;get:public;set:public|System.Single K' },
    [pscustomobject]@{ Ordinal = 149; Native = @('jyppx_ocv_ximgproc_graph_segmentation_set_min_size'); ManagedMarker = 'MEMBER|JYPPX.OpenCvSharp.XImgProc.GraphSegmentation|property|instance;get:public;set:public|System.Int32 MinSize' },
    [pscustomobject]@{ Ordinal = 150; Native = @('jyppx_ocv_ximgproc_graph_segmentation_get_min_size'); ManagedMarker = 'MEMBER|JYPPX.OpenCvSharp.XImgProc.GraphSegmentation|property|instance;get:public;set:public|System.Int32 MinSize' },
    [pscustomobject]@{ Ordinal = 153; Native = @('jyppx_ocv_ximgproc_selective_search_strategy_set_image'); ManagedMarker = 'MEMBER|JYPPX.OpenCvSharp.XImgProc.SelectiveSearchSegmentationStrategy|method|public;instance|System.Void SetImage(' },
    [pscustomobject]@{ Ordinal = 154; Native = @('jyppx_ocv_ximgproc_selective_search_strategy_get'); ManagedMarker = 'MEMBER|JYPPX.OpenCvSharp.XImgProc.SelectiveSearchSegmentationStrategy|method|public;instance|System.Single Get(' },
    [pscustomobject]@{ Ordinal = 155; Native = @('jyppx_ocv_ximgproc_selective_search_strategy_merge'); ManagedMarker = 'MEMBER|JYPPX.OpenCvSharp.XImgProc.SelectiveSearchSegmentationStrategy|method|public;instance|System.Void Merge(' },
    [pscustomobject]@{ Ordinal = 165; Native = @('jyppx_ocv_ximgproc_selective_search_strategy_multiple_add'); ManagedMarker = 'MEMBER|JYPPX.OpenCvSharp.XImgProc.SelectiveSearchSegmentationStrategyMultiple|method|public;instance|System.Void AddStrategy(' },
    [pscustomobject]@{ Ordinal = 166; Native = @('jyppx_ocv_ximgproc_selective_search_strategy_multiple_clear'); ManagedMarker = 'MEMBER|JYPPX.OpenCvSharp.XImgProc.SelectiveSearchSegmentationStrategyMultiple|method|public;instance|System.Void ClearStrategies()' },
    [pscustomobject]@{ Ordinal = 173; Native = @('jyppx_ocv_ximgproc_selective_search_segmentation_set_base_image'); ManagedMarker = 'MEMBER|JYPPX.OpenCvSharp.XImgProc.SelectiveSearchSegmentation|method|public;instance|System.Void SetBaseImage(' },
    [pscustomobject]@{ Ordinal = 174; Native = @('jyppx_ocv_ximgproc_selective_search_segmentation_switch_to_single_strategy'); ManagedMarker = 'MEMBER|JYPPX.OpenCvSharp.XImgProc.SelectiveSearchSegmentation|method|public;instance|System.Void SwitchToSingleStrategy(' },
    [pscustomobject]@{ Ordinal = 175; Native = @('jyppx_ocv_ximgproc_selective_search_segmentation_switch_to_fast'); ManagedMarker = 'MEMBER|JYPPX.OpenCvSharp.XImgProc.SelectiveSearchSegmentation|method|public;instance|System.Void SwitchToSelectiveSearchFast(' },
    [pscustomobject]@{ Ordinal = 176; Native = @('jyppx_ocv_ximgproc_selective_search_segmentation_switch_to_quality'); ManagedMarker = 'MEMBER|JYPPX.OpenCvSharp.XImgProc.SelectiveSearchSegmentation|method|public;instance|System.Void SwitchToSelectiveSearchQuality(' },
    [pscustomobject]@{ Ordinal = 177; Native = @('jyppx_ocv_ximgproc_selective_search_segmentation_add_image'); ManagedMarker = 'MEMBER|JYPPX.OpenCvSharp.XImgProc.SelectiveSearchSegmentation|method|public;instance|System.Void AddImage(' },
    [pscustomobject]@{ Ordinal = 178; Native = @('jyppx_ocv_ximgproc_selective_search_segmentation_clear_images'); ManagedMarker = 'MEMBER|JYPPX.OpenCvSharp.XImgProc.SelectiveSearchSegmentation|method|public;instance|System.Void ClearImages()' },
    [pscustomobject]@{ Ordinal = 179; Native = @('jyppx_ocv_ximgproc_selective_search_segmentation_add_graph_segmentation'); ManagedMarker = 'MEMBER|JYPPX.OpenCvSharp.XImgProc.SelectiveSearchSegmentation|method|public;instance|System.Void AddGraphSegmentation(' },
    [pscustomobject]@{ Ordinal = 180; Native = @('jyppx_ocv_ximgproc_selective_search_segmentation_clear_graph_segmentations'); ManagedMarker = 'MEMBER|JYPPX.OpenCvSharp.XImgProc.SelectiveSearchSegmentation|method|public;instance|System.Void ClearGraphSegmentations()' },
    [pscustomobject]@{ Ordinal = 181; Native = @('jyppx_ocv_ximgproc_selective_search_segmentation_add_strategy'); ManagedMarker = 'MEMBER|JYPPX.OpenCvSharp.XImgProc.SelectiveSearchSegmentation|method|public;instance|System.Void AddStrategy(' },
    [pscustomobject]@{ Ordinal = 182; Native = @('jyppx_ocv_ximgproc_selective_search_segmentation_clear_strategies'); ManagedMarker = 'MEMBER|JYPPX.OpenCvSharp.XImgProc.SelectiveSearchSegmentation|method|public;instance|System.Void ClearStrategies()' },
    [pscustomobject]@{ Ordinal = 183; Native = @('jyppx_ocv_ximgproc_selective_search_segmentation_process_count','jyppx_ocv_ximgproc_selective_search_segmentation_process_fill'); ManagedMarker = 'MEMBER|JYPPX.OpenCvSharp.XImgProc.SelectiveSearchSegmentation|method|public;instance|JYPPX.OpenCvSharp.Core.Rect[] Process()' }
)
foreach ($expected in $segmentationBindings) {
    $row = @($classes.declarations | Where-Object { [int]$_.ordinal -eq $expected.Ordinal })
    if ($row.Count -ne 1) { throw "XImgProc segmentation binding row is missing or duplicated: $($expected.Ordinal)." }
    $actualNative = @($row[0].nativeEntrypoints | Sort-Object -Unique)
    $expectedNative = @($expected.Native | Sort-Object -Unique)
    $nativeMatches = $actualNative.Count -eq $expectedNative.Count -and @($actualNative | Where-Object { $expectedNative -cnotcontains [string]$_ }).Count -eq 0
    $managedMatches = @($row[0].managedMembers | Where-Object { ([string]$_).Contains([string]$expected.ManagedMarker, [StringComparison]::Ordinal) }).Count -eq 1
    if ($row[0].classification -ne 'implemented' -or -not $nativeMatches -or -not $managedMatches) {
        throw "XImgProc segmentation native/managed evidence drifted at ordinal $($expected.Ordinal)."
    }
}
$selectiveSearchMultipleOverloads = @($classes.declarations | Where-Object { [int]$_.ordinal -in @(168,169,170,171) })
if ($selectiveSearchMultipleOverloads.Count -ne 4 -or @($selectiveSearchMultipleOverloads | Where-Object { $_.classification -ne 'intentionally-omitted' -or @($_.nativeEntrypoints).Count -ne 1 -or $_.nativeEntrypoints[0] -cne 'jyppx_ocv_ximgproc_selective_search_strategy_create_multiple' -or @($_.managedMembers).Count -ne 0 }).Count -ne 0) { throw 'XImgProc unexposed Selective Search factory overloads drifted.' }
$structuredEdgeOrdinals = @(240,241,243,244,245,246)
$structuredEdgeRows = @($classes.declarations | Where-Object { [int]$_.ordinal -in $structuredEdgeOrdinals })
if ($structuredEdgeRows.Count -ne $structuredEdgeOrdinals.Count -or
    @($structuredEdgeRows | Where-Object { $_.classification -ne 'intentionally-omitted' -or @($_.nativeEntrypoints).Count -ne 0 -or @($_.managedMembers).Count -ne 0 -or [string]$_.reason -notmatch 'model|RFFeatureGetter' }).Count -ne 0 -or
    (Get-Content (Join-Path $repo 'docs/articles/ximgproc-edge-guide.md') -Raw) -notmatch 'StructuredEdgeDetection.*model') {
    throw 'XImgProc model-backed StructuredEdge omission review drifted.'
}
$statefulFilterOrdinals = @(45,46,53,54,55,56)
$statefulFilterRows = @($classes.declarations | Where-Object { [int]$_.ordinal -in $statefulFilterOrdinals })
if ($statefulFilterRows.Count -ne $statefulFilterOrdinals.Count -or
    @($statefulFilterRows | Where-Object { $_.classification -ne 'intentionally-omitted' -or @($_.nativeEntrypoints).Count -ne 0 -or @($_.managedMembers).Count -ne 0 -or [string]$_.reason -notmatch 'stateful|one-shot|ownership|lifetime' }).Count -ne 0 -or
    (Get-Content (Join-Path $repo 'docs/articles/ximgproc-filter-utilities-guide.md') -Raw) -notmatch 'stateful.*one-shot') {
    throw 'XImgProc stateful filter omission review drifted.'
}
$edgeDrawingParamsOrdinals = @(31,32,33)
$edgeDrawingParamsRows = @($classes.declarations | Where-Object { [int]$_.ordinal -in $edgeDrawingParamsOrdinals })
if ($edgeDrawingParamsRows.Count -ne $edgeDrawingParamsOrdinals.Count -or
    @($edgeDrawingParamsRows | Where-Object { $_.classification -ne 'intentionally-omitted' -or @($_.nativeEntrypoints).Count -ne 0 -or @($_.managedMembers).Count -ne 0 -or [string]$_.reason -notmatch 'EdgeDrawingParams|FileNode|FileStorage|ABI|round-trip' }).Count -ne 0 -or
    (Get-Content (Join-Path $repo 'docs/articles/ximgproc-edge-guide.md') -Raw) -notmatch 'FileNode.*FileStorage|FileStorage.*FileNode') {
    throw 'XImgProc EdgeDrawing.Params omission review drifted.'
}
$native = @(Get-Content (Join-Path $repo 'src/OpenCvSharp.Native/generated/native_abi_manifest.txt') | Where-Object { $_ -match '^jyppx_ocv_ximgproc_' } | ForEach-Object { ($_ -split '\|')[0] })
$managed = @(Get-Content (Join-Path $repo 'compatibility/managed-public-api.txt') | Where-Object { $_ -match 'JYPPX\.OpenCvSharp\.XImgProc' })
foreach ($row in @($classes.declarations)) {
    if ([string]::IsNullOrWhiteSpace([string]$row.identity) -or [string]::IsNullOrWhiteSpace([string]$row.reason)) { throw "XImgProc classification row is incomplete: $($row.ordinal)" }
    foreach ($entry in @($row.nativeEntrypoints)) { if ($native -notcontains [string]$entry) { throw "XImgProc native evidence is not in manifest: $entry" } }
    foreach ($entry in @($row.managedMembers)) { if ($managed -notcontains [string]$entry) { throw 'XImgProc managed evidence is not in managed baseline.' } }
    if ([string]$row.classification -eq 'implemented' -and (@($row.nativeEntrypoints).Count -eq 0 -or @($row.managedMembers).Count -eq 0)) { throw "Implemented XImgProc row lacks evidence: $($row.ordinal)" }
}
$evidenceBackedOmissions = @($classes.declarations | Where-Object { $_.classification -eq 'intentionally-omitted' -and @($_.nativeEntrypoints).Count -gt 0 -and @($_.managedMembers).Count -gt 0 })
if ($evidenceBackedOmissions.Count -ne 0) { throw "XImgProc intentional omission has both native and managed evidence: $($evidenceBackedOmissions[0].ordinal)" }
Write-Host "XIMGPROC_UPSTREAM_MAP_CONTRACT_OK declarations=249 callables=$($summary.callableCount) implemented=$($summary.classificationCounts.implemented) omitted=$($summary.classificationCounts.'intentionally-omitted') missing=0 fixtures=$($summary.negativeFixtureCount) sha256=$($summary.mappingSha256)"
