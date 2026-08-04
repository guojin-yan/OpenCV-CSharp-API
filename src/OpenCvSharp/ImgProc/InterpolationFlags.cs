using System;

namespace JYPPX.OpenCvSharp.ImgProc
{
    /// <summary>
    /// Specifies interpolation algorithms compatible with OpenCV <c>cv::InterpolationFlags</c>.
    /// 指定与 OpenCV <c>cv::InterpolationFlags</c> 兼容的插值算法。
    /// </summary>
    [Flags]
    public enum InterpolationFlags
    {
        /// <summary>
        /// Nearest-neighbor interpolation, equivalent to <c>cv::INTER_NEAREST</c>.
        /// 最近邻插值，等价于 <c>cv::INTER_NEAREST</c>。
        /// </summary>
        Nearest = 0,

        /// <summary>
        /// Bilinear interpolation, equivalent to <c>cv::INTER_LINEAR</c>.
        /// 双线性插值，等价于 <c>cv::INTER_LINEAR</c>。
        /// </summary>
        Linear = 1,

        /// <summary>
        /// Bicubic interpolation, equivalent to <c>cv::INTER_CUBIC</c>.
        /// 双三次插值，等价于 <c>cv::INTER_CUBIC</c>。
        /// </summary>
        Cubic = 2,

        /// <summary>
        /// Resampling using pixel area relation, equivalent to <c>cv::INTER_AREA</c>.
        /// 基于像素区域关系的重采样，等价于 <c>cv::INTER_AREA</c>。
        /// </summary>
        Area = 3,

        /// <summary>
        /// Lanczos interpolation over an 8x8 neighborhood, equivalent to <c>cv::INTER_LANCZOS4</c>.
        /// 8x8 邻域 Lanczos 插值，等价于 <c>cv::INTER_LANCZOS4</c>。
        /// </summary>
        Lanczos4 = 4,

        /// <summary>
        /// Bit-exact bilinear interpolation, equivalent to <c>cv::INTER_LINEAR_EXACT</c>.
        /// 位精确双线性插值，等价于 <c>cv::INTER_LINEAR_EXACT</c>。
        /// </summary>
        LinearExact = 5,

        /// <summary>
        /// Bit-exact nearest-neighbor interpolation, equivalent to <c>cv::INTER_NEAREST_EXACT</c>.
        /// 位精确最近邻插值，等价于 <c>cv::INTER_NEAREST_EXACT</c>。
        /// </summary>
        NearestExact = 6,

        /// <summary>
        /// Interpolation code mask, equivalent to <c>cv::INTER_MAX</c>.
        /// 插值代码掩码，等价于 <c>cv::INTER_MAX</c>。
        /// </summary>
        Max = 7,

        /// <summary>
        /// Fills destination outliers, equivalent to <c>cv::WARP_FILL_OUTLIERS</c>.
        /// 填充目标图像中的越界像素，等价于 <c>cv::WARP_FILL_OUTLIERS</c>。
        /// </summary>
        WarpFillOutliers = 8,

        /// <summary>
        /// Treats the transform matrix as inverse mapping, equivalent to <c>cv::WARP_INVERSE_MAP</c>.
        /// 将变换矩阵视为逆映射，等价于 <c>cv::WARP_INVERSE_MAP</c>。
        /// </summary>
        WarpInverseMap = 16,

        /// <summary>
        /// Uses relative remap coordinates, equivalent to <c>cv::WARP_RELATIVE_MAP</c>.
        /// 使用相对重映射坐标，等价于 <c>cv::WARP_RELATIVE_MAP</c>。
        /// </summary>
        WarpRelativeMap = 32
    }
}
