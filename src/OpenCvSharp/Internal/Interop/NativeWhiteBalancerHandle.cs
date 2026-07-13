using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal sealed class NativeWhiteBalancerHandle : SafeHandle
    {
        private NativeWhiteBalancerHandle()
            : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        internal static NativeWhiteBalancerHandle FromNativePointer(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                throw new OpenCvException("Native WhiteBalancer handle is null.");
            }

            var result = new NativeWhiteBalancerHandle();
            result.SetHandle(nativeHandle);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.XPhotoWhiteBalancerReleaseHandle(handle);
            return true;
        }
    }
}
