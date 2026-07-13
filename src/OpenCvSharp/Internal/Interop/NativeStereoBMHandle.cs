using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal sealed class NativeStereoBMHandle : SafeHandle
    {
        private NativeStereoBMHandle()
            : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        internal static NativeStereoBMHandle FromNativePointer(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                throw new OpenCvException("Native StereoBM handle is null.");
            }

            var result = new NativeStereoBMHandle();
            result.SetHandle(nativeHandle);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.StereoBMRelease(handle);
            return true;
        }
    }
}
