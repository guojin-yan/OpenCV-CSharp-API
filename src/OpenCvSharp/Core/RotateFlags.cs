namespace JYPPX.OpenCvSharp.Core
{
    /// <summary>
    /// Specifies rotation modes compatible with OpenCV <c>cv::RotateFlags</c>.
    /// 指定与 OpenCV <c>cv::RotateFlags</c> 兼容的旋转模式。
    /// </summary>
    public enum RotateFlags
    {
        /// <summary>
        /// Rotates the matrix 90 degrees clockwise.
        /// 将矩阵顺时针旋转 90 度。
        /// </summary>
        Rotate90Clockwise = 0,

        /// <summary>
        /// Rotates the matrix 180 degrees.
        /// 将矩阵旋转 180 度。
        /// </summary>
        Rotate180 = 1,

        /// <summary>
        /// Rotates the matrix 90 degrees counterclockwise.
        /// 将矩阵逆时针旋转 90 度。
        /// </summary>
        Rotate90Counterclockwise = 2
    }
}
