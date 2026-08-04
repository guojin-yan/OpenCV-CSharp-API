using System;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    internal sealed class NativeMlParamGridHandle : SafeHandle
    {
        private NativeMlParamGridHandle()
            : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        internal static NativeMlParamGridHandle FromNativePointer(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                throw new OpenCvException("Native ML ParamGrid handle is null.");
            }

            var result = new NativeMlParamGridHandle();
            result.SetHandle(nativeHandle);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.MlParamGridReleaseHandle(handle);
            return true;
        }
    }
}
