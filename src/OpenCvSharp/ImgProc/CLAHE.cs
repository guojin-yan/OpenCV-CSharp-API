using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.ImgProc
{
    /// <summary>
    /// Provides Contrast Limited Adaptive Histogram Equalization compatible with OpenCV <c>cv::CLAHE</c>.
    /// 提供与 OpenCV <c>cv::CLAHE</c> 兼容的限制对比度自适应直方图均衡化。
    /// </summary>
    public sealed class CLAHE : IDisposable
    {
        private NativeClaheHandle handle;
        private bool disposed;

        internal CLAHE(IntPtr nativeHandle)
        {
            handle = NativeClaheHandle.FromNativePointer(nativeHandle);
        }

        /// <summary>
        /// Gets a value indicating whether this object has been disposed.
        /// 获取此对象是否已经释放。
        /// </summary>
        public bool IsDisposed
        {
            get { return disposed; }
        }

        /// <summary>
        /// Gets or sets the contrast limiting threshold.
        /// 获取或设置对比度限制阈值。
        /// </summary>
        public double ClipLimit
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.ImgProcClaheGetClipLimit(NativeHandle, out double clipLimit));
                return clipLimit;
            }

            set
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.ImgProcClaheSetClipLimit(NativeHandle, value));
            }
        }

        /// <summary>
        /// Gets or sets the tile grid size.
        /// 获取或设置分块网格尺寸。
        /// </summary>
        public Size TilesGridSize
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.ImgProcClaheGetTilesGridSize(NativeHandle, out int width, out int height));
                return new Size(width, height);
            }

            set
            {
                ThrowIfDisposed();
                ValidatePositive(value.Width, nameof(value.Width));
                ValidatePositive(value.Height, nameof(value.Height));
                NativeException.ThrowIfError(NativeMethods.ImgProcClaheSetTilesGridSize(NativeHandle, value.Width, value.Height));
            }
        }

        /// <summary>
        /// Gets or sets the bit shift used for histogram bins.
        /// 获取或设置直方图 bin 使用的位移参数。
        /// </summary>
        public int BitShift
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.ImgProcClaheGetBitShift(NativeHandle, out int bitShift));
                return bitShift;
            }

            set
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.ImgProcClaheSetBitShift(NativeHandle, value));
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
        /// Equalizes the histogram of a grayscale image using CLAHE.
        /// 使用 CLAHE 对灰度图像执行直方图均衡化。
        /// </summary>
        /// <param name="src">The source image. 源图像。</param>
        /// <param name="dst">The destination image. 目标图像。</param>
        public void Apply(Mat src, Mat dst)
        {
            ThrowIfDisposed();
            ValidateNotNull(src, nameof(src));
            ValidateNotNull(dst, nameof(dst));
            NativeException.ThrowIfError(NativeMethods.ImgProcClaheApply(NativeHandle, src.NativeHandle, dst.NativeHandle));
        }

        /// <summary>
        /// Equalizes the histogram of a grayscale image using CLAHE and returns the result.
        /// 使用 CLAHE 对灰度图像执行直方图均衡化并返回结果。
        /// </summary>
        /// <param name="src">The source image. 源图像。</param>
        /// <returns>The equalized image. 均衡化后的图像。</returns>
        public Mat Apply(Mat src)
        {
            var dst = new Mat();
            Apply(src, dst);
            return dst;
        }

        /// <summary>
        /// Releases cached temporary buffers owned by the native CLAHE implementation.
        /// 释放 native CLAHE 实现持有的临时缓存。
        /// </summary>
        public void CollectGarbage()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.ImgProcClaheCollectGarbage(NativeHandle));
        }

        /// <summary>
        /// Releases the native CLAHE object.
        /// 释放 native CLAHE 对象。
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
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

        private static void ValidateNotNull<T>(T value, string parameterName)
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }

        private static void ValidatePositive(int value, string parameterName)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Value must be positive.");
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
