using System;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Tracking
{
    /// <summary>
    /// Modern contrib KCF tracker wrapper.
    /// 现代 contrib KCF 跟踪器包装。
    /// </summary>
    public sealed class TrackerKCF : Tracker
    {
        private TrackerKCF(IntPtr nativeHandle)
            : base(nativeHandle)
        {
        }

        /// <summary>Creates a KCF tracker with OpenCV defaults. 使用 OpenCV 默认参数创建 KCF 跟踪器。</summary>
        public static TrackerKCF Create()
        {
            NativeException.ThrowIfError(NativeMethods.TrackingTrackerKcfCreateDefault(out IntPtr nativeHandle));
            return new TrackerKCF(nativeHandle);
        }

        /// <summary>Creates a KCF tracker with explicit parameters. 使用显式参数创建 KCF 跟踪器。</summary>
        public static TrackerKCF Create(TrackerKCFParams parameters)
        {
            NativeMethods.TrackingKcfParamsNative native = parameters.ToNative();
            NativeException.ThrowIfError(NativeMethods.TrackingTrackerKcfCreate(ref native, out IntPtr nativeHandle));
            return new TrackerKCF(nativeHandle);
        }
    }
}
