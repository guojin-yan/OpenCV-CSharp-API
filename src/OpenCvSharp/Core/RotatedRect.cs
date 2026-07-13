using System;
using System.Globalization;

namespace OpenCvSharp.Core
{
    /// <summary>
    /// Represents a rotated rectangle compatible with OpenCV's <c>cv::RotatedRect</c>.
    /// 表示与 OpenCV <c>cv::RotatedRect</c> 对应的旋转矩形。
    /// </summary>
    public readonly struct RotatedRect : IEquatable<RotatedRect>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RotatedRect"/> struct.
        /// 初始化 <see cref="RotatedRect"/> 结构的新实例。
        /// </summary>
        /// <param name="center">The rectangle center. 矩形中心。</param>
        /// <param name="size">The rectangle size. 矩形尺寸。</param>
        /// <param name="angle">The rotation angle in degrees. 旋转角度，单位为度。</param>
        public RotatedRect(Point2f center, Size2f size, float angle)
        {
            Center = center;
            Size = size;
            Angle = angle;
        }

        /// <summary>
        /// Gets the rectangle center.
        /// 获取矩形中心。
        /// </summary>
        public Point2f Center { get; }

        /// <summary>
        /// Gets the rectangle size.
        /// 获取矩形尺寸。
        /// </summary>
        public Size2f Size { get; }

        /// <summary>
        /// Gets the rotation angle in degrees.
        /// 获取旋转角度，单位为度。
        /// </summary>
        public float Angle { get; }

        /// <summary>
        /// Gets the rectangle area.
        /// 获取矩形面积。
        /// </summary>
        public float Area
        {
            get { return Size.Area; }
        }

        /// <summary>
        /// Determines whether two rotated rectangles are equal.
        /// 判断两个旋转矩形是否相等。
        /// </summary>
        /// <param name="left">The first rotated rectangle. 第一个旋转矩形。</param>
        /// <param name="right">The second rotated rectangle. 第二个旋转矩形。</param>
        /// <returns><c>true</c> if center, size, and angle are equal; otherwise, <c>false</c>. 如果中心、尺寸和角度都相等则返回 <c>true</c>，否则返回 <c>false</c>。</returns>
        public static bool operator ==(RotatedRect left, RotatedRect right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two rotated rectangles are different.
        /// 判断两个旋转矩形是否不同。
        /// </summary>
        /// <param name="left">The first rotated rectangle. 第一个旋转矩形。</param>
        /// <param name="right">The second rotated rectangle. 第二个旋转矩形。</param>
        /// <returns><c>true</c> if any field differs; otherwise, <c>false</c>. 如果任一字段不同则返回 <c>true</c>，否则返回 <c>false</c>。</returns>
        public static bool operator !=(RotatedRect left, RotatedRect right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Indicates whether this rectangle equals another rectangle.
        /// 指示此矩形是否与另一个矩形相等。
        /// </summary>
        /// <param name="other">The other rotated rectangle. 另一个旋转矩形。</param>
        /// <returns><c>true</c> if center, size, and angle are equal; otherwise, <c>false</c>. 如果中心、尺寸和角度都相等则返回 <c>true</c>，否则返回 <c>false</c>。</returns>
        public bool Equals(RotatedRect other)
        {
            return Center.Equals(other.Center) && Size.Equals(other.Size) && Angle.Equals(other.Angle);
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is RotatedRect other && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = Center.GetHashCode();
                hash = (hash * 397) ^ Size.GetHashCode();
                hash = (hash * 397) ^ Angle.GetHashCode();
                return hash;
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "{Center=" + Center +
                ",Size=" + Size +
                ",Angle=" + Angle.ToString(CultureInfo.InvariantCulture) + "}";
        }
    }
}
