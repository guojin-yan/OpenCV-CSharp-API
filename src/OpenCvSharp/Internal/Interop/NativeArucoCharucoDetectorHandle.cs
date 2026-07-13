using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal sealed class NativeArucoCharucoDetectorHandle : SafeHandle
    {
        private NativeArucoCharucoDetectorHandle()
            : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        internal static NativeArucoCharucoDetectorHandle FromNativePointer(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                throw new OpenCvException("Native Aruco CharucoDetector handle is null.");
            }

            var result = new NativeArucoCharucoDetectorHandle();
            result.SetHandle(nativeHandle);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.ArucoCharucoDetectorReleaseHandle(handle);
            return true;
        }
    }
}
