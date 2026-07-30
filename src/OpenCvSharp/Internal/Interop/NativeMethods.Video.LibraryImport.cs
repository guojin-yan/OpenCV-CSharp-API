#if NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal static unsafe partial class NativeMethods
    {
        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_calc_optical_flow_pyr_lk")]
        internal static partial int VideoCalcOpticalFlowPyrLK(IntPtr prevImg, IntPtr nextImg, VideoPoint2fNative* prevPoints, int pointCount, VideoPoint2fNative* initialNextPoints, int useInitialFlow, VideoPoint2fNative* nextPoints, byte* status, float* err, int winWidth, int winHeight, int maxLevel, int criteriaType, int criteriaMaxCount, double criteriaEpsilon, int flags, double minEigThreshold);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_calc_optical_flow_farneback")]
        internal static partial int VideoCalcOpticalFlowFarneback(IntPtr prev, IntPtr next, IntPtr flow, double pyrScale, int levels, int winsize, int iterations, int polyN, double polySigma, int flags);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_read_optical_flow")]
        internal static partial int VideoReadOpticalFlow(byte[] path, out IntPtr flow);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_write_optical_flow")]
        internal static partial int VideoWriteOpticalFlow(byte[] path, IntPtr flow, out int result);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_build_optical_flow_pyramid_count")]
        internal static partial int VideoBuildOpticalFlowPyramidCount(IntPtr image, int winWidth, int winHeight, int maxLevel, int withDerivatives, int pyrBorder, int derivBorder, int tryReuseInputImage, out int levelCount, out int matCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_build_optical_flow_pyramid_fill")]
        internal static partial int VideoBuildOpticalFlowPyramidFill(IntPtr image, int winWidth, int winHeight, int maxLevel, int withDerivatives, int pyrBorder, int derivBorder, int tryReuseInputImage, IntPtr[] pyramid, int pyramidCapacity, out int levelCount, out int matCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_mean_shift")]
        internal static partial int VideoMeanShift(IntPtr probImage, ref VideoRectNative window, int criteriaType, int criteriaMaxCount, double criteriaEpsilon, out int iterations);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_cam_shift")]
        internal static partial int VideoCamShift(IntPtr probImage, ref VideoRectNative window, int criteriaType, int criteriaMaxCount, double criteriaEpsilon, out VideoRotatedRectNative box);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_kalman_filter_create")]
        internal static partial int KalmanFilterCreate(int dynamParams, int measureParams, int controlParams, int type, out IntPtr filter);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_kalman_filter_release_handle")]
        internal static partial void KalmanFilterReleaseHandle(IntPtr filter);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_kalman_filter_init")]
        internal static partial int KalmanFilterInit(IntPtr filter, int dynamParams, int measureParams, int controlParams, int type);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_kalman_filter_predict")]
        internal static partial int KalmanFilterPredict(IntPtr filter, IntPtr control, IntPtr prediction);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_kalman_filter_correct")]
        internal static partial int KalmanFilterCorrect(IntPtr filter, IntPtr measurement, IntPtr corrected);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_kalman_filter_get_matrix")]
        internal static partial int KalmanFilterGetMatrix(IntPtr filter, int matrixId, IntPtr value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_kalman_filter_set_matrix")]
        internal static partial int KalmanFilterSetMatrix(IntPtr filter, int matrixId, IntPtr value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_background_subtractor_release_handle")]
        internal static partial void BackgroundSubtractorReleaseHandle(IntPtr subtractor);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_background_subtractor_apply")]
        internal static partial int BackgroundSubtractorApply(IntPtr subtractor, IntPtr image, IntPtr fgmask, double learningRate);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_background_subtractor_apply_with_known_foreground")]
        internal static partial int BackgroundSubtractorApplyWithKnownForeground(IntPtr subtractor, IntPtr image, IntPtr knownForegroundMask, IntPtr fgmask, double learningRate);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_background_subtractor_get_background_image")]
        internal static partial int BackgroundSubtractorGetBackgroundImage(IntPtr subtractor, IntPtr backgroundImage);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_background_subtractor_mog2_create")]
        internal static partial int BackgroundSubtractorMOG2Create(int history, double varThreshold, int detectShadows, out IntPtr subtractor);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_background_subtractor_mog2_get_history")]
        internal static partial int BackgroundSubtractorMOG2GetHistory(IntPtr subtractor, out int history);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_background_subtractor_mog2_set_history")]
        internal static partial int BackgroundSubtractorMOG2SetHistory(IntPtr subtractor, int history);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_background_subtractor_mog2_get_n_mixtures")]
        internal static partial int BackgroundSubtractorMOG2GetNMixtures(IntPtr subtractor, out int nMixtures);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_background_subtractor_mog2_set_n_mixtures")]
        internal static partial int BackgroundSubtractorMOG2SetNMixtures(IntPtr subtractor, int nMixtures);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_background_subtractor_mog2_get_detect_shadows")]
        internal static partial int BackgroundSubtractorMOG2GetDetectShadows(IntPtr subtractor, out int detectShadows);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_background_subtractor_mog2_set_detect_shadows")]
        internal static partial int BackgroundSubtractorMOG2SetDetectShadows(IntPtr subtractor, int detectShadows);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_background_subtractor_mog2_get_int_property")]
        internal static partial int BackgroundSubtractorMOG2GetIntProperty(IntPtr subtractor, int propertyId, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_background_subtractor_mog2_set_int_property")]
        internal static partial int BackgroundSubtractorMOG2SetIntProperty(IntPtr subtractor, int propertyId, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_background_subtractor_mog2_get_double_property")]
        internal static partial int BackgroundSubtractorMOG2GetDoubleProperty(IntPtr subtractor, int propertyId, out double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_background_subtractor_mog2_set_double_property")]
        internal static partial int BackgroundSubtractorMOG2SetDoubleProperty(IntPtr subtractor, int propertyId, double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_background_subtractor_knn_create")]
        internal static partial int BackgroundSubtractorKNNCreate(int history, double dist2Threshold, int detectShadows, out IntPtr subtractor);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_background_subtractor_knn_get_history")]
        internal static partial int BackgroundSubtractorKNNGetHistory(IntPtr subtractor, out int history);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_background_subtractor_knn_set_history")]
        internal static partial int BackgroundSubtractorKNNSetHistory(IntPtr subtractor, int history);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_background_subtractor_knn_get_n_samples")]
        internal static partial int BackgroundSubtractorKNNGetNSamples(IntPtr subtractor, out int nSamples);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_background_subtractor_knn_set_n_samples")]
        internal static partial int BackgroundSubtractorKNNSetNSamples(IntPtr subtractor, int nSamples);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_background_subtractor_knn_get_detect_shadows")]
        internal static partial int BackgroundSubtractorKNNGetDetectShadows(IntPtr subtractor, out int detectShadows);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_background_subtractor_knn_set_detect_shadows")]
        internal static partial int BackgroundSubtractorKNNSetDetectShadows(IntPtr subtractor, int detectShadows);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_background_subtractor_knn_get_int_property")]
        internal static partial int BackgroundSubtractorKNNGetIntProperty(IntPtr subtractor, int propertyId, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_background_subtractor_knn_set_int_property")]
        internal static partial int BackgroundSubtractorKNNSetIntProperty(IntPtr subtractor, int propertyId, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_background_subtractor_knn_get_double_property")]
        internal static partial int BackgroundSubtractorKNNGetDoubleProperty(IntPtr subtractor, int propertyId, out double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_background_subtractor_knn_set_double_property")]
        internal static partial int BackgroundSubtractorKNNSetDoubleProperty(IntPtr subtractor, int propertyId, double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dense_optical_flow_release_handle")]
        internal static partial void DenseOpticalFlowReleaseHandle(IntPtr opticalFlow);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dense_optical_flow_calc")]
        internal static partial int DenseOpticalFlowCalc(IntPtr opticalFlow, IntPtr first, IntPtr second, IntPtr flow);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dense_optical_flow_collect_garbage")]
        internal static partial int DenseOpticalFlowCollectGarbage(IntPtr opticalFlow);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_sparse_optical_flow_release_handle")]
        internal static partial void SparseOpticalFlowReleaseHandle(IntPtr opticalFlow);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_sparse_optical_flow_calc")]
        internal static partial int SparseOpticalFlowCalc(IntPtr opticalFlow, IntPtr previousImage, IntPtr nextImage, VideoPoint2fNative* previousPoints, int pointCount, VideoPoint2fNative* nextPoints, byte* status, float* error);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_farneback_optical_flow_create")]
        internal static partial int FarnebackOpticalFlowCreate(int numLevels, double pyramidScale, int fastPyramids, int windowSize, int numIterations, int polynomialNeighborhood, double polynomialSigma, int flags, out IntPtr opticalFlow);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_farneback_optical_flow_get_int_property")]
        internal static partial int FarnebackOpticalFlowGetIntProperty(IntPtr opticalFlow, int propertyId, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_farneback_optical_flow_set_int_property")]
        internal static partial int FarnebackOpticalFlowSetIntProperty(IntPtr opticalFlow, int propertyId, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_farneback_optical_flow_get_double_property")]
        internal static partial int FarnebackOpticalFlowGetDoubleProperty(IntPtr opticalFlow, int propertyId, out double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_farneback_optical_flow_set_double_property")]
        internal static partial int FarnebackOpticalFlowSetDoubleProperty(IntPtr opticalFlow, int propertyId, double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_farneback_optical_flow_get_bool_property")]
        internal static partial int FarnebackOpticalFlowGetBoolProperty(IntPtr opticalFlow, int propertyId, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_farneback_optical_flow_set_bool_property")]
        internal static partial int FarnebackOpticalFlowSetBoolProperty(IntPtr opticalFlow, int propertyId, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_variational_refinement_create")]
        internal static partial int VariationalRefinementCreate(out IntPtr opticalFlow);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_variational_refinement_calc_uv")]
        internal static partial int VariationalRefinementCalcUV(IntPtr opticalFlow, IntPtr first, IntPtr second, IntPtr flowU, IntPtr flowV);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_variational_refinement_get_int_property")]
        internal static partial int VariationalRefinementGetIntProperty(IntPtr opticalFlow, int propertyId, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_variational_refinement_set_int_property")]
        internal static partial int VariationalRefinementSetIntProperty(IntPtr opticalFlow, int propertyId, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_variational_refinement_get_float_property")]
        internal static partial int VariationalRefinementGetFloatProperty(IntPtr opticalFlow, int propertyId, out float value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_variational_refinement_set_float_property")]
        internal static partial int VariationalRefinementSetFloatProperty(IntPtr opticalFlow, int propertyId, float value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dis_optical_flow_create")]
        internal static partial int DisOpticalFlowCreate(int preset, out IntPtr opticalFlow);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dis_optical_flow_get_int_property")]
        internal static partial int DisOpticalFlowGetIntProperty(IntPtr opticalFlow, int propertyId, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dis_optical_flow_set_int_property")]
        internal static partial int DisOpticalFlowSetIntProperty(IntPtr opticalFlow, int propertyId, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dis_optical_flow_get_float_property")]
        internal static partial int DisOpticalFlowGetFloatProperty(IntPtr opticalFlow, int propertyId, out float value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dis_optical_flow_set_float_property")]
        internal static partial int DisOpticalFlowSetFloatProperty(IntPtr opticalFlow, int propertyId, float value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dis_optical_flow_get_bool_property")]
        internal static partial int DisOpticalFlowGetBoolProperty(IntPtr opticalFlow, int propertyId, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dis_optical_flow_set_bool_property")]
        internal static partial int DisOpticalFlowSetBoolProperty(IntPtr opticalFlow, int propertyId, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_sparse_pyr_lk_optical_flow_create")]
        internal static partial int SparsePyrLKOpticalFlowCreate(int windowWidth, int windowHeight, int maxLevel, int criteriaType, int criteriaMaxCount, double criteriaEpsilon, int flags, double minEigenvalueThreshold, out IntPtr opticalFlow);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_sparse_pyr_lk_optical_flow_get_size_property")]
        internal static partial int SparsePyrLKOpticalFlowGetSizeProperty(IntPtr opticalFlow, out int width, out int height);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_sparse_pyr_lk_optical_flow_set_size_property")]
        internal static partial int SparsePyrLKOpticalFlowSetSizeProperty(IntPtr opticalFlow, int width, int height);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_sparse_pyr_lk_optical_flow_get_int_property")]
        internal static partial int SparsePyrLKOpticalFlowGetIntProperty(IntPtr opticalFlow, int propertyId, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_sparse_pyr_lk_optical_flow_set_int_property")]
        internal static partial int SparsePyrLKOpticalFlowSetIntProperty(IntPtr opticalFlow, int propertyId, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_sparse_pyr_lk_optical_flow_get_term_criteria")]
        internal static partial int SparsePyrLKOpticalFlowGetTermCriteria(IntPtr opticalFlow, out int type, out int maxCount, out double epsilon);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_sparse_pyr_lk_optical_flow_set_term_criteria")]
        internal static partial int SparsePyrLKOpticalFlowSetTermCriteria(IntPtr opticalFlow, int type, int maxCount, double epsilon);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_sparse_pyr_lk_optical_flow_get_min_eig_threshold")]
        internal static partial int SparsePyrLKOpticalFlowGetMinEigThreshold(IntPtr opticalFlow, out double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_sparse_pyr_lk_optical_flow_set_min_eig_threshold")]
        internal static partial int SparsePyrLKOpticalFlowSetMinEigThreshold(IntPtr opticalFlow, double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_compute_ecc")]
        internal static partial int VideoComputeECC(IntPtr templateImage, IntPtr inputImage, IntPtr inputMask, out double result);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_find_transform_ecc")]
        internal static partial int VideoFindTransformECC(IntPtr templateImage, IntPtr inputImage, IntPtr warpMatrix, int motionType, int criteriaType, int criteriaMaxCount, double criteriaEpsilon, IntPtr inputMask, int gaussianFilterSize, out double result);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_find_transform_ecc_with_mask")]
        internal static partial int VideoFindTransformECCWithMask(IntPtr templateImage, IntPtr inputImage, IntPtr templateMask, IntPtr inputMask, IntPtr warpMatrix, int motionType, int criteriaType, int criteriaMaxCount, double criteriaEpsilon, int gaussianFilterSize, out double result);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_ecc_parameters_get_default")]
        internal static partial int VideoECCParametersGetDefault(out int motionType, out int criteriaType, out int criteriaMaxCount, out double criteriaEpsilon, out int gaussianFilterSize, out int levelCount, out int interpolation);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_find_transform_ecc_multi_scale")]
        internal static partial int VideoFindTransformECCMultiScale(IntPtr referenceImage, IntPtr sampleImage, IntPtr warpMatrix, int motionType, int criteriaType, int criteriaMaxCount, double criteriaEpsilon, int* iterationsPerLevel, int iterationCount, int gaussianFilterSize, int levelCount, int interpolation, IntPtr referenceMask, IntPtr sampleMask, out double result);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_tracker_release_handle")]
        internal static partial void VideoTrackerReleaseHandle(IntPtr tracker);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_tracker_init")]
        internal static partial int VideoTrackerInit(IntPtr tracker, IntPtr image, VideoRectNative boundingBox);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_tracker_update")]
        internal static partial int VideoTrackerUpdate(IntPtr tracker, IntPtr image, ref VideoRectNative boundingBox, out int result);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_tracker_get_tracking_score")]
        internal static partial int VideoTrackerGetTrackingScore(IntPtr tracker, out float score);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_tracker_mil_get_default_params")]
        internal static partial int VideoTrackerMilGetDefaultParams(out VideoTrackerMilParamsNative parameters);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_tracker_mil_create")]
        internal static partial int VideoTrackerMilCreate(ref VideoTrackerMilParamsNative parameters, out IntPtr tracker);
    }
}
#endif
