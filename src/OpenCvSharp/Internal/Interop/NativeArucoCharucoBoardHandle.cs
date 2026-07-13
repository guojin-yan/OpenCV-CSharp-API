using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal sealed class NativeArucoCharucoBoardHandle : SafeHandle
    {
        private NativeArucoCharucoBoardHandle()
            : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        internal static NativeArucoCharucoBoardHandle FromNativePointer(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                throw new OpenCvException("Native Aruco CharucoBoard handle is null.");
            }

            var result = new NativeArucoCharucoBoardHandle();
            result.SetHandle(nativeHandle);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.ArucoCharucoBoardReleaseHandle(handle);
            return true;
        }
    }
}
