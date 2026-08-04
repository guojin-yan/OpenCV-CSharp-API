using System;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Stitching
{
    /// <summary>Matches features and estimates either a full or reduced affine transform.</summary>
    public sealed class AffineBestOf2NearestMatcher : FeaturesMatcher
    {
        /// <summary>Creates an owned affine matcher.</summary>
        public AffineBestOf2NearestMatcher(
            bool fullAffine = false,
            bool tryGpu = false,
            float matchConfidence = 0.3F,
            int numberOfMatchesThreshold1 = 6)
            : base(CreateNative(fullAffine, tryGpu, matchConfidence, numberOfMatchesThreshold1))
        {
        }

        private static IntPtr CreateNative(
            bool fullAffine,
            bool tryGpu,
            float matchConfidence,
            int numberOfMatchesThreshold1)
        {
            ValidateOptions(matchConfidence, numberOfMatchesThreshold1, numberOfMatchesThreshold1, 3.0);
            NativeException.ThrowIfError(NativeMethods.StitchingFeaturesMatcherCreateAffine(
                fullAffine ? 1 : 0,
                tryGpu ? 1 : 0,
                matchConfidence,
                numberOfMatchesThreshold1,
                out IntPtr nativeHandle));
            return nativeHandle;
        }
    }
}
