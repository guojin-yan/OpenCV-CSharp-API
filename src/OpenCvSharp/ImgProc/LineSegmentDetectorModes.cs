namespace OpenCvSharp.ImgProc
{
    /// <summary>
    /// Specifies line segment detector refinement modes compatible with OpenCV <c>cv::LineSegmentDetectorModes</c>.
    /// 指定与 OpenCV <c>cv::LineSegmentDetectorModes</c> 兼容的线段检测器细化模式。
    /// </summary>
    public enum LineSegmentDetectorModes
    {
        /// <summary>
        /// No refinement is applied.
        /// 不执行细化。
        /// </summary>
        None = 0,

        /// <summary>
        /// Standard refinement is applied.
        /// 执行标准细化。
        /// </summary>
        Standard = 1,

        /// <summary>
        /// Advanced refinement with false-alarm estimation is applied.
        /// 执行包含误警估计的高级细化。
        /// </summary>
        Advanced = 2
    }
}
