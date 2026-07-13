using System;

namespace OpenCvSharp.Features2D
{
    /// <summary>
    /// Specifies feature drawing flags compatible with OpenCV <c>cv::DrawMatchesFlags</c>.
    /// 指定与 OpenCV <c>cv::DrawMatchesFlags</c> 兼容的特征绘制标志。
    /// </summary>
    [Flags]
    public enum DrawMatchesFlags
    {
        /// <summary>
        /// Uses OpenCV default drawing behavior.
        /// 使用 OpenCV 默认绘制行为。
        /// </summary>
        Default = 0,

        /// <summary>
        /// Draws over an existing output image, equivalent to <c>cv::DrawMatchesFlags::DRAW_OVER_OUTIMG</c>.
        /// 在已有输出图像上绘制，等价于 <c>cv::DrawMatchesFlags::DRAW_OVER_OUTIMG</c>。
        /// </summary>
        DrawOverOutImg = 1,

        /// <summary>
        /// Does not draw unmatched single keypoints, equivalent to <c>cv::DrawMatchesFlags::NOT_DRAW_SINGLE_POINTS</c>.
        /// 不绘制未匹配的单个关键点，等价于 <c>cv::DrawMatchesFlags::NOT_DRAW_SINGLE_POINTS</c>。
        /// </summary>
        NotDrawSinglePoints = 2,

        /// <summary>
        /// Draws rich keypoint circles with size and orientation, equivalent to <c>cv::DrawMatchesFlags::DRAW_RICH_KEYPOINTS</c>.
        /// 绘制包含尺度和方向的关键点圆，等价于 <c>cv::DrawMatchesFlags::DRAW_RICH_KEYPOINTS</c>。
        /// </summary>
        DrawRichKeypoints = 4
    }
}
