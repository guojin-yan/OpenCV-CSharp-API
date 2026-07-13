using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal sealed class NativeClaheHandle : SafeHandle
    {
        private NativeClaheHandle()
            : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        internal static NativeClaheHandle FromNativePointer(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                throw new OpenCvException("Native CLAHE handle is null.");
            }

            var result = new NativeClaheHandle();
            result.SetHandle(nativeHandle);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.ImgProcClaheRelease(handle);
            return true;
        }
    }
}
