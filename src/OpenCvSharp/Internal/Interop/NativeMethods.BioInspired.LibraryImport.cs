#if NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    internal static unsafe partial class NativeMethods
    {
        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bioinspired_retina_create")]
        internal static partial int BioInspiredRetinaCreate(int width, int height, int colorMode, int colorSamplingMethod, int useRetinaLogSampling, float reductionFactor, float samplingStrength, out IntPtr retina);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bioinspired_retina_release")]
        internal static partial void BioInspiredRetinaRelease(IntPtr retina);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bioinspired_retina_get_input_size")]
        internal static partial int BioInspiredRetinaGetInputSize(IntPtr retina, out int width, out int height);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bioinspired_retina_get_output_size")]
        internal static partial int BioInspiredRetinaGetOutputSize(IntPtr retina, out int width, out int height);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bioinspired_retina_setup")]
        internal static partial int BioInspiredRetinaSetup(IntPtr retina, byte[] retinaParameterFile, int applyDefaultSetupOnFailure);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bioinspired_retina_setup_parvo")]
        internal static partial int BioInspiredRetinaSetupParvo(IntPtr retina, ref NativeBioInspiredRetinaParvoParameters parameters);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bioinspired_retina_setup_magno")]
        internal static partial int BioInspiredRetinaSetupMagno(IntPtr retina, ref NativeBioInspiredRetinaMagnoParameters parameters);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bioinspired_retina_get_parameters")]
        internal static partial int BioInspiredRetinaGetParameters(IntPtr retina, out NativeBioInspiredRetinaParvoParameters parvo, out NativeBioInspiredRetinaMagnoParameters magno);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bioinspired_retina_run")]
        internal static partial int BioInspiredRetinaRun(IntPtr retina, IntPtr input);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bioinspired_retina_apply_fast_tone_mapping")]
        internal static partial int BioInspiredRetinaApplyFastToneMapping(IntPtr retina, IntPtr input, IntPtr output);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bioinspired_retina_get_parvo")]
        internal static partial int BioInspiredRetinaGetParvo(IntPtr retina, IntPtr output);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bioinspired_retina_get_parvo_raw")]
        internal static partial int BioInspiredRetinaGetParvoRaw(IntPtr retina, IntPtr output);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bioinspired_retina_get_magno")]
        internal static partial int BioInspiredRetinaGetMagno(IntPtr retina, IntPtr output);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bioinspired_retina_get_magno_raw")]
        internal static partial int BioInspiredRetinaGetMagnoRaw(IntPtr retina, IntPtr output);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bioinspired_retina_set_color_saturation")]
        internal static partial int BioInspiredRetinaSetColorSaturation(IntPtr retina, int saturateColors, float colorSaturationValue);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bioinspired_retina_clear_buffers")]
        internal static partial int BioInspiredRetinaClearBuffers(IntPtr retina);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bioinspired_retina_activate_moving_contours_processing")]
        internal static partial int BioInspiredRetinaActivateMovingContoursProcessing(IntPtr retina, int activate);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bioinspired_retina_activate_contours_processing")]
        internal static partial int BioInspiredRetinaActivateContoursProcessing(IntPtr retina, int activate);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bioinspired_retina_print_setup_length")]
        internal static partial int BioInspiredRetinaPrintSetupLength(IntPtr retina, out int length);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bioinspired_retina_print_setup_fill")]
        internal static partial int BioInspiredRetinaPrintSetupFill(IntPtr retina, byte* buffer, int bufferCapacity, out int written);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bioinspired_retina_write")]
        internal static partial int BioInspiredRetinaWrite(IntPtr retina, byte[] path);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bioinspired_retina_fast_tone_mapping_create")]
        internal static partial int BioInspiredRetinaFastToneMappingCreate(int width, int height, out IntPtr toneMapping);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bioinspired_retina_fast_tone_mapping_release")]
        internal static partial void BioInspiredRetinaFastToneMappingRelease(IntPtr toneMapping);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bioinspired_retina_fast_tone_mapping_setup")]
        internal static partial int BioInspiredRetinaFastToneMappingSetup(IntPtr toneMapping, float photoreceptorsNeighborhoodRadius, float ganglionCellsNeighborhoodRadius, float meanLuminanceModulatorK);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bioinspired_retina_fast_tone_mapping_apply")]
        internal static partial int BioInspiredRetinaFastToneMappingApply(IntPtr toneMapping, IntPtr input, IntPtr output);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bioinspired_transient_areas_create")]
        internal static partial int BioInspiredTransientAreasCreate(int width, int height, out IntPtr segmentation);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bioinspired_transient_areas_release")]
        internal static partial void BioInspiredTransientAreasRelease(IntPtr segmentation);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bioinspired_transient_areas_get_size")]
        internal static partial int BioInspiredTransientAreasGetSize(IntPtr segmentation, out int width, out int height);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bioinspired_transient_areas_setup")]
        internal static partial int BioInspiredTransientAreasSetup(IntPtr segmentation, byte[] segmentationParameterFile, int applyDefaultSetupOnFailure);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bioinspired_transient_areas_setup_parameters")]
        internal static partial int BioInspiredTransientAreasSetupParameters(IntPtr segmentation, ref NativeBioInspiredSegmentationParameters parameters);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bioinspired_transient_areas_get_parameters")]
        internal static partial int BioInspiredTransientAreasGetParameters(IntPtr segmentation, out NativeBioInspiredSegmentationParameters parameters);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bioinspired_transient_areas_run")]
        internal static partial int BioInspiredTransientAreasRun(IntPtr segmentation, IntPtr input, int channelIndex);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bioinspired_transient_areas_get_segmentation_picture")]
        internal static partial int BioInspiredTransientAreasGetSegmentationPicture(IntPtr segmentation, IntPtr output);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bioinspired_transient_areas_clear_all_buffers")]
        internal static partial int BioInspiredTransientAreasClearAllBuffers(IntPtr segmentation);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bioinspired_transient_areas_print_setup_length")]
        internal static partial int BioInspiredTransientAreasPrintSetupLength(IntPtr segmentation, out int length);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bioinspired_transient_areas_print_setup_fill")]
        internal static partial int BioInspiredTransientAreasPrintSetupFill(IntPtr segmentation, byte* buffer, int bufferCapacity, out int written);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bioinspired_transient_areas_write")]
        internal static partial int BioInspiredTransientAreasWrite(IntPtr segmentation, byte[] path);
    }
}
#endif
