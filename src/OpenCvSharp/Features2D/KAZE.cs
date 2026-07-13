using System;
using System.Globalization;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Features2D
{
    /// <summary>
    /// Provides KAZE keypoint detection and descriptor extraction compatible with <c>cv::xfeatures2d::KAZE</c>.
    /// 提供与 OpenCV <c>cv::xfeatures2d::KAZE</c> 兼容的 KAZE 关键点检测和描述子提取能力。
    /// </summary>
    public sealed unsafe class KAZE : Feature2D
    {
        private NativeKazeHandle handle;
        private bool disposed;

        private KAZE(IntPtr nativeHandle)
        {
            handle = NativeKazeHandle.FromNativePointer(nativeHandle);
        }

        /// <inheritdoc/>
        public override bool IsDisposed
        {
            get { return disposed; }
        }

        /// <summary>
        /// Gets or sets whether to extract extended descriptors.
        /// 获取或设置是否提取扩展描述子。
        /// </summary>
        public bool Extended
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.Features2DKazeGetExtended(NativeHandle, out int value));
                return value != 0;
            }

            set
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.Features2DKazeSetExtended(NativeHandle, value ? 1 : 0));
            }
        }

        /// <summary>
        /// Gets or sets whether to use upright descriptors.
        /// 获取或设置是否使用正立描述子。
        /// </summary>
        public bool Upright
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.Features2DKazeGetUpright(NativeHandle, out int value));
                return value != 0;
            }

            set
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.Features2DKazeSetUpright(NativeHandle, value ? 1 : 0));
            }
        }

        /// <summary>
        /// Gets or sets the detector response threshold.
        /// 获取或设置检测器响应阈值。
        /// </summary>
        public double Threshold
        {
            get { return GetDouble(NativeMethods.Features2DKazeGetThreshold); }
            set { SetDouble(NativeMethods.Features2DKazeSetThreshold, value); }
        }

        /// <summary>
        /// Gets or sets the maximum octave count.
        /// 获取或设置最大 octave 数量。
        /// </summary>
        public int NOctaves
        {
            get { return GetInt(NativeMethods.Features2DKazeGetNOctaves); }
            set { SetInt(NativeMethods.Features2DKazeSetNOctaves, value); }
        }

        /// <summary>
        /// Gets or sets the octave layer count.
        /// 获取或设置 octave 层数量。
        /// </summary>
        public int NOctaveLayers
        {
            get { return GetInt(NativeMethods.Features2DKazeGetNOctaveLayers); }
            set { SetInt(NativeMethods.Features2DKazeSetNOctaveLayers, value); }
        }

        /// <summary>
        /// Gets or sets the diffusivity type.
        /// 获取或设置扩散类型。
        /// </summary>
        public KazeDiffusivityType Diffusivity
        {
            get { return (KazeDiffusivityType)GetInt(NativeMethods.Features2DKazeGetDiffusivity); }
            set
            {
                ValidateDiffusivity(value, nameof(value));
                SetInt(NativeMethods.Features2DKazeSetDiffusivity, (int)value);
            }
        }

        /// <inheritdoc/>
        public override bool Empty
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.Features2DKazeEmpty(NativeHandle, out int empty));
                return empty != 0;
            }
        }

        /// <inheritdoc/>
        public override int DescriptorSize
        {
            get { return GetInt(NativeMethods.Features2DKazeDescriptorSize); }
        }

        /// <inheritdoc/>
        public override int DescriptorType
        {
            get { return GetInt(NativeMethods.Features2DKazeDescriptorType); }
        }

        /// <inheritdoc/>
        public override NormTypes DefaultNorm
        {
            get { return (NormTypes)GetInt(NativeMethods.Features2DKazeDefaultNorm); }
        }

        /// <inheritdoc/>
        public override string DefaultName
        {
            get
            {
                ThrowIfDisposed();
                unsafe
                {
                    return NativeStringMarshaller.GetString(NativeHandle, NativeMethods.Features2DKazeDefaultNameLength, NativeMethods.Features2DKazeDefaultNameFill);
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
        /// Creates a KAZE detector and descriptor extractor.
        /// 创建 KAZE 检测器和描述子提取器。
        /// </summary>
        public static KAZE Create(bool extended = false, bool upright = false, float threshold = 0.001F, int nOctaves = 4, int nOctaveLayers = 4, KazeDiffusivityType diffusivity = KazeDiffusivityType.DiffPmG2)
        {
            ValidateDiffusivity(diffusivity, nameof(diffusivity));
            NativeException.ThrowIfError(NativeMethods.Features2DKazeCreate(extended ? 1 : 0, upright ? 1 : 0, threshold, nOctaves, nOctaveLayers, (int)diffusivity, out IntPtr nativeHandle));
            return new KAZE(nativeHandle);
        }

        /// <inheritdoc/>
        public override void Clear()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.Features2DKazeClear(NativeHandle));
        }

        /// <inheritdoc/>
        public override KeyPoint[] Detect(Mat image, Mat? mask = null)
        {
            ThrowIfDisposed();
            ValidateNotNull(image, nameof(image));
            return Feature2DDescriptorInterop.Detect(NativeHandle, image.NativeHandle, OptionalHandle(mask), NativeMethods.Features2DKazeDetectCount, NativeMethods.Features2DKazeDetectFill);
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
            return Feature2DDescriptorInterop.Compute(NativeHandle, image.NativeHandle, keypoints, descriptors.NativeHandle, NativeMethods.Features2DKazeCompute);
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
            return Feature2DDescriptorInterop.Compute(NativeHandle, image.NativeHandle, keypoints, descriptors.NativeHandle, NativeMethods.Features2DKazeCompute);
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
                NativeMethods.Features2DKazeDetectAndComputeCount,
                NativeMethods.Features2DKazeDetectAndComputeFill);
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
                NativeMethods.Features2DKazeDetectAndComputeCount,
                NativeMethods.Features2DKazeDetectAndComputeFill);
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
            return disposed ? "{Disposed=True}" : "{Extended=" + Extended + ",Upright=" + Upright + ",Threshold=" + Threshold.ToString(CultureInfo.InvariantCulture) + "}";
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
