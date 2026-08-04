using System;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    internal sealed class NativeSvdHandle : SafeHandle
    {
        private NativeSvdHandle()
            : base(IntPtr.Zero, true)
        {
        }

        internal static NativeSvdHandle FromNativePointer(IntPtr handle)
        {
            if (handle == IntPtr.Zero)
            {
                throw new OpenCvException("Native SVD handle is null.");
            }

            var result = new NativeSvdHandle();
            result.SetHandle(handle);
            return result;
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.CoreSvdRelease(handle);
            return true;
        }
    }
}
