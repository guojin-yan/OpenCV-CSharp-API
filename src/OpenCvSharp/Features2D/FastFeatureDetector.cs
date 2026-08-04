using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Features2D
{
    /// <summary>
    /// Provides FAST keypoint detection compatible with <c>cv::FastFeatureDetector</c>.
    /// 提供与 OpenCV <c>cv::FastFeatureDetector</c> 兼容的 FAST 关键点检测能力。
    /// </summary>
    public sealed class FastFeatureDetector : Feature2D
    {
        private NativeFastFeatureDetectorHandle handle;
        private bool disposed;

        private FastFeatureDetector(IntPtr nativeHandle)
        {
            handle = NativeFastFeatureDetectorHandle.FromNativePointer(nativeHandle);
        }

        /// <inheritdoc/>
        public override bool IsDisposed
        {
            get { return disposed; }
        }

        /// <summary>
        /// Gets or sets the intensity threshold used by FAST.
        /// 获取或设置 FAST 使用的亮度差阈值。
        /// </summary>
        public int Threshold
        {
            get { return GetInt(NativeMethods.Features2DFastGetThreshold); }
            set { SetInt(NativeMethods.Features2DFastSetThreshold, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether non-maximum suppression is enabled.
        /// 获取或设置是否启用非极大值抑制。
        /// </summary>
        public bool NonmaxSuppression
        {
            get { return GetInt(NativeMethods.Features2DFastGetNonmaxSuppression) != 0; }
            set { SetInt(NativeMethods.Features2DFastSetNonmaxSuppression, value ? 1 : 0); }
        }

        /// <summary>
        /// Gets or sets the FAST neighborhood type.
        /// 获取或设置 FAST 邻域类型。
        /// </summary>
        public FastFeatureDetectorType Type
        {
            get { return (FastFeatureDetectorType)GetInt(NativeMethods.Features2DFastGetType); }
            set
            {
                ValidateType(value, nameof(value));
                SetInt(NativeMethods.Features2DFastSetType, (int)value);
            }
        }

        /// <inheritdoc/>
        public override bool Empty
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.Features2DFastEmpty(NativeHandle, out int empty));
                return empty != 0;
            }
        }

        /// <inheritdoc/>
        public override int DescriptorSize
        {
            get { return GetInt(NativeMethods.Features2DFastDescriptorSize); }
        }

        /// <inheritdoc/>
        public override int DescriptorType
        {
            get { return GetInt(NativeMethods.Features2DFastDescriptorType); }
        }

        /// <inheritdoc/>
        public override NormTypes DefaultNorm
        {
            get { return (NormTypes)GetInt(NativeMethods.Features2DFastDefaultNorm); }
        }

        /// <inheritdoc/>
        public override string DefaultName
        {
            get
            {
                ThrowIfDisposed();
                unsafe
                {
                    return NativeStringMarshaller.GetString(NativeHandle, NativeMethods.Features2DFastDefaultNameLength, NativeMethods.Features2DFastDefaultNameFill);
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
        /// Creates a FAST feature detector.
        /// 创建 FAST 特征检测器。
        /// </summary>
        public static FastFeatureDetector Create(int threshold = 10, bool nonmaxSuppression = true, FastFeatureDetectorType type = FastFeatureDetectorType.Type9_16)
        {
            ValidateType(type, nameof(type));
            NativeException.ThrowIfError(NativeMethods.Features2DFastCreate(threshold, nonmaxSuppression ? 1 : 0, (int)type, out IntPtr nativeHandle));
            return new FastFeatureDetector(nativeHandle);
        }

        /// <inheritdoc/>
        public override void Clear()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.Features2DFastClear(NativeHandle));
        }

        /// <summary>
        /// Detects FAST keypoints in an image.
        /// 检测图像中的 FAST 关键点。
        /// </summary>
        public override KeyPoint[] Detect(Mat image, Mat? mask = null)
        {
            ThrowIfDisposed();
            ValidateNotNull(image, nameof(image));
            NativeException.ThrowIfError(NativeMethods.Features2DFastDetectCount(NativeHandle, image.NativeHandle, OptionalHandle(mask), out int keypointCount));
            if (keypointCount <= 0)
            {
                return Array.Empty<KeyPoint>();
            }

            var native = new NativeKeyPoint[keypointCount];
            unsafe
            {
                fixed (NativeKeyPoint* nativePtr = native)
                {
                    NativeException.ThrowIfError(NativeMethods.Features2DFastDetectFill(NativeHandle, image.NativeHandle, OptionalHandle(mask), nativePtr, native.Length, out int writtenCount));
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
            return disposed ? "{Disposed=True}" : "{Threshold=" + Threshold + ",NonmaxSuppression=" + NonmaxSuppression + ",Type=" + Type + "}";
        }

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

        private static void ValidateType(FastFeatureDetectorType value, string parameterName)
        {
            if (value != FastFeatureDetectorType.Type5_8 &&
                value != FastFeatureDetectorType.Type7_12 &&
                value != FastFeatureDetectorType.Type9_16)
            {
                throw new ArgumentOutOfRangeException(parameterName, "FAST detector type must be Type5_8, Type7_12, or Type9_16.");
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
