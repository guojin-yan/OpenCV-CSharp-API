using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal sealed class NativeGeneralizedHoughHandle : SafeHandle
    {
        private NativeGeneralizedHoughHandle()
            : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        internal static NativeGeneralizedHoughHandle FromNativePointer(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                throw new OpenCvException("Native GeneralizedHough handle is null.");
            }

            var result = new NativeGeneralizedHoughHandle();
            result.SetHandle(nativeHandle);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.ImgProcGeneralizedHoughRelease(handle);
            return true;
        }
    }
}
