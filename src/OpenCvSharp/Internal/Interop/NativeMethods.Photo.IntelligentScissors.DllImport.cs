#if !NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    internal static partial class NativeMethods
    {
        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_intelligent_scissors_create")]
        internal static extern int PhotoIntelligentScissorsCreate(out IntPtr scissors);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_intelligent_scissors_release_handle")]
        internal static extern void PhotoIntelligentScissorsReleaseHandle(IntPtr scissors);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_intelligent_scissors_set_weights")]
        internal static extern int PhotoIntelligentScissorsSetWeights(IntPtr scissors, float weightNonEdge, float weightGradientDirection, float weightGradientMagnitude);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_intelligent_scissors_set_gradient_magnitude_max_limit")]
        internal static extern int PhotoIntelligentScissorsSetGradientMagnitudeMaxLimit(IntPtr scissors, float gradientMagnitudeThresholdMax);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_intelligent_scissors_set_edge_feature_zero_crossing_parameters")]
        internal static extern int PhotoIntelligentScissorsSetEdgeFeatureZeroCrossingParameters(IntPtr scissors, float gradientMagnitudeMinValue);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_intelligent_scissors_set_edge_feature_canny_parameters")]
        internal static extern int PhotoIntelligentScissorsSetEdgeFeatureCannyParameters(IntPtr scissors, double threshold1, double threshold2, int apertureSize, int l2Gradient);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_intelligent_scissors_apply_image")]
        internal static extern int PhotoIntelligentScissorsApplyImage(IntPtr scissors, IntPtr image);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_intelligent_scissors_apply_image_features")]
        internal static extern int PhotoIntelligentScissorsApplyImageFeatures(IntPtr scissors, IntPtr nonEdge, IntPtr gradientDirection, IntPtr gradientMagnitude, IntPtr image);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_intelligent_scissors_build_map")]
        internal static extern int PhotoIntelligentScissorsBuildMap(IntPtr scissors, int sourceX, int sourceY);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_intelligent_scissors_get_contour")]
        internal static extern int PhotoIntelligentScissorsGetContour(IntPtr scissors, int targetX, int targetY, IntPtr contour, int backward);
    }
}
#endif
