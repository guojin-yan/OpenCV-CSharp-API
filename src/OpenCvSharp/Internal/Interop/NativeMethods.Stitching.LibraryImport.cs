#if NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal static unsafe partial class NativeMethods
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

        [StructLayout(LayoutKind.Sequential)]
        internal struct StitchingPoint2fNative
        {
            public float X;
            public float Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct StitchingPointNative
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct StitchingRectNative
        {
            public int X;
            public int Y;
            public int Width;
            public int Height;
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

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_py_rotation_warper_create_default")]
        internal static partial int StitchingPyRotationWarperCreateDefault(out IntPtr warper);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_py_rotation_warper_create")]
        internal static partial int StitchingPyRotationWarperCreate(byte[] typeUtf8, int typeByteCount, float scale, out IntPtr warper);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_py_rotation_warper_release_handle")]
        internal static partial void StitchingPyRotationWarperReleaseHandle(IntPtr warper);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_py_rotation_warper_warp_point")]
        internal static partial int StitchingPyRotationWarperWarpPoint(IntPtr warper, float pointX, float pointY, IntPtr cameraMatrix, IntPtr rotationMatrix, out StitchingPoint2fNative result);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_py_rotation_warper_warp_point_backward")]
        internal static partial int StitchingPyRotationWarperWarpPointBackward(IntPtr warper, float pointX, float pointY, IntPtr cameraMatrix, IntPtr rotationMatrix, out StitchingPoint2fNative result);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_py_rotation_warper_build_maps")]
        internal static partial int StitchingPyRotationWarperBuildMaps(IntPtr warper, int sourceWidth, int sourceHeight, IntPtr cameraMatrix, IntPtr rotationMatrix, IntPtr xMap, IntPtr yMap, out StitchingRectNative result);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_py_rotation_warper_warp")]
        internal static partial int StitchingPyRotationWarperWarp(IntPtr warper, IntPtr source, IntPtr cameraMatrix, IntPtr rotationMatrix, int interpolationMode, int borderMode, IntPtr destination, out StitchingPointNative result);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_py_rotation_warper_warp_backward")]
        internal static partial int StitchingPyRotationWarperWarpBackward(IntPtr warper, IntPtr source, IntPtr cameraMatrix, IntPtr rotationMatrix, int interpolationMode, int borderMode, int destinationWidth, int destinationHeight, IntPtr destination);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_py_rotation_warper_warp_roi")]
        internal static partial int StitchingPyRotationWarperWarpRoi(IntPtr warper, int sourceWidth, int sourceHeight, IntPtr cameraMatrix, IntPtr rotationMatrix, out StitchingRectNative result);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_py_rotation_warper_get_scale")]
        internal static partial int StitchingPyRotationWarperGetScale(IntPtr warper, out float scale);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_py_rotation_warper_set_scale")]
        internal static partial int StitchingPyRotationWarperSetScale(IntPtr warper, float scale);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_blender_create_default")]
        internal static partial int StitchingBlenderCreateDefault(int type, int tryGpu, out IntPtr blender);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_blender_create_feather")]
        internal static partial int StitchingBlenderCreateFeather(float sharpness, out IntPtr blender);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_blender_create_multi_band")]
        internal static partial int StitchingBlenderCreateMultiBand(int tryGpu, int numberOfBands, int weightType, out IntPtr blender);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_blender_release_handle")]
        internal static partial void StitchingBlenderReleaseHandle(IntPtr blender);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_blender_prepare")]
        internal static partial int StitchingBlenderPrepare(IntPtr blender, int[] cornerX, int[] cornerY, int[] widths, int[] heights, int itemCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_blender_prepare_roi")]
        internal static partial int StitchingBlenderPrepareRoi(IntPtr blender, int x, int y, int width, int height);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_blender_feed")]
        internal static partial int StitchingBlenderFeed(IntPtr blender, IntPtr image, IntPtr mask, int topLeftX, int topLeftY);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_blender_blend")]
        internal static partial int StitchingBlenderBlend(IntPtr blender, IntPtr destination, IntPtr destinationMask);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_blender_get_sharpness")]
        internal static partial int StitchingBlenderGetSharpness(IntPtr blender, out float sharpness);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_blender_set_sharpness")]
        internal static partial int StitchingBlenderSetSharpness(IntPtr blender, float sharpness);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_blender_get_number_of_bands")]
        internal static partial int StitchingBlenderGetNumberOfBands(IntPtr blender, out int numberOfBands);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_blender_set_number_of_bands")]
        internal static partial int StitchingBlenderSetNumberOfBands(IntPtr blender, int numberOfBands);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_blender_create_weight_maps")]
        internal static partial int StitchingBlenderCreateWeightMaps(IntPtr blender, IntPtr[] masks, int maskCount, int[] cornerX, int[] cornerY, int cornerCount, IntPtr[] weightMaps, int weightMapCount, out StitchingRectNative result);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_normalize_using_weight_map")]
        internal static partial int StitchingNormalizeUsingWeightMap(IntPtr weight, IntPtr source);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_create_weight_map")]
        internal static partial int StitchingCreateWeightMap(IntPtr mask, float sharpness, IntPtr weight);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_create_laplace_pyramid")]
        internal static partial int StitchingCreateLaplacePyramid(IntPtr image, int numberOfLevels, IntPtr[] pyramid, int pyramidCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_create_laplace_pyramid_gpu")]
        internal static partial int StitchingCreateLaplacePyramidGpu(IntPtr image, int numberOfLevels, IntPtr[] pyramid, int pyramidCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_restore_image_from_laplace_pyramid")]
        internal static partial int StitchingRestoreImageFromLaplacePyramid(IntPtr[] pyramid, int pyramidCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_restore_image_from_laplace_pyramid_gpu")]
        internal static partial int StitchingRestoreImageFromLaplacePyramidGpu(IntPtr[] pyramid, int pyramidCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_image_features_create")]
        internal static partial int StitchingImageFeaturesCreate(int imageIndex, int imageWidth, int imageHeight, NativeKeyPoint* keypoints, int keypointCount, IntPtr descriptors, out IntPtr features);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_image_features_release_handle")]
        internal static partial void StitchingImageFeaturesReleaseHandle(IntPtr features);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_image_features_get_image_index")]
        internal static partial int StitchingImageFeaturesGetImageIndex(IntPtr features, out int imageIndex);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_image_features_set_image_index")]
        internal static partial int StitchingImageFeaturesSetImageIndex(IntPtr features, int imageIndex);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_image_features_get_image_size")]
        internal static partial int StitchingImageFeaturesGetImageSize(IntPtr features, out int imageWidth, out int imageHeight);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_image_features_set_image_size")]
        internal static partial int StitchingImageFeaturesSetImageSize(IntPtr features, int imageWidth, int imageHeight);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_image_features_get_keypoints_count")]
        internal static partial int StitchingImageFeaturesGetKeypointsCount(IntPtr features, out int keypointCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_image_features_get_keypoints_fill")]
        internal static partial int StitchingImageFeaturesGetKeypointsFill(IntPtr features, NativeKeyPoint* keypoints, int keypointCapacity, out int keypointCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_image_features_copy_descriptors")]
        internal static partial int StitchingImageFeaturesCopyDescriptors(IntPtr features, IntPtr descriptors);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_compute_image_features")]
        internal static partial int StitchingComputeImageFeatures(int finderKind, IntPtr finderHandle, IntPtr image, IntPtr mask, IntPtr features);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_compute_image_features_batch")]
        internal static partial int StitchingComputeImageFeaturesBatch(int finderKind, IntPtr finderHandle, IntPtr[] images, int imageCount, IntPtr[] masks, int maskCount, IntPtr[] features, int featureCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_matches_info_create")]
        internal static partial int StitchingMatchesInfoCreate(out IntPtr matchesInfo);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_matches_info_release_handle")]
        internal static partial void StitchingMatchesInfoReleaseHandle(IntPtr matchesInfo);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_matches_info_get_metadata")]
        internal static partial int StitchingMatchesInfoGetMetadata(IntPtr matchesInfo, out int sourceImageIndex, out int destinationImageIndex, out int numberOfInliers, out double confidence);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_matches_info_copy_homography")]
        internal static partial int StitchingMatchesInfoCopyHomography(IntPtr matchesInfo, IntPtr homography);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_matches_info_get_matches_count")]
        internal static partial int StitchingMatchesInfoGetMatchesCount(IntPtr matchesInfo, out int matchCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_matches_info_get_matches_fill")]
        internal static partial int StitchingMatchesInfoGetMatchesFill(IntPtr matchesInfo, NativeDMatch* matches, int matchCapacity, out int matchCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_matches_info_get_inliers_count")]
        internal static partial int StitchingMatchesInfoGetInliersCount(IntPtr matchesInfo, out int inlierCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_matches_info_get_inliers_fill")]
        internal static partial int StitchingMatchesInfoGetInliersFill(IntPtr matchesInfo, byte* inliers, int inlierCapacity, out int inlierCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_features_matcher_create_best_of_two_nearest")]
        internal static partial int StitchingFeaturesMatcherCreateBestOfTwoNearest(int tryGpu, float matchConfidence, int numberOfMatchesThreshold1, int numberOfMatchesThreshold2, double matchesConfidenceThreshold, out IntPtr matcher);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_features_matcher_factory_best_of_two_nearest")]
        internal static partial int StitchingFeaturesMatcherFactoryBestOfTwoNearest(int tryGpu, float matchConfidence, int numberOfMatchesThreshold1, int numberOfMatchesThreshold2, double matchesConfidenceThreshold, out IntPtr matcher);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_features_matcher_create_range")]
        internal static partial int StitchingFeaturesMatcherCreateRange(int rangeWidth, int tryGpu, float matchConfidence, int numberOfMatchesThreshold1, int numberOfMatchesThreshold2, out IntPtr matcher);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_features_matcher_create_affine")]
        internal static partial int StitchingFeaturesMatcherCreateAffine(int fullAffine, int tryGpu, float matchConfidence, int numberOfMatchesThreshold1, out IntPtr matcher);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_features_matcher_release_handle")]
        internal static partial void StitchingFeaturesMatcherReleaseHandle(IntPtr matcher);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_features_matcher_match_pair")]
        internal static partial int StitchingFeaturesMatcherMatchPair(IntPtr matcher, IntPtr first, IntPtr second, IntPtr matchesInfo);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_features_matcher_match_batch")]
        internal static partial int StitchingFeaturesMatcherMatchBatch(IntPtr matcher, IntPtr[] features, int featureCount, IntPtr mask, IntPtr[] pairwiseMatches, int pairwiseMatchCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_features_matcher_is_thread_safe")]
        internal static partial int StitchingFeaturesMatcherIsThreadSafe(IntPtr matcher, out int isThreadSafe);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_features_matcher_collect_garbage")]
        internal static partial int StitchingFeaturesMatcherCollectGarbage(IntPtr matcher);
    }
}
#endif
