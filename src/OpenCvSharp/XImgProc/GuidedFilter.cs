using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.XImgProc
{
    /// <summary>
    /// Reusable guided filter wrapper.
    /// 可复用 guided filter 包装。
    /// </summary>
    public sealed class GuidedFilter : IDisposable
    {
        private NativeXImgProcGuidedFilterHandle handle;
        private readonly int guideRows;
        private readonly int guideCols;
        private bool disposed;

        private GuidedFilter(IntPtr nativeHandle, int guideRows, int guideCols)
        {
            handle = NativeXImgProcGuidedFilterHandle.FromNativePointer(nativeHandle);
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

        /// <summary>Creates a guided filter. 创建 guided filter。</summary>
        public static GuidedFilter Create(Mat guide, int radius, double eps, double scale = 1.0)
        {
            XImgProcCv2.ValidateNotNull(guide, nameof(guide));
            XImgProcCv2.ValidateGuidedFilterCreateArguments(guide, radius, eps, scale);
            NativeException.ThrowIfError(NativeMethods.XImgProcGuidedFilterCreate(guide.NativeHandle, radius, eps, scale, out IntPtr nativeHandle));
            return new GuidedFilter(nativeHandle, guide.Rows, guide.Cols);
        }

        /// <summary>Applies filtering into <paramref name="dst"/>. 将滤波结果写入 <paramref name="dst"/>。</summary>
        public void Filter(Mat src, Mat dst, int dDepth = -1)
        {
            ThrowIfDisposed();
            XImgProcCv2.ValidateNotNull(src, nameof(src));
            XImgProcCv2.ValidateNotNull(dst, nameof(dst));
            XImgProcCv2.ValidateGuidedFilterSource(src, guideRows, guideCols);
            NativeException.ThrowIfError(NativeMethods.XImgProcGuidedFilterFilter(NativeHandle, src.NativeHandle, dst.NativeHandle, dDepth));
        }

        /// <summary>Applies filtering and returns a new matrix. 执行滤波并返回新矩阵。</summary>
        public Mat Filter(Mat src, int dDepth = -1)
        {
            var dst = new Mat();
            try
            {
                Filter(src, dst, dDepth);
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
