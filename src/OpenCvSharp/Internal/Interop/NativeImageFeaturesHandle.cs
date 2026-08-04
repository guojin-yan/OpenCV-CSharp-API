using System;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    internal sealed class NativeImageFeaturesHandle : SafeHandle
    {
        private NativeImageFeaturesHandle()
            : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        internal static NativeImageFeaturesHandle FromNativePointer(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                throw new OpenCvException("Native ImageFeatures handle is null.");
            }
            var result = new NativeImageFeaturesHandle();
            result.SetHandle(nativeHandle);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.StitchingImageFeaturesReleaseHandle(handle);
            return true;
        }
    }
}
