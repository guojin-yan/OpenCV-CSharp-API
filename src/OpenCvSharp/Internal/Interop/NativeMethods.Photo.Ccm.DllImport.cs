#if !NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal static partial class NativeMethods
    {
        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_ccm_gamma_correction")]
        internal static extern int PhotoCcmGammaCorrection(IntPtr src, IntPtr dst, double gamma);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_ccm_create")]
        internal static extern int PhotoCcmCreate(out IntPtr model);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_ccm_create_color_checker")]
        internal static extern int PhotoCcmCreateColorChecker(IntPtr src, int colorChecker, out IntPtr model);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_ccm_create_reference_colors")]
        internal static extern int PhotoCcmCreateReferenceColors(IntPtr src, IntPtr colors, int referenceColorSpace, out IntPtr model);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_ccm_create_reference_colors_masked")]
        internal static extern int PhotoCcmCreateReferenceColorsMasked(IntPtr src, IntPtr colors, int referenceColorSpace, IntPtr coloredPatchesMask, out IntPtr model);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_ccm_release_handle")]
        internal static extern void PhotoCcmReleaseHandle(IntPtr model);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_ccm_set_color_space")]
        internal static extern int PhotoCcmSetColorSpace(IntPtr model, int colorSpace);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_ccm_set_ccm_type")]
        internal static extern int PhotoCcmSetCcmType(IntPtr model, int ccmType);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_ccm_set_distance")]
        internal static extern int PhotoCcmSetDistance(IntPtr model, int distance);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_ccm_set_linearization")]
        internal static extern int PhotoCcmSetLinearization(IntPtr model, int linearization);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_ccm_set_linearization_gamma")]
        internal static extern int PhotoCcmSetLinearizationGamma(IntPtr model, double gamma);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_ccm_set_linearization_degree")]
        internal static extern int PhotoCcmSetLinearizationDegree(IntPtr model, int degree);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_ccm_set_saturated_threshold")]
        internal static extern int PhotoCcmSetSaturatedThreshold(IntPtr model, double lower, double upper);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_ccm_set_weights_list")]
        internal static extern int PhotoCcmSetWeightsList(IntPtr model, IntPtr weightsList);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_ccm_set_weight_coeff")]
        internal static extern int PhotoCcmSetWeightCoeff(IntPtr model, double weightCoeff);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_ccm_set_initial_method")]
        internal static extern int PhotoCcmSetInitialMethod(IntPtr model, int initialMethod);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_ccm_set_max_count")]
        internal static extern int PhotoCcmSetMaxCount(IntPtr model, int maxCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_ccm_set_epsilon")]
        internal static extern int PhotoCcmSetEpsilon(IntPtr model, double epsilon);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_ccm_set_rgb")]
        internal static extern int PhotoCcmSetRgb(IntPtr model, int rgb);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_ccm_compute")]
        internal static extern int PhotoCcmCompute(IntPtr model, IntPtr colorCorrectionMatrix);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_ccm_get_color_correction_matrix")]
        internal static extern int PhotoCcmGetColorCorrectionMatrix(IntPtr model, IntPtr colorCorrectionMatrix);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_ccm_get_loss")]
        internal static extern int PhotoCcmGetLoss(IntPtr model, out double loss);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_ccm_get_src_linear_rgb")]
        internal static extern int PhotoCcmGetSrcLinearRgb(IntPtr model, IntPtr srcLinearRgb);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_ccm_get_ref_linear_rgb")]
        internal static extern int PhotoCcmGetRefLinearRgb(IntPtr model, IntPtr refLinearRgb);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_ccm_get_mask")]
        internal static extern int PhotoCcmGetMask(IntPtr model, IntPtr mask);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_ccm_get_weights")]
        internal static extern int PhotoCcmGetWeights(IntPtr model, IntPtr weights);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_ccm_correct_image")]
        internal static extern int PhotoCcmCorrectImage(IntPtr model, IntPtr src, IntPtr dst, int isLinear);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_ccm_write")]
        internal static extern int PhotoCcmWrite(IntPtr model, IntPtr storage);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_photo_ccm_read")]
        internal static extern int PhotoCcmRead(IntPtr model, IntPtr node);
    }
}
#endif
