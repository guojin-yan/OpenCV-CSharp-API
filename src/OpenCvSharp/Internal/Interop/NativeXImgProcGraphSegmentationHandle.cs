using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal sealed class NativeXImgProcGraphSegmentationHandle : SafeHandle
    {
        private NativeXImgProcGraphSegmentationHandle()
            : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        internal static NativeXImgProcGraphSegmentationHandle FromNativePointer(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                throw new OpenCvException("Native XImgProc GraphSegmentation handle is null.");
            }

            var result = new NativeXImgProcGraphSegmentationHandle();
            result.SetHandle(nativeHandle);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.XImgProcGraphSegmentationReleaseHandle(handle);
            return true;
        }
    }
}
