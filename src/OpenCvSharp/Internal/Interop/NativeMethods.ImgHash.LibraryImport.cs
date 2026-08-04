#if NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    internal static partial class NativeMethods
    {
        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_img_hash_average_create")]
        internal static partial int ImgHashAverageCreate(out IntPtr hash);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_img_hash_phash_create")]
        internal static partial int ImgHashPHashCreate(out IntPtr hash);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_img_hash_block_mean_create")]
        internal static partial int ImgHashBlockMeanCreate(int mode, out IntPtr hash);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_img_hash_color_moment_create")]
        internal static partial int ImgHashColorMomentCreate(out IntPtr hash);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_img_hash_marr_hildreth_create")]
        internal static partial int ImgHashMarrHildrethCreate(float alpha, float scale, out IntPtr hash);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_img_hash_radial_variance_create")]
        internal static partial int ImgHashRadialVarianceCreate(double sigma, int numOfAngleLine, out IntPtr hash);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_img_hash_release_handle")]
        internal static partial void ImgHashReleaseHandle(IntPtr hash);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_img_hash_compute")]
        internal static partial int ImgHashCompute(IntPtr hash, IntPtr input, IntPtr output);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_img_hash_compare")]
        internal static partial int ImgHashCompare(IntPtr hash, IntPtr hashOne, IntPtr hashTwo, out double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_img_hash_block_mean_set_mode")]
        internal static partial int ImgHashBlockMeanSetMode(IntPtr hash, int mode);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_img_hash_block_mean_get_mean_count")]
        internal static partial int ImgHashBlockMeanGetMeanCount(IntPtr hash, out int count);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_img_hash_block_mean_get_mean_fill")]
        internal static partial int ImgHashBlockMeanGetMeanFill(IntPtr hash, double[] values, int valueCapacity, out int count);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_img_hash_marr_hildreth_get")]
        internal static partial int ImgHashMarrHildrethGet(IntPtr hash, out float alpha, out float scale);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_img_hash_marr_hildreth_set_kernel_param")]
        internal static partial int ImgHashMarrHildrethSetKernelParam(IntPtr hash, float alpha, float scale);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_img_hash_radial_variance_get")]
        internal static partial int ImgHashRadialVarianceGet(IntPtr hash, out double sigma, out int numOfAngleLine);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_img_hash_radial_variance_set_sigma")]
        internal static partial int ImgHashRadialVarianceSetSigma(IntPtr hash, double sigma);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_img_hash_radial_variance_set_num_of_angle_line")]
        internal static partial int ImgHashRadialVarianceSetNumOfAngleLine(IntPtr hash, int numOfAngleLine);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_img_hash_average_compute_static")]
        internal static partial int ImgHashAverageComputeStatic(IntPtr input, IntPtr output);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_img_hash_phash_compute_static")]
        internal static partial int ImgHashPHashComputeStatic(IntPtr input, IntPtr output);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_img_hash_block_mean_compute_static")]
        internal static partial int ImgHashBlockMeanComputeStatic(IntPtr input, IntPtr output, int mode);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_img_hash_color_moment_compute_static")]
        internal static partial int ImgHashColorMomentComputeStatic(IntPtr input, IntPtr output);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_img_hash_marr_hildreth_compute_static")]
        internal static partial int ImgHashMarrHildrethComputeStatic(IntPtr input, IntPtr output, float alpha, float scale);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_img_hash_radial_variance_compute_static")]
        internal static partial int ImgHashRadialVarianceComputeStatic(IntPtr input, IntPtr output, double sigma, int numOfAngleLine);
    }
}
#endif
