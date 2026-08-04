using System;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Core
{
    public static partial class Cv2
    {
        /// <summary>Computes an accurate single-precision cube root. 计算高精度单精度立方根。</summary>
        public static float CubeRoot(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(nameof(value), "CubeRoot requires a finite value.");
            }

            NativeException.ThrowIfError(NativeMethods.CoreCubeRoot(value, out float result));
            return result;
        }

        /// <summary>Calculates a full-range vector angle in degrees. 计算以度为单位的全范围向量角度。</summary>
        public static float FastAtan2(float y, float x)
        {
            if (float.IsNaN(y) || float.IsInfinity(y))
            {
                throw new ArgumentOutOfRangeException(nameof(y), "FastAtan2 requires finite coordinates.");
            }

            if (float.IsNaN(x) || float.IsInfinity(x))
            {
                throw new ArgumentOutOfRangeException(nameof(x), "FastAtan2 requires finite coordinates.");
            }

            NativeException.ThrowIfError(NativeMethods.CoreFastAtan2(y, x, out float degrees));
            return degrees;
        }

        /// <summary>Computes pairwise or K-nearest distances between row vectors. 计算行向量之间的成对或 K 近邻距离。</summary>
        public static void BatchDistance(
            Mat src1,
            Mat src2,
            Mat distances,
            int dtype = -1,
            Mat? indices = null,
            NormTypes normType = NormTypes.L2,
            int k = 0,
            Mat? mask = null,
            int update = 0,
            bool crosscheck = false)
        {
            ValidateNotNull(src1, nameof(src1));
            ValidateNotNull(src2, nameof(src2));
            ValidateNotNull(distances, nameof(distances));
            ValidateBatchDistanceInput(src1, src2, distances, dtype, indices, normType, k, mask, update, crosscheck);
            NativeException.ThrowIfError(NativeMethods.CoreBatchDistance(
                src1.NativeHandle,
                src2.NativeHandle,
                distances.NativeHandle,
                dtype,
                OptionalHandle(indices),
                (int)normType,
                k,
                OptionalHandle(mask),
                update,
                crosscheck ? 1 : 0));
        }

        private static void ValidateBatchDistanceInput(
            Mat src1,
            Mat src2,
            Mat distances,
            int dtype,
            Mat? indices,
            NormTypes normType,
            int k,
            Mat? mask,
            int update,
            bool crosscheck)
        {
            if (src1.Empty || src2.Empty)
            {
                throw new ArgumentException("BatchDistance sources must be non-empty.", nameof(src1));
            }

            if (src1.Type != src2.Type || src1.Cols != src2.Cols)
            {
                throw new ArgumentException("BatchDistance sources must have the same type and column count.", nameof(src2));
            }

            bool isByte = src1.Type == MatType.CV_8UC1;
            bool isFloat = src1.Type == MatType.CV_32FC1;
            if (!isByte && !isFloat)
            {
                throw new ArgumentException("BatchDistance sources must be CV_8UC1 or CV_32FC1.", nameof(src1));
            }

            if (dtype != -1 && dtype != MatType.CV_32S && dtype != MatType.CV_32F)
            {
                throw new ArgumentOutOfRangeException(nameof(dtype), "BatchDistance output depth must be -1, CV_32S, or CV_32F.");
            }

            bool hamming = normType == NormTypes.Hamming || normType == NormTypes.Hamming2;
            bool numeric = normType == NormTypes.L1 || normType == NormTypes.L2 || normType == NormTypes.L2Sqr;
            if ((!hamming && !numeric) || (hamming && !isByte))
            {
                throw new ArgumentOutOfRangeException(nameof(normType), "Unsupported BatchDistance norm for the source type.");
            }

            int effectiveDtype = dtype == -1 ? (hamming ? MatType.CV_32S : MatType.CV_32F) : dtype;
            if ((isFloat && effectiveDtype != MatType.CV_32F) ||
                (hamming && effectiveDtype != MatType.CV_32S) ||
                (normType == NormTypes.L2 && effectiveDtype != MatType.CV_32F))
            {
                throw new ArgumentException("The source type, output depth, and norm combination is not supported.", nameof(dtype));
            }

            if (k < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(k));
            }

            if ((k > 0) != (indices != null))
            {
                throw new ArgumentException("Indices must be supplied exactly when k is positive.", nameof(indices));
            }

            if (update < 0 || (update > 0 && k == 0))
            {
                throw new ArgumentOutOfRangeException(nameof(update));
            }

            if (mask != null && !mask.Empty &&
                (mask.Type != MatType.CV_8UC1 || mask.Rows != src1.Rows || mask.Cols != src2.Rows))
            {
                throw new ArgumentException("BatchDistance mask must be CV_8UC1 with src1.Rows by src2.Rows shape.", nameof(mask));
            }

            if (crosscheck && (k != 1 || update != 0 || (mask != null && !mask.Empty)))
            {
                throw new ArgumentException("Cross-check requires k=1, update=0, and no mask.", nameof(crosscheck));
            }

            if (ReferenceEquals(distances, src1) || ReferenceEquals(distances, src2) ||
                ReferenceEquals(indices, src1) || ReferenceEquals(indices, src2) ||
                ReferenceEquals(distances, indices))
            {
                throw new ArgumentException("BatchDistance outputs cannot alias inputs or each other.", nameof(distances));
            }
        }

        /// <summary>Calculates a covariance matrix from row or column samples. 由行或列样本计算协方差矩阵。</summary>
        public static void CalcCovarMatrix(Mat samples, Mat covar, Mat mean, CovarFlags flags, int ctype = MatType.CV_64F)
        {
            ValidateNotNull(samples, nameof(samples));
            ValidateNotNull(covar, nameof(covar));
            ValidateNotNull(mean, nameof(mean));
            ValidateCovarInput(samples, mean, flags, ctype);
            NativeException.ThrowIfError(NativeMethods.CoreCalcCovarMatrix(samples.NativeHandle, covar.NativeHandle, mean.NativeHandle, (int)flags, ctype));
        }

        private static void ValidateCovarInput(Mat samples, Mat mean, CovarFlags flags, int ctype)
        {
            const CovarFlags allowed = CovarFlags.Normal | CovarFlags.UseAverage | CovarFlags.Scale | CovarFlags.Rows | CovarFlags.Cols;
            if ((flags & ~allowed) != 0 || ((flags & CovarFlags.Rows) != 0) == ((flags & CovarFlags.Cols) != 0))
            {
                throw new ArgumentOutOfRangeException(nameof(flags), "Exactly one of Rows or Cols is required and no unknown covariance flags are allowed.");
            }

            if (samples.Empty || samples.Dims > 2 || samples.Channels != 1)
            {
                throw new ArgumentException("Covariance samples must be a non-empty, single-channel matrix with at most two dimensions.", nameof(samples));
            }

            if (ctype != -1 && ctype != MatType.CV_32F && ctype != MatType.CV_64F)
            {
                throw new ArgumentOutOfRangeException(nameof(ctype), "Covariance output depth must be -1, CV_32F, or CV_64F.");
            }

            if ((flags & CovarFlags.UseAverage) != 0)
            {
                int expectedRows = (flags & CovarFlags.Rows) != 0 ? 1 : samples.Rows;
                int expectedCols = (flags & CovarFlags.Rows) != 0 ? samples.Cols : 1;
                if (mean.Empty || mean.Channels != 1 || mean.Rows != expectedRows || mean.Cols != expectedCols)
                {
                    throw new ArgumentException("The supplied covariance mean has the wrong shape.", nameof(mean));
                }
            }
        }

        /// <summary>Computes principal components limited by component count. 按最大分量数计算主成分。</summary>
        public static void PcaCompute(Mat data, Mat mean, Mat eigenvectors, int maxComponents = 0)
        {
            PcaComputeCore(data, mean, eigenvectors, null, maxComponents);
        }

        /// <summary>Computes principal components and eigenvalues limited by component count. 按最大分量数计算主成分和特征值。</summary>
        public static void PcaCompute(Mat data, Mat mean, Mat eigenvectors, Mat eigenvalues, int maxComponents = 0)
        {
            ValidateNotNull(eigenvalues, nameof(eigenvalues));
            PcaComputeCore(data, mean, eigenvectors, eigenvalues, maxComponents);
        }

        private static void PcaComputeCore(Mat data, Mat mean, Mat eigenvectors, Mat? eigenvalues, int maxComponents)
        {
            ValidatePcaComputeInput(data, mean, eigenvectors);
            if (maxComponents < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxComponents));
            }

            NativeException.ThrowIfError(NativeMethods.CorePcaComputeMaxComponents(
                data.NativeHandle,
                mean.NativeHandle,
                eigenvectors.NativeHandle,
                OptionalHandle(eigenvalues),
                maxComponents));
        }

        /// <summary>Computes principal components retaining the requested variance. 按指定保留方差计算主成分。</summary>
        public static void PcaCompute(Mat data, Mat mean, Mat eigenvectors, double retainedVariance)
        {
            PcaComputeCore(data, mean, eigenvectors, null, retainedVariance);
        }

        /// <summary>Computes principal components and eigenvalues retaining the requested variance. 按指定保留方差计算主成分和特征值。</summary>
        public static void PcaCompute(Mat data, Mat mean, Mat eigenvectors, Mat eigenvalues, double retainedVariance)
        {
            ValidateNotNull(eigenvalues, nameof(eigenvalues));
            PcaComputeCore(data, mean, eigenvectors, eigenvalues, retainedVariance);
        }

        private static void PcaComputeCore(Mat data, Mat mean, Mat eigenvectors, Mat? eigenvalues, double retainedVariance)
        {
            ValidatePcaComputeInput(data, mean, eigenvectors);
            if (!(retainedVariance > 0.0) || retainedVariance > 1.0 || double.IsNaN(retainedVariance) || double.IsInfinity(retainedVariance))
            {
                throw new ArgumentOutOfRangeException(nameof(retainedVariance), "Retained variance must be finite and in (0, 1].");
            }

            NativeException.ThrowIfError(NativeMethods.CorePcaComputeRetainedVariance(
                data.NativeHandle,
                mean.NativeHandle,
                eigenvectors.NativeHandle,
                OptionalHandle(eigenvalues),
                retainedVariance));
        }

        private static void ValidatePcaComputeInput(Mat data, Mat mean, Mat eigenvectors)
        {
            ValidateNotNull(data, nameof(data));
            ValidateNotNull(mean, nameof(mean));
            ValidateNotNull(eigenvectors, nameof(eigenvectors));
            if (data.Empty || data.Dims > 2 || data.Channels != 1 ||
                (data.Depth != MatType.CV_32F && data.Depth != MatType.CV_64F))
            {
                throw new ArgumentException("PCA data must be a non-empty CV_32FC1 or CV_64FC1 matrix.", nameof(data));
            }

            if (!mean.Empty && (mean.Rows != 1 || mean.Cols != data.Cols || mean.Channels != 1 ||
                (mean.Depth != MatType.CV_32F && mean.Depth != MatType.CV_64F)))
            {
                throw new ArgumentException("PCA mean must be empty or a floating-point row vector matching the data columns.", nameof(mean));
            }
        }

        /// <summary>Projects row samples into a PCA basis. 将行样本投影到 PCA 基。</summary>
        public static void PcaProject(Mat data, Mat mean, Mat eigenvectors, Mat result)
        {
            ValidatePcaProjectionInput(data, mean, eigenvectors, result, false);
            NativeException.ThrowIfError(NativeMethods.CorePcaProject(data.NativeHandle, mean.NativeHandle, eigenvectors.NativeHandle, result.NativeHandle));
        }

        /// <summary>Projects row samples into a PCA basis and returns a new matrix. 将行样本投影到 PCA 基并返回新矩阵。</summary>
        public static Mat PcaProject(Mat data, Mat mean, Mat eigenvectors)
        {
            var result = new Mat();
            try
            {
                PcaProject(data, mean, eigenvectors, result);
                return result;
            }
            catch
            {
                result.Dispose();
                throw;
            }
        }

        /// <summary>Reconstructs row samples from a PCA basis. 从 PCA 基重建行样本。</summary>
        public static void PcaBackProject(Mat data, Mat mean, Mat eigenvectors, Mat result)
        {
            ValidatePcaProjectionInput(data, mean, eigenvectors, result, true);
            NativeException.ThrowIfError(NativeMethods.CorePcaBackProject(data.NativeHandle, mean.NativeHandle, eigenvectors.NativeHandle, result.NativeHandle));
        }

        /// <summary>Reconstructs row samples from a PCA basis and returns a new matrix. 从 PCA 基重建行样本并返回新矩阵。</summary>
        public static Mat PcaBackProject(Mat data, Mat mean, Mat eigenvectors)
        {
            var result = new Mat();
            try
            {
                PcaBackProject(data, mean, eigenvectors, result);
                return result;
            }
            catch
            {
                result.Dispose();
                throw;
            }
        }

        private static void ValidatePcaProjectionInput(Mat data, Mat mean, Mat eigenvectors, Mat result, bool backProject)
        {
            ValidateNotNull(data, nameof(data));
            ValidateNotNull(mean, nameof(mean));
            ValidateNotNull(eigenvectors, nameof(eigenvectors));
            ValidateNotNull(result, nameof(result));
            if (data.Empty || mean.Empty || eigenvectors.Empty || data.Channels != 1 || mean.Channels != 1 || eigenvectors.Channels != 1)
            {
                throw new ArgumentException("PCA projection inputs must be non-empty single-channel matrices.", nameof(data));
            }

            if (mean.Rows != 1 || mean.Cols != eigenvectors.Cols)
            {
                throw new ArgumentException("PCA mean and eigenvector feature dimensions must match.", nameof(mean));
            }

            int expectedDataCols = backProject ? eigenvectors.Rows : eigenvectors.Cols;
            if (data.Cols != expectedDataCols)
            {
                throw new ArgumentException("PCA data dimensions do not match the supplied basis.", nameof(data));
            }

            if (ReferenceEquals(result, data) || ReferenceEquals(result, mean) || ReferenceEquals(result, eigenvectors))
            {
                throw new ArgumentException("PCA result cannot alias an input matrix.", nameof(result));
            }
        }

        /// <summary>Sets the default thread-local OpenCV random generator seed. 设置当前线程默认 OpenCV 随机生成器的种子。</summary>
        public static void SetRngSeed(int seed)
        {
            NativeException.ThrowIfError(NativeMethods.CoreSetRngSeed(seed));
        }

        /// <summary>Fills a preallocated matrix with uniformly distributed values using Mat bounds. 使用 Mat 边界以均匀分布填充预分配矩阵。</summary>
        public static void Randu(Mat dst, Mat low, Mat high)
        {
            ValidateRandomMats(dst, low, high, nameof(low), nameof(high));
            NativeException.ThrowIfError(NativeMethods.CoreRanduMat(dst.NativeHandle, low.NativeHandle, high.NativeHandle));
        }

        /// <summary>Fills a preallocated matrix with uniformly distributed values using scalar bounds. 使用标量边界以均匀分布填充预分配矩阵。</summary>
        public static void Randu(Mat dst, Scalar low, Scalar high)
        {
            ValidateRandomDestination(dst);
            ValidateFiniteScalar(low, nameof(low));
            ValidateFiniteScalar(high, nameof(high));
            NativeException.ThrowIfError(NativeMethods.CoreRanduScalar(dst.NativeHandle, low.V0, low.V1, low.V2, low.V3, high.V0, high.V1, high.V2, high.V3));
        }

        /// <summary>Fills a preallocated matrix with normally distributed values using Mat parameters. 使用 Mat 参数以正态分布填充预分配矩阵。</summary>
        public static void Randn(Mat dst, Mat mean, Mat stddev)
        {
            ValidateRandomMats(dst, mean, stddev, nameof(mean), nameof(stddev));
            NativeException.ThrowIfError(NativeMethods.CoreRandnMat(dst.NativeHandle, mean.NativeHandle, stddev.NativeHandle));
        }

        /// <summary>Fills a preallocated matrix with normally distributed values using scalar parameters. 使用标量参数以正态分布填充预分配矩阵。</summary>
        public static void Randn(Mat dst, Scalar mean, Scalar stddev)
        {
            ValidateRandomDestination(dst);
            ValidateFiniteScalar(mean, nameof(mean));
            ValidateFiniteScalar(stddev, nameof(stddev));
            if (stddev.V0 < 0 || stddev.V1 < 0 || stddev.V2 < 0 || stddev.V3 < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(stddev), "Standard deviations cannot be negative.");
            }

            NativeException.ThrowIfError(NativeMethods.CoreRandnScalar(dst.NativeHandle, mean.V0, mean.V1, mean.V2, mean.V3, stddev.V0, stddev.V1, stddev.V2, stddev.V3));
        }

        private static void ValidateRandomMats(Mat dst, Mat first, Mat second, string firstName, string secondName)
        {
            ValidateRandomDestination(dst);
            ValidateNotNull(first, firstName);
            ValidateNotNull(second, secondName);
            if (first.Empty)
            {
                throw new ArgumentException("Random distribution parameter matrices cannot be empty.", firstName);
            }

            if (second.Empty)
            {
                throw new ArgumentException("Random distribution parameter matrices cannot be empty.", secondName);
            }
        }

        private static void ValidateRandomDestination(Mat dst)
        {
            ValidateNotNull(dst, nameof(dst));
            if (dst.Empty)
            {
                throw new ArgumentException("Random destination must be preallocated and non-empty.", nameof(dst));
            }
        }

        private static void ValidateFiniteScalar(Scalar value, string parameterName)
        {
            for (int i = 0; i < 4; i++)
            {
                if (double.IsNaN(value[i]) || double.IsInfinity(value[i]))
                {
                    throw new ArgumentOutOfRangeException(parameterName, "Scalar components must be finite.");
                }
            }
        }

        /// <summary>Shuffles matrix elements using the default or supplied generator. 使用默认或指定生成器打乱矩阵元素。</summary>
        public static void RandShuffle(Mat dst, double iterFactor = 1.0, Rng? rng = null)
        {
            ValidateRandomDestination(dst);
            if (dst.Dims > 2 || dst.ElemSize.ToUInt64() > 32)
            {
                throw new ArgumentException("RandShuffle supports at most two dimensions and element sizes up to 32 bytes.", nameof(dst));
            }

            if (iterFactor < 0.0 || double.IsNaN(iterFactor) || double.IsInfinity(iterFactor))
            {
                throw new ArgumentOutOfRangeException(nameof(iterFactor));
            }

            NativeException.ThrowIfError(NativeMethods.CoreRandShuffle(dst.NativeHandle, iterFactor, rng == null ? IntPtr.Zero : rng.NativeHandle));
        }

        /// <summary>Solves a continuous linear programming problem. 求解连续线性规划问题。</summary>
        public static SolveLpResult SolveLp(Mat objective, Mat constraints, Mat solution, double constraintEpsilon = 1e-12)
        {
            ValidateNotNull(objective, nameof(objective));
            ValidateNotNull(constraints, nameof(constraints));
            ValidateNotNull(solution, nameof(solution));
            ValidateSolveLpInput(objective, constraints, solution, constraintEpsilon);
            NativeException.ThrowIfError(NativeMethods.CoreSolveLp(objective.NativeHandle, constraints.NativeHandle, solution.NativeHandle, constraintEpsilon, out int result));
            return (SolveLpResult)result;
        }

        private static void ValidateSolveLpInput(Mat objective, Mat constraints, Mat solution, double constraintEpsilon)
        {
            bool objectiveType = objective.Type == MatType.CV_32FC1 || objective.Type == MatType.CV_64FC1;
            bool constraintsType = constraints.Type == MatType.CV_32FC1 || constraints.Type == MatType.CV_64FC1;
            if (objective.Empty || !objectiveType || (objective.Rows != 1 && objective.Cols != 1))
            {
                throw new ArgumentException("SolveLp objective must be a non-empty CV_32FC1 or CV_64FC1 row or column vector.", nameof(objective));
            }

            int variableCount = objective.Rows == 1 ? objective.Cols : objective.Rows;
            if (constraints.Empty || !constraintsType || constraints.Cols != variableCount + 1)
            {
                throw new ArgumentException("SolveLp constraints must have one right-hand-side column after the variable columns.", nameof(constraints));
            }

            if (constraintEpsilon < 0.0 || double.IsNaN(constraintEpsilon) || double.IsInfinity(constraintEpsilon))
            {
                throw new ArgumentOutOfRangeException(nameof(constraintEpsilon));
            }

            if (ReferenceEquals(solution, objective) || ReferenceEquals(solution, constraints))
            {
                throw new ArgumentException("SolveLp solution cannot alias an input matrix.", nameof(solution));
            }
        }
    }
}
