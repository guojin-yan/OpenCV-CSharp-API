using System;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    internal sealed class NativeXImgProcEdgeDrawingHandle : SafeHandle
    {
        private NativeXImgProcEdgeDrawingHandle()
            : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        internal static NativeXImgProcEdgeDrawingHandle FromNativePointer(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                throw new OpenCvException("Native XImgProc EdgeDrawing handle is null.");
            }

            var result = new NativeXImgProcEdgeDrawingHandle();
            result.SetHandle(nativeHandle);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.XImgProcEdgeDrawingReleaseHandle(handle);
            return true;
        }
    }
}
