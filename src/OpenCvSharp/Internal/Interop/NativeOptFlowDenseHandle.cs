using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal sealed class NativeOptFlowDenseHandle : SafeHandle
    {
        private NativeOptFlowDenseHandle()
            : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        internal static NativeOptFlowDenseHandle FromNativePointer(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                throw new OpenCvException("Native DenseOpticalFlow handle is null.");
            }

            var result = new NativeOptFlowDenseHandle();
            result.SetHandle(nativeHandle);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.OptFlowDenseReleaseHandle(handle);
            return true;
        }
    }
}
