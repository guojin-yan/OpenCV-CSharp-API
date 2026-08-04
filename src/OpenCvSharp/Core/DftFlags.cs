using System;

namespace JYPPX.OpenCvSharp.Core
{
    /// <summary>
    /// Specifies discrete Fourier transform flags compatible with OpenCV <c>cv::DftFlags</c>.
    /// 指定与 OpenCV <c>cv::DftFlags</c> 兼容的离散傅里叶变换标志。
    /// </summary>
    [Flags]
    public enum DftFlags
    {
        /// <summary>
        /// Uses a forward transform with default output packing.
        /// 使用默认输出打包方式的正向变换。
        /// </summary>
        None = 0,

        /// <summary>
        /// Performs an inverse transform, equivalent to <c>cv::DFT_INVERSE</c>.
        /// 执行逆变换，等价于 <c>cv::DFT_INVERSE</c>。
        /// </summary>
        Inverse = 1,

        /// <summary>
        /// Scales the result, equivalent to <c>cv::DFT_SCALE</c>.
        /// 对结果进行缩放，等价于 <c>cv::DFT_SCALE</c>。
        /// </summary>
        Scale = 2,

        /// <summary>
        /// Transforms each matrix row independently, equivalent to <c>cv::DFT_ROWS</c>.
        /// 独立变换矩阵的每一行，等价于 <c>cv::DFT_ROWS</c>。
        /// </summary>
        Rows = 4,

        /// <summary>
        /// Produces a full complex output array, equivalent to <c>cv::DFT_COMPLEX_OUTPUT</c>.
        /// 生成完整复数输出数组，等价于 <c>cv::DFT_COMPLEX_OUTPUT</c>。
        /// </summary>
        ComplexOutput = 16,

        /// <summary>
        /// Produces a real output array, equivalent to <c>cv::DFT_REAL_OUTPUT</c>.
        /// 生成实数输出数组，等价于 <c>cv::DFT_REAL_OUTPUT</c>。
        /// </summary>
        RealOutput = 32,

        /// <summary>
        /// Treats input as complex, equivalent to <c>cv::DFT_COMPLEX_INPUT</c>.
        /// 将输入视为复数，等价于 <c>cv::DFT_COMPLEX_INPUT</c>。
        /// </summary>
        ComplexInput = 64
    }
}
