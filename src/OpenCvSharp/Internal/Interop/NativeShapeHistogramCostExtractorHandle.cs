using System;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    internal sealed class NativeShapeHistogramCostExtractorHandle : SafeHandle
    {
        private NativeShapeHistogramCostExtractorHandle()
            : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        internal static NativeShapeHistogramCostExtractorHandle FromNativePointer(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                throw new OpenCvException("Native Shape HistogramCostExtractor handle is null.");
            }

            var result = new NativeShapeHistogramCostExtractorHandle();
            result.SetHandle(nativeHandle);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.ShapeHistogramCostExtractorReleaseHandle(handle);
            return true;
        }
    }
}
