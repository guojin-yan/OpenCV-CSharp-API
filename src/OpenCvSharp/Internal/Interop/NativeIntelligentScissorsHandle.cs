using System;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    internal sealed class NativeIntelligentScissorsHandle : SafeHandle
    {
        private NativeIntelligentScissorsHandle()
            : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        internal static NativeIntelligentScissorsHandle FromNativePointer(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                throw new OpenCvException("Native IntelligentScissorsMB handle is null.");
            }

            var result = new NativeIntelligentScissorsHandle();
            result.SetHandle(nativeHandle);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.PhotoIntelligentScissorsReleaseHandle(handle);
            return true;
        }
    }
}
