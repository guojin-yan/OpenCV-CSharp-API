using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal sealed class NativeBioInspiredRetinaHandle : SafeHandle
    {
        private NativeBioInspiredRetinaHandle()
            : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        internal static NativeBioInspiredRetinaHandle FromNativePointer(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                throw new OpenCvException("Native bioinspired Retina handle is null.");
            }

            var result = new NativeBioInspiredRetinaHandle();
            result.SetHandle(nativeHandle);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.BioInspiredRetinaRelease(handle);
            return true;
        }
    }
}
