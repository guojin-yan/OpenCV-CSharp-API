#if NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal static partial class NativeMethods
    {
        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_ni_black_threshold")]
        internal static partial int XImgProcNiBlackThreshold(IntPtr src, IntPtr dst, double maxValue, int type, int blockSize, double k, int binarizationMethod, double r);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_thinning")]
        internal static partial int XImgProcThinning(IntPtr src, IntPtr dst, int thinningType);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_anisotropic_diffusion")]
        internal static partial int XImgProcAnisotropicDiffusion(IntPtr src, IntPtr dst, float alpha, float k, int niters);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_joint_bilateral_filter")]
        internal static partial int XImgProcJointBilateralFilter(IntPtr joint, IntPtr src, IntPtr dst, int d, double sigmaColor, double sigmaSpace, int borderType);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_guided_filter_run")]
        internal static partial int XImgProcGuidedFilter(IntPtr guide, IntPtr src, IntPtr dst, int radius, double eps, int dDepth, double scale);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_rolling_guidance_filter")]
        internal static partial int XImgProcRollingGuidanceFilter(IntPtr src, IntPtr dst, int d, double sigmaColor, double sigmaSpace, int numOfIter, int borderType);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_weighted_median_filter")]
        internal static partial int XImgProcWeightedMedianFilter(IntPtr joint, IntPtr src, IntPtr dst, int r, double sigma, int weightType, IntPtr mask);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_dt_filter")]
        internal static partial int XImgProcDtFilter(IntPtr guide, IntPtr src, IntPtr dst, double sigmaSpatial, double sigmaColor, int mode, int numIters);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_am_filter")]
        internal static partial int XImgProcAmFilter(IntPtr joint, IntPtr src, IntPtr dst, double sigmaS, double sigmaR, int adjustOutliers);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_bilateral_texture_filter")]
        internal static partial int XImgProcBilateralTextureFilter(IntPtr src, IntPtr dst, int fr, int numIter, double sigmaAlpha, double sigmaAvg);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_preserving_filter")]
        internal static partial int XImgProcEdgePreservingFilter(IntPtr src, IntPtr dst, int d, double threshold);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_fast_global_smoother_filter_run")]
        internal static partial int XImgProcFastGlobalSmootherFilterRun(IntPtr guide, IntPtr src, IntPtr dst, double lambda, double sigmaColor, double lambdaAttenuation, int numIter);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_l0_smooth")]
        internal static partial int XImgProcL0Smooth(IntPtr src, IntPtr dst, double lambda, double kappa);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_fast_hough_transform")]
        internal static partial int XImgProcFastHoughTransform(IntPtr src, IntPtr dst, int dstMatDepth, int angleRange, int op, int makeSkew);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_hough_point_to_line")]
        internal static partial int XImgProcHoughPointToLine(int houghX, int houghY, IntPtr srcImgInfo, int angleRange, int makeSkew, int rules, out int x1, out int y1, out int x2, out int y2);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_pei_lin_normalization")]
        internal static partial int XImgProcPeiLinNormalization(IntPtr src, IntPtr dst);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_guided_filter_create")]
        internal static partial int XImgProcGuidedFilterCreate(IntPtr guide, int radius, double eps, double scale, out IntPtr filter);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_guided_filter_release_handle")]
        internal static partial void XImgProcGuidedFilterReleaseHandle(IntPtr filter);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_guided_filter_filter")]
        internal static partial int XImgProcGuidedFilterFilter(IntPtr filter, IntPtr src, IntPtr dst, int dDepth);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_fast_global_smoother_filter_create")]
        internal static partial int XImgProcFastGlobalSmootherFilterCreate(IntPtr guide, double lambda, double sigmaColor, double lambdaAttenuation, int numIter, out IntPtr filter);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_fast_global_smoother_filter_release_handle")]
        internal static partial void XImgProcFastGlobalSmootherFilterReleaseHandle(IntPtr filter);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_fast_global_smoother_filter_filter")]
        internal static partial int XImgProcFastGlobalSmootherFilterFilter(IntPtr filter, IntPtr src, IntPtr dst);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_superpixel_slic_create")]
        internal static partial int XImgProcSuperpixelSLICCreate(IntPtr image, int algorithm, int regionSize, float ruler, out IntPtr superpixel);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_superpixel_slic_release_handle")]
        internal static partial void XImgProcSuperpixelSLICReleaseHandle(IntPtr superpixel);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_superpixel_slic_get_number")]
        internal static partial int XImgProcSuperpixelSLICGetNumber(IntPtr superpixel, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_superpixel_slic_iterate")]
        internal static partial int XImgProcSuperpixelSLICIterate(IntPtr superpixel, int numIterations);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_superpixel_slic_get_labels")]
        internal static partial int XImgProcSuperpixelSLICGetLabels(IntPtr superpixel, IntPtr labels);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_superpixel_slic_get_label_contour_mask")]
        internal static partial int XImgProcSuperpixelSLICGetLabelContourMask(IntPtr superpixel, IntPtr image, int thickLine);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_superpixel_slic_enforce_label_connectivity")]
        internal static partial int XImgProcSuperpixelSLICEnforceLabelConnectivity(IntPtr superpixel, int minElementSize);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_superpixel_seeds_create")]
        internal static partial int XImgProcSuperpixelSEEDSCreate(int imageWidth, int imageHeight, int imageChannels, int numSuperpixels, int numLevels, int prior, int histogramBins, int doubleStep, out IntPtr superpixel);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_superpixel_seeds_release_handle")]
        internal static partial void XImgProcSuperpixelSEEDSReleaseHandle(IntPtr superpixel);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_superpixel_seeds_get_number")]
        internal static partial int XImgProcSuperpixelSEEDSGetNumber(IntPtr superpixel, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_superpixel_seeds_iterate")]
        internal static partial int XImgProcSuperpixelSEEDSIterate(IntPtr superpixel, IntPtr image, int numIterations);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_superpixel_seeds_get_labels")]
        internal static partial int XImgProcSuperpixelSEEDSGetLabels(IntPtr superpixel, IntPtr labels);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_superpixel_seeds_get_label_contour_mask")]
        internal static partial int XImgProcSuperpixelSEEDSGetLabelContourMask(IntPtr superpixel, IntPtr image, int thickLine);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_superpixel_lsc_create")]
        internal static partial int XImgProcSuperpixelLSCCreate(IntPtr image, int regionSize, float ratio, out IntPtr superpixel);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_superpixel_lsc_release_handle")]
        internal static partial void XImgProcSuperpixelLSCReleaseHandle(IntPtr superpixel);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_superpixel_lsc_get_number")]
        internal static partial int XImgProcSuperpixelLSCGetNumber(IntPtr superpixel, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_superpixel_lsc_iterate")]
        internal static partial int XImgProcSuperpixelLSCIterate(IntPtr superpixel, int numIterations);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_superpixel_lsc_get_labels")]
        internal static partial int XImgProcSuperpixelLSCGetLabels(IntPtr superpixel, IntPtr labels);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_superpixel_lsc_get_label_contour_mask")]
        internal static partial int XImgProcSuperpixelLSCGetLabelContourMask(IntPtr superpixel, IntPtr image, int thickLine);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_superpixel_lsc_enforce_label_connectivity")]
        internal static partial int XImgProcSuperpixelLSCEnforceLabelConnectivity(IntPtr superpixel, int minElementSize);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_fast_line_detector_create")]
        internal static partial int XImgProcFastLineDetectorCreate(int lengthThreshold, float distanceThreshold, double cannyTh1, double cannyTh2, int cannyApertureSize, int doMerge, out IntPtr detector);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_fast_line_detector_release_handle")]
        internal static partial void XImgProcFastLineDetectorReleaseHandle(IntPtr detector);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_fast_line_detector_detect")]
        internal static partial int XImgProcFastLineDetectorDetect(IntPtr detector, IntPtr image, IntPtr lines);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_fast_line_detector_detect_count")]
        internal static partial int XImgProcFastLineDetectorDetectCount(IntPtr detector, IntPtr image, out int lineCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_fast_line_detector_detect_fill")]
        internal static partial int XImgProcFastLineDetectorDetectFill(IntPtr detector, IntPtr image, float[] lines, int lineCapacity, out int lineCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_fast_line_detector_draw_segments")]
        internal static partial int XImgProcFastLineDetectorDrawSegments(IntPtr detector, IntPtr image, IntPtr lines, int drawArrow, double colorV0, double colorV1, double colorV2, double colorV3, int lineThickness);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_fast_line_detector_draw_segments_array")]
        internal static partial int XImgProcFastLineDetectorDrawSegmentsArray(IntPtr detector, IntPtr image, float[] lines, int lineCount, int drawArrow, double colorV0, double colorV1, double colorV2, double colorV3, int lineThickness);
        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_disparity_wls_filter_create_generic")]
        internal static partial int XImgProcDisparityWLSFilterCreateGeneric(int useConfidence, out IntPtr filter);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_disparity_wls_filter_create_from_stereo_bm")]
        internal static partial int XImgProcDisparityWLSFilterCreateFromStereoBM(IntPtr matcherLeft, out IntPtr filter);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_disparity_wls_filter_create_from_stereo_sgbm")]
        internal static partial int XImgProcDisparityWLSFilterCreateFromStereoSGBM(IntPtr matcherLeft, out IntPtr filter);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_disparity_wls_filter_create_from_stereo_matcher")]
        internal static partial int XImgProcDisparityWLSFilterCreateFromStereoMatcher(IntPtr matcherLeft, out IntPtr filter);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_create_right_matcher_from_stereo_bm")]
        internal static partial int XImgProcCreateRightMatcherFromStereoBM(IntPtr matcherLeft, out IntPtr matcherRight);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_create_right_matcher_from_stereo_sgbm")]
        internal static partial int XImgProcCreateRightMatcherFromStereoSGBM(IntPtr matcherLeft, out IntPtr matcherRight);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_create_right_matcher_from_stereo_matcher")]
        internal static partial int XImgProcCreateRightMatcherFromStereoMatcher(IntPtr matcherLeft, out IntPtr matcherRight);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_disparity_wls_filter_release_handle")]
        internal static partial void XImgProcDisparityWLSFilterReleaseHandle(IntPtr filter);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_disparity_filter_filter")]
        internal static partial int XImgProcDisparityFilterFilter(IntPtr filter, IntPtr disparityMapLeft, IntPtr leftView, IntPtr filteredDisparityMap, IntPtr disparityMapRight, ref XImgProcRectNative roi, IntPtr rightView);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_disparity_wls_filter_filter")]
        internal static partial int XImgProcDisparityWLSFilterFilter(IntPtr filter, IntPtr disparityMapLeft, IntPtr leftView, IntPtr filteredDisparityMap, IntPtr disparityMapRight, ref XImgProcRectNative roi, IntPtr rightView);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_disparity_wls_filter_get_lambda")]
        internal static partial int XImgProcDisparityWLSFilterGetLambda(IntPtr filter, out double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_disparity_wls_filter_set_lambda")]
        internal static partial int XImgProcDisparityWLSFilterSetLambda(IntPtr filter, double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_disparity_wls_filter_get_sigma_color")]
        internal static partial int XImgProcDisparityWLSFilterGetSigmaColor(IntPtr filter, out double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_disparity_wls_filter_set_sigma_color")]
        internal static partial int XImgProcDisparityWLSFilterSetSigmaColor(IntPtr filter, double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_disparity_wls_filter_get_lrc_thresh")]
        internal static partial int XImgProcDisparityWLSFilterGetLrcThresh(IntPtr filter, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_disparity_wls_filter_set_lrc_thresh")]
        internal static partial int XImgProcDisparityWLSFilterSetLrcThresh(IntPtr filter, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_disparity_wls_filter_get_depth_discontinuity_radius")]
        internal static partial int XImgProcDisparityWLSFilterGetDepthDiscontinuityRadius(IntPtr filter, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_disparity_wls_filter_set_depth_discontinuity_radius")]
        internal static partial int XImgProcDisparityWLSFilterSetDepthDiscontinuityRadius(IntPtr filter, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_disparity_wls_filter_get_confidence_map")]
        internal static partial int XImgProcDisparityWLSFilterGetConfidenceMap(IntPtr filter, IntPtr confidenceMap);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_disparity_wls_filter_get_roi")]
        internal static partial int XImgProcDisparityWLSFilterGetRoi(IntPtr filter, out XImgProcRectNative roi);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_get_disparity_vis")]
        internal static partial int XImgProcGetDisparityVis(IntPtr src, IntPtr dst, double scale);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_compute_mse")]
        internal static partial int XImgProcComputeMSE(IntPtr gt, IntPtr src, ref XImgProcRectNative roi, out double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_compute_bad_pixel_percent")]
        internal static partial int XImgProcComputeBadPixelPercent(IntPtr gt, IntPtr src, ref XImgProcRectNative roi, int thresh, out double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_fast_bilateral_solver_filter_create")]
        internal static partial int XImgProcFastBilateralSolverFilterCreate(IntPtr guide, double sigmaSpatial, double sigmaLuma, double sigmaChroma, double lambda, int numIter, double maxTol, out IntPtr filter);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_fast_bilateral_solver_filter_release_handle")]
        internal static partial void XImgProcFastBilateralSolverFilterReleaseHandle(IntPtr filter);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_fast_bilateral_solver_filter_filter")]
        internal static partial int XImgProcFastBilateralSolverFilterFilter(IntPtr filter, IntPtr src, IntPtr confidence, IntPtr dst);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_fast_bilateral_solver_filter_run")]
        internal static partial int XImgProcFastBilateralSolverFilterRun(IntPtr guide, IntPtr src, IntPtr confidence, IntPtr dst, double sigmaSpatial, double sigmaLuma, double sigmaChroma, double lambda, int numIter, double maxTol);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_aware_interpolator_create")]
        internal static partial int XImgProcEdgeAwareInterpolatorCreate(out IntPtr interpolator);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_aware_interpolator_release_handle")]
        internal static partial void XImgProcEdgeAwareInterpolatorReleaseHandle(IntPtr interpolator);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_ric_interpolator_create")]
        internal static partial int XImgProcRICInterpolatorCreate(out IntPtr interpolator);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_ric_interpolator_release_handle")]
        internal static partial void XImgProcRICInterpolatorReleaseHandle(IntPtr interpolator);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_sparse_match_interpolator_interpolate")]
        internal static partial int XImgProcSparseMatchInterpolatorInterpolate(IntPtr interpolator, IntPtr fromImage, IntPtr fromPoints, IntPtr toImage, IntPtr toPoints, IntPtr denseFlow);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_aware_interpolator_interpolate")]
        internal static partial int XImgProcEdgeAwareInterpolatorInterpolate(IntPtr interpolator, IntPtr fromImage, IntPtr fromPoints, IntPtr toImage, IntPtr toPoints, IntPtr denseFlow);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_ric_interpolator_interpolate")]
        internal static partial int XImgProcRICInterpolatorInterpolate(IntPtr interpolator, IntPtr fromImage, IntPtr fromPoints, IntPtr toImage, IntPtr toPoints, IntPtr denseFlow);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_aware_interpolator_set_cost_map")]
        internal static partial int XImgProcEdgeAwareInterpolatorSetCostMap(IntPtr interpolator, IntPtr costMap);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_ric_interpolator_set_cost_map")]
        internal static partial int XImgProcRICInterpolatorSetCostMap(IntPtr interpolator, IntPtr costMap);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_aware_interpolator_get_k")]
        internal static partial int XImgProcEdgeAwareInterpolatorGetK(IntPtr interpolator, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_aware_interpolator_set_k")]
        internal static partial int XImgProcEdgeAwareInterpolatorSetK(IntPtr interpolator, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_aware_interpolator_get_sigma")]
        internal static partial int XImgProcEdgeAwareInterpolatorGetSigma(IntPtr interpolator, out float value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_aware_interpolator_set_sigma")]
        internal static partial int XImgProcEdgeAwareInterpolatorSetSigma(IntPtr interpolator, float value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_aware_interpolator_get_lambda")]
        internal static partial int XImgProcEdgeAwareInterpolatorGetLambda(IntPtr interpolator, out float value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_aware_interpolator_set_lambda")]
        internal static partial int XImgProcEdgeAwareInterpolatorSetLambda(IntPtr interpolator, float value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_aware_interpolator_get_use_post_processing")]
        internal static partial int XImgProcEdgeAwareInterpolatorGetUsePostProcessing(IntPtr interpolator, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_aware_interpolator_set_use_post_processing")]
        internal static partial int XImgProcEdgeAwareInterpolatorSetUsePostProcessing(IntPtr interpolator, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_aware_interpolator_get_fgs_lambda")]
        internal static partial int XImgProcEdgeAwareInterpolatorGetFgsLambda(IntPtr interpolator, out float value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_aware_interpolator_set_fgs_lambda")]
        internal static partial int XImgProcEdgeAwareInterpolatorSetFgsLambda(IntPtr interpolator, float value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_aware_interpolator_get_fgs_sigma")]
        internal static partial int XImgProcEdgeAwareInterpolatorGetFgsSigma(IntPtr interpolator, out float value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_aware_interpolator_set_fgs_sigma")]
        internal static partial int XImgProcEdgeAwareInterpolatorSetFgsSigma(IntPtr interpolator, float value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_ric_interpolator_get_k")]
        internal static partial int XImgProcRICInterpolatorGetK(IntPtr interpolator, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_ric_interpolator_set_k")]
        internal static partial int XImgProcRICInterpolatorSetK(IntPtr interpolator, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_ric_interpolator_get_superpixel_size")]
        internal static partial int XImgProcRICInterpolatorGetSuperpixelSize(IntPtr interpolator, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_ric_interpolator_set_superpixel_size")]
        internal static partial int XImgProcRICInterpolatorSetSuperpixelSize(IntPtr interpolator, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_ric_interpolator_get_superpixel_nn_count")]
        internal static partial int XImgProcRICInterpolatorGetSuperpixelNNCount(IntPtr interpolator, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_ric_interpolator_set_superpixel_nn_count")]
        internal static partial int XImgProcRICInterpolatorSetSuperpixelNNCount(IntPtr interpolator, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_ric_interpolator_get_superpixel_ruler")]
        internal static partial int XImgProcRICInterpolatorGetSuperpixelRuler(IntPtr interpolator, out float value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_ric_interpolator_set_superpixel_ruler")]
        internal static partial int XImgProcRICInterpolatorSetSuperpixelRuler(IntPtr interpolator, float value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_ric_interpolator_get_superpixel_mode")]
        internal static partial int XImgProcRICInterpolatorGetSuperpixelMode(IntPtr interpolator, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_ric_interpolator_set_superpixel_mode")]
        internal static partial int XImgProcRICInterpolatorSetSuperpixelMode(IntPtr interpolator, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_ric_interpolator_get_alpha")]
        internal static partial int XImgProcRICInterpolatorGetAlpha(IntPtr interpolator, out float value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_ric_interpolator_set_alpha")]
        internal static partial int XImgProcRICInterpolatorSetAlpha(IntPtr interpolator, float value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_ric_interpolator_get_model_iter")]
        internal static partial int XImgProcRICInterpolatorGetModelIter(IntPtr interpolator, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_ric_interpolator_set_model_iter")]
        internal static partial int XImgProcRICInterpolatorSetModelIter(IntPtr interpolator, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_ric_interpolator_get_refine_models")]
        internal static partial int XImgProcRICInterpolatorGetRefineModels(IntPtr interpolator, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_ric_interpolator_set_refine_models")]
        internal static partial int XImgProcRICInterpolatorSetRefineModels(IntPtr interpolator, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_ric_interpolator_get_max_flow")]
        internal static partial int XImgProcRICInterpolatorGetMaxFlow(IntPtr interpolator, out float value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_ric_interpolator_set_max_flow")]
        internal static partial int XImgProcRICInterpolatorSetMaxFlow(IntPtr interpolator, float value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_ric_interpolator_get_use_variational_refinement")]
        internal static partial int XImgProcRICInterpolatorGetUseVariationalRefinement(IntPtr interpolator, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_ric_interpolator_set_use_variational_refinement")]
        internal static partial int XImgProcRICInterpolatorSetUseVariationalRefinement(IntPtr interpolator, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_ric_interpolator_get_use_global_smoother_filter")]
        internal static partial int XImgProcRICInterpolatorGetUseGlobalSmootherFilter(IntPtr interpolator, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_ric_interpolator_set_use_global_smoother_filter")]
        internal static partial int XImgProcRICInterpolatorSetUseGlobalSmootherFilter(IntPtr interpolator, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_ric_interpolator_get_fgs_lambda")]
        internal static partial int XImgProcRICInterpolatorGetFgsLambda(IntPtr interpolator, out float value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_ric_interpolator_set_fgs_lambda")]
        internal static partial int XImgProcRICInterpolatorSetFgsLambda(IntPtr interpolator, float value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_ric_interpolator_get_fgs_sigma")]
        internal static partial int XImgProcRICInterpolatorGetFgsSigma(IntPtr interpolator, out float value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_ric_interpolator_set_fgs_sigma")]
        internal static partial int XImgProcRICInterpolatorSetFgsSigma(IntPtr interpolator, float value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_drawing_create")]
        internal static partial int XImgProcEdgeDrawingCreate(out IntPtr detector);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_drawing_release_handle")]
        internal static partial void XImgProcEdgeDrawingReleaseHandle(IntPtr detector);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_drawing_get_params")]
        internal static partial int XImgProcEdgeDrawingGetParams(IntPtr detector, out XImgProcEdgeDrawingParamsNative parameters);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_drawing_set_params")]
        internal static partial int XImgProcEdgeDrawingSetParams(IntPtr detector, ref XImgProcEdgeDrawingParamsNative parameters);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_drawing_detect_edges")]
        internal static partial int XImgProcEdgeDrawingDetectEdges(IntPtr detector, IntPtr src);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_drawing_get_edge_image")]
        internal static partial int XImgProcEdgeDrawingGetEdgeImage(IntPtr detector, IntPtr dst);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_drawing_get_gradient_image")]
        internal static partial int XImgProcEdgeDrawingGetGradientImage(IntPtr detector, IntPtr dst);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_drawing_get_segments_count")]
        internal static partial int XImgProcEdgeDrawingGetSegmentsCount(IntPtr detector, out int groupCount, out int pointCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_drawing_get_segments_fill")]
        internal static partial int XImgProcEdgeDrawingGetSegmentsFill(IntPtr detector, int[] offsets, int offsetCapacity, XImgProcPointNative[] points, int pointCapacity, out int groupCount, out int pointCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_drawing_detect_lines")]
        internal static partial int XImgProcEdgeDrawingDetectLines(IntPtr detector, IntPtr lines);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_drawing_detect_lines_count")]
        internal static partial int XImgProcEdgeDrawingDetectLinesCount(IntPtr detector, out int lineCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_drawing_detect_lines_fill")]
        internal static partial int XImgProcEdgeDrawingDetectLinesFill(IntPtr detector, float[] lines, int lineCapacity, out int lineCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_drawing_get_segment_indices_of_lines_count")]
        internal static partial int XImgProcEdgeDrawingGetSegmentIndicesOfLinesCount(IntPtr detector, out int indexCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_drawing_get_segment_indices_of_lines_fill")]
        internal static partial int XImgProcEdgeDrawingGetSegmentIndicesOfLinesFill(IntPtr detector, int[] indices, int indexCapacity, out int indexCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_drawing_detect_ellipses")]
        internal static partial int XImgProcEdgeDrawingDetectEllipses(IntPtr detector, IntPtr ellipses);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_drawing_detect_ellipses_count")]
        internal static partial int XImgProcEdgeDrawingDetectEllipsesCount(IntPtr detector, out int ellipseCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_drawing_detect_ellipses_fill")]
        internal static partial int XImgProcEdgeDrawingDetectEllipsesFill(IntPtr detector, double[] ellipses, int ellipseCapacity, out int ellipseCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_boxes_create")]
        internal static partial int XImgProcEdgeBoxesCreate(float alpha, float beta, float eta, float minScore, int maxBoxes, float edgeMinMag, float edgeMergeThr, float clusterMinMag, float maxAspectRatio, float minBoxArea, float gamma, float kappa, out IntPtr edgeBoxes);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_boxes_release_handle")]
        internal static partial void XImgProcEdgeBoxesReleaseHandle(IntPtr edgeBoxes);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_boxes_get_bounding_boxes_count")]
        internal static partial int XImgProcEdgeBoxesGetBoundingBoxesCount(IntPtr edgeBoxes, IntPtr edgeMap, IntPtr orientationMap, out int boxCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_boxes_get_bounding_boxes_fill")]
        internal static partial int XImgProcEdgeBoxesGetBoundingBoxesFill(IntPtr edgeBoxes, IntPtr edgeMap, IntPtr orientationMap, XImgProcEdgeBoxNative[] boxes, int boxCapacity, out int boxCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_boxes_get_alpha")]
        internal static partial int XImgProcEdgeBoxesGetAlpha(IntPtr edgeBoxes, out float value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_boxes_set_alpha")]
        internal static partial int XImgProcEdgeBoxesSetAlpha(IntPtr edgeBoxes, float value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_boxes_get_beta")]
        internal static partial int XImgProcEdgeBoxesGetBeta(IntPtr edgeBoxes, out float value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_boxes_set_beta")]
        internal static partial int XImgProcEdgeBoxesSetBeta(IntPtr edgeBoxes, float value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_boxes_get_eta")]
        internal static partial int XImgProcEdgeBoxesGetEta(IntPtr edgeBoxes, out float value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_boxes_set_eta")]
        internal static partial int XImgProcEdgeBoxesSetEta(IntPtr edgeBoxes, float value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_boxes_get_min_score")]
        internal static partial int XImgProcEdgeBoxesGetMinScore(IntPtr edgeBoxes, out float value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_boxes_set_min_score")]
        internal static partial int XImgProcEdgeBoxesSetMinScore(IntPtr edgeBoxes, float value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_boxes_get_max_boxes")]
        internal static partial int XImgProcEdgeBoxesGetMaxBoxes(IntPtr edgeBoxes, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_boxes_set_max_boxes")]
        internal static partial int XImgProcEdgeBoxesSetMaxBoxes(IntPtr edgeBoxes, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_boxes_get_edge_min_mag")]
        internal static partial int XImgProcEdgeBoxesGetEdgeMinMag(IntPtr edgeBoxes, out float value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_boxes_set_edge_min_mag")]
        internal static partial int XImgProcEdgeBoxesSetEdgeMinMag(IntPtr edgeBoxes, float value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_boxes_get_edge_merge_thr")]
        internal static partial int XImgProcEdgeBoxesGetEdgeMergeThr(IntPtr edgeBoxes, out float value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_boxes_set_edge_merge_thr")]
        internal static partial int XImgProcEdgeBoxesSetEdgeMergeThr(IntPtr edgeBoxes, float value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_boxes_get_cluster_min_mag")]
        internal static partial int XImgProcEdgeBoxesGetClusterMinMag(IntPtr edgeBoxes, out float value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_boxes_set_cluster_min_mag")]
        internal static partial int XImgProcEdgeBoxesSetClusterMinMag(IntPtr edgeBoxes, float value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_boxes_get_max_aspect_ratio")]
        internal static partial int XImgProcEdgeBoxesGetMaxAspectRatio(IntPtr edgeBoxes, out float value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_boxes_set_max_aspect_ratio")]
        internal static partial int XImgProcEdgeBoxesSetMaxAspectRatio(IntPtr edgeBoxes, float value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_boxes_get_min_box_area")]
        internal static partial int XImgProcEdgeBoxesGetMinBoxArea(IntPtr edgeBoxes, out float value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_boxes_set_min_box_area")]
        internal static partial int XImgProcEdgeBoxesSetMinBoxArea(IntPtr edgeBoxes, float value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_boxes_get_gamma")]
        internal static partial int XImgProcEdgeBoxesGetGamma(IntPtr edgeBoxes, out float value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_boxes_set_gamma")]
        internal static partial int XImgProcEdgeBoxesSetGamma(IntPtr edgeBoxes, float value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_boxes_get_kappa")]
        internal static partial int XImgProcEdgeBoxesGetKappa(IntPtr edgeBoxes, out float value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_boxes_set_kappa")]
        internal static partial int XImgProcEdgeBoxesSetKappa(IntPtr edgeBoxes, float value);
    }
}
#endif

