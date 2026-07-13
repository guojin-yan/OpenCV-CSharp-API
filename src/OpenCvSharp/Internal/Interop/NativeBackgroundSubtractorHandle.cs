using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal sealed class NativeBackgroundSubtractorHandle : SafeHandle
    {
        private NativeBackgroundSubtractorHandle()
            : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        internal static NativeBackgroundSubtractorHandle FromNativePointer(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                throw new OpenCvException("Native BackgroundSubtractor handle is null.");
            }

            var result = new NativeBackgroundSubtractorHandle();
            result.SetHandle(nativeHandle);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.BackgroundSubtractorReleaseHandle(handle);
            return true;
        }
    }
}
