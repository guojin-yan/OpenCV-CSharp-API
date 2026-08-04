using System;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Core
{
    /// <summary>
    /// Represents an integer rectangle compatible with OpenCV's <c>cv::Rect</c>.
    /// 表示与 OpenCV <c>cv::Rect</c> 对应的整数矩形。
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Rect : IEquatable<Rect>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Rect"/> struct.
        /// 初始化 <see cref="Rect"/> 结构的新实例。
        /// </summary>
        /// <param name="x">The x coordinate of the top-left corner. 左上角 X 坐标。</param>
        /// <param name="y">The y coordinate of the top-left corner. 左上角 Y 坐标。</param>
        /// <param name="width">The rectangle width. 矩形宽度。</param>
        /// <param name="height">The rectangle height. 矩形高度。</param>
        public Rect(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Rect"/> struct from location and size.
        /// 使用位置和尺寸初始化 <see cref="Rect"/> 结构的新实例。
        /// </summary>
        /// <param name="location">The top-left location. 左上角位置。</param>
        /// <param name="size">The rectangle size. 矩形尺寸。</param>
        public Rect(Point location, Size size)
            : this(location.X, location.Y, size.Width, size.Height)
        {
        }

        /// <summary>
        /// Gets the x coordinate of the top-left corner.
        /// 获取左上角 X 坐标。
        /// </summary>
        public int X { get; }

        /// <summary>
        /// Gets the y coordinate of the top-left corner.
        /// 获取左上角 Y 坐标。
        /// </summary>
        public int Y { get; }

        /// <summary>
        /// Gets the rectangle width.
        /// 获取矩形宽度。
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// Gets the rectangle height.
        /// 获取矩形高度。
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// Gets the left edge coordinate.
        /// 获取左边界坐标。
        /// </summary>
        public int Left
        {
            get { return X; }
        }

        /// <summary>
        /// Gets the top edge coordinate.
        /// 获取上边界坐标。
        /// </summary>
        public int Top
        {
            get { return Y; }
        }

        /// <summary>
        /// Gets the right edge coordinate.
        /// 获取右边界坐标。
        /// </summary>
        public int Right
        {
            get { return X + Width; }
        }

        /// <summary>
        /// Gets the bottom edge coordinate.
        /// 获取下边界坐标。
        /// </summary>
        public int Bottom
        {
            get { return Y + Height; }
        }

        /// <summary>
        /// Gets the rectangle size.
        /// 获取矩形尺寸。
        /// </summary>
        public Size Size
        {
            get { return new Size(Width, Height); }
        }

        /// <summary>
        /// Gets the top-left location.
        /// 获取左上角位置。
        /// </summary>
        public Point Location
        {
            get { return new Point(X, Y); }
        }

        /// <summary>
        /// Gets the rectangle area.
        /// 获取矩形面积。
        /// </summary>
        public int Area
        {
            get { return Width * Height; }
        }

        /// <summary>
        /// Gets a value indicating whether the rectangle has non-positive width or height.
        /// 获取矩形是否具有非正宽度或高度。
        /// </summary>
        public bool Empty
        {
            get { return Width <= 0 || Height <= 0; }
        }

        /// <summary>
        /// Determines whether the rectangle contains the specified point.
        /// 判断矩形是否包含指定点。
        /// </summary>
        /// <param name="point">The point to test. 要测试的点。</param>
        /// <returns><c>true</c> if the point is inside the rectangle; otherwise, <c>false</c>. 如果点在矩形内则返回 <c>true</c>，否则返回 <c>false</c>。</returns>
        public bool Contains(Point point)
        {
            return Contains(point.X, point.Y);
        }

        /// <summary>
        /// Determines whether the rectangle contains the specified point coordinates.
        /// 判断矩形是否包含指定点坐标。
        /// </summary>
        /// <param name="x">The x coordinate. X 坐标。</param>
        /// <param name="y">The y coordinate. Y 坐标。</param>
        /// <returns><c>true</c> if the point is inside the rectangle; otherwise, <c>false</c>. 如果点在矩形内则返回 <c>true</c>，否则返回 <c>false</c>。</returns>
        public bool Contains(int x, int y)
        {
            return x >= X && x < Right && y >= Y && y < Bottom;
        }

        /// <summary>
        /// Determines whether two rectangles are equal.
        /// 判断两个矩形是否相等。
        /// </summary>
        /// <param name="left">The first rectangle. 第一个矩形。</param>
        /// <param name="right">The second rectangle. 第二个矩形。</param>
        /// <returns><c>true</c> if all fields are equal; otherwise, <c>false</c>. 如果所有字段相等则返回 <c>true</c>，否则返回 <c>false</c>。</returns>
        public static bool operator ==(Rect left, Rect right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two rectangles are different.
        /// 判断两个矩形是否不同。
        /// </summary>
        /// <param name="left">The first rectangle. 第一个矩形。</param>
        /// <param name="right">The second rectangle. 第二个矩形。</param>
        /// <returns><c>true</c> if any field differs; otherwise, <c>false</c>. 如果任一字段不同则返回 <c>true</c>，否则返回 <c>false</c>。</returns>
        public static bool operator !=(Rect left, Rect right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Indicates whether this rectangle equals another rectangle.
        /// 指示此矩形是否与另一个矩形相等。
        /// </summary>
        /// <param name="other">The other rectangle. 另一个矩形。</param>
        /// <returns><c>true</c> if all fields are equal; otherwise, <c>false</c>. 如果所有字段相等则返回 <c>true</c>，否则返回 <c>false</c>。</returns>
        public bool Equals(Rect other)
        {
            return X == other.X &&
                Y == other.Y &&
                Width == other.Width &&
                Height == other.Height;
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is Rect other && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = X;
                hash = (hash * 397) ^ Y;
                hash = (hash * 397) ^ Width;
                hash = (hash * 397) ^ Height;
                return hash;
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "{X=" + X + ",Y=" + Y + ",Width=" + Width + ",Height=" + Height + "}";
        }
    }
}
