using System;

namespace OpenCvSharp.ImgProc
{
    /// <summary>
    /// Specifies thresholding operation types compatible with OpenCV <c>cv::ThresholdTypes</c>.
    /// 指定与 OpenCV <c>cv::ThresholdTypes</c> 兼容的阈值操作类型。
    /// </summary>
    [Flags]
    public enum ThresholdTypes
    {
        /// <summary>
        /// Binary thresholding, equivalent to <c>cv::THRESH_BINARY</c>.
        /// 二值阈值处理，等价于 <c>cv::THRESH_BINARY</c>。
        /// </summary>
        Binary = 0,

        /// <summary>
        /// Inverted binary thresholding, equivalent to <c>cv::THRESH_BINARY_INV</c>.
        /// 反向二值阈值处理，等价于 <c>cv::THRESH_BINARY_INV</c>。
        /// </summary>
        BinaryInv = 1,

        /// <summary>
        /// Truncate thresholding, equivalent to <c>cv::THRESH_TRUNC</c>.
        /// 截断阈值处理，等价于 <c>cv::THRESH_TRUNC</c>。
        /// </summary>
        Trunc = 2,

        /// <summary>
        /// To-zero thresholding, equivalent to <c>cv::THRESH_TOZERO</c>.
        /// 置零阈值处理，等价于 <c>cv::THRESH_TOZERO</c>。
        /// </summary>
        ToZero = 3,

        /// <summary>
        /// Inverted to-zero thresholding, equivalent to <c>cv::THRESH_TOZERO_INV</c>.
        /// 反向置零阈值处理，等价于 <c>cv::THRESH_TOZERO_INV</c>。
        /// </summary>
        ToZeroInv = 4,

        /// <summary>
        /// Threshold mode mask, equivalent to <c>cv::THRESH_MASK</c>.
        /// 阈值模式掩码，等价于 <c>cv::THRESH_MASK</c>。
        /// </summary>
        Mask = 7,

        /// <summary>
        /// Otsu threshold flag, equivalent to <c>cv::THRESH_OTSU</c>.
        /// Otsu 阈值标志，等价于 <c>cv::THRESH_OTSU</c>。
        /// </summary>
        Otsu = 8,

        /// <summary>
        /// Triangle threshold flag, equivalent to <c>cv::THRESH_TRIANGLE</c>.
        /// Triangle 阈值标志，等价于 <c>cv::THRESH_TRIANGLE</c>。
        /// </summary>
        Triangle = 16,

        /// <summary>
        /// Dry-run threshold flag, equivalent to <c>cv::THRESH_DRYRUN</c>.
        /// Dry-run 阈值标志，等价于 <c>cv::THRESH_DRYRUN</c>。
        /// </summary>
        DryRun = 128
    }
}
