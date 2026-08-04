using System;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    internal sealed class NativeXImgProcSuperpixelLSCHandle : SafeHandle
    {
        private NativeXImgProcSuperpixelLSCHandle()
            : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        internal static NativeXImgProcSuperpixelLSCHandle FromNativePointer(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                throw new OpenCvException("Native XImgProc SuperpixelLSC handle is null.");
            }

            var result = new NativeXImgProcSuperpixelLSCHandle();
            result.SetHandle(nativeHandle);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.XImgProcSuperpixelLSCReleaseHandle(handle);
            return true;
        }
    }
}
