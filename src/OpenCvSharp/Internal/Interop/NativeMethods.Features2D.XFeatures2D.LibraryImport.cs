#if NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal static unsafe partial class NativeMethods
    {
        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_brisk_create")]
        internal static partial int Features2DBriskCreate(int threshold, int octaves, float patternScale, out IntPtr brisk);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_brisk_create_pattern")]
        internal static partial int Features2DBriskCreatePattern(float* radiusList, int radiusCount, int* numberList, int numberCount, float dMax, float dMin, int* indexChange, int indexChangeCount, out IntPtr brisk);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_brisk_create_pattern_with_threshold")]
        internal static partial int Features2DBriskCreatePatternWithThreshold(int threshold, int octaves, float* radiusList, int radiusCount, int* numberList, int numberCount, float dMax, float dMin, int* indexChange, int indexChangeCount, out IntPtr brisk);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_brisk_release")]
        internal static partial void Features2DBriskRelease(IntPtr brisk);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_brisk_clear")]
        internal static partial int Features2DBriskClear(IntPtr brisk);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_brisk_empty")]
        internal static partial int Features2DBriskEmpty(IntPtr brisk, out int empty);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_brisk_descriptor_size")]
        internal static partial int Features2DBriskDescriptorSize(IntPtr brisk, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_brisk_descriptor_type")]
        internal static partial int Features2DBriskDescriptorType(IntPtr brisk, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_brisk_default_norm")]
        internal static partial int Features2DBriskDefaultNorm(IntPtr brisk, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_brisk_detect_count")]
        internal static partial int Features2DBriskDetectCount(IntPtr brisk, IntPtr image, IntPtr mask, out int keypointCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_brisk_detect_fill")]
        internal static partial int Features2DBriskDetectFill(IntPtr brisk, IntPtr image, IntPtr mask, NativeKeyPoint* keypoints, int keypointCapacity, out int keypointCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_brisk_compute")]
        internal static partial int Features2DBriskCompute(IntPtr brisk, IntPtr image, NativeKeyPoint* keypointsIn, int keypointCount, NativeKeyPoint* keypointsOut, int keypointCapacity, out int writtenKeypointCount, IntPtr descriptors);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_brisk_detect_and_compute_count")]
        internal static partial int Features2DBriskDetectAndComputeCount(IntPtr brisk, IntPtr image, IntPtr mask, NativeKeyPoint* keypointsIn, int keypointCount, int useProvidedKeypoints, out int outputKeypointCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_brisk_detect_and_compute_fill")]
        internal static partial int Features2DBriskDetectAndComputeFill(IntPtr brisk, IntPtr image, IntPtr mask, NativeKeyPoint* keypointsIn, int keypointCount, int useProvidedKeypoints, NativeKeyPoint* keypointsOut, int keypointCapacity, out int outputKeypointCount, IntPtr descriptors);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_brisk_default_name_length")]
        internal static partial int Features2DBriskDefaultNameLength(IntPtr brisk, out int length);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_brisk_default_name_fill")]
        internal static partial int Features2DBriskDefaultNameFill(IntPtr brisk, byte* buffer, int bufferCapacity, out int written);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_brisk_get_threshold")]
        internal static partial int Features2DBriskGetThreshold(IntPtr brisk, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_brisk_set_threshold")]
        internal static partial int Features2DBriskSetThreshold(IntPtr brisk, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_brisk_get_octaves")]
        internal static partial int Features2DBriskGetOctaves(IntPtr brisk, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_brisk_set_octaves")]
        internal static partial int Features2DBriskSetOctaves(IntPtr brisk, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_brisk_get_pattern_scale")]
        internal static partial int Features2DBriskGetPatternScale(IntPtr brisk, out float value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_brisk_set_pattern_scale")]
        internal static partial int Features2DBriskSetPatternScale(IntPtr brisk, float value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_kaze_create")]
        internal static partial int Features2DKazeCreate(int extended, int upright, float threshold, int nOctaves, int nOctaveLayers, int diffusivity, out IntPtr kaze);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_kaze_release")]
        internal static partial void Features2DKazeRelease(IntPtr kaze);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_kaze_clear")]
        internal static partial int Features2DKazeClear(IntPtr kaze);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_kaze_empty")]
        internal static partial int Features2DKazeEmpty(IntPtr kaze, out int empty);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_kaze_descriptor_size")]
        internal static partial int Features2DKazeDescriptorSize(IntPtr kaze, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_kaze_descriptor_type")]
        internal static partial int Features2DKazeDescriptorType(IntPtr kaze, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_kaze_default_norm")]
        internal static partial int Features2DKazeDefaultNorm(IntPtr kaze, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_kaze_detect_count")]
        internal static partial int Features2DKazeDetectCount(IntPtr kaze, IntPtr image, IntPtr mask, out int keypointCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_kaze_detect_fill")]
        internal static partial int Features2DKazeDetectFill(IntPtr kaze, IntPtr image, IntPtr mask, NativeKeyPoint* keypoints, int keypointCapacity, out int keypointCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_kaze_compute")]
        internal static partial int Features2DKazeCompute(IntPtr kaze, IntPtr image, NativeKeyPoint* keypointsIn, int keypointCount, NativeKeyPoint* keypointsOut, int keypointCapacity, out int writtenKeypointCount, IntPtr descriptors);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_kaze_detect_and_compute_count")]
        internal static partial int Features2DKazeDetectAndComputeCount(IntPtr kaze, IntPtr image, IntPtr mask, NativeKeyPoint* keypointsIn, int keypointCount, int useProvidedKeypoints, out int outputKeypointCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_kaze_detect_and_compute_fill")]
        internal static partial int Features2DKazeDetectAndComputeFill(IntPtr kaze, IntPtr image, IntPtr mask, NativeKeyPoint* keypointsIn, int keypointCount, int useProvidedKeypoints, NativeKeyPoint* keypointsOut, int keypointCapacity, out int outputKeypointCount, IntPtr descriptors);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_kaze_default_name_length")]
        internal static partial int Features2DKazeDefaultNameLength(IntPtr kaze, out int length);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_kaze_default_name_fill")]
        internal static partial int Features2DKazeDefaultNameFill(IntPtr kaze, byte* buffer, int bufferCapacity, out int written);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_kaze_get_extended")]
        internal static partial int Features2DKazeGetExtended(IntPtr kaze, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_kaze_set_extended")]
        internal static partial int Features2DKazeSetExtended(IntPtr kaze, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_kaze_get_upright")]
        internal static partial int Features2DKazeGetUpright(IntPtr kaze, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_kaze_set_upright")]
        internal static partial int Features2DKazeSetUpright(IntPtr kaze, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_kaze_get_threshold")]
        internal static partial int Features2DKazeGetThreshold(IntPtr kaze, out double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_kaze_set_threshold")]
        internal static partial int Features2DKazeSetThreshold(IntPtr kaze, double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_kaze_get_n_octaves")]
        internal static partial int Features2DKazeGetNOctaves(IntPtr kaze, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_kaze_set_n_octaves")]
        internal static partial int Features2DKazeSetNOctaves(IntPtr kaze, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_kaze_get_n_octave_layers")]
        internal static partial int Features2DKazeGetNOctaveLayers(IntPtr kaze, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_kaze_set_n_octave_layers")]
        internal static partial int Features2DKazeSetNOctaveLayers(IntPtr kaze, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_kaze_get_diffusivity")]
        internal static partial int Features2DKazeGetDiffusivity(IntPtr kaze, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_kaze_set_diffusivity")]
        internal static partial int Features2DKazeSetDiffusivity(IntPtr kaze, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_akaze_create")]
        internal static partial int Features2DAkazeCreate(int descriptorType, int descriptorSize, int descriptorChannels, float threshold, int nOctaves, int nOctaveLayers, int diffusivity, int maxPoints, out IntPtr akaze);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_akaze_release")]
        internal static partial void Features2DAkazeRelease(IntPtr akaze);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_akaze_clear")]
        internal static partial int Features2DAkazeClear(IntPtr akaze);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_akaze_empty")]
        internal static partial int Features2DAkazeEmpty(IntPtr akaze, out int empty);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_akaze_descriptor_size")]
        internal static partial int Features2DAkazeDescriptorSize(IntPtr akaze, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_akaze_descriptor_type")]
        internal static partial int Features2DAkazeDescriptorType(IntPtr akaze, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_akaze_default_norm")]
        internal static partial int Features2DAkazeDefaultNorm(IntPtr akaze, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_akaze_detect_count")]
        internal static partial int Features2DAkazeDetectCount(IntPtr akaze, IntPtr image, IntPtr mask, out int keypointCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_akaze_detect_fill")]
        internal static partial int Features2DAkazeDetectFill(IntPtr akaze, IntPtr image, IntPtr mask, NativeKeyPoint* keypoints, int keypointCapacity, out int keypointCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_akaze_compute")]
        internal static partial int Features2DAkazeCompute(IntPtr akaze, IntPtr image, NativeKeyPoint* keypointsIn, int keypointCount, NativeKeyPoint* keypointsOut, int keypointCapacity, out int writtenKeypointCount, IntPtr descriptors);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_akaze_detect_and_compute_count")]
        internal static partial int Features2DAkazeDetectAndComputeCount(IntPtr akaze, IntPtr image, IntPtr mask, NativeKeyPoint* keypointsIn, int keypointCount, int useProvidedKeypoints, out int outputKeypointCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_akaze_detect_and_compute_fill")]
        internal static partial int Features2DAkazeDetectAndComputeFill(IntPtr akaze, IntPtr image, IntPtr mask, NativeKeyPoint* keypointsIn, int keypointCount, int useProvidedKeypoints, NativeKeyPoint* keypointsOut, int keypointCapacity, out int outputKeypointCount, IntPtr descriptors);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_akaze_default_name_length")]
        internal static partial int Features2DAkazeDefaultNameLength(IntPtr akaze, out int length);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_akaze_default_name_fill")]
        internal static partial int Features2DAkazeDefaultNameFill(IntPtr akaze, byte* buffer, int bufferCapacity, out int written);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_akaze_get_descriptor_type")]
        internal static partial int Features2DAkazeGetDescriptorType(IntPtr akaze, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_akaze_set_descriptor_type")]
        internal static partial int Features2DAkazeSetDescriptorType(IntPtr akaze, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_akaze_get_descriptor_size")]
        internal static partial int Features2DAkazeGetDescriptorSize(IntPtr akaze, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_akaze_set_descriptor_size")]
        internal static partial int Features2DAkazeSetDescriptorSize(IntPtr akaze, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_akaze_get_descriptor_channels")]
        internal static partial int Features2DAkazeGetDescriptorChannels(IntPtr akaze, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_akaze_set_descriptor_channels")]
        internal static partial int Features2DAkazeSetDescriptorChannels(IntPtr akaze, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_akaze_get_threshold")]
        internal static partial int Features2DAkazeGetThreshold(IntPtr akaze, out double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_akaze_set_threshold")]
        internal static partial int Features2DAkazeSetThreshold(IntPtr akaze, double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_akaze_get_n_octaves")]
        internal static partial int Features2DAkazeGetNOctaves(IntPtr akaze, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_akaze_set_n_octaves")]
        internal static partial int Features2DAkazeSetNOctaves(IntPtr akaze, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_akaze_get_n_octave_layers")]
        internal static partial int Features2DAkazeGetNOctaveLayers(IntPtr akaze, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_akaze_set_n_octave_layers")]
        internal static partial int Features2DAkazeSetNOctaveLayers(IntPtr akaze, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_akaze_get_diffusivity")]
        internal static partial int Features2DAkazeGetDiffusivity(IntPtr akaze, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_akaze_set_diffusivity")]
        internal static partial int Features2DAkazeSetDiffusivity(IntPtr akaze, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_akaze_get_max_points")]
        internal static partial int Features2DAkazeGetMaxPoints(IntPtr akaze, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_akaze_set_max_points")]
        internal static partial int Features2DAkazeSetMaxPoints(IntPtr akaze, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_affine_create_from_brisk")]
        internal static partial int Features2DAffineCreateFromBrisk(IntPtr backend, int maxTilt, int minTilt, float tiltStep, float rotateStepBase, out IntPtr affine);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_affine_create_from_kaze")]
        internal static partial int Features2DAffineCreateFromKaze(IntPtr backend, int maxTilt, int minTilt, float tiltStep, float rotateStepBase, out IntPtr affine);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_features2d_affine_create_from_akaze")]
        internal static partial int Features2DAffineCreateFromAkaze(IntPtr backend, int maxTilt, int minTilt, float tiltStep, float rotateStepBase, out IntPtr affine);
    }
}
#endif
