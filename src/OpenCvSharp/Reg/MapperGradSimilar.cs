using System;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Reg
{
    /// <summary>
    /// Gradient mapper for similarity registration.
    /// 用于相似变换配准的梯度 mapper。
    /// </summary>
    public sealed class MapperGradSimilar : RegMapper
    {
        /// <summary>Creates the mapper. 创建 mapper。</summary>
        public MapperGradSimilar()
            : base(CreateHandle())
        {
        }

        private static NativeRegMapperHandle CreateHandle()
        {
            NativeException.ThrowIfError(NativeMethods.RegMapperGradSimilarCreate(out IntPtr nativeHandle));
            return NativeRegMapperHandle.FromNativePointer(nativeHandle);
        }
    }
}
