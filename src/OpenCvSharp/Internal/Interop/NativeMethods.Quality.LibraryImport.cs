#if NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    internal static partial class NativeMethods
    {
        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_quality_mse_create")]
        internal static partial int QualityMSECreate(IntPtr reference, out IntPtr quality);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_quality_psnr_create")]
        internal static partial int QualityPSNRCreate(IntPtr reference, double maxPixelValue, out IntPtr quality);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_quality_ssim_create")]
        internal static partial int QualitySSIMCreate(IntPtr reference, out IntPtr quality);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_quality_gmsd_create")]
        internal static partial int QualityGMSDCreate(IntPtr reference, out IntPtr quality);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_quality_brisque_create")]
        internal static partial int QualityBRISQUECreate(byte[] modelFilePath, byte[] rangeFilePath, out IntPtr quality);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_quality_release_handle")]
        internal static partial void QualityReleaseHandle(IntPtr quality);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_quality_compute")]
        internal static partial int QualityCompute(IntPtr quality, IntPtr comparison, double[] scalarValues, int scalarCapacity);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_quality_get_quality_map")]
        internal static partial int QualityGetQualityMap(IntPtr quality, IntPtr qualityMap);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_quality_clear")]
        internal static partial int QualityClear(IntPtr quality);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_quality_empty")]
        internal static partial int QualityEmpty(IntPtr quality, out int empty);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_quality_psnr_get_max_pixel_value")]
        internal static partial int QualityPSNRGetMaxPixelValue(IntPtr quality, out double maxPixelValue);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_quality_psnr_set_max_pixel_value")]
        internal static partial int QualityPSNRSetMaxPixelValue(IntPtr quality, double maxPixelValue);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_quality_mse_compute_static")]
        internal static partial int QualityMSEComputeStatic(IntPtr reference, IntPtr comparison, IntPtr qualityMap, double[] scalarValues, int scalarCapacity);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_quality_psnr_compute_static")]
        internal static partial int QualityPSNRComputeStatic(IntPtr reference, IntPtr comparison, IntPtr qualityMap, double maxPixelValue, double[] scalarValues, int scalarCapacity);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_quality_ssim_compute_static")]
        internal static partial int QualitySSIMComputeStatic(IntPtr reference, IntPtr comparison, IntPtr qualityMap, double[] scalarValues, int scalarCapacity);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_quality_gmsd_compute_static")]
        internal static partial int QualityGMSDComputeStatic(IntPtr reference, IntPtr comparison, IntPtr qualityMap, double[] scalarValues, int scalarCapacity);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_quality_brisque_compute_static")]
        internal static partial int QualityBRISQUEComputeStatic(IntPtr image, byte[] modelFilePath, byte[] rangeFilePath, double[] scalarValues, int scalarCapacity);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_quality_brisque_compute_features")]
        internal static partial int QualityBRISQUEComputeFeatures(IntPtr image, IntPtr features);
    }
}
#endif
