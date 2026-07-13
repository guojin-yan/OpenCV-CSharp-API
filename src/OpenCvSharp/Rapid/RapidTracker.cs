using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Rapid
{
    /// <summary>
    /// Base class for stateful OpenCV RAPID trackers.
    /// OpenCV RAPID 有状态 tracker 的基类。
    /// </summary>
    public class RapidTracker : IDisposable
    {
        private NativeRapidTrackerHandle handle;
        private bool disposed;

        internal RapidTracker(NativeRapidTrackerHandle handle)
        {
            this.handle = handle;
        }

        /// <summary>Gets whether this tracker has been disposed. 获取对象是否已经释放。</summary>
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

        /// <summary>Runs tracker computation and updates pose vectors in place. 运行 tracker 计算并原地更新 pose 向量。</summary>
        public float Compute(
            Mat img,
            int num,
            int len,
            Mat cameraMatrix,
            Mat rvec,
            Mat tvec,
            TermCriteria? termCriteria = null)
        {
            ThrowIfDisposed();
            RapidCv2.ValidateNotNull(img, nameof(img));
            RapidCv2.ValidateAtLeast(num, 3, nameof(num));
            RapidCv2.ValidatePositive(len, nameof(len));
            RapidCv2.ValidateNotNull(cameraMatrix, nameof(cameraMatrix));
            RapidCv2.ValidateNotNull(rvec, nameof(rvec));
            RapidCv2.ValidateNotNull(tvec, nameof(tvec));
            TermCriteria criteria = termCriteria ?? TermCriteria.ByCountAndEpsilon(5, 1.5);
            NativeException.ThrowIfError(NativeMethods.RapidTrackerCompute(
                NativeHandle,
                img.NativeHandle,
                num,
                len,
                cameraMatrix.NativeHandle,
                rvec.NativeHandle,
                tvec.NativeHandle,
                (int)criteria.Type,
                criteria.MaxCount,
                criteria.Epsilon,
                out float ratio));
            return ratio;
        }

        /// <summary>Clears tracker state. 清除 tracker 状态。</summary>
        public void ClearState()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.RapidTrackerClearState(NativeHandle));
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

        internal void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }
    }
}
