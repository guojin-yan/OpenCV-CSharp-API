#if NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    internal static partial class NativeMethods
    {
        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_gradient_deriche_x")]
        internal static partial int XImgProcGradientDericheX(IntPtr src, IntPtr dst, double alpha, double omega);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_gradient_deriche_y")]
        internal static partial int XImgProcGradientDericheY(IntPtr src, IntPtr dst, double alpha, double omega);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_gradient_paillou_x")]
        internal static partial int XImgProcGradientPaillouX(IntPtr src, IntPtr dst, double alpha, double omega);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_gradient_paillou_y")]
        internal static partial int XImgProcGradientPaillouY(IntPtr src, IntPtr dst, double alpha, double omega);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_ridge_detection_filter_create")]
        internal static partial int XImgProcRidgeDetectionFilterCreate(int ddepth, int dx, int dy, int ksize, int outDtype, double scale, double delta, int borderType, out IntPtr filter);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_ridge_detection_filter_release_handle")]
        internal static partial void XImgProcRidgeDetectionFilterReleaseHandle(IntPtr filter);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_ridge_detection_filter_get_image")]
        internal static partial int XImgProcRidgeDetectionFilterGetImage(IntPtr filter, IntPtr src, IntPtr dst);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_fourier_descriptor")]
        internal static partial int XImgProcFourierDescriptor(IntPtr src, IntPtr dst, int nbElt, int nbFd);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_transform_fd")]
        internal static partial int XImgProcTransformFD(IntPtr src, IntPtr transform, IntPtr dst, int fdContour);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_contour_sampling")]
        internal static partial int XImgProcContourSampling(IntPtr src, IntPtr dst, int nbElt);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_contour_fitting_create")]
        internal static partial int XImgProcContourFittingCreate(int ctr, int fd, out IntPtr fitting);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_contour_fitting_release_handle")]
        internal static partial void XImgProcContourFittingReleaseHandle(IntPtr fitting);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_contour_fitting_estimate_transformation")]
        internal static partial int XImgProcContourFittingEstimateTransformation(IntPtr fitting, IntPtr src, IntPtr dst, IntPtr alphaPhiST, out double distance, int fdContour);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_contour_fitting_get_ctr_size")]
        internal static partial int XImgProcContourFittingGetCtrSize(IntPtr fitting, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_contour_fitting_set_ctr_size")]
        internal static partial int XImgProcContourFittingSetCtrSize(IntPtr fitting, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_contour_fitting_get_fd_size")]
        internal static partial int XImgProcContourFittingGetFDSize(IntPtr fitting, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_contour_fitting_set_fd_size")]
        internal static partial int XImgProcContourFittingSetFDSize(IntPtr fitting, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_rl_threshold")]
        internal static partial int XImgProcRlThreshold(IntPtr src, IntPtr rlDst, double thresh, int type);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_rl_dilate")]
        internal static partial int XImgProcRlDilate(IntPtr rlSrc, IntPtr rlDst, IntPtr rlKernel, int anchorX, int anchorY);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_rl_erode")]
        internal static partial int XImgProcRlErode(IntPtr rlSrc, IntPtr rlDst, IntPtr rlKernel, int boundaryOn, int anchorX, int anchorY);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_rl_get_structuring_element")]
        internal static partial int XImgProcRlGetStructuringElement(int shape, int width, int height, IntPtr rlKernel);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_rl_paint")]
        internal static partial int XImgProcRlPaint(IntPtr image, IntPtr rlSrc, double valueV0, double valueV1, double valueV2, double valueV3);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_rl_is_morphology_possible")]
        internal static partial int XImgProcRlIsMorphologyPossible(IntPtr rlStructuringElement, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_rl_create_rle_image")]
        internal static partial int XImgProcRlCreateRleImage(XImgProcPoint3iNative[] runs, int runCount, int width, int height, IntPtr dst);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_rl_morphology_ex")]
        internal static partial int XImgProcRlMorphologyEx(IntPtr rlSrc, IntPtr rlDst, int op, IntPtr rlKernel, int boundaryOnForErosion, int anchorX, int anchorY);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_scan_segment_create")]
        internal static partial int XImgProcScanSegmentCreate(int imageWidth, int imageHeight, int numSuperpixels, int slices, int mergeSmall, out IntPtr segment);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_scan_segment_release_handle")]
        internal static partial void XImgProcScanSegmentReleaseHandle(IntPtr segment);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_scan_segment_get_number")]
        internal static partial int XImgProcScanSegmentGetNumber(IntPtr segment, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_scan_segment_iterate")]
        internal static partial int XImgProcScanSegmentIterate(IntPtr segment, IntPtr image);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_scan_segment_get_labels")]
        internal static partial int XImgProcScanSegmentGetLabels(IntPtr segment, IntPtr labels);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_scan_segment_get_label_contour_mask")]
        internal static partial int XImgProcScanSegmentGetLabelContourMask(IntPtr segment, IntPtr image, int thickLine);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_graph_segmentation_create")]
        internal static partial int XImgProcGraphSegmentationCreate(double sigma, float k, int minSize, out IntPtr segmentation);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_graph_segmentation_release_handle")]
        internal static partial void XImgProcGraphSegmentationReleaseHandle(IntPtr segmentation);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_graph_segmentation_process_image")]
        internal static partial int XImgProcGraphSegmentationProcessImage(IntPtr segmentation, IntPtr src, IntPtr dst);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_graph_segmentation_get_sigma")]
        internal static partial int XImgProcGraphSegmentationGetSigma(IntPtr segmentation, out double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_graph_segmentation_set_sigma")]
        internal static partial int XImgProcGraphSegmentationSetSigma(IntPtr segmentation, double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_graph_segmentation_get_k")]
        internal static partial int XImgProcGraphSegmentationGetK(IntPtr segmentation, out float value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_graph_segmentation_set_k")]
        internal static partial int XImgProcGraphSegmentationSetK(IntPtr segmentation, float value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_graph_segmentation_get_min_size")]
        internal static partial int XImgProcGraphSegmentationGetMinSize(IntPtr segmentation, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_graph_segmentation_set_min_size")]
        internal static partial int XImgProcGraphSegmentationSetMinSize(IntPtr segmentation, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_selective_search_strategy_create_color")]
        internal static partial int XImgProcSelectiveSearchStrategyCreateColor(out IntPtr strategy);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_selective_search_strategy_create_size")]
        internal static partial int XImgProcSelectiveSearchStrategyCreateSize(out IntPtr strategy);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_selective_search_strategy_create_texture")]
        internal static partial int XImgProcSelectiveSearchStrategyCreateTexture(out IntPtr strategy);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_selective_search_strategy_create_fill")]
        internal static partial int XImgProcSelectiveSearchStrategyCreateFill(out IntPtr strategy);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_selective_search_strategy_create_multiple")]
        internal static partial int XImgProcSelectiveSearchStrategyCreateMultiple(out IntPtr strategy);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_selective_search_strategy_release_handle")]
        internal static partial void XImgProcSelectiveSearchStrategyReleaseHandle(IntPtr strategy);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_selective_search_strategy_set_image")]
        internal static partial int XImgProcSelectiveSearchStrategySetImage(IntPtr strategy, IntPtr image, IntPtr regions, IntPtr sizes, int imageId);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_selective_search_strategy_get")]
        internal static partial int XImgProcSelectiveSearchStrategyGet(IntPtr strategy, int r1, int r2, out float value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_selective_search_strategy_merge")]
        internal static partial int XImgProcSelectiveSearchStrategyMerge(IntPtr strategy, int r1, int r2);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_selective_search_strategy_multiple_add")]
        internal static partial int XImgProcSelectiveSearchStrategyMultipleAdd(IntPtr multiple, IntPtr strategy, float weight);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_selective_search_strategy_multiple_clear")]
        internal static partial int XImgProcSelectiveSearchStrategyMultipleClear(IntPtr multiple);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_selective_search_segmentation_create")]
        internal static partial int XImgProcSelectiveSearchSegmentationCreate(out IntPtr segmentation);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_selective_search_segmentation_release_handle")]
        internal static partial void XImgProcSelectiveSearchSegmentationReleaseHandle(IntPtr segmentation);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_selective_search_segmentation_set_base_image")]
        internal static partial int XImgProcSelectiveSearchSegmentationSetBaseImage(IntPtr segmentation, IntPtr image);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_selective_search_segmentation_switch_to_single_strategy")]
        internal static partial int XImgProcSelectiveSearchSegmentationSwitchToSingleStrategy(IntPtr segmentation, int k, float sigma);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_selective_search_segmentation_switch_to_fast")]
        internal static partial int XImgProcSelectiveSearchSegmentationSwitchToFast(IntPtr segmentation, int baseK, int incK, float sigma);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_selective_search_segmentation_switch_to_quality")]
        internal static partial int XImgProcSelectiveSearchSegmentationSwitchToQuality(IntPtr segmentation, int baseK, int incK, float sigma);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_selective_search_segmentation_add_image")]
        internal static partial int XImgProcSelectiveSearchSegmentationAddImage(IntPtr segmentation, IntPtr image);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_selective_search_segmentation_clear_images")]
        internal static partial int XImgProcSelectiveSearchSegmentationClearImages(IntPtr segmentation);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_selective_search_segmentation_add_graph_segmentation")]
        internal static partial int XImgProcSelectiveSearchSegmentationAddGraphSegmentation(IntPtr segmentation, IntPtr graphSegmentation);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_selective_search_segmentation_clear_graph_segmentations")]
        internal static partial int XImgProcSelectiveSearchSegmentationClearGraphSegmentations(IntPtr segmentation);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_selective_search_segmentation_add_strategy")]
        internal static partial int XImgProcSelectiveSearchSegmentationAddStrategy(IntPtr segmentation, IntPtr strategy);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_selective_search_segmentation_clear_strategies")]
        internal static partial int XImgProcSelectiveSearchSegmentationClearStrategies(IntPtr segmentation);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_selective_search_segmentation_process_count")]
        internal static partial int XImgProcSelectiveSearchSegmentationProcessCount(IntPtr segmentation, out int rectCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_selective_search_segmentation_process_fill")]
        internal static partial int XImgProcSelectiveSearchSegmentationProcessFill(IntPtr segmentation, XImgProcRectNative[] rects, int rectCapacity, out int rectCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ximgproc_covariance_estimation")]
        internal static partial int XImgProcCovarianceEstimation(IntPtr src, IntPtr dst, int windowRows, int windowCols);
    }
}
#endif
