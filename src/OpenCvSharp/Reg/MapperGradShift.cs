using System;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Reg
{
    /// <summary>
    /// Gradient mapper for translation registration.
    /// 用于平移配准的梯度 mapper。
    /// </summary>
    public sealed class MapperGradShift : RegMapper
    {
        /// <summary>Creates the mapper. 创建 mapper。</summary>
        public MapperGradShift()
            : base(CreateHandle())
        {
        }

        private static NativeRegMapperHandle CreateHandle()
        {
            NativeException.ThrowIfError(NativeMethods.RegMapperGradShiftCreate(out IntPtr nativeHandle));
            return NativeRegMapperHandle.FromNativePointer(nativeHandle);
        }
    }
}
