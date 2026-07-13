using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal sealed class NativeFlannBasedMatcherHandle : SafeHandle
    {
        private NativeFlannBasedMatcherHandle()
            : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        internal static NativeFlannBasedMatcherHandle FromNativePointer(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                throw new OpenCvException("Native FlannBasedMatcher handle is null.");
            }

            var result = new NativeFlannBasedMatcherHandle();
            result.SetHandle(nativeHandle);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.Features2DFlannMatcherRelease(handle);
            return true;
        }
    }
}
