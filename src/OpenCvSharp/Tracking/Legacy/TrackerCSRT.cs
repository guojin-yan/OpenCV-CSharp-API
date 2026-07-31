using System;
using System.Runtime.InteropServices;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Tracking.Legacy
{
    /// <summary>OpenCV legacy CSRT tracker.</summary>
    public sealed class TrackerCSRT : LegacyTracker
    {
        private TrackerCSRT(IntPtr nativeHandle)
            : base(nativeHandle)
        {
        }

        /// <summary>Creates a legacy CSRT tracker with OpenCV defaults.</summary>
        public static TrackerCSRT Create()
        {
            NativeException.ThrowIfError(NativeMethods.TrackingLegacyTrackerCsrtCreateDefault(out IntPtr nativeHandle));
            return new TrackerCSRT(nativeHandle);
        }

        /// <summary>Creates a legacy CSRT tracker with explicit copied parameters.</summary>
        public static TrackerCSRT Create(OpenCvSharp.Tracking.TrackerCSRTParams parameters)
        {
            byte[] windowFunction = OpenCvSharp.Tracking.TrackerCSRTParams.ToNullTerminatedUtf8(parameters.WindowFunction);
            GCHandle pinned = GCHandle.Alloc(windowFunction, GCHandleType.Pinned);
            try
            {
                NativeMethods.TrackingCsrtParamsNative native = parameters.ToNative(pinned.AddrOfPinnedObject());
                NativeException.ThrowIfError(NativeMethods.TrackingLegacyTrackerCsrtCreate(ref native, out IntPtr nativeHandle));
                return new TrackerCSRT(nativeHandle);
            }
            finally
            {
                pinned.Free();
            }
        }

        /// <summary>Sets the segmentation mask to use during initialization.</summary>
        public void SetInitialMask(Mat mask)
        {
            ThrowIfDisposed();
            ValidateNotNull(mask, nameof(mask));
            NativeException.ThrowIfError(NativeMethods.TrackingLegacyTrackerCsrtSetInitialMask(NativeHandle, mask.NativeHandle));
        }
    }
}
