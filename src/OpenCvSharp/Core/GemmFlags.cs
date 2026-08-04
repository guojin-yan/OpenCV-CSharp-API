using System;

namespace JYPPX.OpenCvSharp.Core
{
    /// <summary>
    /// Specifies generalized matrix multiplication flags compatible with OpenCV <c>cv::GemmFlags</c>.
    /// 指定与 OpenCV <c>cv::GemmFlags</c> 兼容的广义矩阵乘法标志。
    /// </summary>
    [Flags]
    public enum GemmFlags
    {
        /// <summary>
        /// Uses OpenCV default multiplication behavior.
        /// 使用 OpenCV 默认矩阵乘法行为。
        /// </summary>
        None = 0,

        /// <summary>
        /// Transposes the first source matrix, equivalent to <c>cv::GEMM_1_T</c>.
        /// 转置第一个源矩阵，等价于 <c>cv::GEMM_1_T</c>。
        /// </summary>
        TransposeSrc1 = 1,

        /// <summary>
        /// Transposes the second source matrix, equivalent to <c>cv::GEMM_2_T</c>.
        /// 转置第二个源矩阵，等价于 <c>cv::GEMM_2_T</c>。
        /// </summary>
        TransposeSrc2 = 2,

        /// <summary>
        /// Transposes the third source matrix, equivalent to <c>cv::GEMM_3_T</c>.
        /// 转置第三个源矩阵，等价于 <c>cv::GEMM_3_T</c>。
        /// </summary>
        TransposeSrc3 = 4
    }
}
