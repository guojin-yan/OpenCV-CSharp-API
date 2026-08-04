namespace JYPPX.OpenCvSharp.Features2D
{
    /// <summary>
    /// Specifies ORB keypoint scoring methods compatible with <c>cv::ORB::ScoreType</c>.
    /// 指定与 OpenCV <c>cv::ORB::ScoreType</c> 兼容的 ORB 关键点评分方式。
    /// </summary>
    public enum OrbScoreType
    {
        /// <summary>
        /// Uses Harris response for ranking keypoints, equivalent to <c>cv::ORB::HARRIS_SCORE</c>.
        /// 使用 Harris 响应对关键点排序，等价于 <c>cv::ORB::HARRIS_SCORE</c>。
        /// </summary>
        HarrisScore = 0,

        /// <summary>
        /// Uses FAST response for ranking keypoints, equivalent to <c>cv::ORB::FAST_SCORE</c>.
        /// 使用 FAST 响应对关键点排序，等价于 <c>cv::ORB::FAST_SCORE</c>。
        /// </summary>
        FastScore = 1
    }
}
