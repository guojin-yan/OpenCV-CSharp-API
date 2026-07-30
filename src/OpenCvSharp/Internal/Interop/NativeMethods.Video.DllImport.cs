#if !NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal static unsafe partial class NativeMethods
    {
        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_calc_optical_flow_pyr_lk")]
        internal static extern int VideoCalcOpticalFlowPyrLK(IntPtr prevImg, IntPtr nextImg, VideoPoint2fNative* prevPoints, int pointCount, VideoPoint2fNative* initialNextPoints, int useInitialFlow, VideoPoint2fNative* nextPoints, byte* status, float* err, int winWidth, int winHeight, int maxLevel, int criteriaType, int criteriaMaxCount, double criteriaEpsilon, int flags, double minEigThreshold);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_calc_optical_flow_farneback")]
        internal static extern int VideoCalcOpticalFlowFarneback(IntPtr prev, IntPtr next, IntPtr flow, double pyrScale, int levels, int winsize, int iterations, int polyN, double polySigma, int flags);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_read_optical_flow")]
        internal static extern int VideoReadOpticalFlow(byte[] path, out IntPtr flow);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_write_optical_flow")]
        internal static extern int VideoWriteOpticalFlow(byte[] path, IntPtr flow, out int result);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_build_optical_flow_pyramid_count")]
        internal static extern int VideoBuildOpticalFlowPyramidCount(IntPtr image, int winWidth, int winHeight, int maxLevel, int withDerivatives, int pyrBorder, int derivBorder, int tryReuseInputImage, out int levelCount, out int matCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_build_optical_flow_pyramid_fill")]
        internal static extern int VideoBuildOpticalFlowPyramidFill(IntPtr image, int winWidth, int winHeight, int maxLevel, int withDerivatives, int pyrBorder, int derivBorder, int tryReuseInputImage, IntPtr[] pyramid, int pyramidCapacity, out int levelCount, out int matCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_mean_shift")]
        internal static extern int VideoMeanShift(IntPtr probImage, ref VideoRectNative window, int criteriaType, int criteriaMaxCount, double criteriaEpsilon, out int iterations);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_cam_shift")]
        internal static extern int VideoCamShift(IntPtr probImage, ref VideoRectNative window, int criteriaType, int criteriaMaxCount, double criteriaEpsilon, out VideoRotatedRectNative box);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_kalman_filter_create")]
        internal static extern int KalmanFilterCreate(int dynamParams, int measureParams, int controlParams, int type, out IntPtr filter);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_kalman_filter_release_handle")]
        internal static extern void KalmanFilterReleaseHandle(IntPtr filter);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_kalman_filter_init")]
        internal static extern int KalmanFilterInit(IntPtr filter, int dynamParams, int measureParams, int controlParams, int type);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_kalman_filter_predict")]
        internal static extern int KalmanFilterPredict(IntPtr filter, IntPtr control, IntPtr prediction);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_kalman_filter_correct")]
        internal static extern int KalmanFilterCorrect(IntPtr filter, IntPtr measurement, IntPtr corrected);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_kalman_filter_get_matrix")]
        internal static extern int KalmanFilterGetMatrix(IntPtr filter, int matrixId, IntPtr value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_kalman_filter_set_matrix")]
        internal static extern int KalmanFilterSetMatrix(IntPtr filter, int matrixId, IntPtr value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_background_subtractor_release_handle")]
        internal static extern void BackgroundSubtractorReleaseHandle(IntPtr subtractor);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_background_subtractor_apply")]
        internal static extern int BackgroundSubtractorApply(IntPtr subtractor, IntPtr image, IntPtr fgmask, double learningRate);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_background_subtractor_apply_with_known_foreground")]
        internal static extern int BackgroundSubtractorApplyWithKnownForeground(IntPtr subtractor, IntPtr image, IntPtr knownForegroundMask, IntPtr fgmask, double learningRate);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_background_subtractor_get_background_image")]
        internal static extern int BackgroundSubtractorGetBackgroundImage(IntPtr subtractor, IntPtr backgroundImage);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_background_subtractor_mog2_create")]
        internal static extern int BackgroundSubtractorMOG2Create(int history, double varThreshold, int detectShadows, out IntPtr subtractor);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_background_subtractor_mog2_get_history")]
        internal static extern int BackgroundSubtractorMOG2GetHistory(IntPtr subtractor, out int history);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_background_subtractor_mog2_set_history")]
        internal static extern int BackgroundSubtractorMOG2SetHistory(IntPtr subtractor, int history);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_background_subtractor_mog2_get_n_mixtures")]
        internal static extern int BackgroundSubtractorMOG2GetNMixtures(IntPtr subtractor, out int nMixtures);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_background_subtractor_mog2_set_n_mixtures")]
        internal static extern int BackgroundSubtractorMOG2SetNMixtures(IntPtr subtractor, int nMixtures);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_background_subtractor_mog2_get_detect_shadows")]
        internal static extern int BackgroundSubtractorMOG2GetDetectShadows(IntPtr subtractor, out int detectShadows);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_background_subtractor_mog2_set_detect_shadows")]
        internal static extern int BackgroundSubtractorMOG2SetDetectShadows(IntPtr subtractor, int detectShadows);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_background_subtractor_mog2_get_int_property")]
        internal static extern int BackgroundSubtractorMOG2GetIntProperty(IntPtr subtractor, int propertyId, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_background_subtractor_mog2_set_int_property")]
        internal static extern int BackgroundSubtractorMOG2SetIntProperty(IntPtr subtractor, int propertyId, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_background_subtractor_mog2_get_double_property")]
        internal static extern int BackgroundSubtractorMOG2GetDoubleProperty(IntPtr subtractor, int propertyId, out double value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_background_subtractor_mog2_set_double_property")]
        internal static extern int BackgroundSubtractorMOG2SetDoubleProperty(IntPtr subtractor, int propertyId, double value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_background_subtractor_knn_create")]
        internal static extern int BackgroundSubtractorKNNCreate(int history, double dist2Threshold, int detectShadows, out IntPtr subtractor);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_background_subtractor_knn_get_history")]
        internal static extern int BackgroundSubtractorKNNGetHistory(IntPtr subtractor, out int history);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_background_subtractor_knn_set_history")]
        internal static extern int BackgroundSubtractorKNNSetHistory(IntPtr subtractor, int history);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_background_subtractor_knn_get_n_samples")]
        internal static extern int BackgroundSubtractorKNNGetNSamples(IntPtr subtractor, out int nSamples);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_background_subtractor_knn_set_n_samples")]
        internal static extern int BackgroundSubtractorKNNSetNSamples(IntPtr subtractor, int nSamples);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_background_subtractor_knn_get_detect_shadows")]
        internal static extern int BackgroundSubtractorKNNGetDetectShadows(IntPtr subtractor, out int detectShadows);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_background_subtractor_knn_set_detect_shadows")]
        internal static extern int BackgroundSubtractorKNNSetDetectShadows(IntPtr subtractor, int detectShadows);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_background_subtractor_knn_get_int_property")]
        internal static extern int BackgroundSubtractorKNNGetIntProperty(IntPtr subtractor, int propertyId, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_background_subtractor_knn_set_int_property")]
        internal static extern int BackgroundSubtractorKNNSetIntProperty(IntPtr subtractor, int propertyId, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_background_subtractor_knn_get_double_property")]
        internal static extern int BackgroundSubtractorKNNGetDoubleProperty(IntPtr subtractor, int propertyId, out double value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_background_subtractor_knn_set_double_property")]
        internal static extern int BackgroundSubtractorKNNSetDoubleProperty(IntPtr subtractor, int propertyId, double value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dense_optical_flow_release_handle")]
        internal static extern void DenseOpticalFlowReleaseHandle(IntPtr opticalFlow);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dense_optical_flow_calc")]
        internal static extern int DenseOpticalFlowCalc(IntPtr opticalFlow, IntPtr first, IntPtr second, IntPtr flow);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dense_optical_flow_collect_garbage")]
        internal static extern int DenseOpticalFlowCollectGarbage(IntPtr opticalFlow);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_sparse_optical_flow_release_handle")]
        internal static extern void SparseOpticalFlowReleaseHandle(IntPtr opticalFlow);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_sparse_optical_flow_calc")]
        internal static extern int SparseOpticalFlowCalc(IntPtr opticalFlow, IntPtr previousImage, IntPtr nextImage, VideoPoint2fNative* previousPoints, int pointCount, VideoPoint2fNative* nextPoints, byte* status, float* error);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_farneback_optical_flow_create")]
        internal static extern int FarnebackOpticalFlowCreate(int numLevels, double pyramidScale, int fastPyramids, int windowSize, int numIterations, int polynomialNeighborhood, double polynomialSigma, int flags, out IntPtr opticalFlow);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_farneback_optical_flow_get_int_property")]
        internal static extern int FarnebackOpticalFlowGetIntProperty(IntPtr opticalFlow, int propertyId, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_farneback_optical_flow_set_int_property")]
        internal static extern int FarnebackOpticalFlowSetIntProperty(IntPtr opticalFlow, int propertyId, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_farneback_optical_flow_get_double_property")]
        internal static extern int FarnebackOpticalFlowGetDoubleProperty(IntPtr opticalFlow, int propertyId, out double value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_farneback_optical_flow_set_double_property")]
        internal static extern int FarnebackOpticalFlowSetDoubleProperty(IntPtr opticalFlow, int propertyId, double value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_farneback_optical_flow_get_bool_property")]
        internal static extern int FarnebackOpticalFlowGetBoolProperty(IntPtr opticalFlow, int propertyId, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_farneback_optical_flow_set_bool_property")]
        internal static extern int FarnebackOpticalFlowSetBoolProperty(IntPtr opticalFlow, int propertyId, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_variational_refinement_create")]
        internal static extern int VariationalRefinementCreate(out IntPtr opticalFlow);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_variational_refinement_calc_uv")]
        internal static extern int VariationalRefinementCalcUV(IntPtr opticalFlow, IntPtr first, IntPtr second, IntPtr flowU, IntPtr flowV);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_variational_refinement_get_int_property")]
        internal static extern int VariationalRefinementGetIntProperty(IntPtr opticalFlow, int propertyId, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_variational_refinement_set_int_property")]
        internal static extern int VariationalRefinementSetIntProperty(IntPtr opticalFlow, int propertyId, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_variational_refinement_get_float_property")]
        internal static extern int VariationalRefinementGetFloatProperty(IntPtr opticalFlow, int propertyId, out float value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_variational_refinement_set_float_property")]
        internal static extern int VariationalRefinementSetFloatProperty(IntPtr opticalFlow, int propertyId, float value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dis_optical_flow_create")]
        internal static extern int DisOpticalFlowCreate(int preset, out IntPtr opticalFlow);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dis_optical_flow_get_int_property")]
        internal static extern int DisOpticalFlowGetIntProperty(IntPtr opticalFlow, int propertyId, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dis_optical_flow_set_int_property")]
        internal static extern int DisOpticalFlowSetIntProperty(IntPtr opticalFlow, int propertyId, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dis_optical_flow_get_float_property")]
        internal static extern int DisOpticalFlowGetFloatProperty(IntPtr opticalFlow, int propertyId, out float value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dis_optical_flow_set_float_property")]
        internal static extern int DisOpticalFlowSetFloatProperty(IntPtr opticalFlow, int propertyId, float value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dis_optical_flow_get_bool_property")]
        internal static extern int DisOpticalFlowGetBoolProperty(IntPtr opticalFlow, int propertyId, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dis_optical_flow_set_bool_property")]
        internal static extern int DisOpticalFlowSetBoolProperty(IntPtr opticalFlow, int propertyId, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_sparse_pyr_lk_optical_flow_create")]
        internal static extern int SparsePyrLKOpticalFlowCreate(int windowWidth, int windowHeight, int maxLevel, int criteriaType, int criteriaMaxCount, double criteriaEpsilon, int flags, double minEigenvalueThreshold, out IntPtr opticalFlow);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_sparse_pyr_lk_optical_flow_get_size_property")]
        internal static extern int SparsePyrLKOpticalFlowGetSizeProperty(IntPtr opticalFlow, out int width, out int height);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_sparse_pyr_lk_optical_flow_set_size_property")]
        internal static extern int SparsePyrLKOpticalFlowSetSizeProperty(IntPtr opticalFlow, int width, int height);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_sparse_pyr_lk_optical_flow_get_int_property")]
        internal static extern int SparsePyrLKOpticalFlowGetIntProperty(IntPtr opticalFlow, int propertyId, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_sparse_pyr_lk_optical_flow_set_int_property")]
        internal static extern int SparsePyrLKOpticalFlowSetIntProperty(IntPtr opticalFlow, int propertyId, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_sparse_pyr_lk_optical_flow_get_term_criteria")]
        internal static extern int SparsePyrLKOpticalFlowGetTermCriteria(IntPtr opticalFlow, out int type, out int maxCount, out double epsilon);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_sparse_pyr_lk_optical_flow_set_term_criteria")]
        internal static extern int SparsePyrLKOpticalFlowSetTermCriteria(IntPtr opticalFlow, int type, int maxCount, double epsilon);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_sparse_pyr_lk_optical_flow_get_min_eig_threshold")]
        internal static extern int SparsePyrLKOpticalFlowGetMinEigThreshold(IntPtr opticalFlow, out double value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_sparse_pyr_lk_optical_flow_set_min_eig_threshold")]
        internal static extern int SparsePyrLKOpticalFlowSetMinEigThreshold(IntPtr opticalFlow, double value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_compute_ecc")]
        internal static extern int VideoComputeECC(IntPtr templateImage, IntPtr inputImage, IntPtr inputMask, out double result);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_find_transform_ecc")]
        internal static extern int VideoFindTransformECC(IntPtr templateImage, IntPtr inputImage, IntPtr warpMatrix, int motionType, int criteriaType, int criteriaMaxCount, double criteriaEpsilon, IntPtr inputMask, int gaussianFilterSize, out double result);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_find_transform_ecc_with_mask")]
        internal static extern int VideoFindTransformECCWithMask(IntPtr templateImage, IntPtr inputImage, IntPtr templateMask, IntPtr inputMask, IntPtr warpMatrix, int motionType, int criteriaType, int criteriaMaxCount, double criteriaEpsilon, int gaussianFilterSize, out double result);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_ecc_parameters_get_default")]
        internal static extern int VideoECCParametersGetDefault(out int motionType, out int criteriaType, out int criteriaMaxCount, out double criteriaEpsilon, out int gaussianFilterSize, out int levelCount, out int interpolation);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_find_transform_ecc_multi_scale")]
        internal static extern int VideoFindTransformECCMultiScale(IntPtr referenceImage, IntPtr sampleImage, IntPtr warpMatrix, int motionType, int criteriaType, int criteriaMaxCount, double criteriaEpsilon, int* iterationsPerLevel, int iterationCount, int gaussianFilterSize, int levelCount, int interpolation, IntPtr referenceMask, IntPtr sampleMask, out double result);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_tracker_release_handle")]
        internal static extern void VideoTrackerReleaseHandle(IntPtr tracker);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_tracker_init")]
        internal static extern int VideoTrackerInit(IntPtr tracker, IntPtr image, VideoRectNative boundingBox);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_tracker_update")]
        internal static extern int VideoTrackerUpdate(IntPtr tracker, IntPtr image, ref VideoRectNative boundingBox, out int result);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_tracker_get_tracking_score")]
        internal static extern int VideoTrackerGetTrackingScore(IntPtr tracker, out float score);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_tracker_mil_get_default_params")]
        internal static extern int VideoTrackerMilGetDefaultParams(out VideoTrackerMilParamsNative parameters);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_tracker_mil_create")]
        internal static extern int VideoTrackerMilCreate(ref VideoTrackerMilParamsNative parameters, out IntPtr tracker);
    }
}
#endif
