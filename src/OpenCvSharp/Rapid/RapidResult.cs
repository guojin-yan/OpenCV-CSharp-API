using System;
using System.Globalization;

namespace OpenCvSharp.Rapid
{
    /// <summary>
    /// Result returned by a RAPID iteration.
    /// RAPID 单次迭代返回结果。
    /// </summary>
    public readonly struct RapidResult : IEquatable<RapidResult>
    {
        /// <summary>Initializes the result. 初始化结果。</summary>
        public RapidResult(float ratio, double? rmsd)
        {
            if (float.IsNaN(ratio) || float.IsInfinity(ratio) || ratio < 0.0F || ratio > 1.0F)
            {
                throw new ArgumentOutOfRangeException(nameof(ratio));
            }

            if (rmsd.HasValue && (double.IsNaN(rmsd.Value) || double.IsInfinity(rmsd.Value) || rmsd.Value < 0.0))
            {
                throw new ArgumentOutOfRangeException(nameof(rmsd));
            }

            Ratio = ratio;
            Rmsd = rmsd;
        }

        /// <summary>Gets the ratio of matched search lines. 获取匹配搜索线比例。</summary>
        public float Ratio { get; }

        /// <summary>Gets the optional RMSD value. 获取可选 RMSD 值。</summary>
        public double? Rmsd { get; }

        /// <summary>Determines whether two results are equal. 判断两个结果是否相等。</summary>
        public static bool operator ==(RapidResult left, RapidResult right)
        {
            return left.Equals(right);
        }

        /// <summary>Determines whether two results are different. 判断两个结果是否不同。</summary>
        public static bool operator !=(RapidResult left, RapidResult right)
        {
            return !left.Equals(right);
        }

        /// <summary>Indicates whether this result equals another result. 指示此结果是否与另一个结果相等。</summary>
        public bool Equals(RapidResult other)
        {
            return Ratio.Equals(other.Ratio) && Nullable.Equals(Rmsd, other.Rmsd);
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is RapidResult other && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                return (Ratio.GetHashCode() * 397) ^ Rmsd.GetHashCode();
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{{Ratio={0},Rmsd={1}}}",
                Ratio,
                Rmsd.HasValue ? Rmsd.Value.ToString(CultureInfo.InvariantCulture) : "null");
        }
    }
}
