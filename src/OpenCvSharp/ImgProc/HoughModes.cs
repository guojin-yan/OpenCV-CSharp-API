namespace OpenCvSharp.ImgProc
{
    /// <summary>
    /// Specifies Hough transform modes compatible with OpenCV <c>cv::HoughModes</c>.
    /// 指定与 OpenCV <c>cv::HoughModes</c> 兼容的霍夫变换模式。
    /// </summary>
    public enum HoughModes
    {
        /// <summary>
        /// Standard Hough transform, equivalent to <c>cv::HOUGH_STANDARD</c>.
        /// 标准霍夫变换，等价于 <c>cv::HOUGH_STANDARD</c>。
        /// </summary>
        Standard = 0,

        /// <summary>
        /// Probabilistic Hough transform, equivalent to <c>cv::HOUGH_PROBABILISTIC</c>.
        /// 概率霍夫变换，等价于 <c>cv::HOUGH_PROBABILISTIC</c>。
        /// </summary>
        Probabilistic = 1,

        /// <summary>
        /// Multi-scale Hough transform, equivalent to <c>cv::HOUGH_MULTI_SCALE</c>.
        /// 多尺度霍夫变换，等价于 <c>cv::HOUGH_MULTI_SCALE</c>。
        /// </summary>
        MultiScale = 2,

        /// <summary>
        /// Gradient Hough circle transform, equivalent to <c>cv::HOUGH_GRADIENT</c>.
        /// 梯度霍夫圆变换，等价于 <c>cv::HOUGH_GRADIENT</c>。
        /// </summary>
        Gradient = 3,

        /// <summary>
        /// Alternative gradient Hough circle transform, equivalent to <c>cv::HOUGH_GRADIENT_ALT</c>.
        /// 替代梯度霍夫圆变换，等价于 <c>cv::HOUGH_GRADIENT_ALT</c>。
        /// </summary>
        GradientAlt = 4
    }
}
