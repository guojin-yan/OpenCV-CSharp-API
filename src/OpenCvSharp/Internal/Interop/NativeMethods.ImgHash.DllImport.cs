#if !NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    internal static partial class NativeMethods
    {
        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_img_hash_average_create")]
        internal static extern int ImgHashAverageCreate(out IntPtr hash);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_img_hash_phash_create")]
        internal static extern int ImgHashPHashCreate(out IntPtr hash);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_img_hash_block_mean_create")]
        internal static extern int ImgHashBlockMeanCreate(int mode, out IntPtr hash);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_img_hash_color_moment_create")]
        internal static extern int ImgHashColorMomentCreate(out IntPtr hash);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_img_hash_marr_hildreth_create")]
        internal static extern int ImgHashMarrHildrethCreate(float alpha, float scale, out IntPtr hash);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_img_hash_radial_variance_create")]
        internal static extern int ImgHashRadialVarianceCreate(double sigma, int numOfAngleLine, out IntPtr hash);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_img_hash_release_handle")]
        internal static extern void ImgHashReleaseHandle(IntPtr hash);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_img_hash_compute")]
        internal static extern int ImgHashCompute(IntPtr hash, IntPtr input, IntPtr output);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_img_hash_compare")]
        internal static extern int ImgHashCompare(IntPtr hash, IntPtr hashOne, IntPtr hashTwo, out double value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_img_hash_block_mean_set_mode")]
        internal static extern int ImgHashBlockMeanSetMode(IntPtr hash, int mode);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_img_hash_block_mean_get_mean_count")]
        internal static extern int ImgHashBlockMeanGetMeanCount(IntPtr hash, out int count);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_img_hash_block_mean_get_mean_fill")]
        internal static extern int ImgHashBlockMeanGetMeanFill(IntPtr hash, double[] values, int valueCapacity, out int count);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_img_hash_marr_hildreth_get")]
        internal static extern int ImgHashMarrHildrethGet(IntPtr hash, out float alpha, out float scale);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_img_hash_marr_hildreth_set_kernel_param")]
        internal static extern int ImgHashMarrHildrethSetKernelParam(IntPtr hash, float alpha, float scale);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_img_hash_radial_variance_get")]
        internal static extern int ImgHashRadialVarianceGet(IntPtr hash, out double sigma, out int numOfAngleLine);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_img_hash_radial_variance_set_sigma")]
        internal static extern int ImgHashRadialVarianceSetSigma(IntPtr hash, double sigma);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_img_hash_radial_variance_set_num_of_angle_line")]
        internal static extern int ImgHashRadialVarianceSetNumOfAngleLine(IntPtr hash, int numOfAngleLine);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_img_hash_average_compute_static")]
        internal static extern int ImgHashAverageComputeStatic(IntPtr input, IntPtr output);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_img_hash_phash_compute_static")]
        internal static extern int ImgHashPHashComputeStatic(IntPtr input, IntPtr output);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_img_hash_block_mean_compute_static")]
        internal static extern int ImgHashBlockMeanComputeStatic(IntPtr input, IntPtr output, int mode);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_img_hash_color_moment_compute_static")]
        internal static extern int ImgHashColorMomentComputeStatic(IntPtr input, IntPtr output);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_img_hash_marr_hildreth_compute_static")]
        internal static extern int ImgHashMarrHildrethComputeStatic(IntPtr input, IntPtr output, float alpha, float scale);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_img_hash_radial_variance_compute_static")]
        internal static extern int ImgHashRadialVarianceComputeStatic(IntPtr input, IntPtr output, double sigma, int numOfAngleLine);
    }
}
#endif
