namespace OpenCvSharp.ImgProc
{
    /// <summary>
    /// Specifies polar warp modes compatible with OpenCV <c>cv::WarpPolarMode</c>.
    /// 指定与 OpenCV <c>cv::WarpPolarMode</c> 兼容的极坐标变换模式。
    /// </summary>
    public enum WarpPolarMode
    {
        /// <summary>
        /// Linear polar mapping, equivalent to <c>cv::WARP_POLAR_LINEAR</c>.
        /// 线性极坐标映射，等价于 <c>cv::WARP_POLAR_LINEAR</c>。
        /// </summary>
        Linear = 0,

        /// <summary>
        /// Semilog polar mapping, equivalent to <c>cv::WARP_POLAR_LOG</c>.
        /// 半对数极坐标映射，等价于 <c>cv::WARP_POLAR_LOG</c>。
        /// </summary>
        Log = 256
    }
}
