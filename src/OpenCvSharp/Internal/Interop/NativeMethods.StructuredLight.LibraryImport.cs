#if NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    internal static unsafe partial class NativeMethods
    {
        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_structured_light_gray_code_pattern_create")]
        internal static partial int StructuredLightGrayCodePatternCreate(int width, int height, out IntPtr pattern);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_structured_light_sinusoidal_pattern_create")]
        internal static partial int StructuredLightSinusoidalPatternCreate(int width, int height, int nbrOfPeriods, float shiftValue, int methodId, int nbrOfPixelsBetweenMarkers, int horizontal, int setMarkers, NativeStructuredLightPoint2f* markers, int markerCount, out IntPtr pattern);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_structured_light_pattern_release")]
        internal static partial void StructuredLightPatternRelease(IntPtr pattern);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_structured_light_pattern_generate_count")]
        internal static partial int StructuredLightPatternGenerateCount(IntPtr pattern, out int imageCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_structured_light_pattern_generate_fill")]
        internal static partial int StructuredLightPatternGenerateFill(IntPtr pattern, IntPtr* images, int imageCapacity, out int imageCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_structured_light_gray_code_pattern_get_number_of_pattern_images")]
        internal static partial int StructuredLightGrayCodePatternGetNumberOfPatternImages(IntPtr pattern, out int imageCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_structured_light_gray_code_pattern_set_white_threshold")]
        internal static partial int StructuredLightGrayCodePatternSetWhiteThreshold(IntPtr pattern, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_structured_light_gray_code_pattern_set_black_threshold")]
        internal static partial int StructuredLightGrayCodePatternSetBlackThreshold(IntPtr pattern, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_structured_light_gray_code_pattern_get_images_for_shadow_masks")]
        internal static partial int StructuredLightGrayCodePatternGetImagesForShadowMasks(IntPtr pattern, IntPtr blackImage, IntPtr whiteImage);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_structured_light_gray_code_pattern_get_proj_pixel")]
        internal static partial int StructuredLightGrayCodePatternGetProjPixel(IntPtr pattern, IntPtr* patternImages, int imageCount, int x, int y, out int found, out int projX, out int projY);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_structured_light_sinusoidal_pattern_compute_phase_map")]
        internal static partial int StructuredLightSinusoidalPatternComputePhaseMap(IntPtr pattern, IntPtr* patternImages, int imageCount, IntPtr wrappedPhaseMap, IntPtr shadowMask, IntPtr fundamental);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_structured_light_sinusoidal_pattern_unwrap_phase_map")]
        internal static partial int StructuredLightSinusoidalPatternUnwrapPhaseMap(IntPtr pattern, IntPtr wrappedPhaseMap, IntPtr unwrappedPhaseMap, int camWidth, int camHeight, IntPtr shadowMask);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_structured_light_sinusoidal_pattern_compute_data_modulation_term")]
        internal static partial int StructuredLightSinusoidalPatternComputeDataModulationTerm(IntPtr pattern, IntPtr* patternImages, int imageCount, IntPtr dataModulationTerm, IntPtr shadowMask);
    }
}
#endif
