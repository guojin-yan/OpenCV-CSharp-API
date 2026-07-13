using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal sealed class NativeTrackingLegacyMultiTrackerHandle : SafeHandle
    {
        private NativeTrackingLegacyMultiTrackerHandle()
            : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        internal static NativeTrackingLegacyMultiTrackerHandle FromNativePointer(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                throw new OpenCvException("Native legacy MultiTracker handle is null.");
            }

            var result = new NativeTrackingLegacyMultiTrackerHandle();
            result.SetHandle(nativeHandle);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.TrackingLegacyMultiTrackerReleaseHandle(handle);
            return true;
        }
    }
}
