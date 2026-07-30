using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal sealed class NativeFileStorageHandle : SafeHandle
    {
        private NativeFileStorageHandle() : base(IntPtr.Zero, true) { }
        public override bool IsInvalid { get { return handle == IntPtr.Zero; } }

        internal static NativeFileStorageHandle FromNativePointer(IntPtr value)
        {
            if (value == IntPtr.Zero) throw new OpenCvException("Native FileStorage handle is null.");
            var result = new NativeFileStorageHandle();
            result.SetHandle(value);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.CoreFileStorageReleaseHandle(handle);
            return true;
        }
    }

    internal sealed class NativeFileNodeHandle : SafeHandle
    {
        private NativeFileNodeHandle() : base(IntPtr.Zero, true) { }
        public override bool IsInvalid { get { return handle == IntPtr.Zero; } }

        internal static NativeFileNodeHandle FromNativePointer(IntPtr value)
        {
            if (value == IntPtr.Zero) throw new OpenCvException("Native FileNode handle is null.");
            var result = new NativeFileNodeHandle();
            result.SetHandle(value);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.CoreFileNodeRelease(handle);
            return true;
        }
    }

    internal sealed class NativeCoreUtf8ResultHandle : SafeHandle
    {
        private NativeCoreUtf8ResultHandle() : base(IntPtr.Zero, true) { }
        public override bool IsInvalid { get { return handle == IntPtr.Zero; } }

        internal static NativeCoreUtf8ResultHandle FromNativePointer(IntPtr value)
        {
            if (value == IntPtr.Zero) throw new OpenCvException("Native UTF-8 result handle is null.");
            var result = new NativeCoreUtf8ResultHandle();
            result.SetHandle(value);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.CoreUtf8ResultRelease(handle);
            return true;
        }
    }

    internal sealed class NativeCoreStringListHandle : SafeHandle
    {
        private NativeCoreStringListHandle() : base(IntPtr.Zero, true) { }
        public override bool IsInvalid { get { return handle == IntPtr.Zero; } }

        internal static NativeCoreStringListHandle FromNativePointer(IntPtr value)
        {
            if (value == IntPtr.Zero) throw new OpenCvException("Native string-list handle is null.");
            var result = new NativeCoreStringListHandle();
            result.SetHandle(value);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.CoreStringListRelease(handle);
            return true;
        }
    }

    internal sealed class NativeTickMeterHandle : SafeHandle
    {
        private NativeTickMeterHandle() : base(IntPtr.Zero, true) { }
        public override bool IsInvalid { get { return handle == IntPtr.Zero; } }

        internal static NativeTickMeterHandle FromNativePointer(IntPtr value)
        {
            if (value == IntPtr.Zero) throw new OpenCvException("Native TickMeter handle is null.");
            var result = new NativeTickMeterHandle();
            result.SetHandle(value);
            return result;
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.CoreTickMeterRelease(handle);
            return true;
        }
    }
}
