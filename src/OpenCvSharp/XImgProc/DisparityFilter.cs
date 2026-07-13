using System;
using OpenCvSharp.Core;

namespace OpenCvSharp.XImgProc
{
    /// <summary>
    /// Base managed wrapper for ximgproc disparity filters.
    /// ximgproc disparity filter 的 managed 基类。
    /// </summary>
    public abstract class DisparityFilter : IDisposable
    {
        private bool disposed;

        /// <summary>Gets whether this filter has been disposed. 获取滤波器是否已经释放。</summary>
        public bool IsDisposed
        {
            get { return disposed; }
        }

        internal abstract IntPtr NativeHandle { get; }

        /// <summary>Filters a disparity map. 滤波 disparity map。</summary>
        public abstract void Filter(Mat disparityMapLeft, Mat leftView, Mat filteredDisparityMap, Mat? disparityMapRight = null, Rect? roi = null, Mat? rightView = null);

        /// <summary>Filters a disparity map and returns a new matrix. 滤波 disparity map 并返回新矩阵。</summary>
        public Mat Filter(Mat disparityMapLeft, Mat leftView, Mat? disparityMapRight = null, Rect? roi = null, Mat? rightView = null)
        {
            var dst = new Mat();
            try
            {
                Filter(disparityMapLeft, leftView, dst, disparityMapRight, roi, rightView);
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

        internal void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        /// <summary>Disposes derived resources. 释放派生资源。</summary>
        protected virtual void Dispose(bool disposing)
        {
            disposed = true;
        }
    }
}
