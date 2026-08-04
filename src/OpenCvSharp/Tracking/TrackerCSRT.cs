using System;
using System.Runtime.InteropServices;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Tracking
{
    /// <summary>
    /// Modern contrib CSRT tracker wrapper.
    /// 现代 contrib CSRT 跟踪器包装。
    /// </summary>
    public sealed class TrackerCSRT : Tracker
    {
        private TrackerCSRT(IntPtr nativeHandle)
            : base(nativeHandle)
        {
        }

        /// <summary>Creates a CSRT tracker with OpenCV defaults. 使用 OpenCV 默认参数创建 CSRT 跟踪器。</summary>
        public static TrackerCSRT Create()
        {
            NativeException.ThrowIfError(NativeMethods.TrackingTrackerCsrtCreateDefault(out IntPtr nativeHandle));
            return new TrackerCSRT(nativeHandle);
        }

        /// <summary>Creates a CSRT tracker with explicit parameters. 使用显式参数创建 CSRT 跟踪器。</summary>
        public static TrackerCSRT Create(TrackerCSRTParams parameters)
        {
            byte[] windowFunction = TrackerCSRTParams.ToNullTerminatedUtf8(parameters.WindowFunction);
            GCHandle pinned = GCHandle.Alloc(windowFunction, GCHandleType.Pinned);
            try
            {
                NativeMethods.TrackingCsrtParamsNative native = parameters.ToNative(pinned.AddrOfPinnedObject());
                NativeException.ThrowIfError(NativeMethods.TrackingTrackerCsrtCreate(ref native, out IntPtr nativeHandle));
                return new TrackerCSRT(nativeHandle);
            }
            finally
            {
                pinned.Free();
            }
        }

        /// <summary>
        /// Sets the initial segmentation mask used by CSRT.
        /// 设置 CSRT 使用的初始分割掩码。
        /// </summary>
        public void SetInitialMask(Mat mask)
        {
            ThrowIfDisposed();
            ValidateNotNull(mask, nameof(mask));
            NativeException.ThrowIfError(NativeMethods.TrackingTrackerCsrtSetInitialMask(NativeHandle, mask.NativeHandle));
        }
    }
}
