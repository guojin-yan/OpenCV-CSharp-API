using System;
using System.Runtime.InteropServices;
using JYPPX.OpenCvSharp.Core;

namespace JYPPX.OpenCvSharp.Video
{
    /// <summary>
    /// Result returned by mean-shift tracking.
    /// mean-shift 跟踪返回的结果。
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct MeanShiftResult : IEquatable<MeanShiftResult>
    {
        /// <summary>
        /// Initializes a new result.
        /// 初始化结果。
        /// </summary>
        public MeanShiftResult(int iterations, Rect window)
        {
            if (iterations < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(iterations));
            }

            Iterations = iterations;
            Window = window;
        }

        /// <summary>
        /// Gets the number of iterations performed.
        /// 获取执行的迭代次数。
        /// </summary>
        public int Iterations { get; }

        /// <summary>
        /// Gets the updated search window.
        /// 获取更新后的搜索窗口。
        /// </summary>
        public Rect Window { get; }

        /// <summary>
        /// Determines whether two results are equal.
        /// 判断两个结果是否相等。
        /// </summary>
        public static bool operator ==(MeanShiftResult left, MeanShiftResult right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two results are different.
        /// 判断两个结果是否不同。
        /// </summary>
        public static bool operator !=(MeanShiftResult left, MeanShiftResult right)
        {
            return !left.Equals(right);
        }

        /// <inheritdoc/>
        public bool Equals(MeanShiftResult other)
        {
            return Iterations == other.Iterations &&
                Window.Equals(other.Window);
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is MeanShiftResult other && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                return (Iterations * 397) ^ Window.GetHashCode();
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "{Iterations=" + Iterations + ",Window=" + Window + "}";
        }
    }
}
