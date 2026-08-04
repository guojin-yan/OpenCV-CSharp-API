using System;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct NativeDnnRect
    {
        internal int X;
        internal int Y;
        internal int Width;
        internal int Height;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct NativeDnnImage2BlobParams
    {
        internal double ScaleV0;
        internal double ScaleV1;
        internal double ScaleV2;
        internal double ScaleV3;
        internal int SizeWidth;
        internal int SizeHeight;
        internal double MeanV0;
        internal double MeanV1;
        internal double MeanV2;
        internal double MeanV3;
        internal int SwapRb;
        internal int DDepth;
        internal int DataLayout;
        internal int PaddingMode;
        internal double BorderV0;
        internal double BorderV1;
        internal double BorderV2;
        internal double BorderV3;
    }

    internal sealed class NativeDnnLayerHandle : SafeHandle
    {
        private NativeDnnLayerHandle() : base(IntPtr.Zero, true) { }
        public override bool IsInvalid { get { return handle == IntPtr.Zero; } }

        internal static NativeDnnLayerHandle FromNativePointer(IntPtr value)
        {
            if (value == IntPtr.Zero) throw new OpenCvException("Native DNN Layer handle is null.");
            var result = new NativeDnnLayerHandle();
            result.SetHandle(value);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.DnnLayerReleaseHandle(handle);
            return true;
        }
    }

    internal sealed class NativeDnnMatGroupsHandle : SafeHandle
    {
        private NativeDnnMatGroupsHandle() : base(IntPtr.Zero, true) { }
        public override bool IsInvalid { get { return handle == IntPtr.Zero; } }

        internal static NativeDnnMatGroupsHandle FromNativePointer(IntPtr value)
        {
            if (value == IntPtr.Zero) throw new OpenCvException("Native DNN grouped Mat result handle is null.");
            var result = new NativeDnnMatGroupsHandle();
            result.SetHandle(value);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.DnnMatGroupsReleaseHandle(handle);
            return true;
        }
    }
}
