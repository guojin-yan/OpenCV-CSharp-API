#if NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    internal static partial class NativeMethods
    {
        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bgsegm_background_subtractor_release_handle")]
        internal static partial void BgSegmBackgroundSubtractorReleaseHandle(IntPtr subtractor);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bgsegm_background_subtractor_apply")]
        internal static partial int BgSegmBackgroundSubtractorApply(IntPtr subtractor, IntPtr image, IntPtr fgmask, double learningRate);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bgsegm_background_subtractor_apply_with_known_foreground")]
        internal static partial int BgSegmBackgroundSubtractorApplyWithKnownForeground(IntPtr subtractor, IntPtr image, IntPtr knownForegroundMask, IntPtr fgmask, double learningRate);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bgsegm_background_subtractor_get_background_image")]
        internal static partial int BgSegmBackgroundSubtractorGetBackgroundImage(IntPtr subtractor, IntPtr backgroundImage);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bgsegm_background_subtractor_mog_create")]
        internal static partial int BgSegmBackgroundSubtractorMOGCreate(int history, int nmixtures, double backgroundRatio, double noiseSigma, out IntPtr subtractor);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bgsegm_background_subtractor_mog_get_int")]
        internal static partial int BgSegmBackgroundSubtractorMOGGetInt(IntPtr subtractor, int propertyId, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bgsegm_background_subtractor_mog_set_int")]
        internal static partial int BgSegmBackgroundSubtractorMOGSetInt(IntPtr subtractor, int propertyId, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bgsegm_background_subtractor_mog_get_double")]
        internal static partial int BgSegmBackgroundSubtractorMOGGetDouble(IntPtr subtractor, int propertyId, out double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bgsegm_background_subtractor_mog_set_double")]
        internal static partial int BgSegmBackgroundSubtractorMOGSetDouble(IntPtr subtractor, int propertyId, double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bgsegm_background_subtractor_gmg_create")]
        internal static partial int BgSegmBackgroundSubtractorGMGCreate(int initializationFrames, double decisionThreshold, out IntPtr subtractor);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bgsegm_background_subtractor_gmg_get_int")]
        internal static partial int BgSegmBackgroundSubtractorGMGGetInt(IntPtr subtractor, int propertyId, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bgsegm_background_subtractor_gmg_set_int")]
        internal static partial int BgSegmBackgroundSubtractorGMGSetInt(IntPtr subtractor, int propertyId, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bgsegm_background_subtractor_gmg_get_double")]
        internal static partial int BgSegmBackgroundSubtractorGMGGetDouble(IntPtr subtractor, int propertyId, out double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bgsegm_background_subtractor_gmg_set_double")]
        internal static partial int BgSegmBackgroundSubtractorGMGSetDouble(IntPtr subtractor, int propertyId, double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bgsegm_background_subtractor_cnt_create")]
        internal static partial int BgSegmBackgroundSubtractorCNTCreate(int minPixelStability, int useHistory, int maxPixelStability, int isParallel, out IntPtr subtractor);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bgsegm_background_subtractor_cnt_get_int")]
        internal static partial int BgSegmBackgroundSubtractorCNTGetInt(IntPtr subtractor, int propertyId, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bgsegm_background_subtractor_cnt_set_int")]
        internal static partial int BgSegmBackgroundSubtractorCNTSetInt(IntPtr subtractor, int propertyId, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bgsegm_synthetic_sequence_generator_create")]
        internal static partial int BgSegmSyntheticSequenceGeneratorCreate(IntPtr background, IntPtr objectMat, double amplitude, double wavelength, double wavespeed, double objspeed, out IntPtr generator);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bgsegm_synthetic_sequence_generator_release_handle")]
        internal static partial void BgSegmSyntheticSequenceGeneratorReleaseHandle(IntPtr generator);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bgsegm_synthetic_sequence_generator_get_next_frame")]
        internal static partial int BgSegmSyntheticSequenceGeneratorGetNextFrame(IntPtr generator, IntPtr frame, IntPtr gtMask);
    }
}
#endif
