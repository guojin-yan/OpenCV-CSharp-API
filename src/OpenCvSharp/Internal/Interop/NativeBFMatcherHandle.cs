using System;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    internal sealed class NativeBFMatcherHandle : SafeHandle
    {
        private NativeBFMatcherHandle()
            : base(IntPtr.Zero, true)
        {
        }

        internal static NativeBFMatcherHandle FromNativePointer(IntPtr handle)
        {
            if (handle == IntPtr.Zero)
            {
                throw new OpenCvException("Native BFMatcher handle is null.");
            }

            var result = new NativeBFMatcherHandle();
            result.SetHandle(handle);
            return result;
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.Features2DBFMatcherRelease(handle);
            return true;
        }
    }
}
