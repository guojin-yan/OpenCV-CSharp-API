using System;

namespace OpenCvSharp.Core
{
    /// <summary>
    /// Specifies discrete cosine transform flags compatible with OpenCV DCT flags.
    /// 指定与 OpenCV DCT 标志兼容的离散余弦变换标志。
    /// </summary>
    [Flags]
    public enum DctFlags
    {
        /// <summary>
        /// Uses a forward DCT.
        /// 使用正向 DCT。
        /// </summary>
        None = 0,

        /// <summary>
        /// Performs an inverse DCT, equivalent to <c>cv::DCT_INVERSE</c>.
        /// 执行逆 DCT，等价于 <c>cv::DCT_INVERSE</c>。
        /// </summary>
        Inverse = 1,

        /// <summary>
        /// Transforms each matrix row independently, equivalent to <c>cv::DCT_ROWS</c>.
        /// 独立变换矩阵的每一行，等价于 <c>cv::DCT_ROWS</c>。
        /// </summary>
        Rows = 4
    }
}
