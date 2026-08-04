namespace JYPPX.OpenCvSharp.ImgProc
{
    /// <summary>
    /// Specifies adaptive thresholding algorithms compatible with OpenCV <c>cv::AdaptiveThresholdTypes</c>.
    /// 指定与 OpenCV <c>cv::AdaptiveThresholdTypes</c> 兼容的自适应阈值算法。
    /// </summary>
    public enum AdaptiveThresholdTypes
    {
        /// <summary>
        /// Mean adaptive thresholding, equivalent to <c>cv::ADAPTIVE_THRESH_MEAN_C</c>.
        /// 均值自适应阈值处理，等价于 <c>cv::ADAPTIVE_THRESH_MEAN_C</c>。
        /// </summary>
        MeanC = 0,

        /// <summary>
        /// Gaussian adaptive thresholding, equivalent to <c>cv::ADAPTIVE_THRESH_GAUSSIAN_C</c>.
        /// 高斯自适应阈值处理，等价于 <c>cv::ADAPTIVE_THRESH_GAUSSIAN_C</c>。
        /// </summary>
        GaussianC = 1
    }
}
