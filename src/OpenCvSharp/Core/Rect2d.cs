using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Core
{
    /// <summary>
    /// Represents a double-precision rectangle compatible with OpenCV's <c>cv::Rect2d</c>.
    /// 表示与 OpenCV <c>cv::Rect2d</c> 对应的双精度浮点矩形。
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Rect2d : IEquatable<Rect2d>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Rect2d"/> struct.
        /// 初始化 <see cref="Rect2d"/> 结构的新实例。
        /// </summary>
        public Rect2d(double x, double y, double width, double height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        /// <summary>
        /// Initializes a new instance from a location and size.
        /// 使用位置和尺寸初始化新实例。
        /// </summary>
        public Rect2d(Point2d location, Size2d size)
            : this(location.X, location.Y, size.Width, size.Height)
        {
        }

        /// <summary>Gets the x coordinate of the top-left corner. 获取左上角 X 坐标。</summary>
        public double X { get; }

        /// <summary>Gets the y coordinate of the top-left corner. 获取左上角 Y 坐标。</summary>
        public double Y { get; }

        /// <summary>Gets the rectangle width. 获取矩形宽度。</summary>
        public double Width { get; }

        /// <summary>Gets the rectangle height. 获取矩形高度。</summary>
        public double Height { get; }

        /// <summary>Gets the left edge coordinate. 获取左边界坐标。</summary>
        public double Left { get { return X; } }

        /// <summary>Gets the top edge coordinate. 获取上边界坐标。</summary>
        public double Top { get { return Y; } }

        /// <summary>Gets the right edge coordinate. 获取右边界坐标。</summary>
        public double Right { get { return X + Width; } }

        /// <summary>Gets the bottom edge coordinate. 获取下边界坐标。</summary>
        public double Bottom { get { return Y + Height; } }

        /// <summary>Gets the rectangle size. 获取矩形尺寸。</summary>
        public Size2d Size { get { return new Size2d(Width, Height); } }

        /// <summary>Gets the top-left location. 获取左上角位置。</summary>
        public Point2d Location { get { return new Point2d(X, Y); } }

        /// <summary>Gets the rectangle area. 获取矩形面积。</summary>
        public double Area { get { return Width * Height; } }

        /// <summary>Gets whether the rectangle has non-positive width or height. 获取矩形是否为空。</summary>
        public bool Empty { get { return Width <= 0.0 || Height <= 0.0; } }

        /// <summary>
        /// Determines whether the rectangle contains the specified point.
        /// 判断矩形是否包含指定点。
        /// </summary>
        public bool Contains(Point2d point)
        {
            return Contains(point.X, point.Y);
        }

        /// <summary>
        /// Determines whether the rectangle contains the specified coordinates.
        /// 判断矩形是否包含指定坐标。
        /// </summary>
        public bool Contains(double x, double y)
        {
            return x >= X && x < Right && y >= Y && y < Bottom;
        }

        /// <summary>Determines whether two rectangles are equal. 判断两个矩形是否相等。</summary>
        public static bool operator ==(Rect2d left, Rect2d right)
        {
            return left.Equals(right);
        }

        /// <summary>Determines whether two rectangles are different. 判断两个矩形是否不同。</summary>
        public static bool operator !=(Rect2d left, Rect2d right)
        {
            return !left.Equals(right);
        }

        /// <inheritdoc/>
        public bool Equals(Rect2d other)
        {
            return X.Equals(other.X) &&
                Y.Equals(other.Y) &&
                Width.Equals(other.Width) &&
                Height.Equals(other.Height);
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is Rect2d other && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = X.GetHashCode();
                hashCode = (hashCode * 397) ^ Y.GetHashCode();
                hashCode = (hashCode * 397) ^ Width.GetHashCode();
                hashCode = (hashCode * 397) ^ Height.GetHashCode();
                return hashCode;
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "{X=" + X.ToString(CultureInfo.InvariantCulture) +
                ",Y=" + Y.ToString(CultureInfo.InvariantCulture) +
                ",Width=" + Width.ToString(CultureInfo.InvariantCulture) +
                ",Height=" + Height.ToString(CultureInfo.InvariantCulture) + "}";
        }
    }
}
