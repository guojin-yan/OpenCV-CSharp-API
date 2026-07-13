using System;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Reg
{
    /// <summary>
    /// Gradient mapper for Euclidean registration.
    /// 用于欧氏运动配准的梯度 mapper。
    /// </summary>
    public sealed class MapperGradEuclid : RegMapper
    {
        /// <summary>Creates the mapper. 创建 mapper。</summary>
        public MapperGradEuclid()
            : base(CreateHandle())
        {
        }

        private static NativeRegMapperHandle CreateHandle()
        {
            NativeException.ThrowIfError(NativeMethods.RegMapperGradEuclidCreate(out IntPtr nativeHandle));
            return NativeRegMapperHandle.FromNativePointer(nativeHandle);
        }
    }
}
