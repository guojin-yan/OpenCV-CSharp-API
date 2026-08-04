using System;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    internal sealed class NativeRegMapHandle : SafeHandle
    {
        private NativeRegMapHandle()
            : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        internal static NativeRegMapHandle FromNativePointer(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                throw new OpenCvException("Native reg map handle is null.");
            }

            var result = new NativeRegMapHandle();
            result.SetHandle(nativeHandle);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.RegMapRelease(handle);
            return true;
        }
    }
}
