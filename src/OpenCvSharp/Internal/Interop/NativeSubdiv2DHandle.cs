using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal sealed class NativeSubdiv2DHandle : SafeHandle
    {
        private NativeSubdiv2DHandle()
            : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid { get { return handle == IntPtr.Zero; } }

        internal static NativeSubdiv2DHandle FromNativePointer(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                throw new OpenCvException("Native Subdiv2D handle is null.");
            }
            var result = new NativeSubdiv2DHandle();
            result.SetHandle(nativeHandle);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.Calib3DSubdiv2DRelease(handle);
            return true;
        }
    }
}
