#if NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal static unsafe partial class NativeMethods
    {
        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_get_available_targets_count")]
        internal static partial int DnnGetAvailableTargetsCount(int backendId, out int targetCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_get_available_targets_fill")]
        internal static partial int DnnGetAvailableTargetsFill(int backendId, int[] targets, int targetCapacity, out int targetCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_read_net_from_tensorflow_ex")]
        internal static partial int DnnReadNetFromTensorflowEx(byte[] model, byte[] config, int engine, byte[] extraOutputsBuffer, int[] extraOutputOffsets, int extraOutputCount, out IntPtr net);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_read_net_from_tensorflow_buffer")]
        internal static partial int DnnReadNetFromTensorflowBuffer(byte* modelBuffer, int modelBufferSize, byte* configBuffer, int configBufferSize, int engine, byte[] extraOutputsBuffer, int[] extraOutputOffsets, int extraOutputCount, out IntPtr net);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_read_net_from_tflite_buffer")]
        internal static partial int DnnReadNetFromTFLiteBuffer(byte* modelBuffer, int modelBufferSize, int engine, out IntPtr net);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_read_net_from_model_optimizer_buffer")]
        internal static partial int DnnReadNetFromModelOptimizerBuffer(byte* modelConfigBuffer, int modelConfigBufferSize, byte* weightsBuffer, int weightsBufferSize, out IntPtr net);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_read_net_from_onnx_buffer")]
        internal static partial int DnnReadNetFromOnnxBuffer(byte* modelBuffer, int modelBufferSize, int engine, out IntPtr net);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_read_tensor_from_onnx")]
        internal static partial int DnnReadTensorFromOnnx(byte[] path, IntPtr output);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_dump")]
        internal static partial int DnnNetDump(IntPtr net, out IntPtr result);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_dump_to_file")]
        internal static partial int DnnNetDumpToFile(IntPtr net, byte[] path);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_dump_to_pbtxt")]
        internal static partial int DnnNetDumpToPbtxt(IntPtr net, byte[] path);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_connect")]
        internal static partial int DnnNetConnect(IntPtr net, byte[] outputPin, byte[] inputPin);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_register_output")]
        internal static partial int DnnNetRegisterOutput(IntPtr net, byte[] outputName, int layerId, int outputPort, out int registeredLayerId);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_get_layer_by_id")]
        internal static partial int DnnNetGetLayerById(IntPtr net, int layerId, out IntPtr layer);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_get_layer_by_name")]
        internal static partial int DnnNetGetLayerByName(IntPtr net, byte[] layerName, out IntPtr layer);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_layer_release_handle")]
        internal static partial void DnnLayerReleaseHandle(IntPtr layer);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_layer_output_name_to_index")]
        internal static partial int DnnLayerOutputNameToIndex(IntPtr layer, byte[] outputName, out int outputIndex);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_forward_and_retrieve")]
        internal static partial int DnnNetForwardAndRetrieve(IntPtr net, byte[] namesBuffer, int[] nameOffsets, int nameCount, out IntPtr result);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_mat_groups_get_counts")]
        internal static partial int DnnMatGroupsGetCounts(IntPtr result, out int groupCount, out int matCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_mat_groups_get_group_offsets")]
        internal static partial int DnnMatGroupsGetGroupOffsets(IntPtr result, int[] offsets, int offsetCapacity, out int groupCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_mat_groups_take_mats")]
        internal static partial int DnnMatGroupsTakeMats(IntPtr result, IntPtr[] mats, int matCapacity, out int matCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_mat_groups_release_handle")]
        internal static partial void DnnMatGroupsReleaseHandle(IntPtr result);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_finalize")]
        internal static partial int DnnNetFinalize(IntPtr net);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_set_tracing_mode")]
        internal static partial int DnnNetSetTracingMode(IntPtr net, int mode);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_get_tracing_mode")]
        internal static partial int DnnNetGetTracingMode(IntPtr net, out int mode);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_set_profiling_mode")]
        internal static partial int DnnNetSetProfilingMode(IntPtr net, int mode);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_get_profiling_mode")]
        internal static partial int DnnNetGetProfilingMode(IntPtr net, out int mode);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_get_model_format")]
        internal static partial int DnnNetGetModelFormat(IntPtr net, out int format);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_set_param_by_id")]
        internal static partial int DnnNetSetParamById(IntPtr net, int layerId, int parameterIndex, IntPtr value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_set_param_by_name")]
        internal static partial int DnnNetSetParamByName(IntPtr net, byte[] layerName, int parameterIndex, IntPtr value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_get_param_by_id")]
        internal static partial int DnnNetGetParamById(IntPtr net, int layerId, int parameterIndex, IntPtr value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_get_param_by_name")]
        internal static partial int DnnNetGetParamByName(IntPtr net, byte[] layerName, int parameterIndex, IntPtr value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_get_layer_shapes_count")]
        internal static partial int DnnNetGetLayerShapesCount(IntPtr net, int[] inputShapeOffsets, int inputShapeCount, int[] inputShapeValues, int inputValueCount, int[] inputTypes, int inputTypeCount, int layerId, out int inputLayerShapeCount, out int inputLayerValueCount, out int outputLayerShapeCount, out int outputLayerValueCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_get_layer_shapes_fill")]
        internal static partial int DnnNetGetLayerShapesFill(IntPtr net, int[] inputShapeOffsets, int inputShapeCount, int[] inputShapeValues, int inputValueCount, int[] inputTypes, int inputTypeCount, int layerId, int[] inputLayerOffsets, int inputLayerOffsetCapacity, int[] inputLayerValues, int inputLayerValueCapacity, int[] outputLayerOffsets, int outputLayerOffsetCapacity, int[] outputLayerValues, int outputLayerValueCapacity, out int inputLayerShapeCount, out int inputLayerValueCount, out int outputLayerShapeCount, out int outputLayerValueCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_get_flops_many")]
        internal static partial int DnnNetGetFLOPSMany(IntPtr net, int[] inputShapeOffsets, int inputShapeCount, int[] inputShapeValues, int inputValueCount, int[] inputTypes, int inputTypeCount, out long flops);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_get_memory_consumption")]
        internal static partial int DnnNetGetMemoryConsumption(IntPtr net, int[] inputShapeOffsets, int inputShapeCount, int[] inputShapeValues, int inputValueCount, int[] inputTypes, int inputTypeCount, out ulong weightsBytes, out ulong blobBytes);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_enable_fusion")]
        internal static partial int DnnNetEnableFusion(IntPtr net, int enabled);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_enable_winograd")]
        internal static partial int DnnNetEnableWinograd(IntPtr net, int enabled);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_enable_kv_cache")]
        internal static partial int DnnNetEnableKvCache(IntPtr net);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_disable_kv_cache")]
        internal static partial int DnnNetDisableKvCache(IntPtr net);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_reset_kv_cache")]
        internal static partial int DnnNetResetKvCache(IntPtr net);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_get_detailed_perf_profile_count")]
        internal static partial int DnnNetGetDetailedPerfProfileCount(IntPtr net, out int rowCount, out int nameByteCount, out int timeByteCount, out int invocationByteCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_get_detailed_perf_profile_fill")]
        internal static partial int DnnNetGetDetailedPerfProfileFill(IntPtr net, int[] nameOffsets, int nameOffsetCapacity, byte[] names, int nameCapacity, int[] timeOffsets, int timeOffsetCapacity, byte[] times, int timeCapacity, int[] invocationOffsets, int invocationOffsetCapacity, byte[] invocations, int invocationCapacity, out int rowCount, out int nameByteCount, out int timeByteCount, out int invocationByteCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_blob_from_image_with_params")]
        internal static partial int DnnBlobFromImageWithParams(IntPtr image, IntPtr blob, in NativeDnnImage2BlobParams parameters);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_blob_from_images_with_params")]
        internal static partial int DnnBlobFromImagesWithParams(IntPtr[] images, int imageCount, IntPtr blob, in NativeDnnImage2BlobParams parameters);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_blob_rect_to_image_rect")]
        internal static partial int DnnBlobRectToImageRect(in NativeDnnImage2BlobParams parameters, in NativeDnnRect blobRect, int imageWidth, int imageHeight, out NativeDnnRect imageRect);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_blob_rects_to_image_rects")]
        internal static partial int DnnBlobRectsToImageRects(in NativeDnnImage2BlobParams parameters, NativeDnnRect[] blobRects, int blobRectCount, int imageWidth, int imageHeight, NativeDnnRect[] imageRects, int imageRectCapacity, out int imageRectCount);
    }
}
#endif
