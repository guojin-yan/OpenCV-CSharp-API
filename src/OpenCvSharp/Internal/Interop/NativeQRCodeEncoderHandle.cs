using System;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    internal sealed class NativeQRCodeEncoderHandle : SafeHandle
    {
        private NativeQRCodeEncoderHandle()
            : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        internal static NativeQRCodeEncoderHandle FromNativePointer(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                throw new OpenCvException("Native QRCodeEncoder handle is null.");
            }

            var result = new NativeQRCodeEncoderHandle();
            result.SetHandle(nativeHandle);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.QRCodeEncoderReleaseHandle(handle);
            return true;
        }
    }
}
