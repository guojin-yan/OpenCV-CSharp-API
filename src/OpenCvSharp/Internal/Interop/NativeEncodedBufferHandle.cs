using System;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    internal sealed class NativeEncodedBufferHandle : SafeHandle
    {
        private NativeEncodedBufferHandle()
            : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        internal static NativeEncodedBufferHandle FromNativePointer(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                throw new ArgumentNullException(nameof(nativeHandle));
            }

            var handle = new NativeEncodedBufferHandle();
            handle.SetHandle(nativeHandle);
            return handle;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.EncodedBufferRelease(handle);
            return true;
        }
    }
}
