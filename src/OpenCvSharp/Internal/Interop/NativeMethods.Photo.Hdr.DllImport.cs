#if !NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal static partial class NativeMethods
    {
        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_align_mtb_create")]
        internal static extern int AlignMtbCreate(int maxBits, int excludeRange, int cut, out IntPtr aligner);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_align_mtb_release_handle")]
        internal static extern void AlignMtbReleaseHandle(IntPtr aligner);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_align_mtb_process")]
        internal static extern int AlignMtbProcess(IntPtr aligner, IntPtr[] srcImages, IntPtr[] dstImages, int imageCount, IntPtr times, IntPtr response, int useExtraInputs);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_align_mtb_calculate_shift")]
        internal static extern int AlignMtbCalculateShift(IntPtr aligner, IntPtr img0, IntPtr img1, out int shiftX, out int shiftY);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_align_mtb_shift_mat")]
        internal static extern int AlignMtbShiftMat(IntPtr aligner, IntPtr src, IntPtr dst, int shiftX, int shiftY);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_align_mtb_compute_bitmaps")]
        internal static extern int AlignMtbComputeBitmaps(IntPtr aligner, IntPtr img, IntPtr thresholdBitmap, IntPtr excludeBitmap);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_align_mtb_get_max_bits")]
        internal static extern int AlignMtbGetMaxBits(IntPtr aligner, out int maxBits);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_align_mtb_set_max_bits")]
        internal static extern int AlignMtbSetMaxBits(IntPtr aligner, int maxBits);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_align_mtb_get_exclude_range")]
        internal static extern int AlignMtbGetExcludeRange(IntPtr aligner, out int excludeRange);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_align_mtb_set_exclude_range")]
        internal static extern int AlignMtbSetExcludeRange(IntPtr aligner, int excludeRange);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_align_mtb_get_cut")]
        internal static extern int AlignMtbGetCut(IntPtr aligner, out int cut);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_align_mtb_set_cut")]
        internal static extern int AlignMtbSetCut(IntPtr aligner, int cut);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calibrate_debevec_create")]
        internal static extern int CalibrateDebevecCreate(int samples, float lambda, int random, out IntPtr calibrator);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calibrate_robertson_create")]
        internal static extern int CalibrateRobertsonCreate(int maxIter, float threshold, out IntPtr calibrator);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calibrate_crf_release_handle")]
        internal static extern void CalibrateCrfReleaseHandle(IntPtr calibrator);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calibrate_crf_process")]
        internal static extern int CalibrateCrfProcess(IntPtr calibrator, IntPtr[] srcImages, int imageCount, IntPtr dst, IntPtr times);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calibrate_debevec_get_lambda")]
        internal static extern int CalibrateDebevecGetLambda(IntPtr calibrator, out float lambda);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calibrate_debevec_set_lambda")]
        internal static extern int CalibrateDebevecSetLambda(IntPtr calibrator, float lambda);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calibrate_debevec_get_samples")]
        internal static extern int CalibrateDebevecGetSamples(IntPtr calibrator, out int samples);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calibrate_debevec_set_samples")]
        internal static extern int CalibrateDebevecSetSamples(IntPtr calibrator, int samples);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calibrate_debevec_get_random")]
        internal static extern int CalibrateDebevecGetRandom(IntPtr calibrator, out int random);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calibrate_debevec_set_random")]
        internal static extern int CalibrateDebevecSetRandom(IntPtr calibrator, int random);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calibrate_robertson_get_max_iter")]
        internal static extern int CalibrateRobertsonGetMaxIter(IntPtr calibrator, out int maxIter);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calibrate_robertson_set_max_iter")]
        internal static extern int CalibrateRobertsonSetMaxIter(IntPtr calibrator, int maxIter);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calibrate_robertson_get_threshold")]
        internal static extern int CalibrateRobertsonGetThreshold(IntPtr calibrator, out float threshold);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calibrate_robertson_set_threshold")]
        internal static extern int CalibrateRobertsonSetThreshold(IntPtr calibrator, float threshold);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calibrate_robertson_get_radiance")]
        internal static extern int CalibrateRobertsonGetRadiance(IntPtr calibrator, IntPtr radiance);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_merge_debevec_create")]
        internal static extern int MergeDebevecCreate(out IntPtr merger);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_merge_mertens_create")]
        internal static extern int MergeMertensCreate(float contrastWeight, float saturationWeight, float exposureWeight, out IntPtr merger);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_merge_robertson_create")]
        internal static extern int MergeRobertsonCreate(out IntPtr merger);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_merge_exposures_release_handle")]
        internal static extern void MergeExposuresReleaseHandle(IntPtr merger);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_merge_exposures_process")]
        internal static extern int MergeExposuresProcess(IntPtr merger, IntPtr[] srcImages, int imageCount, IntPtr dst, IntPtr times, IntPtr response, int inputMode);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_merge_mertens_get_contrast_weight")]
        internal static extern int MergeMertensGetContrastWeight(IntPtr merger, out float contrastWeight);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_merge_mertens_set_contrast_weight")]
        internal static extern int MergeMertensSetContrastWeight(IntPtr merger, float contrastWeight);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_merge_mertens_get_saturation_weight")]
        internal static extern int MergeMertensGetSaturationWeight(IntPtr merger, out float saturationWeight);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_merge_mertens_set_saturation_weight")]
        internal static extern int MergeMertensSetSaturationWeight(IntPtr merger, float saturationWeight);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_merge_mertens_get_exposure_weight")]
        internal static extern int MergeMertensGetExposureWeight(IntPtr merger, out float exposureWeight);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_merge_mertens_set_exposure_weight")]
        internal static extern int MergeMertensSetExposureWeight(IntPtr merger, float exposureWeight);
    }
}
#endif
