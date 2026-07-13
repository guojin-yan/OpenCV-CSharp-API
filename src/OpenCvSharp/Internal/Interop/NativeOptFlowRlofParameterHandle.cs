using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal sealed class NativeOptFlowRlofParameterHandle : SafeHandle
    {
        private NativeOptFlowRlofParameterHandle()
            : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        internal static NativeOptFlowRlofParameterHandle FromNativePointer(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                throw new OpenCvException("Native RLOFOpticalFlowParameter handle is null.");
            }

            var result = new NativeOptFlowRlofParameterHandle();
            result.SetHandle(nativeHandle);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.OptFlowRlofParameterReleaseHandle(handle);
            return true;
        }
    }
}
