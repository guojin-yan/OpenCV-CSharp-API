#if NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal static partial class NativeMethods
    {
        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_saliency_saliency_release_handle")]
        internal static partial void SaliencyReleaseHandle(IntPtr saliency);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_saliency_compute_saliency")]
        internal static partial int SaliencyComputeSaliency(IntPtr saliency, IntPtr image, IntPtr saliencyMap, out int result);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_saliency_static_compute_binary_map")]
        internal static partial int SaliencyStaticComputeBinaryMap(IntPtr saliency, IntPtr saliencyMap, IntPtr binaryMap, out int result);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_saliency_spectral_residual_create")]
        internal static partial int SaliencySpectralResidualCreate(out IntPtr saliency);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_saliency_spectral_residual_get_image_width")]
        internal static partial int SaliencySpectralResidualGetImageWidth(IntPtr saliency, out int width);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_saliency_spectral_residual_set_image_width")]
        internal static partial int SaliencySpectralResidualSetImageWidth(IntPtr saliency, int width);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_saliency_spectral_residual_get_image_height")]
        internal static partial int SaliencySpectralResidualGetImageHeight(IntPtr saliency, out int height);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_saliency_spectral_residual_set_image_height")]
        internal static partial int SaliencySpectralResidualSetImageHeight(IntPtr saliency, int height);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_saliency_fine_grained_create")]
        internal static partial int SaliencyFineGrainedCreate(out IntPtr saliency);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_saliency_motion_bin_wang_create")]
        internal static partial int SaliencyMotionBinWangCreate(out IntPtr saliency);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_saliency_motion_bin_wang_set_image_size")]
        internal static partial int SaliencyMotionBinWangSetImageSize(IntPtr saliency, int width, int height);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_saliency_motion_bin_wang_init")]
        internal static partial int SaliencyMotionBinWangInit(IntPtr saliency, out int result);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_saliency_motion_bin_wang_get_image_width")]
        internal static partial int SaliencyMotionBinWangGetImageWidth(IntPtr saliency, out int width);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_saliency_motion_bin_wang_set_image_width")]
        internal static partial int SaliencyMotionBinWangSetImageWidth(IntPtr saliency, int width);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_saliency_motion_bin_wang_get_image_height")]
        internal static partial int SaliencyMotionBinWangGetImageHeight(IntPtr saliency, out int height);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_saliency_motion_bin_wang_set_image_height")]
        internal static partial int SaliencyMotionBinWangSetImageHeight(IntPtr saliency, int height);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_saliency_objectness_bing_create")]
        internal static partial int SaliencyObjectnessBINGCreate(out IntPtr saliency);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_saliency_objectness_bing_set_training_path")]
        internal static partial int SaliencyObjectnessBINGSetTrainingPath(IntPtr saliency, byte[] trainingPath);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_saliency_objectness_bing_set_bb_res_dir")]
        internal static partial int SaliencyObjectnessBINGSetBBResDir(IntPtr saliency, byte[] resultsDir);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_saliency_objectness_bing_get_base")]
        internal static partial int SaliencyObjectnessBINGGetBase(IntPtr saliency, out double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_saliency_objectness_bing_set_base")]
        internal static partial int SaliencyObjectnessBINGSetBase(IntPtr saliency, double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_saliency_objectness_bing_get_nss")]
        internal static partial int SaliencyObjectnessBINGGetNSS(IntPtr saliency, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_saliency_objectness_bing_set_nss")]
        internal static partial int SaliencyObjectnessBINGSetNSS(IntPtr saliency, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_saliency_objectness_bing_get_w")]
        internal static partial int SaliencyObjectnessBINGGetW(IntPtr saliency, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_saliency_objectness_bing_set_w")]
        internal static partial int SaliencyObjectnessBINGSetW(IntPtr saliency, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_saliency_objectness_bing_compute")]
        internal static partial int SaliencyObjectnessBINGCompute(IntPtr saliency, IntPtr image, out int result);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_saliency_objectness_bing_get_boxes_count")]
        internal static partial int SaliencyObjectnessBINGGetBoxesCount(IntPtr saliency, out int count);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_saliency_objectness_bing_get_boxes_fill")]
        internal static partial int SaliencyObjectnessBINGGetBoxesFill(IntPtr saliency, int[] boxes, int boxCapacity, out int count);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_saliency_objectness_bing_get_objectness_values_count")]
        internal static partial int SaliencyObjectnessBINGGetObjectnessValuesCount(IntPtr saliency, out int count);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_saliency_objectness_bing_get_objectness_values_fill")]
        internal static partial int SaliencyObjectnessBINGGetObjectnessValuesFill(IntPtr saliency, float[] values, int valueCapacity, out int count);
    }
}
#endif
