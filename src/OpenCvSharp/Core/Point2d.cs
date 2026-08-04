using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Core
{
    /// <summary>
    /// Represents a two-dimensional double-precision point compatible with OpenCV's <c>cv::Point2d</c>.
    /// 表示与 OpenCV <c>cv::Point2d</c> 对应的二维双精度浮点点。
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Point2d : IEquatable<Point2d>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Point2d"/> struct.
        /// 初始化 <see cref="Point2d"/> 结构的新实例。
        /// </summary>
        /// <param name="x">The x coordinate. X 坐标。</param>
        /// <param name="y">The y coordinate. Y 坐标。</param>
        public Point2d(double x, double y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Gets the x coordinate.
        /// 获取 X 坐标。
        /// </summary>
        public double X { get; }

        /// <summary>
        /// Gets the y coordinate.
        /// 获取 Y 坐标。
        /// </summary>
        public double Y { get; }

        /// <summary>
        /// Determines whether two points are equal.
        /// 判断两个点是否相等。
        /// </summary>
        /// <param name="left">The first point. 第一个点。</param>
        /// <param name="right">The second point. 第二个点。</param>
        /// <returns><c>true</c> if both coordinates are equal; otherwise, <c>false</c>. 如果两个坐标相等则返回 <c>true</c>，否则返回 <c>false</c>。</returns>
        public static bool operator ==(Point2d left, Point2d right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two points are different.
        /// 判断两个点是否不同。
        /// </summary>
        /// <param name="left">The first point. 第一个点。</param>
        /// <param name="right">The second point. 第二个点。</param>
        /// <returns><c>true</c> if any coordinate differs; otherwise, <c>false</c>. 如果任一坐标不同则返回 <c>true</c>，否则返回 <c>false</c>。</returns>
        public static bool operator !=(Point2d left, Point2d right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Indicates whether this point equals another point.
        /// 指示此点是否与另一个点相等。
        /// </summary>
        /// <param name="other">The other point. 另一个点。</param>
        /// <returns><c>true</c> if both coordinates are equal; otherwise, <c>false</c>. 如果两个坐标相等则返回 <c>true</c>，否则返回 <c>false</c>。</returns>
        public bool Equals(Point2d other)
        {
            return X.Equals(other.X) && Y.Equals(other.Y);
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is Point2d other && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                return (X.GetHashCode() * 397) ^ Y.GetHashCode();
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{{X={0},Y={1}}}",
                X,
                Y);
        }
    }
}
