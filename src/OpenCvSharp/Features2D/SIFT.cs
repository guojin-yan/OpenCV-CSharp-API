using System;
using System.Globalization;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

#if NETCOREAPP3_1_OR_GREATER
using System.Buffers;
#endif

namespace JYPPX.OpenCvSharp.Features2D
{
    /// <summary>
    /// Provides SIFT keypoint detection and descriptor extraction compatible with <c>cv::SIFT</c>.
    /// 提供与 OpenCV <c>cv::SIFT</c> 兼容的 SIFT 关键点检测和描述子提取能力。
    /// </summary>
    public sealed class SIFT : Feature2D
    {
#if NETCOREAPP3_1_OR_GREATER
        private const int StackallocKeyPointThreshold = 64;
#endif

        private NativeSiftHandle handle;
        private bool disposed;

        private SIFT(IntPtr nativeHandle)
        {
            handle = NativeSiftHandle.FromNativePointer(nativeHandle);
        }

        /// <inheritdoc/>
        public override bool IsDisposed
        {
            get { return disposed; }
        }

        /// <summary>
        /// Gets or sets the number of best features retained by SIFT.
        /// 获取或设置 SIFT 保留的最佳特征数量。
        /// </summary>
        public int NFeatures
        {
            get { return GetInt(NativeMethods.Features2DSiftGetNFeatures); }
            set { SetInt(NativeMethods.Features2DSiftSetNFeatures, value); }
        }

        /// <summary>
        /// Gets or sets the number of layers in each octave.
        /// 获取或设置每个 octave 中的层数。
        /// </summary>
        public int NOctaveLayers
        {
            get { return GetInt(NativeMethods.Features2DSiftGetNOctaveLayers); }
            set { SetInt(NativeMethods.Features2DSiftSetNOctaveLayers, value); }
        }

        /// <summary>
        /// Gets or sets the contrast threshold used to filter weak features.
        /// 获取或设置用于过滤弱特征的对比度阈值。
        /// </summary>
        public double ContrastThreshold
        {
            get { return GetDouble(NativeMethods.Features2DSiftGetContrastThreshold); }
            set { SetDouble(NativeMethods.Features2DSiftSetContrastThreshold, value); }
        }

        /// <summary>
        /// Gets or sets the edge threshold used to filter edge-like features.
        /// 获取或设置用于过滤边缘类特征的边缘阈值。
        /// </summary>
        public double EdgeThreshold
        {
            get { return GetDouble(NativeMethods.Features2DSiftGetEdgeThreshold); }
            set { SetDouble(NativeMethods.Features2DSiftSetEdgeThreshold, value); }
        }

        /// <summary>
        /// Gets or sets the Gaussian sigma applied to octave zero.
        /// 获取或设置应用于第 0 个 octave 的高斯 sigma。
        /// </summary>
        public double Sigma
        {
            get { return GetDouble(NativeMethods.Features2DSiftGetSigma); }
            set { SetDouble(NativeMethods.Features2DSiftSetSigma, value); }
        }

        /// <inheritdoc/>
        public override bool Empty
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.Features2DSiftEmpty(NativeHandle, out int empty));
                return empty != 0;
            }
        }

        /// <inheritdoc/>
        public override int DescriptorSize
        {
            get { return GetInt(NativeMethods.Features2DSiftDescriptorSize); }
        }

        /// <inheritdoc/>
        public override int DescriptorType
        {
            get { return GetInt(NativeMethods.Features2DSiftDescriptorType); }
        }

        /// <inheritdoc/>
        public override NormTypes DefaultNorm
        {
            get { return (NormTypes)GetInt(NativeMethods.Features2DSiftDefaultNorm); }
        }

        /// <inheritdoc/>
        public override string DefaultName
        {
            get
            {
                ThrowIfDisposed();
                unsafe
                {
                    return NativeStringMarshaller.GetString(NativeHandle, NativeMethods.Features2DSiftDefaultNameLength, NativeMethods.Features2DSiftDefaultNameFill);
                }
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
        /// Creates a SIFT detector and descriptor extractor.
        /// 创建 SIFT 检测器和描述子提取器。
        /// </summary>
        /// <param name="nFeatures">The number of best features to retain. 要保留的最佳特征数量。</param>
        /// <param name="nOctaveLayers">The number of layers in each octave. 每个 octave 中的层数。</param>
        /// <param name="contrastThreshold">The contrast threshold. 对比度阈值。</param>
        /// <param name="edgeThreshold">The edge threshold. 边缘阈值。</param>
        /// <param name="sigma">The Gaussian sigma for octave zero. 第 0 个 octave 的高斯 sigma。</param>
        /// <param name="descriptorType">The descriptor type, usually <see cref="MatType.CV_32F"/> or <see cref="MatType.CV_8U"/>. 描述子类型，通常为 <see cref="MatType.CV_32F"/> 或 <see cref="MatType.CV_8U"/>。</param>
        /// <param name="enablePreciseUpscale">Whether to enable precise scale pyramid upscaling. 是否启用精确尺度金字塔上采样。</param>
        /// <returns>The created SIFT object. 创建的 SIFT 对象。</returns>
        public static SIFT Create(
            int nFeatures = 0,
            int nOctaveLayers = 3,
            double contrastThreshold = 0.04,
            double edgeThreshold = 10.0,
            double sigma = 1.6,
            int descriptorType = MatType.CV_32F,
            bool enablePreciseUpscale = false)
        {
            NativeException.ThrowIfError(NativeMethods.Features2DSiftCreate(
                nFeatures,
                nOctaveLayers,
                contrastThreshold,
                edgeThreshold,
                sigma,
                descriptorType,
                enablePreciseUpscale ? 1 : 0,
                out IntPtr nativeHandle));
            return new SIFT(nativeHandle);
        }

        /// <inheritdoc/>
        public override void Clear()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.Features2DSiftClear(NativeHandle));
        }

        /// <summary>
        /// Detects keypoints in an image.
        /// 检测图像中的关键点。
        /// </summary>
        public override KeyPoint[] Detect(Mat image, Mat? mask = null)
        {
            ThrowIfDisposed();
            ValidateNotNull(image, nameof(image));
            NativeException.ThrowIfError(NativeMethods.Features2DSiftDetectCount(NativeHandle, image.NativeHandle, OptionalHandle(mask), out int keypointCount));
            if (keypointCount <= 0)
            {
                return Array.Empty<KeyPoint>();
            }

            var native = new NativeKeyPoint[keypointCount];
            unsafe
            {
                fixed (NativeKeyPoint* nativePtr = native)
                {
                    NativeException.ThrowIfError(NativeMethods.Features2DSiftDetectFill(NativeHandle, image.NativeHandle, OptionalHandle(mask), nativePtr, native.Length, out int writtenCount));
                    return KeyPointMarshaller.FromNative(native, writtenCount);
                }
            }
        }

        /// <summary>
        /// Computes descriptors for caller-provided keypoints and returns the keypoints kept by OpenCV.
        /// 为调用方提供的关键点计算描述子，并返回 OpenCV 保留的关键点。
        /// </summary>
        public KeyPoint[] Compute(Mat image, KeyPoint[] keypoints, Mat descriptors)
        {
            ValidateNotNull(keypoints, nameof(keypoints));
            return ComputeCore(image, keypoints, descriptors);
        }

        /// <summary>
        /// Computes descriptors and replaces the keypoint array with the keypoints kept by OpenCV.
        /// 计算描述子，并用 OpenCV 保留的关键点替换关键点数组。
        /// </summary>
        public void Compute(Mat image, ref KeyPoint[] keypoints, Mat descriptors)
        {
            ValidateNotNull(keypoints, nameof(keypoints));
            keypoints = ComputeCore(image, keypoints, descriptors);
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Computes descriptors from a span-backed keypoint sequence.
        /// 从 Span 支持的关键点序列计算描述子。
        /// </summary>
        public KeyPoint[] Compute(Mat image, ReadOnlySpan<KeyPoint> keypoints, Mat descriptors)
        {
            return ComputeCore(image, keypoints, descriptors);
        }
#endif

        /// <summary>
        /// Detects keypoints and computes descriptors.
        /// 检测关键点并计算描述子。
        /// </summary>
        public void DetectAndCompute(Mat image, Mat? mask, out KeyPoint[] keypoints, Mat descriptors, bool useProvidedKeypoints = false)
        {
            keypoints = DetectAndComputeCore(image, mask, Array.Empty<KeyPoint>(), descriptors, useProvidedKeypoints);
        }

        /// <summary>
        /// Detects or reuses keypoints and computes descriptors.
        /// 检测或复用关键点并计算描述子。
        /// </summary>
        public KeyPoint[] DetectAndCompute(Mat image, Mat? mask, KeyPoint[] keypoints, Mat descriptors, bool useProvidedKeypoints = true)
        {
            ValidateNotNull(keypoints, nameof(keypoints));
            return DetectAndComputeCore(image, mask, keypoints, descriptors, useProvidedKeypoints);
        }

        /// <summary>
        /// Detects or reuses keypoints, computes descriptors, and replaces the keypoint array with the OpenCV result.
        /// 检测或复用关键点、计算描述子，并用 OpenCV 返回结果替换关键点数组。
        /// </summary>
        public void DetectAndComputeInPlace(Mat image, Mat? mask, ref KeyPoint[] keypoints, Mat descriptors, bool useProvidedKeypoints)
        {
            ValidateNotNull(keypoints, nameof(keypoints));
            keypoints = DetectAndComputeCore(image, mask, keypoints, descriptors, useProvidedKeypoints);
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Computes descriptors using span-backed provided keypoints.
        /// 使用 Span 支持的已有关键点计算描述子。
        /// </summary>
        public KeyPoint[] DetectAndCompute(Mat image, Mat? mask, ReadOnlySpan<KeyPoint> keypoints, Mat descriptors)
        {
            return DetectAndComputeCore(image, mask, keypoints, descriptors, useProvidedKeypoints: true);
        }
#endif

        /// <inheritdoc/>
        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return disposed
                ? "{Disposed=True}"
                : "{NFeatures=" + NFeatures
                    + ",NOctaveLayers=" + NOctaveLayers
                    + ",ContrastThreshold=" + ContrastThreshold.ToString(CultureInfo.InvariantCulture)
                    + ",EdgeThreshold=" + EdgeThreshold.ToString(CultureInfo.InvariantCulture)
                    + ",Sigma=" + Sigma.ToString(CultureInfo.InvariantCulture)
                    + ",DescriptorType=" + DescriptorType + "}";
        }

        private KeyPoint[] ComputeCore(Mat image, KeyPoint[] keypoints, Mat descriptors)
        {
            ValidateNotNull(image, nameof(image));
            ValidateNotNull(descriptors, nameof(descriptors));
            ThrowIfDisposed();

            NativeKeyPoint[] nativeInput = KeyPointMarshaller.ToNative(keypoints);
            var nativeOutput = new NativeKeyPoint[Math.Max(nativeInput.Length, 1)];
            unsafe
            {
                fixed (NativeKeyPoint* inputPtr = nativeInput)
                fixed (NativeKeyPoint* outputPtr = nativeOutput)
                {
                    NativeException.ThrowIfError(NativeMethods.Features2DSiftCompute(
                        NativeHandle,
                        image.NativeHandle,
                        inputPtr,
                        nativeInput.Length,
                        outputPtr,
                        nativeOutput.Length,
                        out int writtenCount,
                        descriptors.NativeHandle));
                    return KeyPointMarshaller.FromNative(nativeOutput, writtenCount);
                }
            }
        }

#if NETCOREAPP3_1_OR_GREATER
        private unsafe KeyPoint[] ComputeCore(Mat image, ReadOnlySpan<KeyPoint> keypoints, Mat descriptors)
        {
            ValidateNotNull(image, nameof(image));
            ValidateNotNull(descriptors, nameof(descriptors));
            ThrowIfDisposed();

            NativeKeyPoint[]? rentedInput = null;
            NativeKeyPoint[]? rentedOutput = null;
            Span<NativeKeyPoint> nativeInput = keypoints.Length <= StackallocKeyPointThreshold
                ? stackalloc NativeKeyPoint[keypoints.Length]
                : (rentedInput = ArrayPool<NativeKeyPoint>.Shared.Rent(keypoints.Length)).AsSpan(0, keypoints.Length);
            Span<NativeKeyPoint> nativeOutput = keypoints.Length <= StackallocKeyPointThreshold
                ? stackalloc NativeKeyPoint[Math.Max(keypoints.Length, 1)]
                : (rentedOutput = ArrayPool<NativeKeyPoint>.Shared.Rent(Math.Max(keypoints.Length, 1))).AsSpan(0, Math.Max(keypoints.Length, 1));

            try
            {
                KeyPointMarshaller.CopyToNative(keypoints, nativeInput);
                fixed (NativeKeyPoint* inputPtr = nativeInput)
                fixed (NativeKeyPoint* outputPtr = nativeOutput)
                {
                    NativeException.ThrowIfError(NativeMethods.Features2DSiftCompute(
                        NativeHandle,
                        image.NativeHandle,
                        inputPtr,
                        keypoints.Length,
                        outputPtr,
                        nativeOutput.Length,
                        out int writtenCount,
                        descriptors.NativeHandle));
                    return KeyPointMarshaller.FromNative(nativeOutput, writtenCount);
                }
            }
            finally
            {
                if (rentedInput != null)
                {
                    ArrayPool<NativeKeyPoint>.Shared.Return(rentedInput);
                }

                if (rentedOutput != null)
                {
                    ArrayPool<NativeKeyPoint>.Shared.Return(rentedOutput);
                }
            }
        }
#endif

        private KeyPoint[] DetectAndComputeCore(Mat image, Mat? mask, KeyPoint[] keypoints, Mat descriptors, bool useProvidedKeypoints)
        {
            ValidateNotNull(image, nameof(image));
            ValidateNotNull(descriptors, nameof(descriptors));
            ThrowIfDisposed();

            NativeKeyPoint[] nativeInput = KeyPointMarshaller.ToNative(keypoints);
            unsafe
            {
                fixed (NativeKeyPoint* inputPtr = nativeInput)
                {
                    NativeException.ThrowIfError(NativeMethods.Features2DSiftDetectAndComputeCount(
                        NativeHandle,
                        image.NativeHandle,
                        OptionalHandle(mask),
                        inputPtr,
                        nativeInput.Length,
                        useProvidedKeypoints ? 1 : 0,
                        out int outputCount));

                    var nativeOutput = new NativeKeyPoint[Math.Max(outputCount, 1)];
                    fixed (NativeKeyPoint* outputPtr = nativeOutput)
                    {
                        NativeException.ThrowIfError(NativeMethods.Features2DSiftDetectAndComputeFill(
                            NativeHandle,
                            image.NativeHandle,
                            OptionalHandle(mask),
                            inputPtr,
                            nativeInput.Length,
                            useProvidedKeypoints ? 1 : 0,
                            outputPtr,
                            nativeOutput.Length,
                            out int writtenCount,
                            descriptors.NativeHandle));
                        return KeyPointMarshaller.FromNative(nativeOutput, writtenCount);
                    }
                }
            }
        }

#if NETCOREAPP3_1_OR_GREATER
        private unsafe KeyPoint[] DetectAndComputeCore(Mat image, Mat? mask, ReadOnlySpan<KeyPoint> keypoints, Mat descriptors, bool useProvidedKeypoints)
        {
            ValidateNotNull(image, nameof(image));
            ValidateNotNull(descriptors, nameof(descriptors));
            ThrowIfDisposed();

            NativeKeyPoint[]? rentedInput = null;
            Span<NativeKeyPoint> nativeInput = keypoints.Length <= StackallocKeyPointThreshold
                ? stackalloc NativeKeyPoint[keypoints.Length]
                : (rentedInput = ArrayPool<NativeKeyPoint>.Shared.Rent(keypoints.Length)).AsSpan(0, keypoints.Length);

            try
            {
                KeyPointMarshaller.CopyToNative(keypoints, nativeInput);
                fixed (NativeKeyPoint* inputPtr = nativeInput)
                {
                    NativeException.ThrowIfError(NativeMethods.Features2DSiftDetectAndComputeCount(
                        NativeHandle,
                        image.NativeHandle,
                        OptionalHandle(mask),
                        inputPtr,
                        keypoints.Length,
                        useProvidedKeypoints ? 1 : 0,
                        out int outputCount));

                    var nativeOutput = new NativeKeyPoint[Math.Max(outputCount, 1)];
                    fixed (NativeKeyPoint* outputPtr = nativeOutput)
                    {
                        NativeException.ThrowIfError(NativeMethods.Features2DSiftDetectAndComputeFill(
                            NativeHandle,
                            image.NativeHandle,
                            OptionalHandle(mask),
                            inputPtr,
                            keypoints.Length,
                            useProvidedKeypoints ? 1 : 0,
                            outputPtr,
                            nativeOutput.Length,
                            out int writtenCount,
                            descriptors.NativeHandle));
                        return KeyPointMarshaller.FromNative(nativeOutput, writtenCount);
                    }
                }
            }
            finally
            {
                if (rentedInput != null)
                {
                    ArrayPool<NativeKeyPoint>.Shared.Return(rentedInput);
                }
            }
        }
#endif

        private delegate int IntGetter(IntPtr handle, out int value);

        private delegate int IntSetter(IntPtr handle, int value);

        private delegate int DoubleGetter(IntPtr handle, out double value);

        private delegate int DoubleSetter(IntPtr handle, double value);

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

        private double GetDouble(DoubleGetter getter)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(getter(NativeHandle, out double value));
            return value;
        }

        private void SetDouble(DoubleSetter setter, double value)
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
