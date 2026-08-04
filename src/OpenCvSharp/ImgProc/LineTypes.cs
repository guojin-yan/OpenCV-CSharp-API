namespace JYPPX.OpenCvSharp.ImgProc
{
    /// <summary>
    /// Specifies line drawing algorithms compatible with OpenCV <c>cv::LineTypes</c>.
    /// 指定与 OpenCV <c>cv::LineTypes</c> 兼容的线条绘制算法。
    /// </summary>
    public enum LineTypes
    {
        /// <summary>
        /// Four-connected line, equivalent to <c>cv::LINE_4</c>.
        /// 四连通线条，等价于 <c>cv::LINE_4</c>。
        /// </summary>
        Line4 = 4,

        /// <summary>
        /// Eight-connected line, equivalent to <c>cv::LINE_8</c>.
        /// 八连通线条，等价于 <c>cv::LINE_8</c>。
        /// </summary>
        Line8 = 8,

        /// <summary>
        /// Antialiased line, equivalent to <c>cv::LINE_AA</c>.
        /// 抗锯齿线条，等价于 <c>cv::LINE_AA</c>。
        /// </summary>
        AntiAlias = 16
    }
}
