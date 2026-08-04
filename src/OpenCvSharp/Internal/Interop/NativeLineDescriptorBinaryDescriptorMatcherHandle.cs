using System;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    internal sealed class NativeLineDescriptorBinaryDescriptorMatcherHandle : SafeHandle
    {
        private NativeLineDescriptorBinaryDescriptorMatcherHandle()
            : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        internal static NativeLineDescriptorBinaryDescriptorMatcherHandle FromNativePointer(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                throw new OpenCvException("Native LineDescriptor BinaryDescriptorMatcher handle is null.");
            }

            var result = new NativeLineDescriptorBinaryDescriptorMatcherHandle();
            result.SetHandle(nativeHandle);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.LineDescriptorBinaryDescriptorMatcherRelease(handle);
            return true;
        }
    }
}
