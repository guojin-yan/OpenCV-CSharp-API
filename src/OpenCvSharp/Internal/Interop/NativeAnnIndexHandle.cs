using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal sealed class NativeAnnIndexHandle : SafeHandle
    {
        private NativeAnnIndexHandle()
            : base(IntPtr.Zero, true)
        {
        }

        internal static NativeAnnIndexHandle FromNativePointer(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                throw new OpenCvException("Native ANNIndex handle is null.");
            }

            var result = new NativeAnnIndexHandle();
            result.SetHandle(nativeHandle);
            return result;
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.Features2DAnnIndexRelease(handle);
            return true;
        }
    }
}
