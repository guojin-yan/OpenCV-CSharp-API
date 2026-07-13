using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal sealed class NativeAkazeHandle : SafeHandle
    {
        private NativeAkazeHandle()
            : base(IntPtr.Zero, true)
        {
        }

        internal static NativeAkazeHandle FromNativePointer(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                throw new OpenCvException("Native AKAZE handle is null.");
            }

            var result = new NativeAkazeHandle();
            result.SetHandle(nativeHandle);
            return result;
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.Features2DAkazeRelease(handle);
            return true;
        }
    }
}
