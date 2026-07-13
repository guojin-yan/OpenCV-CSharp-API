using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal sealed class NativeLineDescriptorBinaryDescriptorHandle : SafeHandle
    {
        private NativeLineDescriptorBinaryDescriptorHandle()
            : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        internal static NativeLineDescriptorBinaryDescriptorHandle FromNativePointer(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                throw new OpenCvException("Native LineDescriptor BinaryDescriptor handle is null.");
            }

            var result = new NativeLineDescriptorBinaryDescriptorHandle();
            result.SetHandle(nativeHandle);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.LineDescriptorBinaryDescriptorRelease(handle);
            return true;
        }
    }
}
