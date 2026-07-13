#if !NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal static partial class NativeMethods
    {
        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_intensity_transform_log")]
        internal static extern int IntensityTransformLog(IntPtr src, IntPtr dst);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_intensity_transform_gamma_correction")]
        internal static extern int IntensityTransformGammaCorrection(IntPtr src, IntPtr dst, float gamma);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_intensity_transform_autoscaling")]
        internal static extern int IntensityTransformAutoscaling(IntPtr src, IntPtr dst);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_intensity_transform_contrast_stretching")]
        internal static extern int IntensityTransformContrastStretching(IntPtr src, IntPtr dst, int r1, int s1, int r2, int s2);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_intensity_transform_bimef")]
        internal static extern int IntensityTransformBimef(IntPtr src, IntPtr dst, float mu, float a, float b);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_intensity_transform_bimef_with_k")]
        internal static extern int IntensityTransformBimefWithK(IntPtr src, IntPtr dst, float k, float mu, float a, float b);
    }
}
#endif
