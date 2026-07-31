#if !NET7_0_OR_GREATER
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

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_py_rotation_warper_create_default")]
        internal static extern int StitchingPyRotationWarperCreateDefault(out IntPtr warper);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_py_rotation_warper_create")]
        internal static extern int StitchingPyRotationWarperCreate(byte[] typeUtf8, int typeByteCount, float scale, out IntPtr warper);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_py_rotation_warper_release_handle")]
        internal static extern void StitchingPyRotationWarperReleaseHandle(IntPtr warper);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_py_rotation_warper_warp_point")]
        internal static extern int StitchingPyRotationWarperWarpPoint(IntPtr warper, float pointX, float pointY, IntPtr cameraMatrix, IntPtr rotationMatrix, out StitchingPoint2fNative result);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_py_rotation_warper_warp_point_backward")]
        internal static extern int StitchingPyRotationWarperWarpPointBackward(IntPtr warper, float pointX, float pointY, IntPtr cameraMatrix, IntPtr rotationMatrix, out StitchingPoint2fNative result);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_py_rotation_warper_build_maps")]
        internal static extern int StitchingPyRotationWarperBuildMaps(IntPtr warper, int sourceWidth, int sourceHeight, IntPtr cameraMatrix, IntPtr rotationMatrix, IntPtr xMap, IntPtr yMap, out StitchingRectNative result);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_py_rotation_warper_warp")]
        internal static extern int StitchingPyRotationWarperWarp(IntPtr warper, IntPtr source, IntPtr cameraMatrix, IntPtr rotationMatrix, int interpolationMode, int borderMode, IntPtr destination, out StitchingPointNative result);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_py_rotation_warper_warp_backward")]
        internal static extern int StitchingPyRotationWarperWarpBackward(IntPtr warper, IntPtr source, IntPtr cameraMatrix, IntPtr rotationMatrix, int interpolationMode, int borderMode, int destinationWidth, int destinationHeight, IntPtr destination);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_py_rotation_warper_warp_roi")]
        internal static extern int StitchingPyRotationWarperWarpRoi(IntPtr warper, int sourceWidth, int sourceHeight, IntPtr cameraMatrix, IntPtr rotationMatrix, out StitchingRectNative result);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_py_rotation_warper_get_scale")]
        internal static extern int StitchingPyRotationWarperGetScale(IntPtr warper, out float scale);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_py_rotation_warper_set_scale")]
        internal static extern int StitchingPyRotationWarperSetScale(IntPtr warper, float scale);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_blender_create_default")]
        internal static extern int StitchingBlenderCreateDefault(int type, int tryGpu, out IntPtr blender);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_blender_create_feather")]
        internal static extern int StitchingBlenderCreateFeather(float sharpness, out IntPtr blender);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_blender_create_multi_band")]
        internal static extern int StitchingBlenderCreateMultiBand(int tryGpu, int numberOfBands, int weightType, out IntPtr blender);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_blender_release_handle")]
        internal static extern void StitchingBlenderReleaseHandle(IntPtr blender);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_blender_prepare")]
        internal static extern int StitchingBlenderPrepare(IntPtr blender, int[] cornerX, int[] cornerY, int[] widths, int[] heights, int itemCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_blender_prepare_roi")]
        internal static extern int StitchingBlenderPrepareRoi(IntPtr blender, int x, int y, int width, int height);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_blender_feed")]
        internal static extern int StitchingBlenderFeed(IntPtr blender, IntPtr image, IntPtr mask, int topLeftX, int topLeftY);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_blender_blend")]
        internal static extern int StitchingBlenderBlend(IntPtr blender, IntPtr destination, IntPtr destinationMask);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_blender_get_sharpness")]
        internal static extern int StitchingBlenderGetSharpness(IntPtr blender, out float sharpness);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_blender_set_sharpness")]
        internal static extern int StitchingBlenderSetSharpness(IntPtr blender, float sharpness);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_blender_get_number_of_bands")]
        internal static extern int StitchingBlenderGetNumberOfBands(IntPtr blender, out int numberOfBands);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_blender_set_number_of_bands")]
        internal static extern int StitchingBlenderSetNumberOfBands(IntPtr blender, int numberOfBands);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_blender_create_weight_maps")]
        internal static extern int StitchingBlenderCreateWeightMaps(IntPtr blender, IntPtr[] masks, int maskCount, int[] cornerX, int[] cornerY, int cornerCount, IntPtr[] weightMaps, int weightMapCount, out StitchingRectNative result);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_normalize_using_weight_map")]
        internal static extern int StitchingNormalizeUsingWeightMap(IntPtr weight, IntPtr source);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_create_weight_map")]
        internal static extern int StitchingCreateWeightMap(IntPtr mask, float sharpness, IntPtr weight);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_create_laplace_pyramid")]
        internal static extern int StitchingCreateLaplacePyramid(IntPtr image, int numberOfLevels, IntPtr[] pyramid, int pyramidCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_create_laplace_pyramid_gpu")]
        internal static extern int StitchingCreateLaplacePyramidGpu(IntPtr image, int numberOfLevels, IntPtr[] pyramid, int pyramidCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_restore_image_from_laplace_pyramid")]
        internal static extern int StitchingRestoreImageFromLaplacePyramid(IntPtr[] pyramid, int pyramidCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_restore_image_from_laplace_pyramid_gpu")]
        internal static extern int StitchingRestoreImageFromLaplacePyramidGpu(IntPtr[] pyramid, int pyramidCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_image_features_create")]
        internal static extern int StitchingImageFeaturesCreate(int imageIndex, int imageWidth, int imageHeight, NativeKeyPoint* keypoints, int keypointCount, IntPtr descriptors, out IntPtr features);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_image_features_release_handle")]
        internal static extern void StitchingImageFeaturesReleaseHandle(IntPtr features);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_image_features_get_image_index")]
        internal static extern int StitchingImageFeaturesGetImageIndex(IntPtr features, out int imageIndex);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_image_features_set_image_index")]
        internal static extern int StitchingImageFeaturesSetImageIndex(IntPtr features, int imageIndex);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_image_features_get_image_size")]
        internal static extern int StitchingImageFeaturesGetImageSize(IntPtr features, out int imageWidth, out int imageHeight);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_image_features_set_image_size")]
        internal static extern int StitchingImageFeaturesSetImageSize(IntPtr features, int imageWidth, int imageHeight);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_image_features_get_keypoints_count")]
        internal static extern int StitchingImageFeaturesGetKeypointsCount(IntPtr features, out int keypointCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_image_features_get_keypoints_fill")]
        internal static extern int StitchingImageFeaturesGetKeypointsFill(IntPtr features, NativeKeyPoint* keypoints, int keypointCapacity, out int keypointCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_image_features_copy_descriptors")]
        internal static extern int StitchingImageFeaturesCopyDescriptors(IntPtr features, IntPtr descriptors);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_compute_image_features")]
        internal static extern int StitchingComputeImageFeatures(int finderKind, IntPtr finderHandle, IntPtr image, IntPtr mask, IntPtr features);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_compute_image_features_batch")]
        internal static extern int StitchingComputeImageFeaturesBatch(int finderKind, IntPtr finderHandle, IntPtr[] images, int imageCount, IntPtr[] masks, int maskCount, IntPtr[] features, int featureCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_matches_info_create")]
        internal static extern int StitchingMatchesInfoCreate(out IntPtr matchesInfo);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_matches_info_release_handle")]
        internal static extern void StitchingMatchesInfoReleaseHandle(IntPtr matchesInfo);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_matches_info_get_metadata")]
        internal static extern int StitchingMatchesInfoGetMetadata(IntPtr matchesInfo, out int sourceImageIndex, out int destinationImageIndex, out int numberOfInliers, out double confidence);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_matches_info_copy_homography")]
        internal static extern int StitchingMatchesInfoCopyHomography(IntPtr matchesInfo, IntPtr homography);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_matches_info_get_matches_count")]
        internal static extern int StitchingMatchesInfoGetMatchesCount(IntPtr matchesInfo, out int matchCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_matches_info_get_matches_fill")]
        internal static extern int StitchingMatchesInfoGetMatchesFill(IntPtr matchesInfo, NativeDMatch* matches, int matchCapacity, out int matchCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_matches_info_get_inliers_count")]
        internal static extern int StitchingMatchesInfoGetInliersCount(IntPtr matchesInfo, out int inlierCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_matches_info_get_inliers_fill")]
        internal static extern int StitchingMatchesInfoGetInliersFill(IntPtr matchesInfo, byte* inliers, int inlierCapacity, out int inlierCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_features_matcher_create_best_of_two_nearest")]
        internal static extern int StitchingFeaturesMatcherCreateBestOfTwoNearest(int tryGpu, float matchConfidence, int numberOfMatchesThreshold1, int numberOfMatchesThreshold2, double matchesConfidenceThreshold, out IntPtr matcher);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_features_matcher_factory_best_of_two_nearest")]
        internal static extern int StitchingFeaturesMatcherFactoryBestOfTwoNearest(int tryGpu, float matchConfidence, int numberOfMatchesThreshold1, int numberOfMatchesThreshold2, double matchesConfidenceThreshold, out IntPtr matcher);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_features_matcher_create_range")]
        internal static extern int StitchingFeaturesMatcherCreateRange(int rangeWidth, int tryGpu, float matchConfidence, int numberOfMatchesThreshold1, int numberOfMatchesThreshold2, out IntPtr matcher);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_features_matcher_create_affine")]
        internal static extern int StitchingFeaturesMatcherCreateAffine(int fullAffine, int tryGpu, float matchConfidence, int numberOfMatchesThreshold1, out IntPtr matcher);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_features_matcher_release_handle")]
        internal static extern void StitchingFeaturesMatcherReleaseHandle(IntPtr matcher);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_features_matcher_match_pair")]
        internal static extern int StitchingFeaturesMatcherMatchPair(IntPtr matcher, IntPtr first, IntPtr second, IntPtr matchesInfo);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_features_matcher_match_batch")]
        internal static extern int StitchingFeaturesMatcherMatchBatch(IntPtr matcher, IntPtr[] features, int featureCount, IntPtr mask, IntPtr[] pairwiseMatches, int pairwiseMatchCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_features_matcher_is_thread_safe")]
        internal static extern int StitchingFeaturesMatcherIsThreadSafe(IntPtr matcher, out int isThreadSafe);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_features_matcher_collect_garbage")]
        internal static extern int StitchingFeaturesMatcherCollectGarbage(IntPtr matcher);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_camera_params_get_k")]
        internal static extern int StitchingCameraParamsGetK(double focal, double aspect, double ppx, double ppy, IntPtr cameraMatrix);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_focals_from_homography")]
        internal static extern int StitchingFocalsFromHomography(IntPtr homography, out double focalX, out double focalY, out int focalXEstimated, out int focalYEstimated);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_calibrate_rotating_camera")]
        internal static extern int StitchingCalibrateRotatingCamera(IntPtr[] homographies, int homographyCount, IntPtr cameraMatrix, out int calibrated);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_estimator_create_homography")]
        internal static extern int StitchingEstimatorCreateHomography(int focalLengthsEstimated, out IntPtr estimator);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_estimator_create_affine")]
        internal static extern int StitchingEstimatorCreateAffine(out IntPtr estimator);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_estimator_create_no_bundle_adjuster")]
        internal static extern int StitchingEstimatorCreateNoBundleAdjuster(out IntPtr estimator);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_estimator_create_bundle_adjuster_reproj")]
        internal static extern int StitchingEstimatorCreateBundleAdjusterReproj(out IntPtr estimator);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_estimator_create_bundle_adjuster_ray")]
        internal static extern int StitchingEstimatorCreateBundleAdjusterRay(out IntPtr estimator);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_estimator_create_bundle_adjuster_affine")]
        internal static extern int StitchingEstimatorCreateBundleAdjusterAffine(out IntPtr estimator);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_estimator_create_bundle_adjuster_affine_partial")]
        internal static extern int StitchingEstimatorCreateBundleAdjusterAffinePartial(out IntPtr estimator);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_estimator_release_handle")]
        internal static extern void StitchingEstimatorReleaseHandle(IntPtr estimator);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_estimator_apply")]
        internal static extern int StitchingEstimatorApply(IntPtr estimator, IntPtr[] features, int featureCount, IntPtr[] pairwiseMatches, int pairwiseMatchCount, StitchingCameraParamsNative[] initialCameras, int initialCameraCount, StitchingCameraParamsNative[] cameras, int cameraCapacity, out int succeeded);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_bundle_adjuster_copy_refinement_mask")]
        internal static extern int StitchingBundleAdjusterCopyRefinementMask(IntPtr estimator, IntPtr refinementMask);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_bundle_adjuster_set_refinement_mask")]
        internal static extern int StitchingBundleAdjusterSetRefinementMask(IntPtr estimator, IntPtr refinementMask);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_bundle_adjuster_get_confidence_threshold")]
        internal static extern int StitchingBundleAdjusterGetConfidenceThreshold(IntPtr estimator, out double confidenceThreshold);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_bundle_adjuster_set_confidence_threshold")]
        internal static extern int StitchingBundleAdjusterSetConfidenceThreshold(IntPtr estimator, double confidenceThreshold);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_bundle_adjuster_get_term_criteria")]
        internal static extern int StitchingBundleAdjusterGetTermCriteria(IntPtr estimator, out int criteriaType, out int maxCount, out double epsilon);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_bundle_adjuster_set_term_criteria")]
        internal static extern int StitchingBundleAdjusterSetTermCriteria(IntPtr estimator, int criteriaType, int maxCount, double epsilon);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_wave_correct")]
        internal static extern int StitchingWaveCorrect(IntPtr[] rotationMatrices, int rotationMatrixCount, int correctionKind);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_matches_graph_as_string")]
        internal static extern int StitchingMatchesGraphAsString(byte[] pathBuffer, int pathByteCount, int[] pathOffsets, int pathCount, int pathOffsetCount, IntPtr[] pairwiseMatches, int pairwiseMatchCount, float confidenceThreshold, out IntPtr result);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitching_leave_biggest_component")]
        internal static extern int StitchingLeaveBiggestComponent(IntPtr[] features, int featureCount, IntPtr[] pairwiseMatches, int pairwiseMatchCount, float confidenceThreshold, IntPtr[] componentFeatures, int componentFeatureCapacity, IntPtr[] componentMatches, int componentMatchCapacity, int[] originalIndices, int originalIndexCapacity, out int componentCount);
    }
}
#endif
