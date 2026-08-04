using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Reg
{
    /// <summary>
    /// Base class for OpenCV registration mappers.
    /// OpenCV registration mapper 的基类。
    /// </summary>
    public class RegMapper : IDisposable
    {
        private NativeRegMapperHandle handle;
        private bool disposed;

        internal RegMapper(NativeRegMapperHandle handle)
        {
            this.handle = handle;
        }

        /// <summary>Gets whether this mapper has been disposed. 获取对象是否已经释放。</summary>
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

        /// <summary>Calculates a map between two images. 计算两幅图像之间的 map。</summary>
        public RegMap Calculate(Mat img1, Mat img2, RegMap? init = null)
        {
            ThrowIfDisposed();
            ValidateNotNull(img1, nameof(img1));
            ValidateNotNull(img2, nameof(img2));
            NativeException.ThrowIfError(NativeMethods.RegMapperCalculate(
                NativeHandle,
                img1.NativeHandle,
                img2.NativeHandle,
                init == null ? IntPtr.Zero : init.NativeHandle,
                out IntPtr nativeHandle));
            return RegMap.FromNativePointer(nativeHandle);
        }

        /// <summary>Returns an identity map compatible with this mapper. 返回与此 mapper 兼容的单位 map。</summary>
        public RegMap GetMap()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.RegMapperGetMap(NativeHandle, out IntPtr nativeHandle));
            return RegMap.FromNativePointer(nativeHandle);
        }

        /// <summary>Releases native resources. 释放 native 资源。</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        internal static NativeRegMapperHandle Create(Func<IntPtr> factory)
        {
            return NativeRegMapperHandle.FromNativePointer(factory());
        }

        internal static void ValidateNotNull<T>(T value, string parameterName)
            where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }

        internal static void ValidatePositive(int value, string parameterName)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Value must be positive.");
            }
        }

        internal void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        private void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing && handle != null)
            {
                handle.Dispose();
            }

            disposed = true;
        }
    }
}
