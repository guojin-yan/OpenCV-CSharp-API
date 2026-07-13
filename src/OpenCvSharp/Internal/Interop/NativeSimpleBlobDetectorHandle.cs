using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal sealed class NativeSimpleBlobDetectorHandle : SafeHandle
    {
        private NativeSimpleBlobDetectorHandle()
            : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        internal static NativeSimpleBlobDetectorHandle FromNativePointer(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                throw new OpenCvException("Native SimpleBlobDetector handle is null.");
            }

            var result = new NativeSimpleBlobDetectorHandle();
            result.SetHandle(nativeHandle);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.Features2DSimpleBlobRelease(handle);
            return true;
        }
    }
}
