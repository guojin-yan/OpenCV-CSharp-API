using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal sealed class NativeFaceDetectorYNHandle : SafeHandle
    {
        private NativeFaceDetectorYNHandle()
            : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        internal static NativeFaceDetectorYNHandle FromNativePointer(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                throw new OpenCvException("Native FaceDetectorYN handle is null.");
            }

            var result = new NativeFaceDetectorYNHandle();
            result.SetHandle(nativeHandle);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.FaceDetectorYNReleaseHandle(handle);
            return true;
        }
    }
}
