using System;
using System.Globalization;
using System.Runtime.InteropServices;
using JYPPX.OpenCvSharp.Core;

namespace JYPPX.OpenCvSharp.XImgProc
{
    /// <summary>
    /// Represents an EdgeBoxes object proposal and its score.
    /// 表示 EdgeBoxes 目标候选框及其分数。
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct EdgeBox : IEquatable<EdgeBox>
    {
        /// <summary>Initializes a new proposal value. 初始化候选框值。</summary>
        public EdgeBox(Rect rectangle, float score)
        {
            Rectangle = rectangle;
            Score = score;
        }

        /// <summary>Gets the proposal rectangle. 获取候选矩形。</summary>
        public Rect Rectangle { get; }

        /// <summary>Gets the proposal score. 获取候选分数。</summary>
        public float Score { get; }

        /// <summary>Determines whether two boxes are equal. 判断两个候选框是否相等。</summary>
        public static bool operator ==(EdgeBox left, EdgeBox right)
        {
            return left.Equals(right);
        }

        /// <summary>Determines whether two boxes are different. 判断两个候选框是否不同。</summary>
        public static bool operator !=(EdgeBox left, EdgeBox right)
        {
            return !left.Equals(right);
        }

        /// <summary>Indicates whether this box equals another box. 指示此候选框是否与另一个候选框相等。</summary>
        public bool Equals(EdgeBox other)
        {
            return Rectangle.Equals(other.Rectangle) && Score.Equals(other.Score);
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is EdgeBox other && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                return (Rectangle.GetHashCode() * 397) ^ Score.GetHashCode();
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{{Rectangle={{X={0},Y={1},Width={2},Height={3}}},Score={4}}}",
                Rectangle.X,
                Rectangle.Y,
                Rectangle.Width,
                Rectangle.Height,
                Score);
        }
    }
}
