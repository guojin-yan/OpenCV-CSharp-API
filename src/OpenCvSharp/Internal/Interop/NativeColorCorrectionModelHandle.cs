using System;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    internal sealed class NativeColorCorrectionModelHandle : SafeHandle
    {
        private NativeColorCorrectionModelHandle()
            : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        internal static NativeColorCorrectionModelHandle FromNativePointer(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                throw new OpenCvException("Native ColorCorrectionModel handle is null.");
            }

            var result = new NativeColorCorrectionModelHandle();
            result.SetHandle(nativeHandle);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.PhotoCcmReleaseHandle(handle);
            return true;
        }
    }
}
