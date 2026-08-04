using System;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    internal sealed class NativeBgSegmBackgroundSubtractorHandle : SafeHandle
    {
        private NativeBgSegmBackgroundSubtractorHandle()
            : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        internal static NativeBgSegmBackgroundSubtractorHandle FromNativePointer(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                throw new OpenCvException("Native bgsegm BackgroundSubtractor handle is null.");
            }

            var result = new NativeBgSegmBackgroundSubtractorHandle();
            result.SetHandle(nativeHandle);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.BgSegmBackgroundSubtractorReleaseHandle(handle);
            return true;
        }
    }
}
