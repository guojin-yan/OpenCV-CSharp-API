using System;

namespace OpenCvSharp.Core
{
    /// <summary>
    /// Specifies norm and normalization modes compatible with OpenCV <c>cv::NormTypes</c>.
    /// 指定与 OpenCV <c>cv::NormTypes</c> 兼容的范数与归一化模式。
    /// </summary>
    [Flags]
    public enum NormTypes
    {
        /// <summary>
        /// Infinity norm, equivalent to <c>cv::NORM_INF</c>.
        /// 无穷范数，等价于 <c>cv::NORM_INF</c>。
        /// </summary>
        Inf = 1,

        /// <summary>
        /// L1 norm, equivalent to <c>cv::NORM_L1</c>.
        /// L1 范数，等价于 <c>cv::NORM_L1</c>。
        /// </summary>
        L1 = 2,

        /// <summary>
        /// L2 norm, equivalent to <c>cv::NORM_L2</c>.
        /// L2 范数，等价于 <c>cv::NORM_L2</c>。
        /// </summary>
        L2 = 4,

        /// <summary>
        /// Squared L2 norm, equivalent to <c>cv::NORM_L2SQR</c>.
        /// L2 范数平方，等价于 <c>cv::NORM_L2SQR</c>。
        /// </summary>
        L2Sqr = 5,

        /// <summary>
        /// Hamming distance, equivalent to <c>cv::NORM_HAMMING</c>.
        /// Hamming 距离，等价于 <c>cv::NORM_HAMMING</c>。
        /// </summary>
        Hamming = 6,

        /// <summary>
        /// Hamming distance over two-bit cells, equivalent to <c>cv::NORM_HAMMING2</c>.
        /// 基于二位单元的 Hamming 距离，等价于 <c>cv::NORM_HAMMING2</c>。
        /// </summary>
        Hamming2 = 7,

        /// <summary>
        /// Relative norm flag, equivalent to <c>cv::NORM_RELATIVE</c>.
        /// 相对范数标志，等价于 <c>cv::NORM_RELATIVE</c>。
        /// </summary>
        Relative = 8,

        /// <summary>
        /// Min-max normalization mode, equivalent to <c>cv::NORM_MINMAX</c>.
        /// 最小最大归一化模式，等价于 <c>cv::NORM_MINMAX</c>。
        /// </summary>
        MinMax = 32
    }
}
