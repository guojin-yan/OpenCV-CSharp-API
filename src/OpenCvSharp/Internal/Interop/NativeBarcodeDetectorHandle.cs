using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal sealed class NativeBarcodeDetectorHandle : SafeHandle
    {
        private NativeBarcodeDetectorHandle()
            : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        internal static NativeBarcodeDetectorHandle FromNativePointer(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                throw new OpenCvException("Native BarcodeDetector handle is null.");
            }

            var result = new NativeBarcodeDetectorHandle();
            result.SetHandle(nativeHandle);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.BarcodeDetectorReleaseHandle(handle);
            return true;
        }
    }
}
