using System;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    internal sealed class NativeBgSegmSyntheticSequenceGeneratorHandle : SafeHandle
    {
        private NativeBgSegmSyntheticSequenceGeneratorHandle()
            : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        internal static NativeBgSegmSyntheticSequenceGeneratorHandle FromNativePointer(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                throw new OpenCvException("Native SyntheticSequenceGenerator handle is null.");
            }

            var result = new NativeBgSegmSyntheticSequenceGeneratorHandle();
            result.SetHandle(nativeHandle);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.BgSegmSyntheticSequenceGeneratorReleaseHandle(handle);
            return true;
        }
    }
}
