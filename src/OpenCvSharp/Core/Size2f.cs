using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Core
{
    /// <summary>
    /// Represents a two-dimensional single-precision size compatible with OpenCV's <c>cv::Size2f</c>.
    /// 表示与 OpenCV <c>cv::Size2f</c> 对应的二维单精度浮点尺寸。
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Size2f : IEquatable<Size2f>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Size2f"/> struct.
        /// 初始化 <see cref="Size2f"/> 结构的新实例。
        /// </summary>
        /// <param name="width">The width. 宽度。</param>
        /// <param name="height">The height. 高度。</param>
        public Size2f(float width, float height)
        {
            Width = width;
            Height = height;
        }

        /// <summary>
        /// Gets the width.
        /// 获取宽度。
        /// </summary>
        public float Width { get; }

        /// <summary>
        /// Gets the height.
        /// 获取高度。
        /// </summary>
        public float Height { get; }

        /// <summary>
        /// Gets the area.
        /// 获取面积。
        /// </summary>
        public float Area
        {
            get { return Width * Height; }
        }

        /// <summary>
        /// Gets a value indicating whether the size has non-positive width or height.
        /// 获取尺寸是否具有非正宽度或高度。
        /// </summary>
        public bool Empty
        {
            get { return Width <= 0 || Height <= 0; }
        }

        /// <summary>
        /// Determines whether two sizes are equal.
        /// 判断两个尺寸是否相等。
        /// </summary>
        /// <param name="left">The first size. 第一个尺寸。</param>
        /// <param name="right">The second size. 第二个尺寸。</param>
        /// <returns><c>true</c> if both dimensions are equal; otherwise, <c>false</c>. 如果两个维度相等则返回 <c>true</c>，否则返回 <c>false</c>。</returns>
        public static bool operator ==(Size2f left, Size2f right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two sizes are different.
        /// 判断两个尺寸是否不同。
        /// </summary>
        /// <param name="left">The first size. 第一个尺寸。</param>
        /// <param name="right">The second size. 第二个尺寸。</param>
        /// <returns><c>true</c> if any dimension differs; otherwise, <c>false</c>. 如果任一维度不同则返回 <c>true</c>，否则返回 <c>false</c>。</returns>
        public static bool operator !=(Size2f left, Size2f right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Indicates whether this size equals another size.
        /// 指示此尺寸是否与另一个尺寸相等。
        /// </summary>
        /// <param name="other">The other size. 另一个尺寸。</param>
        /// <returns><c>true</c> if both dimensions are equal; otherwise, <c>false</c>. 如果两个维度相等则返回 <c>true</c>，否则返回 <c>false</c>。</returns>
        public bool Equals(Size2f other)
        {
            return Width.Equals(other.Width) && Height.Equals(other.Height);
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is Size2f other && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                return (Width.GetHashCode() * 397) ^ Height.GetHashCode();
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "{Width=" + Width.ToString(CultureInfo.InvariantCulture) +
                ",Height=" + Height.ToString(CultureInfo.InvariantCulture) + "}";
        }
    }
}
