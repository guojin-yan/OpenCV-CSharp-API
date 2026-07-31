using System;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Stitching
{
    /// <summary>Matches each descriptor against its two nearest candidates and estimates a homography.</summary>
    public sealed class BestOf2NearestMatcher : FeaturesMatcher
    {
        /// <summary>Creates an owned best-of-two-nearest matcher.</summary>
        public BestOf2NearestMatcher(
            bool tryGpu = false,
            float matchConfidence = 0.3F,
            int numberOfMatchesThreshold1 = 6,
            int numberOfMatchesThreshold2 = 6,
            double matchesConfidenceThreshold = 3.0)
            : base(CreateNative(
                false,
                tryGpu,
                matchConfidence,
                numberOfMatchesThreshold1,
                numberOfMatchesThreshold2,
                matchesConfidenceThreshold))
        {
        }

        private BestOf2NearestMatcher(IntPtr nativeHandle)
            : base(nativeHandle)
        {
        }

        /// <summary>Creates the same matcher through OpenCV's static factory.</summary>
        public static BestOf2NearestMatcher Create(
            bool tryGpu = false,
            float matchConfidence = 0.3F,
            int numberOfMatchesThreshold1 = 6,
            int numberOfMatchesThreshold2 = 6,
            double matchesConfidenceThreshold = 3.0)
        {
            return new BestOf2NearestMatcher(CreateNative(
                true,
                tryGpu,
                matchConfidence,
                numberOfMatchesThreshold1,
                numberOfMatchesThreshold2,
                matchesConfidenceThreshold));
        }

        private static IntPtr CreateNative(
            bool useFactory,
            bool tryGpu,
            float matchConfidence,
            int numberOfMatchesThreshold1,
            int numberOfMatchesThreshold2,
            double matchesConfidenceThreshold)
        {
            ValidateOptions(matchConfidence, numberOfMatchesThreshold1, numberOfMatchesThreshold2, matchesConfidenceThreshold);
            int status = useFactory
                ? NativeMethods.StitchingFeaturesMatcherFactoryBestOfTwoNearest(
                    tryGpu ? 1 : 0, matchConfidence, numberOfMatchesThreshold1,
                    numberOfMatchesThreshold2, matchesConfidenceThreshold, out IntPtr nativeHandle)
                : NativeMethods.StitchingFeaturesMatcherCreateBestOfTwoNearest(
                    tryGpu ? 1 : 0, matchConfidence, numberOfMatchesThreshold1,
                    numberOfMatchesThreshold2, matchesConfidenceThreshold, out nativeHandle);
            NativeException.ThrowIfError(status);
            return nativeHandle;
        }
    }
}
