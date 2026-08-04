using System;
using JYPPX.OpenCvSharp.Core;

namespace JYPPX.OpenCvSharp.Calib3D
{
    /// <summary>
    /// Multi-solution pose result returned by <c>SolvePnPGeneric</c>.
    /// <c>SolvePnPGeneric</c> 返回的多解位姿结果。
    /// </summary>
    public readonly struct SolvePnPGenericResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SolvePnPGenericResult"/> struct.
        /// 初始化 <see cref="SolvePnPGenericResult"/> 结构的新实例。
        /// </summary>
        /// <param name="solutionCount">The number of pose solutions. 位姿解数量。</param>
        /// <param name="rvecs">An <c>N x 3</c> matrix containing rotation vectors. 包含旋转向量的 <c>N x 3</c> 矩阵。</param>
        /// <param name="tvecs">An <c>N x 3</c> matrix containing translation vectors. 包含平移向量的 <c>N x 3</c> 矩阵。</param>
        /// <param name="reprojectionError">Optional reprojection error output. 可选重投影误差输出。</param>
        public SolvePnPGenericResult(int solutionCount, Mat rvecs, Mat tvecs, Mat? reprojectionError)
        {
            if (solutionCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(solutionCount), "Solution count cannot be negative.");
            }

            SolutionCount = solutionCount;
            Rvecs = rvecs ?? throw new ArgumentNullException(nameof(rvecs));
            Tvecs = tvecs ?? throw new ArgumentNullException(nameof(tvecs));
            ReprojectionError = reprojectionError;
            ValidatePoseVectorRows(solutionCount, Rvecs, nameof(rvecs));
            ValidatePoseVectorRows(solutionCount, Tvecs, nameof(tvecs));
            ValidatePoseVectorColumns(Rvecs, nameof(rvecs));
            ValidatePoseVectorColumns(Tvecs, nameof(tvecs));
        }

        /// <summary>
        /// Gets the number of pose solutions.
        /// 获取位姿解数量。
        /// </summary>
        public int SolutionCount { get; }

        /// <summary>
        /// Gets an <c>N x 3</c> matrix containing rotation vectors in row-major solution order.
        /// 获取按解顺序逐行存放旋转向量的 <c>N x 3</c> 矩阵。
        /// </summary>
        public Mat Rvecs { get; }

        /// <summary>
        /// Gets an <c>N x 3</c> matrix containing translation vectors in row-major solution order.
        /// 获取按解顺序逐行存放平移向量的 <c>N x 3</c> 矩阵。
        /// </summary>
        public Mat Tvecs { get; }

        /// <summary>
        /// Gets the optional reprojection error matrix.
        /// 获取可选重投影误差矩阵。
        /// </summary>
        public Mat? ReprojectionError { get; }

        /// <summary>
        /// Gets whether reprojection error output is available.
        /// 获取是否包含重投影误差输出。
        /// </summary>
        public bool HasReprojectionError
        {
            get { return ReprojectionError != null; }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "{SolutionCount=" + SolutionCount
                + ",Rvecs=" + Rvecs.Rows + "x" + Rvecs.Cols
                + ",Tvecs=" + Tvecs.Rows + "x" + Tvecs.Cols
                + ",ReprojectionError=" + (ReprojectionError == null ? "<null>" : ReprojectionError.Rows + "x" + ReprojectionError.Cols)
                + "}";
        }

        private static void ValidatePoseVectorRows(int solutionCount, Mat vectorMatrix, string parameterName)
        {
            if (vectorMatrix.Rows != solutionCount)
            {
                throw new ArgumentException("Pose vector row count must match the solution count.", parameterName);
            }
        }

        private static void ValidatePoseVectorColumns(Mat vectorMatrix, string parameterName)
        {
            if (vectorMatrix.Rows != 0 && vectorMatrix.Cols != 3)
            {
                throw new ArgumentException("Pose vector column count must be 3.", parameterName);
            }
        }
    }
}
