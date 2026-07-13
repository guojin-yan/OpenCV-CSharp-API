#if NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal static partial class NativeMethods
    {
        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_census_transform")]
        internal static partial int XStereoCensusTransform(IntPtr image, int kernelSize, IntPtr dist, int type);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_census_transform_pair")]
        internal static partial int XStereoCensusTransformPair(IntPtr image1, IntPtr image2, int kernelSize, IntPtr dist1, IntPtr dist2, int type);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_modified_census_transform")]
        internal static partial int XStereoModifiedCensusTransform(IntPtr image, int kernelSize, IntPtr dist, int type, int t, IntPtr integralImage);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_modified_census_transform_pair")]
        internal static partial int XStereoModifiedCensusTransformPair(IntPtr image1, IntPtr image2, int kernelSize, IntPtr dist1, IntPtr dist2, int type, int t, IntPtr integralImage1, IntPtr integralImage2);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_symetric_census_transform")]
        internal static partial int XStereoSymetricCensusTransform(IntPtr image, int kernelSize, IntPtr dist, int type);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_symetric_census_transform_pair")]
        internal static partial int XStereoSymetricCensusTransformPair(IntPtr image1, IntPtr image2, int kernelSize, IntPtr dist1, IntPtr dist2, int type);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_star_census_transform")]
        internal static partial int XStereoStarCensusTransform(IntPtr image, int kernelSize, IntPtr dist);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_star_census_transform_pair")]
        internal static partial int XStereoStarCensusTransformPair(IntPtr image1, IntPtr image2, int kernelSize, IntPtr dist1, IntPtr dist2);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_bm_create")]
        internal static partial int XStereoBinaryBMCreate(int numDisparities, int blockSize, out IntPtr matcher);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_bm_release")]
        internal static partial void XStereoBinaryBMRelease(IntPtr matcher);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_bm_compute")]
        internal static partial int XStereoBinaryBMCompute(IntPtr matcher, IntPtr left, IntPtr right, IntPtr disparity);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_sgbm_create")]
        internal static partial int XStereoBinarySGBMCreate(int minDisparity, int numDisparities, int blockSize, int p1, int p2, int disp12MaxDiff, int preFilterCap, int uniquenessRatio, int speckleWindowSize, int speckleRange, int mode, out IntPtr matcher);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_sgbm_release")]
        internal static partial void XStereoBinarySGBMRelease(IntPtr matcher);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_sgbm_compute")]
        internal static partial int XStereoBinarySGBMCompute(IntPtr matcher, IntPtr left, IntPtr right, IntPtr disparity);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_bm_get_min_disparity")]
        internal static partial int XStereoBinaryBMGetMinDisparity(IntPtr matcher, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_bm_set_min_disparity")]
        internal static partial int XStereoBinaryBMSetMinDisparity(IntPtr matcher, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_bm_get_num_disparities")]
        internal static partial int XStereoBinaryBMGetNumDisparities(IntPtr matcher, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_bm_set_num_disparities")]
        internal static partial int XStereoBinaryBMSetNumDisparities(IntPtr matcher, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_bm_get_block_size")]
        internal static partial int XStereoBinaryBMGetBlockSize(IntPtr matcher, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_bm_set_block_size")]
        internal static partial int XStereoBinaryBMSetBlockSize(IntPtr matcher, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_bm_get_speckle_window_size")]
        internal static partial int XStereoBinaryBMGetSpeckleWindowSize(IntPtr matcher, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_bm_set_speckle_window_size")]
        internal static partial int XStereoBinaryBMSetSpeckleWindowSize(IntPtr matcher, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_bm_get_speckle_range")]
        internal static partial int XStereoBinaryBMGetSpeckleRange(IntPtr matcher, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_bm_set_speckle_range")]
        internal static partial int XStereoBinaryBMSetSpeckleRange(IntPtr matcher, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_bm_get_disp12_max_diff")]
        internal static partial int XStereoBinaryBMGetDisp12MaxDiff(IntPtr matcher, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_bm_set_disp12_max_diff")]
        internal static partial int XStereoBinaryBMSetDisp12MaxDiff(IntPtr matcher, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_bm_get_pre_filter_type")]
        internal static partial int XStereoBinaryBMGetPreFilterType(IntPtr matcher, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_bm_set_pre_filter_type")]
        internal static partial int XStereoBinaryBMSetPreFilterType(IntPtr matcher, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_bm_get_pre_filter_size")]
        internal static partial int XStereoBinaryBMGetPreFilterSize(IntPtr matcher, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_bm_set_pre_filter_size")]
        internal static partial int XStereoBinaryBMSetPreFilterSize(IntPtr matcher, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_bm_get_pre_filter_cap")]
        internal static partial int XStereoBinaryBMGetPreFilterCap(IntPtr matcher, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_bm_set_pre_filter_cap")]
        internal static partial int XStereoBinaryBMSetPreFilterCap(IntPtr matcher, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_bm_get_texture_threshold")]
        internal static partial int XStereoBinaryBMGetTextureThreshold(IntPtr matcher, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_bm_set_texture_threshold")]
        internal static partial int XStereoBinaryBMSetTextureThreshold(IntPtr matcher, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_bm_get_uniqueness_ratio")]
        internal static partial int XStereoBinaryBMGetUniquenessRatio(IntPtr matcher, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_bm_set_uniqueness_ratio")]
        internal static partial int XStereoBinaryBMSetUniquenessRatio(IntPtr matcher, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_bm_get_smaller_block_size")]
        internal static partial int XStereoBinaryBMGetSmallerBlockSize(IntPtr matcher, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_bm_set_smaller_block_size")]
        internal static partial int XStereoBinaryBMSetSmallerBlockSize(IntPtr matcher, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_bm_get_scalle_factor")]
        internal static partial int XStereoBinaryBMGetScaleFactor(IntPtr matcher, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_bm_set_scalle_factor")]
        internal static partial int XStereoBinaryBMSetScaleFactor(IntPtr matcher, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_bm_get_spekle_removal_technique")]
        internal static partial int XStereoBinaryBMGetSpeckleRemovalTechnique(IntPtr matcher, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_bm_set_spekle_removal_technique")]
        internal static partial int XStereoBinaryBMSetSpeckleRemovalTechnique(IntPtr matcher, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_bm_get_use_prefilter")]
        internal static partial int XStereoBinaryBMGetUsePrefilter(IntPtr matcher, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_bm_set_use_prefilter")]
        internal static partial int XStereoBinaryBMSetUsePrefilter(IntPtr matcher, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_bm_get_binary_kernel_type")]
        internal static partial int XStereoBinaryBMGetBinaryKernelType(IntPtr matcher, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_bm_set_binary_kernel_type")]
        internal static partial int XStereoBinaryBMSetBinaryKernelType(IntPtr matcher, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_bm_get_agregation_window_size")]
        internal static partial int XStereoBinaryBMGetAggregationWindowSize(IntPtr matcher, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_bm_set_agregation_window_size")]
        internal static partial int XStereoBinaryBMSetAggregationWindowSize(IntPtr matcher, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_sgbm_get_min_disparity")]
        internal static partial int XStereoBinarySGBMGetMinDisparity(IntPtr matcher, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_sgbm_set_min_disparity")]
        internal static partial int XStereoBinarySGBMSetMinDisparity(IntPtr matcher, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_sgbm_get_num_disparities")]
        internal static partial int XStereoBinarySGBMGetNumDisparities(IntPtr matcher, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_sgbm_set_num_disparities")]
        internal static partial int XStereoBinarySGBMSetNumDisparities(IntPtr matcher, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_sgbm_get_block_size")]
        internal static partial int XStereoBinarySGBMGetBlockSize(IntPtr matcher, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_sgbm_set_block_size")]
        internal static partial int XStereoBinarySGBMSetBlockSize(IntPtr matcher, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_sgbm_get_speckle_window_size")]
        internal static partial int XStereoBinarySGBMGetSpeckleWindowSize(IntPtr matcher, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_sgbm_set_speckle_window_size")]
        internal static partial int XStereoBinarySGBMSetSpeckleWindowSize(IntPtr matcher, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_sgbm_get_speckle_range")]
        internal static partial int XStereoBinarySGBMGetSpeckleRange(IntPtr matcher, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_sgbm_set_speckle_range")]
        internal static partial int XStereoBinarySGBMSetSpeckleRange(IntPtr matcher, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_sgbm_get_disp12_max_diff")]
        internal static partial int XStereoBinarySGBMGetDisp12MaxDiff(IntPtr matcher, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_sgbm_set_disp12_max_diff")]
        internal static partial int XStereoBinarySGBMSetDisp12MaxDiff(IntPtr matcher, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_sgbm_get_pre_filter_cap")]
        internal static partial int XStereoBinarySGBMGetPreFilterCap(IntPtr matcher, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_sgbm_set_pre_filter_cap")]
        internal static partial int XStereoBinarySGBMSetPreFilterCap(IntPtr matcher, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_sgbm_get_uniqueness_ratio")]
        internal static partial int XStereoBinarySGBMGetUniquenessRatio(IntPtr matcher, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_sgbm_set_uniqueness_ratio")]
        internal static partial int XStereoBinarySGBMSetUniquenessRatio(IntPtr matcher, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_sgbm_get_p1")]
        internal static partial int XStereoBinarySGBMGetP1(IntPtr matcher, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_sgbm_set_p1")]
        internal static partial int XStereoBinarySGBMSetP1(IntPtr matcher, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_sgbm_get_p2")]
        internal static partial int XStereoBinarySGBMGetP2(IntPtr matcher, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_sgbm_set_p2")]
        internal static partial int XStereoBinarySGBMSetP2(IntPtr matcher, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_sgbm_get_mode")]
        internal static partial int XStereoBinarySGBMGetMode(IntPtr matcher, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_sgbm_set_mode")]
        internal static partial int XStereoBinarySGBMSetMode(IntPtr matcher, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_sgbm_get_spekle_removal_technique")]
        internal static partial int XStereoBinarySGBMGetSpeckleRemovalTechnique(IntPtr matcher, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_sgbm_set_spekle_removal_technique")]
        internal static partial int XStereoBinarySGBMSetSpeckleRemovalTechnique(IntPtr matcher, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_sgbm_get_binary_kernel_type")]
        internal static partial int XStereoBinarySGBMGetBinaryKernelType(IntPtr matcher, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_sgbm_set_binary_kernel_type")]
        internal static partial int XStereoBinarySGBMSetBinaryKernelType(IntPtr matcher, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_sgbm_get_sub_pixel_interpolation_method")]
        internal static partial int XStereoBinarySGBMGetSubPixelInterpolationMethod(IntPtr matcher, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_binary_sgbm_set_sub_pixel_interpolation_method")]
        internal static partial int XStereoBinarySGBMSetSubPixelInterpolationMethod(IntPtr matcher, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_quasi_dense_create")]
        internal static partial int XStereoQuasiDenseCreate(int width, int height, byte[] parameterFilePath, out IntPtr stereo);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_quasi_dense_release")]
        internal static partial void XStereoQuasiDenseRelease(IntPtr stereo);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_quasi_dense_load_parameters")]
        internal static partial int XStereoQuasiDenseLoadParameters(IntPtr stereo, byte[] parameterFilePath, out int result);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_quasi_dense_save_parameters")]
        internal static partial int XStereoQuasiDenseSaveParameters(IntPtr stereo, byte[] parameterFilePath, out int result);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_quasi_dense_process")]
        internal static partial int XStereoQuasiDenseProcess(IntPtr stereo, IntPtr left, IntPtr right);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_quasi_dense_get_sparse_matches_count")]
        internal static partial int XStereoQuasiDenseGetSparseMatchesCount(IntPtr stereo, out int count);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_quasi_dense_get_sparse_matches_fill")]
        internal static partial int XStereoQuasiDenseGetSparseMatchesFill(IntPtr stereo, NativeXStereoMatchQuasiDense[] matches, int matchCapacity, out int count);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_quasi_dense_get_dense_matches_count")]
        internal static partial int XStereoQuasiDenseGetDenseMatchesCount(IntPtr stereo, out int count);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_quasi_dense_get_dense_matches_fill")]
        internal static partial int XStereoQuasiDenseGetDenseMatchesFill(IntPtr stereo, NativeXStereoMatchQuasiDense[] matches, int matchCapacity, out int count);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_quasi_dense_get_match")]
        internal static partial int XStereoQuasiDenseGetMatch(IntPtr stereo, int x, int y, out float matchX, out float matchY);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_quasi_dense_get_disparity")]
        internal static partial int XStereoQuasiDenseGetDisparity(IntPtr stereo, IntPtr disparity);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_quasi_dense_get_parameters")]
        internal static partial int XStereoQuasiDenseGetParameters(IntPtr stereo, out NativeXStereoPropagationParameters parameters);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xstereo_quasi_dense_set_parameters")]
        internal static partial int XStereoQuasiDenseSetParameters(IntPtr stereo, ref NativeXStereoPropagationParameters parameters);
    }
}
#endif
