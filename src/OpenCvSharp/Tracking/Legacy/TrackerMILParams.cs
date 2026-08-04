using System;
using System.Globalization;
using System.Runtime.InteropServices;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Tracking.Legacy
{
    /// <summary>
    /// Parameters for OpenCV legacy MIL tracker.
    /// OpenCV legacy MIL 跟踪器参数。
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct TrackerMILParams : IEquatable<TrackerMILParams>
    {
        /// <summary>Initializes MIL parameters. 初始化 MIL 参数。</summary>
        public TrackerMILParams(
            float samplerInitInRadius,
            float samplerSearchWinSize,
            int samplerInitMaxNegNum,
            float samplerTrackInRadius,
            int samplerTrackMaxPosNum,
            int samplerTrackMaxNegNum,
            int featureSetNumFeatures)
        {
            SamplerInitInRadius = samplerInitInRadius;
            SamplerSearchWinSize = samplerSearchWinSize;
            SamplerInitMaxNegNum = samplerInitMaxNegNum;
            SamplerTrackInRadius = samplerTrackInRadius;
            SamplerTrackMaxPosNum = samplerTrackMaxPosNum;
            SamplerTrackMaxNegNum = samplerTrackMaxNegNum;
            FeatureSetNumFeatures = featureSetNumFeatures;
        }

        /// <summary>Gets init positive sample radius. 获取初始化正样本半径。</summary>
        public float SamplerInitInRadius { get; }

        /// <summary>Gets search window size. 获取搜索窗口尺寸。</summary>
        public float SamplerSearchWinSize { get; }

        /// <summary>Gets init negative sample count. 获取初始化负样本数量。</summary>
        public int SamplerInitMaxNegNum { get; }

        /// <summary>Gets tracking positive sample radius. 获取跟踪正样本半径。</summary>
        public float SamplerTrackInRadius { get; }

        /// <summary>Gets tracking positive sample count. 获取跟踪正样本数量。</summary>
        public int SamplerTrackMaxPosNum { get; }

        /// <summary>Gets tracking negative sample count. 获取跟踪负样本数量。</summary>
        public int SamplerTrackMaxNegNum { get; }

        /// <summary>Gets feature count. 获取特征数量。</summary>
        public int FeatureSetNumFeatures { get; }

        /// <summary>Determines whether two parameter values are equal. 判断两个参数值是否相等。</summary>
        public static bool operator ==(TrackerMILParams left, TrackerMILParams right)
        {
            return left.Equals(right);
        }

        /// <summary>Determines whether two parameter values are different. 判断两个参数值是否不同。</summary>
        public static bool operator !=(TrackerMILParams left, TrackerMILParams right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Gets a practical OpenCV MIL parameter set for managed construction.
        /// 获取用于 managed 构造的常用 OpenCV MIL 参数集。
        /// </summary>
        public static TrackerMILParams Default
        {
            get { return new TrackerMILParams(3.0F, 25.0F, 65, 4.0F, 100000, 65, 250); }
        }

        /// <summary>
        /// Gets MIL defaults from the linked native OpenCV runtime.
        /// 从已链接 native OpenCV runtime 读取 MIL 默认参数。
        /// </summary>
        public static TrackerMILParams GetDefaultFromNative()
        {
            NativeException.ThrowIfError(NativeMethods.TrackingLegacyTrackerMilGetDefaultParams(out NativeMethods.TrackingMilParamsNative native));
            return FromNative(native);
        }

        internal NativeMethods.TrackingMilParamsNative ToNative()
        {
            return new NativeMethods.TrackingMilParamsNative
            {
                SamplerInitInRadius = SamplerInitInRadius,
                SamplerSearchWinSize = SamplerSearchWinSize,
                SamplerInitMaxNegNum = SamplerInitMaxNegNum,
                SamplerTrackInRadius = SamplerTrackInRadius,
                SamplerTrackMaxPosNum = SamplerTrackMaxPosNum,
                SamplerTrackMaxNegNum = SamplerTrackMaxNegNum,
                FeatureSetNumFeatures = FeatureSetNumFeatures
            };
        }

        internal static TrackerMILParams FromNative(NativeMethods.TrackingMilParamsNative native)
        {
            return new TrackerMILParams(
                native.SamplerInitInRadius,
                native.SamplerSearchWinSize,
                native.SamplerInitMaxNegNum,
                native.SamplerTrackInRadius,
                native.SamplerTrackMaxPosNum,
                native.SamplerTrackMaxNegNum,
                native.FeatureSetNumFeatures);
        }

        /// <summary>Indicates whether this value equals another value. 指示此值是否与另一个值相等。</summary>
        public bool Equals(TrackerMILParams other)
        {
            return SamplerInitInRadius.Equals(other.SamplerInitInRadius)
                && SamplerSearchWinSize.Equals(other.SamplerSearchWinSize)
                && SamplerInitMaxNegNum == other.SamplerInitMaxNegNum
                && SamplerTrackInRadius.Equals(other.SamplerTrackInRadius)
                && SamplerTrackMaxPosNum == other.SamplerTrackMaxPosNum
                && SamplerTrackMaxNegNum == other.SamplerTrackMaxNegNum
                && FeatureSetNumFeatures == other.FeatureSetNumFeatures;
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is TrackerMILParams other && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = SamplerInitInRadius.GetHashCode();
                hash = (hash * 397) ^ SamplerSearchWinSize.GetHashCode();
                hash = (hash * 397) ^ SamplerInitMaxNegNum;
                hash = (hash * 397) ^ SamplerTrackInRadius.GetHashCode();
                hash = (hash * 397) ^ SamplerTrackMaxPosNum;
                hash = (hash * 397) ^ SamplerTrackMaxNegNum;
                hash = (hash * 397) ^ FeatureSetNumFeatures;
                return hash;
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "{SamplerInitInRadius=" + SamplerInitInRadius.ToString(CultureInfo.InvariantCulture)
                + ",SamplerSearchWinSize=" + SamplerSearchWinSize.ToString(CultureInfo.InvariantCulture)
                + ",SamplerInitMaxNegNum=" + SamplerInitMaxNegNum
                + ",SamplerTrackInRadius=" + SamplerTrackInRadius.ToString(CultureInfo.InvariantCulture)
                + ",SamplerTrackMaxPosNum=" + SamplerTrackMaxPosNum
                + ",SamplerTrackMaxNegNum=" + SamplerTrackMaxNegNum
                + ",FeatureSetNumFeatures=" + FeatureSetNumFeatures + "}";
        }
    }
}
