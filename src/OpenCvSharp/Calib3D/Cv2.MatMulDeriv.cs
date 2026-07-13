using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Calib3D
{
    public static partial class Cv2
    {
        /// <summary>
        /// Computes the Jacobian matrices of a matrix product with respect to both inputs.
        /// 计算矩阵乘积相对于两个输入矩阵的 Jacobian 矩阵。
        /// </summary>
        /// <param name="a">The M x L input matrix. M x L 输入矩阵。</param>
        /// <param name="b">The L x N input matrix. L x N 输入矩阵。</param>
        /// <param name="dABdA">
        /// The caller-owned (M*N) x (M*L) derivative of A*B with respect to A.
        /// 调用方持有的 (M*N) x (M*L) 矩阵，表示 A*B 对 A 的导数。
        /// </param>
        /// <param name="dABdB">
        /// The caller-owned (M*N) x (L*N) derivative of A*B with respect to B.
        /// 调用方持有的 (M*N) x (L*N) 矩阵，表示 A*B 对 B 的导数。
        /// </param>
        public static void MatMulDeriv(
            Mat a,
            Mat b,
            Mat dABdA,
            Mat dABdB)
        {
            ThrowIfNull(a, nameof(a));
            ThrowIfNull(b, nameof(b));
            ThrowIfNull(dABdA, nameof(dABdA));
            ThrowIfNull(dABdB, nameof(dABdB));

            ValidateMatMulDerivInputs(a, b);
            ValidateMatMulDerivOutputs(a, b, dABdA, dABdB);

            NativeException.ThrowIfError(NativeMethods.Calib3DMatMulDeriv(
                a.NativeHandle,
                b.NativeHandle,
                dABdA.NativeHandle,
                dABdB.NativeHandle));
        }

        /// <summary>
        /// Computes the Jacobian matrices of a matrix product and returns owned outputs.
        /// 计算矩阵乘积的 Jacobian 矩阵并返回拥有所有权的输出。
        /// </summary>
        /// <param name="a">The M x L input matrix. M x L 输入矩阵。</param>
        /// <param name="b">The L x N input matrix. L x N 输入矩阵。</param>
        /// <param name="dABdA">
        /// The owned (M*N) x (M*L) derivative of A*B with respect to A.
        /// 拥有所有权的 (M*N) x (M*L) 矩阵，表示 A*B 对 A 的导数。
        /// </param>
        /// <param name="dABdB">
        /// The owned (M*N) x (L*N) derivative of A*B with respect to B.
        /// 拥有所有权的 (M*N) x (L*N) 矩阵，表示 A*B 对 B 的导数。
        /// </param>
        public static void MatMulDeriv(
            Mat a,
            Mat b,
            out Mat dABdA,
            out Mat dABdB)
        {
            dABdA = new Mat();
            dABdB = new Mat();
            try
            {
                MatMulDeriv(a, b, dABdA, dABdB);
            }
            catch
            {
                dABdA.Dispose();
                dABdB.Dispose();
                throw;
            }
        }

        private static void ValidateMatMulDerivInputs(Mat a, Mat b)
        {
            if (a.Empty)
            {
                throw new ArgumentException("The first matrix cannot be empty.", nameof(a));
            }
            if (b.Empty)
            {
                throw new ArgumentException("The second matrix cannot be empty.", nameof(b));
            }
            if (a.Type != b.Type)
            {
                throw new ArgumentException(
                    "The input matrices must have exactly the same type.",
                    nameof(b));
            }
            if (a.Type != MatType.CV_32FC1 && a.Type != MatType.CV_64FC1)
            {
                throw new ArgumentException(
                    "MatMulDeriv requires single-channel CV_32F or CV_64F input matrices.",
                    nameof(a));
            }
            if (a.Cols != b.Rows)
            {
                throw new ArgumentException(
                    "The first matrix column count must equal the second matrix row count.",
                    nameof(b));
            }

            ValidateMatMulDerivDimensions(a, b);
        }

        private static void ValidateMatMulDerivDimensions(Mat a, Mat b)
        {
            try
            {
                checked
                {
                    _ = a.Rows * b.Cols;
                    _ = a.Rows * a.Cols;
                    _ = b.Rows * b.Cols;
                }
            }
            catch (OverflowException exception)
            {
                throw new ArgumentException(
                    "The derivative matrix dimensions exceed the supported range.",
                    nameof(a),
                    exception);
            }
        }

        private static void ValidateMatMulDerivOutputs(
            Mat a,
            Mat b,
            Mat dABdA,
            Mat dABdB)
        {
            IntPtr aHandle = a.NativeHandle;
            IntPtr bHandle = b.NativeHandle;
            IntPtr dABdAHandle = dABdA.NativeHandle;
            IntPtr dABdBHandle = dABdB.NativeHandle;

            if (MatMulDerivMatsAlias(dABdA, dABdAHandle, dABdB, dABdBHandle))
            {
                throw new ArgumentException(
                    "The derivative output matrices must not alias each other.",
                    nameof(dABdB));
            }
            if (MatMulDerivMatsAlias(dABdA, dABdAHandle, a, aHandle) ||
                MatMulDerivMatsAlias(dABdA, dABdAHandle, b, bHandle))
            {
                throw new ArgumentException(
                    "The derivative output with respect to A must not alias an input matrix.",
                    nameof(dABdA));
            }
            if (MatMulDerivMatsAlias(dABdB, dABdBHandle, a, aHandle) ||
                MatMulDerivMatsAlias(dABdB, dABdBHandle, b, bHandle))
            {
                throw new ArgumentException(
                    "The derivative output with respect to B must not alias an input matrix.",
                    nameof(dABdB));
            }
        }

        private static bool MatMulDerivMatsAlias(
            Mat first,
            IntPtr firstHandle,
            Mat second,
            IntPtr secondHandle)
        {
            return ReferenceEquals(first, second) || firstHandle == secondHandle;
        }
    }
}
