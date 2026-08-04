using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Face
{
    /// <summary>
    /// Represents a collector prediction result item.
    /// 表示 collector 中的一条预测结果。
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct FacePredictionResult : IEquatable<FacePredictionResult>
    {
        /// <summary>Initializes a result item. 初始化结果项。</summary>
        public FacePredictionResult(int label, double distance)
        {
            if (distance < 0.0)
            {
                throw new ArgumentOutOfRangeException(nameof(distance));
            }

            Label = label;
            Distance = distance;
        }

        /// <summary>Gets the label. 获取标签。</summary>
        public int Label { get; }

        /// <summary>Gets the distance. 获取距离。</summary>
        public double Distance { get; }

        /// <summary>Determines whether two result items are equal. 判断两个结果项是否相等。</summary>
        public static bool operator ==(FacePredictionResult left, FacePredictionResult right)
        {
            return left.Equals(right);
        }

        /// <summary>Determines whether two result items are different. 判断两个结果项是否不同。</summary>
        public static bool operator !=(FacePredictionResult left, FacePredictionResult right)
        {
            return !left.Equals(right);
        }

        /// <summary>Indicates whether this result item equals another result item. 指示此结果项是否与另一个结果项相等。</summary>
        public bool Equals(FacePredictionResult other)
        {
            return Label == other.Label && Distance.Equals(other.Distance);
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is FacePredictionResult other && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                return (Label * 397) ^ Distance.GetHashCode();
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{{Label={0},Distance={1}}}",
                Label,
                Distance);
        }
    }
}
