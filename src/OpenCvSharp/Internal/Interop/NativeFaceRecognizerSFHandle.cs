using System;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    internal sealed class NativeFaceRecognizerSFHandle : SafeHandle
    {
        private NativeFaceRecognizerSFHandle()
            : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        internal static NativeFaceRecognizerSFHandle FromNativePointer(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                throw new OpenCvException("Native FaceRecognizerSF handle is null.");
            }

            var result = new NativeFaceRecognizerSFHandle();
            result.SetHandle(nativeHandle);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.FaceRecognizerSFReleaseHandle(handle);
            return true;
        }
    }
}
