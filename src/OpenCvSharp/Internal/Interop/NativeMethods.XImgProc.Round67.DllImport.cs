#if !NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal static partial class NativeMethods
    {
        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_gradient_deriche_x")]
        internal static extern int XImgProcGradientDericheX(IntPtr src, IntPtr dst, double alpha, double omega);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_gradient_deriche_y")]
        internal static extern int XImgProcGradientDericheY(IntPtr src, IntPtr dst, double alpha, double omega);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_gradient_paillou_x")]
        internal static extern int XImgProcGradientPaillouX(IntPtr src, IntPtr dst, double alpha, double omega);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_gradient_paillou_y")]
        internal static extern int XImgProcGradientPaillouY(IntPtr src, IntPtr dst, double alpha, double omega);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_ridge_detection_filter_create")]
        internal static extern int XImgProcRidgeDetectionFilterCreate(int ddepth, int dx, int dy, int ksize, int outDtype, double scale, double delta, int borderType, out IntPtr filter);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_ridge_detection_filter_release_handle")]
        internal static extern void XImgProcRidgeDetectionFilterReleaseHandle(IntPtr filter);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_ridge_detection_filter_get_image")]
        internal static extern int XImgProcRidgeDetectionFilterGetImage(IntPtr filter, IntPtr src, IntPtr dst);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_fourier_descriptor")]
        internal static extern int XImgProcFourierDescriptor(IntPtr src, IntPtr dst, int nbElt, int nbFd);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_transform_fd")]
        internal static extern int XImgProcTransformFD(IntPtr src, IntPtr transform, IntPtr dst, int fdContour);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_contour_sampling")]
        internal static extern int XImgProcContourSampling(IntPtr src, IntPtr dst, int nbElt);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_contour_fitting_create")]
        internal static extern int XImgProcContourFittingCreate(int ctr, int fd, out IntPtr fitting);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_contour_fitting_release_handle")]
        internal static extern void XImgProcContourFittingReleaseHandle(IntPtr fitting);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_contour_fitting_estimate_transformation")]
        internal static extern int XImgProcContourFittingEstimateTransformation(IntPtr fitting, IntPtr src, IntPtr dst, IntPtr alphaPhiST, out double distance, int fdContour);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_contour_fitting_get_ctr_size")]
        internal static extern int XImgProcContourFittingGetCtrSize(IntPtr fitting, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_contour_fitting_set_ctr_size")]
        internal static extern int XImgProcContourFittingSetCtrSize(IntPtr fitting, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_contour_fitting_get_fd_size")]
        internal static extern int XImgProcContourFittingGetFDSize(IntPtr fitting, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_contour_fitting_set_fd_size")]
        internal static extern int XImgProcContourFittingSetFDSize(IntPtr fitting, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_rl_threshold")]
        internal static extern int XImgProcRlThreshold(IntPtr src, IntPtr rlDst, double thresh, int type);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_rl_dilate")]
        internal static extern int XImgProcRlDilate(IntPtr rlSrc, IntPtr rlDst, IntPtr rlKernel, int anchorX, int anchorY);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_rl_erode")]
        internal static extern int XImgProcRlErode(IntPtr rlSrc, IntPtr rlDst, IntPtr rlKernel, int boundaryOn, int anchorX, int anchorY);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_rl_get_structuring_element")]
        internal static extern int XImgProcRlGetStructuringElement(int shape, int width, int height, IntPtr rlKernel);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_rl_paint")]
        internal static extern int XImgProcRlPaint(IntPtr image, IntPtr rlSrc, double valueV0, double valueV1, double valueV2, double valueV3);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_rl_is_morphology_possible")]
        internal static extern int XImgProcRlIsMorphologyPossible(IntPtr rlStructuringElement, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_rl_create_rle_image")]
        internal static extern int XImgProcRlCreateRleImage(XImgProcPoint3iNative[] runs, int runCount, int width, int height, IntPtr dst);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_rl_morphology_ex")]
        internal static extern int XImgProcRlMorphologyEx(IntPtr rlSrc, IntPtr rlDst, int op, IntPtr rlKernel, int boundaryOnForErosion, int anchorX, int anchorY);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_scan_segment_create")]
        internal static extern int XImgProcScanSegmentCreate(int imageWidth, int imageHeight, int numSuperpixels, int slices, int mergeSmall, out IntPtr segment);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_scan_segment_release_handle")]
        internal static extern void XImgProcScanSegmentReleaseHandle(IntPtr segment);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_scan_segment_get_number")]
        internal static extern int XImgProcScanSegmentGetNumber(IntPtr segment, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_scan_segment_iterate")]
        internal static extern int XImgProcScanSegmentIterate(IntPtr segment, IntPtr image);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_scan_segment_get_labels")]
        internal static extern int XImgProcScanSegmentGetLabels(IntPtr segment, IntPtr labels);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_scan_segment_get_label_contour_mask")]
        internal static extern int XImgProcScanSegmentGetLabelContourMask(IntPtr segment, IntPtr image, int thickLine);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_graph_segmentation_create")]
        internal static extern int XImgProcGraphSegmentationCreate(double sigma, float k, int minSize, out IntPtr segmentation);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_graph_segmentation_release_handle")]
        internal static extern void XImgProcGraphSegmentationReleaseHandle(IntPtr segmentation);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_graph_segmentation_process_image")]
        internal static extern int XImgProcGraphSegmentationProcessImage(IntPtr segmentation, IntPtr src, IntPtr dst);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_graph_segmentation_get_sigma")]
        internal static extern int XImgProcGraphSegmentationGetSigma(IntPtr segmentation, out double value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_graph_segmentation_set_sigma")]
        internal static extern int XImgProcGraphSegmentationSetSigma(IntPtr segmentation, double value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_graph_segmentation_get_k")]
        internal static extern int XImgProcGraphSegmentationGetK(IntPtr segmentation, out float value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_graph_segmentation_set_k")]
        internal static extern int XImgProcGraphSegmentationSetK(IntPtr segmentation, float value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_graph_segmentation_get_min_size")]
        internal static extern int XImgProcGraphSegmentationGetMinSize(IntPtr segmentation, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_graph_segmentation_set_min_size")]
        internal static extern int XImgProcGraphSegmentationSetMinSize(IntPtr segmentation, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_selective_search_strategy_create_color")]
        internal static extern int XImgProcSelectiveSearchStrategyCreateColor(out IntPtr strategy);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_selective_search_strategy_create_size")]
        internal static extern int XImgProcSelectiveSearchStrategyCreateSize(out IntPtr strategy);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_selective_search_strategy_create_texture")]
        internal static extern int XImgProcSelectiveSearchStrategyCreateTexture(out IntPtr strategy);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_selective_search_strategy_create_fill")]
        internal static extern int XImgProcSelectiveSearchStrategyCreateFill(out IntPtr strategy);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_selective_search_strategy_create_multiple")]
        internal static extern int XImgProcSelectiveSearchStrategyCreateMultiple(out IntPtr strategy);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_selective_search_strategy_release_handle")]
        internal static extern void XImgProcSelectiveSearchStrategyReleaseHandle(IntPtr strategy);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_selective_search_strategy_set_image")]
        internal static extern int XImgProcSelectiveSearchStrategySetImage(IntPtr strategy, IntPtr image, IntPtr regions, IntPtr sizes, int imageId);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_selective_search_strategy_get")]
        internal static extern int XImgProcSelectiveSearchStrategyGet(IntPtr strategy, int r1, int r2, out float value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_selective_search_strategy_merge")]
        internal static extern int XImgProcSelectiveSearchStrategyMerge(IntPtr strategy, int r1, int r2);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_selective_search_strategy_multiple_add")]
        internal static extern int XImgProcSelectiveSearchStrategyMultipleAdd(IntPtr multiple, IntPtr strategy, float weight);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_selective_search_strategy_multiple_clear")]
        internal static extern int XImgProcSelectiveSearchStrategyMultipleClear(IntPtr multiple);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_selective_search_segmentation_create")]
        internal static extern int XImgProcSelectiveSearchSegmentationCreate(out IntPtr segmentation);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_selective_search_segmentation_release_handle")]
        internal static extern void XImgProcSelectiveSearchSegmentationReleaseHandle(IntPtr segmentation);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_selective_search_segmentation_set_base_image")]
        internal static extern int XImgProcSelectiveSearchSegmentationSetBaseImage(IntPtr segmentation, IntPtr image);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_selective_search_segmentation_switch_to_single_strategy")]
        internal static extern int XImgProcSelectiveSearchSegmentationSwitchToSingleStrategy(IntPtr segmentation, int k, float sigma);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_selective_search_segmentation_switch_to_fast")]
        internal static extern int XImgProcSelectiveSearchSegmentationSwitchToFast(IntPtr segmentation, int baseK, int incK, float sigma);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_selective_search_segmentation_switch_to_quality")]
        internal static extern int XImgProcSelectiveSearchSegmentationSwitchToQuality(IntPtr segmentation, int baseK, int incK, float sigma);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_selective_search_segmentation_add_image")]
        internal static extern int XImgProcSelectiveSearchSegmentationAddImage(IntPtr segmentation, IntPtr image);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_selective_search_segmentation_clear_images")]
        internal static extern int XImgProcSelectiveSearchSegmentationClearImages(IntPtr segmentation);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_selective_search_segmentation_add_graph_segmentation")]
        internal static extern int XImgProcSelectiveSearchSegmentationAddGraphSegmentation(IntPtr segmentation, IntPtr graphSegmentation);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_selective_search_segmentation_clear_graph_segmentations")]
        internal static extern int XImgProcSelectiveSearchSegmentationClearGraphSegmentations(IntPtr segmentation);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_selective_search_segmentation_add_strategy")]
        internal static extern int XImgProcSelectiveSearchSegmentationAddStrategy(IntPtr segmentation, IntPtr strategy);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_selective_search_segmentation_clear_strategies")]
        internal static extern int XImgProcSelectiveSearchSegmentationClearStrategies(IntPtr segmentation);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_selective_search_segmentation_process_count")]
        internal static extern int XImgProcSelectiveSearchSegmentationProcessCount(IntPtr segmentation, out int rectCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_selective_search_segmentation_process_fill")]
        internal static extern int XImgProcSelectiveSearchSegmentationProcessFill(IntPtr segmentation, XImgProcRectNative[] rects, int rectCapacity, out int rectCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_covariance_estimation")]
        internal static extern int XImgProcCovarianceEstimation(IntPtr src, IntPtr dst, int windowRows, int windowCols);
    }
}
#endif
