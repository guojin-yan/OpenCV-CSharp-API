using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Core
{
    /// <summary>
    /// Represents the result of OpenCV <c>cv::minMaxLoc</c>.
    /// 表示 OpenCV <c>cv::minMaxLoc</c> 的结果。
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct MinMaxLocResult : IEquatable<MinMaxLocResult>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MinMaxLocResult"/> struct.
        /// 初始化 <see cref="MinMaxLocResult"/> 结构的新实例。
        /// </summary>
        /// <param name="minVal">The minimum value. 最小值。</param>
        /// <param name="maxVal">The maximum value. 最大值。</param>
        /// <param name="minLoc">The minimum value location. 最小值位置。</param>
        /// <param name="maxLoc">The maximum value location. 最大值位置。</param>
        public MinMaxLocResult(double minVal, double maxVal, Point minLoc, Point maxLoc)
        {
            MinVal = minVal;
            MaxVal = maxVal;
            MinLoc = minLoc;
            MaxLoc = maxLoc;
        }

        /// <summary>
        /// Gets the minimum value.
        /// 获取最小值。
        /// </summary>
        public double MinVal { get; }

        /// <summary>
        /// Gets the maximum value.
        /// 获取最大值。
        /// </summary>
        public double MaxVal { get; }

        /// <summary>
        /// Gets the minimum value location.
        /// 获取最小值位置。
        /// </summary>
        public Point MinLoc { get; }

        /// <summary>
        /// Gets the maximum value location.
        /// 获取最大值位置。
        /// </summary>
        public Point MaxLoc { get; }

        /// <summary>
        /// Determines whether two results are equal.
        /// 判断两个结果是否相等。
        /// </summary>
        public static bool operator ==(MinMaxLocResult left, MinMaxLocResult right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two results are different.
        /// 判断两个结果是否不同。
        /// </summary>
        public static bool operator !=(MinMaxLocResult left, MinMaxLocResult right)
        {
            return !left.Equals(right);
        }

        /// <inheritdoc/>
        public bool Equals(MinMaxLocResult other)
        {
            return MinVal.Equals(other.MinVal) &&
                MaxVal.Equals(other.MaxVal) &&
                MinLoc.Equals(other.MinLoc) &&
                MaxLoc.Equals(other.MaxLoc);
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is MinMaxLocResult other && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = MinVal.GetHashCode();
                hash = (hash * 397) ^ MaxVal.GetHashCode();
                hash = (hash * 397) ^ MinLoc.GetHashCode();
                hash = (hash * 397) ^ MaxLoc.GetHashCode();
                return hash;
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "{MinVal=" + MinVal.ToString(CultureInfo.InvariantCulture) +
                ",MaxVal=" + MaxVal.ToString(CultureInfo.InvariantCulture) +
                ",MinLoc=" + MinLoc +
                ",MaxLoc=" + MaxLoc + "}";
        }
    }
}
