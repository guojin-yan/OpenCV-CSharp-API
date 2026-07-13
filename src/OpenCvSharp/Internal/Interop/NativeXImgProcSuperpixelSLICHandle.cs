using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal sealed class NativeXImgProcSuperpixelSLICHandle : SafeHandle
    {
        private NativeXImgProcSuperpixelSLICHandle()
            : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        internal static NativeXImgProcSuperpixelSLICHandle FromNativePointer(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                throw new OpenCvException("Native XImgProc SuperpixelSLIC handle is null.");
            }

            var result = new NativeXImgProcSuperpixelSLICHandle();
            result.SetHandle(nativeHandle);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.XImgProcSuperpixelSLICReleaseHandle(handle);
            return true;
        }
    }
}
