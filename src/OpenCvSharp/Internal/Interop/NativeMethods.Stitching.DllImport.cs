#if !NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal static partial class NativeMethods
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct StitchingCameraParamsNative
        {
            public double Focal;
            public double Aspect;
            public double Ppx;
            public double Ppy;
            public IntPtr R;
            public IntPtr T;
        }

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitcher_create")]
        internal static extern int StitcherCreate(int mode, out IntPtr stitcher);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitcher_release_handle")]
        internal static extern void StitcherReleaseHandle(IntPtr stitcher);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitcher_get_double_property")]
        internal static extern int StitcherGetDoubleProperty(IntPtr stitcher, int propertyId, out double value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitcher_set_double_property")]
        internal static extern int StitcherSetDoubleProperty(IntPtr stitcher, int propertyId, double value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitcher_get_int_property")]
        internal static extern int StitcherGetIntProperty(IntPtr stitcher, int propertyId, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitcher_set_int_property")]
        internal static extern int StitcherSetIntProperty(IntPtr stitcher, int propertyId, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitcher_estimate_transform")]
        internal static extern int StitcherEstimateTransform(IntPtr stitcher, IntPtr[] images, int imageCount, IntPtr[] masks, int maskCount, out int statusCode);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitcher_compose_panorama")]
        internal static extern int StitcherComposePanorama(IntPtr stitcher, IntPtr pano, out int statusCode);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitcher_compose_panorama_images")]
        internal static extern int StitcherComposePanoramaImages(IntPtr stitcher, IntPtr[] images, int imageCount, IntPtr pano, out int statusCode);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitcher_stitch")]
        internal static extern int StitcherStitch(IntPtr stitcher, IntPtr[] images, int imageCount, IntPtr[] masks, int maskCount, IntPtr pano, out int statusCode);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitcher_get_component_count")]
        internal static extern int StitcherGetComponentCount(IntPtr stitcher, out int componentCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitcher_get_component_fill")]
        internal static extern int StitcherGetComponentFill(IntPtr stitcher, int[] components, int componentCapacity, out int componentCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitcher_get_cameras_count")]
        internal static extern int StitcherGetCamerasCount(IntPtr stitcher, out int cameraCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitcher_get_cameras_fill")]
        internal static extern int StitcherGetCamerasFill(IntPtr stitcher, StitchingCameraParamsNative[] cameras, int cameraCapacity, out int cameraCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitcher_get_result_mask")]
        internal static extern int StitcherGetResultMask(IntPtr stitcher, IntPtr resultMask);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_exposure_create_default")]
        internal static extern int StitchingExposureCreateDefault(int type, out IntPtr compensator);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_exposure_create_no")]
        internal static extern int StitchingExposureCreateNo(out IntPtr compensator);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_exposure_create_gain")]
        internal static extern int StitchingExposureCreateGain(int numberOfFeeds, out IntPtr compensator);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_exposure_create_channels")]
        internal static extern int StitchingExposureCreateChannels(int numberOfFeeds, out IntPtr compensator);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_exposure_create_blocks_gain")]
        internal static extern int StitchingExposureCreateBlocksGain(int blockWidth, int blockHeight, int numberOfFeeds, out IntPtr compensator);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_exposure_create_blocks_channels")]
        internal static extern int StitchingExposureCreateBlocksChannels(int blockWidth, int blockHeight, int numberOfFeeds, out IntPtr compensator);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_exposure_release_handle")]
        internal static extern void StitchingExposureReleaseHandle(IntPtr compensator);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_exposure_feed")]
        internal static extern int StitchingExposureFeed(IntPtr compensator, int[] cornerX, int[] cornerY, int cornerCount, IntPtr[] images, int imageCount, IntPtr[] masks, int maskCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_exposure_apply")]
        internal static extern int StitchingExposureApply(IntPtr compensator, int index, int cornerX, int cornerY, IntPtr image, IntPtr mask);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_exposure_get_mat_gains_count")]
        internal static extern int StitchingExposureGetMatGainsCount(IntPtr compensator, out int gainCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_exposure_get_mat_gains_fill")]
        internal static extern int StitchingExposureGetMatGainsFill(IntPtr compensator, IntPtr[] gains, int gainCapacity, out int gainCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_exposure_set_mat_gains")]
        internal static extern int StitchingExposureSetMatGains(IntPtr compensator, IntPtr[] gains, int gainCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_exposure_get_update_gain")]
        internal static extern int StitchingExposureGetUpdateGain(IntPtr compensator, out int updateGain);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_exposure_set_update_gain")]
        internal static extern int StitchingExposureSetUpdateGain(IntPtr compensator, int updateGain);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_exposure_get_number_of_feeds")]
        internal static extern int StitchingExposureGetNumberOfFeeds(IntPtr compensator, out int numberOfFeeds);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_exposure_set_number_of_feeds")]
        internal static extern int StitchingExposureSetNumberOfFeeds(IntPtr compensator, int numberOfFeeds);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_exposure_get_similarity_threshold")]
        internal static extern int StitchingExposureGetSimilarityThreshold(IntPtr compensator, out double similarityThreshold);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_exposure_set_similarity_threshold")]
        internal static extern int StitchingExposureSetSimilarityThreshold(IntPtr compensator, double similarityThreshold);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_exposure_get_block_size")]
        internal static extern int StitchingExposureGetBlockSize(IntPtr compensator, out int blockWidth, out int blockHeight);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_exposure_set_block_size")]
        internal static extern int StitchingExposureSetBlockSize(IntPtr compensator, int blockWidth, int blockHeight);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_exposure_get_filtering_iterations")]
        internal static extern int StitchingExposureGetFilteringIterations(IntPtr compensator, out int filteringIterations);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_exposure_set_filtering_iterations")]
        internal static extern int StitchingExposureSetFilteringIterations(IntPtr compensator, int filteringIterations);
    }
}
#endif
