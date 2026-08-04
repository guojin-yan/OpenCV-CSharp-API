using System;

namespace JYPPX.OpenCvSharp.ImgProc
{
    /// <summary>
    /// Specifies flood-fill options compatible with OpenCV <c>cv::FloodFillFlags</c>.
    /// 指定与 OpenCV <c>cv::FloodFillFlags</c> 兼容的泛洪填充选项。
    /// </summary>
    [Flags]
    public enum FloodFillFlags
    {
        /// <summary>
        /// Uses floating range comparison, equivalent to no special flood-fill flag.
        /// 使用浮动范围比较，等价于不设置特殊泛洪填充标志。
        /// </summary>
        None = 0,

        /// <summary>
        /// Uses 4-connected neighborhood, equivalent to a connectivity value of 4 in <c>cv::floodFill</c>.
        /// 使用 4 连通邻域，等价于 <c>cv::floodFill</c> 中的连通性值 4。
        /// </summary>
        Connectivity4 = 4,

        /// <summary>
        /// Uses 8-connected neighborhood, equivalent to a connectivity value of 8 in <c>cv::floodFill</c>.
        /// 使用 8 连通邻域，等价于 <c>cv::floodFill</c> 中的连通性值 8。
        /// </summary>
        Connectivity8 = 8,

        /// <summary>
        /// Uses fixed range comparison, equivalent to <c>cv::FLOODFILL_FIXED_RANGE</c>.
        /// 使用固定范围比较，等价于 <c>cv::FLOODFILL_FIXED_RANGE</c>。
        /// </summary>
        FixedRange = 1 << 16,

        /// <summary>
        /// Fills only the mask, equivalent to <c>cv::FLOODFILL_MASK_ONLY</c>.
        /// 只填充掩码，等价于 <c>cv::FLOODFILL_MASK_ONLY</c>。
        /// </summary>
        MaskOnly = 1 << 17
    }
}
