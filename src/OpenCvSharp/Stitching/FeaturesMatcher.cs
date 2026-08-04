using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Stitching
{
    /// <summary>Base class for owned OpenCV stitching feature matchers.</summary>
    public abstract class FeaturesMatcher : IDisposable
    {
        private NativeFeaturesMatcherHandle handle;
        private bool disposed;

        internal FeaturesMatcher(IntPtr nativeHandle)
        {
            handle = NativeFeaturesMatcherHandle.FromNativePointer(nativeHandle);
        }

        /// <summary>Gets whether this matcher has been disposed.</summary>
        public bool IsDisposed
        {
            get { return disposed; }
        }

        /// <summary>Gets whether OpenCV permits this matcher instance to be used in parallel.</summary>
        public bool IsThreadSafe
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.StitchingFeaturesMatcherIsThreadSafe(NativeHandle, out int value));
                return value != 0;
            }
        }

        internal IntPtr NativeHandle
        {
            get
            {
                ThrowIfDisposed();
                return handle.DangerousGetHandle();
            }
        }

        /// <summary>Matches two feature records and returns an independently owned result.</summary>
        public MatchesInfo Match(ImageFeatures first, ImageFeatures second)
        {
            ThrowIfDisposed();
            ValidateFeature(first, nameof(first));
            ValidateFeature(second, nameof(second));
            var result = new MatchesInfo();
            try
            {
                NativeException.ThrowIfError(NativeMethods.StitchingFeaturesMatcherMatchPair(
                    NativeHandle, first.NativeHandle, second.NativeHandle, result.NativeHandle));
                GC.KeepAlive(first);
                GC.KeepAlive(second);
                return result;
            }
            catch
            {
                result.Dispose();
                throw;
            }
        }

        /// <summary>Matches a collection and returns exactly N squared row-major pairwise results.</summary>
        public MatchesInfo[] Match(ImageFeatures[] features, Mat? mask = null)
        {
            ThrowIfDisposed();
            if (features == null) throw new ArgumentNullException(nameof(features));
            if (features.Length == 0) throw new ArgumentException("At least one feature record is required.", nameof(features));
            int resultCount = checked(features.Length * features.Length);
            var featureHandles = new IntPtr[features.Length];
            for (int i = 0; i < features.Length; ++i)
            {
                ValidateFeature(features[i], nameof(features));
                featureHandles[i] = features[i].NativeHandle;
            }
            if (mask != null && (mask.Empty || mask.Type != MatType.CV_8UC1 ||
                mask.Rows != features.Length || mask.Cols != features.Length))
            {
                throw new ArgumentException("The match mask must be an exact N by N CV_8UC1 matrix.", nameof(mask));
            }

            var result = new MatchesInfo[resultCount];
            int created = 0;
            try
            {
                var resultHandles = new IntPtr[resultCount];
                for (; created < result.Length; ++created)
                {
                    result[created] = new MatchesInfo();
                    resultHandles[created] = result[created].NativeHandle;
                }
                NativeException.ThrowIfError(NativeMethods.StitchingFeaturesMatcherMatchBatch(
                    NativeHandle,
                    featureHandles,
                    featureHandles.Length,
                    mask == null ? IntPtr.Zero : mask.NativeHandle,
                    resultHandles,
                    resultHandles.Length));
                GC.KeepAlive(features);
                GC.KeepAlive(mask);
                return result;
            }
            catch
            {
                for (int i = 0; i < created; ++i) result[i]?.Dispose();
                throw;
            }
        }

        /// <summary>Asks OpenCV to release any reusable matcher working memory.</summary>
        public void CollectGarbage()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.StitchingFeaturesMatcherCollectGarbage(NativeHandle));
        }

        /// <summary>Releases the owned native matcher.</summary>
        public void Dispose()
        {
            if (disposed) return;
            handle.Dispose();
            disposed = true;
            GC.SuppressFinalize(this);
        }

        internal static void ValidateOptions(
            float matchConfidence,
            int numberOfMatchesThreshold1,
            int numberOfMatchesThreshold2,
            double matchesConfidenceThreshold)
        {
            if (float.IsNaN(matchConfidence) || float.IsInfinity(matchConfidence) || matchConfidence < 0.0F)
            {
                throw new ArgumentOutOfRangeException(nameof(matchConfidence), "Match confidence must be finite and non-negative.");
            }
            if (numberOfMatchesThreshold1 < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(numberOfMatchesThreshold1));
            }
            if (numberOfMatchesThreshold2 < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(numberOfMatchesThreshold2));
            }
            if (double.IsNaN(matchesConfidenceThreshold) || double.IsInfinity(matchesConfidenceThreshold))
            {
                throw new ArgumentOutOfRangeException(nameof(matchesConfidenceThreshold), "Matches confidence threshold must be finite.");
            }
        }

        private static void ValidateFeature(ImageFeatures feature, string parameterName)
        {
            if (feature == null) throw new ArgumentNullException(parameterName);
            if (feature.IsDisposed) throw new ObjectDisposedException(feature.GetType().FullName);
        }

        private void ThrowIfDisposed()
        {
            if (disposed) throw new ObjectDisposedException(GetType().FullName);
        }
    }
}
