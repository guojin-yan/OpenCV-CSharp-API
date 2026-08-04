using System;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    internal sealed class NativeDenseOpticalFlowHandle : SafeHandle
    {
        private NativeDenseOpticalFlowHandle()
            : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid => handle == IntPtr.Zero;

        internal static NativeDenseOpticalFlowHandle FromNativePointer(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                throw new OpenCvException("Native DenseOpticalFlow handle is null.");
            }
            var result = new NativeDenseOpticalFlowHandle();
            result.SetHandle(nativeHandle);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.DenseOpticalFlowReleaseHandle(handle);
            return true;
        }
    }

    internal sealed class NativeSparseOpticalFlowHandle : SafeHandle
    {
        private NativeSparseOpticalFlowHandle()
            : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid => handle == IntPtr.Zero;

        internal static NativeSparseOpticalFlowHandle FromNativePointer(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                throw new OpenCvException("Native SparseOpticalFlow handle is null.");
            }
            var result = new NativeSparseOpticalFlowHandle();
            result.SetHandle(nativeHandle);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.SparseOpticalFlowReleaseHandle(handle);
            return true;
        }
    }
}
