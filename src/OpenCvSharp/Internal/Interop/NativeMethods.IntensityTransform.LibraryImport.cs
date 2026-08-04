#if NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    internal static partial class NativeMethods
    {
        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_intensity_transform_log")]
        internal static partial int IntensityTransformLog(IntPtr src, IntPtr dst);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_intensity_transform_gamma_correction")]
        internal static partial int IntensityTransformGammaCorrection(IntPtr src, IntPtr dst, float gamma);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_intensity_transform_autoscaling")]
        internal static partial int IntensityTransformAutoscaling(IntPtr src, IntPtr dst);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_intensity_transform_contrast_stretching")]
        internal static partial int IntensityTransformContrastStretching(IntPtr src, IntPtr dst, int r1, int s1, int r2, int s2);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_intensity_transform_bimef")]
        internal static partial int IntensityTransformBimef(IntPtr src, IntPtr dst, float mu, float a, float b);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_intensity_transform_bimef_with_k")]
        internal static partial int IntensityTransformBimefWithK(IntPtr src, IntPtr dst, float k, float mu, float a, float b);
    }
}
#endif
