using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.ImgProc
{
    /// <summary>
    /// Represents a Hough line detected from a point set, encoded as votes, rho, and theta.
    /// 表示从点集检测到的霍夫直线，由票数、rho 和 theta 编码。
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct HoughLinePointSet : IEquatable<HoughLinePointSet>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HoughLinePointSet"/> struct.
        /// 初始化 <see cref="HoughLinePointSet"/> 结构的新实例。
        /// </summary>
        /// <param name="votes">The accumulator votes. 累加器票数。</param>
        /// <param name="rho">The distance from origin to the line. 原点到直线的距离。</param>
        /// <param name="theta">The line normal angle in radians. 直线法线角度，单位为弧度。</param>
        public HoughLinePointSet(double votes, double rho, double theta)
        {
            Votes = votes;
            Rho = rho;
            Theta = theta;
        }

        /// <summary>
        /// Gets the accumulator votes.
        /// 获取累加器票数。
        /// </summary>
        public double Votes { get; }

        /// <summary>
        /// Gets the distance from origin to the line.
        /// 获取原点到直线的距离。
        /// </summary>
        public double Rho { get; }

        /// <summary>
        /// Gets the line normal angle in radians.
        /// 获取直线法线角度，单位为弧度。
        /// </summary>
        public double Theta { get; }

        /// <summary>
        /// Gets the value at the specified index.
        /// 获取指定索引处的值。
        /// </summary>
        /// <param name="index">The value index, 0 for votes, 1 for rho, and 2 for theta. 值索引，0 表示票数，1 表示 rho，2 表示 theta。</param>
        /// <returns>The indexed value. 索引处的值。</returns>
        public double this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return Votes;
                    case 1:
                        return Rho;
                    case 2:
                        return Theta;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
        }

        /// <summary>
        /// Determines whether two values are equal.
        /// 判断两个值是否相等。
        /// </summary>
        public static bool operator ==(HoughLinePointSet left, HoughLinePointSet right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two values are different.
        /// 判断两个值是否不同。
        /// </summary>
        public static bool operator !=(HoughLinePointSet left, HoughLinePointSet right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Indicates whether this value equals another value.
        /// 指示此值是否与另一个值相等。
        /// </summary>
        public bool Equals(HoughLinePointSet other)
        {
            return Votes.Equals(other.Votes) && Rho.Equals(other.Rho) && Theta.Equals(other.Theta);
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is HoughLinePointSet other && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = Votes.GetHashCode();
                hash = (hash * 397) ^ Rho.GetHashCode();
                hash = (hash * 397) ^ Theta.GetHashCode();
                return hash;
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{{Votes={0},Rho={1},Theta={2}}}",
                Votes,
                Rho,
                Theta);
        }
    }
}
