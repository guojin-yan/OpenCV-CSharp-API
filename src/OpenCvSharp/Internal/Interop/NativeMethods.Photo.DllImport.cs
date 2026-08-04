#if !NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    internal static unsafe partial class NativeMethods
    {
        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_decolor")]
        internal static extern int PhotoDecolor(IntPtr src, IntPtr grayscale, IntPtr colorBoost);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_inpaint")]
        internal static extern int PhotoInpaint(IntPtr src, IntPtr inpaintMask, IntPtr dst, double inpaintRadius, int flags);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_fast_nl_means_denoising")]
        internal static extern int PhotoFastNlMeansDenoising(IntPtr src, IntPtr dst, float h, int templateWindowSize, int searchWindowSize);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_fast_nl_means_denoising_with_h_array")]
        internal static extern int PhotoFastNlMeansDenoisingWithHArray(IntPtr src, IntPtr dst, float[] h, int hCount, int templateWindowSize, int searchWindowSize, int normType);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_fast_nl_means_denoising_with_h_array")]
        internal static extern int PhotoFastNlMeansDenoisingWithHArray(IntPtr src, IntPtr dst, float* h, int hCount, int templateWindowSize, int searchWindowSize, int normType);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_fast_nl_means_denoising_colored")]
        internal static extern int PhotoFastNlMeansDenoisingColored(IntPtr src, IntPtr dst, float h, float hColor, int templateWindowSize, int searchWindowSize);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_fast_nl_means_denoising_multi")]
        internal static extern int PhotoFastNlMeansDenoisingMulti(IntPtr[] srcImages, int imageCount, IntPtr dst, int imgToDenoiseIndex, int temporalWindowSize, float h, int templateWindowSize, int searchWindowSize);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_fast_nl_means_denoising_multi_with_h_array")]
        internal static extern int PhotoFastNlMeansDenoisingMultiWithHArray(IntPtr[] srcImages, int imageCount, IntPtr dst, int imgToDenoiseIndex, int temporalWindowSize, float[] h, int hCount, int templateWindowSize, int searchWindowSize, int normType);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_fast_nl_means_denoising_multi_with_h_array")]
        internal static extern int PhotoFastNlMeansDenoisingMultiWithHArray(IntPtr[] srcImages, int imageCount, IntPtr dst, int imgToDenoiseIndex, int temporalWindowSize, float* h, int hCount, int templateWindowSize, int searchWindowSize, int normType);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_fast_nl_means_denoising_colored_multi")]
        internal static extern int PhotoFastNlMeansDenoisingColoredMulti(IntPtr[] srcImages, int imageCount, IntPtr dst, int imgToDenoiseIndex, int temporalWindowSize, float h, float hColor, int templateWindowSize, int searchWindowSize);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_denoise_tvl1")]
        internal static extern int PhotoDenoiseTvl1(IntPtr[] observations, int observationCount, IntPtr result, double lambda, int niters);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_correct_chromatic_aberration")]
        internal static extern int PhotoCorrectChromaticAberration(IntPtr inputImage, IntPtr coefficients, IntPtr outputImage, int calibrationWidth, int calibrationHeight, int calibrationDegree, int bayerPattern);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_load_chromatic_aberration_params")]
        internal static extern int PhotoLoadChromaticAberrationParams(IntPtr node, IntPtr coefficients, out int calibrationWidth, out int calibrationHeight, out int degree);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_seamless_clone")]
        internal static extern int PhotoSeamlessClone(IntPtr src, IntPtr dst, IntPtr mask, int pointX, int pointY, IntPtr blend, int flags);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_color_change")]
        internal static extern int PhotoColorChange(IntPtr src, IntPtr mask, IntPtr dst, float redMul, float greenMul, float blueMul);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_illumination_change")]
        internal static extern int PhotoIlluminationChange(IntPtr src, IntPtr mask, IntPtr dst, float alpha, float beta);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_texture_flattening")]
        internal static extern int PhotoTextureFlattening(IntPtr src, IntPtr mask, IntPtr dst, float lowThreshold, float highThreshold, int kernelSize);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_edge_preserving_filter")]
        internal static extern int PhotoEdgePreservingFilter(IntPtr src, IntPtr dst, int flags, float sigmaS, float sigmaR);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_detail_enhance")]
        internal static extern int PhotoDetailEnhance(IntPtr src, IntPtr dst, float sigmaS, float sigmaR);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_pencil_sketch")]
        internal static extern int PhotoPencilSketch(IntPtr src, IntPtr dst1, IntPtr dst2, float sigmaS, float sigmaR, float shadeFactor);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_stylization")]
        internal static extern int PhotoStylization(IntPtr src, IntPtr dst, float sigmaS, float sigmaR);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tonemap_create")]
        internal static extern int TonemapCreate(float gamma, out IntPtr tonemap);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tonemap_drago_create")]
        internal static extern int TonemapDragoCreate(float gamma, float saturation, float bias, out IntPtr tonemap);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tonemap_reinhard_create")]
        internal static extern int TonemapReinhardCreate(float gamma, float intensity, float lightAdapt, float colorAdapt, out IntPtr tonemap);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tonemap_mantiuk_create")]
        internal static extern int TonemapMantiukCreate(float gamma, float scale, float saturation, out IntPtr tonemap);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tonemap_release_handle")]
        internal static extern void TonemapReleaseHandle(IntPtr tonemap);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tonemap_process")]
        internal static extern int TonemapProcess(IntPtr tonemap, IntPtr src, IntPtr dst);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tonemap_get_gamma")]
        internal static extern int TonemapGetGamma(IntPtr tonemap, out float gamma);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tonemap_set_gamma")]
        internal static extern int TonemapSetGamma(IntPtr tonemap, float gamma);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tonemap_drago_get_saturation")]
        internal static extern int TonemapDragoGetSaturation(IntPtr tonemap, out float saturation);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tonemap_drago_set_saturation")]
        internal static extern int TonemapDragoSetSaturation(IntPtr tonemap, float saturation);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tonemap_drago_get_bias")]
        internal static extern int TonemapDragoGetBias(IntPtr tonemap, out float bias);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tonemap_drago_set_bias")]
        internal static extern int TonemapDragoSetBias(IntPtr tonemap, float bias);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tonemap_reinhard_get_intensity")]
        internal static extern int TonemapReinhardGetIntensity(IntPtr tonemap, out float intensity);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tonemap_reinhard_set_intensity")]
        internal static extern int TonemapReinhardSetIntensity(IntPtr tonemap, float intensity);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tonemap_reinhard_get_light_adaptation")]
        internal static extern int TonemapReinhardGetLightAdaptation(IntPtr tonemap, out float lightAdapt);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tonemap_reinhard_set_light_adaptation")]
        internal static extern int TonemapReinhardSetLightAdaptation(IntPtr tonemap, float lightAdapt);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tonemap_reinhard_get_color_adaptation")]
        internal static extern int TonemapReinhardGetColorAdaptation(IntPtr tonemap, out float colorAdapt);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tonemap_reinhard_set_color_adaptation")]
        internal static extern int TonemapReinhardSetColorAdaptation(IntPtr tonemap, float colorAdapt);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tonemap_mantiuk_get_scale")]
        internal static extern int TonemapMantiukGetScale(IntPtr tonemap, out float scale);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tonemap_mantiuk_set_scale")]
        internal static extern int TonemapMantiukSetScale(IntPtr tonemap, float scale);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tonemap_mantiuk_get_saturation")]
        internal static extern int TonemapMantiukGetSaturation(IntPtr tonemap, out float saturation);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tonemap_mantiuk_set_saturation")]
        internal static extern int TonemapMantiukSetSaturation(IntPtr tonemap, float saturation);
    }
}
#endif
