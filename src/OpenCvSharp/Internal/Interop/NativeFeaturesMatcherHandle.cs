using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal sealed class NativeFeaturesMatcherHandle : SafeHandle
    {
        private NativeFeaturesMatcherHandle()
            : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        internal static NativeFeaturesMatcherHandle FromNativePointer(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                throw new OpenCvException("Native FeaturesMatcher handle is null.");
            }
            var result = new NativeFeaturesMatcherHandle();
            result.SetHandle(nativeHandle);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.StitchingFeaturesMatcherReleaseHandle(handle);
            return true;
        }
    }
}
