using System;
using System.Globalization;
using System.Runtime.InteropServices;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Tracking.Legacy
{
    /// <summary>
    /// Parameters for the OpenCV legacy Boosting tracker.
    /// OpenCV legacy Boosting 跟踪器参数。
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct TrackerBoostingParams : IEquatable<TrackerBoostingParams>
    {
        /// <summary>Initializes Boosting parameters. 初始化 Boosting 参数。</summary>
        public TrackerBoostingParams(
            int numClassifiers,
            float samplerOverlap,
            float samplerSearchFactor,
            int iterationInit,
            int featureSetNumFeatures)
        {
            NumClassifiers = numClassifiers;
            SamplerOverlap = samplerOverlap;
            SamplerSearchFactor = samplerSearchFactor;
            IterationInit = iterationInit;
            FeatureSetNumFeatures = featureSetNumFeatures;
        }

        /// <summary>Gets the number of online classifiers. 获取在线分类器数量。</summary>
        public int NumClassifiers { get; }

        /// <summary>Gets the overlap used by the sampler. 获取采样器使用的重叠率。</summary>
        public float SamplerOverlap { get; }

        /// <summary>Gets the search-region scale factor. 获取搜索区域缩放因子。</summary>
        public float SamplerSearchFactor { get; }

        /// <summary>Gets the initialization iteration count. 获取初始化迭代次数。</summary>
        public int IterationInit { get; }

        /// <summary>Gets the feature count. 获取特征数量。</summary>
        public int FeatureSetNumFeatures { get; }

        /// <summary>Gets the exact OpenCV 5.0.0 source defaults.</summary>
        public static TrackerBoostingParams Default
        {
            get { return new TrackerBoostingParams(100, 0.99F, 1.8F, 50, 1050); }
        }

        /// <summary>Gets defaults from the linked native OpenCV runtime.</summary>
        public static TrackerBoostingParams GetDefaultFromNative()
        {
            NativeException.ThrowIfError(NativeMethods.TrackingLegacyTrackerBoostingGetDefaultParams(out NativeMethods.TrackingBoostingParamsNative native));
            return FromNative(native);
        }

        internal NativeMethods.TrackingBoostingParamsNative ToNative()
        {
            return new NativeMethods.TrackingBoostingParamsNative
            {
                NumClassifiers = NumClassifiers,
                SamplerOverlap = SamplerOverlap,
                SamplerSearchFactor = SamplerSearchFactor,
                IterationInit = IterationInit,
                FeatureSetNumFeatures = FeatureSetNumFeatures
            };
        }

        internal static TrackerBoostingParams FromNative(NativeMethods.TrackingBoostingParamsNative native)
        {
            return new TrackerBoostingParams(
                native.NumClassifiers,
                native.SamplerOverlap,
                native.SamplerSearchFactor,
                native.IterationInit,
                native.FeatureSetNumFeatures);
        }

        /// <summary>Determines whether two parameter values are equal.</summary>
        public static bool operator ==(TrackerBoostingParams left, TrackerBoostingParams right)
        {
            return left.Equals(right);
        }

        /// <summary>Determines whether two parameter values are different.</summary>
        public static bool operator !=(TrackerBoostingParams left, TrackerBoostingParams right)
        {
            return !left.Equals(right);
        }

        /// <inheritdoc/>
        public bool Equals(TrackerBoostingParams other)
        {
            return NumClassifiers == other.NumClassifiers
                && SamplerOverlap.Equals(other.SamplerOverlap)
                && SamplerSearchFactor.Equals(other.SamplerSearchFactor)
                && IterationInit == other.IterationInit
                && FeatureSetNumFeatures == other.FeatureSetNumFeatures;
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is TrackerBoostingParams other && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = NumClassifiers;
                hash = (hash * 397) ^ SamplerOverlap.GetHashCode();
                hash = (hash * 397) ^ SamplerSearchFactor.GetHashCode();
                hash = (hash * 397) ^ IterationInit;
                hash = (hash * 397) ^ FeatureSetNumFeatures;
                return hash;
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "{NumClassifiers=" + NumClassifiers
                + ",SamplerOverlap=" + SamplerOverlap.ToString(CultureInfo.InvariantCulture)
                + ",SamplerSearchFactor=" + SamplerSearchFactor.ToString(CultureInfo.InvariantCulture)
                + ",IterationInit=" + IterationInit
                + ",FeatureSetNumFeatures=" + FeatureSetNumFeatures + "}";
        }
    }
}
