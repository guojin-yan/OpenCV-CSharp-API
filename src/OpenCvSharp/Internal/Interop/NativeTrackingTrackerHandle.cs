using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal sealed class NativeTrackingTrackerHandle : SafeHandle
    {
        private NativeTrackingTrackerHandle()
            : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        internal static NativeTrackingTrackerHandle FromNativePointer(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                throw new OpenCvException("Native tracking Tracker handle is null.");
            }

            var result = new NativeTrackingTrackerHandle();
            result.SetHandle(nativeHandle);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.TrackingTrackerReleaseHandle(handle);
            return true;
        }
    }
}
