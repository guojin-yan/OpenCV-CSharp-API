#if !NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal static unsafe partial class NativeMethods
    {
        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_create_empty")]
        internal static extern int DnnNetCreateEmpty(out IntPtr net);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_read_net")]
        internal static extern int DnnReadNet(byte[] model, byte[] config, byte[] framework, int engine, out IntPtr net);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_read_net_from_buffer")]
        internal static extern int DnnReadNetFromBuffer(byte[] framework, byte* modelBuffer, int modelBufferSize, byte* configBuffer, int configBufferSize, int engine, out IntPtr net);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_read_net_from_onnx")]
        internal static extern int DnnReadNetFromOnnx(byte[] model, int engine, out IntPtr net);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_read_net_from_tensorflow")]
        internal static extern int DnnReadNetFromTensorflow(byte[] model, byte[] config, int engine, out IntPtr net);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_read_net_from_tflite")]
        internal static extern int DnnReadNetFromTFLite(byte[] model, int engine, out IntPtr net);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_read_net_from_model_optimizer")]
        internal static extern int DnnReadNetFromModelOptimizer(byte[] xml, byte[] bin, out IntPtr net);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_release_handle")]
        internal static extern void DnnNetReleaseHandle(IntPtr net);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_empty")]
        internal static extern int DnnNetEmpty(IntPtr net, out int empty);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_set_preferable_backend")]
        internal static extern int DnnNetSetPreferableBackend(IntPtr net, int backendId);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_set_preferable_target")]
        internal static extern int DnnNetSetPreferableTarget(IntPtr net, int targetId);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_set_input")]
        internal static extern int DnnNetSetInput(IntPtr net, IntPtr blob, byte[] name, double scaleFactor, double meanV0, double meanV1, double meanV2, double meanV3);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_forward")]
        internal static extern int DnnNetForward(IntPtr net, byte[] outputName, IntPtr output);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_forward_many")]
        internal static extern int DnnNetForwardMany(IntPtr net, byte[] namesBuffer, int[] nameOffsets, int nameCount, IntPtr[] outputs, int outputCapacity, out int outputCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_get_layer_id")]
        internal static extern int DnnNetGetLayerId(IntPtr net, byte[] layerName, out int layerId);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_get_unconnected_out_layers_count")]
        internal static extern int DnnNetGetUnconnectedOutLayersCount(IntPtr net, out int layerCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_get_unconnected_out_layers_fill")]
        internal static extern int DnnNetGetUnconnectedOutLayersFill(IntPtr net, int[] layers, int layerCapacity, out int layerCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_set_inputs_names")]
        internal static extern int DnnNetSetInputsNames(IntPtr net, byte[] namesBuffer, int[] nameOffsets, int nameCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_set_input_shape")]
        internal static extern int DnnNetSetInputShape(IntPtr net, byte[] inputName, int[] shape, int shapeCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_get_flops")]
        internal static extern int DnnNetGetFLOPS(IntPtr net, int[] inputShape, int inputShapeCount, int inputType, out long flops);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_get_layer_flops")]
        internal static extern int DnnNetGetLayerFLOPS(IntPtr net, int layerId, int[] inputShape, int inputShapeCount, int inputType, out long flops);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_get_perf_profile_count")]
        internal static extern int DnnNetGetPerfProfileCount(IntPtr net, out int timingCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_get_perf_profile_fill")]
        internal static extern int DnnNetGetPerfProfileFill(IntPtr net, double[] timings, int timingCapacity, out int timingCount, out long tickCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_get_layer_names_count")]
        internal static extern int DnnNetGetLayerNamesCount(IntPtr net, out int stringCount, out int byteCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_get_layer_names_fill")]
        internal static extern int DnnNetGetLayerNamesFill(IntPtr net, int[] offsets, int offsetCapacity, byte[] buffer, int bufferCapacity, out int stringCount, out int byteCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_get_unconnected_out_layers_names_count")]
        internal static extern int DnnNetGetUnconnectedOutLayersNamesCount(IntPtr net, out int stringCount, out int byteCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_get_unconnected_out_layers_names_fill")]
        internal static extern int DnnNetGetUnconnectedOutLayersNamesFill(IntPtr net, int[] offsets, int offsetCapacity, byte[] buffer, int bufferCapacity, out int stringCount, out int byteCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_get_layer_types_count")]
        internal static extern int DnnNetGetLayerTypesCount(IntPtr net, out int stringCount, out int byteCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_get_layer_types_fill")]
        internal static extern int DnnNetGetLayerTypesFill(IntPtr net, int[] offsets, int offsetCapacity, byte[] buffer, int bufferCapacity, out int stringCount, out int byteCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_get_layers_count_by_type")]
        internal static extern int DnnNetGetLayersCountByType(IntPtr net, byte[] layerType, out int layerCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_blob_from_image")]
        internal static extern int DnnBlobFromImage(IntPtr image, IntPtr blob, double scaleFactor, int sizeWidth, int sizeHeight, double meanV0, double meanV1, double meanV2, double meanV3, int swapRb, int crop, int ddepth);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_blob_from_images")]
        internal static extern int DnnBlobFromImages(IntPtr[] images, int imageCount, IntPtr blob, double scaleFactor, int sizeWidth, int sizeHeight, double meanV0, double meanV1, double meanV2, double meanV3, int swapRb, int crop, int ddepth);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_images_from_blob_count")]
        internal static extern int DnnImagesFromBlobCount(IntPtr blob, out int imageCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_images_from_blob_fill")]
        internal static extern int DnnImagesFromBlobFill(IntPtr blob, IntPtr[] images, int imageCapacity, out int imageCount);
    }
}
#endif
