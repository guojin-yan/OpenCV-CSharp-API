using System;
using System.Globalization;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Features2D
{
    /// <summary>
    /// Provides AKAZE keypoint detection and descriptor extraction compatible with <c>cv::xfeatures2d::AKAZE</c>.
    /// 提供与 OpenCV <c>cv::xfeatures2d::AKAZE</c> 兼容的 AKAZE 关键点检测和描述子提取能力。
    /// </summary>
    public sealed unsafe class AKAZE : Feature2D
    {
        private NativeAkazeHandle handle;
        private bool disposed;

        private AKAZE(IntPtr nativeHandle)
        {
            handle = NativeAkazeHandle.FromNativePointer(nativeHandle);
        }

        /// <inheritdoc/>
        public override bool IsDisposed
        {
            get { return disposed; }
        }

        /// <summary>
        /// Gets or sets the AKAZE descriptor type.
        /// 获取或设置 AKAZE 描述子类型。
        /// </summary>
        public AkazeDescriptorType AkazeDescriptorType
        {
            get { return (AkazeDescriptorType)GetInt(NativeMethods.Features2DAkazeGetDescriptorType); }
            set
            {
                ValidateDescriptorType(value, nameof(value));
                SetInt(NativeMethods.Features2DAkazeSetDescriptorType, (int)value);
            }
        }

        /// <summary>
        /// Gets or sets the descriptor size in bits.
        /// 获取或设置描述子位数。
        /// </summary>
        public int AkazeDescriptorSize
        {
            get { return GetInt(NativeMethods.Features2DAkazeGetDescriptorSize); }
            set { SetInt(NativeMethods.Features2DAkazeSetDescriptorSize, value); }
        }

        /// <summary>
        /// Gets or sets the descriptor channel count.
        /// 获取或设置描述子通道数量。
        /// </summary>
        public int DescriptorChannels
        {
            get { return GetInt(NativeMethods.Features2DAkazeGetDescriptorChannels); }
            set { SetInt(NativeMethods.Features2DAkazeSetDescriptorChannels, value); }
        }

        /// <summary>
        /// Gets or sets the detector response threshold.
        /// 获取或设置检测器响应阈值。
        /// </summary>
        public double Threshold
        {
            get { return GetDouble(NativeMethods.Features2DAkazeGetThreshold); }
            set { SetDouble(NativeMethods.Features2DAkazeSetThreshold, value); }
        }

        /// <summary>
        /// Gets or sets the maximum octave count.
        /// 获取或设置最大 octave 数量。
        /// </summary>
        public int NOctaves
        {
            get { return GetInt(NativeMethods.Features2DAkazeGetNOctaves); }
            set { SetInt(NativeMethods.Features2DAkazeSetNOctaves, value); }
        }

        /// <summary>
        /// Gets or sets the octave layer count.
        /// 获取或设置 octave 层数量。
        /// </summary>
        public int NOctaveLayers
        {
            get { return GetInt(NativeMethods.Features2DAkazeGetNOctaveLayers); }
            set { SetInt(NativeMethods.Features2DAkazeSetNOctaveLayers, value); }
        }

        /// <summary>
        /// Gets or sets the diffusivity type.
        /// 获取或设置扩散类型。
        /// </summary>
        public KazeDiffusivityType Diffusivity
        {
            get { return (KazeDiffusivityType)GetInt(NativeMethods.Features2DAkazeGetDiffusivity); }
            set
            {
                ValidateDiffusivity(value, nameof(value));
                SetInt(NativeMethods.Features2DAkazeSetDiffusivity, (int)value);
            }
        }

        /// <summary>
        /// Gets or sets the maximum number of returned points.
        /// 获取或设置返回点的最大数量。
        /// </summary>
        public int MaxPoints
        {
            get { return GetInt(NativeMethods.Features2DAkazeGetMaxPoints); }
            set { SetInt(NativeMethods.Features2DAkazeSetMaxPoints, value); }
        }

        /// <inheritdoc/>
        public override bool Empty
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.Features2DAkazeEmpty(NativeHandle, out int empty));
                return empty != 0;
            }
        }

        /// <inheritdoc/>
        public override int DescriptorSize
        {
            get { return GetInt(NativeMethods.Features2DAkazeDescriptorSize); }
        }

        /// <inheritdoc/>
        public override int DescriptorType
        {
            get { return GetInt(NativeMethods.Features2DAkazeDescriptorType); }
        }

        /// <inheritdoc/>
        public override NormTypes DefaultNorm
        {
            get { return (NormTypes)GetInt(NativeMethods.Features2DAkazeDefaultNorm); }
        }

        /// <inheritdoc/>
        public override string DefaultName
        {
            get
            {
                ThrowIfDisposed();
                unsafe
                {
                    return NativeStringMarshaller.GetString(NativeHandle, NativeMethods.Features2DAkazeDefaultNameLength, NativeMethods.Features2DAkazeDefaultNameFill);
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
        /// Creates an AKAZE detector and descriptor extractor.
        /// 创建 AKAZE 检测器和描述子提取器。
        /// </summary>
        public static AKAZE Create(
            AkazeDescriptorType descriptorType = AkazeDescriptorType.DescriptorMldb,
            int descriptorSize = 0,
            int descriptorChannels = 3,
            float threshold = 0.001F,
            int nOctaves = 4,
            int nOctaveLayers = 4,
            KazeDiffusivityType diffusivity = KazeDiffusivityType.DiffPmG2,
            int maxPoints = -1)
        {
            ValidateDescriptorType(descriptorType, nameof(descriptorType));
            ValidateDiffusivity(diffusivity, nameof(diffusivity));
            NativeException.ThrowIfError(NativeMethods.Features2DAkazeCreate((int)descriptorType, descriptorSize, descriptorChannels, threshold, nOctaves, nOctaveLayers, (int)diffusivity, maxPoints, out IntPtr nativeHandle));
            return new AKAZE(nativeHandle);
        }

        /// <inheritdoc/>
        public override void Clear()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.Features2DAkazeClear(NativeHandle));
        }

        /// <inheritdoc/>
        public override KeyPoint[] Detect(Mat image, Mat? mask = null)
        {
            ThrowIfDisposed();
            ValidateNotNull(image, nameof(image));
            return Feature2DDescriptorInterop.Detect(NativeHandle, image.NativeHandle, OptionalHandle(mask), NativeMethods.Features2DAkazeDetectCount, NativeMethods.Features2DAkazeDetectFill);
        }

        /// <summary>
        /// Computes descriptors for caller-provided keypoints and returns the keypoints kept by OpenCV.
        /// 为调用方提供的关键点计算描述子，并返回 OpenCV 保留的关键点。
        /// </summary>
        public KeyPoint[] Compute(Mat image, KeyPoint[] keypoints, Mat descriptors)
        {
            ValidateNotNull(image, nameof(image));
            ValidateNotNull(keypoints, nameof(keypoints));
            ValidateNotNull(descriptors, nameof(descriptors));
            ThrowIfDisposed();
            return Feature2DDescriptorInterop.Compute(NativeHandle, image.NativeHandle, keypoints, descriptors.NativeHandle, NativeMethods.Features2DAkazeCompute);
        }

        /// <summary>
        /// Computes descriptors and replaces the keypoint array with the keypoints kept by OpenCV.
        /// 计算描述子，并用 OpenCV 保留的关键点替换关键点数组。
        /// </summary>
        public void Compute(Mat image, ref KeyPoint[] keypoints, Mat descriptors)
        {
            ValidateNotNull(keypoints, nameof(keypoints));
            keypoints = Compute(image, keypoints, descriptors);
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Computes descriptors from a span-backed keypoint sequence.
        /// 从 Span 支持的关键点序列计算描述子。
        /// </summary>
        public KeyPoint[] Compute(Mat image, ReadOnlySpan<KeyPoint> keypoints, Mat descriptors)
        {
            ValidateNotNull(image, nameof(image));
            ValidateNotNull(descriptors, nameof(descriptors));
            ThrowIfDisposed();
            return Feature2DDescriptorInterop.Compute(NativeHandle, image.NativeHandle, keypoints, descriptors.NativeHandle, NativeMethods.Features2DAkazeCompute);
        }
#endif

        /// <summary>
        /// Detects keypoints and computes descriptors.
        /// 检测关键点并计算描述子。
        /// </summary>
        public void DetectAndCompute(Mat image, Mat? mask, out KeyPoint[] keypoints, Mat descriptors, bool useProvidedKeypoints = false)
        {
            keypoints = DetectAndCompute(image, mask, Array.Empty<KeyPoint>(), descriptors, useProvidedKeypoints);
        }

        /// <summary>
        /// Detects or reuses keypoints and computes descriptors.
        /// 检测或复用关键点并计算描述子。
        /// </summary>
        public KeyPoint[] DetectAndCompute(Mat image, Mat? mask, KeyPoint[] keypoints, Mat descriptors, bool useProvidedKeypoints = true)
        {
            ValidateNotNull(image, nameof(image));
            ValidateNotNull(keypoints, nameof(keypoints));
            ValidateNotNull(descriptors, nameof(descriptors));
            ThrowIfDisposed();
            return Feature2DDescriptorInterop.DetectAndCompute(
                NativeHandle,
                image.NativeHandle,
                OptionalHandle(mask),
                keypoints,
                descriptors.NativeHandle,
                useProvidedKeypoints,
                NativeMethods.Features2DAkazeDetectAndComputeCount,
                NativeMethods.Features2DAkazeDetectAndComputeFill);
        }

        /// <summary>
        /// Detects or reuses keypoints, computes descriptors, and replaces the keypoint array with the OpenCV result.
        /// 检测或复用关键点、计算描述子，并用 OpenCV 返回结果替换关键点数组。
        /// </summary>
        public void DetectAndComputeInPlace(Mat image, Mat? mask, ref KeyPoint[] keypoints, Mat descriptors, bool useProvidedKeypoints)
        {
            ValidateNotNull(keypoints, nameof(keypoints));
            keypoints = DetectAndCompute(image, mask, keypoints, descriptors, useProvidedKeypoints);
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Computes descriptors using span-backed provided keypoints.
        /// 使用 Span 支持的已有关键点计算描述子。
        /// </summary>
        public KeyPoint[] DetectAndCompute(Mat image, Mat? mask, ReadOnlySpan<KeyPoint> keypoints, Mat descriptors)
        {
            ValidateNotNull(image, nameof(image));
            ValidateNotNull(descriptors, nameof(descriptors));
            ThrowIfDisposed();
            return Feature2DDescriptorInterop.DetectAndCompute(
                NativeHandle,
                image.NativeHandle,
                OptionalHandle(mask),
                keypoints,
                descriptors.NativeHandle,
                useProvidedKeypoints: true,
                NativeMethods.Features2DAkazeDetectAndComputeCount,
                NativeMethods.Features2DAkazeDetectAndComputeFill);
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
                : "{AkazeDescriptorType=" + AkazeDescriptorType
                    + ",AkazeDescriptorSize=" + AkazeDescriptorSize
                    + ",Threshold=" + Threshold.ToString(CultureInfo.InvariantCulture)
                    + ",MaxPoints=" + MaxPoints + "}";
        }

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

        private static void ValidateDescriptorType(AkazeDescriptorType value, string parameterName)
        {
            if (value != AkazeDescriptorType.DescriptorKazeUpright
                && value != AkazeDescriptorType.DescriptorKaze
                && value != AkazeDescriptorType.DescriptorMldbUpright
                && value != AkazeDescriptorType.DescriptorMldb)
            {
                throw new ArgumentOutOfRangeException(parameterName, "AKAZE descriptor type must be a defined descriptor type.");
            }
        }

        private static void ValidateDiffusivity(KazeDiffusivityType value, string parameterName)
        {
            if (value != KazeDiffusivityType.DiffPmG1
                && value != KazeDiffusivityType.DiffPmG2
                && value != KazeDiffusivityType.DiffWeickert
                && value != KazeDiffusivityType.DiffCharbonnier)
            {
                throw new ArgumentOutOfRangeException(parameterName, "KAZE diffusivity type must be a defined diffusivity type.");
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
