using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal sealed class NativeQRCodeDetectorArucoHandle : SafeHandle
    {
        private NativeQRCodeDetectorArucoHandle()
            : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        internal static NativeQRCodeDetectorArucoHandle FromNativePointer(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                throw new OpenCvException("Native QRCodeDetectorAruco handle is null.");
            }

            var result = new NativeQRCodeDetectorArucoHandle();
            result.SetHandle(nativeHandle);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.QRCodeDetectorArucoReleaseHandle(handle);
            return true;
        }
    }
}
