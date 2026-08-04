using System;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    internal sealed class NativeCascadeClassifierHandle : SafeHandle
    {
        private NativeCascadeClassifierHandle()
            : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        internal static NativeCascadeClassifierHandle FromNativePointer(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                throw new OpenCvException("Native CascadeClassifier handle is null.");
            }

            var result = new NativeCascadeClassifierHandle();
            result.SetHandle(nativeHandle);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.CascadeClassifierReleaseHandle(handle);
            return true;
        }
    }
}
