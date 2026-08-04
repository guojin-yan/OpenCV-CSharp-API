using System;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    internal sealed class NativeXImgProcSelectiveSearchStrategyHandle : SafeHandle
    {
        private NativeXImgProcSelectiveSearchStrategyHandle()
            : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        internal static NativeXImgProcSelectiveSearchStrategyHandle FromNativePointer(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                throw new OpenCvException("Native XImgProc SelectiveSearchStrategy handle is null.");
            }

            var result = new NativeXImgProcSelectiveSearchStrategyHandle();
            result.SetHandle(nativeHandle);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.XImgProcSelectiveSearchStrategyReleaseHandle(handle);
            return true;
        }
    }
}
