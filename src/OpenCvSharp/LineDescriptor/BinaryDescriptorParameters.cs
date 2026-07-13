using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.LineDescriptor
{
    /// <summary>
    /// Parameters used to create a line-descriptor binary descriptor.
    /// 用于创建 line_descriptor 二进制描述子的参数。
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct BinaryDescriptorParameters : IEquatable<BinaryDescriptorParameters>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryDescriptorParameters"/> struct.
        /// 初始化 <see cref="BinaryDescriptorParameters"/> 结构的新实例。
        /// </summary>
        public BinaryDescriptorParameters(int numOfOctaves, int widthOfBand, int reductionRatio, int ksize)
        {
            NumOfOctaves = numOfOctaves;
            WidthOfBand = widthOfBand;
            ReductionRatio = reductionRatio;
            KSize = ksize;
        }

        /// <summary>Gets OpenCV's default binary descriptor parameters. 获取 OpenCV 默认二进制描述子参数。</summary>
        public static BinaryDescriptorParameters Default
        {
            get { return new BinaryDescriptorParameters(1, 7, 2, 5); }
        }

        /// <summary>Gets the number of octaves. 获取 octave 数量。</summary>
        public int NumOfOctaves { get; }

        /// <summary>Gets the descriptor support band width. 获取描述子支撑带宽。</summary>
        public int WidthOfBand { get; }

        /// <summary>Gets the reduction ratio. 获取降采样比例。</summary>
        public int ReductionRatio { get; }

        /// <summary>Gets the Gaussian kernel size used internally by OpenCV. 获取 OpenCV 内部使用的 Gaussian kernel 尺寸。</summary>
        public int KSize { get; }

        /// <summary>Determines whether two parameter values are equal. 判断两个参数值是否相等。</summary>
        public static bool operator ==(BinaryDescriptorParameters left, BinaryDescriptorParameters right)
        {
            return left.Equals(right);
        }

        /// <summary>Determines whether two parameter values are different. 判断两个参数值是否不同。</summary>
        public static bool operator !=(BinaryDescriptorParameters left, BinaryDescriptorParameters right)
        {
            return !left.Equals(right);
        }

        /// <summary>Validates parameter ranges. 验证参数范围。</summary>
        public void Validate()
        {
            if (NumOfOctaves <= 0) { throw new ArgumentOutOfRangeException(nameof(NumOfOctaves)); }
            if (WidthOfBand <= 0) { throw new ArgumentOutOfRangeException(nameof(WidthOfBand)); }
            if (ReductionRatio <= 0) { throw new ArgumentOutOfRangeException(nameof(ReductionRatio)); }
            if (KSize <= 0) { throw new ArgumentOutOfRangeException(nameof(KSize)); }
        }

        /// <summary>Indicates whether this value equals another value. 指示此值是否与另一个值相等。</summary>
        public bool Equals(BinaryDescriptorParameters other)
        {
            return NumOfOctaves == other.NumOfOctaves
                && WidthOfBand == other.WidthOfBand
                && ReductionRatio == other.ReductionRatio
                && KSize == other.KSize;
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is BinaryDescriptorParameters other && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = NumOfOctaves;
                hash = (hash * 397) ^ WidthOfBand;
                hash = (hash * 397) ^ ReductionRatio;
                hash = (hash * 397) ^ KSize;
                return hash;
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "{NumOfOctaves=" + NumOfOctaves + ",WidthOfBand=" + WidthOfBand + ",ReductionRatio=" + ReductionRatio + ",KSize=" + KSize + "}";
        }
    }
}
