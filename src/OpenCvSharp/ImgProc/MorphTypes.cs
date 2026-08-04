namespace JYPPX.OpenCvSharp.ImgProc
{
    /// <summary>
    /// Specifies morphological operation types compatible with OpenCV <c>cv::MorphTypes</c>.
    /// 指定与 OpenCV <c>cv::MorphTypes</c> 兼容的形态学操作类型。
    /// </summary>
    public enum MorphTypes
    {
        /// <summary>
        /// Erosion operation, equivalent to <c>cv::MORPH_ERODE</c>.
        /// 腐蚀操作，等价于 <c>cv::MORPH_ERODE</c>。
        /// </summary>
        Erode = 0,

        /// <summary>
        /// Dilation operation, equivalent to <c>cv::MORPH_DILATE</c>.
        /// 膨胀操作，等价于 <c>cv::MORPH_DILATE</c>。
        /// </summary>
        Dilate = 1,

        /// <summary>
        /// Opening operation, equivalent to <c>cv::MORPH_OPEN</c>.
        /// 开运算，等价于 <c>cv::MORPH_OPEN</c>。
        /// </summary>
        Open = 2,

        /// <summary>
        /// Closing operation, equivalent to <c>cv::MORPH_CLOSE</c>.
        /// 闭运算，等价于 <c>cv::MORPH_CLOSE</c>。
        /// </summary>
        Close = 3,

        /// <summary>
        /// Morphological gradient operation, equivalent to <c>cv::MORPH_GRADIENT</c>.
        /// 形态学梯度操作，等价于 <c>cv::MORPH_GRADIENT</c>。
        /// </summary>
        Gradient = 4,

        /// <summary>
        /// Top hat operation, equivalent to <c>cv::MORPH_TOPHAT</c>.
        /// 顶帽操作，等价于 <c>cv::MORPH_TOPHAT</c>。
        /// </summary>
        TopHat = 5,

        /// <summary>
        /// Black hat operation, equivalent to <c>cv::MORPH_BLACKHAT</c>.
        /// 黑帽操作，等价于 <c>cv::MORPH_BLACKHAT</c>。
        /// </summary>
        BlackHat = 6,

        /// <summary>
        /// Hit-or-miss operation, equivalent to <c>cv::MORPH_HITMISS</c>.
        /// 击中击不中操作，等价于 <c>cv::MORPH_HITMISS</c>。
        /// </summary>
        HitMiss = 7
    }
}
