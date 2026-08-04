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
    /// Provides good-features-to-track detection compatible with <c>cv::GFTTDetector</c>.
    /// 提供与 OpenCV <c>cv::GFTTDetector</c> 兼容的优质跟踪特征检测能力。
    /// </summary>
    public sealed class GFTTDetector : Feature2D
    {
#if NETCOREAPP3_1_OR_GREATER
        private const int StackallocKeyPointThreshold = 64;
#endif

        private NativeGfttHandle handle;
        private bool disposed;

        private GFTTDetector(IntPtr nativeHandle)
        {
            handle = NativeGfttHandle.FromNativePointer(nativeHandle);
        }

        /// <inheritdoc/>
        public override bool IsDisposed
        {
            get { return disposed; }
        }

        /// <summary>
        /// Gets or sets the maximum number of features.
        /// 获取或设置最大特征数量。
        /// </summary>
        public int MaxFeatures
        {
            get { return GetInt(NativeMethods.Features2DGfttGetMaxFeatures); }
            set { SetInt(NativeMethods.Features2DGfttSetMaxFeatures, value); }
        }

        /// <summary>
        /// Gets or sets the quality threshold.
        /// 获取或设置质量阈值。
        /// </summary>
        public double QualityLevel
        {
            get { return GetDouble(NativeMethods.Features2DGfttGetQualityLevel); }
            set { SetDouble(NativeMethods.Features2DGfttSetQualityLevel, value); }
        }

        /// <summary>
        /// Gets or sets the minimum distance between features.
        /// 获取或设置特征之间的最小距离。
        /// </summary>
        public double MinDistance
        {
            get { return GetDouble(NativeMethods.Features2DGfttGetMinDistance); }
            set { SetDouble(NativeMethods.Features2DGfttSetMinDistance, value); }
        }

        /// <summary>
        /// Gets or sets the block size.
        /// 获取或设置块尺寸。
        /// </summary>
        public int BlockSize
        {
            get { return GetInt(NativeMethods.Features2DGfttGetBlockSize); }
            set { SetInt(NativeMethods.Features2DGfttSetBlockSize, value); }
        }

        /// <summary>
        /// Gets or sets the gradient aperture size.
        /// 获取或设置梯度孔径尺寸。
        /// </summary>
        public int GradientSize
        {
            get { return GetInt(NativeMethods.Features2DGfttGetGradientSize); }
            set { SetInt(NativeMethods.Features2DGfttSetGradientSize, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the Harris detector is used.
        /// 获取或设置是否使用 Harris 检测器。
        /// </summary>
        public bool HarrisDetector
        {
            get { return GetInt(NativeMethods.Features2DGfttGetHarrisDetector) != 0; }
            set { SetInt(NativeMethods.Features2DGfttSetHarrisDetector, value ? 1 : 0); }
        }

        /// <summary>
        /// Gets or sets the Harris detector free parameter.
        /// 获取或设置 Harris 检测器自由参数。
        /// </summary>
        public double K
        {
            get { return GetDouble(NativeMethods.Features2DGfttGetK); }
            set { SetDouble(NativeMethods.Features2DGfttSetK, value); }
        }

        /// <inheritdoc/>
        public override bool Empty
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.Features2DGfttEmpty(NativeHandle, out int empty));
                return empty != 0;
            }
        }

        /// <inheritdoc/>
        public override int DescriptorSize
        {
            get { return GetInt(NativeMethods.Features2DGfttDescriptorSize); }
        }

        /// <inheritdoc/>
        public override int DescriptorType
        {
            get { return GetInt(NativeMethods.Features2DGfttDescriptorType); }
        }

        /// <inheritdoc/>
        public override NormTypes DefaultNorm
        {
            get { return (NormTypes)GetInt(NativeMethods.Features2DGfttDefaultNorm); }
        }

        /// <inheritdoc/>
        public override string DefaultName
        {
            get
            {
                ThrowIfDisposed();
                unsafe
                {
                    return NativeStringMarshaller.GetString(NativeHandle, NativeMethods.Features2DGfttDefaultNameLength, NativeMethods.Features2DGfttDefaultNameFill);
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
        /// Creates a GFTT detector.
        /// 创建 GFTT 检测器。
        /// </summary>
        public static GFTTDetector Create(
            int maxCorners = 1000,
            double qualityLevel = 0.01,
            double minDistance = 1.0,
            int blockSize = 3,
            int gradientSize = 3,
            bool useHarrisDetector = false,
            double k = 0.04)
        {
            NativeException.ThrowIfError(NativeMethods.Features2DGfttCreate(
                maxCorners,
                qualityLevel,
                minDistance,
                blockSize,
                gradientSize,
                useHarrisDetector ? 1 : 0,
                k,
                out IntPtr nativeHandle));
            return new GFTTDetector(nativeHandle);
        }

        /// <inheritdoc/>
        public override void Clear()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.Features2DGfttClear(NativeHandle));
        }

        /// <summary>
        /// Detects GFTT keypoints in an image.
        /// 检测图像中的 GFTT 关键点。
        /// </summary>
        public override KeyPoint[] Detect(Mat image, Mat? mask = null)
        {
            ThrowIfDisposed();
            ValidateNotNull(image, nameof(image));
            NativeException.ThrowIfError(NativeMethods.Features2DGfttDetectCount(NativeHandle, image.NativeHandle, OptionalHandle(mask), out int keypointCount));
            if (keypointCount <= 0)
            {
                return Array.Empty<KeyPoint>();
            }

            var native = new NativeKeyPoint[keypointCount];
            unsafe
            {
                fixed (NativeKeyPoint* nativePtr = native)
                {
                    NativeException.ThrowIfError(NativeMethods.Features2DGfttDetectFill(NativeHandle, image.NativeHandle, OptionalHandle(mask), nativePtr, native.Length, out int writtenCount));
                    return KeyPointMarshaller.FromNative(native, writtenCount);
                }
            }
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return disposed ? "{Disposed=True}" : "{MaxFeatures=" + MaxFeatures + ",QualityLevel=" + QualityLevel.ToString(CultureInfo.InvariantCulture) + ",MinDistance=" + MinDistance.ToString(CultureInfo.InvariantCulture) + "}";
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

        private void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }
    }
}
