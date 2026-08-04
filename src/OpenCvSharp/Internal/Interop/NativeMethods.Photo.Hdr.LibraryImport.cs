#if NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    internal static partial class NativeMethods
    {
        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_align_mtb_create")]
        internal static partial int AlignMtbCreate(int maxBits, int excludeRange, int cut, out IntPtr aligner);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_align_mtb_release_handle")]
        internal static partial void AlignMtbReleaseHandle(IntPtr aligner);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_align_mtb_process")]
        internal static partial int AlignMtbProcess(IntPtr aligner, IntPtr[] srcImages, IntPtr[] dstImages, int imageCount, IntPtr times, IntPtr response, int useExtraInputs);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_align_mtb_calculate_shift")]
        internal static partial int AlignMtbCalculateShift(IntPtr aligner, IntPtr img0, IntPtr img1, out int shiftX, out int shiftY);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_align_mtb_shift_mat")]
        internal static partial int AlignMtbShiftMat(IntPtr aligner, IntPtr src, IntPtr dst, int shiftX, int shiftY);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_align_mtb_compute_bitmaps")]
        internal static partial int AlignMtbComputeBitmaps(IntPtr aligner, IntPtr img, IntPtr thresholdBitmap, IntPtr excludeBitmap);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_align_mtb_get_max_bits")]
        internal static partial int AlignMtbGetMaxBits(IntPtr aligner, out int maxBits);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_align_mtb_set_max_bits")]
        internal static partial int AlignMtbSetMaxBits(IntPtr aligner, int maxBits);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_align_mtb_get_exclude_range")]
        internal static partial int AlignMtbGetExcludeRange(IntPtr aligner, out int excludeRange);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_align_mtb_set_exclude_range")]
        internal static partial int AlignMtbSetExcludeRange(IntPtr aligner, int excludeRange);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_align_mtb_get_cut")]
        internal static partial int AlignMtbGetCut(IntPtr aligner, out int cut);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_align_mtb_set_cut")]
        internal static partial int AlignMtbSetCut(IntPtr aligner, int cut);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calibrate_debevec_create")]
        internal static partial int CalibrateDebevecCreate(int samples, float lambda, int random, out IntPtr calibrator);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calibrate_robertson_create")]
        internal static partial int CalibrateRobertsonCreate(int maxIter, float threshold, out IntPtr calibrator);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calibrate_crf_release_handle")]
        internal static partial void CalibrateCrfReleaseHandle(IntPtr calibrator);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calibrate_crf_process")]
        internal static partial int CalibrateCrfProcess(IntPtr calibrator, IntPtr[] srcImages, int imageCount, IntPtr dst, IntPtr times);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calibrate_debevec_get_lambda")]
        internal static partial int CalibrateDebevecGetLambda(IntPtr calibrator, out float lambda);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calibrate_debevec_set_lambda")]
        internal static partial int CalibrateDebevecSetLambda(IntPtr calibrator, float lambda);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calibrate_debevec_get_samples")]
        internal static partial int CalibrateDebevecGetSamples(IntPtr calibrator, out int samples);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calibrate_debevec_set_samples")]
        internal static partial int CalibrateDebevecSetSamples(IntPtr calibrator, int samples);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calibrate_debevec_get_random")]
        internal static partial int CalibrateDebevecGetRandom(IntPtr calibrator, out int random);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calibrate_debevec_set_random")]
        internal static partial int CalibrateDebevecSetRandom(IntPtr calibrator, int random);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calibrate_robertson_get_max_iter")]
        internal static partial int CalibrateRobertsonGetMaxIter(IntPtr calibrator, out int maxIter);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calibrate_robertson_set_max_iter")]
        internal static partial int CalibrateRobertsonSetMaxIter(IntPtr calibrator, int maxIter);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calibrate_robertson_get_threshold")]
        internal static partial int CalibrateRobertsonGetThreshold(IntPtr calibrator, out float threshold);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calibrate_robertson_set_threshold")]
        internal static partial int CalibrateRobertsonSetThreshold(IntPtr calibrator, float threshold);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calibrate_robertson_get_radiance")]
        internal static partial int CalibrateRobertsonGetRadiance(IntPtr calibrator, IntPtr radiance);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_merge_debevec_create")]
        internal static partial int MergeDebevecCreate(out IntPtr merger);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_merge_mertens_create")]
        internal static partial int MergeMertensCreate(float contrastWeight, float saturationWeight, float exposureWeight, out IntPtr merger);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_merge_robertson_create")]
        internal static partial int MergeRobertsonCreate(out IntPtr merger);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_merge_exposures_release_handle")]
        internal static partial void MergeExposuresReleaseHandle(IntPtr merger);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_merge_exposures_process")]
        internal static partial int MergeExposuresProcess(IntPtr merger, IntPtr[] srcImages, int imageCount, IntPtr dst, IntPtr times, IntPtr response, int inputMode);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_merge_mertens_get_contrast_weight")]
        internal static partial int MergeMertensGetContrastWeight(IntPtr merger, out float contrastWeight);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_merge_mertens_set_contrast_weight")]
        internal static partial int MergeMertensSetContrastWeight(IntPtr merger, float contrastWeight);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_merge_mertens_get_saturation_weight")]
        internal static partial int MergeMertensGetSaturationWeight(IntPtr merger, out float saturationWeight);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_merge_mertens_set_saturation_weight")]
        internal static partial int MergeMertensSetSaturationWeight(IntPtr merger, float saturationWeight);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_merge_mertens_get_exposure_weight")]
        internal static partial int MergeMertensGetExposureWeight(IntPtr merger, out float exposureWeight);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_merge_mertens_set_exposure_weight")]
        internal static partial int MergeMertensSetExposureWeight(IntPtr merger, float exposureWeight);
    }
}
#endif
