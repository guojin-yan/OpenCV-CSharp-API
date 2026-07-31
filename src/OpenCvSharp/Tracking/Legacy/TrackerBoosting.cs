using System;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Tracking.Legacy
{
    /// <summary>OpenCV legacy online Boosting tracker.</summary>
    public sealed class TrackerBoosting : LegacyTracker
    {
        private TrackerBoosting(IntPtr nativeHandle)
            : base(nativeHandle)
        {
        }

        /// <summary>Creates a Boosting tracker with OpenCV defaults.</summary>
        public static TrackerBoosting Create()
        {
            NativeException.ThrowIfError(NativeMethods.TrackingLegacyTrackerBoostingCreateDefault(out IntPtr nativeHandle));
            return new TrackerBoosting(nativeHandle);
        }

        /// <summary>Creates a Boosting tracker with explicit copied parameters.</summary>
        public static TrackerBoosting Create(TrackerBoostingParams parameters)
        {
            NativeMethods.TrackingBoostingParamsNative native = parameters.ToNative();
            NativeException.ThrowIfError(NativeMethods.TrackingLegacyTrackerBoostingCreate(ref native, out IntPtr nativeHandle));
            return new TrackerBoosting(nativeHandle);
        }
    }
}
