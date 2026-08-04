#if NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal.Interop
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

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_camera_params_get_k")]
        internal static partial int StitchingCameraParamsGetK(double focal, double aspect, double ppx, double ppy, IntPtr cameraMatrix);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_focals_from_homography")]
        internal static partial int StitchingFocalsFromHomography(IntPtr homography, out double focalX, out double focalY, out int focalXEstimated, out int focalYEstimated);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_calibrate_rotating_camera")]
        internal static partial int StitchingCalibrateRotatingCamera(IntPtr[] homographies, int homographyCount, IntPtr cameraMatrix, out int calibrated);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_estimator_create_homography")]
        internal static partial int StitchingEstimatorCreateHomography(int focalLengthsEstimated, out IntPtr estimator);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_estimator_create_affine")]
        internal static partial int StitchingEstimatorCreateAffine(out IntPtr estimator);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_estimator_create_no_bundle_adjuster")]
        internal static partial int StitchingEstimatorCreateNoBundleAdjuster(out IntPtr estimator);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_estimator_create_bundle_adjuster_reproj")]
        internal static partial int StitchingEstimatorCreateBundleAdjusterReproj(out IntPtr estimator);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_estimator_create_bundle_adjuster_ray")]
        internal static partial int StitchingEstimatorCreateBundleAdjusterRay(out IntPtr estimator);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_estimator_create_bundle_adjuster_affine")]
        internal static partial int StitchingEstimatorCreateBundleAdjusterAffine(out IntPtr estimator);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_estimator_create_bundle_adjuster_affine_partial")]
        internal static partial int StitchingEstimatorCreateBundleAdjusterAffinePartial(out IntPtr estimator);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_estimator_release_handle")]
        internal static partial void StitchingEstimatorReleaseHandle(IntPtr estimator);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_estimator_apply")]
        internal static partial int StitchingEstimatorApply(IntPtr estimator, IntPtr[] features, int featureCount, IntPtr[] pairwiseMatches, int pairwiseMatchCount, StitchingCameraParamsNative[] initialCameras, int initialCameraCount, StitchingCameraParamsNative[] cameras, int cameraCapacity, out int succeeded);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_bundle_adjuster_copy_refinement_mask")]
        internal static partial int StitchingBundleAdjusterCopyRefinementMask(IntPtr estimator, IntPtr refinementMask);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_bundle_adjuster_set_refinement_mask")]
        internal static partial int StitchingBundleAdjusterSetRefinementMask(IntPtr estimator, IntPtr refinementMask);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_bundle_adjuster_get_confidence_threshold")]
        internal static partial int StitchingBundleAdjusterGetConfidenceThreshold(IntPtr estimator, out double confidenceThreshold);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_bundle_adjuster_set_confidence_threshold")]
        internal static partial int StitchingBundleAdjusterSetConfidenceThreshold(IntPtr estimator, double confidenceThreshold);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_bundle_adjuster_get_term_criteria")]
        internal static partial int StitchingBundleAdjusterGetTermCriteria(IntPtr estimator, out int criteriaType, out int maxCount, out double epsilon);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_bundle_adjuster_set_term_criteria")]
        internal static partial int StitchingBundleAdjusterSetTermCriteria(IntPtr estimator, int criteriaType, int maxCount, double epsilon);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_wave_correct")]
        internal static partial int StitchingWaveCorrect(IntPtr[] rotationMatrices, int rotationMatrixCount, int correctionKind);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_matches_graph_as_string")]
        internal static partial int StitchingMatchesGraphAsString(byte[] pathBuffer, int pathByteCount, int[] pathOffsets, int pathCount, int pathOffsetCount, IntPtr[] pairwiseMatches, int pairwiseMatchCount, float confidenceThreshold, out IntPtr result);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_leave_biggest_component")]
        internal static partial int StitchingLeaveBiggestComponent(IntPtr[] features, int featureCount, IntPtr[] pairwiseMatches, int pairwiseMatchCount, float confidenceThreshold, IntPtr[] componentFeatures, int componentFeatureCapacity, IntPtr[] componentMatches, int componentMatchCapacity, int[] originalIndices, int originalIndexCapacity, out int componentCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_seam_finder_create_default")]
        internal static partial int StitchingSeamFinderCreateDefault(int type, out IntPtr seamFinder);
        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_seam_finder_create_dp")]
        internal static partial int StitchingSeamFinderCreateDp(byte[] cost, int costByteCount, out IntPtr seamFinder);
        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_seam_finder_create_graph_cut")]
        internal static partial int StitchingSeamFinderCreateGraphCut(byte[] cost, int costByteCount, float terminalCost, float badRegionPenalty, out IntPtr seamFinder);
        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_seam_finder_release_handle")]
        internal static partial void StitchingSeamFinderReleaseHandle(IntPtr seamFinder);
        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_seam_finder_set_dp_cost")]
        internal static partial int StitchingSeamFinderSetDpCost(IntPtr seamFinder, byte[] cost, int costByteCount);
        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_seam_finder_find")]
        internal static partial int StitchingSeamFinderFind(IntPtr seamFinder, IntPtr[] images, int imageCount, int[] cornerX, int[] cornerY, int cornerCount, IntPtr[] masks, int maskCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_timelapser_create_default")]
        internal static partial int StitchingTimelapserCreateDefault(int type, out IntPtr timelapser);
        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_timelapser_release_handle")]
        internal static partial void StitchingTimelapserReleaseHandle(IntPtr timelapser);
        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_timelapser_initialize")]
        internal static partial int StitchingTimelapserInitialize(IntPtr timelapser, int[] cornerX, int[] cornerY, int cornerCount, int[] widths, int[] heights, int sizeCount);
        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_timelapser_process")]
        internal static partial int StitchingTimelapserProcess(IntPtr timelapser, IntPtr image, IntPtr mask, int topLeftX, int topLeftY);
        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_timelapser_get_dst")]
        internal static partial int StitchingTimelapserGetDst(IntPtr timelapser, IntPtr destination);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_overlap_roi")]
        internal static partial int StitchingOverlapRoi(int firstX, int firstY, int firstWidth, int firstHeight, int secondX, int secondY, int secondWidth, int secondHeight, out StitchingRectNative roi, out int overlaps);
        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_result_roi_sizes")]
        internal static partial int StitchingResultRoiSizes(int[] cornerX, int[] cornerY, int cornerCount, int[] widths, int[] heights, int sizeCount, out StitchingRectNative roi);
        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_result_roi_images")]
        internal static partial int StitchingResultRoiImages(int[] cornerX, int[] cornerY, int cornerCount, IntPtr[] images, int imageCount, out StitchingRectNative roi);
        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_result_roi_intersection")]
        internal static partial int StitchingResultRoiIntersection(int[] cornerX, int[] cornerY, int cornerCount, int[] widths, int[] heights, int sizeCount, out StitchingRectNative roi);
        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_result_tl")]
        internal static partial int StitchingResultTl(int[] cornerX, int[] cornerY, int cornerCount, out StitchingPointNative point);
        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_select_random_subset")]
        internal static partial int StitchingSelectRandomSubset(int count, int size, int[] subset, int subsetCapacity, out int subsetCount);
        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_log_level")]
        internal static partial int StitchingLogLevel(out int level);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_spherical_projector_create")]
        internal static partial int StitchingSphericalProjectorCreate(float scale, IntPtr cameraMatrix, IntPtr rotationMatrix, IntPtr translation, out IntPtr projector);
        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_spherical_projector_release_handle")]
        internal static partial void StitchingSphericalProjectorReleaseHandle(IntPtr projector);
        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_spherical_projector_map_forward")]
        internal static partial int StitchingSphericalProjectorMapForward(IntPtr projector, float x, float y, out float u, out float v);
        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_spherical_projector_map_backward")]
        internal static partial int StitchingSphericalProjectorMapBackward(IntPtr projector, float u, float v, out float x, out float y);
    }
}
#endif
