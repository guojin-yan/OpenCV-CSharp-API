using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal sealed class NativeRgbdNormalsHandle : SafeHandle
    {
        private NativeRgbdNormalsHandle()
            : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        internal static NativeRgbdNormalsHandle FromNativePointer(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                throw new OpenCvException("Native RgbdNormals handle is null.");
            }

            var result = new NativeRgbdNormalsHandle();
            result.SetHandle(nativeHandle);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.RgbdNormalsReleaseHandle(handle);
            return true;
        }
    }
}
