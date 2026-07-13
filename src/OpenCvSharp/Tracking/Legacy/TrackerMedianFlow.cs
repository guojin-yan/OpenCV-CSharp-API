using System;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Tracking.Legacy
{
    /// <summary>
    /// OpenCV legacy MedianFlow tracker wrapper.
    /// OpenCV legacy MedianFlow 跟踪器包装。
    /// </summary>
    public sealed class TrackerMedianFlow : LegacyTracker
    {
        private TrackerMedianFlow(IntPtr nativeHandle)
            : base(nativeHandle)
        {
        }

        /// <summary>Creates a MedianFlow tracker with OpenCV defaults. 使用 OpenCV 默认参数创建 MedianFlow 跟踪器。</summary>
        public static TrackerMedianFlow Create()
        {
            NativeException.ThrowIfError(NativeMethods.TrackingLegacyTrackerMedianFlowCreateDefault(out IntPtr nativeHandle));
            return new TrackerMedianFlow(nativeHandle);
        }

        /// <summary>Creates a MedianFlow tracker with explicit parameters. 使用显式参数创建 MedianFlow 跟踪器。</summary>
        public static TrackerMedianFlow Create(TrackerMedianFlowParams parameters)
        {
            NativeMethods.TrackingMedianFlowParamsNative native = parameters.ToNative();
            NativeException.ThrowIfError(NativeMethods.TrackingLegacyTrackerMedianFlowCreate(ref native, out IntPtr nativeHandle));
            return new TrackerMedianFlow(nativeHandle);
        }
    }
}
