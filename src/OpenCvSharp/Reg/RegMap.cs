using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Reg
{
    /// <summary>
    /// Base class for OpenCV registration coordinate maps.
    /// OpenCV registration 坐标变换 map 的基类。
    /// </summary>
    public class RegMap : IDisposable
    {
        private NativeRegMapHandle handle;
        private bool disposed;

        internal RegMap(NativeRegMapHandle handle)
        {
            this.handle = handle;
        }

        /// <summary>Gets whether this map has been disposed. 获取对象是否已经释放。</summary>
        public bool IsDisposed
        {
            get { return disposed; }
        }

        /// <summary>Gets the concrete map kind. 获取具体 map 类型。</summary>
        public RegMapKind Kind
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.RegMapGetKind(NativeHandle, out int kind));
                return (RegMapKind)kind;
            }
        }

        internal IntPtr NativeHandle
        {
            get
            {
                ThrowIfDisposed();
                return handle.DangerousGetHandle();
            }
        }

        /// <summary>Warps <paramref name="src"/> into <paramref name="dst"/>. 将 <paramref name="src"/> 变换到 <paramref name="dst"/>。</summary>
        public void Warp(Mat src, Mat dst)
        {
            ThrowIfDisposed();
            ValidateNotNull(src, nameof(src));
            ValidateNotNull(dst, nameof(dst));
            NativeException.ThrowIfError(NativeMethods.RegMapWarp(NativeHandle, src.NativeHandle, dst.NativeHandle));
        }

        /// <summary>Warps and returns a new matrix. 变换并返回新矩阵。</summary>
        public Mat Warp(Mat src)
        {
            var dst = new Mat();
            try
            {
                Warp(src, dst);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        /// <summary>Applies inverse warping into <paramref name="dst"/>. 执行 inverse warp 并写入 <paramref name="dst"/>。</summary>
        public void InverseWarp(Mat src, Mat dst)
        {
            ThrowIfDisposed();
            ValidateNotNull(src, nameof(src));
            ValidateNotNull(dst, nameof(dst));
            NativeException.ThrowIfError(NativeMethods.RegMapInverseWarp(NativeHandle, src.NativeHandle, dst.NativeHandle));
        }

        /// <summary>Applies inverse warping and returns a new matrix. 执行 inverse warp 并返回新矩阵。</summary>
        public Mat InverseWarp(Mat src)
        {
            var dst = new Mat();
            try
            {
                InverseWarp(src, dst);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        /// <summary>Creates the inverse map. 创建逆 map。</summary>
        public RegMap InverseMap()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.RegMapInverseMap(NativeHandle, out IntPtr nativeHandle));
            return FromNativePointer(nativeHandle);
        }

        /// <summary>Composes this map with another map of the same concrete kind. 将此 map 与同类型 map 组合。</summary>
        public void Compose(RegMap other)
        {
            ThrowIfDisposed();
            ValidateNotNull(other, nameof(other));
            NativeException.ThrowIfError(NativeMethods.RegMapCompose(NativeHandle, other.NativeHandle));
        }

        /// <summary>Scales the map coordinate system. 缩放 map 坐标系。</summary>
        public void Scale(double factor)
        {
            ThrowIfDisposed();
            ValidateFinite(factor, nameof(factor));
            NativeException.ThrowIfError(NativeMethods.RegMapScale(NativeHandle, factor));
        }

        /// <summary>Releases native resources. 释放 native 资源。</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        internal static RegMap FromNativePointer(IntPtr nativeHandle)
        {
            NativeRegMapHandle safeHandle = NativeRegMapHandle.FromNativePointer(nativeHandle);
            try
            {
                NativeException.ThrowIfError(NativeMethods.RegMapGetKind(nativeHandle, out int kind));
                switch ((RegMapKind)kind)
                {
                    case RegMapKind.Shift:
                        return new MapShift(safeHandle);
                    case RegMapKind.Affine:
                        return new MapAffine(safeHandle);
                    case RegMapKind.Projec:
                        return new MapProjec(safeHandle);
                    default:
                        return new RegMap(safeHandle);
                }
            }
            catch
            {
                safeHandle.Dispose();
                throw;
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

        internal static void ValidateFinite(double value, string parameterName)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(parameterName, "Value must be finite.");
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
