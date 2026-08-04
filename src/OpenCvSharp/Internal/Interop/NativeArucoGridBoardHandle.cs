using System;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    internal sealed class NativeArucoGridBoardHandle : SafeHandle
    {
        private NativeArucoGridBoardHandle()
            : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        internal static NativeArucoGridBoardHandle FromNativePointer(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                throw new OpenCvException("Native Aruco GridBoard handle is null.");
            }

            var result = new NativeArucoGridBoardHandle();
            result.SetHandle(nativeHandle);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.ArucoGridBoardReleaseHandle(handle);
            return true;
        }
    }
}
