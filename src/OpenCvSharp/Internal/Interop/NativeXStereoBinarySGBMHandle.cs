using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal sealed class NativeXStereoBinarySGBMHandle : SafeHandle
    {
        private NativeXStereoBinarySGBMHandle()
            : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        internal static NativeXStereoBinarySGBMHandle FromNativePointer(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                throw new OpenCvException("Native xstereo StereoBinarySGBM handle is null.");
            }

            var result = new NativeXStereoBinarySGBMHandle();
            result.SetHandle(nativeHandle);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.XStereoBinarySGBMRelease(handle);
            return true;
        }
    }
}
