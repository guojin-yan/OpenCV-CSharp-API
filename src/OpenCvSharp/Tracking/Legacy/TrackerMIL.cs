using System;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Tracking.Legacy
{
    /// <summary>
    /// OpenCV legacy MIL tracker wrapper.
    /// OpenCV legacy MIL 跟踪器包装。
    /// </summary>
    public sealed class TrackerMIL : LegacyTracker
    {
        private TrackerMIL(IntPtr nativeHandle)
            : base(nativeHandle)
        {
        }

        /// <summary>Creates a MIL tracker with OpenCV defaults. 使用 OpenCV 默认参数创建 MIL 跟踪器。</summary>
        public static TrackerMIL Create()
        {
            NativeException.ThrowIfError(NativeMethods.TrackingLegacyTrackerMilCreateDefault(out IntPtr nativeHandle));
            return new TrackerMIL(nativeHandle);
        }

        /// <summary>Creates a MIL tracker with explicit parameters. 使用显式参数创建 MIL 跟踪器。</summary>
        public static TrackerMIL Create(TrackerMILParams parameters)
        {
            NativeMethods.TrackingMilParamsNative native = parameters.ToNative();
            NativeException.ThrowIfError(NativeMethods.TrackingLegacyTrackerMilCreate(ref native, out IntPtr nativeHandle));
            return new TrackerMIL(nativeHandle);
        }
    }
}
