using System;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    internal sealed class NativeXStereoQuasiDenseStereoHandle : SafeHandle
    {
        private NativeXStereoQuasiDenseStereoHandle()
            : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        internal static NativeXStereoQuasiDenseStereoHandle FromNativePointer(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                throw new OpenCvException("Native xstereo QuasiDenseStereo handle is null.");
            }

            var result = new NativeXStereoQuasiDenseStereoHandle();
            result.SetHandle(nativeHandle);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.XStereoQuasiDenseRelease(handle);
            return true;
        }
    }
}
