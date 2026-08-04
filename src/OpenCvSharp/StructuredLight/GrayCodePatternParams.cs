using System;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.StructuredLight
{
    /// <summary>
    /// Parameters for a Gray-code structured-light pattern.
    /// Gray-code 结构光图案参数。
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct GrayCodePatternParams : IEquatable<GrayCodePatternParams>
    {
        /// <summary>
        /// Initializes a new parameter set.
        /// 初始化一组新参数。
        /// </summary>
        public GrayCodePatternParams(int width, int height)
        {
            Width = width;
            Height = height;
        }

        /// <summary>Gets OpenCV's default parameter set. 获取 OpenCV 默认参数。</summary>
        public static GrayCodePatternParams Default
        {
            get { return new GrayCodePatternParams(1024, 768); }
        }

        /// <summary>Gets the projector width. 获取投影仪宽度。</summary>
        public int Width { get; }

        /// <summary>Gets the projector height. 获取投影仪高度。</summary>
        public int Height { get; }

        /// <summary>Determines whether two parameter values are equal. 判断两个参数值是否相等。</summary>
        public static bool operator ==(GrayCodePatternParams left, GrayCodePatternParams right)
        {
            return left.Equals(right);
        }

        /// <summary>Determines whether two parameter values are different. 判断两个参数值是否不同。</summary>
        public static bool operator !=(GrayCodePatternParams left, GrayCodePatternParams right)
        {
            return !left.Equals(right);
        }

        /// <summary>Validates parameter ranges. 验证参数范围。</summary>
        public void Validate()
        {
            if (Width <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(Width), "Width must be positive.");
            }

            if (Height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(Height), "Height must be positive.");
            }
        }

        /// <summary>Indicates whether this value equals another value. 指示此值是否与另一个值相等。</summary>
        public bool Equals(GrayCodePatternParams other)
        {
            return Width == other.Width && Height == other.Height;
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is GrayCodePatternParams other && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                return (Width * 397) ^ Height;
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "{Width=" + Width + ",Height=" + Height + "}";
        }
    }
}
