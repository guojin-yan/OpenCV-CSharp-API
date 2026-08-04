using System;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    internal sealed class NativeXStereoBinaryBMHandle : SafeHandle
    {
        private NativeXStereoBinaryBMHandle()
            : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        internal static NativeXStereoBinaryBMHandle FromNativePointer(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                throw new OpenCvException("Native xstereo StereoBinaryBM handle is null.");
            }

            var result = new NativeXStereoBinaryBMHandle();
            result.SetHandle(nativeHandle);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.XStereoBinaryBMRelease(handle);
            return true;
        }
    }
}
