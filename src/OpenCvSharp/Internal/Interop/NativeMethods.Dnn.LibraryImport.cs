#if NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal static unsafe partial class NativeMethods
    {
        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_create_empty")]
        internal static partial int DnnNetCreateEmpty(out IntPtr net);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_read_net")]
        internal static partial int DnnReadNet(byte[] model, byte[] config, byte[] framework, int engine, out IntPtr net);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_read_net_from_buffer")]
        internal static partial int DnnReadNetFromBuffer(byte[] framework, byte* modelBuffer, int modelBufferSize, byte* configBuffer, int configBufferSize, int engine, out IntPtr net);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_read_net_from_onnx")]
        internal static partial int DnnReadNetFromOnnx(byte[] model, int engine, out IntPtr net);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_read_net_from_tensorflow")]
        internal static partial int DnnReadNetFromTensorflow(byte[] model, byte[] config, int engine, out IntPtr net);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_read_net_from_tflite")]
        internal static partial int DnnReadNetFromTFLite(byte[] model, int engine, out IntPtr net);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_read_net_from_model_optimizer")]
        internal static partial int DnnReadNetFromModelOptimizer(byte[] xml, byte[] bin, out IntPtr net);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_release_handle")]
        internal static partial void DnnNetReleaseHandle(IntPtr net);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_empty")]
        internal static partial int DnnNetEmpty(IntPtr net, out int empty);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_set_preferable_backend")]
        internal static partial int DnnNetSetPreferableBackend(IntPtr net, int backendId);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_set_preferable_target")]
        internal static partial int DnnNetSetPreferableTarget(IntPtr net, int targetId);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_set_input")]
        internal static partial int DnnNetSetInput(IntPtr net, IntPtr blob, byte[] name, double scaleFactor, double meanV0, double meanV1, double meanV2, double meanV3);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_forward")]
        internal static partial int DnnNetForward(IntPtr net, byte[] outputName, IntPtr output);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_forward_many")]
        internal static partial int DnnNetForwardMany(IntPtr net, byte[] namesBuffer, int[] nameOffsets, int nameCount, IntPtr[] outputs, int outputCapacity, out int outputCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_get_layer_id")]
        internal static partial int DnnNetGetLayerId(IntPtr net, byte[] layerName, out int layerId);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_get_unconnected_out_layers_count")]
        internal static partial int DnnNetGetUnconnectedOutLayersCount(IntPtr net, out int layerCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_get_unconnected_out_layers_fill")]
        internal static partial int DnnNetGetUnconnectedOutLayersFill(IntPtr net, int[] layers, int layerCapacity, out int layerCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_set_inputs_names")]
        internal static partial int DnnNetSetInputsNames(IntPtr net, byte[] namesBuffer, int[] nameOffsets, int nameCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_set_input_shape")]
        internal static partial int DnnNetSetInputShape(IntPtr net, byte[] inputName, int[] shape, int shapeCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_get_flops")]
        internal static partial int DnnNetGetFLOPS(IntPtr net, int[] inputShape, int inputShapeCount, int inputType, out long flops);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_get_layer_flops")]
        internal static partial int DnnNetGetLayerFLOPS(IntPtr net, int layerId, int[] inputShape, int inputShapeCount, int inputType, out long flops);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_get_perf_profile_count")]
        internal static partial int DnnNetGetPerfProfileCount(IntPtr net, out int timingCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_get_perf_profile_fill")]
        internal static partial int DnnNetGetPerfProfileFill(IntPtr net, double[] timings, int timingCapacity, out int timingCount, out long tickCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_get_layer_names_count")]
        internal static partial int DnnNetGetLayerNamesCount(IntPtr net, out int stringCount, out int byteCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_get_layer_names_fill")]
        internal static partial int DnnNetGetLayerNamesFill(IntPtr net, int[] offsets, int offsetCapacity, byte[] buffer, int bufferCapacity, out int stringCount, out int byteCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_get_unconnected_out_layers_names_count")]
        internal static partial int DnnNetGetUnconnectedOutLayersNamesCount(IntPtr net, out int stringCount, out int byteCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_get_unconnected_out_layers_names_fill")]
        internal static partial int DnnNetGetUnconnectedOutLayersNamesFill(IntPtr net, int[] offsets, int offsetCapacity, byte[] buffer, int bufferCapacity, out int stringCount, out int byteCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_get_layer_types_count")]
        internal static partial int DnnNetGetLayerTypesCount(IntPtr net, out int stringCount, out int byteCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_get_layer_types_fill")]
        internal static partial int DnnNetGetLayerTypesFill(IntPtr net, int[] offsets, int offsetCapacity, byte[] buffer, int bufferCapacity, out int stringCount, out int byteCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_get_layers_count_by_type")]
        internal static partial int DnnNetGetLayersCountByType(IntPtr net, byte[] layerType, out int layerCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_blob_from_image")]
        internal static partial int DnnBlobFromImage(IntPtr image, IntPtr blob, double scaleFactor, int sizeWidth, int sizeHeight, double meanV0, double meanV1, double meanV2, double meanV3, int swapRb, int crop, int ddepth);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_blob_from_images")]
        internal static partial int DnnBlobFromImages(IntPtr[] images, int imageCount, IntPtr blob, double scaleFactor, int sizeWidth, int sizeHeight, double meanV0, double meanV1, double meanV2, double meanV3, int swapRb, int crop, int ddepth);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_images_from_blob_count")]
        internal static partial int DnnImagesFromBlobCount(IntPtr blob, out int imageCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_images_from_blob_fill")]
        internal static partial int DnnImagesFromBlobFill(IntPtr blob, IntPtr[] images, int imageCapacity, out int imageCount);
    }
}
#endif
