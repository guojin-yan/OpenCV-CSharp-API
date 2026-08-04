using System;
using System.Globalization;
using System.Runtime.InteropServices;
using JYPPX.OpenCvSharp.Core;

namespace JYPPX.OpenCvSharp.ImgProc
{
    /// <summary>
    /// Represents a Hough circle encoded as center and radius.
    /// 表示由圆心和半径编码的霍夫圆。
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct HoughCircle : IEquatable<HoughCircle>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HoughCircle"/> struct.
        /// 初始化 <see cref="HoughCircle"/> 结构的新实例。
        /// </summary>
        /// <param name="x">The center x coordinate. 圆心 X 坐标。</param>
        /// <param name="y">The center y coordinate. 圆心 Y 坐标。</param>
        /// <param name="radius">The circle radius. 圆半径。</param>
        public HoughCircle(float x, float y, float radius)
        {
            if (radius < 0)
                throw new ArgumentOutOfRangeException(nameof(radius), radius, "Circle radius must be non-negative.");

            Center = new Point2f(x, y);
            Radius = radius;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HoughCircle"/> struct.
        /// 初始化 <see cref="HoughCircle"/> 结构的新实例。
        /// </summary>
        /// <param name="center">The circle center. 圆心。</param>
        /// <param name="radius">The circle radius. 圆半径。</param>
        public HoughCircle(Point2f center, float radius)
        {
            if (radius < 0)
                throw new ArgumentOutOfRangeException(nameof(radius), radius, "Circle radius must be non-negative.");

            Center = center;
            Radius = radius;
        }

        /// <summary>
        /// Gets the circle center.
        /// 获取圆心。
        /// </summary>
        public Point2f Center { get; }

        /// <summary>
        /// Gets the center x coordinate.
        /// 获取圆心 X 坐标。
        /// </summary>
        public float X
        {
            get { return Center.X; }
        }

        /// <summary>
        /// Gets the center y coordinate.
        /// 获取圆心 Y 坐标。
        /// </summary>
        public float Y
        {
            get { return Center.Y; }
        }

        /// <summary>
        /// Gets the circle radius.
        /// 获取圆半径。
        /// </summary>
        public float Radius { get; }

        /// <summary>
        /// Gets the value at the specified index.
        /// 获取指定索引处的值。
        /// </summary>
        /// <param name="index">The value index, 0 for x, 1 for y, and 2 for radius. 值索引，0 表示 x，1 表示 y，2 表示半径。</param>
        /// <returns>The indexed value. 索引处的值。</returns>
        public float this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return X;
                    case 1:
                        return Y;
                    case 2:
                        return Radius;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
        }

        /// <summary>
        /// Determines whether two circles are equal.
        /// 判断两个圆是否相等。
        /// </summary>
        public static bool operator ==(HoughCircle left, HoughCircle right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two circles are different.
        /// 判断两个圆是否不同。
        /// </summary>
        public static bool operator !=(HoughCircle left, HoughCircle right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Indicates whether this circle equals another circle.
        /// 指示此圆是否与另一个圆相等。
        /// </summary>
        public bool Equals(HoughCircle other)
        {
            return Center.Equals(other.Center) && Radius.Equals(other.Radius);
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is HoughCircle other && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                return (Center.GetHashCode() * 397) ^ Radius.GetHashCode();
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{{Center={{X={0},Y={1}}},Radius={2}}}",
                X,
                Y,
                Radius);
        }
    }
}
