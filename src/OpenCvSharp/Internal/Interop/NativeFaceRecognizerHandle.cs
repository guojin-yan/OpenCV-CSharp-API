using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal sealed class NativeFaceRecognizerHandle : SafeHandle
    {
        private NativeFaceRecognizerHandle()
            : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        internal static NativeFaceRecognizerHandle FromNativePointer(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                throw new OpenCvException("Native face recognizer handle is null.");
            }

            var result = new NativeFaceRecognizerHandle();
            result.SetHandle(nativeHandle);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.FaceRecognizerReleaseHandle(handle);
            return true;
        }
    }
}
