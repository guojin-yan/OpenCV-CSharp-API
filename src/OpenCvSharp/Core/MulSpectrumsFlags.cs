using System;

namespace JYPPX.OpenCvSharp.Core
{
    /// <summary>
    /// Specifies spectrum multiplication flags compatible with OpenCV <c>mulSpectrums</c>.
    /// 指定与 OpenCV <c>mulSpectrums</c> 兼容的频谱乘法标志。
    /// </summary>
    [Flags]
    public enum MulSpectrumsFlags
    {
        /// <summary>
        /// Uses the default whole-array spectrum operation.
        /// 使用默认的整数组频谱运算。
        /// </summary>
        None = 0,

        /// <summary>
        /// Treats each row as an independent spectrum, equivalent to <c>cv::DFT_ROWS</c>.
        /// 将每一行视为独立频谱，等价于 <c>cv::DFT_ROWS</c>。
        /// </summary>
        Rows = 4
    }
}
