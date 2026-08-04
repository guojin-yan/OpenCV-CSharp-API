#if NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    internal static partial class NativeMethods
    {
        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_ccm_gamma_correction")]
        internal static partial int PhotoCcmGammaCorrection(IntPtr src, IntPtr dst, double gamma);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_ccm_create")]
        internal static partial int PhotoCcmCreate(out IntPtr model);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_ccm_create_color_checker")]
        internal static partial int PhotoCcmCreateColorChecker(IntPtr src, int colorChecker, out IntPtr model);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_ccm_create_reference_colors")]
        internal static partial int PhotoCcmCreateReferenceColors(IntPtr src, IntPtr colors, int referenceColorSpace, out IntPtr model);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_ccm_create_reference_colors_masked")]
        internal static partial int PhotoCcmCreateReferenceColorsMasked(IntPtr src, IntPtr colors, int referenceColorSpace, IntPtr coloredPatchesMask, out IntPtr model);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_ccm_release_handle")]
        internal static partial void PhotoCcmReleaseHandle(IntPtr model);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_ccm_set_color_space")]
        internal static partial int PhotoCcmSetColorSpace(IntPtr model, int colorSpace);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_ccm_set_ccm_type")]
        internal static partial int PhotoCcmSetCcmType(IntPtr model, int ccmType);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_ccm_set_distance")]
        internal static partial int PhotoCcmSetDistance(IntPtr model, int distance);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_ccm_set_linearization")]
        internal static partial int PhotoCcmSetLinearization(IntPtr model, int linearization);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_ccm_set_linearization_gamma")]
        internal static partial int PhotoCcmSetLinearizationGamma(IntPtr model, double gamma);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_ccm_set_linearization_degree")]
        internal static partial int PhotoCcmSetLinearizationDegree(IntPtr model, int degree);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_ccm_set_saturated_threshold")]
        internal static partial int PhotoCcmSetSaturatedThreshold(IntPtr model, double lower, double upper);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_ccm_set_weights_list")]
        internal static partial int PhotoCcmSetWeightsList(IntPtr model, IntPtr weightsList);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_ccm_set_weight_coeff")]
        internal static partial int PhotoCcmSetWeightCoeff(IntPtr model, double weightCoeff);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_ccm_set_initial_method")]
        internal static partial int PhotoCcmSetInitialMethod(IntPtr model, int initialMethod);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_ccm_set_max_count")]
        internal static partial int PhotoCcmSetMaxCount(IntPtr model, int maxCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_ccm_set_epsilon")]
        internal static partial int PhotoCcmSetEpsilon(IntPtr model, double epsilon);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_ccm_set_rgb")]
        internal static partial int PhotoCcmSetRgb(IntPtr model, int rgb);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_ccm_compute")]
        internal static partial int PhotoCcmCompute(IntPtr model, IntPtr colorCorrectionMatrix);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_ccm_get_color_correction_matrix")]
        internal static partial int PhotoCcmGetColorCorrectionMatrix(IntPtr model, IntPtr colorCorrectionMatrix);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_ccm_get_loss")]
        internal static partial int PhotoCcmGetLoss(IntPtr model, out double loss);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_ccm_get_src_linear_rgb")]
        internal static partial int PhotoCcmGetSrcLinearRgb(IntPtr model, IntPtr srcLinearRgb);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_ccm_get_ref_linear_rgb")]
        internal static partial int PhotoCcmGetRefLinearRgb(IntPtr model, IntPtr refLinearRgb);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_ccm_get_mask")]
        internal static partial int PhotoCcmGetMask(IntPtr model, IntPtr mask);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_ccm_get_weights")]
        internal static partial int PhotoCcmGetWeights(IntPtr model, IntPtr weights);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_ccm_correct_image")]
        internal static partial int PhotoCcmCorrectImage(IntPtr model, IntPtr src, IntPtr dst, int isLinear);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_ccm_write")]
        internal static partial int PhotoCcmWrite(IntPtr model, IntPtr storage);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_ccm_read")]
        internal static partial int PhotoCcmRead(IntPtr model, IntPtr node);
    }
}
#endif
