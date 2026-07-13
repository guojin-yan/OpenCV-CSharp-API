#if !NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal static unsafe partial class NativeMethods
    {
        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_cascade_classifier_create")]
        internal static extern int CascadeClassifierCreate(out IntPtr classifier);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_cascade_classifier_create_from_file")]
        internal static extern int CascadeClassifierCreateFromFile(byte[] filename, out IntPtr classifier);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_cascade_classifier_release_handle")]
        internal static extern void CascadeClassifierReleaseHandle(IntPtr classifier);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_cascade_classifier_load")]
        internal static extern int CascadeClassifierLoad(IntPtr classifier, byte[] filename, out int loaded);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_cascade_classifier_empty")]
        internal static extern int CascadeClassifierEmpty(IntPtr classifier, out int empty);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_cascade_classifier_get_original_window_size")]
        internal static extern int CascadeClassifierGetOriginalWindowSize(IntPtr classifier, out int width, out int height);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_cascade_classifier_is_old_format_cascade")]
        internal static extern int CascadeClassifierIsOldFormatCascade(IntPtr classifier, out int result);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_cascade_classifier_get_feature_type")]
        internal static extern int CascadeClassifierGetFeatureType(IntPtr classifier, out int featureType);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_cascade_classifier_detect_multi_scale_count")]
        internal static extern int CascadeClassifierDetectMultiScaleCount(IntPtr classifier, IntPtr image, double scaleFactor, int minNeighbors, int flags, int minWidth, int minHeight, int maxWidth, int maxHeight, out int count);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_cascade_classifier_detect_multi_scale_fill")]
        internal static extern int CascadeClassifierDetectMultiScaleFill(IntPtr classifier, IntPtr image, double scaleFactor, int minNeighbors, int flags, int minWidth, int minHeight, int maxWidth, int maxHeight, int[] rectangles, int rectangleCapacity, out int count);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_cascade_classifier_detect_multi_scale2_count")]
        internal static extern int CascadeClassifierDetectMultiScale2Count(IntPtr classifier, IntPtr image, double scaleFactor, int minNeighbors, int flags, int minWidth, int minHeight, int maxWidth, int maxHeight, out int count);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_cascade_classifier_detect_multi_scale2_fill")]
        internal static extern int CascadeClassifierDetectMultiScale2Fill(IntPtr classifier, IntPtr image, double scaleFactor, int minNeighbors, int flags, int minWidth, int minHeight, int maxWidth, int maxHeight, int[] rectangles, int rectangleCapacity, int[] numDetections, int numDetectionCapacity, out int count);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_cascade_classifier_detect_multi_scale3_count")]
        internal static extern int CascadeClassifierDetectMultiScale3Count(IntPtr classifier, IntPtr image, double scaleFactor, int minNeighbors, int flags, int minWidth, int minHeight, int maxWidth, int maxHeight, int outputRejectLevels, out int count);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_cascade_classifier_detect_multi_scale3_fill")]
        internal static extern int CascadeClassifierDetectMultiScale3Fill(IntPtr classifier, IntPtr image, double scaleFactor, int minNeighbors, int flags, int minWidth, int minHeight, int maxWidth, int maxHeight, int outputRejectLevels, int[] rectangles, int rectangleCapacity, int[] rejectLevels, int rejectLevelCapacity, double[] levelWeights, int levelWeightCapacity, out int count);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_hog_descriptor_create")]
        internal static extern int HOGDescriptorCreate(out IntPtr descriptor);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_hog_descriptor_create_with_params")]
        internal static extern int HOGDescriptorCreateWithParams(int winWidth, int winHeight, int blockWidth, int blockHeight, int blockStrideWidth, int blockStrideHeight, int cellWidth, int cellHeight, int nbins, int derivAperture, double winSigma, int histogramNormType, double l2HysThreshold, int gammaCorrection, int nlevels, int signedGradient, out IntPtr descriptor);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_hog_descriptor_create_from_file")]
        internal static extern int HOGDescriptorCreateFromFile(byte[] filename, out IntPtr descriptor);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_hog_descriptor_release_handle")]
        internal static extern void HOGDescriptorReleaseHandle(IntPtr descriptor);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_hog_descriptor_get_default_people_detector_count")]
        internal static extern int HOGDescriptorGetDefaultPeopleDetectorCount(out int count);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_hog_descriptor_get_default_people_detector_fill")]
        internal static extern int HOGDescriptorGetDefaultPeopleDetectorFill(float[] values, int valueCapacity, out int count);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_hog_descriptor_get_daimler_people_detector_count")]
        internal static extern int HOGDescriptorGetDaimlerPeopleDetectorCount(out int count);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_hog_descriptor_get_daimler_people_detector_fill")]
        internal static extern int HOGDescriptorGetDaimlerPeopleDetectorFill(float[] values, int valueCapacity, out int count);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_hog_descriptor_set_svm_detector")]
        internal static extern int HOGDescriptorSetSVMDetector(IntPtr descriptor, float[] values, int valueCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_hog_descriptor_set_svm_detector")]
        internal static extern int HOGDescriptorSetSVMDetector(IntPtr descriptor, float* values, int valueCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_hog_descriptor_check_detector_size")]
        internal static extern int HOGDescriptorCheckDetectorSize(IntPtr descriptor, out int result);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_hog_descriptor_get_descriptor_size")]
        internal static extern int HOGDescriptorGetDescriptorSize(IntPtr descriptor, out UIntPtr descriptorSize);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_hog_descriptor_get_win_sigma")]
        internal static extern int HOGDescriptorGetWinSigma(IntPtr descriptor, out double winSigma);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_hog_descriptor_get_property")]
        internal static extern int HOGDescriptorGetProperty(IntPtr descriptor, int propertyId, out double value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_hog_descriptor_set_property")]
        internal static extern int HOGDescriptorSetProperty(IntPtr descriptor, int propertyId, double value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_hog_descriptor_detect_count")]
        internal static extern int HOGDescriptorDetectCount(IntPtr descriptor, IntPtr image, double hitThreshold, int winStrideWidth, int winStrideHeight, int paddingWidth, int paddingHeight, out int count);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_hog_descriptor_detect_fill")]
        internal static extern int HOGDescriptorDetectFill(IntPtr descriptor, IntPtr image, double hitThreshold, int winStrideWidth, int winStrideHeight, int paddingWidth, int paddingHeight, int[] locationsXy, int locationCapacity, double[] weights, int weightCapacity, out int count);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_hog_descriptor_detect_multi_scale_count")]
        internal static extern int HOGDescriptorDetectMultiScaleCount(IntPtr descriptor, IntPtr image, double hitThreshold, int winStrideWidth, int winStrideHeight, int paddingWidth, int paddingHeight, double scale, double groupThreshold, int useMeanshiftGrouping, out int count);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_hog_descriptor_detect_multi_scale_fill")]
        internal static extern int HOGDescriptorDetectMultiScaleFill(IntPtr descriptor, IntPtr image, double hitThreshold, int winStrideWidth, int winStrideHeight, int paddingWidth, int paddingHeight, double scale, double groupThreshold, int useMeanshiftGrouping, int[] rectangles, int rectangleCapacity, double[] weights, int weightCapacity, out int count);
    }
}
#endif
