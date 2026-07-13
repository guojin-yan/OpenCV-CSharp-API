using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal sealed class NativeFaceStandardCollectorHandle : SafeHandle
    {
        private NativeFaceStandardCollectorHandle()
            : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        internal static NativeFaceStandardCollectorHandle FromNativePointer(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                throw new OpenCvException("Native face StandardCollector handle is null.");
            }

            var result = new NativeFaceStandardCollectorHandle();
            result.SetHandle(nativeHandle);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.FaceStandardCollectorReleaseHandle(handle);
            return true;
        }
    }
}
