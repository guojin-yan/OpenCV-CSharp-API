using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.XImgProc
{
    /// <summary>
    /// Reusable fast global smoother filter wrapper.
    /// 可复用 fast global smoother filter 包装。
    /// </summary>
    public sealed class FastGlobalSmootherFilter : IDisposable
    {
        private NativeXImgProcFastGlobalSmootherFilterHandle handle;
        private readonly int guideRows;
        private readonly int guideCols;
        private bool disposed;

        private FastGlobalSmootherFilter(IntPtr nativeHandle, int guideRows, int guideCols)
        {
            handle = NativeXImgProcFastGlobalSmootherFilterHandle.FromNativePointer(nativeHandle);
            this.guideRows = guideRows;
            this.guideCols = guideCols;
        }

        /// <summary>Gets whether this filter has been disposed. 获取滤波器是否已经释放。</summary>
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

        /// <summary>Creates a fast global smoother filter. 创建 fast global smoother filter。</summary>
        public static FastGlobalSmootherFilter Create(Mat guide, double lambda, double sigmaColor, double lambdaAttenuation = 0.25, int numIter = 3)
        {
            XImgProcCv2.ValidateNotNull(guide, nameof(guide));
            XImgProcCv2.ValidateFastGlobalSmootherCreateArguments(guide, lambda, sigmaColor, numIter);
            NativeException.ThrowIfError(NativeMethods.XImgProcFastGlobalSmootherFilterCreate(guide.NativeHandle, lambda, sigmaColor, lambdaAttenuation, numIter, out IntPtr nativeHandle));
            return new FastGlobalSmootherFilter(nativeHandle, guide.Rows, guide.Cols);
        }

        /// <summary>Applies filtering into <paramref name="dst"/>. 将滤波结果写入 <paramref name="dst"/>。</summary>
        public void Filter(Mat src, Mat dst)
        {
            ThrowIfDisposed();
            XImgProcCv2.ValidateNotNull(src, nameof(src));
            XImgProcCv2.ValidateNotNull(dst, nameof(dst));
            XImgProcCv2.ValidateFastGlobalSmootherFilterSource(src, guideRows, guideCols);
            NativeException.ThrowIfError(NativeMethods.XImgProcFastGlobalSmootherFilterFilter(NativeHandle, src.NativeHandle, dst.NativeHandle));
        }

        /// <summary>Applies filtering and returns a new matrix. 执行滤波并返回新矩阵。</summary>
        public Mat Filter(Mat src)
        {
            var dst = new Mat();
            try
            {
                Filter(src, dst);
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

        private void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }
    }
}
