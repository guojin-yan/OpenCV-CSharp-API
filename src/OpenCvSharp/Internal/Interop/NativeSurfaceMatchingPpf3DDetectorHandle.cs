using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal sealed class NativeSurfaceMatchingPpf3DDetectorHandle : SafeHandle
    {
        private NativeSurfaceMatchingPpf3DDetectorHandle()
            : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        internal static NativeSurfaceMatchingPpf3DDetectorHandle FromNativePointer(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                throw new OpenCvException("Native PPF3DDetector handle is null.");
            }

            var result = new NativeSurfaceMatchingPpf3DDetectorHandle();
            result.SetHandle(nativeHandle);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.SurfaceMatchingPpf3DDetectorRelease(handle);
            return true;
        }
    }
}
