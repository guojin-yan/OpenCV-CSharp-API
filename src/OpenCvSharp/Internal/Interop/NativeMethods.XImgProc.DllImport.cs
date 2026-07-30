#if !NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal static partial class NativeMethods
    {
        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_ni_black_threshold")]
        internal static extern int XImgProcNiBlackThreshold(IntPtr src, IntPtr dst, double maxValue, int type, int blockSize, double k, int binarizationMethod, double r);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_thinning")]
        internal static extern int XImgProcThinning(IntPtr src, IntPtr dst, int thinningType);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_anisotropic_diffusion")]
        internal static extern int XImgProcAnisotropicDiffusion(IntPtr src, IntPtr dst, float alpha, float k, int niters);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_joint_bilateral_filter")]
        internal static extern int XImgProcJointBilateralFilter(IntPtr joint, IntPtr src, IntPtr dst, int d, double sigmaColor, double sigmaSpace, int borderType);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_guided_filter_run")]
        internal static extern int XImgProcGuidedFilter(IntPtr guide, IntPtr src, IntPtr dst, int radius, double eps, int dDepth, double scale);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_rolling_guidance_filter")]
        internal static extern int XImgProcRollingGuidanceFilter(IntPtr src, IntPtr dst, int d, double sigmaColor, double sigmaSpace, int numOfIter, int borderType);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_weighted_median_filter")]
        internal static extern int XImgProcWeightedMedianFilter(IntPtr joint, IntPtr src, IntPtr dst, int r, double sigma, int weightType, IntPtr mask);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_dt_filter")]
        internal static extern int XImgProcDtFilter(IntPtr guide, IntPtr src, IntPtr dst, double sigmaSpatial, double sigmaColor, int mode, int numIters);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_am_filter")]
        internal static extern int XImgProcAmFilter(IntPtr joint, IntPtr src, IntPtr dst, double sigmaS, double sigmaR, int adjustOutliers);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_bilateral_texture_filter")]
        internal static extern int XImgProcBilateralTextureFilter(IntPtr src, IntPtr dst, int fr, int numIter, double sigmaAlpha, double sigmaAvg);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_preserving_filter")]
        internal static extern int XImgProcEdgePreservingFilter(IntPtr src, IntPtr dst, int d, double threshold);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_fast_global_smoother_filter_run")]
        internal static extern int XImgProcFastGlobalSmootherFilterRun(IntPtr guide, IntPtr src, IntPtr dst, double lambda, double sigmaColor, double lambdaAttenuation, int numIter);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_l0_smooth")]
        internal static extern int XImgProcL0Smooth(IntPtr src, IntPtr dst, double lambda, double kappa);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_fast_hough_transform")]
        internal static extern int XImgProcFastHoughTransform(IntPtr src, IntPtr dst, int dstMatDepth, int angleRange, int op, int makeSkew);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_hough_point_to_line")]
        internal static extern int XImgProcHoughPointToLine(int houghX, int houghY, IntPtr srcImgInfo, int angleRange, int makeSkew, int rules, out int x1, out int y1, out int x2, out int y2);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_pei_lin_normalization")]
        internal static extern int XImgProcPeiLinNormalization(IntPtr src, IntPtr dst);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_guided_filter_create")]
        internal static extern int XImgProcGuidedFilterCreate(IntPtr guide, int radius, double eps, double scale, out IntPtr filter);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_guided_filter_release_handle")]
        internal static extern void XImgProcGuidedFilterReleaseHandle(IntPtr filter);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_guided_filter_filter")]
        internal static extern int XImgProcGuidedFilterFilter(IntPtr filter, IntPtr src, IntPtr dst, int dDepth);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_fast_global_smoother_filter_create")]
        internal static extern int XImgProcFastGlobalSmootherFilterCreate(IntPtr guide, double lambda, double sigmaColor, double lambdaAttenuation, int numIter, out IntPtr filter);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_fast_global_smoother_filter_release_handle")]
        internal static extern void XImgProcFastGlobalSmootherFilterReleaseHandle(IntPtr filter);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_fast_global_smoother_filter_filter")]
        internal static extern int XImgProcFastGlobalSmootherFilterFilter(IntPtr filter, IntPtr src, IntPtr dst);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_superpixel_slic_create")]
        internal static extern int XImgProcSuperpixelSLICCreate(IntPtr image, int algorithm, int regionSize, float ruler, out IntPtr superpixel);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_superpixel_slic_release_handle")]
        internal static extern void XImgProcSuperpixelSLICReleaseHandle(IntPtr superpixel);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_superpixel_slic_get_number")]
        internal static extern int XImgProcSuperpixelSLICGetNumber(IntPtr superpixel, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_superpixel_slic_iterate")]
        internal static extern int XImgProcSuperpixelSLICIterate(IntPtr superpixel, int numIterations);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_superpixel_slic_get_labels")]
        internal static extern int XImgProcSuperpixelSLICGetLabels(IntPtr superpixel, IntPtr labels);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_superpixel_slic_get_label_contour_mask")]
        internal static extern int XImgProcSuperpixelSLICGetLabelContourMask(IntPtr superpixel, IntPtr image, int thickLine);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_superpixel_slic_enforce_label_connectivity")]
        internal static extern int XImgProcSuperpixelSLICEnforceLabelConnectivity(IntPtr superpixel, int minElementSize);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_superpixel_seeds_create")]
        internal static extern int XImgProcSuperpixelSEEDSCreate(int imageWidth, int imageHeight, int imageChannels, int numSuperpixels, int numLevels, int prior, int histogramBins, int doubleStep, out IntPtr superpixel);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_superpixel_seeds_release_handle")]
        internal static extern void XImgProcSuperpixelSEEDSReleaseHandle(IntPtr superpixel);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_superpixel_seeds_get_number")]
        internal static extern int XImgProcSuperpixelSEEDSGetNumber(IntPtr superpixel, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_superpixel_seeds_iterate")]
        internal static extern int XImgProcSuperpixelSEEDSIterate(IntPtr superpixel, IntPtr image, int numIterations);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_superpixel_seeds_get_labels")]
        internal static extern int XImgProcSuperpixelSEEDSGetLabels(IntPtr superpixel, IntPtr labels);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_superpixel_seeds_get_label_contour_mask")]
        internal static extern int XImgProcSuperpixelSEEDSGetLabelContourMask(IntPtr superpixel, IntPtr image, int thickLine);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_superpixel_lsc_create")]
        internal static extern int XImgProcSuperpixelLSCCreate(IntPtr image, int regionSize, float ratio, out IntPtr superpixel);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_superpixel_lsc_release_handle")]
        internal static extern void XImgProcSuperpixelLSCReleaseHandle(IntPtr superpixel);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_superpixel_lsc_get_number")]
        internal static extern int XImgProcSuperpixelLSCGetNumber(IntPtr superpixel, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_superpixel_lsc_iterate")]
        internal static extern int XImgProcSuperpixelLSCIterate(IntPtr superpixel, int numIterations);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_superpixel_lsc_get_labels")]
        internal static extern int XImgProcSuperpixelLSCGetLabels(IntPtr superpixel, IntPtr labels);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_superpixel_lsc_get_label_contour_mask")]
        internal static extern int XImgProcSuperpixelLSCGetLabelContourMask(IntPtr superpixel, IntPtr image, int thickLine);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_superpixel_lsc_enforce_label_connectivity")]
        internal static extern int XImgProcSuperpixelLSCEnforceLabelConnectivity(IntPtr superpixel, int minElementSize);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_fast_line_detector_create")]
        internal static extern int XImgProcFastLineDetectorCreate(int lengthThreshold, float distanceThreshold, double cannyTh1, double cannyTh2, int cannyApertureSize, int doMerge, out IntPtr detector);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_fast_line_detector_release_handle")]
        internal static extern void XImgProcFastLineDetectorReleaseHandle(IntPtr detector);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_fast_line_detector_detect")]
        internal static extern int XImgProcFastLineDetectorDetect(IntPtr detector, IntPtr image, IntPtr lines);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_fast_line_detector_detect_count")]
        internal static extern int XImgProcFastLineDetectorDetectCount(IntPtr detector, IntPtr image, out int lineCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_fast_line_detector_detect_fill")]
        internal static extern int XImgProcFastLineDetectorDetectFill(IntPtr detector, IntPtr image, float[] lines, int lineCapacity, out int lineCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_fast_line_detector_draw_segments")]
        internal static extern int XImgProcFastLineDetectorDrawSegments(IntPtr detector, IntPtr image, IntPtr lines, int drawArrow, double colorV0, double colorV1, double colorV2, double colorV3, int lineThickness);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_fast_line_detector_draw_segments_array")]
        internal static extern int XImgProcFastLineDetectorDrawSegmentsArray(IntPtr detector, IntPtr image, float[] lines, int lineCount, int drawArrow, double colorV0, double colorV1, double colorV2, double colorV3, int lineThickness);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_disparity_wls_filter_create_generic")]
        internal static extern int XImgProcDisparityWLSFilterCreateGeneric(int useConfidence, out IntPtr filter);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_disparity_wls_filter_create_from_stereo_bm")]
        internal static extern int XImgProcDisparityWLSFilterCreateFromStereoBM(IntPtr matcherLeft, out IntPtr filter);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_disparity_wls_filter_create_from_stereo_sgbm")]
        internal static extern int XImgProcDisparityWLSFilterCreateFromStereoSGBM(IntPtr matcherLeft, out IntPtr filter);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_disparity_wls_filter_create_from_stereo_matcher")]
        internal static extern int XImgProcDisparityWLSFilterCreateFromStereoMatcher(IntPtr matcherLeft, out IntPtr filter);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_create_right_matcher_from_stereo_bm")]
        internal static extern int XImgProcCreateRightMatcherFromStereoBM(IntPtr matcherLeft, out IntPtr matcherRight);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_create_right_matcher_from_stereo_sgbm")]
        internal static extern int XImgProcCreateRightMatcherFromStereoSGBM(IntPtr matcherLeft, out IntPtr matcherRight);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_create_right_matcher_from_stereo_matcher")]
        internal static extern int XImgProcCreateRightMatcherFromStereoMatcher(IntPtr matcherLeft, out IntPtr matcherRight);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_disparity_wls_filter_release_handle")]
        internal static extern void XImgProcDisparityWLSFilterReleaseHandle(IntPtr filter);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_disparity_filter_filter")]
        internal static extern int XImgProcDisparityFilterFilter(IntPtr filter, IntPtr disparityMapLeft, IntPtr leftView, IntPtr filteredDisparityMap, IntPtr disparityMapRight, ref XImgProcRectNative roi, IntPtr rightView);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_disparity_wls_filter_filter")]
        internal static extern int XImgProcDisparityWLSFilterFilter(IntPtr filter, IntPtr disparityMapLeft, IntPtr leftView, IntPtr filteredDisparityMap, IntPtr disparityMapRight, ref XImgProcRectNative roi, IntPtr rightView);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_disparity_wls_filter_get_lambda")]
        internal static extern int XImgProcDisparityWLSFilterGetLambda(IntPtr filter, out double value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_disparity_wls_filter_set_lambda")]
        internal static extern int XImgProcDisparityWLSFilterSetLambda(IntPtr filter, double value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_disparity_wls_filter_get_sigma_color")]
        internal static extern int XImgProcDisparityWLSFilterGetSigmaColor(IntPtr filter, out double value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_disparity_wls_filter_set_sigma_color")]
        internal static extern int XImgProcDisparityWLSFilterSetSigmaColor(IntPtr filter, double value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_disparity_wls_filter_get_lrc_thresh")]
        internal static extern int XImgProcDisparityWLSFilterGetLrcThresh(IntPtr filter, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_disparity_wls_filter_set_lrc_thresh")]
        internal static extern int XImgProcDisparityWLSFilterSetLrcThresh(IntPtr filter, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_disparity_wls_filter_get_depth_discontinuity_radius")]
        internal static extern int XImgProcDisparityWLSFilterGetDepthDiscontinuityRadius(IntPtr filter, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_disparity_wls_filter_set_depth_discontinuity_radius")]
        internal static extern int XImgProcDisparityWLSFilterSetDepthDiscontinuityRadius(IntPtr filter, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_disparity_wls_filter_get_confidence_map")]
        internal static extern int XImgProcDisparityWLSFilterGetConfidenceMap(IntPtr filter, IntPtr confidenceMap);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_disparity_wls_filter_get_roi")]
        internal static extern int XImgProcDisparityWLSFilterGetRoi(IntPtr filter, out XImgProcRectNative roi);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_get_disparity_vis")]
        internal static extern int XImgProcGetDisparityVis(IntPtr src, IntPtr dst, double scale);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_compute_mse")]
        internal static extern int XImgProcComputeMSE(IntPtr gt, IntPtr src, ref XImgProcRectNative roi, out double value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_compute_bad_pixel_percent")]
        internal static extern int XImgProcComputeBadPixelPercent(IntPtr gt, IntPtr src, ref XImgProcRectNative roi, int thresh, out double value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_fast_bilateral_solver_filter_create")]
        internal static extern int XImgProcFastBilateralSolverFilterCreate(IntPtr guide, double sigmaSpatial, double sigmaLuma, double sigmaChroma, double lambda, int numIter, double maxTol, out IntPtr filter);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_fast_bilateral_solver_filter_release_handle")]
        internal static extern void XImgProcFastBilateralSolverFilterReleaseHandle(IntPtr filter);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_fast_bilateral_solver_filter_filter")]
        internal static extern int XImgProcFastBilateralSolverFilterFilter(IntPtr filter, IntPtr src, IntPtr confidence, IntPtr dst);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_fast_bilateral_solver_filter_run")]
        internal static extern int XImgProcFastBilateralSolverFilterRun(IntPtr guide, IntPtr src, IntPtr confidence, IntPtr dst, double sigmaSpatial, double sigmaLuma, double sigmaChroma, double lambda, int numIter, double maxTol);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_aware_interpolator_create")]
        internal static extern int XImgProcEdgeAwareInterpolatorCreate(out IntPtr interpolator);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_aware_interpolator_release_handle")]
        internal static extern void XImgProcEdgeAwareInterpolatorReleaseHandle(IntPtr interpolator);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_ric_interpolator_create")]
        internal static extern int XImgProcRICInterpolatorCreate(out IntPtr interpolator);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_ric_interpolator_release_handle")]
        internal static extern void XImgProcRICInterpolatorReleaseHandle(IntPtr interpolator);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_sparse_match_interpolator_interpolate")]
        internal static extern int XImgProcSparseMatchInterpolatorInterpolate(IntPtr interpolator, IntPtr fromImage, IntPtr fromPoints, IntPtr toImage, IntPtr toPoints, IntPtr denseFlow);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_aware_interpolator_interpolate")]
        internal static extern int XImgProcEdgeAwareInterpolatorInterpolate(IntPtr interpolator, IntPtr fromImage, IntPtr fromPoints, IntPtr toImage, IntPtr toPoints, IntPtr denseFlow);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_ric_interpolator_interpolate")]
        internal static extern int XImgProcRICInterpolatorInterpolate(IntPtr interpolator, IntPtr fromImage, IntPtr fromPoints, IntPtr toImage, IntPtr toPoints, IntPtr denseFlow);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_aware_interpolator_set_cost_map")]
        internal static extern int XImgProcEdgeAwareInterpolatorSetCostMap(IntPtr interpolator, IntPtr costMap);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_ric_interpolator_set_cost_map")]
        internal static extern int XImgProcRICInterpolatorSetCostMap(IntPtr interpolator, IntPtr costMap);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_aware_interpolator_get_k")]
        internal static extern int XImgProcEdgeAwareInterpolatorGetK(IntPtr interpolator, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_aware_interpolator_set_k")]
        internal static extern int XImgProcEdgeAwareInterpolatorSetK(IntPtr interpolator, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_aware_interpolator_get_sigma")]
        internal static extern int XImgProcEdgeAwareInterpolatorGetSigma(IntPtr interpolator, out float value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_aware_interpolator_set_sigma")]
        internal static extern int XImgProcEdgeAwareInterpolatorSetSigma(IntPtr interpolator, float value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_aware_interpolator_get_lambda")]
        internal static extern int XImgProcEdgeAwareInterpolatorGetLambda(IntPtr interpolator, out float value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_aware_interpolator_set_lambda")]
        internal static extern int XImgProcEdgeAwareInterpolatorSetLambda(IntPtr interpolator, float value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_aware_interpolator_get_use_post_processing")]
        internal static extern int XImgProcEdgeAwareInterpolatorGetUsePostProcessing(IntPtr interpolator, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_aware_interpolator_set_use_post_processing")]
        internal static extern int XImgProcEdgeAwareInterpolatorSetUsePostProcessing(IntPtr interpolator, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_aware_interpolator_get_fgs_lambda")]
        internal static extern int XImgProcEdgeAwareInterpolatorGetFgsLambda(IntPtr interpolator, out float value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_aware_interpolator_set_fgs_lambda")]
        internal static extern int XImgProcEdgeAwareInterpolatorSetFgsLambda(IntPtr interpolator, float value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_aware_interpolator_get_fgs_sigma")]
        internal static extern int XImgProcEdgeAwareInterpolatorGetFgsSigma(IntPtr interpolator, out float value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_aware_interpolator_set_fgs_sigma")]
        internal static extern int XImgProcEdgeAwareInterpolatorSetFgsSigma(IntPtr interpolator, float value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_ric_interpolator_get_k")]
        internal static extern int XImgProcRICInterpolatorGetK(IntPtr interpolator, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_ric_interpolator_set_k")]
        internal static extern int XImgProcRICInterpolatorSetK(IntPtr interpolator, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_ric_interpolator_get_superpixel_size")]
        internal static extern int XImgProcRICInterpolatorGetSuperpixelSize(IntPtr interpolator, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_ric_interpolator_set_superpixel_size")]
        internal static extern int XImgProcRICInterpolatorSetSuperpixelSize(IntPtr interpolator, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_ric_interpolator_get_superpixel_nn_count")]
        internal static extern int XImgProcRICInterpolatorGetSuperpixelNNCount(IntPtr interpolator, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_ric_interpolator_set_superpixel_nn_count")]
        internal static extern int XImgProcRICInterpolatorSetSuperpixelNNCount(IntPtr interpolator, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_ric_interpolator_get_superpixel_ruler")]
        internal static extern int XImgProcRICInterpolatorGetSuperpixelRuler(IntPtr interpolator, out float value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_ric_interpolator_set_superpixel_ruler")]
        internal static extern int XImgProcRICInterpolatorSetSuperpixelRuler(IntPtr interpolator, float value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_ric_interpolator_get_superpixel_mode")]
        internal static extern int XImgProcRICInterpolatorGetSuperpixelMode(IntPtr interpolator, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_ric_interpolator_set_superpixel_mode")]
        internal static extern int XImgProcRICInterpolatorSetSuperpixelMode(IntPtr interpolator, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_ric_interpolator_get_alpha")]
        internal static extern int XImgProcRICInterpolatorGetAlpha(IntPtr interpolator, out float value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_ric_interpolator_set_alpha")]
        internal static extern int XImgProcRICInterpolatorSetAlpha(IntPtr interpolator, float value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_ric_interpolator_get_model_iter")]
        internal static extern int XImgProcRICInterpolatorGetModelIter(IntPtr interpolator, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_ric_interpolator_set_model_iter")]
        internal static extern int XImgProcRICInterpolatorSetModelIter(IntPtr interpolator, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_ric_interpolator_get_refine_models")]
        internal static extern int XImgProcRICInterpolatorGetRefineModels(IntPtr interpolator, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_ric_interpolator_set_refine_models")]
        internal static extern int XImgProcRICInterpolatorSetRefineModels(IntPtr interpolator, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_ric_interpolator_get_max_flow")]
        internal static extern int XImgProcRICInterpolatorGetMaxFlow(IntPtr interpolator, out float value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_ric_interpolator_set_max_flow")]
        internal static extern int XImgProcRICInterpolatorSetMaxFlow(IntPtr interpolator, float value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_ric_interpolator_get_use_variational_refinement")]
        internal static extern int XImgProcRICInterpolatorGetUseVariationalRefinement(IntPtr interpolator, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_ric_interpolator_set_use_variational_refinement")]
        internal static extern int XImgProcRICInterpolatorSetUseVariationalRefinement(IntPtr interpolator, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_ric_interpolator_get_use_global_smoother_filter")]
        internal static extern int XImgProcRICInterpolatorGetUseGlobalSmootherFilter(IntPtr interpolator, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_ric_interpolator_set_use_global_smoother_filter")]
        internal static extern int XImgProcRICInterpolatorSetUseGlobalSmootherFilter(IntPtr interpolator, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_ric_interpolator_get_fgs_lambda")]
        internal static extern int XImgProcRICInterpolatorGetFgsLambda(IntPtr interpolator, out float value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_ric_interpolator_set_fgs_lambda")]
        internal static extern int XImgProcRICInterpolatorSetFgsLambda(IntPtr interpolator, float value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_ric_interpolator_get_fgs_sigma")]
        internal static extern int XImgProcRICInterpolatorGetFgsSigma(IntPtr interpolator, out float value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_ric_interpolator_set_fgs_sigma")]
        internal static extern int XImgProcRICInterpolatorSetFgsSigma(IntPtr interpolator, float value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_drawing_create")]
        internal static extern int XImgProcEdgeDrawingCreate(out IntPtr detector);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_drawing_release_handle")]
        internal static extern void XImgProcEdgeDrawingReleaseHandle(IntPtr detector);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_drawing_get_params")]
        internal static extern int XImgProcEdgeDrawingGetParams(IntPtr detector, out XImgProcEdgeDrawingParamsNative parameters);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_drawing_set_params")]
        internal static extern int XImgProcEdgeDrawingSetParams(IntPtr detector, ref XImgProcEdgeDrawingParamsNative parameters);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_drawing_detect_edges")]
        internal static extern int XImgProcEdgeDrawingDetectEdges(IntPtr detector, IntPtr src);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_drawing_get_edge_image")]
        internal static extern int XImgProcEdgeDrawingGetEdgeImage(IntPtr detector, IntPtr dst);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_drawing_get_gradient_image")]
        internal static extern int XImgProcEdgeDrawingGetGradientImage(IntPtr detector, IntPtr dst);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_drawing_get_segments_count")]
        internal static extern int XImgProcEdgeDrawingGetSegmentsCount(IntPtr detector, out int groupCount, out int pointCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_drawing_get_segments_fill")]
        internal static extern int XImgProcEdgeDrawingGetSegmentsFill(IntPtr detector, int[] offsets, int offsetCapacity, XImgProcPointNative[] points, int pointCapacity, out int groupCount, out int pointCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_drawing_detect_lines")]
        internal static extern int XImgProcEdgeDrawingDetectLines(IntPtr detector, IntPtr lines);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_drawing_detect_lines_count")]
        internal static extern int XImgProcEdgeDrawingDetectLinesCount(IntPtr detector, out int lineCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_drawing_detect_lines_fill")]
        internal static extern int XImgProcEdgeDrawingDetectLinesFill(IntPtr detector, float[] lines, int lineCapacity, out int lineCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_drawing_get_segment_indices_of_lines_count")]
        internal static extern int XImgProcEdgeDrawingGetSegmentIndicesOfLinesCount(IntPtr detector, out int indexCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_drawing_get_segment_indices_of_lines_fill")]
        internal static extern int XImgProcEdgeDrawingGetSegmentIndicesOfLinesFill(IntPtr detector, int[] indices, int indexCapacity, out int indexCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_drawing_detect_ellipses")]
        internal static extern int XImgProcEdgeDrawingDetectEllipses(IntPtr detector, IntPtr ellipses);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_drawing_detect_ellipses_count")]
        internal static extern int XImgProcEdgeDrawingDetectEllipsesCount(IntPtr detector, out int ellipseCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_drawing_detect_ellipses_fill")]
        internal static extern int XImgProcEdgeDrawingDetectEllipsesFill(IntPtr detector, double[] ellipses, int ellipseCapacity, out int ellipseCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_boxes_create")]
        internal static extern int XImgProcEdgeBoxesCreate(float alpha, float beta, float eta, float minScore, int maxBoxes, float edgeMinMag, float edgeMergeThr, float clusterMinMag, float maxAspectRatio, float minBoxArea, float gamma, float kappa, out IntPtr edgeBoxes);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_boxes_release_handle")]
        internal static extern void XImgProcEdgeBoxesReleaseHandle(IntPtr edgeBoxes);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_boxes_get_bounding_boxes_count")]
        internal static extern int XImgProcEdgeBoxesGetBoundingBoxesCount(IntPtr edgeBoxes, IntPtr edgeMap, IntPtr orientationMap, out int boxCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_boxes_get_bounding_boxes_fill")]
        internal static extern int XImgProcEdgeBoxesGetBoundingBoxesFill(IntPtr edgeBoxes, IntPtr edgeMap, IntPtr orientationMap, XImgProcEdgeBoxNative[] boxes, int boxCapacity, out int boxCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_boxes_get_alpha")]
        internal static extern int XImgProcEdgeBoxesGetAlpha(IntPtr edgeBoxes, out float value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_boxes_set_alpha")]
        internal static extern int XImgProcEdgeBoxesSetAlpha(IntPtr edgeBoxes, float value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_boxes_get_beta")]
        internal static extern int XImgProcEdgeBoxesGetBeta(IntPtr edgeBoxes, out float value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_boxes_set_beta")]
        internal static extern int XImgProcEdgeBoxesSetBeta(IntPtr edgeBoxes, float value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_boxes_get_eta")]
        internal static extern int XImgProcEdgeBoxesGetEta(IntPtr edgeBoxes, out float value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_boxes_set_eta")]
        internal static extern int XImgProcEdgeBoxesSetEta(IntPtr edgeBoxes, float value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_boxes_get_min_score")]
        internal static extern int XImgProcEdgeBoxesGetMinScore(IntPtr edgeBoxes, out float value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_boxes_set_min_score")]
        internal static extern int XImgProcEdgeBoxesSetMinScore(IntPtr edgeBoxes, float value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_boxes_get_max_boxes")]
        internal static extern int XImgProcEdgeBoxesGetMaxBoxes(IntPtr edgeBoxes, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_boxes_set_max_boxes")]
        internal static extern int XImgProcEdgeBoxesSetMaxBoxes(IntPtr edgeBoxes, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_boxes_get_edge_min_mag")]
        internal static extern int XImgProcEdgeBoxesGetEdgeMinMag(IntPtr edgeBoxes, out float value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_boxes_set_edge_min_mag")]
        internal static extern int XImgProcEdgeBoxesSetEdgeMinMag(IntPtr edgeBoxes, float value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_boxes_get_edge_merge_thr")]
        internal static extern int XImgProcEdgeBoxesGetEdgeMergeThr(IntPtr edgeBoxes, out float value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_boxes_set_edge_merge_thr")]
        internal static extern int XImgProcEdgeBoxesSetEdgeMergeThr(IntPtr edgeBoxes, float value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_boxes_get_cluster_min_mag")]
        internal static extern int XImgProcEdgeBoxesGetClusterMinMag(IntPtr edgeBoxes, out float value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_boxes_set_cluster_min_mag")]
        internal static extern int XImgProcEdgeBoxesSetClusterMinMag(IntPtr edgeBoxes, float value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_boxes_get_max_aspect_ratio")]
        internal static extern int XImgProcEdgeBoxesGetMaxAspectRatio(IntPtr edgeBoxes, out float value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_boxes_set_max_aspect_ratio")]
        internal static extern int XImgProcEdgeBoxesSetMaxAspectRatio(IntPtr edgeBoxes, float value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_boxes_get_min_box_area")]
        internal static extern int XImgProcEdgeBoxesGetMinBoxArea(IntPtr edgeBoxes, out float value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_boxes_set_min_box_area")]
        internal static extern int XImgProcEdgeBoxesSetMinBoxArea(IntPtr edgeBoxes, float value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_boxes_get_gamma")]
        internal static extern int XImgProcEdgeBoxesGetGamma(IntPtr edgeBoxes, out float value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_boxes_set_gamma")]
        internal static extern int XImgProcEdgeBoxesSetGamma(IntPtr edgeBoxes, float value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_boxes_get_kappa")]
        internal static extern int XImgProcEdgeBoxesGetKappa(IntPtr edgeBoxes, out float value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_edge_boxes_set_kappa")]
        internal static extern int XImgProcEdgeBoxesSetKappa(IntPtr edgeBoxes, float value);
    }
}
#endif
