namespace OpenCvSharp.ImgProc
{
    /// <summary>
    /// Specifies contour approximation modes compatible with OpenCV <c>cv::ContourApproximationModes</c>.
    /// 指定与 OpenCV <c>cv::ContourApproximationModes</c> 兼容的轮廓近似模式。
    /// </summary>
    public enum ContourApproximationModes
    {
        /// <summary>
        /// Stores all contour points, equivalent to <c>cv::CHAIN_APPROX_NONE</c>.
        /// 保存所有轮廓点，等价于 <c>cv::CHAIN_APPROX_NONE</c>。
        /// </summary>
        ApproxNone = 1,

        /// <summary>
        /// Compresses horizontal, vertical, and diagonal segments, equivalent to <c>cv::CHAIN_APPROX_SIMPLE</c>.
        /// 压缩水平、垂直和对角线段，等价于 <c>cv::CHAIN_APPROX_SIMPLE</c>。
        /// </summary>
        ApproxSimple = 2,

        /// <summary>
        /// Teh-Chin chain approximation with L1 metric, equivalent to <c>cv::CHAIN_APPROX_TC89_L1</c>.
        /// 使用 L1 度量的 Teh-Chin 链近似，等价于 <c>cv::CHAIN_APPROX_TC89_L1</c>。
        /// </summary>
        ApproxTC89L1 = 3,

        /// <summary>
        /// Teh-Chin chain approximation with KCOS metric, equivalent to <c>cv::CHAIN_APPROX_TC89_KCOS</c>.
        /// 使用 KCOS 度量的 Teh-Chin 链近似，等价于 <c>cv::CHAIN_APPROX_TC89_KCOS</c>。
        /// </summary>
        ApproxTC89KCOS = 4
    }
}
