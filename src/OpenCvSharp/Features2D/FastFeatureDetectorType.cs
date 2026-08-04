namespace JYPPX.OpenCvSharp.Features2D
{
    /// <summary>
    /// Specifies FAST detector neighborhoods compatible with <c>cv::FastFeatureDetector::DetectorType</c>.
    /// 指定与 OpenCV <c>cv::FastFeatureDetector::DetectorType</c> 兼容的 FAST 检测邻域类型。
    /// </summary>
    public enum FastFeatureDetectorType
    {
        /// <summary>
        /// Uses the 5-8 FAST neighborhood, equivalent to <c>cv::FastFeatureDetector::TYPE_5_8</c>.
        /// 使用 5-8 FAST 邻域，等价于 <c>cv::FastFeatureDetector::TYPE_5_8</c>。
        /// </summary>
        Type5_8 = 0,

        /// <summary>
        /// Uses the 7-12 FAST neighborhood, equivalent to <c>cv::FastFeatureDetector::TYPE_7_12</c>.
        /// 使用 7-12 FAST 邻域，等价于 <c>cv::FastFeatureDetector::TYPE_7_12</c>。
        /// </summary>
        Type7_12 = 1,

        /// <summary>
        /// Uses the 9-16 FAST neighborhood, equivalent to <c>cv::FastFeatureDetector::TYPE_9_16</c>.
        /// 使用 9-16 FAST 邻域，等价于 <c>cv::FastFeatureDetector::TYPE_9_16</c>。
        /// </summary>
        Type9_16 = 2
    }
}
