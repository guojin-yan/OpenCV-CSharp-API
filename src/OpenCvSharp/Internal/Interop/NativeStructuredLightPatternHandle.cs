using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal sealed class NativeStructuredLightPatternHandle : SafeHandle
    {
        private NativeStructuredLightPatternHandle()
            : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        internal static NativeStructuredLightPatternHandle FromNativePointer(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                throw new OpenCvException("Native StructuredLightPattern handle is null.");
            }

            var result = new NativeStructuredLightPatternHandle();
            result.SetHandle(nativeHandle);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.StructuredLightPatternRelease(handle);
            return true;
        }
    }
}
