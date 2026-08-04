using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Photo
{
    /// <summary>
    /// Base class for tonemapping operators.
    /// tone mapping 算子的基类。
    /// </summary>
    public class Tonemap : IDisposable
    {
        private NativeTonemapHandle handle;
        private bool disposed;

        internal Tonemap(NativeTonemapHandle handle)
        {
            this.handle = handle;
        }

        /// <summary>Gets whether this tonemap has been disposed. 获取对象是否已经释放。</summary>
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

        /// <summary>Gets or sets gamma correction value. 获取或设置 gamma 校正值。</summary>
        public float Gamma
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.TonemapGetGamma(NativeHandle, out float gamma));
                return gamma;
            }

            set
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.TonemapSetGamma(NativeHandle, value));
            }
        }

        /// <summary>Creates a linear tonemap operator. 创建线性 tone mapping 算子。</summary>
        public static Tonemap Create(float gamma = 1.0F)
        {
            NativeException.ThrowIfError(NativeMethods.TonemapCreate(gamma, out IntPtr nativeHandle));
            return new Tonemap(NativeTonemapHandle.FromNativePointer(nativeHandle));
        }

        /// <summary>Applies tonemapping. 执行 tone mapping。</summary>
        public void Process(Mat src, Mat dst)
        {
            ThrowIfDisposed();
            ValidateNotNull(src, nameof(src));
            ValidateNotNull(dst, nameof(dst));
            NativeException.ThrowIfError(NativeMethods.TonemapProcess(NativeHandle, src.NativeHandle, dst.NativeHandle));
        }

        /// <summary>Applies tonemapping and returns a new matrix. 执行 tone mapping 并返回新矩阵。</summary>
        public Mat Process(Mat src)
        {
            var dst = new Mat();
            try
            {
                Process(src, dst);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        /// <summary>Releases the native tonemap. 释放 native tone mapping 对象。</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>Releases managed and native resources. 释放托管和 native 资源。</summary>
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

        /// <summary>Throws when this object has been disposed. 对象已释放时抛出异常。</summary>
        protected void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        private static void ValidateNotNull<T>(T value, string parameterName)
            where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }
    }
}
