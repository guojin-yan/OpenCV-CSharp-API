#if !NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    internal static unsafe partial class NativeMethods
    {
        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bioinspired_retina_create")]
        internal static extern int BioInspiredRetinaCreate(int width, int height, int colorMode, int colorSamplingMethod, int useRetinaLogSampling, float reductionFactor, float samplingStrength, out IntPtr retina);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bioinspired_retina_release")]
        internal static extern void BioInspiredRetinaRelease(IntPtr retina);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bioinspired_retina_get_input_size")]
        internal static extern int BioInspiredRetinaGetInputSize(IntPtr retina, out int width, out int height);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bioinspired_retina_get_output_size")]
        internal static extern int BioInspiredRetinaGetOutputSize(IntPtr retina, out int width, out int height);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bioinspired_retina_setup")]
        internal static extern int BioInspiredRetinaSetup(IntPtr retina, byte[] retinaParameterFile, int applyDefaultSetupOnFailure);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bioinspired_retina_setup_parvo")]
        internal static extern int BioInspiredRetinaSetupParvo(IntPtr retina, ref NativeBioInspiredRetinaParvoParameters parameters);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bioinspired_retina_setup_magno")]
        internal static extern int BioInspiredRetinaSetupMagno(IntPtr retina, ref NativeBioInspiredRetinaMagnoParameters parameters);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bioinspired_retina_get_parameters")]
        internal static extern int BioInspiredRetinaGetParameters(IntPtr retina, out NativeBioInspiredRetinaParvoParameters parvo, out NativeBioInspiredRetinaMagnoParameters magno);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bioinspired_retina_run")]
        internal static extern int BioInspiredRetinaRun(IntPtr retina, IntPtr input);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bioinspired_retina_apply_fast_tone_mapping")]
        internal static extern int BioInspiredRetinaApplyFastToneMapping(IntPtr retina, IntPtr input, IntPtr output);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bioinspired_retina_get_parvo")]
        internal static extern int BioInspiredRetinaGetParvo(IntPtr retina, IntPtr output);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bioinspired_retina_get_parvo_raw")]
        internal static extern int BioInspiredRetinaGetParvoRaw(IntPtr retina, IntPtr output);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bioinspired_retina_get_magno")]
        internal static extern int BioInspiredRetinaGetMagno(IntPtr retina, IntPtr output);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bioinspired_retina_get_magno_raw")]
        internal static extern int BioInspiredRetinaGetMagnoRaw(IntPtr retina, IntPtr output);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bioinspired_retina_set_color_saturation")]
        internal static extern int BioInspiredRetinaSetColorSaturation(IntPtr retina, int saturateColors, float colorSaturationValue);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bioinspired_retina_clear_buffers")]
        internal static extern int BioInspiredRetinaClearBuffers(IntPtr retina);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bioinspired_retina_activate_moving_contours_processing")]
        internal static extern int BioInspiredRetinaActivateMovingContoursProcessing(IntPtr retina, int activate);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bioinspired_retina_activate_contours_processing")]
        internal static extern int BioInspiredRetinaActivateContoursProcessing(IntPtr retina, int activate);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bioinspired_retina_print_setup_length")]
        internal static extern int BioInspiredRetinaPrintSetupLength(IntPtr retina, out int length);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bioinspired_retina_print_setup_fill")]
        internal static extern int BioInspiredRetinaPrintSetupFill(IntPtr retina, byte* buffer, int bufferCapacity, out int written);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bioinspired_retina_write")]
        internal static extern int BioInspiredRetinaWrite(IntPtr retina, byte[] path);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bioinspired_retina_fast_tone_mapping_create")]
        internal static extern int BioInspiredRetinaFastToneMappingCreate(int width, int height, out IntPtr toneMapping);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bioinspired_retina_fast_tone_mapping_release")]
        internal static extern void BioInspiredRetinaFastToneMappingRelease(IntPtr toneMapping);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bioinspired_retina_fast_tone_mapping_setup")]
        internal static extern int BioInspiredRetinaFastToneMappingSetup(IntPtr toneMapping, float photoreceptorsNeighborhoodRadius, float ganglionCellsNeighborhoodRadius, float meanLuminanceModulatorK);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bioinspired_retina_fast_tone_mapping_apply")]
        internal static extern int BioInspiredRetinaFastToneMappingApply(IntPtr toneMapping, IntPtr input, IntPtr output);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bioinspired_transient_areas_create")]
        internal static extern int BioInspiredTransientAreasCreate(int width, int height, out IntPtr segmentation);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bioinspired_transient_areas_release")]
        internal static extern void BioInspiredTransientAreasRelease(IntPtr segmentation);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bioinspired_transient_areas_get_size")]
        internal static extern int BioInspiredTransientAreasGetSize(IntPtr segmentation, out int width, out int height);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bioinspired_transient_areas_setup")]
        internal static extern int BioInspiredTransientAreasSetup(IntPtr segmentation, byte[] segmentationParameterFile, int applyDefaultSetupOnFailure);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bioinspired_transient_areas_setup_parameters")]
        internal static extern int BioInspiredTransientAreasSetupParameters(IntPtr segmentation, ref NativeBioInspiredSegmentationParameters parameters);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bioinspired_transient_areas_get_parameters")]
        internal static extern int BioInspiredTransientAreasGetParameters(IntPtr segmentation, out NativeBioInspiredSegmentationParameters parameters);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bioinspired_transient_areas_run")]
        internal static extern int BioInspiredTransientAreasRun(IntPtr segmentation, IntPtr input, int channelIndex);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bioinspired_transient_areas_get_segmentation_picture")]
        internal static extern int BioInspiredTransientAreasGetSegmentationPicture(IntPtr segmentation, IntPtr output);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bioinspired_transient_areas_clear_all_buffers")]
        internal static extern int BioInspiredTransientAreasClearAllBuffers(IntPtr segmentation);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bioinspired_transient_areas_print_setup_length")]
        internal static extern int BioInspiredTransientAreasPrintSetupLength(IntPtr segmentation, out int length);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bioinspired_transient_areas_print_setup_fill")]
        internal static extern int BioInspiredTransientAreasPrintSetupFill(IntPtr segmentation, byte* buffer, int bufferCapacity, out int written);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bioinspired_transient_areas_write")]
        internal static extern int BioInspiredTransientAreasWrite(IntPtr segmentation, byte[] path);
    }
}
#endif
