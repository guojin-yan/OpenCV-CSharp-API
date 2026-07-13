using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Saliency
{
    /// <summary>
    /// Base wrapper for OpenCV contrib saliency algorithms.
    /// OpenCV contrib 显著性算法基类包装。
    /// </summary>
    public class Saliency : IDisposable
    {
        private NativeSaliencyHandle handle;
        private bool disposed;

        internal Saliency(IntPtr nativeHandle)
        {
            handle = NativeSaliencyHandle.FromNativePointer(nativeHandle);
        }

        /// <summary>Gets whether this object has been disposed. 获取对象是否已经释放。</summary>
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
        /// Computes saliency into <paramref name="saliencyMap"/>.
        /// 将显著性结果计算到 <paramref name="saliencyMap"/>。
        /// </summary>
        public bool ComputeSaliency(Mat image, Mat saliencyMap)
        {
            ThrowIfDisposed();
            ValidateNotNull(image, nameof(image));
            ValidateNotNull(saliencyMap, nameof(saliencyMap));
            ValidateComputeSaliencyImage(image, nameof(image));
            NativeException.ThrowIfError(NativeMethods.SaliencyComputeSaliency(NativeHandle, image.NativeHandle, saliencyMap.NativeHandle, out int result));
            return result != 0;
        }

        /// <summary>
        /// Computes saliency and returns a new matrix.
        /// 计算显著性并返回新矩阵。
        /// </summary>
        public Mat ComputeSaliency(Mat image)
        {
            var saliencyMap = new Mat();
            try
            {
                ComputeSaliency(image, saliencyMap);
                return saliencyMap;
            }
            catch
            {
                saliencyMap.Dispose();
                throw;
            }
        }

        /// <summary>Releases native resources. 释放 native 资源。</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        internal static void ValidateNotNull<T>(T value, string parameterName)
            where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }

        internal virtual void ValidateComputeSaliencyImage(Mat image, string parameterName)
        {
        }

        /// <summary>Throws when disposed. 已释放时抛出异常。</summary>
        protected void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        /// <summary>Disposes this instance. 释放当前实例。</summary>
        protected virtual void Dispose(bool disposing)
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
    }
}
