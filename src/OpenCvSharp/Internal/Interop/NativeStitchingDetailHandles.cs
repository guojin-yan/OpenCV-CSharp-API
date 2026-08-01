using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal sealed class NativeSeamFinderHandle : SafeHandle
    {
        private NativeSeamFinderHandle() : base(IntPtr.Zero, true) { }
        public override bool IsInvalid => handle == IntPtr.Zero;
        internal static NativeSeamFinderHandle FromNativePointer(IntPtr value)
        {
            if (value == IntPtr.Zero) throw new OpenCvException("Native SeamFinder handle is null.");
            var result = new NativeSeamFinderHandle(); result.SetHandle(value); return result;
        }
        protected override bool ReleaseHandle()
        {
            NativeMethods.StitchingSeamFinderReleaseHandle(handle); return true;
        }
    }

    internal sealed class NativeTimelapserHandle : SafeHandle
    {
        private NativeTimelapserHandle() : base(IntPtr.Zero, true) { }
        public override bool IsInvalid => handle == IntPtr.Zero;
        internal static NativeTimelapserHandle FromNativePointer(IntPtr value)
        {
            if (value == IntPtr.Zero) throw new OpenCvException("Native Timelapser handle is null.");
            var result = new NativeTimelapserHandle(); result.SetHandle(value); return result;
        }
        protected override bool ReleaseHandle()
        {
            NativeMethods.StitchingTimelapserReleaseHandle(handle); return true;
        }
    }

    internal sealed class NativeSphericalProjectorHandle : SafeHandle
    {
        private NativeSphericalProjectorHandle() : base(IntPtr.Zero, true) { }
        public override bool IsInvalid => handle == IntPtr.Zero;
        internal static NativeSphericalProjectorHandle FromNativePointer(IntPtr value)
        {
            if (value == IntPtr.Zero) throw new OpenCvException("Native SphericalProjector handle is null.");
            var result = new NativeSphericalProjectorHandle(); result.SetHandle(value); return result;
        }
        protected override bool ReleaseHandle()
        {
            NativeMethods.StitchingSphericalProjectorReleaseHandle(handle); return true;
        }
    }
}
