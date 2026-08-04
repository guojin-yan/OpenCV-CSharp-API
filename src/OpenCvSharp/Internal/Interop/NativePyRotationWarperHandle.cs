using System;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    internal sealed class NativePyRotationWarperHandle : SafeHandle
    {
        private NativePyRotationWarperHandle()
            : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        internal static NativePyRotationWarperHandle FromNativePointer(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                throw new OpenCvException("Native PyRotationWarper handle is null.");
            }

            var result = new NativePyRotationWarperHandle();
            result.SetHandle(nativeHandle);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.StitchingPyRotationWarperReleaseHandle(handle);
            return true;
        }
    }
}
