using System;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    internal sealed class NativeBriskHandle : SafeHandle
    {
        private NativeBriskHandle()
            : base(IntPtr.Zero, true)
        {
        }

        internal static NativeBriskHandle FromNativePointer(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                throw new OpenCvException("Native BRISK handle is null.");
            }

            var result = new NativeBriskHandle();
            result.SetHandle(nativeHandle);
            return result;
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.Features2DBriskRelease(handle);
            return true;
        }
    }
}
