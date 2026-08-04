using System;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    internal sealed class NativeTrackingLegacyTrackerHandle : SafeHandle
    {
        private NativeTrackingLegacyTrackerHandle()
            : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        internal static NativeTrackingLegacyTrackerHandle FromNativePointer(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                throw new OpenCvException("Native legacy tracking Tracker handle is null.");
            }

            var result = new NativeTrackingLegacyTrackerHandle();
            result.SetHandle(nativeHandle);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.TrackingLegacyTrackerReleaseHandle(handle);
            return true;
        }
    }
}
