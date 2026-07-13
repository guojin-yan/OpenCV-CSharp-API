using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal sealed class NativeXImgProcSelectiveSearchSegmentationHandle : SafeHandle
    {
        private NativeXImgProcSelectiveSearchSegmentationHandle()
            : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        internal static NativeXImgProcSelectiveSearchSegmentationHandle FromNativePointer(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                throw new OpenCvException("Native XImgProc SelectiveSearchSegmentation handle is null.");
            }

            var result = new NativeXImgProcSelectiveSearchSegmentationHandle();
            result.SetHandle(nativeHandle);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.XImgProcSelectiveSearchSegmentationReleaseHandle(handle);
            return true;
        }
    }
}
