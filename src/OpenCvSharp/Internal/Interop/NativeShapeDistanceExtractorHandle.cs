using System;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    internal sealed class NativeShapeDistanceExtractorHandle : SafeHandle
    {
        private NativeShapeDistanceExtractorHandle()
            : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        internal static NativeShapeDistanceExtractorHandle FromNativePointer(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                throw new OpenCvException("Native ShapeDistanceExtractor handle is null.");
            }

            var result = new NativeShapeDistanceExtractorHandle();
            result.SetHandle(nativeHandle);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.ShapeDistanceExtractorReleaseHandle(handle);
            return true;
        }
    }
}
