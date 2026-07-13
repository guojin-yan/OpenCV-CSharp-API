using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.XImgProc
{
    /// <summary>
    /// Reusable fast bilateral solver filter wrapper.
    /// 可复用 fast bilateral solver filter 包装。
    /// </summary>
    public sealed class FastBilateralSolverFilter : IDisposable
    {
        private NativeXImgProcFastBilateralSolverFilterHandle handle;
        private bool disposed;
        private readonly int guideRows;
        private readonly int guideCols;

        private FastBilateralSolverFilter(IntPtr nativeHandle, int guideRows, int guideCols)
        {
            handle = NativeXImgProcFastBilateralSolverFilterHandle.FromNativePointer(nativeHandle);
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

        /// <summary>Creates a fast bilateral solver filter. 创建 fast bilateral solver filter。</summary>
        public static FastBilateralSolverFilter Create(Mat guide, double sigmaSpatial, double sigmaLuma, double sigmaChroma, double lambda = 128.0, int numIter = 25, double maxTol = 1e-5)
        {
            XImgProcCv2.ValidateNotNull(guide, nameof(guide));
            XImgProcCv2.ValidateFastBilateralSolverCreateArguments(guide);
            NativeException.ThrowIfError(NativeMethods.XImgProcFastBilateralSolverFilterCreate(
                guide.NativeHandle,
                sigmaSpatial,
                sigmaLuma,
                sigmaChroma,
                lambda,
                numIter,
                maxTol,
                out IntPtr nativeHandle));
            return new FastBilateralSolverFilter(nativeHandle, guide.Rows, guide.Cols);
        }

        /// <summary>Applies filtering into <paramref name="dst"/>. 将滤波结果写入 <paramref name="dst"/>。</summary>
        public void Filter(Mat src, Mat confidence, Mat dst)
        {
            ThrowIfDisposed();
            XImgProcCv2.ValidateNotNull(src, nameof(src));
            XImgProcCv2.ValidateNotNull(confidence, nameof(confidence));
            XImgProcCv2.ValidateNotNull(dst, nameof(dst));
            XImgProcCv2.ValidateFastBilateralSolverFilterSource(src, guideRows, guideCols);
            XImgProcCv2.ValidateFastBilateralSolverConfidence(confidence, guideRows, guideCols);
            NativeException.ThrowIfError(NativeMethods.XImgProcFastBilateralSolverFilterFilter(NativeHandle, src.NativeHandle, confidence.NativeHandle, dst.NativeHandle));
        }

        /// <summary>Applies filtering and returns a new matrix. 执行滤波并返回新矩阵。</summary>
        public Mat Filter(Mat src, Mat confidence)
        {
            var dst = new Mat();
            try
            {
                Filter(src, confidence, dst);
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
