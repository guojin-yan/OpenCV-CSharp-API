using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal enum HdrPhotoHandleKind
    {
        AlignMtb,
        CalibrateCrf,
        MergeExposures,
    }

    internal sealed class NativeHdrPhotoHandle : SafeHandle
    {
        private readonly HdrPhotoHandleKind kind;

        private NativeHdrPhotoHandle(HdrPhotoHandleKind kind)
            : base(IntPtr.Zero, true)
        {
            this.kind = kind;
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        internal static NativeHdrPhotoHandle FromNativePointer(IntPtr nativeHandle, HdrPhotoHandleKind kind)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                throw new OpenCvException("Native HDR Photo handle is null.");
            }

            var result = new NativeHdrPhotoHandle(kind);
            result.SetHandle(nativeHandle);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            switch (kind)
            {
                case HdrPhotoHandleKind.AlignMtb:
                    NativeMethods.AlignMtbReleaseHandle(handle);
                    break;
                case HdrPhotoHandleKind.CalibrateCrf:
                    NativeMethods.CalibrateCrfReleaseHandle(handle);
                    break;
                case HdrPhotoHandleKind.MergeExposures:
                    NativeMethods.MergeExposuresReleaseHandle(handle);
                    break;
            }
            return true;
        }
    }
}
