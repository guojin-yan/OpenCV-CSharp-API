using System;

namespace OpenCvSharp.Calib3D
{
    /// <summary>
    /// Chessboard detection flags used by OpenCV calibration pattern APIs.
    /// OpenCV 标定棋盘格检测 API 使用的标志。
    /// </summary>
    [Flags]
    public enum ChessboardFlags
    {
        /// <summary>
        /// Uses OpenCV's default adaptive threshold and image normalization behavior.
        /// 使用 OpenCV 默认的自适应阈值和图像归一化行为。
        /// </summary>
        Default = AdaptiveThresh | NormalizeImage,

        /// <summary>
        /// Uses adaptive thresholding before corner detection.
        /// 在角点检测前使用自适应阈值。
        /// </summary>
        AdaptiveThresh = 1,

        /// <summary>
        /// Normalizes image gamma before detection.
        /// 在检测前归一化图像灰度。
        /// </summary>
        NormalizeImage = 2,

        /// <summary>
        /// Applies additional quad filtering criteria.
        /// 应用额外的四边形过滤条件。
        /// </summary>
        FilterQuads = 4,

        /// <summary>
        /// Runs a fast preliminary chessboard check.
        /// 运行快速棋盘格预检查。
        /// </summary>
        FastCheck = 8,

        /// <summary>
        /// Runs an exhaustive search.
        /// 运行穷举搜索。
        /// </summary>
        Exhaustive = 16,

        /// <summary>
        /// Upsamples input to improve sub-pixel accuracy.
        /// 上采样输入以提升亚像素精度。
        /// </summary>
        Accuracy = 32,

        /// <summary>
        /// Allows the detected pattern to be larger than the requested size.
        /// 允许检测到的图案大于请求尺寸。
        /// </summary>
        Larger = 64,

        /// <summary>
        /// Requires a marker in the detected pattern.
        /// 要求检测图案中存在标记。
        /// </summary>
        Marker = 128,

        /// <summary>
        /// Treats input as already prepared; other flags are ignored by OpenCV.
        /// 将输入视为已预处理；OpenCV 会忽略其他标志。
        /// </summary>
        Plain = 256
    }
}
