using System;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Reg
{
    /// <summary>
    /// Gradient mapper for affine registration.
    /// 用于仿射配准的梯度 mapper。
    /// </summary>
    public sealed class MapperGradAffine : RegMapper
    {
        /// <summary>Creates the mapper. 创建 mapper。</summary>
        public MapperGradAffine()
            : base(CreateHandle())
        {
        }

        private static NativeRegMapperHandle CreateHandle()
        {
            NativeException.ThrowIfError(NativeMethods.RegMapperGradAffineCreate(out IntPtr nativeHandle));
            return NativeRegMapperHandle.FromNativePointer(nativeHandle);
        }
    }
}
