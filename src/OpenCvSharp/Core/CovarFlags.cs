using System;

namespace OpenCvSharp.Core
{
    /// <summary>
    /// Specifies covariance calculation modes compatible with OpenCV <c>cv::CovarFlags</c>.
    /// 指定与 OpenCV <c>cv::CovarFlags</c> 兼容的协方差计算模式。
    /// </summary>
    [Flags]
    public enum CovarFlags
    {
        /// <summary>Produces the sample-by-sample scrambled covariance matrix. 生成样本间的 scrambled 协方差矩阵。</summary>
        Scrambled = 0,

        /// <summary>Produces the normal feature covariance matrix. 生成普通特征协方差矩阵。</summary>
        Normal = 1,

        /// <summary>Uses the supplied mean instead of calculating it. 使用调用方提供的均值。</summary>
        UseAverage = 2,

        /// <summary>Scales the covariance matrix. 对协方差矩阵进行缩放。</summary>
        Scale = 4,

        /// <summary>Treats each input row as one sample. 将每一行视为一个样本。</summary>
        Rows = 8,

        /// <summary>Treats each input column as one sample. 将每一列视为一个样本。</summary>
        Cols = 16
    }
}
