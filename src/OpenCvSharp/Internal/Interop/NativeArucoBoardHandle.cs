using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal sealed class NativeArucoBoardHandle : SafeHandle
    {
        private NativeArucoBoardHandle() : base(IntPtr.Zero, true) { }

        public override bool IsInvalid => handle == IntPtr.Zero;

        internal static NativeArucoBoardHandle FromNativePointer(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero) throw new OpenCvException("Native ArucoBoard handle is null.");
            var result = new NativeArucoBoardHandle();
            result.SetHandle(nativeHandle);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.ArucoBoardReleaseHandle(handle);
            return true;
        }
    }
}
