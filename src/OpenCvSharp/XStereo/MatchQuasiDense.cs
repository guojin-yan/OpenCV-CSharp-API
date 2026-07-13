using System;
using System.Globalization;
using System.Runtime.InteropServices;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.XStereo
{
    /// <summary>
    /// One quasi-dense stereo correspondence.
    /// 一个 quasi-dense stereo 对应点。
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct MatchQuasiDense : IEquatable<MatchQuasiDense>
    {
        /// <summary>Initializes a quasi-dense match. 初始化 quasi-dense match。</summary>
        public MatchQuasiDense(Point p0, Point p1, float correlation)
        {
            P0 = p0;
            P1 = p1;
            Correlation = correlation;
        }

        /// <summary>Gets the point in the left image. 获取左图点。</summary>
        public Point P0 { get; }

        /// <summary>Gets the point in the right image. 获取右图点。</summary>
        public Point P1 { get; }

        /// <summary>Gets the correlation value. 获取相关性值。</summary>
        public float Correlation { get; }

        /// <summary>Determines whether two matches are equal. 判断两个匹配是否相等。</summary>
        public static bool operator ==(MatchQuasiDense left, MatchQuasiDense right)
        {
            return left.Equals(right);
        }

        /// <summary>Determines whether two matches are different. 判断两个匹配是否不同。</summary>
        public static bool operator !=(MatchQuasiDense left, MatchQuasiDense right)
        {
            return !left.Equals(right);
        }

        /// <summary>Indicates whether this match equals another match. 指示此匹配是否与另一个匹配相等。</summary>
        public bool Equals(MatchQuasiDense other)
        {
            return P0.Equals(other.P0) && P1.Equals(other.P1) && Correlation.Equals(other.Correlation);
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is MatchQuasiDense other && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = P0.GetHashCode();
                hash = (hash * 397) ^ P1.GetHashCode();
                hash = (hash * 397) ^ Correlation.GetHashCode();
                return hash;
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{{P0={0},P1={1},Correlation={2}}}",
                P0,
                P1,
                Correlation);
        }

        internal static MatchQuasiDense FromNative(NativeXStereoMatchQuasiDense value)
        {
            return new MatchQuasiDense(
                new Point(value.P0X, value.P0Y),
                new Point(value.P1X, value.P1Y),
                value.Corr);
        }
    }
}
