using System;
using System.Text;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Dnn
{
    /// <summary>
    /// Deep neural network object compatible with OpenCV <c>cv::dnn::Net</c>.
    /// 与 OpenCV <c>cv::dnn::Net</c> 兼容的深度神经网络对象。
    /// </summary>
    public sealed unsafe class Net : IDisposable
    {
        private delegate int StringArrayCount(IntPtr net, out int stringCount, out int byteCount);

        private delegate int StringArrayFill(IntPtr net, int[] offsets, int offsetCapacity, byte[] buffer, int bufferCapacity, out int stringCount, out int byteCount);

        private NativeDnnNetHandle handle;
        private bool disposed;

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
            get { return disposed; }
        }

        /// <summary>
        /// Gets whether the network contains no layers.
        /// 获取网络是否不包含任何层。
        /// </summary>
        public bool Empty
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.DnnNetEmpty(NativeHandle, out int empty));
                return empty != 0;
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
            ThrowIfDisposed();
            ValidateBackend(backend, nameof(backend));
            NativeException.ThrowIfError(NativeMethods.DnnNetSetPreferableBackend(NativeHandle, (int)backend));
            return this;
        }

        /// <summary>
        /// Sets the preferable computation target.
        /// 设置首选计算目标。
        /// </summary>
        public Net SetPreferableTarget(DnnTarget target)
        {
            ThrowIfDisposed();
            ValidateTarget(target, nameof(target));
            NativeException.ThrowIfError(NativeMethods.DnnNetSetPreferableTarget(NativeHandle, (int)target));
            return this;
        }

        /// <summary>
        /// Sets an input blob.
        /// 设置输入 blob。
        /// </summary>
        public Net SetInput(Mat blob, string name = "", double scaleFactor = 1.0, Scalar? mean = null)
        {
            ThrowIfDisposed();
            ValidateNotNull(blob, nameof(blob));
            Scalar actualMean = mean ?? new Scalar(0.0);
            byte[] nativeName = DnnStringConvert.ToNullTerminatedUtf8(name ?? string.Empty, nameof(name));
            NativeException.ThrowIfError(NativeMethods.DnnNetSetInput(NativeHandle, blob.NativeHandle, nativeName, scaleFactor, actualMean.V0, actualMean.V1, actualMean.V2, actualMean.V3));
            return this;
        }

        /// <summary>
        /// Runs forward pass and writes the output blob.
        /// 执行 forward 并写入输出 blob。
        /// </summary>
        public void Forward(Mat output, string outputName = "")
        {
            ThrowIfDisposed();
            ValidateNotNull(output, nameof(output));
            byte[] nativeOutputName = DnnStringConvert.ToNullTerminatedUtf8(outputName ?? string.Empty, nameof(outputName));
            NativeException.ThrowIfError(NativeMethods.DnnNetForward(NativeHandle, nativeOutputName, output.NativeHandle));
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
            ThrowIfDisposed();
            ValidateStringArray(outputNames, nameof(outputNames), allowEmpty: false);
            PackStringArray(outputNames, out byte[] buffer, out int[] offsets);
            var handles = new IntPtr[outputNames.Length];
            NativeException.ThrowIfError(NativeMethods.DnnNetForwardMany(
                NativeHandle,
                buffer,
                offsets,
                outputNames.Length,
                handles,
                handles.Length,
                out int outputCount));
            return ToMatArray(handles, outputCount);
        }

        /// <summary>
        /// Gets the numeric id for a layer name.
        /// 获取层名称对应的数值 id。
        /// </summary>
        public int GetLayerId(string layerName)
        {
            ThrowIfDisposed();
            byte[] nativeLayerName = DnnStringConvert.ToNullTerminatedUtf8(layerName, nameof(layerName));
            NativeException.ThrowIfError(NativeMethods.DnnNetGetLayerId(NativeHandle, nativeLayerName, out int layerId));
            return layerId;
        }

        /// <summary>
        /// Gets ids of unconnected output layers.
        /// 获取未连接输出层的 id。
        /// </summary>
        public int[] GetUnconnectedOutLayers()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.DnnNetGetUnconnectedOutLayersCount(NativeHandle, out int layerCount));
            if (layerCount <= 0)
            {
                return Array.Empty<int>();
            }

            var layers = new int[layerCount];
            NativeException.ThrowIfError(NativeMethods.DnnNetGetUnconnectedOutLayersFill(NativeHandle, layers, layers.Length, out int written));
            return TrimArray(layers, written);
        }

        /// <summary>
        /// Sets names for network input blobs.
        /// 设置网络输入 blob 名称。
        /// </summary>
        public Net SetInputsNames(string[] inputBlobNames)
        {
            ThrowIfDisposed();
            ValidateStringArray(inputBlobNames, nameof(inputBlobNames), allowEmpty: true);
            PackStringArray(inputBlobNames, out byte[] buffer, out int[] offsets);
            NativeException.ThrowIfError(NativeMethods.DnnNetSetInputsNames(NativeHandle, buffer, offsets, inputBlobNames.Length));
            return this;
        }

        /// <summary>
        /// Sets the shape of a named network input.
        /// 设置命名网络输入的形状。
        /// </summary>
        public Net SetInputShape(string inputName, int[] shape)
        {
            ThrowIfDisposed();
            ValidateNotNull(shape, nameof(shape));
            byte[] nativeInputName = DnnStringConvert.ToNullTerminatedUtf8(inputName, nameof(inputName));
            NativeException.ThrowIfError(NativeMethods.DnnNetSetInputShape(NativeHandle, nativeInputName, shape, shape.Length));
            return this;
        }

        /// <summary>
        /// Computes FLOPS for the whole network and an input shape.
        /// 根据输入形状计算整个网络的 FLOPS。
        /// </summary>
        public long GetFLOPS(int[] inputShape, int inputType = MatType.CV_32F)
        {
            ThrowIfDisposed();
            ValidateNotNull(inputShape, nameof(inputShape));
            NativeException.ThrowIfError(NativeMethods.DnnNetGetFLOPS(NativeHandle, inputShape, inputShape.Length, inputType, out long flops));
            return flops;
        }

        /// <summary>
        /// Computes FLOPS for a specific layer and input shape.
        /// 根据输入形状计算指定层的 FLOPS。
        /// </summary>
        public long GetLayerFLOPS(int layerId, int[] inputShape, int inputType = MatType.CV_32F)
        {
            ThrowIfDisposed();
            ValidateNotNull(inputShape, nameof(inputShape));
            NativeException.ThrowIfError(NativeMethods.DnnNetGetLayerFLOPS(NativeHandle, layerId, inputShape, inputShape.Length, inputType, out long flops));
            return flops;
        }

        /// <summary>
        /// Gets performance profile timings from the last forward pass.
        /// 获取最近一次 forward 的性能剖析耗时。
        /// </summary>
        public DnnPerfProfile GetPerfProfile()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.DnnNetGetPerfProfileCount(NativeHandle, out int timingCount));
            var timings = new double[Math.Max(timingCount, 0)];
            NativeException.ThrowIfError(NativeMethods.DnnNetGetPerfProfileFill(NativeHandle, timings, timings.Length, out int written, out long tickCount));
            return new DnnPerfProfile(tickCount, TrimArray(timings, written));
        }

        /// <summary>
        /// Gets all layer names.
        /// 获取所有层名称。
        /// </summary>
        public string[] GetLayerNames()
        {
            ThrowIfDisposed();
            return GetStringArray(NativeMethods.DnnNetGetLayerNamesCount, NativeMethods.DnnNetGetLayerNamesFill);
        }

        /// <summary>
        /// Gets names of unconnected output layers.
        /// 获取未连接输出层名称。
        /// </summary>
        public string[] GetUnconnectedOutLayersNames()
        {
            ThrowIfDisposed();
            return GetStringArray(NativeMethods.DnnNetGetUnconnectedOutLayersNamesCount, NativeMethods.DnnNetGetUnconnectedOutLayersNamesFill);
        }

        /// <summary>
        /// Gets layer type names present in the network.
        /// 获取网络中存在的层类型名称。
        /// </summary>
        public string[] GetLayerTypes()
        {
            ThrowIfDisposed();
            return GetStringArray(NativeMethods.DnnNetGetLayerTypesCount, NativeMethods.DnnNetGetLayerTypesFill);
        }

        /// <summary>
        /// Gets the count of layers for a given layer type.
        /// 获取指定层类型的层数量。
        /// </summary>
        public int GetLayersCountByType(string layerType)
        {
            ThrowIfDisposed();
            byte[] nativeLayerType = DnnStringConvert.ToNullTerminatedUtf8(layerType, nameof(layerType));
            NativeException.ThrowIfError(NativeMethods.DnnNetGetLayersCountByType(NativeHandle, nativeLayerType, out int layerCount));
            return layerCount;
        }

        /// <summary>
        /// Releases native resources.
        /// 释放 native 资源。
        /// </summary>
        public void Dispose()
        {
            if (!disposed)
            {
                handle.Dispose();
                disposed = true;
                GC.SuppressFinalize(this);
            }
        }

        private string[] GetStringArray(
            StringArrayCount count,
            StringArrayFill fill)
        {
            NativeException.ThrowIfError(count(NativeHandle, out int stringCount, out int byteCount));
            if (stringCount <= 0)
            {
                return Array.Empty<string>();
            }

            var offsets = new int[stringCount + 1];
            var buffer = new byte[Math.Max(byteCount, 0)];
            NativeException.ThrowIfError(fill(NativeHandle, offsets, offsets.Length, buffer, buffer.Length, out int writtenStrings, out int writtenBytes));
            int resultCount = Math.Max(0, Math.Min(writtenStrings, stringCount));
            var result = new string[resultCount];
            for (int i = 0; i < result.Length; i++)
            {
                int start = Math.Max(0, Math.Min(offsets[i], buffer.Length));
                int end = Math.Max(start, Math.Min(offsets[i + 1], Math.Min(writtenBytes, buffer.Length)));
                result[i] = DnnStringConvert.FromUtf8Bytes(buffer, start, end - start);
            }

            return result;
        }

        private static void PackStringArray(string[] values, out byte[] buffer, out int[] offsets)
        {
            offsets = new int[values.Length + 1];
            int byteCount = 0;
            for (int i = 0; i < values.Length; i++)
            {
                byteCount += Encoding.UTF8.GetByteCount(values[i]);
                offsets[i + 1] = byteCount;
            }

            buffer = new byte[byteCount == 0 ? 1 : byteCount];
            int cursor = 0;
            for (int i = 0; i < values.Length; i++)
            {
                cursor += Encoding.UTF8.GetBytes(values[i], 0, values[i].Length, buffer, cursor);
            }
        }

        private static Mat[] ToMatArray(IntPtr[] handles, int outputCount)
        {
            int count = Math.Max(0, Math.Min(outputCount, handles.Length));
            var result = new Mat[count];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = new Mat(handles[i]);
            }

            return result;
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

        private void ThrowIfDisposed()
        {
            if (disposed)
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

        private static void ValidateEngine(DnnEngine value, string parameterName)
        {
            if (value != DnnEngine.Auto
                && value != DnnEngine.Classic
                && value != DnnEngine.New)
            {
                throw new ArgumentOutOfRangeException(parameterName, "DNN engine must be Auto, Classic, or New.");
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
            }
        }
    }
}
