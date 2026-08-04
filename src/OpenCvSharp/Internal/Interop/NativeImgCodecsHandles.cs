using System;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    internal sealed class NativeImgCodecsMatVectorHandle : SafeHandle
    {
        private NativeImgCodecsMatVectorHandle() : base(IntPtr.Zero, true) { }
        public override bool IsInvalid { get { return handle == IntPtr.Zero; } }

        internal static NativeImgCodecsMatVectorHandle FromNativePointer(IntPtr value)
        {
            if (value == IntPtr.Zero) throw new OpenCvException("Native image vector handle is null.");
            var result = new NativeImgCodecsMatVectorHandle();
            result.SetHandle(value);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.ImgCodecsMatVectorRelease(handle);
            return true;
        }
    }

    internal sealed class NativeImgCodecsMetadataResultHandle : SafeHandle
    {
        private NativeImgCodecsMetadataResultHandle() : base(IntPtr.Zero, true) { }
        public override bool IsInvalid { get { return handle == IntPtr.Zero; } }

        internal static NativeImgCodecsMetadataResultHandle FromNativePointer(IntPtr value)
        {
            if (value == IntPtr.Zero) throw new OpenCvException("Native metadata result handle is null.");
            var result = new NativeImgCodecsMetadataResultHandle();
            result.SetHandle(value);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.ImgCodecsMetadataResultRelease(handle);
            return true;
        }
    }

    internal sealed class NativeImgCodecsAnimationHandle : SafeHandle
    {
        private NativeImgCodecsAnimationHandle() : base(IntPtr.Zero, true) { }
        public override bool IsInvalid { get { return handle == IntPtr.Zero; } }

        internal static NativeImgCodecsAnimationHandle FromNativePointer(IntPtr value)
        {
            if (value == IntPtr.Zero) throw new OpenCvException("Native animation handle is null.");
            var result = new NativeImgCodecsAnimationHandle();
            result.SetHandle(value);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.ImgCodecsAnimationRelease(handle);
            return true;
        }
    }

    internal sealed class NativeImgCodecsImageCollectionHandle : SafeHandle
    {
        private NativeImgCodecsImageCollectionHandle() : base(IntPtr.Zero, true) { }
        public override bool IsInvalid { get { return handle == IntPtr.Zero; } }

        internal static NativeImgCodecsImageCollectionHandle FromNativePointer(IntPtr value)
        {
            if (value == IntPtr.Zero) throw new OpenCvException("Native image collection handle is null.");
            var result = new NativeImgCodecsImageCollectionHandle();
            result.SetHandle(value);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.ImgCodecsImageCollectionRelease(handle);
            return true;
        }
    }
}
