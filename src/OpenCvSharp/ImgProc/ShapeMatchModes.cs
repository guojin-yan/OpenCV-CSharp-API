namespace OpenCvSharp.ImgProc
{
    /// <summary>
    /// Specifies contour shape comparison methods compatible with OpenCV <c>cv::ShapeMatchModes</c>.
    /// 指定与 OpenCV <c>cv::ShapeMatchModes</c> 兼容的轮廓形状比较方法。
    /// </summary>
    public enum ShapeMatchModes
    {
        /// <summary>
        /// Compares shapes with OpenCV <c>cv::CONTOURS_MATCH_I1</c>.
        /// 使用 OpenCV <c>cv::CONTOURS_MATCH_I1</c> 比较形状。
        /// </summary>
        I1 = 1,

        /// <summary>
        /// Compares shapes with OpenCV <c>cv::CONTOURS_MATCH_I2</c>.
        /// 使用 OpenCV <c>cv::CONTOURS_MATCH_I2</c> 比较形状。
        /// </summary>
        I2 = 2,

        /// <summary>
        /// Compares shapes with OpenCV <c>cv::CONTOURS_MATCH_I3</c>.
        /// 使用 OpenCV <c>cv::CONTOURS_MATCH_I3</c> 比较形状。
        /// </summary>
        I3 = 3
    }
}
