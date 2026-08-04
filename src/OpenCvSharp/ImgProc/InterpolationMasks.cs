namespace JYPPX.OpenCvSharp.ImgProc
{
    /// <summary>
    /// Specifies interpolation table constants compatible with OpenCV <c>cv::InterpolationMasks</c>.
    /// 指定与 OpenCV <c>cv::InterpolationMasks</c> 兼容的插值表常量。
    /// </summary>
    public enum InterpolationMasks
    {
        /// <summary>
        /// Number of fractional bits, equivalent to <c>cv::INTER_BITS</c>.
        /// 小数位数量，等价于 <c>cv::INTER_BITS</c>。
        /// </summary>
        Bits = 5,

        /// <summary>
        /// Number of combined two-dimensional fractional bits, equivalent to <c>cv::INTER_BITS2</c>.
        /// 二维组合小数位数量，等价于 <c>cv::INTER_BITS2</c>。
        /// </summary>
        Bits2 = 10,

        /// <summary>
        /// Interpolation lookup table size, equivalent to <c>cv::INTER_TAB_SIZE</c>.
        /// 插值查找表尺寸，等价于 <c>cv::INTER_TAB_SIZE</c>。
        /// </summary>
        TabSize = 32,

        /// <summary>
        /// Two-dimensional interpolation lookup table size, equivalent to <c>cv::INTER_TAB_SIZE2</c>.
        /// 二维插值查找表尺寸，等价于 <c>cv::INTER_TAB_SIZE2</c>。
        /// </summary>
        TabSize2 = 1024
    }
}
