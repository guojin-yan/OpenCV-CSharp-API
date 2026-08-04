using System;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    internal sealed class NativeMccCheckerDetectorHandle : SafeHandle
    {
        private NativeMccCheckerDetectorHandle()
            : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        internal static NativeMccCheckerDetectorHandle FromNativePointer(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                throw new OpenCvException("Native MCC CCheckerDetector handle is null.");
            }

            var result = new NativeMccCheckerDetectorHandle();
            result.SetHandle(nativeHandle);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.MccCheckerDetectorReleaseHandle(handle);
            return true;
        }
    }
}
