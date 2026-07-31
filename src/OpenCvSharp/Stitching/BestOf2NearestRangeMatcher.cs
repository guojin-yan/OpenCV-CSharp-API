using System;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Stitching
{
    /// <summary>Restricts batch matching to image pairs inside a configured index range.</summary>
    public sealed class BestOf2NearestRangeMatcher : FeaturesMatcher
    {
        /// <summary>Creates an owned range-limited matcher.</summary>
        public BestOf2NearestRangeMatcher(
            int rangeWidth = 5,
            bool tryGpu = false,
            float matchConfidence = 0.3F,
            int numberOfMatchesThreshold1 = 6,
            int numberOfMatchesThreshold2 = 6)
            : base(CreateNative(rangeWidth, tryGpu, matchConfidence, numberOfMatchesThreshold1, numberOfMatchesThreshold2))
        {
        }

        private static IntPtr CreateNative(
            int rangeWidth,
            bool tryGpu,
            float matchConfidence,
            int numberOfMatchesThreshold1,
            int numberOfMatchesThreshold2)
        {
            if (rangeWidth <= 0) throw new ArgumentOutOfRangeException(nameof(rangeWidth));
            ValidateOptions(matchConfidence, numberOfMatchesThreshold1, numberOfMatchesThreshold2, 3.0);
            NativeException.ThrowIfError(NativeMethods.StitchingFeaturesMatcherCreateRange(
                rangeWidth,
                tryGpu ? 1 : 0,
                matchConfidence,
                numberOfMatchesThreshold1,
                numberOfMatchesThreshold2,
                out IntPtr nativeHandle));
            return nativeHandle;
        }
    }
}
