using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal sealed class NativeOptFlowSparseHandle : SafeHandle
    {
        private NativeOptFlowSparseHandle()
            : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        internal static NativeOptFlowSparseHandle FromNativePointer(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                throw new OpenCvException("Native SparseOpticalFlow handle is null.");
            }

            var result = new NativeOptFlowSparseHandle();
            result.SetHandle(nativeHandle);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.OptFlowSparseReleaseHandle(handle);
            return true;
        }
    }
}
