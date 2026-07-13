namespace OpenCvSharp.ImgProc
{
    /// <summary>
    /// Specifies structuring element shapes compatible with OpenCV <c>cv::MorphShapes</c>.
    /// 指定与 OpenCV <c>cv::MorphShapes</c> 兼容的结构元素形状。
    /// </summary>
    public enum MorphShapes
    {
        /// <summary>
        /// Rectangular structuring element, equivalent to <c>cv::MORPH_RECT</c>.
        /// 矩形结构元素，等价于 <c>cv::MORPH_RECT</c>。
        /// </summary>
        Rect = 0,

        /// <summary>
        /// Cross-shaped structuring element, equivalent to <c>cv::MORPH_CROSS</c>.
        /// 十字形结构元素，等价于 <c>cv::MORPH_CROSS</c>。
        /// </summary>
        Cross = 1,

        /// <summary>
        /// Elliptic structuring element, equivalent to <c>cv::MORPH_ELLIPSE</c>.
        /// 椭圆形结构元素，等价于 <c>cv::MORPH_ELLIPSE</c>。
        /// </summary>
        Ellipse = 2,

        /// <summary>
        /// Diamond-shaped structuring element, equivalent to <c>cv::MORPH_DIAMOND</c>.
        /// 菱形结构元素，等价于 <c>cv::MORPH_DIAMOND</c>。
        /// </summary>
        Diamond = 3
    }
}
