#if !NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    internal static unsafe partial class NativeMethods
    {
        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_get_available_targets_count")]
        internal static extern int DnnGetAvailableTargetsCount(int backendId, out int targetCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_get_available_targets_fill")]
        internal static extern int DnnGetAvailableTargetsFill(int backendId, int[] targets, int targetCapacity, out int targetCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_read_net_from_tensorflow_ex")]
        internal static extern int DnnReadNetFromTensorflowEx(byte[] model, byte[] config, int engine, byte[] extraOutputsBuffer, int[] extraOutputOffsets, int extraOutputCount, out IntPtr net);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_read_net_from_tensorflow_buffer")]
        internal static extern int DnnReadNetFromTensorflowBuffer(byte* modelBuffer, int modelBufferSize, byte* configBuffer, int configBufferSize, int engine, byte[] extraOutputsBuffer, int[] extraOutputOffsets, int extraOutputCount, out IntPtr net);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_read_net_from_tflite_buffer")]
        internal static extern int DnnReadNetFromTFLiteBuffer(byte* modelBuffer, int modelBufferSize, int engine, out IntPtr net);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_read_net_from_model_optimizer_buffer")]
        internal static extern int DnnReadNetFromModelOptimizerBuffer(byte* modelConfigBuffer, int modelConfigBufferSize, byte* weightsBuffer, int weightsBufferSize, out IntPtr net);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_read_net_from_onnx_buffer")]
        internal static extern int DnnReadNetFromOnnxBuffer(byte* modelBuffer, int modelBufferSize, int engine, out IntPtr net);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_read_tensor_from_onnx")]
        internal static extern int DnnReadTensorFromOnnx(byte[] path, IntPtr output);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_dump")]
        internal static extern int DnnNetDump(IntPtr net, out IntPtr result);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_dump_to_file")]
        internal static extern int DnnNetDumpToFile(IntPtr net, byte[] path);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_dump_to_pbtxt")]
        internal static extern int DnnNetDumpToPbtxt(IntPtr net, byte[] path);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_connect")]
        internal static extern int DnnNetConnect(IntPtr net, byte[] outputPin, byte[] inputPin);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_register_output")]
        internal static extern int DnnNetRegisterOutput(IntPtr net, byte[] outputName, int layerId, int outputPort, out int registeredLayerId);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_get_layer_by_id")]
        internal static extern int DnnNetGetLayerById(IntPtr net, int layerId, out IntPtr layer);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_get_layer_by_name")]
        internal static extern int DnnNetGetLayerByName(IntPtr net, byte[] layerName, out IntPtr layer);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_layer_release_handle")]
        internal static extern void DnnLayerReleaseHandle(IntPtr layer);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_layer_output_name_to_index")]
        internal static extern int DnnLayerOutputNameToIndex(IntPtr layer, byte[] outputName, out int outputIndex);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_forward_and_retrieve")]
        internal static extern int DnnNetForwardAndRetrieve(IntPtr net, byte[] namesBuffer, int[] nameOffsets, int nameCount, out IntPtr result);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_mat_groups_get_counts")]
        internal static extern int DnnMatGroupsGetCounts(IntPtr result, out int groupCount, out int matCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_mat_groups_get_group_offsets")]
        internal static extern int DnnMatGroupsGetGroupOffsets(IntPtr result, int[] offsets, int offsetCapacity, out int groupCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_mat_groups_take_mats")]
        internal static extern int DnnMatGroupsTakeMats(IntPtr result, IntPtr[] mats, int matCapacity, out int matCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_mat_groups_release_handle")]
        internal static extern void DnnMatGroupsReleaseHandle(IntPtr result);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_finalize")]
        internal static extern int DnnNetFinalize(IntPtr net);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_set_tracing_mode")]
        internal static extern int DnnNetSetTracingMode(IntPtr net, int mode);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_get_tracing_mode")]
        internal static extern int DnnNetGetTracingMode(IntPtr net, out int mode);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_set_profiling_mode")]
        internal static extern int DnnNetSetProfilingMode(IntPtr net, int mode);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_get_profiling_mode")]
        internal static extern int DnnNetGetProfilingMode(IntPtr net, out int mode);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_get_model_format")]
        internal static extern int DnnNetGetModelFormat(IntPtr net, out int format);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_set_param_by_id")]
        internal static extern int DnnNetSetParamById(IntPtr net, int layerId, int parameterIndex, IntPtr value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_set_param_by_name")]
        internal static extern int DnnNetSetParamByName(IntPtr net, byte[] layerName, int parameterIndex, IntPtr value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_get_param_by_id")]
        internal static extern int DnnNetGetParamById(IntPtr net, int layerId, int parameterIndex, IntPtr value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_get_param_by_name")]
        internal static extern int DnnNetGetParamByName(IntPtr net, byte[] layerName, int parameterIndex, IntPtr value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_get_layer_shapes_count")]
        internal static extern int DnnNetGetLayerShapesCount(IntPtr net, int[] inputShapeOffsets, int inputShapeCount, int[] inputShapeValues, int inputValueCount, int[] inputTypes, int inputTypeCount, int layerId, out int inputLayerShapeCount, out int inputLayerValueCount, out int outputLayerShapeCount, out int outputLayerValueCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_get_layer_shapes_fill")]
        internal static extern int DnnNetGetLayerShapesFill(IntPtr net, int[] inputShapeOffsets, int inputShapeCount, int[] inputShapeValues, int inputValueCount, int[] inputTypes, int inputTypeCount, int layerId, int[] inputLayerOffsets, int inputLayerOffsetCapacity, int[] inputLayerValues, int inputLayerValueCapacity, int[] outputLayerOffsets, int outputLayerOffsetCapacity, int[] outputLayerValues, int outputLayerValueCapacity, out int inputLayerShapeCount, out int inputLayerValueCount, out int outputLayerShapeCount, out int outputLayerValueCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_get_flops_many")]
        internal static extern int DnnNetGetFLOPSMany(IntPtr net, int[] inputShapeOffsets, int inputShapeCount, int[] inputShapeValues, int inputValueCount, int[] inputTypes, int inputTypeCount, out long flops);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_get_memory_consumption")]
        internal static extern int DnnNetGetMemoryConsumption(IntPtr net, int[] inputShapeOffsets, int inputShapeCount, int[] inputShapeValues, int inputValueCount, int[] inputTypes, int inputTypeCount, out ulong weightsBytes, out ulong blobBytes);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_enable_fusion")]
        internal static extern int DnnNetEnableFusion(IntPtr net, int enabled);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_enable_winograd")]
        internal static extern int DnnNetEnableWinograd(IntPtr net, int enabled);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_enable_kv_cache")]
        internal static extern int DnnNetEnableKvCache(IntPtr net);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_disable_kv_cache")]
        internal static extern int DnnNetDisableKvCache(IntPtr net);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_reset_kv_cache")]
        internal static extern int DnnNetResetKvCache(IntPtr net);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_get_detailed_perf_profile_count")]
        internal static extern int DnnNetGetDetailedPerfProfileCount(IntPtr net, out int rowCount, out int nameByteCount, out int timeByteCount, out int invocationByteCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_net_get_detailed_perf_profile_fill")]
        internal static extern int DnnNetGetDetailedPerfProfileFill(IntPtr net, int[] nameOffsets, int nameOffsetCapacity, byte[] names, int nameCapacity, int[] timeOffsets, int timeOffsetCapacity, byte[] times, int timeCapacity, int[] invocationOffsets, int invocationOffsetCapacity, byte[] invocations, int invocationCapacity, out int rowCount, out int nameByteCount, out int timeByteCount, out int invocationByteCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_blob_from_image_with_params")]
        internal static extern int DnnBlobFromImageWithParams(IntPtr image, IntPtr blob, in NativeDnnImage2BlobParams parameters);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_blob_from_images_with_params")]
        internal static extern int DnnBlobFromImagesWithParams(IntPtr[] images, int imageCount, IntPtr blob, in NativeDnnImage2BlobParams parameters);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_blob_rect_to_image_rect")]
        internal static extern int DnnBlobRectToImageRect(in NativeDnnImage2BlobParams parameters, in NativeDnnRect blobRect, int imageWidth, int imageHeight, out NativeDnnRect imageRect);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_blob_rects_to_image_rects")]
        internal static extern int DnnBlobRectsToImageRects(in NativeDnnImage2BlobParams parameters, NativeDnnRect[] blobRects, int blobRectCount, int imageWidth, int imageHeight, NativeDnnRect[] imageRects, int imageRectCapacity, out int imageRectCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_nms_boxes_rect")]
        internal static extern int DnnNmsBoxesRect(NativeDnnRect[] boxes, int boxCount, float[] scores, int scoreCount, float scoreThreshold, float nmsThreshold, float eta, int topK, int[] indices, int indexCapacity, out int indexCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_nms_boxes_rect2d")]
        internal static extern int DnnNmsBoxesRect2d(NativeDnnRect2d[] boxes, int boxCount, float[] scores, int scoreCount, float scoreThreshold, float nmsThreshold, float eta, int topK, int[] indices, int indexCapacity, out int indexCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_nms_boxes_rotated_rect")]
        internal static extern int DnnNmsBoxesRotatedRect(NativeDnnRotatedRect[] boxes, int boxCount, float[] scores, int scoreCount, float scoreThreshold, float nmsThreshold, float eta, int topK, int[] indices, int indexCapacity, out int indexCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_nms_boxes_batched_rect")]
        internal static extern int DnnNmsBoxesBatchedRect(NativeDnnRect[] boxes, int boxCount, float[] scores, int scoreCount, int[] classIds, int classIdCount, float scoreThreshold, float nmsThreshold, float eta, int topK, int[] indices, int indexCapacity, out int indexCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_nms_boxes_batched_rect2d")]
        internal static extern int DnnNmsBoxesBatchedRect2d(NativeDnnRect2d[] boxes, int boxCount, float[] scores, int scoreCount, int[] classIds, int classIdCount, float scoreThreshold, float nmsThreshold, float eta, int topK, int[] indices, int indexCapacity, out int indexCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_dnn_soft_nms_boxes_rect")]
        internal static extern int DnnSoftNmsBoxesRect(NativeDnnRect[] boxes, int boxCount, float[] scores, int scoreCount, float scoreThreshold, float nmsThreshold, float[] updatedScores, int updatedScoreCapacity, out int updatedScoreCount, int[] indices, int indexCapacity, out int indexCount, int topK, float sigma, int method);
    }
}
#endif
