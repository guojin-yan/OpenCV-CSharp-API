namespace JYPPX.OpenCvSharp.Core
{
    /// <summary>
    /// Specifies random distribution types compatible with OpenCV <c>cv::RNG</c>.
    /// 指定与 OpenCV <c>cv::RNG</c> 兼容的随机分布类型。
    /// </summary>
    public enum RngDistributionTypes
    {
        /// <summary>
        /// Uniform distribution, equivalent to <c>cv::RNG::UNIFORM</c>.
        /// 均匀分布，等价于 <c>cv::RNG::UNIFORM</c>。
        /// </summary>
        Uniform = 0,

        /// <summary>
        /// Normal distribution, equivalent to <c>cv::RNG::NORMAL</c>.
        /// 正态分布，等价于 <c>cv::RNG::NORMAL</c>。
        /// </summary>
        Normal = 1
    }
}
