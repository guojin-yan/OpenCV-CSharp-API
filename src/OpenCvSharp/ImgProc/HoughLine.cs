using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.ImgProc
{
    /// <summary>
    /// Represents a standard Hough line encoded as rho and theta.
    /// 表示由 rho 和 theta 编码的标准霍夫直线。
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct HoughLine : IEquatable<HoughLine>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HoughLine"/> struct.
        /// 初始化 <see cref="HoughLine"/> 结构的新实例。
        /// </summary>
        /// <param name="rho">The distance from origin to the line. 原点到直线的距离。</param>
        /// <param name="theta">The line normal angle in radians. 直线法线角度，单位为弧度。</param>
        public HoughLine(float rho, float theta)
        {
            Rho = rho;
            Theta = theta;
        }

        /// <summary>
        /// Gets the distance from origin to the line.
        /// 获取原点到直线的距离。
        /// </summary>
        public float Rho { get; }

        /// <summary>
        /// Gets the line normal angle in radians.
        /// 获取直线法线角度，单位为弧度。
        /// </summary>
        public float Theta { get; }

        /// <summary>
        /// Gets the value at the specified index.
        /// 获取指定索引处的值。
        /// </summary>
        /// <param name="index">The value index, 0 for rho and 1 for theta. 值索引，0 表示 rho，1 表示 theta。</param>
        /// <returns>The indexed value. 索引处的值。</returns>
        public float this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return Rho;
                    case 1:
                        return Theta;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
        }

        /// <summary>
        /// Determines whether two lines are equal.
        /// 判断两条直线是否相等。
        /// </summary>
        public static bool operator ==(HoughLine left, HoughLine right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two lines are different.
        /// 判断两条直线是否不同。
        /// </summary>
        public static bool operator !=(HoughLine left, HoughLine right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Indicates whether this line equals another line.
        /// 指示此直线是否与另一条直线相等。
        /// </summary>
        public bool Equals(HoughLine other)
        {
            return Rho.Equals(other.Rho) && Theta.Equals(other.Theta);
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is HoughLine other && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                return (Rho.GetHashCode() * 397) ^ Theta.GetHashCode();
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{{Rho={0},Theta={1}}}",
                Rho,
                Theta);
        }
    }
}
