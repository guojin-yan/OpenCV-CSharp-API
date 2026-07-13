using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal sealed class NativeXImgProcEdgeBoxesHandle : SafeHandle
    {
        private NativeXImgProcEdgeBoxesHandle()
            : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        internal static NativeXImgProcEdgeBoxesHandle FromNativePointer(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                throw new OpenCvException("Native XImgProc EdgeBoxes handle is null.");
            }

            var result = new NativeXImgProcEdgeBoxesHandle();
            result.SetHandle(nativeHandle);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.XImgProcEdgeBoxesReleaseHandle(handle);
            return true;
        }
    }
}
