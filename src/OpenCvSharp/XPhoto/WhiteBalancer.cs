using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.XPhoto
{
    /// <summary>
    /// Base class for xphoto white balancers.
    /// xphoto 白平衡器基类。
    /// </summary>
    public abstract class WhiteBalancer : IDisposable
    {
        private NativeWhiteBalancerHandle handle;
        private bool disposed;

        internal WhiteBalancer(NativeWhiteBalancerHandle handle)
        {
            this.handle = handle;
        }

        /// <summary>Gets whether this white balancer has been disposed. 获取白平衡器是否已经释放。</summary>
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

        /// <summary>
        /// Applies white balancing to an image.
        /// 对图像执行白平衡。
        /// </summary>
        public void BalanceWhite(Mat src, Mat dst)
        {
            ThrowIfDisposed();
            ValidateNotNull(src, nameof(src));
            ValidateNotNull(dst, nameof(dst));
            ValidateBalanceWhiteSource(src);
            NativeException.ThrowIfError(NativeMethods.XPhotoWhiteBalancerBalanceWhite(NativeHandle, src.NativeHandle, dst.NativeHandle));
        }

        /// <summary>
        /// Applies white balancing and returns a new matrix.
        /// 执行白平衡并返回新矩阵。
        /// </summary>
        public Mat BalanceWhite(Mat src)
        {
            var dst = new Mat();
            try
            {
                BalanceWhite(src, dst);
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
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>Releases managed and native resources. 释放托管和 native 资源。</summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing && handle != null)
            {
                handle.Dispose();
            }

            disposed = true;
        }

        /// <summary>Throws when the object has been disposed. 对象已释放时抛出异常。</summary>
        protected void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        /// <summary>Validates the source image before native white balancing. native 白平衡前校验输入图像。</summary>
        protected virtual void ValidateBalanceWhiteSource(Mat src)
        {
        }

        internal static void ValidateNotNull<T>(T value, string parameterName)
            where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }
    }
}
