using System;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    internal sealed class NativeSurfaceMatchingIcpHandle : SafeHandle
    {
        private NativeSurfaceMatchingIcpHandle()
            : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        internal static NativeSurfaceMatchingIcpHandle FromNativePointer(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                throw new OpenCvException("Native ICP handle is null.");
            }

            var result = new NativeSurfaceMatchingIcpHandle();
            result.SetHandle(nativeHandle);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.SurfaceMatchingIcpRelease(handle);
            return true;
        }
    }
}
