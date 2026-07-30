using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal sealed class NativeVideoStreamReaderHandle : SafeHandle
    {
        private NativeVideoStreamReaderHandle()
            : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        internal static NativeVideoStreamReaderHandle FromNativePointer(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                throw new OpenCvException("Native Video stream reader handle is null.");
            }

            var result = new NativeVideoStreamReaderHandle();
            result.SetHandle(nativeHandle);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.VideoStreamReaderReleaseHandle(handle);
            return true;
        }
    }
}
