using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal sealed class NativeFastFeatureDetectorHandle : SafeHandle
    {
        private NativeFastFeatureDetectorHandle()
            : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        internal static NativeFastFeatureDetectorHandle FromNativePointer(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                throw new OpenCvException("Native FastFeatureDetector handle is null.");
            }

            var result = new NativeFastFeatureDetectorHandle();
            result.SetHandle(nativeHandle);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.Features2DFastRelease(handle);
            return true;
        }
    }
}
