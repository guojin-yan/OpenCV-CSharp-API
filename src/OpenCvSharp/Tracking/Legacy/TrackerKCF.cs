using System;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Tracking.Legacy
{
    /// <summary>OpenCV legacy KCF tracker.</summary>
    public sealed class TrackerKCF : LegacyTracker
    {
        private TrackerKCF(IntPtr nativeHandle)
            : base(nativeHandle)
        {
        }

        /// <summary>Creates a legacy KCF tracker with OpenCV defaults.</summary>
        public static TrackerKCF Create()
        {
            NativeException.ThrowIfError(NativeMethods.TrackingLegacyTrackerKcfCreateDefault(out IntPtr nativeHandle));
            return new TrackerKCF(nativeHandle);
        }

        /// <summary>Creates a legacy KCF tracker with explicit copied parameters.</summary>
        public static TrackerKCF Create(JYPPX.OpenCvSharp.Tracking.TrackerKCFParams parameters)
        {
            NativeMethods.TrackingKcfParamsNative native = parameters.ToNative();
            NativeException.ThrowIfError(NativeMethods.TrackingLegacyTrackerKcfCreate(ref native, out IntPtr nativeHandle));
            return new TrackerKCF(nativeHandle);
        }
    }
}
