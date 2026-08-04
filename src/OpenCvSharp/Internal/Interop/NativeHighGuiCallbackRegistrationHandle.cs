using System;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    internal sealed class NativeHighGuiCallbackRegistrationHandle : SafeHandle
    {
        private NativeHighGuiCallbackRegistrationHandle()
            : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        internal static NativeHighGuiCallbackRegistrationHandle FromNativePointer(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                throw new OpenCvException("Native HighGUI callback registration handle is null.");
            }

            var result = new NativeHighGuiCallbackRegistrationHandle();
            result.SetHandle(nativeHandle);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.HighGuiCallbackRegistrationReleaseHandle(handle);
            return true;
        }
    }
}
