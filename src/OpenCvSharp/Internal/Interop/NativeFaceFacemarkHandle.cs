using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal sealed class NativeFaceFacemarkHandle : SafeHandle
    {
        private NativeFaceFacemarkHandle()
            : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        internal static NativeFaceFacemarkHandle FromNativePointer(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                throw new OpenCvException("Native face Facemark handle is null.");
            }

            var result = new NativeFaceFacemarkHandle();
            result.SetHandle(nativeHandle);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.FaceFacemarkReleaseHandle(handle);
            return true;
        }
    }
}
