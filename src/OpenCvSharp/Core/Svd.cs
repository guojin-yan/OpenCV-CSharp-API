using System;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Core
{
    /// <summary>
    /// Represents OpenCV singular value decomposition compatible with <c>cv::SVD</c>.
    /// 表示与 OpenCV <c>cv::SVD</c> 兼容的奇异值分解对象。
    /// </summary>
    public sealed class Svd : IDisposable
    {
        private delegate int MatrixGetter(IntPtr svd, out IntPtr dst);

        private NativeSvdHandle handle;
        private bool disposed;

        /// <summary>
        /// Initializes an empty SVD object.
        /// 初始化一个空的 SVD 对象。
        /// </summary>
        public Svd()
        {
            NativeException.ThrowIfError(NativeMethods.CoreSvdCreateEmpty(out IntPtr nativeHandle));
            handle = NativeSvdHandle.FromNativePointer(nativeHandle);
        }

        /// <summary>
        /// Initializes an SVD object and decomposes the source matrix.
        /// 初始化 SVD 对象并分解源矩阵。
        /// </summary>
        /// <param name="src">The source matrix to decompose. 要分解的源矩阵。</param>
        /// <param name="flags">The SVD flags. SVD 标志。</param>
        public Svd(Mat src, SvdFlags flags = SvdFlags.None)
        {
            ValidateNotNull(src, nameof(src));
            ValidateSvdFlags(flags, nameof(flags));
            NativeException.ThrowIfError(NativeMethods.CoreSvdCreate(src.NativeHandle, (int)flags, out IntPtr nativeHandle));
            handle = NativeSvdHandle.FromNativePointer(nativeHandle);
        }

        /// <summary>
        /// Gets a value indicating whether this object has been disposed.
        /// 获取此对象是否已经释放。
        /// </summary>
        public bool IsDisposed
        {
            get { return disposed; }
        }

        /// <summary>
        /// Gets a clone of the singular values matrix.
        /// 获取奇异值矩阵的克隆。
        /// </summary>
        public Mat W
        {
            get { return GetMatrix(NativeMethods.CoreSvdGetW); }
        }

        /// <summary>
        /// Gets a clone of the left singular vectors matrix.
        /// 获取左奇异向量矩阵的克隆。
        /// </summary>
        public Mat U
        {
            get { return GetMatrix(NativeMethods.CoreSvdGetU); }
        }

        /// <summary>
        /// Gets a clone of the transposed right singular vectors matrix.
        /// 获取转置右奇异向量矩阵的克隆。
        /// </summary>
        public Mat Vt
        {
            get { return GetMatrix(NativeMethods.CoreSvdGetVt); }
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
        /// Computes singular value decomposition for the source matrix.
        /// 计算源矩阵的奇异值分解。
        /// </summary>
        /// <param name="src">The source matrix. 源矩阵。</param>
        /// <param name="flags">The SVD flags. SVD 标志。</param>
        public void Compute(Mat src, SvdFlags flags = SvdFlags.None)
        {
            ThrowIfDisposed();
            ValidateNotNull(src, nameof(src));
            ValidateSvdFlags(flags, nameof(flags));
            NativeException.ThrowIfError(NativeMethods.CoreSvdCompute(NativeHandle, src.NativeHandle, (int)flags));
        }

        /// <summary>
        /// Performs back substitution using this decomposition.
        /// 使用当前分解结果执行反代。
        /// </summary>
        /// <param name="rhs">The right-hand side matrix. 右端矩阵。</param>
        /// <param name="dst">The destination solution matrix. 目标解矩阵。</param>
        public void BackSubst(Mat rhs, Mat dst)
        {
            ThrowIfDisposed();
            ValidateNotNull(rhs, nameof(rhs));
            ValidateNotNull(dst, nameof(dst));
            NativeException.ThrowIfError(NativeMethods.CoreSvdBackSubst(NativeHandle, rhs.NativeHandle, dst.NativeHandle));
        }

        /// <summary>
        /// Performs back substitution and returns a new solution matrix.
        /// 执行反代并返回新的解矩阵。
        /// </summary>
        /// <param name="rhs">The right-hand side matrix. 右端矩阵。</param>
        /// <returns>The solution matrix. 解矩阵。</returns>
        public Mat BackSubst(Mat rhs)
        {
            var dst = new Mat();
            try
            {
                BackSubst(rhs, dst);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Computes full SVD into caller-provided matrices.
        /// 将完整 SVD 计算到调用方提供的矩阵中。
        /// </summary>
        /// <param name="src">The source matrix. 源矩阵。</param>
        /// <param name="w">The singular values matrix. 奇异值矩阵。</param>
        /// <param name="u">The left singular vectors matrix. 左奇异向量矩阵。</param>
        /// <param name="vt">The transposed right singular vectors matrix. 转置右奇异向量矩阵。</param>
        /// <param name="flags">The SVD flags. SVD 标志。</param>
        public static void Compute(Mat src, Mat w, Mat u, Mat vt, SvdFlags flags = SvdFlags.None)
        {
            ValidateNotNull(src, nameof(src));
            ValidateNotNull(w, nameof(w));
            ValidateNotNull(u, nameof(u));
            ValidateNotNull(vt, nameof(vt));
            ValidateSvdFlags(flags, nameof(flags));
            NativeException.ThrowIfError(NativeMethods.CoreSvdStaticCompute(src.NativeHandle, w.NativeHandle, u.NativeHandle, vt.NativeHandle, (int)flags));
        }

        /// <summary>
        /// Computes singular values into a caller-provided matrix.
        /// 将奇异值计算到调用方提供的矩阵中。
        /// </summary>
        /// <param name="src">The source matrix. 源矩阵。</param>
        /// <param name="w">The singular values matrix. 奇异值矩阵。</param>
        /// <param name="flags">The SVD flags. SVD 标志。</param>
        public static void Compute(Mat src, Mat w, SvdFlags flags = SvdFlags.None)
        {
            ValidateNotNull(src, nameof(src));
            ValidateNotNull(w, nameof(w));
            ValidateSvdFlags(flags, nameof(flags));
            NativeException.ThrowIfError(NativeMethods.CoreSvdStaticComputeValues(src.NativeHandle, w.NativeHandle, (int)flags));
        }

        private static void ValidateSvdFlags(SvdFlags value, string parameterName)
        {
            const SvdFlags allowed = SvdFlags.ModifyA | SvdFlags.NoUv | SvdFlags.FullUv;
            if ((value & ~allowed) != 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Unsupported singular value decomposition flags.");
            }
        }

        /// <summary>
        /// Computes singular values and returns a new matrix.
        /// 计算奇异值并返回新矩阵。
        /// </summary>
        /// <param name="src">The source matrix. 源矩阵。</param>
        /// <param name="flags">The SVD flags. SVD 标志。</param>
        /// <returns>The singular values matrix. 奇异值矩阵。</returns>
        public static Mat ComputeValues(Mat src, SvdFlags flags = SvdFlags.None)
        {
            var w = new Mat();
            try
            {
                Compute(src, w, flags);
                return w;
            }
            catch
            {
                w.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Performs static SVD back substitution.
        /// 执行静态 SVD 反代。
        /// </summary>
        /// <param name="w">The singular values matrix. 奇异值矩阵。</param>
        /// <param name="u">The left singular vectors matrix. 左奇异向量矩阵。</param>
        /// <param name="vt">The transposed right singular vectors matrix. 转置右奇异向量矩阵。</param>
        /// <param name="rhs">The right-hand side matrix. 右端矩阵。</param>
        /// <param name="dst">The destination solution matrix. 目标解矩阵。</param>
        public static void BackSubst(Mat w, Mat u, Mat vt, Mat rhs, Mat dst)
        {
            ValidateNotNull(w, nameof(w));
            ValidateNotNull(u, nameof(u));
            ValidateNotNull(vt, nameof(vt));
            ValidateNotNull(rhs, nameof(rhs));
            ValidateNotNull(dst, nameof(dst));
            NativeException.ThrowIfError(NativeMethods.CoreSvdStaticBackSubst(w.NativeHandle, u.NativeHandle, vt.NativeHandle, rhs.NativeHandle, dst.NativeHandle));
        }

        /// <summary>
        /// Solves an under-determined singular linear system.
        /// 求解欠定奇异线性系统。
        /// </summary>
        /// <param name="src">The left-hand-side matrix. 左端矩阵。</param>
        /// <param name="dst">The destination solution matrix. 目标解矩阵。</param>
        public static void SolveZ(Mat src, Mat dst)
        {
            ValidateNotNull(src, nameof(src));
            ValidateNotNull(dst, nameof(dst));
            NativeException.ThrowIfError(NativeMethods.CoreSvdSolveZ(src.NativeHandle, dst.NativeHandle));
        }

        /// <summary>
        /// Solves an under-determined singular linear system and returns a new matrix.
        /// 求解欠定奇异线性系统并返回新矩阵。
        /// </summary>
        /// <param name="src">The left-hand-side matrix. 左端矩阵。</param>
        /// <returns>The solution matrix. 解矩阵。</returns>
        public static Mat SolveZ(Mat src)
        {
            var dst = new Mat();
            try
            {
                SolveZ(src, dst);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Releases the native SVD object.
        /// 释放 native SVD 对象。
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return disposed ? "{Disposed=True}" : "{Disposed=False}";
        }

        private Mat GetMatrix(MatrixGetter getter)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(getter(NativeHandle, out IntPtr nativeHandle));
            return new Mat(nativeHandle);
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

        private static void ValidateNotNull<T>(T value, string parameterName)
            where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
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
