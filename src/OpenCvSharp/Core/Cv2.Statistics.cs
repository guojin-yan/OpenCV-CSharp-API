using System;
using OpenCvSharp.Internal.Interop;

#if NETCOREAPP3_1_OR_GREATER
#endif

namespace OpenCvSharp.Core
{
    public static partial class Cv2
    {
        /// <summary>
        /// Counts non-zero elements in a single-channel array.
        /// 统计单通道数组中的非零元素数量。
        /// </summary>
        public static int CountNonZero(Mat src)
        {
            ValidateNotNull(src, nameof(src));
            ValidateCountNonZeroInput(src, nameof(src));
            NativeException.ThrowIfError(NativeMethods.CoreCountNonZero(src.NativeHandle, out int count));
            return count;
        }

        private static void ValidateCountNonZeroInput(Mat src, string parameterName)
        {
            if (src.Channels != 1)
            {
                throw new ArgumentException("CountNonZero requires a single-channel source matrix.", parameterName);
            }
        }

        /// <summary>
        /// Calculates the per-channel mean of an array.
        /// 计算数组的各通道均值。
        /// </summary>
        public static Scalar Mean(Mat src, Mat? mask = null)
        {
            ValidateNotNull(src, nameof(src));
            ValidateMeanInput(src, mask);
#if NETCOREAPP3_1_OR_GREATER
            Span<double> values = stackalloc double[4];
            unsafe
            {
                fixed (double* valuesPtr = values)
                {
                    NativeException.ThrowIfError(NativeMethods.CoreMeanPtr(src.NativeHandle, OptionalHandle(mask), valuesPtr, values.Length));
                }
            }

            return ToScalar(values);
#else
            double[] values = new double[4];
            NativeException.ThrowIfError(NativeMethods.CoreMean(src.NativeHandle, OptionalHandle(mask), values, values.Length));
            return ToScalar(values);
#endif
        }

        private static void ValidateMeanInput(Mat src, Mat? mask)
        {
            if (src.Channels > 4)
            {
                throw new ArgumentException("Mean supports source matrices with at most four channels.", nameof(src));
            }

            ValidateStatisticsMask(src, mask, "Mean");
        }

        /// <summary>
        /// Calculates the per-channel mean and standard deviation of an array.
        /// 计算数组的各通道均值和标准差。
        /// </summary>
        public static MeanStdDevResult MeanStdDev(Mat src, Mat? mask = null)
        {
            ValidateNotNull(src, nameof(src));
            ValidateMeanStdDevInput(src, mask);
#if NETCOREAPP3_1_OR_GREATER
            Span<double> mean = stackalloc double[4];
            Span<double> stdDev = stackalloc double[4];
            unsafe
            {
                fixed (double* meanPtr = mean)
                fixed (double* stdDevPtr = stdDev)
                {
                    NativeException.ThrowIfError(NativeMethods.CoreMeanStdDevPtr(
                        src.NativeHandle,
                        OptionalHandle(mask),
                        meanPtr,
                        mean.Length,
                        stdDevPtr,
                        stdDev.Length));
                }
            }

            return new MeanStdDevResult(ToScalar(mean), ToScalar(stdDev));
#else
            double[] mean = new double[4];
            double[] stdDev = new double[4];
            NativeException.ThrowIfError(NativeMethods.CoreMeanStdDev(src.NativeHandle, OptionalHandle(mask), mean, mean.Length, stdDev, stdDev.Length));
            return new MeanStdDevResult(ToScalar(mean), ToScalar(stdDev));
#endif
        }

        private static void ValidateMeanStdDevInput(Mat src, Mat? mask)
        {
            if (src.Empty)
            {
                throw new ArgumentException("MeanStdDev requires a non-empty source matrix.", nameof(src));
            }

            ValidateStatisticsMask(src, mask, "MeanStdDev");
        }

        private static void ValidateStatisticsMask(Mat src, Mat? mask, string operationName)
        {
            if (mask == null || mask.Empty)
            {
                return;
            }

            int maskType = mask.Type;
            if (maskType != MatType.CV_8UC1 &&
                maskType != MatType.CV_8SC1 &&
                maskType != MatType.CV_BoolC1)
            {
                throw new ArgumentException($"{operationName} mask must be empty, CV_8UC1, CV_8SC1, or CV_BoolC1.", nameof(mask));
            }

            if (!mask.Size.Equals(src.Size))
            {
                throw new ArgumentException($"{operationName} mask must have the same size as the source matrix.", nameof(mask));
            }
        }

        /// <summary>
        /// Finds the global minimum and maximum values and their locations.
        /// 查找全局最小值、最大值及其位置。
        /// </summary>
        public static MinMaxLocResult MinMaxLoc(Mat src, Mat? mask = null)
        {
            ValidateNotNull(src, nameof(src));
            ValidateMinMaxLocInput(src, mask);
            NativeException.ThrowIfError(NativeMethods.CoreMinMaxLoc(
                src.NativeHandle,
                OptionalHandle(mask),
                out double minVal,
                out double maxVal,
                out int minX,
                out int minY,
                out int maxX,
                out int maxY));
            return new MinMaxLocResult(minVal, maxVal, new Point(minX, minY), new Point(maxX, maxY));
        }

        private static void ValidateMinMaxLocInput(Mat src, Mat? mask)
        {
            if (src.Dims > 2)
            {
                throw new ArgumentException("MinMaxLoc source matrix must have at most two dimensions.", nameof(src));
            }

            if (src.Channels != 1)
            {
                throw new ArgumentException("MinMaxLoc requires a single-channel source matrix.", nameof(src));
            }

            ValidateStatisticsMask(src, mask, "MinMaxLoc");
        }

        /// <summary>
        /// Calculates a norm of an array.
        /// 计算数组的范数。
        /// </summary>
        public static double Norm(Mat src1, NormTypes normType = NormTypes.L2, Mat? mask = null)
        {
            ValidateNotNull(src1, nameof(src1));
            ValidateNormInput(src1, normType, mask);
            NativeException.ThrowIfError(NativeMethods.CoreNorm(src1.NativeHandle, (int)normType, OptionalHandle(mask), out double value));
            return value;
        }

        /// <summary>
        /// Calculates a norm of the difference between two arrays.
        /// 计算两个数组差值的范数。
        /// </summary>
        public static double Norm(Mat src1, Mat src2, NormTypes normType = NormTypes.L2, Mat? mask = null)
        {
            ValidateNotNull(src1, nameof(src1));
            ValidateNotNull(src2, nameof(src2));
            ValidateNormDiffInputs(src1, src2);
            ValidateNormInput(src1, normType, mask);
            NativeException.ThrowIfError(NativeMethods.CoreNormDiff(src1.NativeHandle, src2.NativeHandle, (int)normType, OptionalHandle(mask), out double value));
            return value;
        }

        private static void ValidateNormInput(Mat src1, NormTypes normType, Mat? mask)
        {
            ValidateNormType(src1, normType, nameof(normType));
            ValidateStatisticsMask(src1, mask, "Norm");
        }

        private static void ValidateNormType(Mat src1, NormTypes value, string parameterName)
        {
            int baseType = (int)value & 7;
            if (baseType == (int)NormTypes.Inf ||
                baseType == (int)NormTypes.L1 ||
                baseType == (int)NormTypes.L2 ||
                baseType == (int)NormTypes.L2Sqr)
            {
                return;
            }

            if (baseType == (int)NormTypes.Hamming ||
                baseType == (int)NormTypes.Hamming2)
            {
                if (src1.Type != MatType.CV_8UC1)
                {
                    throw new ArgumentException("Hamming norms require a CV_8UC1 source matrix.", nameof(src1));
                }

                return;
            }

            throw new ArgumentOutOfRangeException(parameterName, "Unsupported norm type.");
        }

        private static void ValidateNormDiffInputs(Mat src1, Mat src2)
        {
            if (src2.Type != src1.Type)
            {
                throw new ArgumentException("Norm input matrices must have the same type.", nameof(src2));
            }

            if (!src2.Size.Equals(src1.Size))
            {
                throw new ArgumentException("Norm input matrices must have the same size.", nameof(src2));
            }
        }

        /// <summary>
        /// Normalizes an array into a destination array.
        /// 将数组归一化到目标数组。
        /// </summary>
        public static void Normalize(Mat src, Mat dst, double alpha = 1.0, double beta = 0.0, NormTypes normType = NormTypes.L2, int dtype = -1, Mat? mask = null)
        {
            ValidateMatPair(src, dst);
            ValidateNormalizeType(normType, nameof(normType));
            NativeException.ThrowIfError(NativeMethods.CoreNormalize(src.NativeHandle, dst.NativeHandle, alpha, beta, (int)normType, dtype, OptionalHandle(mask)));
        }

        private static void ValidateNormalizeType(NormTypes value, string parameterName)
        {
            if (value != NormTypes.Inf &&
                value != NormTypes.L1 &&
                value != NormTypes.L2 &&
                value != NormTypes.MinMax)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Unsupported normalization mode.");
            }
        }

        /// <summary>
        /// Reduces a matrix to a vector by applying an operation along one dimension.
        /// 沿一个维度对矩阵应用归约操作并生成向量。
        /// </summary>
        public static void Reduce(Mat src, Mat dst, int dim, ReduceTypes rtype, int dtype = -1)
        {
            ValidateMatPair(src, dst);
            ValidateReduceInput(src, dim, dtype);
            ValidateReduceType(rtype, nameof(rtype));
            NativeException.ThrowIfError(NativeMethods.CoreReduce(src.NativeHandle, dst.NativeHandle, dim, (int)rtype, dtype));
        }

        private static void ValidateReduceInput(Mat src, int dim, int dtype)
        {
            if (src.Dims > 2)
            {
                throw new ArgumentException("Reduce source matrix must have at most two dimensions.", nameof(src));
            }

            if (dim != 0 && dim != 1)
            {
                throw new ArgumentOutOfRangeException(nameof(dim), "Reduce dimension must be 0 or 1.");
            }

            if (dtype >= 0 && MatType.Channels(dtype) != src.Channels)
            {
                throw new ArgumentException("Reduce destination type must preserve the source channel count.", nameof(dtype));
            }
        }

        private static void ValidateReduceType(ReduceTypes value, string parameterName)
        {
            if (value != ReduceTypes.Sum &&
                value != ReduceTypes.Avg &&
                value != ReduceTypes.Max &&
                value != ReduceTypes.Min)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Unsupported reduction operation.");
            }
        }

        /// <summary>
        /// Calculates the per-channel sum of array elements.
        /// 计算数组元素的各通道总和。
        /// </summary>
        public static Scalar Sum(Mat src)
        {
            ValidateNotNull(src, nameof(src));
            ValidateSumInput(src, nameof(src));
#if NETCOREAPP3_1_OR_GREATER
            Span<double> values = stackalloc double[4];
            unsafe
            {
                fixed (double* valuesPtr = values)
                {
                    NativeException.ThrowIfError(NativeMethods.CoreSumPtr(src.NativeHandle, valuesPtr, values.Length));
                }
            }

            return ToScalar(values);
#else
            double[] values = new double[4];
            NativeException.ThrowIfError(NativeMethods.CoreSum(src.NativeHandle, values, values.Length));
            return ToScalar(values);
#endif
        }

        private static void ValidateSumInput(Mat src, string parameterName)
        {
            if (src.Channels > 4)
            {
                throw new ArgumentException("Sum supports source matrices with at most four channels.", parameterName);
            }
        }

        /// <summary>
        /// Calculates the trace of a matrix.
        /// 计算矩阵的迹。
        /// </summary>
        public static Scalar Trace(Mat src)
        {
            ValidateNotNull(src, nameof(src));
            ValidateTraceInput(src, nameof(src));
#if NETCOREAPP3_1_OR_GREATER
            Span<double> values = stackalloc double[4];
            unsafe
            {
                fixed (double* valuesPtr = values)
                {
                    NativeException.ThrowIfError(NativeMethods.CoreTracePtr(src.NativeHandle, valuesPtr, values.Length));
                }
            }

            return ToScalar(values);
#else
            double[] values = new double[4];
            NativeException.ThrowIfError(NativeMethods.CoreTrace(src.NativeHandle, values, values.Length));
            return ToScalar(values);
#endif
        }

        private static void ValidateTraceInput(Mat src, string parameterName)
        {
            if (src.Dims > 2)
            {
                throw new ArgumentException("Trace requires a source matrix with two dimensions or less.", parameterName);
            }
        }

        /// <summary>
        /// Calculates the determinant of a square matrix.
        /// 计算方阵的行列式。
        /// </summary>
        public static double Determinant(Mat src)
        {
            ValidateNotNull(src, nameof(src));
            ValidateDeterminantInput(src, nameof(src));
            NativeException.ThrowIfError(NativeMethods.CoreDeterminant(src.NativeHandle, out double value));
            return value;
        }

        private static void ValidateDeterminantInput(Mat src, string parameterName)
        {
            if (src.Empty)
            {
                throw new ArgumentException("Determinant requires a non-empty source matrix.", parameterName);
            }

            if (src.Rows != src.Cols)
            {
                throw new ArgumentException("Determinant requires a square source matrix.", parameterName);
            }

            int type = src.Type;
            if (type != MatType.CV_32FC1 && type != MatType.CV_64FC1)
            {
                throw new ArgumentException("Determinant source matrix must be CV_32FC1 or CV_64FC1.", parameterName);
            }
        }

        /// <summary>
        /// Inverts a matrix and returns the OpenCV inversion quality value.
        /// 求矩阵逆，并返回 OpenCV 的求逆质量值。
        /// </summary>
        public static double Invert(Mat src, Mat dst, DecompTypes flags = DecompTypes.LU)
        {
            ValidateMatPair(src, dst);
            ValidateInvertDecompType(flags, nameof(flags));
            ValidateInvertInput(src, flags, nameof(src));
            NativeException.ThrowIfError(NativeMethods.CoreInvert(src.NativeHandle, dst.NativeHandle, (int)flags, out double value));
            return value;
        }

        /// <summary>
        /// Solves a linear system or least-squares problem.
        /// 求解线性系统或最小二乘问题。
        /// </summary>
        public static bool Solve(Mat src1, Mat src2, Mat dst, DecompTypes flags = DecompTypes.LU)
        {
            ValidateMatTriple(src1, src2, dst);
            ValidateSolveDecompType(flags, nameof(flags));
            ValidateSolveInput(src1, src2, flags);
            NativeException.ThrowIfError(NativeMethods.CoreSolve(src1.NativeHandle, src2.NativeHandle, dst.NativeHandle, (int)flags, out int success));
            return success != 0;
        }

        private static void ValidateInvertDecompType(DecompTypes value, string parameterName)
        {
            if (value != DecompTypes.LU &&
                value != DecompTypes.SVD &&
                value != DecompTypes.EIG &&
                value != DecompTypes.Cholesky)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Unsupported inversion decomposition method.");
            }
        }

        private static void ValidateInvertInput(Mat src, DecompTypes flags, string parameterName)
        {
            int type = src.Type;
            if (type != MatType.CV_32FC1 && type != MatType.CV_64FC1)
            {
                throw new ArgumentException("Invert source matrix must be CV_32FC1 or CV_64FC1.", parameterName);
            }

            if (flags != DecompTypes.SVD && src.Rows != src.Cols)
            {
                throw new ArgumentException("Invert requires a square source matrix unless SVD decomposition is used.", parameterName);
            }
        }

        private static void ValidateSolveDecompType(DecompTypes value, string parameterName)
        {
            DecompTypes method = value & ~DecompTypes.Normal;
            if (method != DecompTypes.LU &&
                method != DecompTypes.SVD &&
                method != DecompTypes.EIG &&
                method != DecompTypes.Cholesky &&
                method != DecompTypes.QR)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Unsupported linear solve decomposition method.");
            }
        }

        private static void ValidateSolveInput(Mat src1, Mat src2, DecompTypes flags)
        {
            int type = src1.Type;
            if (type != MatType.CV_32FC1 && type != MatType.CV_64FC1)
            {
                throw new ArgumentException("Solve coefficient matrix must be CV_32FC1 or CV_64FC1.", nameof(src1));
            }

            if (src2.Type != type)
            {
                throw new ArgumentException("Solve right-hand side matrix must have the same type as the coefficient matrix.", nameof(src2));
            }

            DecompTypes method = flags & ~DecompTypes.Normal;
            bool isNormal = (flags & DecompTypes.Normal) != 0;
            if ((method == DecompTypes.LU || method == DecompTypes.Cholesky) && !isNormal && src1.Rows != src1.Cols)
            {
                throw new ArgumentException("Solve requires a square coefficient matrix for LU or Cholesky unless normal equations are used.", nameof(src1));
            }

            if (src1.Rows < src1.Cols)
            {
                throw new ArgumentException("Solve cannot solve under-determined linear systems.", nameof(src1));
            }

            if (src1.Rows != src2.Rows)
            {
                throw new ArgumentException("Solve right-hand side matrix row count must match the coefficient matrix row count.", nameof(src2));
            }
        }

        /// <summary>
        /// Calculates the Mahalanobis distance between two vectors.
        /// 计算两个向量之间的 Mahalanobis 距离。
        /// </summary>
        public static double Mahalanobis(Mat v1, Mat v2, Mat icovar)
        {
            ValidateNotNull(v1, nameof(v1));
            ValidateNotNull(v2, nameof(v2));
            ValidateNotNull(icovar, nameof(icovar));
            ValidateMahalanobisInputs(v1, v2, icovar);
            NativeException.ThrowIfError(NativeMethods.CoreMahalanobis(v1.NativeHandle, v2.NativeHandle, icovar.NativeHandle, out double value));
            return value;
        }

        private static void ValidateMahalanobisInputs(Mat v1, Mat v2, Mat icovar)
        {
            if (v2.Type != v1.Type)
            {
                throw new ArgumentException("Mahalanobis input vectors must have the same type.", nameof(v2));
            }

            if (!v2.Size.Equals(v1.Size))
            {
                throw new ArgumentException("Mahalanobis input vectors must have the same size.", nameof(v2));
            }

            if (icovar.Type != v1.Type)
            {
                throw new ArgumentException("Mahalanobis inverse covariance matrix must have the same type as the input vectors.", nameof(icovar));
            }

            int length = checked(v1.Rows * v1.Cols * v1.Channels);
            if (icovar.Rows != length || icovar.Cols != length)
            {
                throw new ArgumentException("Mahalanobis inverse covariance matrix must be square with side length equal to the input vector element count.", nameof(icovar));
            }
        }
    }
}
