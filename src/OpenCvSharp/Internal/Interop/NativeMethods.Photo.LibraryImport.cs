#if NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    internal static unsafe partial class NativeMethods
    {
        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_decolor")]
        internal static partial int PhotoDecolor(IntPtr src, IntPtr grayscale, IntPtr colorBoost);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_inpaint")]
        internal static partial int PhotoInpaint(IntPtr src, IntPtr inpaintMask, IntPtr dst, double inpaintRadius, int flags);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_fast_nl_means_denoising")]
        internal static partial int PhotoFastNlMeansDenoising(IntPtr src, IntPtr dst, float h, int templateWindowSize, int searchWindowSize);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_fast_nl_means_denoising_with_h_array")]
        internal static partial int PhotoFastNlMeansDenoisingWithHArray(IntPtr src, IntPtr dst, float[] h, int hCount, int templateWindowSize, int searchWindowSize, int normType);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_fast_nl_means_denoising_with_h_array")]
        internal static partial int PhotoFastNlMeansDenoisingWithHArray(IntPtr src, IntPtr dst, float* h, int hCount, int templateWindowSize, int searchWindowSize, int normType);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_fast_nl_means_denoising_colored")]
        internal static partial int PhotoFastNlMeansDenoisingColored(IntPtr src, IntPtr dst, float h, float hColor, int templateWindowSize, int searchWindowSize);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_fast_nl_means_denoising_multi")]
        internal static partial int PhotoFastNlMeansDenoisingMulti(IntPtr[] srcImages, int imageCount, IntPtr dst, int imgToDenoiseIndex, int temporalWindowSize, float h, int templateWindowSize, int searchWindowSize);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_fast_nl_means_denoising_multi_with_h_array")]
        internal static partial int PhotoFastNlMeansDenoisingMultiWithHArray(IntPtr[] srcImages, int imageCount, IntPtr dst, int imgToDenoiseIndex, int temporalWindowSize, float[] h, int hCount, int templateWindowSize, int searchWindowSize, int normType);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_fast_nl_means_denoising_multi_with_h_array")]
        internal static partial int PhotoFastNlMeansDenoisingMultiWithHArray(IntPtr[] srcImages, int imageCount, IntPtr dst, int imgToDenoiseIndex, int temporalWindowSize, float* h, int hCount, int templateWindowSize, int searchWindowSize, int normType);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_fast_nl_means_denoising_colored_multi")]
        internal static partial int PhotoFastNlMeansDenoisingColoredMulti(IntPtr[] srcImages, int imageCount, IntPtr dst, int imgToDenoiseIndex, int temporalWindowSize, float h, float hColor, int templateWindowSize, int searchWindowSize);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_denoise_tvl1")]
        internal static partial int PhotoDenoiseTvl1(IntPtr[] observations, int observationCount, IntPtr result, double lambda, int niters);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_correct_chromatic_aberration")]
        internal static partial int PhotoCorrectChromaticAberration(IntPtr inputImage, IntPtr coefficients, IntPtr outputImage, int calibrationWidth, int calibrationHeight, int calibrationDegree, int bayerPattern);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_load_chromatic_aberration_params")]
        internal static partial int PhotoLoadChromaticAberrationParams(IntPtr node, IntPtr coefficients, out int calibrationWidth, out int calibrationHeight, out int degree);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_seamless_clone")]
        internal static partial int PhotoSeamlessClone(IntPtr src, IntPtr dst, IntPtr mask, int pointX, int pointY, IntPtr blend, int flags);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_color_change")]
        internal static partial int PhotoColorChange(IntPtr src, IntPtr mask, IntPtr dst, float redMul, float greenMul, float blueMul);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_illumination_change")]
        internal static partial int PhotoIlluminationChange(IntPtr src, IntPtr mask, IntPtr dst, float alpha, float beta);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_texture_flattening")]
        internal static partial int PhotoTextureFlattening(IntPtr src, IntPtr mask, IntPtr dst, float lowThreshold, float highThreshold, int kernelSize);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_edge_preserving_filter")]
        internal static partial int PhotoEdgePreservingFilter(IntPtr src, IntPtr dst, int flags, float sigmaS, float sigmaR);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_detail_enhance")]
        internal static partial int PhotoDetailEnhance(IntPtr src, IntPtr dst, float sigmaS, float sigmaR);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_pencil_sketch")]
        internal static partial int PhotoPencilSketch(IntPtr src, IntPtr dst1, IntPtr dst2, float sigmaS, float sigmaR, float shadeFactor);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_stylization")]
        internal static partial int PhotoStylization(IntPtr src, IntPtr dst, float sigmaS, float sigmaR);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tonemap_create")]
        internal static partial int TonemapCreate(float gamma, out IntPtr tonemap);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tonemap_drago_create")]
        internal static partial int TonemapDragoCreate(float gamma, float saturation, float bias, out IntPtr tonemap);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tonemap_reinhard_create")]
        internal static partial int TonemapReinhardCreate(float gamma, float intensity, float lightAdapt, float colorAdapt, out IntPtr tonemap);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tonemap_mantiuk_create")]
        internal static partial int TonemapMantiukCreate(float gamma, float scale, float saturation, out IntPtr tonemap);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tonemap_release_handle")]
        internal static partial void TonemapReleaseHandle(IntPtr tonemap);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tonemap_process")]
        internal static partial int TonemapProcess(IntPtr tonemap, IntPtr src, IntPtr dst);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tonemap_get_gamma")]
        internal static partial int TonemapGetGamma(IntPtr tonemap, out float gamma);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tonemap_set_gamma")]
        internal static partial int TonemapSetGamma(IntPtr tonemap, float gamma);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tonemap_drago_get_saturation")]
        internal static partial int TonemapDragoGetSaturation(IntPtr tonemap, out float saturation);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tonemap_drago_set_saturation")]
        internal static partial int TonemapDragoSetSaturation(IntPtr tonemap, float saturation);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tonemap_drago_get_bias")]
        internal static partial int TonemapDragoGetBias(IntPtr tonemap, out float bias);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tonemap_drago_set_bias")]
        internal static partial int TonemapDragoSetBias(IntPtr tonemap, float bias);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tonemap_reinhard_get_intensity")]
        internal static partial int TonemapReinhardGetIntensity(IntPtr tonemap, out float intensity);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tonemap_reinhard_set_intensity")]
        internal static partial int TonemapReinhardSetIntensity(IntPtr tonemap, float intensity);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tonemap_reinhard_get_light_adaptation")]
        internal static partial int TonemapReinhardGetLightAdaptation(IntPtr tonemap, out float lightAdapt);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tonemap_reinhard_set_light_adaptation")]
        internal static partial int TonemapReinhardSetLightAdaptation(IntPtr tonemap, float lightAdapt);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tonemap_reinhard_get_color_adaptation")]
        internal static partial int TonemapReinhardGetColorAdaptation(IntPtr tonemap, out float colorAdapt);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tonemap_reinhard_set_color_adaptation")]
        internal static partial int TonemapReinhardSetColorAdaptation(IntPtr tonemap, float colorAdapt);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tonemap_mantiuk_get_scale")]
        internal static partial int TonemapMantiukGetScale(IntPtr tonemap, out float scale);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tonemap_mantiuk_set_scale")]
        internal static partial int TonemapMantiukSetScale(IntPtr tonemap, float scale);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tonemap_mantiuk_get_saturation")]
        internal static partial int TonemapMantiukGetSaturation(IntPtr tonemap, out float saturation);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tonemap_mantiuk_set_saturation")]
        internal static partial int TonemapMantiukSetSaturation(IntPtr tonemap, float saturation);
    }
}
#endif
