#if NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal static unsafe partial class NativeMethods
    {
        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_cascade_classifier_create")]
        internal static partial int CascadeClassifierCreate(out IntPtr classifier);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_cascade_classifier_create_from_file")]
        internal static partial int CascadeClassifierCreateFromFile(byte[] filename, out IntPtr classifier);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_cascade_classifier_release_handle")]
        internal static partial void CascadeClassifierReleaseHandle(IntPtr classifier);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_cascade_classifier_load")]
        internal static partial int CascadeClassifierLoad(IntPtr classifier, byte[] filename, out int loaded);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_cascade_classifier_empty")]
        internal static partial int CascadeClassifierEmpty(IntPtr classifier, out int empty);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_cascade_classifier_get_original_window_size")]
        internal static partial int CascadeClassifierGetOriginalWindowSize(IntPtr classifier, out int width, out int height);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_cascade_classifier_is_old_format_cascade")]
        internal static partial int CascadeClassifierIsOldFormatCascade(IntPtr classifier, out int result);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_cascade_classifier_get_feature_type")]
        internal static partial int CascadeClassifierGetFeatureType(IntPtr classifier, out int featureType);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_cascade_classifier_detect_multi_scale_count")]
        internal static partial int CascadeClassifierDetectMultiScaleCount(IntPtr classifier, IntPtr image, double scaleFactor, int minNeighbors, int flags, int minWidth, int minHeight, int maxWidth, int maxHeight, out int count);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_cascade_classifier_detect_multi_scale_fill")]
        internal static partial int CascadeClassifierDetectMultiScaleFill(IntPtr classifier, IntPtr image, double scaleFactor, int minNeighbors, int flags, int minWidth, int minHeight, int maxWidth, int maxHeight, int[] rectangles, int rectangleCapacity, out int count);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_cascade_classifier_detect_multi_scale2_count")]
        internal static partial int CascadeClassifierDetectMultiScale2Count(IntPtr classifier, IntPtr image, double scaleFactor, int minNeighbors, int flags, int minWidth, int minHeight, int maxWidth, int maxHeight, out int count);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_cascade_classifier_detect_multi_scale2_fill")]
        internal static partial int CascadeClassifierDetectMultiScale2Fill(IntPtr classifier, IntPtr image, double scaleFactor, int minNeighbors, int flags, int minWidth, int minHeight, int maxWidth, int maxHeight, int[] rectangles, int rectangleCapacity, int[] numDetections, int numDetectionCapacity, out int count);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_cascade_classifier_detect_multi_scale3_count")]
        internal static partial int CascadeClassifierDetectMultiScale3Count(IntPtr classifier, IntPtr image, double scaleFactor, int minNeighbors, int flags, int minWidth, int minHeight, int maxWidth, int maxHeight, int outputRejectLevels, out int count);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_cascade_classifier_detect_multi_scale3_fill")]
        internal static partial int CascadeClassifierDetectMultiScale3Fill(IntPtr classifier, IntPtr image, double scaleFactor, int minNeighbors, int flags, int minWidth, int minHeight, int maxWidth, int maxHeight, int outputRejectLevels, int[] rectangles, int rectangleCapacity, int[] rejectLevels, int rejectLevelCapacity, double[] levelWeights, int levelWeightCapacity, out int count);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_hog_descriptor_create")]
        internal static partial int HOGDescriptorCreate(out IntPtr descriptor);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_hog_descriptor_create_with_params")]
        internal static partial int HOGDescriptorCreateWithParams(int winWidth, int winHeight, int blockWidth, int blockHeight, int blockStrideWidth, int blockStrideHeight, int cellWidth, int cellHeight, int nbins, int derivAperture, double winSigma, int histogramNormType, double l2HysThreshold, int gammaCorrection, int nlevels, int signedGradient, out IntPtr descriptor);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_hog_descriptor_create_from_file")]
        internal static partial int HOGDescriptorCreateFromFile(byte[] filename, out IntPtr descriptor);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_hog_descriptor_release_handle")]
        internal static partial void HOGDescriptorReleaseHandle(IntPtr descriptor);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_hog_descriptor_get_default_people_detector_count")]
        internal static partial int HOGDescriptorGetDefaultPeopleDetectorCount(out int count);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_hog_descriptor_get_default_people_detector_fill")]
        internal static partial int HOGDescriptorGetDefaultPeopleDetectorFill(float[] values, int valueCapacity, out int count);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_hog_descriptor_get_daimler_people_detector_count")]
        internal static partial int HOGDescriptorGetDaimlerPeopleDetectorCount(out int count);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_hog_descriptor_get_daimler_people_detector_fill")]
        internal static partial int HOGDescriptorGetDaimlerPeopleDetectorFill(float[] values, int valueCapacity, out int count);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_hog_descriptor_set_svm_detector")]
        internal static partial int HOGDescriptorSetSVMDetector(IntPtr descriptor, float[] values, int valueCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_hog_descriptor_set_svm_detector")]
        internal static unsafe partial int HOGDescriptorSetSVMDetector(IntPtr descriptor, float* values, int valueCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_hog_descriptor_check_detector_size")]
        internal static partial int HOGDescriptorCheckDetectorSize(IntPtr descriptor, out int result);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_hog_descriptor_get_descriptor_size")]
        internal static partial int HOGDescriptorGetDescriptorSize(IntPtr descriptor, out UIntPtr descriptorSize);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_hog_descriptor_get_win_sigma")]
        internal static partial int HOGDescriptorGetWinSigma(IntPtr descriptor, out double winSigma);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_hog_descriptor_get_property")]
        internal static partial int HOGDescriptorGetProperty(IntPtr descriptor, int propertyId, out double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_hog_descriptor_set_property")]
        internal static partial int HOGDescriptorSetProperty(IntPtr descriptor, int propertyId, double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_hog_descriptor_detect_count")]
        internal static partial int HOGDescriptorDetectCount(IntPtr descriptor, IntPtr image, double hitThreshold, int winStrideWidth, int winStrideHeight, int paddingWidth, int paddingHeight, out int count);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_hog_descriptor_detect_fill")]
        internal static partial int HOGDescriptorDetectFill(IntPtr descriptor, IntPtr image, double hitThreshold, int winStrideWidth, int winStrideHeight, int paddingWidth, int paddingHeight, int[] locationsXy, int locationCapacity, double[] weights, int weightCapacity, out int count);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_hog_descriptor_detect_multi_scale_count")]
        internal static partial int HOGDescriptorDetectMultiScaleCount(IntPtr descriptor, IntPtr image, double hitThreshold, int winStrideWidth, int winStrideHeight, int paddingWidth, int paddingHeight, double scale, double groupThreshold, int useMeanshiftGrouping, out int count);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_hog_descriptor_detect_multi_scale_fill")]
        internal static partial int HOGDescriptorDetectMultiScaleFill(IntPtr descriptor, IntPtr image, double hitThreshold, int winStrideWidth, int winStrideHeight, int paddingWidth, int paddingHeight, double scale, double groupThreshold, int useMeanshiftGrouping, int[] rectangles, int rectangleCapacity, double[] weights, int weightCapacity, out int count);
    }
}
#endif
