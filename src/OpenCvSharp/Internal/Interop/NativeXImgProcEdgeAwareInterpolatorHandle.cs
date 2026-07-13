using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal sealed class NativeXImgProcEdgeAwareInterpolatorHandle : SafeHandle
    {
        private NativeXImgProcEdgeAwareInterpolatorHandle()
            : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        internal static NativeXImgProcEdgeAwareInterpolatorHandle FromNativePointer(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                throw new OpenCvException("Native XImgProc EdgeAwareInterpolator handle is null.");
            }

            var result = new NativeXImgProcEdgeAwareInterpolatorHandle();
            result.SetHandle(nativeHandle);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.XImgProcEdgeAwareInterpolatorReleaseHandle(handle);
            return true;
        }
    }
}
