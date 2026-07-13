using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal sealed class NativeXImgProcDisparityWLSFilterHandle : SafeHandle
    {
        private NativeXImgProcDisparityWLSFilterHandle()
            : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        internal static NativeXImgProcDisparityWLSFilterHandle FromNativePointer(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                throw new OpenCvException("Native XImgProc DisparityWLSFilter handle is null.");
            }

            var result = new NativeXImgProcDisparityWLSFilterHandle();
            result.SetHandle(nativeHandle);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.XImgProcDisparityWLSFilterReleaseHandle(handle);
            return true;
        }
    }
}
