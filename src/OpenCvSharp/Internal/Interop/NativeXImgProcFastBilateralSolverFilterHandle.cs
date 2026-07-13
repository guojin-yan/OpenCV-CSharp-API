using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal sealed class NativeXImgProcFastBilateralSolverFilterHandle : SafeHandle
    {
        private NativeXImgProcFastBilateralSolverFilterHandle()
            : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        internal static NativeXImgProcFastBilateralSolverFilterHandle FromNativePointer(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                throw new OpenCvException("Native XImgProc FastBilateralSolverFilter handle is null.");
            }

            var result = new NativeXImgProcFastBilateralSolverFilterHandle();
            result.SetHandle(nativeHandle);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.XImgProcFastBilateralSolverFilterReleaseHandle(handle);
            return true;
        }
    }
}
