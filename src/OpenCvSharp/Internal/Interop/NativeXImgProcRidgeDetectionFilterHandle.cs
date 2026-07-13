using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal sealed class NativeXImgProcRidgeDetectionFilterHandle : SafeHandle
    {
        private NativeXImgProcRidgeDetectionFilterHandle()
            : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        internal static NativeXImgProcRidgeDetectionFilterHandle FromNativePointer(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                throw new OpenCvException("Native XImgProc RidgeDetectionFilter handle is null.");
            }

            var result = new NativeXImgProcRidgeDetectionFilterHandle();
            result.SetHandle(nativeHandle);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.XImgProcRidgeDetectionFilterReleaseHandle(handle);
            return true;
        }
    }
}
