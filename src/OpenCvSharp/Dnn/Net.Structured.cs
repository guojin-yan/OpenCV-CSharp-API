using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Dnn
{
    public sealed unsafe partial class Net
    {
        /// <summary>Returns OpenCV's textual representation of this network.</summary>
        /// <returns>A managed UTF-8-decoded copy of the network description.</returns>
        /// <remarks>The returned string is independently owned and remains valid after this network is disposed.</remarks>
        public string Dump()
        {
            return WithNativeHandle(nativeHandle =>
            {
                NativeException.ThrowIfError(NativeMethods.DnnNetDump(nativeHandle, out IntPtr result));
                return CorePersistenceMarshal.ReadUtf8Result(result);
            });
        }

        /// <summary>Writes OpenCV's textual network representation to a UTF-8 path.</summary>
        /// <param name="path">Destination path. Embedded null characters are rejected.</param>
        public void DumpToFile(string path)
        {
            byte[] nativePath = DnnStringConvert.ToNullTerminatedUtf8(path, nameof(path));
            WithNativeHandle(nativeHandle =>
                NativeException.ThrowIfError(NativeMethods.DnnNetDumpToFile(nativeHandle, nativePath)));
        }

        /// <summary>Writes an OpenCV-compatible PBTXT representation to a UTF-8 path.</summary>
        /// <param name="path">Destination path. Embedded null characters are rejected.</param>
        public void DumpToPbtxt(string path)
        {
            byte[] nativePath = DnnStringConvert.ToNullTerminatedUtf8(path, nameof(path));
            WithNativeHandle(nativeHandle =>
                NativeException.ThrowIfError(NativeMethods.DnnNetDumpToPbtxt(nativeHandle, nativePath)));
        }

        /// <summary>Connects an output pin to an input pin using OpenCV pin descriptor syntax.</summary>
        /// <param name="outputPin">Source pin descriptor encoded as UTF-8 for OpenCV.</param>
        /// <param name="inputPin">Destination pin descriptor encoded as UTF-8 for OpenCV.</param>
        /// <returns>This network, for fluent configuration.</returns>
        /// <remarks>This mutates the graph and must be called before the affected inference execution.</remarks>
        public Net Connect(string outputPin, string inputPin)
        {
            byte[] nativeOutputPin = DnnStringConvert.ToNullTerminatedUtf8(outputPin, nameof(outputPin));
            byte[] nativeInputPin = DnnStringConvert.ToNullTerminatedUtf8(inputPin, nameof(inputPin));
            WithNativeHandle(nativeHandle =>
                NativeException.ThrowIfError(NativeMethods.DnnNetConnect(nativeHandle, nativeOutputPin, nativeInputPin)));
            return this;
        }

        /// <summary>Registers a named network output and returns its layer id.</summary>
        /// <param name="outputName">Public output name encoded as UTF-8 for OpenCV.</param>
        /// <param name="layerId">Source layer identifier returned by OpenCV.</param>
        /// <param name="outputPort">Zero-based output port on the source layer.</param>
        /// <returns>The identifier of the registered output layer.</returns>
        public int RegisterOutput(string outputName, int layerId, int outputPort)
        {
            ValidateLayerId(layerId, nameof(layerId));
            if (outputPort < 0) throw new ArgumentOutOfRangeException(nameof(outputPort));
            byte[] nativeOutputName = DnnStringConvert.ToNullTerminatedUtf8(outputName, nameof(outputName));
            return WithNativeHandle(nativeHandle =>
            {
                NativeException.ThrowIfError(NativeMethods.DnnNetRegisterOutput(nativeHandle, nativeOutputName, layerId, outputPort, out int registeredLayerId));
                return registeredLayerId;
            });
        }

        /// <summary>Gets an independently owned ref-counted layer reference by numeric id.</summary>
        /// <param name="layerId">Layer identifier returned by OpenCV.</param>
        /// <returns>A disposable, ref-counted layer reference that can outlive this <see cref="Net"/>.</returns>
        public Layer GetLayer(int layerId)
        {
            ValidateLayerId(layerId, nameof(layerId));
            return WithNativeHandle(nativeHandle =>
            {
                NativeException.ThrowIfError(NativeMethods.DnnNetGetLayerById(nativeHandle, layerId, out IntPtr layer));
                return new Layer(layer);
            });
        }

        /// <summary>Gets an independently owned ref-counted layer reference by name.</summary>
        /// <param name="layerName">Layer or tensor name encoded as UTF-8 for OpenCV.</param>
        /// <returns>A disposable, ref-counted layer reference that can outlive this <see cref="Net"/>.</returns>
        public Layer GetLayer(string layerName)
        {
            byte[] nativeLayerName = DnnStringConvert.ToNullTerminatedUtf8(layerName, nameof(layerName));
            return WithNativeHandle(nativeHandle =>
            {
                NativeException.ThrowIfError(NativeMethods.DnnNetGetLayerByName(nativeHandle, nativeLayerName, out IntPtr layer));
                return new Layer(layer);
            });
        }

        /// <summary>Runs a nested multi-output forward pass.</summary>
        /// <param name="outputNames">Requested output names in result order; the array must not be empty.</param>
        /// <returns>One independently disposable matrix group per requested output.</returns>
        /// <remarks>Every returned <see cref="Mat"/> owns an OpenCV reference. Partial native results are released if retrieval fails.</remarks>
        public Mat[][] ForwardAndRetrieve(string[] outputNames)
        {
            ValidateStringArray(outputNames, nameof(outputNames), false);
            PackStringArray(outputNames, out byte[] names, out int[] offsets);
            return WithNativeHandle(nativeHandle =>
            {
                NativeException.ThrowIfError(NativeMethods.DnnNetForwardAndRetrieve(nativeHandle, names, offsets, outputNames.Length, out IntPtr nativeResult));
                using (NativeDnnMatGroupsHandle result = NativeDnnMatGroupsHandle.FromNativePointer(nativeResult))
                {
                    return TakeMatGroups(result);
                }
            });
        }

        /// <summary>Finalizes backend and target configuration before inference.</summary>
        /// <returns>This network, for fluent configuration.</returns>
        /// <remarks>Backend support is runtime-dependent. This method does not make an unavailable backend available.</remarks>
        public Net FinalizeNetwork()
        {
            WithNativeHandle(nativeHandle =>
                NativeException.ThrowIfError(NativeMethods.DnnNetFinalize(nativeHandle)));
            return this;
        }

        /// <summary>Sets OpenCV DNN tracing behavior.</summary>
        /// <param name="mode">Tracing detail defined by OpenCV 5.0.0.</param>
        /// <returns>This network, for fluent configuration.</returns>
        public Net SetTracingMode(DnnTracingMode mode)
        {
            ValidateTracingMode(mode, nameof(mode));
            WithNativeHandle(nativeHandle =>
                NativeException.ThrowIfError(NativeMethods.DnnNetSetTracingMode(nativeHandle, (int)mode)));
            return this;
        }

        /// <summary>Gets OpenCV DNN tracing behavior.</summary>
        /// <returns>The tracing mode currently retained by the network.</returns>
        public DnnTracingMode GetTracingMode()
        {
            return WithNativeHandle(nativeHandle =>
            {
                NativeException.ThrowIfError(NativeMethods.DnnNetGetTracingMode(nativeHandle, out int mode));
                return ToTracingMode(mode);
            });
        }

        /// <summary>Sets OpenCV DNN profiling behavior.</summary>
        /// <param name="mode">Profiling detail defined by OpenCV 5.0.0.</param>
        /// <returns>This network, for fluent configuration.</returns>
        public Net SetProfilingMode(DnnProfilingMode mode)
        {
            ValidateProfilingMode(mode, nameof(mode));
            WithNativeHandle(nativeHandle =>
                NativeException.ThrowIfError(NativeMethods.DnnNetSetProfilingMode(nativeHandle, (int)mode)));
            return this;
        }

        /// <summary>Gets OpenCV DNN profiling behavior.</summary>
        /// <returns>The profiling mode currently retained by the network.</returns>
        public DnnProfilingMode GetProfilingMode()
        {
            return WithNativeHandle(nativeHandle =>
            {
                NativeException.ThrowIfError(NativeMethods.DnnNetGetProfilingMode(nativeHandle, out int mode));
                return ToProfilingMode(mode);
            });
        }

        /// <summary>Gets the model format retained by this network.</summary>
        /// <remarks>Classic-engine ONNX loaders may report <see cref="DnnModelFormat.Generic"/>; the value describes OpenCV's retained graph metadata.</remarks>
        public DnnModelFormat ModelFormat
        {
            get
            {
                return WithNativeHandle(nativeHandle =>
                {
                    NativeException.ThrowIfError(NativeMethods.DnnNetGetModelFormat(nativeHandle, out int format));
                    return ToModelFormat(format);
                });
            }
        }

        /// <summary>Sets a learned parameter blob by layer id.</summary>
        /// <param name="layerId">Layer identifier returned by OpenCV.</param>
        /// <param name="parameterIndex">Zero-based parameter index.</param>
        /// <param name="value">Caller-owned parameter matrix. Native code does not retain its managed handle.</param>
        /// <returns>This network, for fluent configuration.</returns>
        public Net SetParam(int layerId, int parameterIndex, Mat value)
        {
            ValidateLayerId(layerId, nameof(layerId));
            ValidateParameterIndex(parameterIndex, nameof(parameterIndex));
            ValidateNotNull(value, nameof(value));
            IntPtr valueHandle = value.NativeHandle;
            WithNativeHandle(nativeHandle =>
                NativeException.ThrowIfError(NativeMethods.DnnNetSetParamById(nativeHandle, layerId, parameterIndex, valueHandle)));
            return this;
        }

        /// <summary>Sets a learned parameter blob by layer or tensor name.</summary>
        /// <param name="layerName">Layer or tensor name encoded as UTF-8 for OpenCV.</param>
        /// <param name="parameterIndex">Zero-based parameter index.</param>
        /// <param name="value">Caller-owned parameter matrix. Native code does not retain its managed handle.</param>
        /// <returns>This network, for fluent configuration.</returns>
        public Net SetParam(string layerName, int parameterIndex, Mat value)
        {
            ValidateParameterIndex(parameterIndex, nameof(parameterIndex));
            ValidateNotNull(value, nameof(value));
            byte[] nativeLayerName = DnnStringConvert.ToNullTerminatedUtf8(layerName, nameof(layerName));
            IntPtr valueHandle = value.NativeHandle;
            WithNativeHandle(nativeHandle =>
                NativeException.ThrowIfError(NativeMethods.DnnNetSetParamByName(nativeHandle, nativeLayerName, parameterIndex, valueHandle)));
            return this;
        }

        /// <summary>Gets a learned parameter blob by layer id.</summary>
        /// <param name="layerId">Layer identifier returned by OpenCV.</param>
        /// <param name="parameterIndex">Zero-based parameter index.</param>
        /// <returns>An independently disposable <see cref="Mat"/> containing OpenCV's ref-counted parameter data.</returns>
        public Mat GetParam(int layerId, int parameterIndex = 0)
        {
            ValidateLayerId(layerId, nameof(layerId));
            ValidateParameterIndex(parameterIndex, nameof(parameterIndex));
            var value = new Mat();
            try
            {
                IntPtr valueHandle = value.NativeHandle;
                WithNativeHandle(nativeHandle =>
                    NativeException.ThrowIfError(NativeMethods.DnnNetGetParamById(nativeHandle, layerId, parameterIndex, valueHandle)));
                return value;
            }
            catch
            {
                value.Dispose();
                throw;
            }
        }

        /// <summary>Gets a learned parameter blob by layer or tensor name.</summary>
        /// <param name="layerName">Layer or tensor name encoded as UTF-8 for OpenCV.</param>
        /// <param name="parameterIndex">Zero-based parameter index.</param>
        /// <returns>An independently disposable <see cref="Mat"/> containing OpenCV's ref-counted parameter data.</returns>
        public Mat GetParam(string layerName, int parameterIndex = 0)
        {
            ValidateParameterIndex(parameterIndex, nameof(parameterIndex));
            byte[] nativeLayerName = DnnStringConvert.ToNullTerminatedUtf8(layerName, nameof(layerName));
            var value = new Mat();
            try
            {
                IntPtr valueHandle = value.NativeHandle;
                WithNativeHandle(nativeHandle =>
                    NativeException.ThrowIfError(NativeMethods.DnnNetGetParamByName(nativeHandle, nativeLayerName, parameterIndex, valueHandle)));
                return value;
            }
            catch
            {
                value.Dispose();
                throw;
            }
        }

        /// <summary>Infers input and output shapes for one layer from all network input shapes and types.</summary>
        /// <param name="inputShapes">One dimension array per network input. Dynamic and zero-length shapes are forwarded to OpenCV.</param>
        /// <param name="inputTypes">One OpenCV Mat type per input shape.</param>
        /// <param name="layerId">Layer identifier returned by OpenCV.</param>
        /// <returns>Independent managed copies of the inferred input and output shapes.</returns>
        public DnnLayerShapes GetLayerShapes(int[][] inputShapes, int[] inputTypes, int layerId)
        {
            ValidateLayerId(layerId, nameof(layerId));
            PackInputShapes(inputShapes, inputTypes, out int[] offsets, out int[] values, out int[] types);
            return WithNativeHandle(nativeHandle => GetLayerShapesCore(nativeHandle, offsets, values, types, layerId));
        }

        /// <summary>Computes FLOPS for all network inputs.</summary>
        /// <param name="inputShapes">One dimension array per network input.</param>
        /// <param name="inputTypes">One OpenCV Mat type per input shape.</param>
        /// <returns>OpenCV's estimated floating-point operation count.</returns>
        public long GetFLOPS(int[][] inputShapes, int[] inputTypes)
        {
            PackInputShapes(inputShapes, inputTypes, out int[] offsets, out int[] values, out int[] types);
            return WithNativeHandle(nativeHandle =>
            {
                NativeException.ThrowIfError(NativeMethods.DnnNetGetFLOPSMany(nativeHandle, offsets, inputShapes.Length, values, values.Length, types, types.Length, out long flops));
                return flops;
            });
        }

        /// <summary>Estimates parameter and intermediate-blob memory for all network inputs.</summary>
        /// <param name="inputShapes">One dimension array per network input.</param>
        /// <param name="inputTypes">One OpenCV Mat type per input shape.</param>
        /// <returns>Weight and intermediate-blob byte counts reported by OpenCV.</returns>
        public DnnMemoryConsumption GetMemoryConsumption(int[][] inputShapes, int[] inputTypes)
        {
            PackInputShapes(inputShapes, inputTypes, out int[] offsets, out int[] values, out int[] types);
            return WithNativeHandle(nativeHandle =>
            {
                NativeException.ThrowIfError(NativeMethods.DnnNetGetMemoryConsumption(nativeHandle, offsets, inputShapes.Length, values, values.Length, types, types.Length, out ulong weightsBytes, out ulong blobBytes));
                return new DnnMemoryConsumption(weightsBytes, blobBytes);
            });
        }

        /// <summary>Enables or disables graph fusion.</summary>
        /// <param name="enabled"><see langword="true"/> to permit supported fusion passes.</param>
        /// <returns>This network, for fluent configuration.</returns>
        public Net EnableFusion(bool enabled)
        {
            WithNativeHandle(nativeHandle =>
                NativeException.ThrowIfError(NativeMethods.DnnNetEnableFusion(nativeHandle, enabled ? 1 : 0)));
            return this;
        }

        /// <summary>Enables or disables Winograd convolution where OpenCV can use it.</summary>
        /// <param name="enabled"><see langword="true"/> to permit supported Winograd convolution paths.</param>
        /// <returns>This network, for fluent configuration.</returns>
        public Net EnableWinograd(bool enabled)
        {
            WithNativeHandle(nativeHandle =>
                NativeException.ThrowIfError(NativeMethods.DnnNetEnableWinograd(nativeHandle, enabled ? 1 : 0)));
            return this;
        }

        /// <summary>Enables the OpenCV DNN key-value cache.</summary>
        /// <returns>This network, for fluent configuration.</returns>
        /// <remarks>KV cache is supported by OpenCV's new-engine graph path. Unsupported Classic graphs fail through <see cref="OpenCvException"/>.</remarks>
        public Net EnableKvCache()
        {
            WithNativeHandle(nativeHandle =>
                NativeException.ThrowIfError(NativeMethods.DnnNetEnableKvCache(nativeHandle)));
            return this;
        }

        /// <summary>Disables the OpenCV DNN key-value cache.</summary>
        /// <returns>This network, for fluent configuration.</returns>
        public Net DisableKvCache()
        {
            WithNativeHandle(nativeHandle =>
                NativeException.ThrowIfError(NativeMethods.DnnNetDisableKvCache(nativeHandle)));
            return this;
        }

        /// <summary>Clears accumulated OpenCV DNN key-value cache state.</summary>
        /// <returns>This network, for fluent configuration.</returns>
        public Net ResetKvCache()
        {
            WithNativeHandle(nativeHandle =>
                NativeException.ThrowIfError(NativeMethods.DnnNetResetKvCache(nativeHandle)));
            return this;
        }

        /// <summary>Gets structured detailed profiling text from the most recent network execution.</summary>
        /// <returns>Independent managed copies of the name, timing, and invocation-count columns.</returns>
        /// <remarks>Timing text is backend-defined. Count/fill retrieval fails closed if the native row set changes between calls.</remarks>
        public DnnDetailedPerfProfile GetDetailedPerfProfile()
        {
            return WithNativeHandle(nativeHandle =>
            {
                NativeException.ThrowIfError(NativeMethods.DnnNetGetDetailedPerfProfileCount(nativeHandle, out int rowCount, out int nameByteCount, out int timeByteCount, out int invocationByteCount));
                ValidateCount(rowCount, "Native DNN profile row count");
                ValidateCount(nameByteCount, "Native DNN profile name byte count");
                ValidateCount(timeByteCount, "Native DNN profile time byte count");
                ValidateCount(invocationByteCount, "Native DNN profile invocation byte count");

                var nameOffsets = new int[checked(rowCount + 1)];
                var names = new byte[nameByteCount];
                var timeOffsets = new int[checked(rowCount + 1)];
                var times = new byte[timeByteCount];
                var invocationOffsets = new int[checked(rowCount + 1)];
                var invocations = new byte[invocationByteCount];
                NativeException.ThrowIfError(NativeMethods.DnnNetGetDetailedPerfProfileFill(
                    nativeHandle,
                    nameOffsets, nameOffsets.Length, names, names.Length,
                    timeOffsets, timeOffsets.Length, times, times.Length,
                    invocationOffsets, invocationOffsets.Length, invocations, invocations.Length,
                    out int writtenRows, out int writtenNameBytes, out int writtenTimeBytes, out int writtenInvocationBytes));
                if (writtenRows != rowCount || writtenNameBytes != nameByteCount || writtenTimeBytes != timeByteCount || writtenInvocationBytes != invocationByteCount)
                    throw new OpenCvException("Native DNN detailed profile changed during count/fill retrieval.");
                return new DnnDetailedPerfProfile(
                    DecodePackedStrings(nameOffsets, rowCount, names, nameByteCount),
                    DecodePackedStrings(timeOffsets, rowCount, times, timeByteCount),
                    DecodePackedStrings(invocationOffsets, rowCount, invocations, invocationByteCount));
            });
        }

        private static Mat[][] TakeMatGroups(NativeDnnMatGroupsHandle result)
        {
            IntPtr resultHandle = result.DangerousGetHandle();
            NativeException.ThrowIfError(NativeMethods.DnnMatGroupsGetCounts(resultHandle, out int groupCount, out int matCount));
            ValidateCount(groupCount, "Native DNN group count");
            ValidateCount(matCount, "Native DNN Mat count");
            var offsets = new int[checked(groupCount + 1)];
            NativeException.ThrowIfError(NativeMethods.DnnMatGroupsGetGroupOffsets(resultHandle, offsets, offsets.Length, out int writtenGroups));
            if (writtenGroups != groupCount) throw new OpenCvException("Native DNN group count changed during retrieval.");
            ValidateOffsets(offsets, matCount, "Native DNN group offsets");
            var handles = new IntPtr[matCount];
            NativeException.ThrowIfError(NativeMethods.DnnMatGroupsTakeMats(resultHandle, handles, handles.Length, out int writtenMats));
            if (writtenMats != matCount)
            {
                ReleaseMatHandles(handles);
                throw new OpenCvException("Native DNN Mat count changed during retrieval.");
            }

            Mat[] flat = ToMatArray(handles, matCount);
            var groups = new Mat[groupCount][];
            try
            {
                for (int i = 0; i < groups.Length; i++)
                {
                    int length = checked(offsets[i + 1] - offsets[i]);
                    groups[i] = new Mat[length];
                    Array.Copy(flat, offsets[i], groups[i], 0, length);
                }
                return groups;
            }
            catch
            {
                for (int i = 0; i < flat.Length; i++) flat[i]?.Dispose();
                throw;
            }
        }

        private static DnnLayerShapes GetLayerShapesCore(IntPtr nativeHandle, int[] inputOffsets, int[] inputValues, int[] inputTypes, int layerId)
        {
            NativeException.ThrowIfError(NativeMethods.DnnNetGetLayerShapesCount(nativeHandle, inputOffsets, inputTypes.Length, inputValues, inputValues.Length, inputTypes, inputTypes.Length, layerId, out int inputShapeCount, out int inputValueCount, out int outputShapeCount, out int outputValueCount));
            ValidateCount(inputShapeCount, "Native DNN input-layer shape count");
            ValidateCount(inputValueCount, "Native DNN input-layer shape value count");
            ValidateCount(outputShapeCount, "Native DNN output-layer shape count");
            ValidateCount(outputValueCount, "Native DNN output-layer shape value count");
            var inputLayerOffsets = new int[checked(inputShapeCount + 1)];
            var inputLayerValues = new int[inputValueCount];
            var outputLayerOffsets = new int[checked(outputShapeCount + 1)];
            var outputLayerValues = new int[outputValueCount];
            NativeException.ThrowIfError(NativeMethods.DnnNetGetLayerShapesFill(nativeHandle, inputOffsets, inputTypes.Length, inputValues, inputValues.Length, inputTypes, inputTypes.Length, layerId, inputLayerOffsets, inputLayerOffsets.Length, inputLayerValues, inputLayerValues.Length, outputLayerOffsets, outputLayerOffsets.Length, outputLayerValues, outputLayerValues.Length, out int writtenInputShapes, out int writtenInputValues, out int writtenOutputShapes, out int writtenOutputValues));
            if (writtenInputShapes != inputShapeCount || writtenInputValues != inputValueCount || writtenOutputShapes != outputShapeCount || writtenOutputValues != outputValueCount)
                throw new OpenCvException("Native DNN layer shapes changed during count/fill retrieval.");
            return new DnnLayerShapes(UnpackShapes(inputLayerOffsets, inputLayerValues), UnpackShapes(outputLayerOffsets, outputLayerValues));
        }

        private static void PackInputShapes(int[][] inputShapes, int[] inputTypes, out int[] offsets, out int[] values, out int[] types)
        {
            if (inputShapes == null) throw new ArgumentNullException(nameof(inputShapes));
            if (inputTypes == null) throw new ArgumentNullException(nameof(inputTypes));
            if (inputShapes.Length == 0) throw new ArgumentException("At least one input shape is required.", nameof(inputShapes));
            if (inputShapes.Length != inputTypes.Length) throw new ArgumentException("Input shape and type counts must match.", nameof(inputTypes));
            offsets = new int[checked(inputShapes.Length + 1)];
            int valueCount = 0;
            for (int i = 0; i < inputShapes.Length; i++)
            {
                ValidateShape(inputShapes[i], nameof(inputShapes));
                ValidateMatType(inputTypes[i], nameof(inputTypes));
                valueCount = checked(valueCount + inputShapes[i].Length);
                offsets[i + 1] = valueCount;
            }
            values = new int[valueCount];
            for (int i = 0; i < inputShapes.Length; i++) Array.Copy(inputShapes[i], 0, values, offsets[i], inputShapes[i].Length);
            types = new int[inputTypes.Length];
            Array.Copy(inputTypes, types, inputTypes.Length);
        }

        private static int[][] UnpackShapes(int[] offsets, int[] values)
        {
            ValidateOffsets(offsets, values.Length, "Native DNN shape offsets");
            var result = new int[offsets.Length - 1][];
            for (int i = 0; i < result.Length; i++)
            {
                int length = checked(offsets[i + 1] - offsets[i]);
                result[i] = new int[length];
                Array.Copy(values, offsets[i], result[i], 0, length);
            }
            return result;
        }

        private static DnnTracingMode ToTracingMode(int value)
        {
            if (value == (int)DnnTracingMode.None || value == (int)DnnTracingMode.All || value == (int)DnnTracingMode.Operation) return (DnnTracingMode)value;
            throw new OpenCvException("Native DNN tracing mode is invalid.");
        }

        private static DnnProfilingMode ToProfilingMode(int value)
        {
            if (value == (int)DnnProfilingMode.None || value == (int)DnnProfilingMode.Summary || value == (int)DnnProfilingMode.Detailed) return (DnnProfilingMode)value;
            throw new OpenCvException("Native DNN profiling mode is invalid.");
        }

        private static DnnModelFormat ToModelFormat(int value)
        {
            if (value >= (int)DnnModelFormat.Generic && value <= (int)DnnModelFormat.TensorFlowLite) return (DnnModelFormat)value;
            throw new OpenCvException("Native DNN model format is invalid.");
        }

        private static void ValidateTracingMode(DnnTracingMode value, string parameterName)
        {
            if (value != DnnTracingMode.None && value != DnnTracingMode.All && value != DnnTracingMode.Operation) throw new ArgumentOutOfRangeException(parameterName);
        }

        private static void ValidateProfilingMode(DnnProfilingMode value, string parameterName)
        {
            if (value != DnnProfilingMode.None && value != DnnProfilingMode.Summary && value != DnnProfilingMode.Detailed) throw new ArgumentOutOfRangeException(parameterName);
        }

        private static void ValidateLayerId(int value, string parameterName)
        {
            if (value < 0) throw new ArgumentOutOfRangeException(parameterName);
        }

        private static void ValidateParameterIndex(int value, string parameterName)
        {
            if (value < 0) throw new ArgumentOutOfRangeException(parameterName);
        }
    }
}
