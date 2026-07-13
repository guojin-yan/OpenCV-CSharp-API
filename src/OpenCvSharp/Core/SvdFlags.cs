using System;

namespace OpenCvSharp.Core
{
    /// <summary>
    /// Specifies singular value decomposition flags compatible with OpenCV <c>cv::SVD::Flags</c>.
    /// 指定与 OpenCV <c>cv::SVD::Flags</c> 兼容的奇异值分解标志。
    /// </summary>
    [Flags]
    public enum SvdFlags
    {
        /// <summary>
        /// Uses OpenCV default SVD behavior.
        /// 使用 OpenCV 默认 SVD 行为。
        /// </summary>
        None = 0,

        /// <summary>
        /// Allows OpenCV to modify the decomposed matrix, equivalent to <c>cv::SVD::MODIFY_A</c>.
        /// 允许 OpenCV 修改被分解矩阵，等价于 <c>cv::SVD::MODIFY_A</c>。
        /// </summary>
        ModifyA = 1,

        /// <summary>
        /// Computes only singular values, equivalent to <c>cv::SVD::NO_UV</c>.
        /// 只计算奇异值，等价于 <c>cv::SVD::NO_UV</c>。
        /// </summary>
        NoUv = 2,

        /// <summary>
        /// Computes full-size orthogonal matrices, equivalent to <c>cv::SVD::FULL_UV</c>.
        /// 计算完整尺寸的正交矩阵，等价于 <c>cv::SVD::FULL_UV</c>。
        /// </summary>
        FullUv = 4
    }
}
