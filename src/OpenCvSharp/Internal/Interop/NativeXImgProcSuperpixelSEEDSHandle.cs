using System;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    internal sealed class NativeXImgProcSuperpixelSEEDSHandle : SafeHandle
    {
        private NativeXImgProcSuperpixelSEEDSHandle()
            : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        internal static NativeXImgProcSuperpixelSEEDSHandle FromNativePointer(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                throw new OpenCvException("Native XImgProc SuperpixelSEEDS handle is null.");
            }

            var result = new NativeXImgProcSuperpixelSEEDSHandle();
            result.SetHandle(nativeHandle);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.XImgProcSuperpixelSEEDSReleaseHandle(handle);
            return true;
        }
    }
}
