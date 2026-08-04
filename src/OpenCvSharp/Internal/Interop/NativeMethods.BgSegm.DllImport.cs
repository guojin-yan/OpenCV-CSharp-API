#if !NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    internal static partial class NativeMethods
    {
        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bgsegm_background_subtractor_release_handle")]
        internal static extern void BgSegmBackgroundSubtractorReleaseHandle(IntPtr subtractor);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bgsegm_background_subtractor_apply")]
        internal static extern int BgSegmBackgroundSubtractorApply(IntPtr subtractor, IntPtr image, IntPtr fgmask, double learningRate);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bgsegm_background_subtractor_apply_with_known_foreground")]
        internal static extern int BgSegmBackgroundSubtractorApplyWithKnownForeground(IntPtr subtractor, IntPtr image, IntPtr knownForegroundMask, IntPtr fgmask, double learningRate);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bgsegm_background_subtractor_get_background_image")]
        internal static extern int BgSegmBackgroundSubtractorGetBackgroundImage(IntPtr subtractor, IntPtr backgroundImage);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bgsegm_background_subtractor_mog_create")]
        internal static extern int BgSegmBackgroundSubtractorMOGCreate(int history, int nmixtures, double backgroundRatio, double noiseSigma, out IntPtr subtractor);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bgsegm_background_subtractor_mog_get_int")]
        internal static extern int BgSegmBackgroundSubtractorMOGGetInt(IntPtr subtractor, int propertyId, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bgsegm_background_subtractor_mog_set_int")]
        internal static extern int BgSegmBackgroundSubtractorMOGSetInt(IntPtr subtractor, int propertyId, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bgsegm_background_subtractor_mog_get_double")]
        internal static extern int BgSegmBackgroundSubtractorMOGGetDouble(IntPtr subtractor, int propertyId, out double value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bgsegm_background_subtractor_mog_set_double")]
        internal static extern int BgSegmBackgroundSubtractorMOGSetDouble(IntPtr subtractor, int propertyId, double value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bgsegm_background_subtractor_gmg_create")]
        internal static extern int BgSegmBackgroundSubtractorGMGCreate(int initializationFrames, double decisionThreshold, out IntPtr subtractor);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bgsegm_background_subtractor_gmg_get_int")]
        internal static extern int BgSegmBackgroundSubtractorGMGGetInt(IntPtr subtractor, int propertyId, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bgsegm_background_subtractor_gmg_set_int")]
        internal static extern int BgSegmBackgroundSubtractorGMGSetInt(IntPtr subtractor, int propertyId, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bgsegm_background_subtractor_gmg_get_double")]
        internal static extern int BgSegmBackgroundSubtractorGMGGetDouble(IntPtr subtractor, int propertyId, out double value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bgsegm_background_subtractor_gmg_set_double")]
        internal static extern int BgSegmBackgroundSubtractorGMGSetDouble(IntPtr subtractor, int propertyId, double value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bgsegm_background_subtractor_cnt_create")]
        internal static extern int BgSegmBackgroundSubtractorCNTCreate(int minPixelStability, int useHistory, int maxPixelStability, int isParallel, out IntPtr subtractor);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bgsegm_background_subtractor_cnt_get_int")]
        internal static extern int BgSegmBackgroundSubtractorCNTGetInt(IntPtr subtractor, int propertyId, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bgsegm_background_subtractor_cnt_set_int")]
        internal static extern int BgSegmBackgroundSubtractorCNTSetInt(IntPtr subtractor, int propertyId, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bgsegm_synthetic_sequence_generator_create")]
        internal static extern int BgSegmSyntheticSequenceGeneratorCreate(IntPtr background, IntPtr objectMat, double amplitude, double wavelength, double wavespeed, double objspeed, out IntPtr generator);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bgsegm_synthetic_sequence_generator_release_handle")]
        internal static extern void BgSegmSyntheticSequenceGeneratorReleaseHandle(IntPtr generator);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_bgsegm_synthetic_sequence_generator_get_next_frame")]
        internal static extern int BgSegmSyntheticSequenceGeneratorGetNextFrame(IntPtr generator, IntPtr frame, IntPtr gtMask);
    }
}
#endif
