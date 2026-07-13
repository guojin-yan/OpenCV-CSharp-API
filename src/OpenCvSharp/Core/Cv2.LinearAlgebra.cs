using System;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Core
{
    public static partial class Cv2
    {
        /// <summary>
        /// Performs generalized matrix multiplication.
        /// 执行广义矩阵乘法。
        /// </summary>
        public static void Gemm(Mat src1, Mat src2, double alpha, Mat? src3, double beta, Mat dst, GemmFlags flags = GemmFlags.None)
        {
            ValidateNotNull(src1, nameof(src1));
            ValidateNotNull(src2, nameof(src2));
            ValidateNotNull(dst, nameof(dst));
            ValidateGemmFlags(flags, nameof(flags));
            ValidateGemmInput(src1, src2, src3, beta, flags);
            NativeException.ThrowIfError(NativeMethods.CoreGemm(src1.NativeHandle, src2.NativeHandle, alpha, OptionalHandle(src3), beta, dst.NativeHandle, (int)flags));
        }

        /// <summary>
        /// Performs generalized matrix multiplication and returns a new matrix.
        /// 执行广义矩阵乘法并返回新矩阵。
        /// </summary>
        public static Mat Gemm(Mat src1, Mat src2, double alpha = 1.0, Mat? src3 = null, double beta = 0.0, GemmFlags flags = GemmFlags.None)
        {
            var dst = new Mat();
            try
            {
                Gemm(src1, src2, alpha, src3, beta, dst, flags);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        private static void ValidateGemmFlags(GemmFlags value, string parameterName)
        {
            const GemmFlags allowed = GemmFlags.TransposeSrc1 | GemmFlags.TransposeSrc2 | GemmFlags.TransposeSrc3;
            if ((value & ~allowed) != 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Unsupported generalized matrix multiplication flags.");
            }
        }

        private static void ValidateGemmInput(Mat src1, Mat src2, Mat? src3, double beta, GemmFlags flags)
        {
            int type = src1.Type;
            if (!IsSupportedGemmType(type))
            {
                throw new ArgumentException("Gemm source matrices must be CV_32FC1, CV_64FC1, CV_32FC2, or CV_64FC2.", nameof(src1));
            }

            if (src2.Type != type)
            {
                throw new ArgumentException("Gemm source matrices must have the same type.", nameof(src2));
            }

            int dstRows;
            int dstCols;
            switch (flags & (GemmFlags.TransposeSrc1 | GemmFlags.TransposeSrc2))
            {
                case GemmFlags.None:
                    if (src1.Cols != src2.Rows)
                    {
                        throw new ArgumentException("Gemm source dimensions are not compatible for multiplication.", nameof(src2));
                    }

                    dstRows = src1.Rows;
                    dstCols = src2.Cols;
                    break;
                case GemmFlags.TransposeSrc1:
                    if (src1.Rows != src2.Rows)
                    {
                        throw new ArgumentException("Gemm source dimensions are not compatible for multiplication.", nameof(src2));
                    }

                    dstRows = src1.Cols;
                    dstCols = src2.Cols;
                    break;
                case GemmFlags.TransposeSrc2:
                    if (src1.Cols != src2.Cols)
                    {
                        throw new ArgumentException("Gemm source dimensions are not compatible for multiplication.", nameof(src2));
                    }

                    dstRows = src1.Rows;
                    dstCols = src2.Rows;
                    break;
                default:
                    if (src1.Rows != src2.Cols)
                    {
                        throw new ArgumentException("Gemm source dimensions are not compatible for multiplication.", nameof(src2));
                    }

                    dstRows = src1.Cols;
                    dstCols = src2.Rows;
                    break;
            }

            if (beta == 0.0 || src3 == null || src3.Empty)
            {
                return;
            }

            if (src3.Type != type)
            {
                throw new ArgumentException("Gemm addend matrix must have the same type as the source matrices.", nameof(src3));
            }

            bool transposeSrc3 = (flags & GemmFlags.TransposeSrc3) != 0;
            int expectedRows = transposeSrc3 ? dstCols : dstRows;
            int expectedCols = transposeSrc3 ? dstRows : dstCols;
            if (src3.Rows != expectedRows || src3.Cols != expectedCols)
            {
                throw new ArgumentException("Gemm addend matrix dimensions must match the output dimensions.", nameof(src3));
            }
        }

        private static bool IsSupportedGemmType(int type)
        {
            return type == MatType.CV_32FC1 ||
                type == MatType.CV_64FC1 ||
                type == MatType.CV_32FC2 ||
                type == MatType.CV_64FC2;
        }

        /// <summary>
        /// Multiplies a matrix by its transpose.
        /// 计算矩阵与其转置的乘积。
        /// </summary>
        public static void MulTransposed(Mat src, Mat dst, bool aTa, Mat? delta = null, double scale = 1.0, int dtype = -1)
        {
            ValidateMatPair(src, dst);
            ValidateMulTransposedInput(src, delta);
            NativeException.ThrowIfError(NativeMethods.CoreMulTransposed(src.NativeHandle, dst.NativeHandle, aTa ? 1 : 0, OptionalHandle(delta), scale, dtype));
        }

        /// <summary>
        /// Multiplies a matrix by its transpose and returns a new matrix.
        /// 计算矩阵与其转置的乘积并返回新矩阵。
        /// </summary>
        public static Mat MulTransposed(Mat src, bool aTa, Mat? delta = null, double scale = 1.0, int dtype = -1)
        {
            var dst = new Mat();
            try
            {
                MulTransposed(src, dst, aTa, delta, scale, dtype);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        private static void ValidateMulTransposedInput(Mat src, Mat? delta)
        {
            if (src.Channels != 1)
            {
                throw new ArgumentException("MulTransposed requires a single-channel source matrix.", nameof(src));
            }

            if (delta == null || delta.Empty)
            {
                return;
            }

            if (delta.Channels != 1)
            {
                throw new ArgumentException("MulTransposed delta matrix must be single-channel.", nameof(delta));
            }

            if (delta.Rows != src.Rows && delta.Rows != 1)
            {
                throw new ArgumentException("MulTransposed delta matrix row count must match the source rows or be one.", nameof(delta));
            }

            if (delta.Cols != src.Cols && delta.Cols != 1)
            {
                throw new ArgumentException("MulTransposed delta matrix column count must match the source columns or be one.", nameof(delta));
            }
        }

        /// <summary>
        /// Calculates eigenvalues and eigenvectors of a symmetric matrix.
        /// 计算对称矩阵的特征值和特征向量。
        /// </summary>
        public static bool Eigen(Mat src, Mat eigenvalues, Mat eigenvectors)
        {
            ValidateNotNull(src, nameof(src));
            ValidateNotNull(eigenvalues, nameof(eigenvalues));
            ValidateNotNull(eigenvectors, nameof(eigenvectors));
            ValidateEigenInput(src, nameof(src));
            NativeException.ThrowIfError(NativeMethods.CoreEigen(src.NativeHandle, eigenvalues.NativeHandle, eigenvectors.NativeHandle, out int success));
            return success != 0;
        }

        /// <summary>
        /// Calculates eigenvalues and eigenvectors of a non-symmetric matrix.
        /// 计算非对称矩阵的特征值和特征向量。
        /// </summary>
        public static void EigenNonSymmetric(Mat src, Mat eigenvalues, Mat eigenvectors)
        {
            ValidateNotNull(src, nameof(src));
            ValidateNotNull(eigenvalues, nameof(eigenvalues));
            ValidateNotNull(eigenvectors, nameof(eigenvectors));
            ValidateEigenInput(src, nameof(src));
            NativeException.ThrowIfError(NativeMethods.CoreEigenNonSymmetric(src.NativeHandle, eigenvalues.NativeHandle, eigenvectors.NativeHandle));
        }

        private static void ValidateEigenInput(Mat src, string parameterName)
        {
            if (src.Rows != src.Cols)
            {
                throw new ArgumentException("Eigen source matrix must be square.", parameterName);
            }

            if (src.Type != MatType.CV_32FC1 && src.Type != MatType.CV_64FC1)
            {
                throw new ArgumentException("Eigen source matrix must be CV_32FC1 or CV_64FC1.", parameterName);
            }
        }

        /// <summary>
        /// Solves a cubic equation and writes real roots to the output matrix.
        /// 求解三次方程，并将实根写入输出矩阵。
        /// </summary>
        public static int SolveCubic(Mat coeffs, Mat roots)
        {
            ValidateMatPair(coeffs, roots);
            ValidateSolveCubicInput(coeffs, nameof(coeffs));
            NativeException.ThrowIfError(NativeMethods.CoreSolveCubic(coeffs.NativeHandle, roots.NativeHandle, out int rootCount));
            return rootCount;
        }

        private static void ValidateSolveCubicInput(Mat coeffs, string parameterName)
        {
            if (coeffs.Type != MatType.CV_32FC1 && coeffs.Type != MatType.CV_64FC1)
            {
                throw new ArgumentException("SolveCubic coefficients must be CV_32FC1 or CV_64FC1.", parameterName);
            }

            bool isValidVector =
                (coeffs.Rows == 1 && (coeffs.Cols == 3 || coeffs.Cols == 4)) ||
                (coeffs.Cols == 1 && (coeffs.Rows == 3 || coeffs.Rows == 4));
            if (!isValidVector)
            {
                throw new ArgumentException("SolveCubic coefficients must be a 3- or 4-element row or column vector.", parameterName);
            }
        }

        /// <summary>
        /// Solves a polynomial equation and writes roots to the output matrix.
        /// 求解多项式方程，并将根写入输出矩阵。
        /// </summary>
        public static double SolvePoly(Mat coeffs, Mat roots, int maxIters = 300)
        {
            ValidateMatPair(coeffs, roots);
            ValidateSolvePolyInput(coeffs, nameof(coeffs));
            NativeException.ThrowIfError(NativeMethods.CoreSolvePoly(coeffs.NativeHandle, roots.NativeHandle, maxIters, out double error));
            return error;
        }

        private static void ValidateSolvePolyInput(Mat coeffs, string parameterName)
        {
            int depth = MatType.Depth(coeffs.Type);
            if (depth < MatType.CV_32F)
            {
                throw new ArgumentException("SolvePoly coefficients must have CV_32F or deeper floating-point depth.", parameterName);
            }

            if (coeffs.Channels > 2)
            {
                throw new ArgumentException("SolvePoly coefficients must have one or two channels.", parameterName);
            }

            if (coeffs.Rows != 1 && coeffs.Cols != 1)
            {
                throw new ArgumentException("SolvePoly coefficients must be a row or column vector.", parameterName);
            }
        }
    }
}
