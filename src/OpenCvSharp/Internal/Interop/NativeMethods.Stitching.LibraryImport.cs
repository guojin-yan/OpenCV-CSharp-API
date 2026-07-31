#if NET7_0_OR_GREATER
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

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitcher_create")]
        internal static partial int StitcherCreate(int mode, out IntPtr stitcher);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitcher_release_handle")]
        internal static partial void StitcherReleaseHandle(IntPtr stitcher);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitcher_get_double_property")]
        internal static partial int StitcherGetDoubleProperty(IntPtr stitcher, int propertyId, out double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitcher_set_double_property")]
        internal static partial int StitcherSetDoubleProperty(IntPtr stitcher, int propertyId, double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitcher_get_int_property")]
        internal static partial int StitcherGetIntProperty(IntPtr stitcher, int propertyId, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitcher_set_int_property")]
        internal static partial int StitcherSetIntProperty(IntPtr stitcher, int propertyId, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitcher_estimate_transform")]
        internal static partial int StitcherEstimateTransform(IntPtr stitcher, IntPtr[] images, int imageCount, IntPtr[] masks, int maskCount, out int statusCode);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitcher_compose_panorama")]
        internal static partial int StitcherComposePanorama(IntPtr stitcher, IntPtr pano, out int statusCode);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitcher_compose_panorama_images")]
        internal static partial int StitcherComposePanoramaImages(IntPtr stitcher, IntPtr[] images, int imageCount, IntPtr pano, out int statusCode);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitcher_stitch")]
        internal static partial int StitcherStitch(IntPtr stitcher, IntPtr[] images, int imageCount, IntPtr[] masks, int maskCount, IntPtr pano, out int statusCode);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitcher_get_component_count")]
        internal static partial int StitcherGetComponentCount(IntPtr stitcher, out int componentCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitcher_get_component_fill")]
        internal static partial int StitcherGetComponentFill(IntPtr stitcher, int[] components, int componentCapacity, out int componentCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitcher_get_cameras_count")]
        internal static partial int StitcherGetCamerasCount(IntPtr stitcher, out int cameraCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitcher_get_cameras_fill")]
        internal static partial int StitcherGetCamerasFill(IntPtr stitcher, StitchingCameraParamsNative[] cameras, int cameraCapacity, out int cameraCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitcher_get_result_mask")]
        internal static partial int StitcherGetResultMask(IntPtr stitcher, IntPtr resultMask);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_exposure_create_default")]
        internal static partial int StitchingExposureCreateDefault(int type, out IntPtr compensator);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_exposure_create_no")]
        internal static partial int StitchingExposureCreateNo(out IntPtr compensator);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_exposure_create_gain")]
        internal static partial int StitchingExposureCreateGain(int numberOfFeeds, out IntPtr compensator);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_exposure_create_channels")]
        internal static partial int StitchingExposureCreateChannels(int numberOfFeeds, out IntPtr compensator);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_exposure_create_blocks_gain")]
        internal static partial int StitchingExposureCreateBlocksGain(int blockWidth, int blockHeight, int numberOfFeeds, out IntPtr compensator);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_exposure_create_blocks_channels")]
        internal static partial int StitchingExposureCreateBlocksChannels(int blockWidth, int blockHeight, int numberOfFeeds, out IntPtr compensator);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_exposure_release_handle")]
        internal static partial void StitchingExposureReleaseHandle(IntPtr compensator);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_exposure_feed")]
        internal static partial int StitchingExposureFeed(IntPtr compensator, int[] cornerX, int[] cornerY, int cornerCount, IntPtr[] images, int imageCount, IntPtr[] masks, int maskCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_exposure_apply")]
        internal static partial int StitchingExposureApply(IntPtr compensator, int index, int cornerX, int cornerY, IntPtr image, IntPtr mask);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_exposure_get_mat_gains_count")]
        internal static partial int StitchingExposureGetMatGainsCount(IntPtr compensator, out int gainCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_exposure_get_mat_gains_fill")]
        internal static partial int StitchingExposureGetMatGainsFill(IntPtr compensator, IntPtr[] gains, int gainCapacity, out int gainCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_exposure_set_mat_gains")]
        internal static partial int StitchingExposureSetMatGains(IntPtr compensator, IntPtr[] gains, int gainCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_exposure_get_update_gain")]
        internal static partial int StitchingExposureGetUpdateGain(IntPtr compensator, out int updateGain);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_exposure_set_update_gain")]
        internal static partial int StitchingExposureSetUpdateGain(IntPtr compensator, int updateGain);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_exposure_get_number_of_feeds")]
        internal static partial int StitchingExposureGetNumberOfFeeds(IntPtr compensator, out int numberOfFeeds);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_exposure_set_number_of_feeds")]
        internal static partial int StitchingExposureSetNumberOfFeeds(IntPtr compensator, int numberOfFeeds);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_exposure_get_similarity_threshold")]
        internal static partial int StitchingExposureGetSimilarityThreshold(IntPtr compensator, out double similarityThreshold);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_exposure_set_similarity_threshold")]
        internal static partial int StitchingExposureSetSimilarityThreshold(IntPtr compensator, double similarityThreshold);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_exposure_get_block_size")]
        internal static partial int StitchingExposureGetBlockSize(IntPtr compensator, out int blockWidth, out int blockHeight);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_exposure_set_block_size")]
        internal static partial int StitchingExposureSetBlockSize(IntPtr compensator, int blockWidth, int blockHeight);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_exposure_get_filtering_iterations")]
        internal static partial int StitchingExposureGetFilteringIterations(IntPtr compensator, out int filteringIterations);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_exposure_set_filtering_iterations")]
        internal static partial int StitchingExposureSetFilteringIterations(IntPtr compensator, int filteringIterations);
    }
}
#endif
