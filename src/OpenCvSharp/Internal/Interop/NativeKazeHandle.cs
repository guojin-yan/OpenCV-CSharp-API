using System;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    internal sealed class NativeKazeHandle : SafeHandle
    {
        private NativeKazeHandle()
            : base(IntPtr.Zero, true)
        {
        }

        internal static NativeKazeHandle FromNativePointer(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                throw new OpenCvException("Native KAZE handle is null.");
            }

            var result = new NativeKazeHandle();
            result.SetHandle(nativeHandle);
            return result;
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.Features2DKazeRelease(handle);
            return true;
        }
    }
}
