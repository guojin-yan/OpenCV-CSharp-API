using System;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Tracking.Legacy
{
    /// <summary>OpenCV legacy Tracking-Learning-Detection tracker.</summary>
    public sealed class TrackerTLD : LegacyTracker
    {
        private TrackerTLD(IntPtr nativeHandle)
            : base(nativeHandle)
        {
        }

        /// <summary>Creates a TLD tracker with OpenCV defaults.</summary>
        public static TrackerTLD Create()
        {
            NativeException.ThrowIfError(NativeMethods.TrackingLegacyTrackerTldCreate(out IntPtr nativeHandle));
            return new TrackerTLD(nativeHandle);
        }
    }
}
