using System;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    internal sealed class NativeHighGuiTrackbarHandle : SafeHandle
    {
        private NativeHighGuiTrackbarHandle()
            : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        internal static NativeHighGuiTrackbarHandle FromNativePointer(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                throw new OpenCvException("Native HighGUI trackbar handle is null.");
            }

            var result = new NativeHighGuiTrackbarHandle();
            result.SetHandle(nativeHandle);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.HighGuiTrackbarReleaseHandle(handle);
            return true;
        }
    }
}
