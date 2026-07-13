namespace OpenCvSharp.ImgProc
{
    /// <summary>
    /// Specifies distance-transform mask sizes compatible with OpenCV <c>cv::DistanceTransformMasks</c>.
    /// 指定与 OpenCV <c>cv::DistanceTransformMasks</c> 兼容的距离变换掩码尺寸。
    /// </summary>
    public enum DistanceTransformMasks
    {
        /// <summary>
        /// Precise distance transform, equivalent to <c>cv::DIST_MASK_PRECISE</c>.
        /// 精确距离变换，等价于 <c>cv::DIST_MASK_PRECISE</c>。
        /// </summary>
        Precise = 0,

        /// <summary>
        /// 3x3 mask, equivalent to <c>cv::DIST_MASK_3</c>.
        /// 3x3 掩码，等价于 <c>cv::DIST_MASK_3</c>。
        /// </summary>
        Mask3 = 3,

        /// <summary>
        /// 5x5 mask, equivalent to <c>cv::DIST_MASK_5</c>.
        /// 5x5 掩码，等价于 <c>cv::DIST_MASK_5</c>。
        /// </summary>
        Mask5 = 5
    }
}
