#if !NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal static partial class NativeMethods
    {
        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_saliency_saliency_release_handle")]
        internal static extern void SaliencyReleaseHandle(IntPtr saliency);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_saliency_compute_saliency")]
        internal static extern int SaliencyComputeSaliency(IntPtr saliency, IntPtr image, IntPtr saliencyMap, out int result);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_saliency_static_compute_binary_map")]
        internal static extern int SaliencyStaticComputeBinaryMap(IntPtr saliency, IntPtr saliencyMap, IntPtr binaryMap, out int result);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_saliency_spectral_residual_create")]
        internal static extern int SaliencySpectralResidualCreate(out IntPtr saliency);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_saliency_spectral_residual_get_image_width")]
        internal static extern int SaliencySpectralResidualGetImageWidth(IntPtr saliency, out int width);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_saliency_spectral_residual_set_image_width")]
        internal static extern int SaliencySpectralResidualSetImageWidth(IntPtr saliency, int width);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_saliency_spectral_residual_get_image_height")]
        internal static extern int SaliencySpectralResidualGetImageHeight(IntPtr saliency, out int height);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_saliency_spectral_residual_set_image_height")]
        internal static extern int SaliencySpectralResidualSetImageHeight(IntPtr saliency, int height);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_saliency_fine_grained_create")]
        internal static extern int SaliencyFineGrainedCreate(out IntPtr saliency);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_saliency_motion_bin_wang_create")]
        internal static extern int SaliencyMotionBinWangCreate(out IntPtr saliency);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_saliency_motion_bin_wang_set_image_size")]
        internal static extern int SaliencyMotionBinWangSetImageSize(IntPtr saliency, int width, int height);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_saliency_motion_bin_wang_init")]
        internal static extern int SaliencyMotionBinWangInit(IntPtr saliency, out int result);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_saliency_motion_bin_wang_get_image_width")]
        internal static extern int SaliencyMotionBinWangGetImageWidth(IntPtr saliency, out int width);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_saliency_motion_bin_wang_set_image_width")]
        internal static extern int SaliencyMotionBinWangSetImageWidth(IntPtr saliency, int width);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_saliency_motion_bin_wang_get_image_height")]
        internal static extern int SaliencyMotionBinWangGetImageHeight(IntPtr saliency, out int height);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_saliency_motion_bin_wang_set_image_height")]
        internal static extern int SaliencyMotionBinWangSetImageHeight(IntPtr saliency, int height);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_saliency_objectness_bing_create")]
        internal static extern int SaliencyObjectnessBINGCreate(out IntPtr saliency);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_saliency_objectness_bing_set_training_path")]
        internal static extern int SaliencyObjectnessBINGSetTrainingPath(IntPtr saliency, byte[] trainingPath);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_saliency_objectness_bing_set_bb_res_dir")]
        internal static extern int SaliencyObjectnessBINGSetBBResDir(IntPtr saliency, byte[] resultsDir);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_saliency_objectness_bing_get_base")]
        internal static extern int SaliencyObjectnessBINGGetBase(IntPtr saliency, out double value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_saliency_objectness_bing_set_base")]
        internal static extern int SaliencyObjectnessBINGSetBase(IntPtr saliency, double value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_saliency_objectness_bing_get_nss")]
        internal static extern int SaliencyObjectnessBINGGetNSS(IntPtr saliency, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_saliency_objectness_bing_set_nss")]
        internal static extern int SaliencyObjectnessBINGSetNSS(IntPtr saliency, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_saliency_objectness_bing_get_w")]
        internal static extern int SaliencyObjectnessBINGGetW(IntPtr saliency, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_saliency_objectness_bing_set_w")]
        internal static extern int SaliencyObjectnessBINGSetW(IntPtr saliency, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_saliency_objectness_bing_compute")]
        internal static extern int SaliencyObjectnessBINGCompute(IntPtr saliency, IntPtr image, out int result);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_saliency_objectness_bing_get_boxes_count")]
        internal static extern int SaliencyObjectnessBINGGetBoxesCount(IntPtr saliency, out int count);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_saliency_objectness_bing_get_boxes_fill")]
        internal static extern int SaliencyObjectnessBINGGetBoxesFill(IntPtr saliency, int[] boxes, int boxCapacity, out int count);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_saliency_objectness_bing_get_objectness_values_count")]
        internal static extern int SaliencyObjectnessBINGGetObjectnessValuesCount(IntPtr saliency, out int count);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_saliency_objectness_bing_get_objectness_values_fill")]
        internal static extern int SaliencyObjectnessBINGGetObjectnessValuesFill(IntPtr saliency, float[] values, int valueCapacity, out int count);
    }
}
#endif
