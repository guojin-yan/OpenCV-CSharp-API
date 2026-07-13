using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Face
{
    /// <summary>
    /// Represents a face recognizer prediction.
    /// 表示人脸识别器预测结果。
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct FacePrediction : IEquatable<FacePrediction>
    {
        /// <summary>Initializes a prediction. 初始化预测结果。</summary>
        public FacePrediction(int label, double confidence)
        {
            Label = label;
            Confidence = confidence;
        }

        /// <summary>Gets the predicted label. 获取预测标签。</summary>
        public int Label { get; }

        /// <summary>Gets the prediction confidence or distance. 获取预测置信度或距离。</summary>
        public double Confidence { get; }

        /// <summary>Determines whether two predictions are equal. 判断两个预测结果是否相等。</summary>
        public static bool operator ==(FacePrediction left, FacePrediction right)
        {
            return left.Equals(right);
        }

        /// <summary>Determines whether two predictions are different. 判断两个预测结果是否不同。</summary>
        public static bool operator !=(FacePrediction left, FacePrediction right)
        {
            return !left.Equals(right);
        }

        /// <summary>Indicates whether this prediction equals another prediction. 指示此预测结果是否与另一个预测结果相等。</summary>
        public bool Equals(FacePrediction other)
        {
            return Label == other.Label && Confidence.Equals(other.Confidence);
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is FacePrediction other && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                return (Label * 397) ^ Confidence.GetHashCode();
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{{Label={0},Confidence={1}}}",
                Label,
                Confidence);
        }
    }
}
