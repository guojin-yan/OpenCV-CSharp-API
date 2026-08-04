using System;

namespace JYPPX.OpenCvSharp.ImgProc
{
    /// <summary>
    /// Specifies border extrapolation methods compatible with OpenCV <c>cv::BorderTypes</c>.
    /// 指定与 OpenCV <c>cv::BorderTypes</c> 兼容的边界外推方式。
    /// </summary>
    [Flags]
    public enum BorderTypes
    {
        /// <summary>
        /// Constant border, equivalent to <c>cv::BORDER_CONSTANT</c>.
        /// 常量边界，等价于 <c>cv::BORDER_CONSTANT</c>。
        /// </summary>
        Constant = 0,

        /// <summary>
        /// Replicated border, equivalent to <c>cv::BORDER_REPLICATE</c>.
        /// 复制边界，等价于 <c>cv::BORDER_REPLICATE</c>。
        /// </summary>
        Replicate = 1,

        /// <summary>
        /// Reflected border, equivalent to <c>cv::BORDER_REFLECT</c>.
        /// 反射边界，等价于 <c>cv::BORDER_REFLECT</c>。
        /// </summary>
        Reflect = 2,

        /// <summary>
        /// Wrapped border, equivalent to <c>cv::BORDER_WRAP</c>.
        /// 环绕边界，等价于 <c>cv::BORDER_WRAP</c>。
        /// </summary>
        Wrap = 3,

        /// <summary>
        /// Reflected 101 border, equivalent to <c>cv::BORDER_REFLECT_101</c>.
        /// 101 反射边界，等价于 <c>cv::BORDER_REFLECT_101</c>。
        /// </summary>
        Reflect101 = 4,

        /// <summary>
        /// Transparent border, equivalent to <c>cv::BORDER_TRANSPARENT</c>.
        /// 透明边界，等价于 <c>cv::BORDER_TRANSPARENT</c>。
        /// </summary>
        Transparent = 5,

        /// <summary>
        /// Default OpenCV border mode, equivalent to <c>cv::BORDER_DEFAULT</c>.
        /// OpenCV 默认边界模式，等价于 <c>cv::BORDER_DEFAULT</c>。
        /// </summary>
        Default = Reflect101,

        /// <summary>
        /// Treats ROI pixels as isolated, equivalent to <c>cv::BORDER_ISOLATED</c>.
        /// 将 ROI 像素视为隔离区域，等价于 <c>cv::BORDER_ISOLATED</c>。
        /// </summary>
        Isolated = 16
    }
}
