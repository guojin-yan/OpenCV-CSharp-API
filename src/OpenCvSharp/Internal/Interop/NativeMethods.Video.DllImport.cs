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
    }
}
#endif
