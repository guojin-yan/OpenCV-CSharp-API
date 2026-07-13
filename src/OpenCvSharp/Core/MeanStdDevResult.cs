using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Core
{
    /// <summary>
    /// Represents the result of OpenCV <c>cv::meanStdDev</c>.
    /// 表示 OpenCV <c>cv::meanStdDev</c> 的结果。
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct MeanStdDevResult : IEquatable<MeanStdDevResult>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MeanStdDevResult"/> struct.
        /// 初始化 <see cref="MeanStdDevResult"/> 结构的新实例。
        /// </summary>
        /// <param name="mean">The per-channel mean values. 各通道均值。</param>
        /// <param name="stdDev">The per-channel standard deviation values. 各通道标准差。</param>
        public MeanStdDevResult(Scalar mean, Scalar stdDev)
        {
            Mean = mean;
            StdDev = stdDev;
        }

        /// <summary>
        /// Gets the per-channel mean values.
        /// 获取各通道均值。
        /// </summary>
        public Scalar Mean { get; }

        /// <summary>
        /// Gets the per-channel standard deviation values.
        /// 获取各通道标准差。
        /// </summary>
        public Scalar StdDev { get; }

        /// <summary>
        /// Determines whether two results are equal.
        /// 判断两个结果是否相等。
        /// </summary>
        public static bool operator ==(MeanStdDevResult left, MeanStdDevResult right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two results are different.
        /// 判断两个结果是否不同。
        /// </summary>
        public static bool operator !=(MeanStdDevResult left, MeanStdDevResult right)
        {
            return !left.Equals(right);
        }

        /// <inheritdoc/>
        public bool Equals(MeanStdDevResult other)
        {
            return Mean.Equals(other.Mean) &&
                StdDev.Equals(other.StdDev);
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is MeanStdDevResult other && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = Mean.GetHashCode();
                hash = (hash * 397) ^ StdDev.GetHashCode();
                return hash;
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "{Mean=" + Mean + ",StdDev=" + StdDev + "}";
        }
    }
}
