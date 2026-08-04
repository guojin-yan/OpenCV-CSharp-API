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
    /// Provides ORB keypoint detection and binary descriptor extraction compatible with <c>cv::ORB</c>.
    /// 提供与 OpenCV <c>cv::ORB</c> 兼容的 ORB 关键点检测和二进制描述子提取能力。
    /// </summary>
    public sealed class ORB : Feature2D
    {
#if NETCOREAPP3_1_OR_GREATER
        private const int StackallocKeyPointThreshold = 64;
#endif

        private NativeOrbHandle handle;
        private bool disposed;

        private ORB(IntPtr nativeHandle)
        {
            handle = NativeOrbHandle.FromNativePointer(nativeHandle);
        }

        /// <inheritdoc/>
        public override bool IsDisposed
        {
            get { return disposed; }
        }

        /// <summary>
        /// Gets or sets the maximum number of retained features.
        /// 获取或设置保留的最大特征数量。
        /// </summary>
        public int MaxFeatures
        {
            get { return GetInt(NativeMethods.Features2DOrbGetMaxFeatures); }
            set { SetInt(NativeMethods.Features2DOrbSetMaxFeatures, value); }
        }

        /// <summary>
        /// Gets or sets the pyramid scale factor.
        /// 获取或设置金字塔缩放因子。
        /// </summary>
        public double ScaleFactor
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.Features2DOrbGetScaleFactor(NativeHandle, out double value));
                return value;
            }

            set
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.Features2DOrbSetScaleFactor(NativeHandle, value));
            }
        }

        /// <summary>
        /// Gets or sets the number of pyramid levels.
        /// 获取或设置金字塔层数。
        /// </summary>
        public int NLevels
        {
            get { return GetInt(NativeMethods.Features2DOrbGetNLevels); }
            set { SetInt(NativeMethods.Features2DOrbSetNLevels, value); }
        }

        /// <summary>
        /// Gets or sets the edge threshold.
        /// 获取或设置边缘阈值。
        /// </summary>
        public int EdgeThreshold
        {
            get { return GetInt(NativeMethods.Features2DOrbGetEdgeThreshold); }
            set { SetInt(NativeMethods.Features2DOrbSetEdgeThreshold, value); }
        }

        /// <summary>
        /// Gets or sets the first pyramid level.
        /// 获取或设置第一个金字塔层级。
        /// </summary>
        public int FirstLevel
        {
            get { return GetInt(NativeMethods.Features2DOrbGetFirstLevel); }
            set { SetInt(NativeMethods.Features2DOrbSetFirstLevel, value); }
        }

        /// <summary>
        /// Gets or sets the number of points used by each BRIEF descriptor element.
        /// 获取或设置每个 BRIEF 描述子元素使用的点数。
        /// </summary>
        public int WtaK
        {
            get { return GetInt(NativeMethods.Features2DOrbGetWtaK); }
            set { SetInt(NativeMethods.Features2DOrbSetWtaK, value); }
        }

        /// <summary>
        /// Gets or sets the keypoint score type.
        /// 获取或设置关键点评分类型。
        /// </summary>
        public OrbScoreType ScoreType
        {
            get { return (OrbScoreType)GetInt(NativeMethods.Features2DOrbGetScoreType); }
            set
            {
                ValidateScoreType(value, nameof(value));
                SetInt(NativeMethods.Features2DOrbSetScoreType, (int)value);
            }
        }

        /// <summary>
        /// Gets or sets the descriptor patch size.
        /// 获取或设置描述子 patch 尺寸。
        /// </summary>
        public int PatchSize
        {
            get { return GetInt(NativeMethods.Features2DOrbGetPatchSize); }
            set { SetInt(NativeMethods.Features2DOrbSetPatchSize, value); }
        }

        /// <summary>
        /// Gets or sets the FAST detector threshold.
        /// 获取或设置 FAST 检测阈值。
        /// </summary>
        public int FastThreshold
        {
            get { return GetInt(NativeMethods.Features2DOrbGetFastThreshold); }
            set { SetInt(NativeMethods.Features2DOrbSetFastThreshold, value); }
        }

        /// <inheritdoc/>
        public override bool Empty
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.Features2DOrbEmpty(NativeHandle, out int empty));
                return empty != 0;
            }
        }

        /// <inheritdoc/>
        public override int DescriptorSize
        {
            get { return GetInt(NativeMethods.Features2DOrbDescriptorSize); }
        }

        /// <inheritdoc/>
        public override int DescriptorType
        {
            get { return GetInt(NativeMethods.Features2DOrbDescriptorType); }
        }

        /// <inheritdoc/>
        public override NormTypes DefaultNorm
        {
            get { return (NormTypes)GetInt(NativeMethods.Features2DOrbDefaultNorm); }
        }

        /// <inheritdoc/>
        public override string DefaultName
        {
            get
            {
                ThrowIfDisposed();
                unsafe
                {
                    return NativeStringMarshaller.GetString(NativeHandle, NativeMethods.Features2DOrbDefaultNameLength, NativeMethods.Features2DOrbDefaultNameFill);
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
        /// Creates an ORB detector and descriptor extractor.
        /// 创建 ORB 检测器和描述子提取器。
        /// </summary>
        public static ORB Create(
            int maxFeatures = 500,
            float scaleFactor = 1.2F,
            int nLevels = 8,
            int edgeThreshold = 31,
            int firstLevel = 0,
            int wtaK = 2,
            OrbScoreType scoreType = OrbScoreType.HarrisScore,
            int patchSize = 31,
            int fastThreshold = 20)
        {
            ValidateScoreType(scoreType, nameof(scoreType));
            NativeException.ThrowIfError(NativeMethods.Features2DOrbCreate(
                maxFeatures,
                scaleFactor,
                nLevels,
                edgeThreshold,
                firstLevel,
                wtaK,
                (int)scoreType,
                patchSize,
                fastThreshold,
                out IntPtr nativeHandle));
            return new ORB(nativeHandle);
        }

        /// <inheritdoc/>
        public override void Clear()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.Features2DOrbClear(NativeHandle));
        }

        /// <summary>
        /// Detects keypoints in an image.
        /// 检测图像中的关键点。
        /// </summary>
        public override KeyPoint[] Detect(Mat image, Mat? mask = null)
        {
            ThrowIfDisposed();
            ValidateNotNull(image, nameof(image));
            NativeException.ThrowIfError(NativeMethods.Features2DOrbDetectCount(NativeHandle, image.NativeHandle, OptionalHandle(mask), out int keypointCount));
            if (keypointCount <= 0)
            {
                return Array.Empty<KeyPoint>();
            }

            var native = new NativeKeyPoint[keypointCount];
            unsafe
            {
                fixed (NativeKeyPoint* nativePtr = native)
                {
                    NativeException.ThrowIfError(NativeMethods.Features2DOrbDetectFill(NativeHandle, image.NativeHandle, OptionalHandle(mask), nativePtr, native.Length, out int writtenCount));
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
            KeyPoint[] initial = Array.Empty<KeyPoint>();
            keypoints = DetectAndComputeCore(image, mask, initial, descriptors, useProvidedKeypoints);
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
                : "{MaxFeatures=" + MaxFeatures
                    + ",ScaleFactor=" + ScaleFactor.ToString(CultureInfo.InvariantCulture)
                    + ",NLevels=" + NLevels
                    + ",ScoreType=" + ScoreType + "}";
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
                    NativeException.ThrowIfError(NativeMethods.Features2DOrbCompute(
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
                    NativeException.ThrowIfError(NativeMethods.Features2DOrbCompute(
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
                    NativeException.ThrowIfError(NativeMethods.Features2DOrbDetectAndComputeCount(
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
                        NativeException.ThrowIfError(NativeMethods.Features2DOrbDetectAndComputeFill(
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
                    NativeException.ThrowIfError(NativeMethods.Features2DOrbDetectAndComputeCount(
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
                        NativeException.ThrowIfError(NativeMethods.Features2DOrbDetectAndComputeFill(
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

        private static void ValidateNotNull<T>(T value, string parameterName)
            where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }

        private static void ValidateScoreType(OrbScoreType value, string parameterName)
        {
            if (value != OrbScoreType.HarrisScore && value != OrbScoreType.FastScore)
            {
                throw new ArgumentOutOfRangeException(parameterName, "ORB score type must be HarrisScore or FastScore.");
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
