using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

#if NETCOREAPP3_1_OR_GREATER
using System.Buffers;
#endif

namespace OpenCvSharp.LineDescriptor
{
    /// <summary>
    /// Detects key lines and computes OpenCV line_descriptor binary descriptors.
    /// 检测关键线段并计算 OpenCV line_descriptor 二进制描述子。
    /// </summary>
    public sealed class BinaryDescriptor : IDisposable
    {
#if NETCOREAPP3_1_OR_GREATER
        private const int StackallocKeyLineThreshold = LineDescriptorKeyLineMarshaller.StackallocThreshold;
#endif

        private NativeLineDescriptorBinaryDescriptorHandle handle;
        private bool disposed;

        private BinaryDescriptor(IntPtr nativeHandle)
        {
            handle = NativeLineDescriptorBinaryDescriptorHandle.FromNativePointer(nativeHandle);
        }

        /// <summary>Gets whether this descriptor has been disposed. 获取描述子对象是否已经释放。</summary>
        public bool IsDisposed
        {
            get { return disposed; }
        }

        /// <summary>Gets or sets the number of octaves. 获取或设置 octave 数量。</summary>
        public int NumOfOctaves
        {
            get { return GetInt(NativeMethods.LineDescriptorBinaryDescriptorGetNumOfOctaves); }
            set { SetInt(NativeMethods.LineDescriptorBinaryDescriptorSetNumOfOctaves, value); }
        }

        /// <summary>Gets or sets the descriptor support band width. 获取或设置描述子支撑带宽。</summary>
        public int WidthOfBand
        {
            get { return GetInt(NativeMethods.LineDescriptorBinaryDescriptorGetWidthOfBand); }
            set { SetInt(NativeMethods.LineDescriptorBinaryDescriptorSetWidthOfBand, value); }
        }

        /// <summary>Gets or sets the image pyramid reduction ratio. 获取或设置图像金字塔降采样比例。</summary>
        public int ReductionRatio
        {
            get { return GetInt(NativeMethods.LineDescriptorBinaryDescriptorGetReductionRatio); }
            set { SetInt(NativeMethods.LineDescriptorBinaryDescriptorSetReductionRatio, value); }
        }

        /// <summary>Gets whether the native algorithm is empty. 获取 native 算法对象是否为空。</summary>
        public bool Empty
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.LineDescriptorBinaryDescriptorEmpty(NativeHandle, out int empty));
                return empty != 0;
            }
        }

        /// <summary>Gets the descriptor size in bytes. 获取描述子字节尺寸。</summary>
        public int DescriptorSize
        {
            get { return GetInt(NativeMethods.LineDescriptorBinaryDescriptorDescriptorSize); }
        }

        /// <summary>Gets the descriptor OpenCV matrix type. 获取描述子的 OpenCV 矩阵类型。</summary>
        public int DescriptorType
        {
            get { return GetInt(NativeMethods.LineDescriptorBinaryDescriptorDescriptorType); }
        }

        /// <summary>Gets the default norm type used for descriptor matching. 获取描述子匹配默认使用的范数类型。</summary>
        public NormTypes DefaultNorm
        {
            get { return (NormTypes)GetInt(NativeMethods.LineDescriptorBinaryDescriptorDefaultNorm); }
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
        /// Creates a binary descriptor with OpenCV default parameters.
        /// 使用 OpenCV 默认参数创建二进制描述子。
        /// </summary>
        public static BinaryDescriptor Create()
        {
            return Create(BinaryDescriptorParameters.Default);
        }

        /// <summary>
        /// Creates a binary descriptor.
        /// 创建二进制描述子。
        /// </summary>
        public static BinaryDescriptor Create(BinaryDescriptorParameters parameters)
        {
            parameters.Validate();
            NativeException.ThrowIfError(NativeMethods.LineDescriptorBinaryDescriptorCreate(
                parameters.NumOfOctaves,
                parameters.WidthOfBand,
                parameters.ReductionRatio,
                parameters.KSize,
                out IntPtr nativeHandle));
            return new BinaryDescriptor(nativeHandle);
        }

        /// <summary>
        /// Creates a binary descriptor.
        /// 创建二进制描述子。
        /// </summary>
        public static BinaryDescriptor Create(int numOfOctaves = 1, int widthOfBand = 7, int reductionRatio = 2, int ksize = 5)
        {
            return Create(new BinaryDescriptorParameters(numOfOctaves, widthOfBand, reductionRatio, ksize));
        }

        /// <summary>Clears native algorithm state. 清除 native 算法状态。</summary>
        public void Clear()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.LineDescriptorBinaryDescriptorClear(NativeHandle));
        }

        /// <summary>
        /// Detects key lines in an image.
        /// 检测图像中的关键线段。
        /// </summary>
        public KeyLine[] Detect(Mat image, Mat? mask = null)
        {
            ThrowIfDisposed();
            ValidateNotNull(image, nameof(image));
            ValidateDetectMask(image, mask, nameof(mask));
            NativeException.ThrowIfError(NativeMethods.LineDescriptorBinaryDescriptorDetectCount(
                NativeHandle,
                image.NativeHandle,
                OptionalHandle(mask),
                out int keylineCount));
            if (keylineCount <= 0)
            {
                return Array.Empty<KeyLine>();
            }

            var nativeKeylines = new NativeLineDescriptorKeyLine[keylineCount];
            unsafe
            {
                fixed (NativeLineDescriptorKeyLine* keylinesPtr = nativeKeylines)
                {
                    NativeException.ThrowIfError(NativeMethods.LineDescriptorBinaryDescriptorDetectFill(
                        NativeHandle,
                        image.NativeHandle,
                        OptionalHandle(mask),
                        keylinesPtr,
                        nativeKeylines.Length,
                        out int writtenCount));
                    return LineDescriptorKeyLineMarshaller.FromNative(nativeKeylines, writtenCount);
                }
            }
        }

        /// <summary>
        /// Computes descriptors for caller-provided key lines.
        /// 为调用方提供的关键线段计算描述子。
        /// </summary>
        public KeyLine[] Compute(Mat image, KeyLine[] keylines, Mat descriptors, bool returnFloatDescriptor = false)
        {
            ValidateNotNull(keylines, nameof(keylines));
            return ComputeCore(image, keylines, descriptors, returnFloatDescriptor);
        }

        /// <summary>
        /// Computes descriptors and replaces the keyline array with the key lines kept by OpenCV.
        /// 计算描述子，并用 OpenCV 保留的关键线段替换数组。
        /// </summary>
        public void Compute(Mat image, ref KeyLine[] keylines, Mat descriptors, bool returnFloatDescriptor = false)
        {
            ValidateNotNull(keylines, nameof(keylines));
            keylines = ComputeCore(image, keylines, descriptors, returnFloatDescriptor);
        }

        /// <summary>
        /// Computes descriptors and returns the descriptor matrix.
        /// 计算描述子并返回描述子矩阵。
        /// </summary>
        public Mat Compute(Mat image, ref KeyLine[] keylines, bool returnFloatDescriptor = false)
        {
            var descriptors = new Mat();
            try
            {
                Compute(image, ref keylines, descriptors, returnFloatDescriptor);
                return descriptors;
            }
            catch
            {
                descriptors.Dispose();
                throw;
            }
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Computes descriptors from a span-backed keyline sequence.
        /// 从 Span 支持的关键线段序列计算描述子。
        /// </summary>
        public KeyLine[] Compute(Mat image, ReadOnlySpan<KeyLine> keylines, Mat descriptors, bool returnFloatDescriptor = false)
        {
            return ComputeCore(image, keylines, descriptors, returnFloatDescriptor);
        }
#endif

        /// <summary>
        /// Detects key lines and computes descriptors.
        /// 检测关键线段并计算描述子。
        /// </summary>
        public void DetectAndCompute(Mat image, Mat? mask, out KeyLine[] keylines, Mat descriptors, bool returnFloatDescriptor = false)
        {
            keylines = DetectAndComputeCore(image, mask, Array.Empty<KeyLine>(), descriptors, useProvidedKeylines: false, returnFloatDescriptor: returnFloatDescriptor);
        }

        /// <summary>
        /// Detects key lines and computes descriptors into a returned matrix.
        /// 检测关键线段并将描述子计算到返回的矩阵中。
        /// </summary>
        public Mat DetectAndCompute(Mat image, Mat? mask, out KeyLine[] keylines, bool returnFloatDescriptor = false)
        {
            var descriptors = new Mat();
            try
            {
                DetectAndCompute(image, mask, out keylines, descriptors, returnFloatDescriptor);
                return descriptors;
            }
            catch
            {
                descriptors.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Detects or reuses key lines and computes descriptors.
        /// 检测或复用关键线段并计算描述子。
        /// </summary>
        public KeyLine[] DetectAndCompute(Mat image, Mat? mask, KeyLine[] keylines, Mat descriptors, bool useProvidedKeylines = true, bool returnFloatDescriptor = false)
        {
            ValidateNotNull(keylines, nameof(keylines));
            return DetectAndComputeCore(image, mask, keylines, descriptors, useProvidedKeylines, returnFloatDescriptor);
        }

        /// <summary>
        /// Detects or reuses key lines, computes descriptors, and replaces the keyline array.
        /// 检测或复用关键线段、计算描述子，并替换关键线段数组。
        /// </summary>
        public void DetectAndComputeInPlace(Mat image, Mat? mask, ref KeyLine[] keylines, Mat descriptors, bool useProvidedKeylines, bool returnFloatDescriptor = false)
        {
            ValidateNotNull(keylines, nameof(keylines));
            keylines = DetectAndComputeCore(image, mask, keylines, descriptors, useProvidedKeylines, returnFloatDescriptor);
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Reuses span-backed key lines and computes descriptors.
        /// 复用 Span 支持的关键线段并计算描述子。
        /// </summary>
        public KeyLine[] DetectAndCompute(Mat image, Mat? mask, ReadOnlySpan<KeyLine> keylines, Mat descriptors, bool returnFloatDescriptor = false)
        {
            return DetectAndComputeCore(image, mask, keylines, descriptors, useProvidedKeylines: true, returnFloatDescriptor: returnFloatDescriptor);
        }
#endif

        /// <summary>Releases native resources. 释放 native 资源。</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return disposed ? "{Disposed=True}" : "{NumOfOctaves=" + NumOfOctaves + ",WidthOfBand=" + WidthOfBand + ",ReductionRatio=" + ReductionRatio + "}";
        }

        private KeyLine[] ComputeCore(Mat image, KeyLine[] keylines, Mat descriptors, bool returnFloatDescriptor)
        {
            ValidateNotNull(image, nameof(image));
            ValidateNotNull(descriptors, nameof(descriptors));
            ThrowIfDisposed();

            NativeLineDescriptorKeyLine[] nativeInput = LineDescriptorKeyLineMarshaller.ToNative(keylines);
            var nativeOutput = new NativeLineDescriptorKeyLine[Math.Max(nativeInput.Length, 1)];
            unsafe
            {
                fixed (NativeLineDescriptorKeyLine* inputPtr = nativeInput)
                fixed (NativeLineDescriptorKeyLine* outputPtr = nativeOutput)
                {
                    NativeException.ThrowIfError(NativeMethods.LineDescriptorBinaryDescriptorCompute(
                        NativeHandle,
                        image.NativeHandle,
                        inputPtr,
                        nativeInput.Length,
                        outputPtr,
                        nativeOutput.Length,
                        out int writtenCount,
                        descriptors.NativeHandle,
                        returnFloatDescriptor ? 1 : 0));
                    return LineDescriptorKeyLineMarshaller.FromNative(nativeOutput, writtenCount);
                }
            }
        }

#if NETCOREAPP3_1_OR_GREATER
        private unsafe KeyLine[] ComputeCore(Mat image, ReadOnlySpan<KeyLine> keylines, Mat descriptors, bool returnFloatDescriptor)
        {
            ValidateNotNull(image, nameof(image));
            ValidateNotNull(descriptors, nameof(descriptors));
            ThrowIfDisposed();

            NativeLineDescriptorKeyLine[]? rentedInput = null;
            NativeLineDescriptorKeyLine[]? rentedOutput = null;
            Span<NativeLineDescriptorKeyLine> nativeInput = keylines.Length <= StackallocKeyLineThreshold
                ? stackalloc NativeLineDescriptorKeyLine[keylines.Length]
                : (rentedInput = ArrayPool<NativeLineDescriptorKeyLine>.Shared.Rent(keylines.Length)).AsSpan(0, keylines.Length);
            Span<NativeLineDescriptorKeyLine> nativeOutput = keylines.Length <= StackallocKeyLineThreshold
                ? stackalloc NativeLineDescriptorKeyLine[Math.Max(keylines.Length, 1)]
                : (rentedOutput = ArrayPool<NativeLineDescriptorKeyLine>.Shared.Rent(Math.Max(keylines.Length, 1))).AsSpan(0, Math.Max(keylines.Length, 1));

            try
            {
                LineDescriptorKeyLineMarshaller.CopyToNative(keylines, nativeInput);
                fixed (NativeLineDescriptorKeyLine* inputPtr = nativeInput)
                fixed (NativeLineDescriptorKeyLine* outputPtr = nativeOutput)
                {
                    NativeException.ThrowIfError(NativeMethods.LineDescriptorBinaryDescriptorCompute(
                        NativeHandle,
                        image.NativeHandle,
                        inputPtr,
                        keylines.Length,
                        outputPtr,
                        nativeOutput.Length,
                        out int writtenCount,
                        descriptors.NativeHandle,
                        returnFloatDescriptor ? 1 : 0));
                    return LineDescriptorKeyLineMarshaller.FromNative(nativeOutput, writtenCount);
                }
            }
            finally
            {
                if (rentedInput != null)
                {
                    ArrayPool<NativeLineDescriptorKeyLine>.Shared.Return(rentedInput);
                }

                if (rentedOutput != null)
                {
                    ArrayPool<NativeLineDescriptorKeyLine>.Shared.Return(rentedOutput);
                }
            }
        }
#endif

        private KeyLine[] DetectAndComputeCore(Mat image, Mat? mask, KeyLine[] keylines, Mat descriptors, bool useProvidedKeylines, bool returnFloatDescriptor)
        {
            ValidateNotNull(image, nameof(image));
            ValidateNotNull(descriptors, nameof(descriptors));
            ThrowIfDisposed();
            ValidateDetectMask(image, mask, nameof(mask));

            NativeLineDescriptorKeyLine[] nativeInput = LineDescriptorKeyLineMarshaller.ToNative(keylines);
            unsafe
            {
                fixed (NativeLineDescriptorKeyLine* inputPtr = nativeInput)
                {
                    NativeException.ThrowIfError(NativeMethods.LineDescriptorBinaryDescriptorDetectAndComputeCount(
                        NativeHandle,
                        image.NativeHandle,
                        OptionalHandle(mask),
                        inputPtr,
                        nativeInput.Length,
                        useProvidedKeylines ? 1 : 0,
                        returnFloatDescriptor ? 1 : 0,
                        out int outputCount));

                    var nativeOutput = new NativeLineDescriptorKeyLine[Math.Max(outputCount, 1)];
                    fixed (NativeLineDescriptorKeyLine* outputPtr = nativeOutput)
                    {
                        NativeException.ThrowIfError(NativeMethods.LineDescriptorBinaryDescriptorDetectAndComputeFill(
                            NativeHandle,
                            image.NativeHandle,
                            OptionalHandle(mask),
                            inputPtr,
                            nativeInput.Length,
                            useProvidedKeylines ? 1 : 0,
                            returnFloatDescriptor ? 1 : 0,
                            outputPtr,
                            nativeOutput.Length,
                            out int writtenCount,
                            descriptors.NativeHandle));
                        return LineDescriptorKeyLineMarshaller.FromNative(nativeOutput, writtenCount);
                    }
                }
            }
        }

#if NETCOREAPP3_1_OR_GREATER
        private unsafe KeyLine[] DetectAndComputeCore(Mat image, Mat? mask, ReadOnlySpan<KeyLine> keylines, Mat descriptors, bool useProvidedKeylines, bool returnFloatDescriptor)
        {
            ValidateNotNull(image, nameof(image));
            ValidateNotNull(descriptors, nameof(descriptors));
            ThrowIfDisposed();
            ValidateDetectMask(image, mask, nameof(mask));

            NativeLineDescriptorKeyLine[]? rentedInput = null;
            Span<NativeLineDescriptorKeyLine> nativeInput = keylines.Length <= StackallocKeyLineThreshold
                ? stackalloc NativeLineDescriptorKeyLine[keylines.Length]
                : (rentedInput = ArrayPool<NativeLineDescriptorKeyLine>.Shared.Rent(keylines.Length)).AsSpan(0, keylines.Length);

            try
            {
                LineDescriptorKeyLineMarshaller.CopyToNative(keylines, nativeInput);
                fixed (NativeLineDescriptorKeyLine* inputPtr = nativeInput)
                {
                    NativeException.ThrowIfError(NativeMethods.LineDescriptorBinaryDescriptorDetectAndComputeCount(
                        NativeHandle,
                        image.NativeHandle,
                        OptionalHandle(mask),
                        inputPtr,
                        keylines.Length,
                        useProvidedKeylines ? 1 : 0,
                        returnFloatDescriptor ? 1 : 0,
                        out int outputCount));

                    var nativeOutput = new NativeLineDescriptorKeyLine[Math.Max(outputCount, 1)];
                    fixed (NativeLineDescriptorKeyLine* outputPtr = nativeOutput)
                    {
                        NativeException.ThrowIfError(NativeMethods.LineDescriptorBinaryDescriptorDetectAndComputeFill(
                            NativeHandle,
                            image.NativeHandle,
                            OptionalHandle(mask),
                            inputPtr,
                            keylines.Length,
                            useProvidedKeylines ? 1 : 0,
                            returnFloatDescriptor ? 1 : 0,
                            outputPtr,
                            nativeOutput.Length,
                            out int writtenCount,
                            descriptors.NativeHandle));
                        return LineDescriptorKeyLineMarshaller.FromNative(nativeOutput, writtenCount);
                    }
                }
            }
            finally
            {
                if (rentedInput != null)
                {
                    ArrayPool<NativeLineDescriptorKeyLine>.Shared.Return(rentedInput);
                }
            }
        }
#endif

        private delegate int IntGetter(IntPtr handle, out int value);

        private delegate int IntSetter(IntPtr handle, int value);

        private int GetInt(IntGetter getter)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(getter(NativeHandle, out int value));
            return value;
        }

        private void SetInt(IntSetter setter, int value)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(setter(NativeHandle, value));
        }

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing && handle != null)
                {
                    handle.Dispose();
                }

                disposed = true;
            }
        }

        private static IntPtr OptionalHandle(Mat? mat)
        {
            return mat == null ? IntPtr.Zero : mat.NativeHandle;
        }

        private static void ValidateDetectMask(Mat image, Mat? mask, string parameterName)
        {
            if (mask == null || mask.Empty)
            {
                return;
            }

            if (mask.Rows != image.Rows || mask.Cols != image.Cols)
            {
                throw new ArgumentException("Detection mask must have the same size as the image.", parameterName);
            }

            if (mask.Type != MatType.CV_8UC1)
            {
                throw new ArgumentException("Detection mask must be CV_8UC1.", parameterName);
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

        private void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }
    }
}
