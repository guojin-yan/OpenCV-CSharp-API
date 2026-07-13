using System;
using System.Runtime.InteropServices;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.BioInspired
{
    /// <summary>
    /// Parameters for transient areas segmentation.
    /// transient areas 分割参数。
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct SegmentationParameters : IEquatable<SegmentationParameters>
    {
        /// <summary>Initializes segmentation parameters. 初始化分割参数。</summary>
        public SegmentationParameters(
            float thresholdOn = 0.25f,
            float thresholdOff = 0.25f,
            float localEnergyTemporalConstant = 0.5f,
            float localEnergySpatialConstant = 5.0f,
            float neighborhoodEnergyTemporalConstant = 1.0f,
            float neighborhoodEnergySpatialConstant = 7.0f,
            float contextEnergyTemporalConstant = 1.0f,
            float contextEnergySpatialConstant = 7.0f)
        {
            ThresholdOn = thresholdOn;
            ThresholdOff = thresholdOff;
            LocalEnergyTemporalConstant = localEnergyTemporalConstant;
            LocalEnergySpatialConstant = localEnergySpatialConstant;
            NeighborhoodEnergyTemporalConstant = neighborhoodEnergyTemporalConstant;
            NeighborhoodEnergySpatialConstant = neighborhoodEnergySpatialConstant;
            ContextEnergyTemporalConstant = contextEnergyTemporalConstant;
            ContextEnergySpatialConstant = contextEnergySpatialConstant;
        }

        /// <summary>Gets positive-event threshold. 获取正向事件阈值。</summary>
        public float ThresholdOn { get; }

        /// <summary>Gets negative-event threshold. 获取负向事件阈值。</summary>
        public float ThresholdOff { get; }

        /// <summary>Gets local energy temporal constant. 获取局部能量时间常数。</summary>
        public float LocalEnergyTemporalConstant { get; }

        /// <summary>Gets local energy spatial constant. 获取局部能量空间常数。</summary>
        public float LocalEnergySpatialConstant { get; }

        /// <summary>Gets neighborhood energy temporal constant. 获取邻域能量时间常数。</summary>
        public float NeighborhoodEnergyTemporalConstant { get; }

        /// <summary>Gets neighborhood energy spatial constant. 获取邻域能量空间常数。</summary>
        public float NeighborhoodEnergySpatialConstant { get; }

        /// <summary>Gets context energy temporal constant. 获取上下文能量时间常数。</summary>
        public float ContextEnergyTemporalConstant { get; }

        /// <summary>Gets context energy spatial constant. 获取上下文能量空间常数。</summary>
        public float ContextEnergySpatialConstant { get; }

        /// <summary>Determines whether two parameter values are equal. 判断两个参数值是否相等。</summary>
        public static bool operator ==(SegmentationParameters left, SegmentationParameters right)
        {
            return left.Equals(right);
        }

        /// <summary>Determines whether two parameter values are different. 判断两个参数值是否不同。</summary>
        public static bool operator !=(SegmentationParameters left, SegmentationParameters right)
        {
            return !left.Equals(right);
        }

        /// <summary>Creates default parameters. 创建默认参数。</summary>
        public static SegmentationParameters Default
        {
            get { return new SegmentationParameters(); }
        }

        internal NativeBioInspiredSegmentationParameters ToNative()
        {
            return new NativeBioInspiredSegmentationParameters
            {
                ThresholdOn = ThresholdOn,
                ThresholdOff = ThresholdOff,
                LocalEnergyTemporalConstant = LocalEnergyTemporalConstant,
                LocalEnergySpatialConstant = LocalEnergySpatialConstant,
                NeighborhoodEnergyTemporalConstant = NeighborhoodEnergyTemporalConstant,
                NeighborhoodEnergySpatialConstant = NeighborhoodEnergySpatialConstant,
                ContextEnergyTemporalConstant = ContextEnergyTemporalConstant,
                ContextEnergySpatialConstant = ContextEnergySpatialConstant
            };
        }

        internal static SegmentationParameters FromNative(NativeBioInspiredSegmentationParameters value)
        {
            return new SegmentationParameters(
                value.ThresholdOn,
                value.ThresholdOff,
                value.LocalEnergyTemporalConstant,
                value.LocalEnergySpatialConstant,
                value.NeighborhoodEnergyTemporalConstant,
                value.NeighborhoodEnergySpatialConstant,
                value.ContextEnergyTemporalConstant,
                value.ContextEnergySpatialConstant);
        }

        /// <inheritdoc />
        public bool Equals(SegmentationParameters other)
        {
            return ThresholdOn.Equals(other.ThresholdOn) &&
                ThresholdOff.Equals(other.ThresholdOff) &&
                LocalEnergyTemporalConstant.Equals(other.LocalEnergyTemporalConstant) &&
                LocalEnergySpatialConstant.Equals(other.LocalEnergySpatialConstant) &&
                NeighborhoodEnergyTemporalConstant.Equals(other.NeighborhoodEnergyTemporalConstant) &&
                NeighborhoodEnergySpatialConstant.Equals(other.NeighborhoodEnergySpatialConstant) &&
                ContextEnergyTemporalConstant.Equals(other.ContextEnergyTemporalConstant) &&
                ContextEnergySpatialConstant.Equals(other.ContextEnergySpatialConstant);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is SegmentationParameters other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = ThresholdOn.GetHashCode();
                hashCode = (hashCode * 397) ^ ThresholdOff.GetHashCode();
                hashCode = (hashCode * 397) ^ LocalEnergyTemporalConstant.GetHashCode();
                hashCode = (hashCode * 397) ^ LocalEnergySpatialConstant.GetHashCode();
                hashCode = (hashCode * 397) ^ NeighborhoodEnergyTemporalConstant.GetHashCode();
                hashCode = (hashCode * 397) ^ NeighborhoodEnergySpatialConstant.GetHashCode();
                hashCode = (hashCode * 397) ^ ContextEnergyTemporalConstant.GetHashCode();
                hashCode = (hashCode * 397) ^ ContextEnergySpatialConstant.GetHashCode();
                return hashCode;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format(
                System.Globalization.CultureInfo.InvariantCulture,
                "SegmentationParameters(ThresholdOn={0}, ThresholdOff={1}, LocalEnergyTemporalConstant={2}, LocalEnergySpatialConstant={3}, NeighborhoodEnergyTemporalConstant={4}, NeighborhoodEnergySpatialConstant={5}, ContextEnergyTemporalConstant={6}, ContextEnergySpatialConstant={7})",
                ThresholdOn,
                ThresholdOff,
                LocalEnergyTemporalConstant,
                LocalEnergySpatialConstant,
                NeighborhoodEnergyTemporalConstant,
                NeighborhoodEnergySpatialConstant,
                ContextEnergyTemporalConstant,
                ContextEnergySpatialConstant);
        }
    }
}
