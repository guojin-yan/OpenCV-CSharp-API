using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal sealed class NativePhaseUnwrappingHandle : SafeHandle
    {
        private NativePhaseUnwrappingHandle()
            : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        internal static NativePhaseUnwrappingHandle FromNativePointer(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                throw new OpenCvException("Native PhaseUnwrapping handle is null.");
            }

            var result = new NativePhaseUnwrappingHandle();
            result.SetHandle(nativeHandle);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.PhaseUnwrappingRelease(handle);
            return true;
        }
    }
}
