using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal sealed class NativeBioInspiredRetinaFastToneMappingHandle : SafeHandle
    {
        private NativeBioInspiredRetinaFastToneMappingHandle()
            : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        internal static NativeBioInspiredRetinaFastToneMappingHandle FromNativePointer(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                throw new OpenCvException("Native bioinspired RetinaFastToneMapping handle is null.");
            }

            var result = new NativeBioInspiredRetinaFastToneMappingHandle();
            result.SetHandle(nativeHandle);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.BioInspiredRetinaFastToneMappingRelease(handle);
            return true;
        }
    }
}
