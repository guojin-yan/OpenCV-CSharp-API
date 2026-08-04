using System;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Reg
{
    /// <summary>
    /// Gradient mapper for projective registration.
    /// 用于投影配准的梯度 mapper。
    /// </summary>
    public sealed class MapperGradProj : RegMapper
    {
        /// <summary>Creates the mapper. 创建 mapper。</summary>
        public MapperGradProj()
            : base(CreateHandle())
        {
        }

        private static NativeRegMapperHandle CreateHandle()
        {
            NativeException.ThrowIfError(NativeMethods.RegMapperGradProjCreate(out IntPtr nativeHandle));
            return NativeRegMapperHandle.FromNativePointer(nativeHandle);
        }
    }
}
