using System;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Tracking.Legacy
{
    /// <summary>
    /// OpenCV legacy MOSSE tracker wrapper.
    /// OpenCV legacy MOSSE 跟踪器包装。
    /// </summary>
    public sealed class TrackerMOSSE : LegacyTracker
    {
        private TrackerMOSSE(IntPtr nativeHandle)
            : base(nativeHandle)
        {
        }

        /// <summary>Creates a MOSSE tracker. 创建 MOSSE 跟踪器。</summary>
        public static TrackerMOSSE Create()
        {
            NativeException.ThrowIfError(NativeMethods.TrackingLegacyTrackerMosseCreate(out IntPtr nativeHandle));
            return new TrackerMOSSE(nativeHandle);
        }
    }
}
