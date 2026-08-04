using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.StructuredLight
{
    /// <summary>
    /// Base class for OpenCV structured-light pattern generators.
    /// OpenCV 结构光图案生成器基类。
    /// </summary>
    public abstract class StructuredLightPattern : IDisposable
    {
        private NativeStructuredLightPatternHandle handle;
        private bool disposed;

        internal StructuredLightPattern(NativeStructuredLightPatternHandle handle)
        {
            this.handle = handle;
        }

        /// <summary>Gets whether this pattern has been disposed. 获取图案对象是否已经释放。</summary>
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
        /// Generates structured-light pattern images.
        /// 生成结构光图案图像。
        /// </summary>
        public unsafe Mat[] Generate()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.StructuredLightPatternGenerateCount(NativeHandle, out int imageCount));
            if (imageCount <= 0)
            {
                return Array.Empty<Mat>();
            }

            var handles = new IntPtr[imageCount];
            int written = 0;
            try
            {
                fixed (IntPtr* handlesPtr = handles)
                {
                    NativeException.ThrowIfError(NativeMethods.StructuredLightPatternGenerateFill(
                        NativeHandle,
                        handlesPtr,
                        handles.Length,
                        out written));
                }

                int count = Math.Max(0, Math.Min(written, handles.Length));
                var result = new Mat[count];
                for (int i = 0; i < result.Length; i++)
                {
                    result[i] = new Mat(handles[i]);
                    handles[i] = IntPtr.Zero;
                }

                return result;
            }
            finally
            {
                ReleaseUnclaimedMatHandles(handles, written);
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

        internal static IntPtr[] ToNativeHandles(Mat[] mats, string parameterName)
        {
            ValidateNotNull(mats, parameterName);
            var handles = new IntPtr[mats.Length];
            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i] == null)
                {
                    throw new ArgumentNullException(parameterName);
                }

                handles[i] = mats[i].NativeHandle;
            }

            return handles;
        }

        internal static void ValidateNotNull<T>(T value, string parameterName)
            where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }

        internal static void ValidateNonNegativeThreshold(int value, string parameterName)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Threshold must be non-negative.");
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

        private static void ReleaseUnclaimedMatHandles(IntPtr[] handles, int written)
        {
            int count = Math.Max(0, Math.Min(written, handles.Length));
            for (int i = 0; i < count; i++)
            {
                if (handles[i] != IntPtr.Zero)
                {
                    NativeMethods.MatRelease(handles[i]);
                    handles[i] = IntPtr.Zero;
                }
            }
        }
    }
}
