#if !NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    internal static partial class NativeMethods
    {
        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_quality_mse_create")]
        internal static extern int QualityMSECreate(IntPtr reference, out IntPtr quality);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_quality_psnr_create")]
        internal static extern int QualityPSNRCreate(IntPtr reference, double maxPixelValue, out IntPtr quality);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_quality_ssim_create")]
        internal static extern int QualitySSIMCreate(IntPtr reference, out IntPtr quality);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_quality_gmsd_create")]
        internal static extern int QualityGMSDCreate(IntPtr reference, out IntPtr quality);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_quality_brisque_create")]
        internal static extern int QualityBRISQUECreate(byte[] modelFilePath, byte[] rangeFilePath, out IntPtr quality);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_quality_release_handle")]
        internal static extern void QualityReleaseHandle(IntPtr quality);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_quality_compute")]
        internal static extern int QualityCompute(IntPtr quality, IntPtr comparison, double[] scalarValues, int scalarCapacity);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_quality_get_quality_map")]
        internal static extern int QualityGetQualityMap(IntPtr quality, IntPtr qualityMap);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_quality_clear")]
        internal static extern int QualityClear(IntPtr quality);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_quality_empty")]
        internal static extern int QualityEmpty(IntPtr quality, out int empty);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_quality_psnr_get_max_pixel_value")]
        internal static extern int QualityPSNRGetMaxPixelValue(IntPtr quality, out double maxPixelValue);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_quality_psnr_set_max_pixel_value")]
        internal static extern int QualityPSNRSetMaxPixelValue(IntPtr quality, double maxPixelValue);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_quality_mse_compute_static")]
        internal static extern int QualityMSEComputeStatic(IntPtr reference, IntPtr comparison, IntPtr qualityMap, double[] scalarValues, int scalarCapacity);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_quality_psnr_compute_static")]
        internal static extern int QualityPSNRComputeStatic(IntPtr reference, IntPtr comparison, IntPtr qualityMap, double maxPixelValue, double[] scalarValues, int scalarCapacity);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_quality_ssim_compute_static")]
        internal static extern int QualitySSIMComputeStatic(IntPtr reference, IntPtr comparison, IntPtr qualityMap, double[] scalarValues, int scalarCapacity);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_quality_gmsd_compute_static")]
        internal static extern int QualityGMSDComputeStatic(IntPtr reference, IntPtr comparison, IntPtr qualityMap, double[] scalarValues, int scalarCapacity);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_quality_brisque_compute_static")]
        internal static extern int QualityBRISQUEComputeStatic(IntPtr image, byte[] modelFilePath, byte[] rangeFilePath, double[] scalarValues, int scalarCapacity);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_quality_brisque_compute_features")]
        internal static extern int QualityBRISQUEComputeFeatures(IntPtr image, IntPtr features);
    }
}
#endif
