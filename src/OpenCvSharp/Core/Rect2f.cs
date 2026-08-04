using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Core
{
    /// <summary>
    /// Represents a single-precision rectangle compatible with OpenCV's <c>cv::Rect2f</c>.
    /// 表示与 OpenCV <c>cv::Rect2f</c> 对应的单精度浮点矩形。
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Rect2f : IEquatable<Rect2f>
    {
        /// <summary>Initializes a rectangle. 初始化矩形。</summary>
        public Rect2f(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        /// <summary>Gets the left coordinate. 获取左边界坐标。</summary>
        public float X { get; }

        /// <summary>Gets the top coordinate. 获取上边界坐标。</summary>
        public float Y { get; }

        /// <summary>Gets the width. 获取宽度。</summary>
        public float Width { get; }

        /// <summary>Gets the height. 获取高度。</summary>
        public float Height { get; }

        /// <summary>Gets the right coordinate. 获取右边界坐标。</summary>
        public float Right { get { return X + Width; } }

        /// <summary>Gets the bottom coordinate. 获取下边界坐标。</summary>
        public float Bottom { get { return Y + Height; } }

        /// <summary>Gets whether the rectangle has non-positive dimensions. 获取矩形是否具有非正尺寸。</summary>
        public bool Empty { get { return Width <= 0.0F || Height <= 0.0F; } }

        /// <summary>Tests whether the rectangle contains a point. 测试矩形是否包含指定点。</summary>
        public bool Contains(Point2f point)
        {
            return point.X >= X && point.X < Right && point.Y >= Y && point.Y < Bottom;
        }

        /// <inheritdoc/>
        public bool Equals(Rect2f other)
        {
            return X.Equals(other.X) && Y.Equals(other.Y) &&
                Width.Equals(other.Width) && Height.Equals(other.Height);
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is Rect2f other && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = X.GetHashCode();
                hash = (hash * 397) ^ Y.GetHashCode();
                hash = (hash * 397) ^ Width.GetHashCode();
                return (hash * 397) ^ Height.GetHashCode();
            }
        }

        /// <summary>Determines whether two rectangles are equal. 判断两个矩形是否相等。</summary>
        public static bool operator ==(Rect2f left, Rect2f right) { return left.Equals(right); }

        /// <summary>Determines whether two rectangles differ. 判断两个矩形是否不同。</summary>
        public static bool operator !=(Rect2f left, Rect2f right) { return !left.Equals(right); }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "{X=" + X.ToString(CultureInfo.InvariantCulture)
                + ",Y=" + Y.ToString(CultureInfo.InvariantCulture)
                + ",Width=" + Width.ToString(CultureInfo.InvariantCulture)
                + ",Height=" + Height.ToString(CultureInfo.InvariantCulture) + "}";
        }
    }
}
