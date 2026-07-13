using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.OptFlow
{
    /// <summary>
    /// Base wrapper for OpenCV dense optical flow algorithms.
    /// OpenCV 密集光流算法基类包装。
    /// </summary>
    public class DenseOpticalFlow : IDisposable
    {
        private NativeOptFlowDenseHandle handle;
        private bool disposed;

        internal DenseOpticalFlow(IntPtr nativeHandle)
        {
            handle = NativeOptFlowDenseHandle.FromNativePointer(nativeHandle);
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
        /// Calculates dense optical flow into <paramref name="flow"/>.
        /// 将密集光流计算到 <paramref name="flow"/>。
        /// </summary>
        public void Calc(Mat i0, Mat i1, Mat flow)
        {
            ThrowIfDisposed();
            ValidateNotNull(i0, nameof(i0));
            ValidateNotNull(i1, nameof(i1));
            ValidateNotNull(flow, nameof(flow));
            NativeException.ThrowIfError(NativeMethods.OptFlowDenseCalc(NativeHandle, i0.NativeHandle, i1.NativeHandle, flow.NativeHandle));
        }

        /// <summary>
        /// Calculates and returns a dense optical flow matrix.
        /// 计算并返回密集光流矩阵。
        /// </summary>
        public Mat Calc(Mat i0, Mat i1)
        {
            var flow = new Mat();
            try
            {
                Calc(i0, i1, flow);
                return flow;
            }
            catch
            {
                flow.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Releases algorithm inner buffers.
        /// 释放算法内部缓冲区。
        /// </summary>
        public void CollectGarbage()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.OptFlowDenseCollectGarbage(NativeHandle));
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

        internal static void ValidateNotNull<T>(T value, string parameterName)
            where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
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
