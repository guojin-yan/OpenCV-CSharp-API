#if !NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal static unsafe partial class NativeMethods
    {
        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_brisk_create")]
        internal static extern int Features2DBriskCreate(int threshold, int octaves, float patternScale, out IntPtr brisk);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_brisk_create_pattern")]
        internal static extern int Features2DBriskCreatePattern(float* radiusList, int radiusCount, int* numberList, int numberCount, float dMax, float dMin, int* indexChange, int indexChangeCount, out IntPtr brisk);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_brisk_create_pattern_with_threshold")]
        internal static extern int Features2DBriskCreatePatternWithThreshold(int threshold, int octaves, float* radiusList, int radiusCount, int* numberList, int numberCount, float dMax, float dMin, int* indexChange, int indexChangeCount, out IntPtr brisk);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_brisk_release")]
        internal static extern void Features2DBriskRelease(IntPtr brisk);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_brisk_clear")]
        internal static extern int Features2DBriskClear(IntPtr brisk);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_brisk_empty")]
        internal static extern int Features2DBriskEmpty(IntPtr brisk, out int empty);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_brisk_descriptor_size")]
        internal static extern int Features2DBriskDescriptorSize(IntPtr brisk, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_brisk_descriptor_type")]
        internal static extern int Features2DBriskDescriptorType(IntPtr brisk, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_brisk_default_norm")]
        internal static extern int Features2DBriskDefaultNorm(IntPtr brisk, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_brisk_detect_count")]
        internal static extern int Features2DBriskDetectCount(IntPtr brisk, IntPtr image, IntPtr mask, out int keypointCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_brisk_detect_fill")]
        internal static extern int Features2DBriskDetectFill(IntPtr brisk, IntPtr image, IntPtr mask, NativeKeyPoint* keypoints, int keypointCapacity, out int keypointCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_brisk_compute")]
        internal static extern int Features2DBriskCompute(IntPtr brisk, IntPtr image, NativeKeyPoint* keypointsIn, int keypointCount, NativeKeyPoint* keypointsOut, int keypointCapacity, out int writtenKeypointCount, IntPtr descriptors);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_brisk_detect_and_compute_count")]
        internal static extern int Features2DBriskDetectAndComputeCount(IntPtr brisk, IntPtr image, IntPtr mask, NativeKeyPoint* keypointsIn, int keypointCount, int useProvidedKeypoints, out int outputKeypointCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_brisk_detect_and_compute_fill")]
        internal static extern int Features2DBriskDetectAndComputeFill(IntPtr brisk, IntPtr image, IntPtr mask, NativeKeyPoint* keypointsIn, int keypointCount, int useProvidedKeypoints, NativeKeyPoint* keypointsOut, int keypointCapacity, out int outputKeypointCount, IntPtr descriptors);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_brisk_default_name_length")]
        internal static extern int Features2DBriskDefaultNameLength(IntPtr brisk, out int length);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_brisk_default_name_fill")]
        internal static extern int Features2DBriskDefaultNameFill(IntPtr brisk, byte* buffer, int bufferCapacity, out int written);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_brisk_get_threshold")]
        internal static extern int Features2DBriskGetThreshold(IntPtr brisk, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_brisk_set_threshold")]
        internal static extern int Features2DBriskSetThreshold(IntPtr brisk, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_brisk_get_octaves")]
        internal static extern int Features2DBriskGetOctaves(IntPtr brisk, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_brisk_set_octaves")]
        internal static extern int Features2DBriskSetOctaves(IntPtr brisk, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_brisk_get_pattern_scale")]
        internal static extern int Features2DBriskGetPatternScale(IntPtr brisk, out float value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_brisk_set_pattern_scale")]
        internal static extern int Features2DBriskSetPatternScale(IntPtr brisk, float value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_kaze_create")]
        internal static extern int Features2DKazeCreate(int extended, int upright, float threshold, int nOctaves, int nOctaveLayers, int diffusivity, out IntPtr kaze);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_kaze_release")]
        internal static extern void Features2DKazeRelease(IntPtr kaze);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_kaze_clear")]
        internal static extern int Features2DKazeClear(IntPtr kaze);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_kaze_empty")]
        internal static extern int Features2DKazeEmpty(IntPtr kaze, out int empty);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_kaze_descriptor_size")]
        internal static extern int Features2DKazeDescriptorSize(IntPtr kaze, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_kaze_descriptor_type")]
        internal static extern int Features2DKazeDescriptorType(IntPtr kaze, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_kaze_default_norm")]
        internal static extern int Features2DKazeDefaultNorm(IntPtr kaze, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_kaze_detect_count")]
        internal static extern int Features2DKazeDetectCount(IntPtr kaze, IntPtr image, IntPtr mask, out int keypointCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_kaze_detect_fill")]
        internal static extern int Features2DKazeDetectFill(IntPtr kaze, IntPtr image, IntPtr mask, NativeKeyPoint* keypoints, int keypointCapacity, out int keypointCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_kaze_compute")]
        internal static extern int Features2DKazeCompute(IntPtr kaze, IntPtr image, NativeKeyPoint* keypointsIn, int keypointCount, NativeKeyPoint* keypointsOut, int keypointCapacity, out int writtenKeypointCount, IntPtr descriptors);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_kaze_detect_and_compute_count")]
        internal static extern int Features2DKazeDetectAndComputeCount(IntPtr kaze, IntPtr image, IntPtr mask, NativeKeyPoint* keypointsIn, int keypointCount, int useProvidedKeypoints, out int outputKeypointCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_kaze_detect_and_compute_fill")]
        internal static extern int Features2DKazeDetectAndComputeFill(IntPtr kaze, IntPtr image, IntPtr mask, NativeKeyPoint* keypointsIn, int keypointCount, int useProvidedKeypoints, NativeKeyPoint* keypointsOut, int keypointCapacity, out int outputKeypointCount, IntPtr descriptors);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_kaze_default_name_length")]
        internal static extern int Features2DKazeDefaultNameLength(IntPtr kaze, out int length);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_kaze_default_name_fill")]
        internal static extern int Features2DKazeDefaultNameFill(IntPtr kaze, byte* buffer, int bufferCapacity, out int written);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_kaze_get_extended")]
        internal static extern int Features2DKazeGetExtended(IntPtr kaze, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_kaze_set_extended")]
        internal static extern int Features2DKazeSetExtended(IntPtr kaze, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_kaze_get_upright")]
        internal static extern int Features2DKazeGetUpright(IntPtr kaze, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_kaze_set_upright")]
        internal static extern int Features2DKazeSetUpright(IntPtr kaze, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_kaze_get_threshold")]
        internal static extern int Features2DKazeGetThreshold(IntPtr kaze, out double value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_kaze_set_threshold")]
        internal static extern int Features2DKazeSetThreshold(IntPtr kaze, double value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_kaze_get_n_octaves")]
        internal static extern int Features2DKazeGetNOctaves(IntPtr kaze, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_kaze_set_n_octaves")]
        internal static extern int Features2DKazeSetNOctaves(IntPtr kaze, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_kaze_get_n_octave_layers")]
        internal static extern int Features2DKazeGetNOctaveLayers(IntPtr kaze, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_kaze_set_n_octave_layers")]
        internal static extern int Features2DKazeSetNOctaveLayers(IntPtr kaze, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_kaze_get_diffusivity")]
        internal static extern int Features2DKazeGetDiffusivity(IntPtr kaze, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_kaze_set_diffusivity")]
        internal static extern int Features2DKazeSetDiffusivity(IntPtr kaze, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_akaze_create")]
        internal static extern int Features2DAkazeCreate(int descriptorType, int descriptorSize, int descriptorChannels, float threshold, int nOctaves, int nOctaveLayers, int diffusivity, int maxPoints, out IntPtr akaze);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_akaze_release")]
        internal static extern void Features2DAkazeRelease(IntPtr akaze);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_akaze_clear")]
        internal static extern int Features2DAkazeClear(IntPtr akaze);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_akaze_empty")]
        internal static extern int Features2DAkazeEmpty(IntPtr akaze, out int empty);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_akaze_descriptor_size")]
        internal static extern int Features2DAkazeDescriptorSize(IntPtr akaze, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_akaze_descriptor_type")]
        internal static extern int Features2DAkazeDescriptorType(IntPtr akaze, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_akaze_default_norm")]
        internal static extern int Features2DAkazeDefaultNorm(IntPtr akaze, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_akaze_detect_count")]
        internal static extern int Features2DAkazeDetectCount(IntPtr akaze, IntPtr image, IntPtr mask, out int keypointCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_akaze_detect_fill")]
        internal static extern int Features2DAkazeDetectFill(IntPtr akaze, IntPtr image, IntPtr mask, NativeKeyPoint* keypoints, int keypointCapacity, out int keypointCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_akaze_compute")]
        internal static extern int Features2DAkazeCompute(IntPtr akaze, IntPtr image, NativeKeyPoint* keypointsIn, int keypointCount, NativeKeyPoint* keypointsOut, int keypointCapacity, out int writtenKeypointCount, IntPtr descriptors);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_akaze_detect_and_compute_count")]
        internal static extern int Features2DAkazeDetectAndComputeCount(IntPtr akaze, IntPtr image, IntPtr mask, NativeKeyPoint* keypointsIn, int keypointCount, int useProvidedKeypoints, out int outputKeypointCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_akaze_detect_and_compute_fill")]
        internal static extern int Features2DAkazeDetectAndComputeFill(IntPtr akaze, IntPtr image, IntPtr mask, NativeKeyPoint* keypointsIn, int keypointCount, int useProvidedKeypoints, NativeKeyPoint* keypointsOut, int keypointCapacity, out int outputKeypointCount, IntPtr descriptors);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_akaze_default_name_length")]
        internal static extern int Features2DAkazeDefaultNameLength(IntPtr akaze, out int length);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_akaze_default_name_fill")]
        internal static extern int Features2DAkazeDefaultNameFill(IntPtr akaze, byte* buffer, int bufferCapacity, out int written);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_akaze_get_descriptor_type")]
        internal static extern int Features2DAkazeGetDescriptorType(IntPtr akaze, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_akaze_set_descriptor_type")]
        internal static extern int Features2DAkazeSetDescriptorType(IntPtr akaze, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_akaze_get_descriptor_size")]
        internal static extern int Features2DAkazeGetDescriptorSize(IntPtr akaze, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_akaze_set_descriptor_size")]
        internal static extern int Features2DAkazeSetDescriptorSize(IntPtr akaze, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_akaze_get_descriptor_channels")]
        internal static extern int Features2DAkazeGetDescriptorChannels(IntPtr akaze, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_akaze_set_descriptor_channels")]
        internal static extern int Features2DAkazeSetDescriptorChannels(IntPtr akaze, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_akaze_get_threshold")]
        internal static extern int Features2DAkazeGetThreshold(IntPtr akaze, out double value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_akaze_set_threshold")]
        internal static extern int Features2DAkazeSetThreshold(IntPtr akaze, double value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_akaze_get_n_octaves")]
        internal static extern int Features2DAkazeGetNOctaves(IntPtr akaze, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_akaze_set_n_octaves")]
        internal static extern int Features2DAkazeSetNOctaves(IntPtr akaze, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_akaze_get_n_octave_layers")]
        internal static extern int Features2DAkazeGetNOctaveLayers(IntPtr akaze, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_akaze_set_n_octave_layers")]
        internal static extern int Features2DAkazeSetNOctaveLayers(IntPtr akaze, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_akaze_get_diffusivity")]
        internal static extern int Features2DAkazeGetDiffusivity(IntPtr akaze, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_akaze_set_diffusivity")]
        internal static extern int Features2DAkazeSetDiffusivity(IntPtr akaze, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_akaze_get_max_points")]
        internal static extern int Features2DAkazeGetMaxPoints(IntPtr akaze, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_akaze_set_max_points")]
        internal static extern int Features2DAkazeSetMaxPoints(IntPtr akaze, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_affine_create_from_brisk")]
        internal static extern int Features2DAffineCreateFromBrisk(IntPtr backend, int maxTilt, int minTilt, float tiltStep, float rotateStepBase, out IntPtr affine);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_affine_create_from_kaze")]
        internal static extern int Features2DAffineCreateFromKaze(IntPtr backend, int maxTilt, int minTilt, float tiltStep, float rotateStepBase, out IntPtr affine);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_affine_create_from_akaze")]
        internal static extern int Features2DAffineCreateFromAkaze(IntPtr backend, int maxTilt, int minTilt, float tiltStep, float rotateStepBase, out IntPtr affine);
    }
}
#endif
