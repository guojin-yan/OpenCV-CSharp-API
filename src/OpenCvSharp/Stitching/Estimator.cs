using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Stitching
{
    /// <summary>Base class for owned camera estimators and bundle adjusters.</summary>
    public abstract class Estimator : IDisposable
    {
        private NativeEstimatorHandle handle;
        private readonly bool requiresInitialCameras;
        private bool disposed;

        internal Estimator(IntPtr nativeHandle, bool requiresInitialCameras)
        {
            handle = NativeEstimatorHandle.FromNativePointer(nativeHandle);
            this.requiresInitialCameras = requiresInitialCameras;
        }

        /// <summary>Gets whether this estimator has been disposed.</summary>
        public bool IsDisposed
        {
            get { return disposed; }
        }

        internal IntPtr NativeHandle
        {
            get
            {
                ThrowIfDisposed();
                return handle.DangerousGetHandle();
            }
        }

        /// <summary>Estimates independently owned camera parameters without initial camera values.</summary>
        public bool Apply(
            ImageFeatures[] features,
            MatchesInfo[] pairwiseMatches,
            out StitcherCameraParams[] cameras)
        {
            return Apply(features, pairwiseMatches, null, out cameras);
        }

        /// <summary>Estimates or adjusts cameras from optional initial values without modifying the inputs.</summary>
        public bool Apply(
            ImageFeatures[] features,
            MatchesInfo[] pairwiseMatches,
            StitcherCameraParams[]? initialCameras,
            out StitcherCameraParams[] cameras)
        {
            ThrowIfDisposed();
            cameras = Array.Empty<StitcherCameraParams>();
            IntPtr[] featureHandles = StitchingMotionMarshal.GetFeatureHandles(features, nameof(features));
            IntPtr[] matchHandles = StitchingMotionMarshal.GetMatchHandles(
                pairwiseMatches, featureHandles.Length, nameof(pairwiseMatches));
            NativeMethods.StitchingCameraParamsNative[] initial =
                StitchingMotionMarshal.GetCameraValues(initialCameras, featureHandles.Length, requiresInitialCameras);
            var nativeResults = new NativeMethods.StitchingCameraParamsNative[featureHandles.Length];
            NativeException.ThrowIfError(NativeMethods.StitchingEstimatorApply(
                NativeHandle,
                featureHandles,
                featureHandles.Length,
                matchHandles,
                matchHandles.Length,
                initial,
                initial.Length,
                nativeResults,
                nativeResults.Length,
                out int succeeded));
            cameras = StitchingMotionMarshal.TakeCameras(nativeResults);
            GC.KeepAlive(features);
            GC.KeepAlive(pairwiseMatches);
            GC.KeepAlive(initialCameras);
            return succeeded != 0;
        }

        /// <summary>Releases the owned native estimator.</summary>
        public void Dispose()
        {
            if (disposed) return;
            handle.Dispose();
            disposed = true;
            GC.SuppressFinalize(this);
        }

        private void ThrowIfDisposed()
        {
            if (disposed) throw new ObjectDisposedException(GetType().FullName);
        }
    }

    /// <summary>Estimates camera rotations from pairwise homographies.</summary>
    public sealed class HomographyBasedEstimator : Estimator
    {
        /// <summary>Creates a homography estimator.</summary>
        public HomographyBasedEstimator(bool focalLengthsEstimated = false)
            : base(CreateNative(focalLengthsEstimated), focalLengthsEstimated)
        {
        }

        private static IntPtr CreateNative(bool focalLengthsEstimated)
        {
            NativeException.ThrowIfError(NativeMethods.StitchingEstimatorCreateHomography(
                focalLengthsEstimated ? 1 : 0, out IntPtr nativeHandle));
            return nativeHandle;
        }
    }

    /// <summary>Chains affine pairwise transforms into camera transformations.</summary>
    public sealed class AffineBasedEstimator : Estimator
    {
        /// <summary>Creates an affine estimator.</summary>
        public AffineBasedEstimator()
            : base(CreateNative(), false)
        {
        }

        private static IntPtr CreateNative()
        {
            NativeException.ThrowIfError(NativeMethods.StitchingEstimatorCreateAffine(out IntPtr nativeHandle));
            return nativeHandle;
        }
    }
}
