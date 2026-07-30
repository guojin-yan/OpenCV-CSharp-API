using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.ML
{
    /// <summary>
    /// Represents OpenCV ML training data.
    /// 表示 OpenCV ML 训练数据。
    /// </summary>
    public sealed class TrainData : IDisposable
    {
        private const int PropertyLayout = 0;
        private const int PropertyNTrainSamples = 1;
        private const int PropertyNTestSamples = 2;
        private const int PropertyNSamples = 3;
        private const int PropertyNVars = 4;
        private const int PropertyNAllVars = 5;
        private const int PropertyResponseType = 6;
        private const int PropertyCatCount = 7;

        private const int MatSamples = 0;
        private const int MatMissing = 1;
        private const int MatTrainSamples = 2;
        private const int MatTrainResponses = 3;
        private const int MatTrainNormCatResponses = 4;
        private const int MatTestResponses = 5;
        private const int MatTestNormCatResponses = 6;
        private const int MatResponses = 7;
        private const int MatNormCatResponses = 8;
        private const int MatSampleWeights = 9;
        private const int MatTrainSampleWeights = 10;
        private const int MatTestSampleWeights = 11;
        private const int MatVarIdx = 12;
        private const int MatVarType = 13;
        private const int MatVarSymbolFlags = 14;
        private const int MatTrainSampleIdx = 15;
        private const int MatTestSampleIdx = 16;
        private const int MatDefaultSubstValues = 17;
        private const int MatClassLabels = 18;
        private const int MatCatOfs = 19;
        private const int MatCatMap = 20;
        private const int MatTestSamples = 21;

        private NativeMlTrainDataHandle handle;
        private bool disposed;

        internal TrainData(IntPtr nativeHandle)
        {
            handle = NativeMlTrainDataHandle.FromNativePointer(nativeHandle);
        }

        /// <summary>Gets whether this object has been disposed. 获取对象是否已经释放。</summary>
        public bool IsDisposed
        {
            get { return disposed; }
        }

        internal IntPtr NativeHandle
        {
            get
            {
                ThrowIfDisposed();
                return handle.DangerousGetHandle();
            }
        }

        /// <summary>Gets the original sample layout. 获取原始样本布局。</summary>
        public SampleTypes Layout
        {
            get { return (SampleTypes)GetInt(PropertyLayout, 0); }
        }

        /// <summary>Gets the number of training samples. 获取训练样本数量。</summary>
        public int NTrainSamples
        {
            get { return GetInt(PropertyNTrainSamples, 0); }
        }

        /// <summary>Gets the number of test samples. 获取测试样本数量。</summary>
        public int NTestSamples
        {
            get { return GetInt(PropertyNTestSamples, 0); }
        }

        /// <summary>Gets the total number of samples. 获取样本总数。</summary>
        public int NSamples
        {
            get { return GetInt(PropertyNSamples, 0); }
        }

        /// <summary>Gets the number of active variables. 获取活动变量数量。</summary>
        public int NVars
        {
            get { return GetInt(PropertyNVars, 0); }
        }

        /// <summary>Gets the number of all variables. 获取全部变量数量。</summary>
        public int NAllVars
        {
            get { return GetInt(PropertyNAllVars, 0); }
        }

        /// <summary>Gets the response variable type. 获取响应变量类型。</summary>
        public MlVariableType ResponseType
        {
            get { return (MlVariableType)GetInt(PropertyResponseType, 0); }
        }

        /// <summary>
        /// Creates training data from in-memory matrices.
        /// 从内存矩阵创建训练数据。
        /// </summary>
        public static TrainData Create(
            Mat samples,
            SampleTypes layout,
            Mat responses,
            Mat? varIdx = null,
            Mat? sampleIdx = null,
            Mat? sampleWeights = null,
            Mat? varType = null)
        {
            ValidateNotNull(samples, nameof(samples));
            ValidateNotNull(responses, nameof(responses));
            NativeException.ThrowIfError(NativeMethods.MlTrainDataCreate(
                samples.NativeHandle,
                (int)layout,
                responses.NativeHandle,
                OptionalHandle(varIdx),
                OptionalHandle(sampleIdx),
                OptionalHandle(sampleWeights),
                OptionalHandle(varType),
                out IntPtr nativeHandle));
            return new TrainData(nativeHandle);
        }

        /// <summary>
        /// Loads training data from a CSV file.
        /// 从 CSV 文件加载训练数据。
        /// </summary>
        public static TrainData LoadFromCsv(
            string filename,
            int headerLineCount,
            int responseStartIdx = -1,
            int responseEndIdx = -1,
            string? varTypeSpec = null,
            char delimiter = ',',
            char missch = '?')
        {
            byte[] nativeFilename = MLStringConvert.ToNullTerminatedUtf8(filename, nameof(filename));
            byte[] nativeVarTypeSpec = MLStringConvert.ToNullTerminatedUtf8(varTypeSpec, nameof(varTypeSpec), allowNull: true);
            NativeException.ThrowIfError(NativeMethods.MlTrainDataLoadCsv(
                nativeFilename,
                headerLineCount,
                responseStartIdx,
                responseEndIdx,
                nativeVarTypeSpec,
                delimiter,
                missch,
                out IntPtr nativeHandle));
            return new TrainData(nativeHandle);
        }

        /// <summary>Gets the sample matrix. 获取样本矩阵。</summary>
        public Mat GetSamples()
        {
            return GetMat(MatSamples);
        }

        /// <summary>Gets the missing-value mask. 获取缺失值掩码。</summary>
        public Mat GetMissing()
        {
            return GetMat(MatMissing);
        }

        /// <summary>Gets training samples. 获取训练样本。</summary>
        public Mat GetTrainSamples(SampleTypes layout = SampleTypes.RowSample, bool compressSamples = true, bool compressVars = true)
        {
            return GetMat(MatTrainSamples, (int)layout, compressSamples, compressVars);
        }

        /// <summary>Gets test samples. 获取测试样本。</summary>
        public Mat GetTestSamples()
        {
            return GetMat(MatTestSamples);
        }

        /// <summary>Gets training responses. 获取训练响应。</summary>
        public Mat GetTrainResponses()
        {
            return GetMat(MatTrainResponses);
        }

        /// <summary>Gets normalized categorical training responses. 获取归一化分类训练响应。</summary>
        public Mat GetTrainNormCatResponses()
        {
            return GetMat(MatTrainNormCatResponses);
        }

        /// <summary>Gets test responses. 获取测试响应。</summary>
        public Mat GetTestResponses()
        {
            return GetMat(MatTestResponses);
        }

        /// <summary>Gets normalized categorical test responses. 获取归一化分类测试响应。</summary>
        public Mat GetTestNormCatResponses()
        {
            return GetMat(MatTestNormCatResponses);
        }

        /// <summary>Gets all responses. 获取全部响应。</summary>
        public Mat GetResponses()
        {
            return GetMat(MatResponses);
        }

        /// <summary>Gets normalized categorical responses. 获取归一化分类响应。</summary>
        public Mat GetNormCatResponses()
        {
            return GetMat(MatNormCatResponses);
        }

        /// <summary>Gets sample weights. 获取样本权重。</summary>
        public Mat GetSampleWeights()
        {
            return GetMat(MatSampleWeights);
        }

        /// <summary>Gets training sample weights. 获取训练样本权重。</summary>
        public Mat GetTrainSampleWeights()
        {
            return GetMat(MatTrainSampleWeights);
        }

        /// <summary>Gets test sample weights. 获取测试样本权重。</summary>
        public Mat GetTestSampleWeights()
        {
            return GetMat(MatTestSampleWeights);
        }

        /// <summary>Gets variable indexes. 获取变量索引。</summary>
        public Mat GetVarIdx()
        {
            return GetMat(MatVarIdx);
        }

        /// <summary>Gets variable types. 获取变量类型。</summary>
        public Mat GetVarType()
        {
            return GetMat(MatVarType);
        }

        /// <summary>Gets variable symbol flags. 获取变量符号标志。</summary>
        public Mat GetVarSymbolFlags()
        {
            return GetMat(MatVarSymbolFlags);
        }

        /// <summary>Gets training sample indexes. 获取训练样本索引。</summary>
        public Mat GetTrainSampleIdx()
        {
            return GetMat(MatTrainSampleIdx);
        }

        /// <summary>Gets test sample indexes. 获取测试样本索引。</summary>
        public Mat GetTestSampleIdx()
        {
            return GetMat(MatTestSampleIdx);
        }

        /// <summary>Gets default substitution values. 获取默认替代值。</summary>
        public Mat GetDefaultSubstValues()
        {
            return GetMat(MatDefaultSubstValues);
        }

        /// <summary>Gets class labels. 获取类别标签。</summary>
        public Mat GetClassLabels()
        {
            return GetMat(MatClassLabels);
        }

        /// <summary>Gets category offsets. 获取类别偏移。</summary>
        public Mat GetCatOfs()
        {
            return GetMat(MatCatOfs);
        }

        /// <summary>Gets category map. 获取类别映射。</summary>
        public Mat GetCatMap()
        {
            return GetMat(MatCatMap);
        }

        /// <summary>Gets category count for a variable. 获取某个变量的类别数量。</summary>
        public int GetCatCount(int variableIndex)
        {
            return GetInt(PropertyCatCount, variableIndex);
        }

        /// <summary>Returns one sample, optionally restricted to selected variable indexes.</summary>
        public float[] GetSample(int sampleIndex, Mat? variableIndices = null)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.MlTrainDataGetSampleCount(NativeHandle, OptionalHandle(variableIndices), out int count));
            var result = new float[count];
            GetSample(sampleIndex, result, variableIndices);
            return result;
        }

        /// <summary>Copies one sample into an exactly sized caller-owned array.</summary>
        public unsafe void GetSample(int sampleIndex, float[] destination, Mat? variableIndices = null)
        {
            ThrowIfDisposed();
            ValidateNotNull(destination, nameof(destination));
            NativeException.ThrowIfError(NativeMethods.MlTrainDataGetSampleCount(NativeHandle, OptionalHandle(variableIndices), out int count));
            ValidateDestinationLength(destination.Length, count, nameof(destination));
            fixed (float* destinationPtr = destination)
            {
                NativeException.ThrowIfError(NativeMethods.MlTrainDataGetSampleFill(
                    NativeHandle,
                    OptionalHandle(variableIndices),
                    sampleIndex,
                    destinationPtr,
                    destination.Length,
                    out int written));
                ValidateWrittenCount(written, count);
            }
        }

        /// <summary>Returns one variable across all or selected sample indexes.</summary>
        public float[] GetValues(int variableIndex, Mat? sampleIndices = null)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.MlTrainDataGetValuesCount(NativeHandle, OptionalHandle(sampleIndices), out int count));
            var result = new float[count];
            GetValues(variableIndex, result, sampleIndices);
            return result;
        }

        /// <summary>Copies one variable into an exactly sized caller-owned array.</summary>
        public unsafe void GetValues(int variableIndex, float[] destination, Mat? sampleIndices = null)
        {
            ThrowIfDisposed();
            ValidateNotNull(destination, nameof(destination));
            NativeException.ThrowIfError(NativeMethods.MlTrainDataGetValuesCount(NativeHandle, OptionalHandle(sampleIndices), out int count));
            ValidateDestinationLength(destination.Length, count, nameof(destination));
            fixed (float* destinationPtr = destination)
            {
                NativeException.ThrowIfError(NativeMethods.MlTrainDataGetValuesFill(
                    NativeHandle,
                    variableIndex,
                    OptionalHandle(sampleIndices),
                    destinationPtr,
                    destination.Length,
                    out int written));
                ValidateWrittenCount(written, count);
            }
        }

        /// <summary>Sets the train/test split by count. 按数量设置训练/测试划分。</summary>
        public void SetTrainTestSplit(int count, bool shuffle = true)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.MlTrainDataSetTrainTestSplit(NativeHandle, count, shuffle ? 1 : 0));
        }

        /// <summary>Sets the train/test split by ratio. 按比例设置训练/测试划分。</summary>
        public void SetTrainTestSplitRatio(double ratio, bool shuffle = true)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.MlTrainDataSetTrainTestSplitRatio(NativeHandle, ratio, shuffle ? 1 : 0));
        }

        /// <summary>Shuffles the current train/test split. 随机打乱当前训练/测试划分。</summary>
        public void ShuffleTrainTest()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.MlTrainDataShuffleTrainTest(NativeHandle));
        }

        /// <summary>Gets symbolic variable names loaded from CSV. 获取从 CSV 加载的符号变量名。</summary>
        public unsafe string[] GetNames()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.MlTrainDataGetNamesCount(NativeHandle, out int stringCount, out int byteCount));
            if (stringCount <= 0)
            {
                return Array.Empty<string>();
            }

            var offsets = new int[stringCount + 1];
            var buffer = new byte[Math.Max(0, byteCount)];
            fixed (int* offsetsPtr = offsets)
            fixed (byte* bufferPtr = buffer)
            {
                NativeException.ThrowIfError(NativeMethods.MlTrainDataGetNamesFill(
                    NativeHandle,
                    offsetsPtr,
                    offsets.Length,
                    bufferPtr,
                    buffer.Length,
                    out int writtenStrings,
                    out int writtenBytes));

                int count = Math.Max(0, Math.Min(writtenStrings, stringCount));
                var result = new string[count];
                for (int i = 0; i < count; i++)
                {
                    int start = Math.Max(0, Math.Min(offsets[i], buffer.Length));
                    int end = Math.Max(start, Math.Min(offsets[i + 1], Math.Min(writtenBytes, buffer.Length)));
                    result[i] = MLStringConvert.FromUtf8Bytes(buffer, start, end - start);
                }

                return result;
            }
        }

        /// <summary>Extracts indexed elements from a vector. 从向量提取指定索引的元素。</summary>
        public static void GetSubVector(Mat vec, Mat idx, Mat dst)
        {
            ValidateNotNull(vec, nameof(vec));
            ValidateNotNull(idx, nameof(idx));
            ValidateNotNull(dst, nameof(dst));
            NativeException.ThrowIfError(NativeMethods.MlTrainDataGetSubVector(vec.NativeHandle, idx.NativeHandle, dst.NativeHandle));
        }

        /// <summary>Extracts indexed elements from a vector. 从向量提取指定索引的元素。</summary>
        public static Mat GetSubVector(Mat vec, Mat idx)
        {
            var dst = new Mat();
            try
            {
                GetSubVector(vec, idx, dst);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        /// <summary>Extracts indexed rows or columns from a matrix. 从矩阵提取指定行或列。</summary>
        public static void GetSubMatrix(Mat matrix, Mat idx, SampleTypes layout, Mat dst)
        {
            ValidateNotNull(matrix, nameof(matrix));
            ValidateNotNull(idx, nameof(idx));
            ValidateNotNull(dst, nameof(dst));
            NativeException.ThrowIfError(NativeMethods.MlTrainDataGetSubMatrix(matrix.NativeHandle, idx.NativeHandle, (int)layout, dst.NativeHandle));
        }

        /// <summary>Extracts indexed rows or columns from a matrix. 从矩阵提取指定行或列。</summary>
        public static Mat GetSubMatrix(Mat matrix, Mat idx, SampleTypes layout)
        {
            var dst = new Mat();
            try
            {
                GetSubMatrix(matrix, idx, layout, dst);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        /// <summary>Releases native resources. 释放 native 资源。</summary>
        public void Dispose()
        {
            if (!disposed)
            {
                handle.Dispose();
                disposed = true;
                GC.SuppressFinalize(this);
            }
        }

        internal static void ValidateNotNull<T>(T value, string parameterName)
            where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }

        private int GetInt(int propertyId, int argument)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.MlTrainDataGetInt(NativeHandle, propertyId, argument, out int value));
            return value;
        }

        private Mat GetMat(int propertyId, int layout = 0, bool compressSamples = true, bool compressVars = true)
        {
            ThrowIfDisposed();
            var dst = new Mat();
            try
            {
                NativeException.ThrowIfError(NativeMethods.MlTrainDataGetMat(NativeHandle, propertyId, layout, compressSamples ? 1 : 0, compressVars ? 1 : 0, dst.NativeHandle));
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        private static IntPtr OptionalHandle(Mat? mat)
        {
            return mat == null ? IntPtr.Zero : mat.NativeHandle;
        }

        private static void ValidateDestinationLength(int actual, int expected, string parameterName)
        {
            if (actual != expected)
            {
                throw new ArgumentException("The destination length must match the required element count.", parameterName);
            }
        }

        private static void ValidateWrittenCount(int actual, int expected)
        {
            if (actual != expected)
            {
                throw new OpenCvException("The native ML buffer element count changed during retrieval.");
            }
        }

        private void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(TrainData));
            }
        }
    }
}
