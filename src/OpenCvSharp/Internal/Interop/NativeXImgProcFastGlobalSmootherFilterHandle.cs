using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal sealed class NativeXImgProcFastGlobalSmootherFilterHandle : SafeHandle
    {
        private NativeXImgProcFastGlobalSmootherFilterHandle()
            : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        internal static NativeXImgProcFastGlobalSmootherFilterHandle FromNativePointer(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                throw new OpenCvException("Native XImgProc FastGlobalSmootherFilter handle is null.");
            }

            var result = new NativeXImgProcFastGlobalSmootherFilterHandle();
            result.SetHandle(nativeHandle);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.XImgProcFastGlobalSmootherFilterReleaseHandle(handle);
            return true;
        }
    }
}
