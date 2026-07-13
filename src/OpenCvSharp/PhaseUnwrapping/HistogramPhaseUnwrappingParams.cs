using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace OpenCvSharp.PhaseUnwrapping
{
    /// <summary>
    /// Parameters for OpenCV histogram-based phase unwrapping.
    /// OpenCV 基于直方图的相位展开参数。
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct HistogramPhaseUnwrappingParams : IEquatable<HistogramPhaseUnwrappingParams>
    {
        /// <summary>
        /// Initializes a new parameter set.
        /// 初始化一组新参数。
        /// </summary>
        public HistogramPhaseUnwrappingParams(int width, int height, float histThresh, int nbrOfSmallBins, int nbrOfLargeBins)
        {
            Width = width;
            Height = height;
            HistThresh = histThresh;
            NbrOfSmallBins = nbrOfSmallBins;
            NbrOfLargeBins = nbrOfLargeBins;
        }

        /// <summary>Gets OpenCV's default parameter set. 获取 OpenCV 默认参数。</summary>
        public static HistogramPhaseUnwrappingParams Default
        {
            get
            {
                return new HistogramPhaseUnwrappingParams(
                    800,
                    600,
                    (float)(3.0 * Math.PI * Math.PI),
                    10,
                    5);
            }
        }

        /// <summary>Gets the phase map width. 获取相位图宽度。</summary>
        public int Width { get; }

        /// <summary>Gets the phase map height. 获取相位图高度。</summary>
        public int Height { get; }

        /// <summary>Gets the histogram threshold. 获取直方图阈值。</summary>
        public float HistThresh { get; }

        /// <summary>Gets the number of small bins. 获取小 bin 数量。</summary>
        public int NbrOfSmallBins { get; }

        /// <summary>Gets the number of large bins. 获取大 bin 数量。</summary>
        public int NbrOfLargeBins { get; }

        /// <summary>Determines whether two parameter values are equal. 判断两个参数值是否相等。</summary>
        public static bool operator ==(HistogramPhaseUnwrappingParams left, HistogramPhaseUnwrappingParams right)
        {
            return left.Equals(right);
        }

        /// <summary>Determines whether two parameter values are different. 判断两个参数值是否不同。</summary>
        public static bool operator !=(HistogramPhaseUnwrappingParams left, HistogramPhaseUnwrappingParams right)
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

            if (HistThresh <= 0.0F || float.IsNaN(HistThresh) || float.IsInfinity(HistThresh))
            {
                throw new ArgumentOutOfRangeException(nameof(HistThresh), "Histogram threshold must be a finite positive value.");
            }

            if (NbrOfSmallBins <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(NbrOfSmallBins), "Small bin count must be positive.");
            }

            if (NbrOfLargeBins <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(NbrOfLargeBins), "Large bin count must be positive.");
            }
        }

        /// <summary>Indicates whether this value equals another value. 指示此值是否与另一个值相等。</summary>
        public bool Equals(HistogramPhaseUnwrappingParams other)
        {
            return Width == other.Width
                && Height == other.Height
                && HistThresh.Equals(other.HistThresh)
                && NbrOfSmallBins == other.NbrOfSmallBins
                && NbrOfLargeBins == other.NbrOfLargeBins;
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is HistogramPhaseUnwrappingParams other && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = Width;
                hash = (hash * 397) ^ Height;
                hash = (hash * 397) ^ HistThresh.GetHashCode();
                hash = (hash * 397) ^ NbrOfSmallBins;
                hash = (hash * 397) ^ NbrOfLargeBins;
                return hash;
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{{Width={0},Height={1},HistThresh={2},NbrOfSmallBins={3},NbrOfLargeBins={4}}}",
                Width,
                Height,
                HistThresh,
                NbrOfSmallBins,
                NbrOfLargeBins);
        }
    }
}
