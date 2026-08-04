using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Features2D
{
    /// <summary>
    /// Provides blob detection compatible with <c>cv::SimpleBlobDetector</c>.
    /// 提供与 OpenCV <c>cv::SimpleBlobDetector</c> 兼容的斑点检测能力。
    /// </summary>
    public sealed class SimpleBlobDetector : Feature2D
    {
        private NativeSimpleBlobDetectorHandle handle;
        private bool disposed;

        private SimpleBlobDetector(IntPtr nativeHandle)
        {
            handle = NativeSimpleBlobDetectorHandle.FromNativePointer(nativeHandle);
        }

        /// <inheritdoc/>
        public override bool IsDisposed
        {
            get { return disposed; }
        }

        /// <summary>
        /// Gets or sets the detector parameters.
        /// 获取或设置检测器参数。
        /// </summary>
        public SimpleBlobDetectorParams Parameters
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.Features2DSimpleBlobGetParams(NativeHandle, out NativeSimpleBlobParams native));
                return SimpleBlobDetectorParams.FromNative(native);
            }

            set
            {
                ThrowIfDisposed();
                ValidateNotNull(value, nameof(value));
                NativeSimpleBlobParams native = value.ToNative();
                NativeException.ThrowIfError(NativeMethods.Features2DSimpleBlobSetParams(NativeHandle, ref native));
            }
        }

        /// <inheritdoc/>
        public override bool Empty
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.Features2DSimpleBlobEmpty(NativeHandle, out int empty));
                return empty != 0;
            }
        }

        /// <inheritdoc/>
        public override int DescriptorSize
        {
            get { return GetInt(NativeMethods.Features2DSimpleBlobDescriptorSize); }
        }

        /// <inheritdoc/>
        public override int DescriptorType
        {
            get { return GetInt(NativeMethods.Features2DSimpleBlobDescriptorType); }
        }

        /// <inheritdoc/>
        public override NormTypes DefaultNorm
        {
            get { return (NormTypes)GetInt(NativeMethods.Features2DSimpleBlobDefaultNorm); }
        }

        /// <inheritdoc/>
        public override string DefaultName
        {
            get
            {
                ThrowIfDisposed();
                unsafe
                {
                    return NativeStringMarshaller.GetString(NativeHandle, NativeMethods.Features2DSimpleBlobDefaultNameLength, NativeMethods.Features2DSimpleBlobDefaultNameFill);
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
        /// Creates a blob detector with OpenCV default parameters.
        /// 使用 OpenCV 默认参数创建斑点检测器。
        /// </summary>
        /// <returns>The created detector. 创建的检测器。</returns>
        public static SimpleBlobDetector Create()
        {
            NativeException.ThrowIfError(NativeMethods.Features2DSimpleBlobCreateDefault(out IntPtr nativeHandle));
            return new SimpleBlobDetector(nativeHandle);
        }

        /// <summary>
        /// Creates a blob detector with caller-provided parameters.
        /// 使用调用方提供的参数创建斑点检测器。
        /// </summary>
        /// <param name="parameters">The detector parameters. 检测器参数。</param>
        /// <returns>The created detector. 创建的检测器。</returns>
        public static SimpleBlobDetector Create(SimpleBlobDetectorParams parameters)
        {
            ValidateNotNull(parameters, nameof(parameters));
            NativeSimpleBlobParams native = parameters.ToNative();
            NativeException.ThrowIfError(NativeMethods.Features2DSimpleBlobCreate(ref native, out IntPtr nativeHandle));
            return new SimpleBlobDetector(nativeHandle);
        }

        /// <inheritdoc/>
        public override void Clear()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.Features2DSimpleBlobClear(NativeHandle));
        }

        /// <summary>
        /// Detects blob keypoints in an image.
        /// 检测图像中的斑点关键点。
        /// </summary>
        /// <param name="image">The input image. 输入图像。</param>
        /// <param name="mask">The optional mask. 可选掩码。</param>
        /// <returns>The detected blob keypoints. 检测到的斑点关键点。</returns>
        public override KeyPoint[] Detect(Mat image, Mat? mask = null)
        {
            ThrowIfDisposed();
            ValidateNotNull(image, nameof(image));
            NativeException.ThrowIfError(NativeMethods.Features2DSimpleBlobDetectCount(NativeHandle, image.NativeHandle, OptionalHandle(mask), out int keypointCount));
            if (keypointCount <= 0)
            {
                return Array.Empty<KeyPoint>();
            }

            var native = new NativeKeyPoint[keypointCount];
            unsafe
            {
                fixed (NativeKeyPoint* nativePtr = native)
                {
                    NativeException.ThrowIfError(NativeMethods.Features2DSimpleBlobDetectFill(NativeHandle, image.NativeHandle, OptionalHandle(mask), nativePtr, native.Length, out int writtenCount));
                    return KeyPointMarshaller.FromNative(native, writtenCount);
                }
            }
        }

        /// <summary>
        /// Gets blob contours cached by the last <see cref="Detect(Mat, Mat?)"/> call.
        /// 获取上一次 <see cref="Detect(Mat, Mat?)"/> 调用缓存的斑点轮廓。
        /// </summary>
        /// <returns>The collected blob contours, or an empty array when no contours were collected. 收集到的斑点轮廓；未收集到轮廓时为空数组。</returns>
        /// <remarks>
        /// <see cref="SimpleBlobDetectorParams.CollectContours"/> must be enabled before detection.
        /// 必须在检测前启用 <see cref="SimpleBlobDetectorParams.CollectContours"/>。
        /// </remarks>
        public Point[][] GetBlobContours()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.Features2DSimpleBlobGetBlobContoursCount(NativeHandle, out int contourCount, out int totalPointCount));
            if (contourCount <= 0)
            {
                return Array.Empty<Point[]>();
            }

            int[] offsets = new int[contourCount + 1];
            var points = new NativePoint[Math.Max(totalPointCount, 1)];
            unsafe
            {
                fixed (int* offsetsPtr = offsets)
                fixed (NativePoint* pointsPtr = points)
                {
                    NativeException.ThrowIfError(NativeMethods.Features2DSimpleBlobGetBlobContoursFill(
                        NativeHandle,
                        offsetsPtr,
                        offsets.Length,
                        pointsPtr,
                        points.Length,
                        out int writtenContourCount,
                        out int writtenPointCount));
                    return FromNativeContours(offsets, writtenContourCount, points, writtenPointCount);
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
            return disposed ? "{Disposed=True}" : "{Parameters=" + Parameters + "}";
        }

        private delegate int IntGetter(IntPtr handle, out int value);

        private int GetInt(IntGetter getter)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(getter(NativeHandle, out int value));
            return value;
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

        private static Point[][] FromNativeContours(int[] offsets, int contourCount, NativePoint[] points, int pointCount)
        {
            var result = new Point[contourCount][];
            for (int i = 0; i < contourCount; i++)
            {
                int start = offsets[i];
                int end = offsets[i + 1];
                int length = Math.Max(0, Math.Min(end, pointCount) - start);
                var contour = new Point[length];
                for (int j = 0; j < length; j++)
                {
                    NativePoint point = points[start + j];
                    contour[j] = new Point(point.X, point.Y);
                }

                result[i] = contour;
            }

            return result;
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
