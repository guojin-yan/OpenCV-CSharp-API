using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Dnn
{
    /// <summary>
    /// Deep neural network object compatible with OpenCV <c>cv::dnn::Net</c>.
    /// 与 OpenCV <c>cv::dnn::Net</c> 兼容的深度神经网络对象。
    /// </summary>
    public sealed unsafe partial class Net : IDisposable
    {
        private delegate int StringArrayCount(IntPtr net, out int stringCount, out int byteCount);

        private delegate int StringArrayFill(IntPtr net, int[] offsets, int offsetCapacity, byte[] buffer, int bufferCapacity, out int stringCount, out int byteCount);

        private readonly NativeDnnNetHandle handle;

        private Net(IntPtr nativeHandle)
        {
            handle = NativeDnnNetHandle.FromNativePointer(nativeHandle);
        }

        /// <summary>
        /// Gets whether this network has been disposed.
        /// 获取网络是否已经释放。
        /// </summary>
        public bool IsDisposed
        {
            get { return handle.IsClosed; }
        }

        /// <summary>
        /// Gets whether the network contains no layers.
        /// 获取网络是否不包含任何层。
        /// </summary>
        public bool Empty
        {
            get
            {
                return WithNativeHandle(nativeHandle =>
                {
                    NativeException.ThrowIfError(NativeMethods.DnnNetEmpty(nativeHandle, out int empty));
                    return empty != 0;
                });
            }
        }

        /// <summary>
        /// Creates an empty DNN network.
        /// 创建空 DNN 网络。
        /// </summary>
        public static Net CreateEmpty()
        {
            NativeException.ThrowIfError(NativeMethods.DnnNetCreateEmpty(out IntPtr nativeHandle));
            return new Net(nativeHandle);
        }

        /// <summary>
        /// Reads a network from model/config paths.
        /// 从模型/配置路径读取网络。
        /// </summary>
        public static Net ReadNet(string model, string config = "", string framework = "", DnnEngine engine = DnnEngine.Auto)
        {
            ValidateEngine(engine, nameof(engine));
            byte[] nativeModel = DnnStringConvert.ToNullTerminatedUtf8(model, nameof(model));
            byte[] nativeConfig = DnnStringConvert.ToNullTerminatedUtf8(config ?? string.Empty, nameof(config));
            byte[] nativeFramework = DnnStringConvert.ToNullTerminatedUtf8(framework ?? string.Empty, nameof(framework));
            NativeException.ThrowIfError(NativeMethods.DnnReadNet(nativeModel, nativeConfig, nativeFramework, (int)engine, out IntPtr nativeHandle));
            return new Net(nativeHandle);
        }

        /// <summary>
        /// Reads an ONNX network from a file path.
        /// 从文件路径读取 ONNX 网络。
        /// </summary>
        public static Net ReadNetFromOnnx(string model, DnnEngine engine = DnnEngine.Auto)
        {
            ValidateEngine(engine, nameof(engine));
            byte[] nativeModel = DnnStringConvert.ToNullTerminatedUtf8(model, nameof(model));
            NativeException.ThrowIfError(NativeMethods.DnnReadNetFromOnnx(nativeModel, (int)engine, out IntPtr nativeHandle));
            return new Net(nativeHandle);
        }

        /// <summary>Reads an ONNX network from an in-memory buffer.</summary>
        public static Net ReadNetFromOnnx(byte[] modelBuffer, DnnEngine engine = DnnEngine.Auto)
        {
            ValidateEngine(engine, nameof(engine));
            ValidateBuffer(modelBuffer, nameof(modelBuffer));
            unsafe
            {
                fixed (byte* modelPtr = modelBuffer)
                {
                    NativeException.ThrowIfError(NativeMethods.DnnReadNetFromOnnxBuffer(modelPtr, modelBuffer.Length, (int)engine, out IntPtr nativeHandle));
                    return new Net(nativeHandle);
                }
            }
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>Reads an ONNX network from a span-backed in-memory buffer.</summary>
        /// <param name="modelBuffer">Complete non-empty ONNX model bytes. The span is borrowed only for the native call.</param>
        /// <param name="engine">Requested OpenCV DNN engine.</param>
        /// <returns>An independently owned network.</returns>
        /// <remarks>OpenCV copies or parses the bytes before this method returns; no managed interior pointer is retained.</remarks>
        public static Net ReadNetFromOnnx(ReadOnlySpan<byte> modelBuffer, DnnEngine engine = DnnEngine.Auto)
        {
            ValidateEngine(engine, nameof(engine));
            if (modelBuffer.Length == 0) throw new ArgumentException("Model buffer cannot be empty.", nameof(modelBuffer));
            unsafe
            {
                fixed (byte* modelPtr = modelBuffer)
                {
                    NativeException.ThrowIfError(NativeMethods.DnnReadNetFromOnnxBuffer(modelPtr, modelBuffer.Length, (int)engine, out IntPtr nativeHandle));
                    return new Net(nativeHandle);
                }
            }
        }
#endif

        /// <summary>
        /// Reads a TensorFlow network from model/config file paths.
        /// 从模型/配置文件路径读取 TensorFlow 网络。
        /// </summary>
        public static Net ReadNetFromTensorflow(string model, string config = "", DnnEngine engine = DnnEngine.Auto)
        {
            ValidateEngine(engine, nameof(engine));
            byte[] nativeModel = DnnStringConvert.ToNullTerminatedUtf8(model, nameof(model));
            byte[] nativeConfig = DnnStringConvert.ToNullTerminatedUtf8(config ?? string.Empty, nameof(config));
            NativeException.ThrowIfError(NativeMethods.DnnReadNetFromTensorflow(nativeModel, nativeConfig, (int)engine, out IntPtr nativeHandle));
            return new Net(nativeHandle);
        }

        /// <summary>Reads a TensorFlow network from paths and explicitly named extra outputs.</summary>
        public static Net ReadNetFromTensorflow(string model, string config, DnnEngine engine, string[] extraOutputs)
        {
            ValidateEngine(engine, nameof(engine));
            ValidateStringArray(extraOutputs, nameof(extraOutputs), true);
            PackStringArray(extraOutputs, out byte[] buffer, out int[] offsets);
            byte[] nativeModel = DnnStringConvert.ToNullTerminatedUtf8(model, nameof(model));
            byte[] nativeConfig = DnnStringConvert.ToNullTerminatedUtf8(config ?? string.Empty, nameof(config));
            NativeException.ThrowIfError(NativeMethods.DnnReadNetFromTensorflowEx(nativeModel, nativeConfig, (int)engine, buffer, offsets, extraOutputs.Length, out IntPtr nativeHandle));
            return new Net(nativeHandle);
        }

        /// <summary>Reads a TensorFlow network from in-memory model/config buffers.</summary>
        public static Net ReadNetFromTensorflow(byte[] modelBuffer, byte[]? configBuffer = null, DnnEngine engine = DnnEngine.Auto, string[]? extraOutputs = null)
        {
            ValidateEngine(engine, nameof(engine));
            ValidateBuffer(modelBuffer, nameof(modelBuffer));
            byte[] config = configBuffer ?? Array.Empty<byte>();
            string[] outputs = extraOutputs ?? Array.Empty<string>();
            ValidateStringArray(outputs, nameof(extraOutputs), true);
            PackStringArray(outputs, out byte[] names, out int[] offsets);
            unsafe
            {
                fixed (byte* modelPtr = modelBuffer)
                fixed (byte* configPtr = config)
                {
                    NativeException.ThrowIfError(NativeMethods.DnnReadNetFromTensorflowBuffer(modelPtr, modelBuffer.Length, configPtr, config.Length, (int)engine, names, offsets, outputs.Length, out IntPtr nativeHandle));
                    return new Net(nativeHandle);
                }
            }
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>Reads a TensorFlow network from span-backed model/config buffers.</summary>
        /// <param name="modelBuffer">Complete non-empty TensorFlow model bytes.</param>
        /// <param name="configBuffer">Optional text graph/configuration bytes; an empty span is valid.</param>
        /// <param name="engine">Requested OpenCV DNN engine.</param>
        /// <param name="extraOutputs">Optional UTF-8 output names retained by the imported graph.</param>
        /// <returns>An independently owned network.</returns>
        /// <remarks>All spans and pinned pointers are borrowed only for the native call.</remarks>
        public static Net ReadNetFromTensorflow(ReadOnlySpan<byte> modelBuffer, ReadOnlySpan<byte> configBuffer, DnnEngine engine = DnnEngine.Auto, string[]? extraOutputs = null)
        {
            ValidateEngine(engine, nameof(engine));
            if (modelBuffer.Length == 0) throw new ArgumentException("Model buffer cannot be empty.", nameof(modelBuffer));
            string[] outputs = extraOutputs ?? Array.Empty<string>();
            ValidateStringArray(outputs, nameof(extraOutputs), true);
            PackStringArray(outputs, out byte[] names, out int[] offsets);
            unsafe
            {
                fixed (byte* modelPtr = modelBuffer)
                fixed (byte* configPtr = configBuffer)
                {
                    NativeException.ThrowIfError(NativeMethods.DnnReadNetFromTensorflowBuffer(modelPtr, modelBuffer.Length, configPtr, configBuffer.Length, (int)engine, names, offsets, outputs.Length, out IntPtr nativeHandle));
                    return new Net(nativeHandle);
                }
            }
        }
#endif

        /// <summary>
        /// Reads a TensorFlow Lite network from a file path.
        /// 从文件路径读取 TensorFlow Lite 网络。
        /// </summary>
        public static Net ReadNetFromTFLite(string model, DnnEngine engine = DnnEngine.Auto)
        {
            ValidateEngine(engine, nameof(engine));
            byte[] nativeModel = DnnStringConvert.ToNullTerminatedUtf8(model, nameof(model));
            NativeException.ThrowIfError(NativeMethods.DnnReadNetFromTFLite(nativeModel, (int)engine, out IntPtr nativeHandle));
            return new Net(nativeHandle);
        }

        /// <summary>Reads a TensorFlow Lite network from an in-memory buffer.</summary>
        public static Net ReadNetFromTFLite(byte[] modelBuffer, DnnEngine engine = DnnEngine.Auto)
        {
            ValidateEngine(engine, nameof(engine));
            ValidateBuffer(modelBuffer, nameof(modelBuffer));
            unsafe
            {
                fixed (byte* modelPtr = modelBuffer)
                {
                    NativeException.ThrowIfError(NativeMethods.DnnReadNetFromTFLiteBuffer(modelPtr, modelBuffer.Length, (int)engine, out IntPtr nativeHandle));
                    return new Net(nativeHandle);
                }
            }
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>Reads a TensorFlow Lite network from a span-backed buffer.</summary>
        /// <param name="modelBuffer">Complete non-empty TensorFlow Lite model bytes.</param>
        /// <param name="engine">Requested OpenCV DNN engine.</param>
        /// <returns>An independently owned network.</returns>
        /// <remarks>The span is borrowed only for the native call.</remarks>
        public static Net ReadNetFromTFLite(ReadOnlySpan<byte> modelBuffer, DnnEngine engine = DnnEngine.Auto)
        {
            ValidateEngine(engine, nameof(engine));
            if (modelBuffer.Length == 0) throw new ArgumentException("Model buffer cannot be empty.", nameof(modelBuffer));
            unsafe
            {
                fixed (byte* modelPtr = modelBuffer)
                {
                    NativeException.ThrowIfError(NativeMethods.DnnReadNetFromTFLiteBuffer(modelPtr, modelBuffer.Length, (int)engine, out IntPtr nativeHandle));
                    return new Net(nativeHandle);
                }
            }
        }
#endif

        /// <summary>
        /// Reads an OpenVINO Model Optimizer network from XML/bin paths.
        /// 从 XML/bin 路径读取 OpenVINO Model Optimizer 网络。
        /// </summary>
        public static Net ReadNetFromModelOptimizer(string xml, string bin = "")
        {
            byte[] nativeXml = DnnStringConvert.ToNullTerminatedUtf8(xml, nameof(xml));
            byte[] nativeBin = DnnStringConvert.ToNullTerminatedUtf8(bin ?? string.Empty, nameof(bin));
            NativeException.ThrowIfError(NativeMethods.DnnReadNetFromModelOptimizer(nativeXml, nativeBin, out IntPtr nativeHandle));
            return new Net(nativeHandle);
        }

        /// <summary>Reads an OpenVINO Model Optimizer network from XML and weights buffers.</summary>
        public static Net ReadNetFromModelOptimizer(byte[] modelConfigBuffer, byte[] weightsBuffer)
        {
            ValidateBuffer(modelConfigBuffer, nameof(modelConfigBuffer));
            ValidateBuffer(weightsBuffer, nameof(weightsBuffer));
            unsafe
            {
                fixed (byte* modelPtr = modelConfigBuffer)
                fixed (byte* weightsPtr = weightsBuffer)
                {
                    NativeException.ThrowIfError(NativeMethods.DnnReadNetFromModelOptimizerBuffer(modelPtr, modelConfigBuffer.Length, weightsPtr, weightsBuffer.Length, out IntPtr nativeHandle));
                    return new Net(nativeHandle);
                }
            }
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>Reads an OpenVINO Model Optimizer network from span-backed XML and weights buffers.</summary>
        /// <param name="modelConfigBuffer">Complete non-empty XML model bytes.</param>
        /// <param name="weightsBuffer">Complete non-empty binary weight bytes.</param>
        /// <returns>An independently owned network.</returns>
        /// <remarks>Both spans are borrowed only for the native call; no managed interior pointer is retained.</remarks>
        public static Net ReadNetFromModelOptimizer(ReadOnlySpan<byte> modelConfigBuffer, ReadOnlySpan<byte> weightsBuffer)
        {
            if (modelConfigBuffer.Length == 0) throw new ArgumentException("Model config buffer cannot be empty.", nameof(modelConfigBuffer));
            if (weightsBuffer.Length == 0) throw new ArgumentException("Weights buffer cannot be empty.", nameof(weightsBuffer));
            fixed (byte* modelPtr = modelConfigBuffer)
            fixed (byte* weightsPtr = weightsBuffer)
            {
                NativeException.ThrowIfError(NativeMethods.DnnReadNetFromModelOptimizerBuffer(modelPtr, modelConfigBuffer.Length, weightsPtr, weightsBuffer.Length, out IntPtr nativeHandle));
                return new Net(nativeHandle);
            }
        }
#endif

        /// <summary>
        /// Reads a network from in-memory model/config buffers.
        /// 从内存模型/配置缓冲区读取网络。
        /// </summary>
        public static Net ReadNet(string framework, byte[] modelBuffer, byte[]? configBuffer = null, DnnEngine engine = DnnEngine.Auto)
        {
            ValidateEngine(engine, nameof(engine));
            ValidateNotNull(modelBuffer, nameof(modelBuffer));
            byte[] config = configBuffer ?? Array.Empty<byte>();
            byte[] nativeFramework = DnnStringConvert.ToNullTerminatedUtf8(framework, nameof(framework));
            ValidateNotEmpty(modelBuffer, nameof(modelBuffer));
            fixed (byte* modelPtr = modelBuffer)
            fixed (byte* configPtr = config)
            {
                NativeException.ThrowIfError(NativeMethods.DnnReadNetFromBuffer(nativeFramework, modelPtr, modelBuffer.Length, configPtr, config.Length, (int)engine, out IntPtr nativeHandle));
                return new Net(nativeHandle);
            }
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Reads a network from span-backed model/config buffers.
        /// 从 Span 支持的模型/配置缓冲区读取网络。
        /// </summary>
        /// <param name="framework">Framework name encoded as UTF-8 for OpenCV.</param>
        /// <param name="modelBuffer">Complete non-empty model bytes.</param>
        /// <param name="configBuffer">Optional configuration bytes; an empty span is valid.</param>
        /// <param name="engine">Requested OpenCV DNN engine.</param>
        /// <returns>An independently owned network.</returns>
        /// <remarks>Both spans are borrowed only for the native call.</remarks>
        public static Net ReadNet(string framework, ReadOnlySpan<byte> modelBuffer, ReadOnlySpan<byte> configBuffer, DnnEngine engine = DnnEngine.Auto)
        {
            ValidateEngine(engine, nameof(engine));
            byte[] nativeFramework = DnnStringConvert.ToNullTerminatedUtf8(framework, nameof(framework));
            if (modelBuffer.Length == 0)
            {
                throw new ArgumentException("Model buffer cannot be empty.", nameof(modelBuffer));
            }

            fixed (byte* modelPtr = modelBuffer)
            fixed (byte* configPtr = configBuffer)
            {
                NativeException.ThrowIfError(NativeMethods.DnnReadNetFromBuffer(nativeFramework, modelPtr, modelBuffer.Length, configPtr, configBuffer.Length, (int)engine, out IntPtr nativeHandle));
                return new Net(nativeHandle);
            }
        }
#endif

        /// <summary>
        /// Sets the preferable computation backend.
        /// 设置首选计算后端。
        /// </summary>
        public Net SetPreferableBackend(DnnBackend backend)
        {
            ValidateBackend(backend, nameof(backend));
            WithNativeHandle(nativeHandle =>
                NativeException.ThrowIfError(NativeMethods.DnnNetSetPreferableBackend(nativeHandle, (int)backend)));
            return this;
        }

        /// <summary>
        /// Sets the preferable computation target.
        /// 设置首选计算目标。
        /// </summary>
        public Net SetPreferableTarget(DnnTarget target)
        {
            ValidateTarget(target, nameof(target));
            WithNativeHandle(nativeHandle =>
                NativeException.ThrowIfError(NativeMethods.DnnNetSetPreferableTarget(nativeHandle, (int)target)));
            return this;
        }

        /// <summary>
        /// Sets an input blob.
        /// 设置输入 blob。
        /// </summary>
        public Net SetInput(Mat blob, string name = "", double scaleFactor = 1.0, Scalar? mean = null)
        {
            ValidateNotNull(blob, nameof(blob));
            Scalar actualMean = mean ?? new Scalar(0.0);
            byte[] nativeName = DnnStringConvert.ToNullTerminatedUtf8(name ?? string.Empty, nameof(name));
            IntPtr blobHandle = blob.NativeHandle;
            WithNativeHandle(nativeHandle =>
                NativeException.ThrowIfError(NativeMethods.DnnNetSetInput(nativeHandle, blobHandle, nativeName, scaleFactor, actualMean.V0, actualMean.V1, actualMean.V2, actualMean.V3)));
            return this;
        }

        /// <summary>
        /// Runs forward pass and writes the output blob.
        /// 执行 forward 并写入输出 blob。
        /// </summary>
        public void Forward(Mat output, string outputName = "")
        {
            ValidateNotNull(output, nameof(output));
            byte[] nativeOutputName = DnnStringConvert.ToNullTerminatedUtf8(outputName ?? string.Empty, nameof(outputName));
            IntPtr outputHandle = output.NativeHandle;
            WithNativeHandle(nativeHandle =>
                NativeException.ThrowIfError(NativeMethods.DnnNetForward(nativeHandle, nativeOutputName, outputHandle)));
        }

        /// <summary>
        /// Runs forward pass and returns the output blob.
        /// 执行 forward 并返回输出 blob。
        /// </summary>
        public Mat Forward(string outputName = "")
        {
            var output = new Mat();
            try
            {
                Forward(output, outputName);
                return output;
            }
            catch
            {
                output.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Runs forward pass for multiple output layer names.
        /// 对多个输出层名称执行 forward。
        /// </summary>
        public Mat[] Forward(string[] outputNames)
        {
            ValidateStringArray(outputNames, nameof(outputNames), allowEmpty: false);
            PackStringArray(outputNames, out byte[] buffer, out int[] offsets);
            var handles = new IntPtr[outputNames.Length];
            int outputCount = 0;
            WithNativeHandle(nativeHandle =>
                NativeException.ThrowIfError(NativeMethods.DnnNetForwardMany(
                    nativeHandle,
                    buffer,
                    offsets,
                    outputNames.Length,
                    handles,
                    handles.Length,
                    out outputCount)));
            if (outputCount != handles.Length)
            {
                ReleaseMatHandles(handles);
                throw new OpenCvException("Native DNN output count changed during retrieval.");
            }
            return ToMatArray(handles, outputCount);
        }

        /// <summary>
        /// Gets the numeric id for a layer name.
        /// 获取层名称对应的数值 id。
        /// </summary>
        public int GetLayerId(string layerName)
        {
            byte[] nativeLayerName = DnnStringConvert.ToNullTerminatedUtf8(layerName, nameof(layerName));
            return WithNativeHandle(nativeHandle =>
            {
                NativeException.ThrowIfError(NativeMethods.DnnNetGetLayerId(nativeHandle, nativeLayerName, out int layerId));
                return layerId;
            });
        }

        /// <summary>
        /// Gets ids of unconnected output layers.
        /// 获取未连接输出层的 id。
        /// </summary>
        public int[] GetUnconnectedOutLayers()
        {
            return WithNativeHandle(nativeHandle =>
            {
                NativeException.ThrowIfError(NativeMethods.DnnNetGetUnconnectedOutLayersCount(nativeHandle, out int layerCount));
                ValidateCount(layerCount, "Native DNN layer count");
                if (layerCount == 0)
                {
                    return Array.Empty<int>();
                }

                var layers = new int[layerCount];
                NativeException.ThrowIfError(NativeMethods.DnnNetGetUnconnectedOutLayersFill(nativeHandle, layers, layers.Length, out int written));
                if (written != layerCount) throw new OpenCvException("Native DNN layer count changed during retrieval.");
                return TrimArray(layers, written);
            });
        }

        /// <summary>
        /// Sets names for network input blobs.
        /// 设置网络输入 blob 名称。
        /// </summary>
        public Net SetInputsNames(string[] inputBlobNames)
        {
            ValidateStringArray(inputBlobNames, nameof(inputBlobNames), allowEmpty: true);
            PackStringArray(inputBlobNames, out byte[] buffer, out int[] offsets);
            WithNativeHandle(nativeHandle =>
                NativeException.ThrowIfError(NativeMethods.DnnNetSetInputsNames(nativeHandle, buffer, offsets, inputBlobNames.Length)));
            return this;
        }

        /// <summary>
        /// Sets the shape of a named network input.
        /// 设置命名网络输入的形状。
        /// </summary>
        public Net SetInputShape(string inputName, int[] shape)
        {
            ValidateShape(shape, nameof(shape));
            byte[] nativeInputName = DnnStringConvert.ToNullTerminatedUtf8(inputName, nameof(inputName));
            WithNativeHandle(nativeHandle =>
                NativeException.ThrowIfError(NativeMethods.DnnNetSetInputShape(nativeHandle, nativeInputName, shape, shape.Length)));
            return this;
        }

        /// <summary>
        /// Computes FLOPS for the whole network and an input shape.
        /// 根据输入形状计算整个网络的 FLOPS。
        /// </summary>
        public long GetFLOPS(int[] inputShape, int inputType = MatType.CV_32F)
        {
            ValidateShape(inputShape, nameof(inputShape));
            ValidateMatType(inputType, nameof(inputType));
            return WithNativeHandle(nativeHandle =>
            {
                NativeException.ThrowIfError(NativeMethods.DnnNetGetFLOPS(nativeHandle, inputShape, inputShape.Length, inputType, out long flops));
                return flops;
            });
        }

        /// <summary>
        /// Computes FLOPS for a specific layer and input shape.
        /// 根据输入形状计算指定层的 FLOPS。
        /// </summary>
        public long GetLayerFLOPS(int layerId, int[] inputShape, int inputType = MatType.CV_32F)
        {
            ValidateLayerId(layerId, nameof(layerId));
            ValidateShape(inputShape, nameof(inputShape));
            ValidateMatType(inputType, nameof(inputType));
            return WithNativeHandle(nativeHandle =>
            {
                NativeException.ThrowIfError(NativeMethods.DnnNetGetLayerFLOPS(nativeHandle, layerId, inputShape, inputShape.Length, inputType, out long flops));
                return flops;
            });
        }

        /// <summary>
        /// Gets performance profile timings from the last forward pass.
        /// 获取最近一次 forward 的性能剖析耗时。
        /// </summary>
        public DnnPerfProfile GetPerfProfile()
        {
            return WithNativeHandle(nativeHandle =>
            {
                NativeException.ThrowIfError(NativeMethods.DnnNetGetPerfProfileCount(nativeHandle, out int timingCount));
                ValidateCount(timingCount, "Native DNN timing count");
                var timings = new double[timingCount];
                NativeException.ThrowIfError(NativeMethods.DnnNetGetPerfProfileFill(nativeHandle, timings, timings.Length, out int written, out long tickCount));
                if (written != timingCount) throw new OpenCvException("Native DNN timing count changed during retrieval.");
                return new DnnPerfProfile(tickCount, TrimArray(timings, written));
            });
        }

        /// <summary>
        /// Gets all layer names.
        /// 获取所有层名称。
        /// </summary>
        public string[] GetLayerNames()
        {
            return GetStringArray(NativeMethods.DnnNetGetLayerNamesCount, NativeMethods.DnnNetGetLayerNamesFill);
        }

        /// <summary>
        /// Gets names of unconnected output layers.
        /// 获取未连接输出层名称。
        /// </summary>
        public string[] GetUnconnectedOutLayersNames()
        {
            return GetStringArray(NativeMethods.DnnNetGetUnconnectedOutLayersNamesCount, NativeMethods.DnnNetGetUnconnectedOutLayersNamesFill);
        }

        /// <summary>
        /// Gets layer type names present in the network.
        /// 获取网络中存在的层类型名称。
        /// </summary>
        public string[] GetLayerTypes()
        {
            return GetStringArray(NativeMethods.DnnNetGetLayerTypesCount, NativeMethods.DnnNetGetLayerTypesFill);
        }

        /// <summary>
        /// Gets the count of layers for a given layer type.
        /// 获取指定层类型的层数量。
        /// </summary>
        public int GetLayersCountByType(string layerType)
        {
            byte[] nativeLayerType = DnnStringConvert.ToNullTerminatedUtf8(layerType, nameof(layerType));
            return WithNativeHandle(nativeHandle =>
            {
                NativeException.ThrowIfError(NativeMethods.DnnNetGetLayersCountByType(nativeHandle, nativeLayerType, out int layerCount));
                ValidateCount(layerCount, "Native DNN layer count");
                return layerCount;
            });
        }

        /// <summary>
        /// Releases native resources.
        /// 释放 native 资源。
        /// </summary>
        public void Dispose()
        {
            handle.Dispose();
            GC.SuppressFinalize(this);
        }

        private string[] GetStringArray(
            StringArrayCount count,
            StringArrayFill fill)
        {
            return WithNativeHandle(nativeHandle =>
            {
                NativeException.ThrowIfError(count(nativeHandle, out int stringCount, out int byteCount));
                ValidateCount(stringCount, "Native DNN string count");
                ValidateCount(byteCount, "Native DNN string byte count");
                if (stringCount == 0)
                {
                    return Array.Empty<string>();
                }

                var offsets = new int[checked(stringCount + 1)];
                var buffer = new byte[byteCount];
                NativeException.ThrowIfError(fill(nativeHandle, offsets, offsets.Length, buffer, buffer.Length, out int writtenStrings, out int writtenBytes));
                if (writtenStrings != stringCount || writtenBytes != byteCount)
                    throw new OpenCvException("Native DNN string data changed during count/fill retrieval.");
                return DecodePackedStrings(offsets, writtenStrings, buffer, writtenBytes);
            });
        }

        private static void PackStringArray(string[] values, out byte[] buffer, out int[] offsets)
        {
            offsets = new int[values.Length + 1];
            int byteCount = 0;
            for (int i = 0; i < values.Length; i++)
            {
                byteCount = checked(byteCount + DnnStringConvert.ToUtf8Bytes(values[i], nameof(values), true).Length);
                offsets[i + 1] = byteCount;
            }

            buffer = new byte[byteCount == 0 ? 1 : byteCount];
            int cursor = 0;
            for (int i = 0; i < values.Length; i++)
            {
                byte[] encoded = DnnStringConvert.ToUtf8Bytes(values[i], nameof(values), true);
                Array.Copy(encoded, 0, buffer, cursor, encoded.Length);
                cursor = checked(cursor + encoded.Length);
            }
        }

        private static Mat[] ToMatArray(IntPtr[] handles, int outputCount)
        {
            Mat[]? result = null;
            int created = 0;
            try
            {
                ValidateWrittenCount(outputCount, handles.Length, "Native DNN Mat count");
                result = new Mat[outputCount];
                for (; created < result.Length; created++)
                {
                    if (handles[created] == IntPtr.Zero)
                    {
                        throw new OpenCvException("Native DNN Mat handle is null.");
                    }

                    result[created] = new Mat(handles[created]);
                    handles[created] = IntPtr.Zero;
                }

                return result;
            }
            catch
            {
                if (result != null)
                    for (int i = 0; i < created; i++) result[i]?.Dispose();
                throw;
            }
            finally
            {
                ReleaseMatHandles(handles);
            }
        }

        private static int[] TrimArray(int[] values, int count)
        {
            int length = Math.Max(0, Math.Min(count, values.Length));
            if (length == values.Length)
            {
                return values;
            }

            var result = new int[length];
            Array.Copy(values, result, length);
            return result;
        }

        private static double[] TrimArray(double[] values, int count)
        {
            int length = Math.Max(0, Math.Min(count, values.Length));
            if (length == values.Length)
            {
                return values;
            }

            var result = new double[length];
            Array.Copy(values, result, length);
            return result;
        }

        private void WithNativeHandle(Action<IntPtr> action)
        {
            WithNativeHandle(nativeHandle =>
            {
                action(nativeHandle);
                return true;
            });
        }

        private T WithNativeHandle<T>(Func<IntPtr, T> action)
        {
            bool addedReference = false;
            try
            {
                handle.DangerousAddRef(ref addedReference);
                if (handle.IsInvalid) throw new ObjectDisposedException(nameof(Net));
                return action(handle.DangerousGetHandle());
            }
            finally
            {
                if (addedReference) handle.DangerousRelease();
            }
        }

        internal IntPtr NativeHandle
        {
            get
            {
                ThrowIfDisposed();
                return handle.DangerousGetHandle();
            }
        }

        private static string[] DecodePackedStrings(int[] offsets, int stringCount, byte[] buffer, int byteCount)
        {
            if (offsets == null || buffer == null || stringCount < 0 || byteCount < 0 ||
                offsets.Length < checked(stringCount + 1) || byteCount > buffer.Length)
                throw new OpenCvException("Native DNN packed string metadata is invalid.");
            if (offsets[0] != 0 || offsets[stringCount] != byteCount)
                throw new OpenCvException("Native DNN packed string offsets are invalid.");
            var result = new string[stringCount];
            for (int i = 0; i < stringCount; i++)
            {
                int start = offsets[i];
                int end = offsets[i + 1];
                if (start < 0 || end < start || end > byteCount)
                    throw new OpenCvException("Native DNN packed string offsets are invalid.");
                result[i] = DnnStringConvert.FromUtf8Bytes(buffer, start, end - start);
            }
            return result;
        }

        private static void ReleaseMatHandles(IntPtr[] handles)
        {
            for (int i = 0; i < handles.Length; i++)
            {
                if (handles[i] == IntPtr.Zero) continue;
                NativeMethods.MatRelease(handles[i]);
                handles[i] = IntPtr.Zero;
            }
        }

        private static void ValidateCount(int value, string description)
        {
            if (value < 0) throw new OpenCvException(description + " is negative.");
        }

        private static void ValidateWrittenCount(int value, int capacity, string description)
        {
            if (value < 0 || value > capacity) throw new OpenCvException(description + " exceeds the supplied capacity.");
        }

        private static void ValidateOffsets(int[] offsets, int valueCount, string description)
        {
            if (offsets == null || offsets.Length == 0 || valueCount < 0 || offsets[0] != 0 || offsets[offsets.Length - 1] != valueCount)
                throw new OpenCvException(description + " are invalid.");
            for (int i = 0; i + 1 < offsets.Length; i++)
                if (offsets[i] < 0 || offsets[i + 1] < offsets[i] || offsets[i + 1] > valueCount)
                    throw new OpenCvException(description + " are invalid.");
        }

        private static void ValidateShape(int[] value, string parameterName)
        {
            ValidateNotNull(value, parameterName);
            if (value.Length > 10) throw new ArgumentException("DNN shapes cannot contain more than 10 dimensions.", parameterName);
        }

        private static void ValidateMatType(int value, string parameterName)
        {
            if (value < 0 || (value & ~MatType.MatrixTypeMask) != 0 || MatType.Depth(value) >= 13)
                throw new ArgumentOutOfRangeException(parameterName, "Value is not a supported OpenCV matrix type.");
        }

        private void ThrowIfDisposed()
        {
            if (handle.IsClosed || handle.IsInvalid)
            {
                throw new ObjectDisposedException(nameof(Net));
            }
        }

        private static void ValidateNotNull<T>(T value, string parameterName)
            where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }

        private static void ValidateNotEmpty(byte[] value, string parameterName)
        {
            if (value.Length == 0)
            {
                throw new ArgumentException("Model buffer cannot be empty.", parameterName);
            }
        }

        private static void ValidateBuffer(byte[] value, string parameterName)
        {
            ValidateNotNull(value, parameterName);
            ValidateNotEmpty(value, parameterName);
        }

        private static void ValidateEngine(DnnEngine value, string parameterName)
        {
            if (value != DnnEngine.Auto
                && value != DnnEngine.Classic
                && value != DnnEngine.New
                && value != DnnEngine.Ort)
            {
                throw new ArgumentOutOfRangeException(parameterName, "DNN engine must be Auto, Classic, New, or Ort.");
            }
        }

        private static void ValidateBackend(DnnBackend value, string parameterName)
        {
            if (value != DnnBackend.Default
                && value != DnnBackend.InferenceEngine
                && value != DnnBackend.OpenCV
                && value != DnnBackend.VkCom
                && value != DnnBackend.Cuda
                && value != DnnBackend.WebNN
                && value != DnnBackend.TimVx
                && value != DnnBackend.Cann)
            {
                throw new ArgumentOutOfRangeException(parameterName, "DNN backend must be a defined backend.");
            }
        }

        private static void ValidateTarget(DnnTarget value, string parameterName)
        {
            if (value != DnnTarget.Cpu
                && value != DnnTarget.OpenCL
                && value != DnnTarget.OpenCLFp16
                && value != DnnTarget.Myriad
                && value != DnnTarget.Vulkan
                && value != DnnTarget.Fpga
                && value != DnnTarget.Cuda
                && value != DnnTarget.CudaFp16
                && value != DnnTarget.Hddl
                && value != DnnTarget.Npu
                && value != DnnTarget.CpuFp16)
            {
                throw new ArgumentOutOfRangeException(parameterName, "DNN target must be a defined target.");
            }
        }

        private static void ValidateStringArray(string[] values, string parameterName, bool allowEmpty)
        {
            ValidateNotNull(values, parameterName);
            if (!allowEmpty && values.Length == 0)
            {
                throw new ArgumentException("At least one name is required.", parameterName);
            }

            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] == null)
                {
                    throw new ArgumentNullException(parameterName);
                }

                DnnStringConvert.ToUtf8Bytes(values[i], parameterName, true);
            }
        }
    }
}
