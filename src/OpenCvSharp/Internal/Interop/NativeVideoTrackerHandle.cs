using System;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    internal sealed class NativeVideoTrackerHandle : SafeHandle
    {
        private NativeVideoTrackerHandle()
            : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid => handle == IntPtr.Zero;

        internal static NativeVideoTrackerHandle FromNativePointer(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                throw new OpenCvException("Native main Video Tracker handle is null.");
            }

            var result = new NativeVideoTrackerHandle();
            result.SetHandle(nativeHandle);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.VideoTrackerReleaseHandle(handle);
            return true;
        }
    }
}
