#if !NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    internal static partial class NativeMethods
    {
        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_census_transform")]
        internal static extern int XStereoCensusTransform(IntPtr image, int kernelSize, IntPtr dist, int type);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_census_transform_pair")]
        internal static extern int XStereoCensusTransformPair(IntPtr image1, IntPtr image2, int kernelSize, IntPtr dist1, IntPtr dist2, int type);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_modified_census_transform")]
        internal static extern int XStereoModifiedCensusTransform(IntPtr image, int kernelSize, IntPtr dist, int type, int t, IntPtr integralImage);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_modified_census_transform_pair")]
        internal static extern int XStereoModifiedCensusTransformPair(IntPtr image1, IntPtr image2, int kernelSize, IntPtr dist1, IntPtr dist2, int type, int t, IntPtr integralImage1, IntPtr integralImage2);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_symetric_census_transform")]
        internal static extern int XStereoSymetricCensusTransform(IntPtr image, int kernelSize, IntPtr dist, int type);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_symetric_census_transform_pair")]
        internal static extern int XStereoSymetricCensusTransformPair(IntPtr image1, IntPtr image2, int kernelSize, IntPtr dist1, IntPtr dist2, int type);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_star_census_transform")]
        internal static extern int XStereoStarCensusTransform(IntPtr image, int kernelSize, IntPtr dist);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_star_census_transform_pair")]
        internal static extern int XStereoStarCensusTransformPair(IntPtr image1, IntPtr image2, int kernelSize, IntPtr dist1, IntPtr dist2);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_bm_create")]
        internal static extern int XStereoBinaryBMCreate(int numDisparities, int blockSize, out IntPtr matcher);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_bm_release")]
        internal static extern void XStereoBinaryBMRelease(IntPtr matcher);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_bm_compute")]
        internal static extern int XStereoBinaryBMCompute(IntPtr matcher, IntPtr left, IntPtr right, IntPtr disparity);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_sgbm_create")]
        internal static extern int XStereoBinarySGBMCreate(int minDisparity, int numDisparities, int blockSize, int p1, int p2, int disp12MaxDiff, int preFilterCap, int uniquenessRatio, int speckleWindowSize, int speckleRange, int mode, out IntPtr matcher);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_sgbm_release")]
        internal static extern void XStereoBinarySGBMRelease(IntPtr matcher);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_sgbm_compute")]
        internal static extern int XStereoBinarySGBMCompute(IntPtr matcher, IntPtr left, IntPtr right, IntPtr disparity);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_bm_get_min_disparity")]
        internal static extern int XStereoBinaryBMGetMinDisparity(IntPtr matcher, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_bm_set_min_disparity")]
        internal static extern int XStereoBinaryBMSetMinDisparity(IntPtr matcher, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_bm_get_num_disparities")]
        internal static extern int XStereoBinaryBMGetNumDisparities(IntPtr matcher, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_bm_set_num_disparities")]
        internal static extern int XStereoBinaryBMSetNumDisparities(IntPtr matcher, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_bm_get_block_size")]
        internal static extern int XStereoBinaryBMGetBlockSize(IntPtr matcher, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_bm_set_block_size")]
        internal static extern int XStereoBinaryBMSetBlockSize(IntPtr matcher, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_bm_get_speckle_window_size")]
        internal static extern int XStereoBinaryBMGetSpeckleWindowSize(IntPtr matcher, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_bm_set_speckle_window_size")]
        internal static extern int XStereoBinaryBMSetSpeckleWindowSize(IntPtr matcher, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_bm_get_speckle_range")]
        internal static extern int XStereoBinaryBMGetSpeckleRange(IntPtr matcher, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_bm_set_speckle_range")]
        internal static extern int XStereoBinaryBMSetSpeckleRange(IntPtr matcher, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_bm_get_disp12_max_diff")]
        internal static extern int XStereoBinaryBMGetDisp12MaxDiff(IntPtr matcher, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_bm_set_disp12_max_diff")]
        internal static extern int XStereoBinaryBMSetDisp12MaxDiff(IntPtr matcher, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_bm_get_pre_filter_type")]
        internal static extern int XStereoBinaryBMGetPreFilterType(IntPtr matcher, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_bm_set_pre_filter_type")]
        internal static extern int XStereoBinaryBMSetPreFilterType(IntPtr matcher, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_bm_get_pre_filter_size")]
        internal static extern int XStereoBinaryBMGetPreFilterSize(IntPtr matcher, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_bm_set_pre_filter_size")]
        internal static extern int XStereoBinaryBMSetPreFilterSize(IntPtr matcher, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_bm_get_pre_filter_cap")]
        internal static extern int XStereoBinaryBMGetPreFilterCap(IntPtr matcher, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_bm_set_pre_filter_cap")]
        internal static extern int XStereoBinaryBMSetPreFilterCap(IntPtr matcher, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_bm_get_texture_threshold")]
        internal static extern int XStereoBinaryBMGetTextureThreshold(IntPtr matcher, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_bm_set_texture_threshold")]
        internal static extern int XStereoBinaryBMSetTextureThreshold(IntPtr matcher, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_bm_get_uniqueness_ratio")]
        internal static extern int XStereoBinaryBMGetUniquenessRatio(IntPtr matcher, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_bm_set_uniqueness_ratio")]
        internal static extern int XStereoBinaryBMSetUniquenessRatio(IntPtr matcher, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_bm_get_smaller_block_size")]
        internal static extern int XStereoBinaryBMGetSmallerBlockSize(IntPtr matcher, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_bm_set_smaller_block_size")]
        internal static extern int XStereoBinaryBMSetSmallerBlockSize(IntPtr matcher, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_bm_get_scalle_factor")]
        internal static extern int XStereoBinaryBMGetScaleFactor(IntPtr matcher, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_bm_set_scalle_factor")]
        internal static extern int XStereoBinaryBMSetScaleFactor(IntPtr matcher, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_bm_get_spekle_removal_technique")]
        internal static extern int XStereoBinaryBMGetSpeckleRemovalTechnique(IntPtr matcher, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_bm_set_spekle_removal_technique")]
        internal static extern int XStereoBinaryBMSetSpeckleRemovalTechnique(IntPtr matcher, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_bm_get_use_prefilter")]
        internal static extern int XStereoBinaryBMGetUsePrefilter(IntPtr matcher, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_bm_set_use_prefilter")]
        internal static extern int XStereoBinaryBMSetUsePrefilter(IntPtr matcher, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_bm_get_binary_kernel_type")]
        internal static extern int XStereoBinaryBMGetBinaryKernelType(IntPtr matcher, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_bm_set_binary_kernel_type")]
        internal static extern int XStereoBinaryBMSetBinaryKernelType(IntPtr matcher, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_bm_get_agregation_window_size")]
        internal static extern int XStereoBinaryBMGetAggregationWindowSize(IntPtr matcher, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_bm_set_agregation_window_size")]
        internal static extern int XStereoBinaryBMSetAggregationWindowSize(IntPtr matcher, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_sgbm_get_min_disparity")]
        internal static extern int XStereoBinarySGBMGetMinDisparity(IntPtr matcher, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_sgbm_set_min_disparity")]
        internal static extern int XStereoBinarySGBMSetMinDisparity(IntPtr matcher, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_sgbm_get_num_disparities")]
        internal static extern int XStereoBinarySGBMGetNumDisparities(IntPtr matcher, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_sgbm_set_num_disparities")]
        internal static extern int XStereoBinarySGBMSetNumDisparities(IntPtr matcher, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_sgbm_get_block_size")]
        internal static extern int XStereoBinarySGBMGetBlockSize(IntPtr matcher, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_sgbm_set_block_size")]
        internal static extern int XStereoBinarySGBMSetBlockSize(IntPtr matcher, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_sgbm_get_speckle_window_size")]
        internal static extern int XStereoBinarySGBMGetSpeckleWindowSize(IntPtr matcher, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_sgbm_set_speckle_window_size")]
        internal static extern int XStereoBinarySGBMSetSpeckleWindowSize(IntPtr matcher, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_sgbm_get_speckle_range")]
        internal static extern int XStereoBinarySGBMGetSpeckleRange(IntPtr matcher, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_sgbm_set_speckle_range")]
        internal static extern int XStereoBinarySGBMSetSpeckleRange(IntPtr matcher, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_sgbm_get_disp12_max_diff")]
        internal static extern int XStereoBinarySGBMGetDisp12MaxDiff(IntPtr matcher, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_sgbm_set_disp12_max_diff")]
        internal static extern int XStereoBinarySGBMSetDisp12MaxDiff(IntPtr matcher, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_sgbm_get_pre_filter_cap")]
        internal static extern int XStereoBinarySGBMGetPreFilterCap(IntPtr matcher, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_sgbm_set_pre_filter_cap")]
        internal static extern int XStereoBinarySGBMSetPreFilterCap(IntPtr matcher, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_sgbm_get_uniqueness_ratio")]
        internal static extern int XStereoBinarySGBMGetUniquenessRatio(IntPtr matcher, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_sgbm_set_uniqueness_ratio")]
        internal static extern int XStereoBinarySGBMSetUniquenessRatio(IntPtr matcher, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_sgbm_get_p1")]
        internal static extern int XStereoBinarySGBMGetP1(IntPtr matcher, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_sgbm_set_p1")]
        internal static extern int XStereoBinarySGBMSetP1(IntPtr matcher, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_sgbm_get_p2")]
        internal static extern int XStereoBinarySGBMGetP2(IntPtr matcher, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_sgbm_set_p2")]
        internal static extern int XStereoBinarySGBMSetP2(IntPtr matcher, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_sgbm_get_mode")]
        internal static extern int XStereoBinarySGBMGetMode(IntPtr matcher, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_sgbm_set_mode")]
        internal static extern int XStereoBinarySGBMSetMode(IntPtr matcher, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_sgbm_get_spekle_removal_technique")]
        internal static extern int XStereoBinarySGBMGetSpeckleRemovalTechnique(IntPtr matcher, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_sgbm_set_spekle_removal_technique")]
        internal static extern int XStereoBinarySGBMSetSpeckleRemovalTechnique(IntPtr matcher, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_sgbm_get_binary_kernel_type")]
        internal static extern int XStereoBinarySGBMGetBinaryKernelType(IntPtr matcher, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_sgbm_set_binary_kernel_type")]
        internal static extern int XStereoBinarySGBMSetBinaryKernelType(IntPtr matcher, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_sgbm_get_sub_pixel_interpolation_method")]
        internal static extern int XStereoBinarySGBMGetSubPixelInterpolationMethod(IntPtr matcher, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_sgbm_set_sub_pixel_interpolation_method")]
        internal static extern int XStereoBinarySGBMSetSubPixelInterpolationMethod(IntPtr matcher, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_quasi_dense_create")]
        internal static extern int XStereoQuasiDenseCreate(int width, int height, byte[] parameterFilePath, out IntPtr stereo);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_quasi_dense_release")]
        internal static extern void XStereoQuasiDenseRelease(IntPtr stereo);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_quasi_dense_load_parameters")]
        internal static extern int XStereoQuasiDenseLoadParameters(IntPtr stereo, byte[] parameterFilePath, out int result);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_quasi_dense_save_parameters")]
        internal static extern int XStereoQuasiDenseSaveParameters(IntPtr stereo, byte[] parameterFilePath, out int result);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_quasi_dense_process")]
        internal static extern int XStereoQuasiDenseProcess(IntPtr stereo, IntPtr left, IntPtr right);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_quasi_dense_get_sparse_matches_count")]
        internal static extern int XStereoQuasiDenseGetSparseMatchesCount(IntPtr stereo, out int count);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_quasi_dense_get_sparse_matches_fill")]
        internal static extern int XStereoQuasiDenseGetSparseMatchesFill(IntPtr stereo, NativeXStereoMatchQuasiDense[] matches, int matchCapacity, out int count);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_quasi_dense_get_dense_matches_count")]
        internal static extern int XStereoQuasiDenseGetDenseMatchesCount(IntPtr stereo, out int count);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_quasi_dense_get_dense_matches_fill")]
        internal static extern int XStereoQuasiDenseGetDenseMatchesFill(IntPtr stereo, NativeXStereoMatchQuasiDense[] matches, int matchCapacity, out int count);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_quasi_dense_get_match")]
        internal static extern int XStereoQuasiDenseGetMatch(IntPtr stereo, int x, int y, out float matchX, out float matchY);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_quasi_dense_get_disparity")]
        internal static extern int XStereoQuasiDenseGetDisparity(IntPtr stereo, IntPtr disparity);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_quasi_dense_get_parameters")]
        internal static extern int XStereoQuasiDenseGetParameters(IntPtr stereo, out NativeXStereoPropagationParameters parameters);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_quasi_dense_set_parameters")]
        internal static extern int XStereoQuasiDenseSetParameters(IntPtr stereo, ref NativeXStereoPropagationParameters parameters);
    }
}
#endif
