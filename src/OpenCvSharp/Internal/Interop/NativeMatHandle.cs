using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal sealed class NativeMatHandle : SafeHandle
    {
        internal NativeMatHandle()
            : base(IntPtr.Zero, true)
        {
        }

        private NativeMatHandle(IntPtr handle)
            : base(IntPtr.Zero, true)
        {
            SetHandle(handle);
        }

        internal static NativeMatHandle FromNativePointer(IntPtr handle)
        {
            if (handle == IntPtr.Zero)
            {
                throw new OpenCvException("Native Mat handle is null.");
            }

            return new NativeMatHandle(handle);
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        protected override bool ReleaseHandle()
        {
            if (!IsInvalid)
            {
                NativeMethods.MatRelease(handle);
            }

            return true;
        }
    }
}
