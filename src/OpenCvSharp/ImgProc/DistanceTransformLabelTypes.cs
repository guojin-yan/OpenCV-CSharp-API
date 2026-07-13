namespace OpenCvSharp.ImgProc
{
    /// <summary>
    /// Specifies distance-transform label output modes compatible with OpenCV <c>cv::DistanceTransformLabelTypes</c>.
    /// 指定与 OpenCV <c>cv::DistanceTransformLabelTypes</c> 兼容的距离变换标签输出模式。
    /// </summary>
    public enum DistanceTransformLabelTypes
    {
        /// <summary>
        /// Labels connected components, equivalent to <c>cv::DIST_LABEL_CCOMP</c>.
        /// 标记连通分量，等价于 <c>cv::DIST_LABEL_CCOMP</c>。
        /// </summary>
        CComp = 0,

        /// <summary>
        /// Labels nearest zero pixels, equivalent to <c>cv::DIST_LABEL_PIXEL</c>.
        /// 标记最近零像素，等价于 <c>cv::DIST_LABEL_PIXEL</c>。
        /// </summary>
        Pixel = 1
    }
}
