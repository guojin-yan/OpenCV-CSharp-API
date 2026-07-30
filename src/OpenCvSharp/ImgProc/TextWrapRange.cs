using System;

namespace OpenCvSharp.ImgProc
{
    /// <summary>Horizontal wrapping interval used by FontFace text rendering. FontFace 文本渲染使用的水平换行区间。</summary>
    public readonly struct TextWrapRange : IEquatable<TextWrapRange>
    {
        /// <summary>Creates a half-open horizontal wrapping interval. 创建半开水平换行区间。</summary>
        public TextWrapRange(int start, int end)
        {
            if (end < start)
            {
                throw new ArgumentOutOfRangeException(nameof(end), "End cannot be less than start.");
            }

            Start = start;
            End = end;
        }

        /// <summary>Gets the inclusive start coordinate. 获取包含的起始坐标。</summary>
        public int Start { get; }

        /// <summary>Gets the exclusive end coordinate. 获取不包含的结束坐标。</summary>
        public int End { get; }

        /// <inheritdoc/>
        public bool Equals(TextWrapRange other)
        {
            return Start == other.Start && End == other.End;
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is TextWrapRange other && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                return (Start * 397) ^ End;
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "[" + Start + ", " + End + ")";
        }
    }
}
