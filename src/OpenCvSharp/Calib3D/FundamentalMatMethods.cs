namespace JYPPX.OpenCvSharp.Calib3D
{
    /// <summary>
    /// Algorithms for estimating a fundamental matrix.
    /// 估计基础矩阵的算法。
    /// </summary>
    public enum FundamentalMatMethods
    {
        /// <summary>
        /// Seven-point algorithm.
        /// 七点算法。
        /// </summary>
        FM7Point = 1,

        /// <summary>
        /// Eight-point algorithm.
        /// 八点算法。
        /// </summary>
        FM8Point = 2,

        /// <summary>
        /// Least-median robust algorithm.
        /// 最小中值鲁棒算法。
        /// </summary>
        LMEDS = 4,

        /// <summary>
        /// RANSAC robust algorithm.
        /// RANSAC 鲁棒算法。
        /// </summary>
        RANSAC = 8
    }
}
