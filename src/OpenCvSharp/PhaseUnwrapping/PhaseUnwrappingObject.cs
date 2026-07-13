using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.PhaseUnwrapping
{
    /// <summary>
    /// Base class for OpenCV phase unwrapping algorithms.
    /// OpenCV 相位展开算法基类。
    /// </summary>
    public abstract class PhaseUnwrappingObject : IDisposable
    {
        private NativePhaseUnwrappingHandle handle;
        private bool disposed;

        internal PhaseUnwrappingObject(NativePhaseUnwrappingHandle handle)
        {
            this.handle = handle;
        }

        /// <summary>Gets whether this algorithm has been disposed. 获取算法对象是否已经释放。</summary>
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
        /// Unwraps a wrapped phase map into <paramref name="unwrappedPhaseMap"/>.
        /// 将包裹相位图展开到 <paramref name="unwrappedPhaseMap"/>。
        /// </summary>
        public void UnwrapPhaseMap(Mat wrappedPhaseMap, Mat unwrappedPhaseMap, Mat? shadowMask = null)
        {
            ThrowIfDisposed();
            ValidateNotNull(wrappedPhaseMap, nameof(wrappedPhaseMap));
            ValidateNotNull(unwrappedPhaseMap, nameof(unwrappedPhaseMap));
            ValidateWrappedPhaseMap(wrappedPhaseMap, nameof(wrappedPhaseMap));
            ValidateShadowMask(shadowMask, nameof(shadowMask));
            NativeException.ThrowIfError(NativeMethods.PhaseUnwrappingUnwrapPhaseMap(
                NativeHandle,
                wrappedPhaseMap.NativeHandle,
                unwrappedPhaseMap.NativeHandle,
                OptionalHandle(shadowMask)));
        }

        /// <summary>
        /// Unwraps a wrapped phase map and returns the result matrix.
        /// 展开包裹相位图并返回结果矩阵。
        /// </summary>
        public Mat UnwrapPhaseMap(Mat wrappedPhaseMap, Mat? shadowMask = null)
        {
            var result = new Mat();
            try
            {
                UnwrapPhaseMap(wrappedPhaseMap, result, shadowMask);
                return result;
            }
            catch
            {
                result.Dispose();
                throw;
            }
        }

        /// <summary>Releases native resources. 释放 native 资源。</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        internal static IntPtr OptionalHandle(Mat? mat)
        {
            return mat == null ? IntPtr.Zero : mat.NativeHandle;
        }

        internal static void ValidateNotNull<T>(T value, string parameterName)
            where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }

        private static void ValidateWrappedPhaseMap(Mat wrappedPhaseMap, string parameterName)
        {
            if (wrappedPhaseMap.Type != MatType.CV_32FC1)
            {
                throw new ArgumentException("Wrapped phase map must be CV_32FC1.", parameterName);
            }
        }

        private static void ValidateShadowMask(Mat? shadowMask, string parameterName)
        {
            if (shadowMask != null && !shadowMask.Empty && shadowMask.Type != MatType.CV_8UC1)
            {
                throw new ArgumentException("Shadow mask must be empty or CV_8UC1.", parameterName);
            }
        }

        /// <summary>Throws when this object has been disposed. 对象释放后抛出异常。</summary>
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
