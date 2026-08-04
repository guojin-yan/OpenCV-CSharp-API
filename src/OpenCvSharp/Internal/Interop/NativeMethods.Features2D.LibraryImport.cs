#if NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    internal static unsafe partial class NativeMethods
    {
        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_orb_create")]
        internal static partial int Features2DOrbCreate(int maxFeatures, float scaleFactor, int nLevels, int edgeThreshold, int firstLevel, int wtaK, int scoreType, int patchSize, int fastThreshold, out IntPtr orb);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_orb_release")]
        internal static partial void Features2DOrbRelease(IntPtr orb);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_orb_clear")]
        internal static partial int Features2DOrbClear(IntPtr orb);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_orb_empty")]
        internal static partial int Features2DOrbEmpty(IntPtr orb, out int empty);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_orb_get_max_features")]
        internal static partial int Features2DOrbGetMaxFeatures(IntPtr orb, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_orb_set_max_features")]
        internal static partial int Features2DOrbSetMaxFeatures(IntPtr orb, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_orb_get_scale_factor")]
        internal static partial int Features2DOrbGetScaleFactor(IntPtr orb, out double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_orb_set_scale_factor")]
        internal static partial int Features2DOrbSetScaleFactor(IntPtr orb, double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_orb_get_nlevels")]
        internal static partial int Features2DOrbGetNLevels(IntPtr orb, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_orb_set_nlevels")]
        internal static partial int Features2DOrbSetNLevels(IntPtr orb, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_orb_get_edge_threshold")]
        internal static partial int Features2DOrbGetEdgeThreshold(IntPtr orb, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_orb_set_edge_threshold")]
        internal static partial int Features2DOrbSetEdgeThreshold(IntPtr orb, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_orb_get_first_level")]
        internal static partial int Features2DOrbGetFirstLevel(IntPtr orb, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_orb_set_first_level")]
        internal static partial int Features2DOrbSetFirstLevel(IntPtr orb, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_orb_get_wta_k")]
        internal static partial int Features2DOrbGetWtaK(IntPtr orb, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_orb_set_wta_k")]
        internal static partial int Features2DOrbSetWtaK(IntPtr orb, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_orb_get_score_type")]
        internal static partial int Features2DOrbGetScoreType(IntPtr orb, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_orb_set_score_type")]
        internal static partial int Features2DOrbSetScoreType(IntPtr orb, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_orb_get_patch_size")]
        internal static partial int Features2DOrbGetPatchSize(IntPtr orb, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_orb_set_patch_size")]
        internal static partial int Features2DOrbSetPatchSize(IntPtr orb, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_orb_get_fast_threshold")]
        internal static partial int Features2DOrbGetFastThreshold(IntPtr orb, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_orb_set_fast_threshold")]
        internal static partial int Features2DOrbSetFastThreshold(IntPtr orb, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_orb_descriptor_size")]
        internal static partial int Features2DOrbDescriptorSize(IntPtr orb, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_orb_descriptor_type")]
        internal static partial int Features2DOrbDescriptorType(IntPtr orb, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_orb_default_norm")]
        internal static partial int Features2DOrbDefaultNorm(IntPtr orb, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_orb_detect_count")]
        internal static partial int Features2DOrbDetectCount(IntPtr orb, IntPtr image, IntPtr mask, out int keypointCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_orb_detect_fill")]
        internal static partial int Features2DOrbDetectFill(IntPtr orb, IntPtr image, IntPtr mask, NativeKeyPoint* keypoints, int keypointCapacity, out int keypointCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_orb_compute")]
        internal static partial int Features2DOrbCompute(IntPtr orb, IntPtr image, NativeKeyPoint* keypointsIn, int keypointCount, NativeKeyPoint* keypointsOut, int keypointCapacity, out int writtenKeypointCount, IntPtr descriptors);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_orb_detect_and_compute_count")]
        internal static partial int Features2DOrbDetectAndComputeCount(IntPtr orb, IntPtr image, IntPtr mask, NativeKeyPoint* keypointsIn, int keypointCount, int useProvidedKeypoints, out int outputKeypointCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_orb_detect_and_compute_fill")]
        internal static partial int Features2DOrbDetectAndComputeFill(IntPtr orb, IntPtr image, IntPtr mask, NativeKeyPoint* keypointsIn, int keypointCount, int useProvidedKeypoints, NativeKeyPoint* keypointsOut, int keypointCapacity, out int outputKeypointCount, IntPtr descriptors);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_bf_matcher_create")]
        internal static partial int Features2DBFMatcherCreate(int normType, int crossCheck, out IntPtr matcher);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_bf_matcher_release")]
        internal static partial void Features2DBFMatcherRelease(IntPtr matcher);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_bf_matcher_get_norm_type")]
        internal static partial int Features2DBFMatcherGetNormType(IntPtr matcher, out int normType);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_bf_matcher_get_cross_check")]
        internal static partial int Features2DBFMatcherGetCrossCheck(IntPtr matcher, out int crossCheck);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_bf_matcher_is_mask_supported")]
        internal static partial int Features2DBFMatcherIsMaskSupported(IntPtr matcher, out int supported);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_bf_matcher_empty")]
        internal static partial int Features2DBFMatcherEmpty(IntPtr matcher, out int empty);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_bf_matcher_clear")]
        internal static partial int Features2DBFMatcherClear(IntPtr matcher);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_bf_matcher_train")]
        internal static partial int Features2DBFMatcherTrain(IntPtr matcher);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_bf_matcher_add")]
        internal static partial int Features2DBFMatcherAdd(IntPtr matcher, IntPtr* descriptors, int descriptorCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_descriptor_matcher_create_by_type")]
        internal static partial int Features2DDescriptorMatcherCreateByType(int matcherType, out IntPtr matcher);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_descriptor_matcher_create_by_name")]
        internal static partial int Features2DDescriptorMatcherCreateByName(byte* matcherName, int matcherNameLength, out IntPtr matcher);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_descriptor_matcher_release")]
        internal static partial void Features2DDescriptorMatcherRelease(IntPtr matcher);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_descriptor_matcher_clone")]
        internal static partial int Features2DDescriptorMatcherClone(IntPtr matcher, int emptyTrainData, out IntPtr clone);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_descriptor_matcher_is_mask_supported")]
        internal static partial int Features2DDescriptorMatcherIsMaskSupported(IntPtr matcher, out int supported);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_descriptor_matcher_empty")]
        internal static partial int Features2DDescriptorMatcherEmpty(IntPtr matcher, out int empty);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_descriptor_matcher_clear")]
        internal static partial int Features2DDescriptorMatcherClear(IntPtr matcher);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_descriptor_matcher_train")]
        internal static partial int Features2DDescriptorMatcherTrain(IntPtr matcher);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_descriptor_matcher_add")]
        internal static partial int Features2DDescriptorMatcherAdd(IntPtr matcher, IntPtr* descriptors, int descriptorCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_descriptor_matcher_get_train_descriptors_count")]
        internal static partial int Features2DDescriptorMatcherGetTrainDescriptorsCount(IntPtr matcher, out int descriptorCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_descriptor_matcher_get_train_descriptor_clone")]
        internal static partial int Features2DDescriptorMatcherGetTrainDescriptorClone(IntPtr matcher, int index, out IntPtr descriptor);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_bf_matcher_clone")]
        internal static partial int Features2DBFMatcherClone(IntPtr matcher, int emptyTrainData, out IntPtr clone);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_bf_matcher_get_train_descriptors_count")]
        internal static partial int Features2DBFMatcherGetTrainDescriptorsCount(IntPtr matcher, out int descriptorCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_bf_matcher_get_train_descriptor_clone")]
        internal static partial int Features2DBFMatcherGetTrainDescriptorClone(IntPtr matcher, int index, out IntPtr descriptor);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_descriptor_matcher_match_count")]
        internal static partial int Features2DDescriptorMatcherMatchCount(IntPtr matcher, IntPtr queryDescriptors, IntPtr trainDescriptors, IntPtr mask, out int matchCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_descriptor_matcher_match_fill")]
        internal static partial int Features2DDescriptorMatcherMatchFill(IntPtr matcher, IntPtr queryDescriptors, IntPtr trainDescriptors, IntPtr mask, NativeDMatch* matches, int matchCapacity, out int matchCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_descriptor_matcher_match_train_count")]
        internal static partial int Features2DDescriptorMatcherMatchTrainCount(IntPtr matcher, IntPtr queryDescriptors, IntPtr* masks, int maskCount, out int matchCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_descriptor_matcher_match_train_fill")]
        internal static partial int Features2DDescriptorMatcherMatchTrainFill(IntPtr matcher, IntPtr queryDescriptors, IntPtr* masks, int maskCount, NativeDMatch* matches, int matchCapacity, out int matchCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_descriptor_matcher_knn_match_count")]
        internal static partial int Features2DDescriptorMatcherKnnMatchCount(IntPtr matcher, IntPtr queryDescriptors, IntPtr trainDescriptors, int k, IntPtr mask, int compactResult, out int groupCount, out int totalMatchCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_descriptor_matcher_knn_match_fill")]
        internal static partial int Features2DDescriptorMatcherKnnMatchFill(IntPtr matcher, IntPtr queryDescriptors, IntPtr trainDescriptors, int k, IntPtr mask, int compactResult, int* offsets, int offsetCapacity, NativeDMatch* matches, int matchCapacity, out int groupCount, out int totalMatchCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_descriptor_matcher_knn_match_train_count")]
        internal static partial int Features2DDescriptorMatcherKnnMatchTrainCount(IntPtr matcher, IntPtr queryDescriptors, int k, IntPtr* masks, int maskCount, int compactResult, out int groupCount, out int totalMatchCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_descriptor_matcher_knn_match_train_fill")]
        internal static partial int Features2DDescriptorMatcherKnnMatchTrainFill(IntPtr matcher, IntPtr queryDescriptors, int k, IntPtr* masks, int maskCount, int compactResult, int* offsets, int offsetCapacity, NativeDMatch* matches, int matchCapacity, out int groupCount, out int totalMatchCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_descriptor_matcher_radius_match_count")]
        internal static partial int Features2DDescriptorMatcherRadiusMatchCount(IntPtr matcher, IntPtr queryDescriptors, IntPtr trainDescriptors, float maxDistance, IntPtr mask, int compactResult, out int groupCount, out int totalMatchCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_descriptor_matcher_radius_match_fill")]
        internal static partial int Features2DDescriptorMatcherRadiusMatchFill(IntPtr matcher, IntPtr queryDescriptors, IntPtr trainDescriptors, float maxDistance, IntPtr mask, int compactResult, int* offsets, int offsetCapacity, NativeDMatch* matches, int matchCapacity, out int groupCount, out int totalMatchCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_descriptor_matcher_radius_match_train_count")]
        internal static partial int Features2DDescriptorMatcherRadiusMatchTrainCount(IntPtr matcher, IntPtr queryDescriptors, float maxDistance, IntPtr* masks, int maskCount, int compactResult, out int groupCount, out int totalMatchCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_descriptor_matcher_radius_match_train_fill")]
        internal static partial int Features2DDescriptorMatcherRadiusMatchTrainFill(IntPtr matcher, IntPtr queryDescriptors, float maxDistance, IntPtr* masks, int maskCount, int compactResult, int* offsets, int offsetCapacity, NativeDMatch* matches, int matchCapacity, out int groupCount, out int totalMatchCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_bf_matcher_match_count")]
        internal static partial int Features2DBFMatcherMatchCount(IntPtr matcher, IntPtr queryDescriptors, IntPtr trainDescriptors, IntPtr mask, out int matchCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_bf_matcher_match_fill")]
        internal static partial int Features2DBFMatcherMatchFill(IntPtr matcher, IntPtr queryDescriptors, IntPtr trainDescriptors, IntPtr mask, NativeDMatch* matches, int matchCapacity, out int matchCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_bf_matcher_match_train_count")]
        internal static partial int Features2DBFMatcherMatchTrainCount(IntPtr matcher, IntPtr queryDescriptors, out int matchCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_bf_matcher_match_train_fill")]
        internal static partial int Features2DBFMatcherMatchTrainFill(IntPtr matcher, IntPtr queryDescriptors, NativeDMatch* matches, int matchCapacity, out int matchCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_bf_matcher_match_train_with_masks_count")]
        internal static partial int Features2DBFMatcherMatchTrainWithMasksCount(IntPtr matcher, IntPtr queryDescriptors, IntPtr* masks, int maskCount, out int matchCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_bf_matcher_match_train_with_masks_fill")]
        internal static partial int Features2DBFMatcherMatchTrainWithMasksFill(IntPtr matcher, IntPtr queryDescriptors, IntPtr* masks, int maskCount, NativeDMatch* matches, int matchCapacity, out int matchCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_bf_matcher_knn_match_count")]
        internal static partial int Features2DBFMatcherKnnMatchCount(IntPtr matcher, IntPtr queryDescriptors, IntPtr trainDescriptors, int k, IntPtr mask, int compactResult, out int groupCount, out int totalMatchCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_bf_matcher_knn_match_fill")]
        internal static partial int Features2DBFMatcherKnnMatchFill(IntPtr matcher, IntPtr queryDescriptors, IntPtr trainDescriptors, int k, IntPtr mask, int compactResult, int* offsets, int offsetCapacity, NativeDMatch* matches, int matchCapacity, out int groupCount, out int totalMatchCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_bf_matcher_knn_match_train_count")]
        internal static partial int Features2DBFMatcherKnnMatchTrainCount(IntPtr matcher, IntPtr queryDescriptors, int k, int compactResult, out int groupCount, out int totalMatchCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_bf_matcher_knn_match_train_fill")]
        internal static partial int Features2DBFMatcherKnnMatchTrainFill(IntPtr matcher, IntPtr queryDescriptors, int k, int compactResult, int* offsets, int offsetCapacity, NativeDMatch* matches, int matchCapacity, out int groupCount, out int totalMatchCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_bf_matcher_knn_match_train_with_masks_count")]
        internal static partial int Features2DBFMatcherKnnMatchTrainWithMasksCount(IntPtr matcher, IntPtr queryDescriptors, int k, IntPtr* masks, int maskCount, int compactResult, out int groupCount, out int totalMatchCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_bf_matcher_knn_match_train_with_masks_fill")]
        internal static partial int Features2DBFMatcherKnnMatchTrainWithMasksFill(IntPtr matcher, IntPtr queryDescriptors, int k, IntPtr* masks, int maskCount, int compactResult, int* offsets, int offsetCapacity, NativeDMatch* matches, int matchCapacity, out int groupCount, out int totalMatchCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_bf_matcher_radius_match_count")]
        internal static partial int Features2DBFMatcherRadiusMatchCount(IntPtr matcher, IntPtr queryDescriptors, IntPtr trainDescriptors, float maxDistance, IntPtr mask, int compactResult, out int groupCount, out int totalMatchCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_bf_matcher_radius_match_fill")]
        internal static partial int Features2DBFMatcherRadiusMatchFill(IntPtr matcher, IntPtr queryDescriptors, IntPtr trainDescriptors, float maxDistance, IntPtr mask, int compactResult, int* offsets, int offsetCapacity, NativeDMatch* matches, int matchCapacity, out int groupCount, out int totalMatchCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_bf_matcher_radius_match_train_count")]
        internal static partial int Features2DBFMatcherRadiusMatchTrainCount(IntPtr matcher, IntPtr queryDescriptors, float maxDistance, int compactResult, out int groupCount, out int totalMatchCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_bf_matcher_radius_match_train_fill")]
        internal static partial int Features2DBFMatcherRadiusMatchTrainFill(IntPtr matcher, IntPtr queryDescriptors, float maxDistance, int compactResult, int* offsets, int offsetCapacity, NativeDMatch* matches, int matchCapacity, out int groupCount, out int totalMatchCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_bf_matcher_radius_match_train_with_masks_count")]
        internal static partial int Features2DBFMatcherRadiusMatchTrainWithMasksCount(IntPtr matcher, IntPtr queryDescriptors, float maxDistance, IntPtr* masks, int maskCount, int compactResult, out int groupCount, out int totalMatchCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_bf_matcher_radius_match_train_with_masks_fill")]
        internal static partial int Features2DBFMatcherRadiusMatchTrainWithMasksFill(IntPtr matcher, IntPtr queryDescriptors, float maxDistance, IntPtr* masks, int maskCount, int compactResult, int* offsets, int offsetCapacity, NativeDMatch* matches, int matchCapacity, out int groupCount, out int totalMatchCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_draw_keypoints")]
        internal static partial int Features2DDrawKeypoints(IntPtr image, NativeKeyPoint* keypoints, int keypointCount, IntPtr outImage, double colorV0, double colorV1, double colorV2, double colorV3, int flags);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_draw_matches")]
        internal static partial int Features2DDrawMatches(IntPtr img1, NativeKeyPoint* keypoints1, int keypoint1Count, IntPtr img2, NativeKeyPoint* keypoints2, int keypoint2Count, NativeDMatch* matches, int matchCount, IntPtr outImage, double matchColorV0, double matchColorV1, double matchColorV2, double matchColorV3, double singlePointColorV0, double singlePointColorV1, double singlePointColorV2, double singlePointColorV3, int flags);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_draw_matches_knn")]
        internal static partial int Features2DDrawMatchesKnn(IntPtr img1, NativeKeyPoint* keypoints1, int keypoint1Count, IntPtr img2, NativeKeyPoint* keypoints2, int keypoint2Count, int* offsets, int offsetCount, NativeDMatch* matches, int matchCount, IntPtr outImage, double matchColorV0, double matchColorV1, double matchColorV2, double matchColorV3, double singlePointColorV0, double singlePointColorV1, double singlePointColorV2, double singlePointColorV3, int flags);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_sift_create")]
        internal static partial int Features2DSiftCreate(int nFeatures, int nOctaveLayers, double contrastThreshold, double edgeThreshold, double sigma, int descriptorType, int enablePreciseUpscale, out IntPtr sift);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_sift_release")]
        internal static partial void Features2DSiftRelease(IntPtr sift);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_sift_clear")]
        internal static partial int Features2DSiftClear(IntPtr sift);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_sift_empty")]
        internal static partial int Features2DSiftEmpty(IntPtr sift, out int empty);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_sift_descriptor_size")]
        internal static partial int Features2DSiftDescriptorSize(IntPtr sift, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_sift_descriptor_type")]
        internal static partial int Features2DSiftDescriptorType(IntPtr sift, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_sift_default_norm")]
        internal static partial int Features2DSiftDefaultNorm(IntPtr sift, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_sift_get_nfeatures")]
        internal static partial int Features2DSiftGetNFeatures(IntPtr sift, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_sift_set_nfeatures")]
        internal static partial int Features2DSiftSetNFeatures(IntPtr sift, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_sift_get_n_octave_layers")]
        internal static partial int Features2DSiftGetNOctaveLayers(IntPtr sift, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_sift_set_n_octave_layers")]
        internal static partial int Features2DSiftSetNOctaveLayers(IntPtr sift, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_sift_get_contrast_threshold")]
        internal static partial int Features2DSiftGetContrastThreshold(IntPtr sift, out double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_sift_set_contrast_threshold")]
        internal static partial int Features2DSiftSetContrastThreshold(IntPtr sift, double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_sift_get_edge_threshold")]
        internal static partial int Features2DSiftGetEdgeThreshold(IntPtr sift, out double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_sift_set_edge_threshold")]
        internal static partial int Features2DSiftSetEdgeThreshold(IntPtr sift, double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_sift_get_sigma")]
        internal static partial int Features2DSiftGetSigma(IntPtr sift, out double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_sift_set_sigma")]
        internal static partial int Features2DSiftSetSigma(IntPtr sift, double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_sift_detect_count")]
        internal static partial int Features2DSiftDetectCount(IntPtr sift, IntPtr image, IntPtr mask, out int keypointCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_sift_detect_fill")]
        internal static partial int Features2DSiftDetectFill(IntPtr sift, IntPtr image, IntPtr mask, NativeKeyPoint* keypoints, int keypointCapacity, out int keypointCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_sift_compute")]
        internal static partial int Features2DSiftCompute(IntPtr sift, IntPtr image, NativeKeyPoint* keypointsIn, int keypointCount, NativeKeyPoint* keypointsOut, int keypointCapacity, out int writtenKeypointCount, IntPtr descriptors);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_sift_detect_and_compute_count")]
        internal static partial int Features2DSiftDetectAndComputeCount(IntPtr sift, IntPtr image, IntPtr mask, NativeKeyPoint* keypointsIn, int keypointCount, int useProvidedKeypoints, out int outputKeypointCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_sift_detect_and_compute_fill")]
        internal static partial int Features2DSiftDetectAndComputeFill(IntPtr sift, IntPtr image, IntPtr mask, NativeKeyPoint* keypointsIn, int keypointCount, int useProvidedKeypoints, NativeKeyPoint* keypointsOut, int keypointCapacity, out int outputKeypointCount, IntPtr descriptors);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_fast_create")]
        internal static partial int Features2DFastCreate(int threshold, int nonmaxSuppression, int type, out IntPtr fast);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_fast_release")]
        internal static partial void Features2DFastRelease(IntPtr fast);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_fast_clear")]
        internal static partial int Features2DFastClear(IntPtr fast);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_fast_empty")]
        internal static partial int Features2DFastEmpty(IntPtr fast, out int empty);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_fast_descriptor_size")]
        internal static partial int Features2DFastDescriptorSize(IntPtr fast, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_fast_descriptor_type")]
        internal static partial int Features2DFastDescriptorType(IntPtr fast, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_fast_default_norm")]
        internal static partial int Features2DFastDefaultNorm(IntPtr fast, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_fast_get_threshold")]
        internal static partial int Features2DFastGetThreshold(IntPtr fast, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_fast_set_threshold")]
        internal static partial int Features2DFastSetThreshold(IntPtr fast, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_fast_get_nonmax_suppression")]
        internal static partial int Features2DFastGetNonmaxSuppression(IntPtr fast, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_fast_set_nonmax_suppression")]
        internal static partial int Features2DFastSetNonmaxSuppression(IntPtr fast, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_fast_get_type")]
        internal static partial int Features2DFastGetType(IntPtr fast, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_fast_set_type")]
        internal static partial int Features2DFastSetType(IntPtr fast, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_fast_detect_count")]
        internal static partial int Features2DFastDetectCount(IntPtr fast, IntPtr image, IntPtr mask, out int keypointCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_fast_detect_fill")]
        internal static partial int Features2DFastDetectFill(IntPtr fast, IntPtr image, IntPtr mask, NativeKeyPoint* keypoints, int keypointCapacity, out int keypointCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_gftt_create")]
        internal static partial int Features2DGfttCreate(int maxCorners, double qualityLevel, double minDistance, int blockSize, int gradientSize, int useHarrisDetector, double k, out IntPtr gftt);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_gftt_release")]
        internal static partial void Features2DGfttRelease(IntPtr gftt);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_gftt_clear")]
        internal static partial int Features2DGfttClear(IntPtr gftt);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_gftt_empty")]
        internal static partial int Features2DGfttEmpty(IntPtr gftt, out int empty);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_gftt_descriptor_size")]
        internal static partial int Features2DGfttDescriptorSize(IntPtr gftt, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_gftt_descriptor_type")]
        internal static partial int Features2DGfttDescriptorType(IntPtr gftt, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_gftt_default_norm")]
        internal static partial int Features2DGfttDefaultNorm(IntPtr gftt, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_gftt_get_max_features")]
        internal static partial int Features2DGfttGetMaxFeatures(IntPtr gftt, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_gftt_set_max_features")]
        internal static partial int Features2DGfttSetMaxFeatures(IntPtr gftt, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_gftt_get_quality_level")]
        internal static partial int Features2DGfttGetQualityLevel(IntPtr gftt, out double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_gftt_set_quality_level")]
        internal static partial int Features2DGfttSetQualityLevel(IntPtr gftt, double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_gftt_get_min_distance")]
        internal static partial int Features2DGfttGetMinDistance(IntPtr gftt, out double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_gftt_set_min_distance")]
        internal static partial int Features2DGfttSetMinDistance(IntPtr gftt, double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_gftt_get_block_size")]
        internal static partial int Features2DGfttGetBlockSize(IntPtr gftt, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_gftt_set_block_size")]
        internal static partial int Features2DGfttSetBlockSize(IntPtr gftt, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_gftt_get_gradient_size")]
        internal static partial int Features2DGfttGetGradientSize(IntPtr gftt, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_gftt_set_gradient_size")]
        internal static partial int Features2DGfttSetGradientSize(IntPtr gftt, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_gftt_get_harris_detector")]
        internal static partial int Features2DGfttGetHarrisDetector(IntPtr gftt, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_gftt_set_harris_detector")]
        internal static partial int Features2DGfttSetHarrisDetector(IntPtr gftt, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_gftt_get_k")]
        internal static partial int Features2DGfttGetK(IntPtr gftt, out double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_gftt_set_k")]
        internal static partial int Features2DGfttSetK(IntPtr gftt, double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_gftt_detect_count")]
        internal static partial int Features2DGfttDetectCount(IntPtr gftt, IntPtr image, IntPtr mask, out int keypointCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_gftt_detect_fill")]
        internal static partial int Features2DGfttDetectFill(IntPtr gftt, IntPtr image, IntPtr mask, NativeKeyPoint* keypoints, int keypointCapacity, out int keypointCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_mser_create")]
        internal static partial int Features2DMserCreate(int delta, int minArea, int maxArea, double maxVariation, double minDiversity, int maxEvolution, double areaThreshold, double minMargin, int edgeBlurSize, out IntPtr mser);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_mser_release")]
        internal static partial void Features2DMserRelease(IntPtr mser);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_mser_clear")]
        internal static partial int Features2DMserClear(IntPtr mser);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_mser_empty")]
        internal static partial int Features2DMserEmpty(IntPtr mser, out int empty);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_mser_descriptor_size")]
        internal static partial int Features2DMserDescriptorSize(IntPtr mser, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_mser_descriptor_type")]
        internal static partial int Features2DMserDescriptorType(IntPtr mser, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_mser_default_norm")]
        internal static partial int Features2DMserDefaultNorm(IntPtr mser, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_mser_detect_count")]
        internal static partial int Features2DMserDetectCount(IntPtr mser, IntPtr image, IntPtr mask, out int keypointCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_mser_detect_fill")]
        internal static partial int Features2DMserDetectFill(IntPtr mser, IntPtr image, IntPtr mask, NativeKeyPoint* keypoints, int keypointCapacity, out int keypointCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_mser_get_delta")]
        internal static partial int Features2DMserGetDelta(IntPtr mser, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_mser_set_delta")]
        internal static partial int Features2DMserSetDelta(IntPtr mser, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_mser_get_min_area")]
        internal static partial int Features2DMserGetMinArea(IntPtr mser, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_mser_set_min_area")]
        internal static partial int Features2DMserSetMinArea(IntPtr mser, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_mser_get_max_area")]
        internal static partial int Features2DMserGetMaxArea(IntPtr mser, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_mser_set_max_area")]
        internal static partial int Features2DMserSetMaxArea(IntPtr mser, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_mser_get_max_variation")]
        internal static partial int Features2DMserGetMaxVariation(IntPtr mser, out double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_mser_set_max_variation")]
        internal static partial int Features2DMserSetMaxVariation(IntPtr mser, double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_mser_get_min_diversity")]
        internal static partial int Features2DMserGetMinDiversity(IntPtr mser, out double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_mser_set_min_diversity")]
        internal static partial int Features2DMserSetMinDiversity(IntPtr mser, double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_mser_get_max_evolution")]
        internal static partial int Features2DMserGetMaxEvolution(IntPtr mser, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_mser_set_max_evolution")]
        internal static partial int Features2DMserSetMaxEvolution(IntPtr mser, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_mser_get_area_threshold")]
        internal static partial int Features2DMserGetAreaThreshold(IntPtr mser, out double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_mser_set_area_threshold")]
        internal static partial int Features2DMserSetAreaThreshold(IntPtr mser, double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_mser_get_min_margin")]
        internal static partial int Features2DMserGetMinMargin(IntPtr mser, out double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_mser_set_min_margin")]
        internal static partial int Features2DMserSetMinMargin(IntPtr mser, double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_mser_get_edge_blur_size")]
        internal static partial int Features2DMserGetEdgeBlurSize(IntPtr mser, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_mser_set_edge_blur_size")]
        internal static partial int Features2DMserSetEdgeBlurSize(IntPtr mser, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_mser_get_pass2_only")]
        internal static partial int Features2DMserGetPass2Only(IntPtr mser, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_mser_set_pass2_only")]
        internal static partial int Features2DMserSetPass2Only(IntPtr mser, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_mser_detect_regions_count")]
        internal static partial int Features2DMserDetectRegionsCount(IntPtr mser, IntPtr image, out int regionCount, out int totalPointCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_mser_detect_regions_fill")]
        internal static partial int Features2DMserDetectRegionsFill(IntPtr mser, IntPtr image, int* offsets, int offsetCapacity, NativePoint* points, int pointCapacity, NativeRect* bboxes, int bboxCapacity, out int regionCount, out int totalPointCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_simple_blob_create_default")]
        internal static partial int Features2DSimpleBlobCreateDefault(out IntPtr detector);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_simple_blob_create")]
        internal static partial int Features2DSimpleBlobCreate(ref NativeSimpleBlobParams parameters, out IntPtr detector);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_simple_blob_release")]
        internal static partial void Features2DSimpleBlobRelease(IntPtr detector);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_simple_blob_clear")]
        internal static partial int Features2DSimpleBlobClear(IntPtr detector);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_simple_blob_empty")]
        internal static partial int Features2DSimpleBlobEmpty(IntPtr detector, out int empty);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_simple_blob_descriptor_size")]
        internal static partial int Features2DSimpleBlobDescriptorSize(IntPtr detector, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_simple_blob_descriptor_type")]
        internal static partial int Features2DSimpleBlobDescriptorType(IntPtr detector, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_simple_blob_default_norm")]
        internal static partial int Features2DSimpleBlobDefaultNorm(IntPtr detector, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_simple_blob_detect_count")]
        internal static partial int Features2DSimpleBlobDetectCount(IntPtr detector, IntPtr image, IntPtr mask, out int keypointCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_simple_blob_detect_fill")]
        internal static partial int Features2DSimpleBlobDetectFill(IntPtr detector, IntPtr image, IntPtr mask, NativeKeyPoint* keypoints, int keypointCapacity, out int keypointCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_simple_blob_get_params")]
        internal static partial int Features2DSimpleBlobGetParams(IntPtr detector, out NativeSimpleBlobParams parameters);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_simple_blob_set_params")]
        internal static partial int Features2DSimpleBlobSetParams(IntPtr detector, ref NativeSimpleBlobParams parameters);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_simple_blob_get_blob_contours_count")]
        internal static partial int Features2DSimpleBlobGetBlobContoursCount(IntPtr detector, out int contourCount, out int totalPointCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_simple_blob_get_blob_contours_fill")]
        internal static partial int Features2DSimpleBlobGetBlobContoursFill(IntPtr detector, int* offsets, int offsetCapacity, NativePoint* points, int pointCapacity, out int contourCount, out int totalPointCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_orb_default_name_length")]
        internal static partial int Features2DOrbDefaultNameLength(IntPtr orb, out int length);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_orb_default_name_fill")]
        internal static partial int Features2DOrbDefaultNameFill(IntPtr orb, byte* buffer, int bufferCapacity, out int written);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_sift_default_name_length")]
        internal static partial int Features2DSiftDefaultNameLength(IntPtr sift, out int length);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_sift_default_name_fill")]
        internal static partial int Features2DSiftDefaultNameFill(IntPtr sift, byte* buffer, int bufferCapacity, out int written);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_fast_default_name_length")]
        internal static partial int Features2DFastDefaultNameLength(IntPtr fast, out int length);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_fast_default_name_fill")]
        internal static partial int Features2DFastDefaultNameFill(IntPtr fast, byte* buffer, int bufferCapacity, out int written);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_gftt_default_name_length")]
        internal static partial int Features2DGfttDefaultNameLength(IntPtr gftt, out int length);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_gftt_default_name_fill")]
        internal static partial int Features2DGfttDefaultNameFill(IntPtr gftt, byte* buffer, int bufferCapacity, out int written);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_mser_default_name_length")]
        internal static partial int Features2DMserDefaultNameLength(IntPtr mser, out int length);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_mser_default_name_fill")]
        internal static partial int Features2DMserDefaultNameFill(IntPtr mser, byte* buffer, int bufferCapacity, out int written);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_simple_blob_default_name_length")]
        internal static partial int Features2DSimpleBlobDefaultNameLength(IntPtr detector, out int length);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_simple_blob_default_name_fill")]
        internal static partial int Features2DSimpleBlobDefaultNameFill(IntPtr detector, byte* buffer, int bufferCapacity, out int written);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_affine_create_from_orb")]
        internal static partial int Features2DAffineCreateFromOrb(IntPtr backend, int maxTilt, int minTilt, float tiltStep, float rotateStepBase, out IntPtr affine);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_affine_create_from_sift")]
        internal static partial int Features2DAffineCreateFromSift(IntPtr backend, int maxTilt, int minTilt, float tiltStep, float rotateStepBase, out IntPtr affine);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_affine_create_from_fast")]
        internal static partial int Features2DAffineCreateFromFast(IntPtr backend, int maxTilt, int minTilt, float tiltStep, float rotateStepBase, out IntPtr affine);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_affine_create_from_gftt")]
        internal static partial int Features2DAffineCreateFromGftt(IntPtr backend, int maxTilt, int minTilt, float tiltStep, float rotateStepBase, out IntPtr affine);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_affine_create_from_mser")]
        internal static partial int Features2DAffineCreateFromMser(IntPtr backend, int maxTilt, int minTilt, float tiltStep, float rotateStepBase, out IntPtr affine);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_affine_create_from_simple_blob")]
        internal static partial int Features2DAffineCreateFromSimpleBlob(IntPtr backend, int maxTilt, int minTilt, float tiltStep, float rotateStepBase, out IntPtr affine);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_affine_release")]
        internal static partial void Features2DAffineRelease(IntPtr affine);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_affine_clear")]
        internal static partial int Features2DAffineClear(IntPtr affine);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_affine_empty")]
        internal static partial int Features2DAffineEmpty(IntPtr affine, out int empty);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_affine_descriptor_size")]
        internal static partial int Features2DAffineDescriptorSize(IntPtr affine, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_affine_descriptor_type")]
        internal static partial int Features2DAffineDescriptorType(IntPtr affine, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_affine_default_norm")]
        internal static partial int Features2DAffineDefaultNorm(IntPtr affine, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_affine_default_name_length")]
        internal static partial int Features2DAffineDefaultNameLength(IntPtr affine, out int length);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_affine_default_name_fill")]
        internal static partial int Features2DAffineDefaultNameFill(IntPtr affine, byte* buffer, int bufferCapacity, out int written);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_affine_detect_count")]
        internal static partial int Features2DAffineDetectCount(IntPtr affine, IntPtr image, IntPtr mask, out int keypointCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_affine_detect_fill")]
        internal static partial int Features2DAffineDetectFill(IntPtr affine, IntPtr image, IntPtr mask, NativeKeyPoint* keypoints, int keypointCapacity, out int keypointCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_affine_set_view_params")]
        internal static partial int Features2DAffineSetViewParams(IntPtr affine, float* tilts, int tiltCount, float* rolls, int rollCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_affine_get_view_params_count")]
        internal static partial int Features2DAffineGetViewParamsCount(IntPtr affine, out int tiltCount, out int rollCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_affine_get_view_params_fill")]
        internal static partial int Features2DAffineGetViewParamsFill(IntPtr affine, float* tilts, int tiltCapacity, float* rolls, int rollCapacity, out int tiltCount, out int rollCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_flann_matcher_create")]
        internal static partial int Features2DFlannMatcherCreate(out IntPtr matcher);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_flann_matcher_release")]
        internal static partial void Features2DFlannMatcherRelease(IntPtr matcher);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_flann_matcher_clone")]
        internal static partial int Features2DFlannMatcherClone(IntPtr matcher, int emptyTrainData, out IntPtr clone);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_flann_matcher_is_mask_supported")]
        internal static partial int Features2DFlannMatcherIsMaskSupported(IntPtr matcher, out int supported);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_flann_matcher_empty")]
        internal static partial int Features2DFlannMatcherEmpty(IntPtr matcher, out int empty);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_flann_matcher_clear")]
        internal static partial int Features2DFlannMatcherClear(IntPtr matcher);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_flann_matcher_train")]
        internal static partial int Features2DFlannMatcherTrain(IntPtr matcher);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_flann_matcher_add")]
        internal static partial int Features2DFlannMatcherAdd(IntPtr matcher, IntPtr* descriptors, int descriptorCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_flann_matcher_get_train_descriptors_count")]
        internal static partial int Features2DFlannMatcherGetTrainDescriptorsCount(IntPtr matcher, out int descriptorCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_flann_matcher_get_train_descriptor_clone")]
        internal static partial int Features2DFlannMatcherGetTrainDescriptorClone(IntPtr matcher, int index, out IntPtr descriptor);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_flann_matcher_match_count")]
        internal static partial int Features2DFlannMatcherMatchCount(IntPtr matcher, IntPtr queryDescriptors, IntPtr trainDescriptors, out int matchCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_flann_matcher_match_fill")]
        internal static partial int Features2DFlannMatcherMatchFill(IntPtr matcher, IntPtr queryDescriptors, IntPtr trainDescriptors, NativeDMatch* matches, int matchCapacity, out int matchCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_flann_matcher_match_train_count")]
        internal static partial int Features2DFlannMatcherMatchTrainCount(IntPtr matcher, IntPtr queryDescriptors, out int matchCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_flann_matcher_match_train_fill")]
        internal static partial int Features2DFlannMatcherMatchTrainFill(IntPtr matcher, IntPtr queryDescriptors, NativeDMatch* matches, int matchCapacity, out int matchCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_flann_matcher_knn_match_count")]
        internal static partial int Features2DFlannMatcherKnnMatchCount(IntPtr matcher, IntPtr queryDescriptors, IntPtr trainDescriptors, int k, int compactResult, out int groupCount, out int totalMatchCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_flann_matcher_knn_match_fill")]
        internal static partial int Features2DFlannMatcherKnnMatchFill(IntPtr matcher, IntPtr queryDescriptors, IntPtr trainDescriptors, int k, int compactResult, int* offsets, int offsetCapacity, NativeDMatch* matches, int matchCapacity, out int groupCount, out int totalMatchCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_flann_matcher_knn_match_train_count")]
        internal static partial int Features2DFlannMatcherKnnMatchTrainCount(IntPtr matcher, IntPtr queryDescriptors, int k, int compactResult, out int groupCount, out int totalMatchCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_flann_matcher_knn_match_train_fill")]
        internal static partial int Features2DFlannMatcherKnnMatchTrainFill(IntPtr matcher, IntPtr queryDescriptors, int k, int compactResult, int* offsets, int offsetCapacity, NativeDMatch* matches, int matchCapacity, out int groupCount, out int totalMatchCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_flann_matcher_radius_match_count")]
        internal static partial int Features2DFlannMatcherRadiusMatchCount(IntPtr matcher, IntPtr queryDescriptors, IntPtr trainDescriptors, float maxDistance, int compactResult, out int groupCount, out int totalMatchCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_flann_matcher_radius_match_fill")]
        internal static partial int Features2DFlannMatcherRadiusMatchFill(IntPtr matcher, IntPtr queryDescriptors, IntPtr trainDescriptors, float maxDistance, int compactResult, int* offsets, int offsetCapacity, NativeDMatch* matches, int matchCapacity, out int groupCount, out int totalMatchCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_flann_matcher_radius_match_train_count")]
        internal static partial int Features2DFlannMatcherRadiusMatchTrainCount(IntPtr matcher, IntPtr queryDescriptors, float maxDistance, int compactResult, out int groupCount, out int totalMatchCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_flann_matcher_radius_match_train_fill")]
        internal static partial int Features2DFlannMatcherRadiusMatchTrainFill(IntPtr matcher, IntPtr queryDescriptors, float maxDistance, int compactResult, int* offsets, int offsetCapacity, NativeDMatch* matches, int matchCapacity, out int groupCount, out int totalMatchCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_ann_index_create")]
        internal static partial int Features2DAnnIndexCreate(int dimension, int distance, out IntPtr index);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_ann_index_release")]
        internal static partial void Features2DAnnIndexRelease(IntPtr index);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_ann_index_add_items")]
        internal static partial int Features2DAnnIndexAddItems(IntPtr index, IntPtr features);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_ann_index_build")]
        internal static partial int Features2DAnnIndexBuild(IntPtr index, int trees);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_ann_index_knn_search")]
        internal static partial int Features2DAnnIndexKnnSearch(IntPtr index, IntPtr query, IntPtr indices, IntPtr distances, int knn, int searchK);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_ann_index_save")]
        internal static partial int Features2DAnnIndexSave(IntPtr index, byte[] filenameUtf8, int filenameByteLength, int prefault);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_ann_index_load")]
        internal static partial int Features2DAnnIndexLoad(IntPtr index, byte[] filenameUtf8, int filenameByteLength, int prefault);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_ann_index_get_tree_number")]
        internal static partial int Features2DAnnIndexGetTreeNumber(IntPtr index, out int treeNumber);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_ann_index_get_item_number")]
        internal static partial int Features2DAnnIndexGetItemNumber(IntPtr index, out int itemNumber);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_ann_index_set_on_disk_build")]
        internal static partial int Features2DAnnIndexSetOnDiskBuild(IntPtr index, byte[] filenameUtf8, int filenameByteLength, out int enabled);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_ann_index_set_seed")]
        internal static partial int Features2DAnnIndexSetSeed(IntPtr index, int seed);
    }
}
#endif
