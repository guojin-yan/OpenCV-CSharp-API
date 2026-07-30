using System;
using System.Globalization;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Video
{
    /// <summary>Parameters for the model-free main Video MIL tracker.</summary>
    public readonly struct TrackerMILParams : IEquatable<TrackerMILParams>
    {
        /// <summary>Initializes a validated MIL parameter set.</summary>
        public TrackerMILParams(
            float samplerInitInRadius,
            int samplerInitMaxNegNum,
            float samplerSearchWinSize,
            float samplerTrackInRadius,
            int samplerTrackMaxPosNum,
            int samplerTrackMaxNegNum,
            int featureSetNumFeatures)
        {
            ValidateValues(
                samplerInitInRadius,
                samplerInitMaxNegNum,
                samplerSearchWinSize,
                samplerTrackInRadius,
                samplerTrackMaxPosNum,
                samplerTrackMaxNegNum,
                featureSetNumFeatures);
            SamplerInitInRadius = samplerInitInRadius;
            SamplerInitMaxNegNum = samplerInitMaxNegNum;
            SamplerSearchWinSize = samplerSearchWinSize;
            SamplerTrackInRadius = samplerTrackInRadius;
            SamplerTrackMaxPosNum = samplerTrackMaxPosNum;
            SamplerTrackMaxNegNum = samplerTrackMaxNegNum;
            FeatureSetNumFeatures = featureSetNumFeatures;
        }

        /// <summary>Gets the initialization positive-sample radius.</summary>
        public float SamplerInitInRadius { get; }

        /// <summary>Gets the maximum initialization negative-sample count.</summary>
        public int SamplerInitMaxNegNum { get; }

        /// <summary>Gets the tracking search-window size.</summary>
        public float SamplerSearchWinSize { get; }

        /// <summary>Gets the tracking positive-sample radius.</summary>
        public float SamplerTrackInRadius { get; }

        /// <summary>Gets the maximum tracking positive-sample count.</summary>
        public int SamplerTrackMaxPosNum { get; }

        /// <summary>Gets the maximum tracking negative-sample count.</summary>
        public int SamplerTrackMaxNegNum { get; }

        /// <summary>Gets the number of generated Haar features.</summary>
        public int FeatureSetNumFeatures { get; }

        /// <summary>Gets the OpenCV 5.0.0 default parameter values.</summary>
        public static TrackerMILParams Default => new TrackerMILParams(3.0F, 65, 25.0F, 4.0F, 100000, 65, 250);

        /// <summary>Reads default parameters from the linked OpenCV runtime.</summary>
        public static TrackerMILParams GetDefaultFromNative()
        {
            NativeException.ThrowIfError(NativeMethods.VideoTrackerMilGetDefaultParams(out NativeMethods.VideoTrackerMilParamsNative native));
            return FromNative(native);
        }

        internal NativeMethods.VideoTrackerMilParamsNative ToNative()
        {
            Validate();
            return new NativeMethods.VideoTrackerMilParamsNative
            {
                SamplerInitInRadius = SamplerInitInRadius,
                SamplerInitMaxNegNum = SamplerInitMaxNegNum,
                SamplerSearchWinSize = SamplerSearchWinSize,
                SamplerTrackInRadius = SamplerTrackInRadius,
                SamplerTrackMaxPosNum = SamplerTrackMaxPosNum,
                SamplerTrackMaxNegNum = SamplerTrackMaxNegNum,
                FeatureSetNumFeatures = FeatureSetNumFeatures
            };
        }

        internal static TrackerMILParams FromNative(NativeMethods.VideoTrackerMilParamsNative native)
        {
            return new TrackerMILParams(
                native.SamplerInitInRadius,
                native.SamplerInitMaxNegNum,
                native.SamplerSearchWinSize,
                native.SamplerTrackInRadius,
                native.SamplerTrackMaxPosNum,
                native.SamplerTrackMaxNegNum,
                native.FeatureSetNumFeatures);
        }

        internal void Validate()
        {
            ValidateValues(
                SamplerInitInRadius,
                SamplerInitMaxNegNum,
                SamplerSearchWinSize,
                SamplerTrackInRadius,
                SamplerTrackMaxPosNum,
                SamplerTrackMaxNegNum,
                FeatureSetNumFeatures);
        }

        private static void ValidateValues(
            float samplerInitInRadius,
            int samplerInitMaxNegNum,
            float samplerSearchWinSize,
            float samplerTrackInRadius,
            int samplerTrackMaxPosNum,
            int samplerTrackMaxNegNum,
            int featureSetNumFeatures)
        {
            if (float.IsNaN(samplerInitInRadius) || float.IsInfinity(samplerInitInRadius) || samplerInitInRadius < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(samplerInitInRadius));
            }
            if (float.IsNaN(samplerSearchWinSize) || float.IsInfinity(samplerSearchWinSize) || samplerSearchWinSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(samplerSearchWinSize));
            }
            if (float.IsNaN(samplerTrackInRadius) || float.IsInfinity(samplerTrackInRadius) || samplerTrackInRadius < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(samplerTrackInRadius));
            }
            if (samplerInitMaxNegNum <= 0 || samplerTrackMaxPosNum <= 0 || samplerTrackMaxNegNum <= 0 || featureSetNumFeatures <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(featureSetNumFeatures), "MIL sample and feature counts must be positive.");
            }
        }

        /// <inheritdoc/>
        public bool Equals(TrackerMILParams other)
        {
            return SamplerInitInRadius.Equals(other.SamplerInitInRadius) &&
                SamplerInitMaxNegNum == other.SamplerInitMaxNegNum &&
                SamplerSearchWinSize.Equals(other.SamplerSearchWinSize) &&
                SamplerTrackInRadius.Equals(other.SamplerTrackInRadius) &&
                SamplerTrackMaxPosNum == other.SamplerTrackMaxPosNum &&
                SamplerTrackMaxNegNum == other.SamplerTrackMaxNegNum &&
                FeatureSetNumFeatures == other.FeatureSetNumFeatures;
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is TrackerMILParams other && Equals(other);

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = SamplerInitInRadius.GetHashCode();
                hash = (hash * 397) ^ SamplerInitMaxNegNum;
                hash = (hash * 397) ^ SamplerSearchWinSize.GetHashCode();
                hash = (hash * 397) ^ SamplerTrackInRadius.GetHashCode();
                hash = (hash * 397) ^ SamplerTrackMaxPosNum;
                hash = (hash * 397) ^ SamplerTrackMaxNegNum;
                hash = (hash * 397) ^ FeatureSetNumFeatures;
                return hash;
            }
        }

        /// <summary>Determines whether two parameter sets are equal.</summary>
        public static bool operator ==(TrackerMILParams left, TrackerMILParams right) => left.Equals(right);

        /// <summary>Determines whether two parameter sets differ.</summary>
        public static bool operator !=(TrackerMILParams left, TrackerMILParams right) => !left.Equals(right);

        /// <inheritdoc/>
        public override string ToString()
        {
            return "{SamplerInitInRadius=" + SamplerInitInRadius.ToString(CultureInfo.InvariantCulture) +
                ",SamplerInitMaxNegNum=" + SamplerInitMaxNegNum +
                ",SamplerSearchWinSize=" + SamplerSearchWinSize.ToString(CultureInfo.InvariantCulture) +
                ",SamplerTrackInRadius=" + SamplerTrackInRadius.ToString(CultureInfo.InvariantCulture) +
                ",SamplerTrackMaxPosNum=" + SamplerTrackMaxPosNum +
                ",SamplerTrackMaxNegNum=" + SamplerTrackMaxNegNum +
                ",FeatureSetNumFeatures=" + FeatureSetNumFeatures + "}";
        }
    }
}
