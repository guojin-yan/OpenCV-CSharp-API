using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.OptFlow
{
    /// <summary>
    /// Base wrapper for OpenCV sparse optical flow algorithms.
    /// OpenCV 稀疏光流算法基类包装。
    /// </summary>
    public class SparseOpticalFlow : IDisposable
    {
        private NativeOptFlowSparseHandle handle;
        private bool disposed;

        internal SparseOpticalFlow(IntPtr nativeHandle)
        {
            handle = NativeOptFlowSparseHandle.FromNativePointer(nativeHandle);
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
        /// Calculates sparse optical flow.
        /// 计算稀疏光流。
        /// </summary>
        public void Calc(Mat prevImg, Mat nextImg, Mat prevPts, Mat nextPts, Mat status, Mat err)
        {
            ThrowIfDisposed();
            DenseOpticalFlow.ValidateNotNull(prevImg, nameof(prevImg));
            DenseOpticalFlow.ValidateNotNull(nextImg, nameof(nextImg));
            DenseOpticalFlow.ValidateNotNull(prevPts, nameof(prevPts));
            DenseOpticalFlow.ValidateNotNull(nextPts, nameof(nextPts));
            DenseOpticalFlow.ValidateNotNull(status, nameof(status));
            DenseOpticalFlow.ValidateNotNull(err, nameof(err));
            NativeException.ThrowIfError(NativeMethods.OptFlowSparseCalc(NativeHandle, prevImg.NativeHandle, nextImg.NativeHandle, prevPts.NativeHandle, nextPts.NativeHandle, status.NativeHandle, err.NativeHandle));
        }

        /// <summary>Releases native resources. 释放 native 资源。</summary>
        public void Dispose()
        {
            if (!disposed)
            {
                handle.Dispose();
                disposed = true;
                GC.SuppressFinalize(this);
            }
        }

        /// <summary>Throws when disposed. 已释放时抛出异常。</summary>
        protected void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }
    }
}
