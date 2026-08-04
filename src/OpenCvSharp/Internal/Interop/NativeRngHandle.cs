using System;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    internal sealed class NativeRngHandle : SafeHandle
    {
        private NativeRngHandle()
            : base(IntPtr.Zero, true)
        {
        }

        internal static NativeRngHandle FromNativePointer(IntPtr handle)
        {
            if (handle == IntPtr.Zero)
            {
                throw new OpenCvException("Native RNG handle is null.");
            }

            var result = new NativeRngHandle();
            result.SetHandle(handle);
            return result;
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.CoreRngRelease(handle);
            return true;
        }
    }
}
