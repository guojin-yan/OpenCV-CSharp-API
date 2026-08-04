using System;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    internal sealed class NativeBioInspiredTransientAreasHandle : SafeHandle
    {
        private NativeBioInspiredTransientAreasHandle()
            : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        internal static NativeBioInspiredTransientAreasHandle FromNativePointer(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                throw new OpenCvException("Native bioinspired TransientAreasSegmentationModule handle is null.");
            }

            var result = new NativeBioInspiredTransientAreasHandle();
            result.SetHandle(nativeHandle);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.BioInspiredTransientAreasRelease(handle);
            return true;
        }
    }
}
