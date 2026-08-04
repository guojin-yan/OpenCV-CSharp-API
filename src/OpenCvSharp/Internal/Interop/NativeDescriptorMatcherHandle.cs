using System;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    internal sealed class NativeDescriptorMatcherHandle : SafeHandle
    {
        private NativeDescriptorMatcherHandle()
            : base(IntPtr.Zero, true)
        {
        }

        internal static NativeDescriptorMatcherHandle FromNativePointer(IntPtr handle)
        {
            if (handle == IntPtr.Zero)
            {
                throw new OpenCvException("Native DescriptorMatcher handle is null.");
            }

            var result = new NativeDescriptorMatcherHandle();
            result.SetHandle(handle);
            return result;
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.Features2DDescriptorMatcherRelease(handle);
            return true;
        }
    }
}
