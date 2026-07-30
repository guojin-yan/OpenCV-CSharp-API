#if NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal static partial class NativeMethods
    {
        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_intelligent_scissors_create")]
        internal static partial int PhotoIntelligentScissorsCreate(out IntPtr scissors);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_intelligent_scissors_release_handle")]
        internal static partial void PhotoIntelligentScissorsReleaseHandle(IntPtr scissors);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_intelligent_scissors_set_weights")]
        internal static partial int PhotoIntelligentScissorsSetWeights(IntPtr scissors, float weightNonEdge, float weightGradientDirection, float weightGradientMagnitude);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_intelligent_scissors_set_gradient_magnitude_max_limit")]
        internal static partial int PhotoIntelligentScissorsSetGradientMagnitudeMaxLimit(IntPtr scissors, float gradientMagnitudeThresholdMax);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_intelligent_scissors_set_edge_feature_zero_crossing_parameters")]
        internal static partial int PhotoIntelligentScissorsSetEdgeFeatureZeroCrossingParameters(IntPtr scissors, float gradientMagnitudeMinValue);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_intelligent_scissors_set_edge_feature_canny_parameters")]
        internal static partial int PhotoIntelligentScissorsSetEdgeFeatureCannyParameters(IntPtr scissors, double threshold1, double threshold2, int apertureSize, int l2Gradient);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_intelligent_scissors_apply_image")]
        internal static partial int PhotoIntelligentScissorsApplyImage(IntPtr scissors, IntPtr image);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_intelligent_scissors_apply_image_features")]
        internal static partial int PhotoIntelligentScissorsApplyImageFeatures(IntPtr scissors, IntPtr nonEdge, IntPtr gradientDirection, IntPtr gradientMagnitude, IntPtr image);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_intelligent_scissors_build_map")]
        internal static partial int PhotoIntelligentScissorsBuildMap(IntPtr scissors, int sourceX, int sourceY);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_intelligent_scissors_get_contour")]
        internal static partial int PhotoIntelligentScissorsGetContour(IntPtr scissors, int targetX, int targetY, IntPtr contour, int backward);
    }
}
#endif
