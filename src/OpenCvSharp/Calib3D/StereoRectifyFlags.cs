using System;

namespace JYPPX.OpenCvSharp.Calib3D
{
    /// <summary>
    /// Stereo rectification flags.
    /// 双目校正标志。
    /// </summary>
    [Flags]
    public enum StereoRectifyFlags
    {
        /// <summary>
        /// Uses OpenCV's default free principal-point placement.
        /// 使用 OpenCV 默认的主点自由布局。
        /// </summary>
        None = 0,

        /// <summary>
        /// Moves principal points to the same pixel coordinates in rectified images.
        /// 将校正后图像中的主点移动到相同像素坐标。
        /// </summary>
        ZeroDisparity = 0x00400
    }
}
