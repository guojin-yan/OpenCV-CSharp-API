using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal sealed class NativeOrbHandle : SafeHandle
    {
        private NativeOrbHandle()
            : base(IntPtr.Zero, true)
        {
        }

        internal static NativeOrbHandle FromNativePointer(IntPtr handle)
        {
            if (handle == IntPtr.Zero)
            {
                throw new OpenCvException("Native ORB handle is null.");
            }

            var result = new NativeOrbHandle();
            result.SetHandle(handle);
            return result;
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.Features2DOrbRelease(handle);
            return true;
        }
    }
}
