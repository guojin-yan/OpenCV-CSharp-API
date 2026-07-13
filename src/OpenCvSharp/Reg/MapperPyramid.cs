using System;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Reg
{
    /// <summary>
    /// Hierarchical registration mapper using a Gaussian pyramid.
    /// 使用高斯金字塔的分层 registration mapper。
    /// </summary>
    public sealed class MapperPyramid : RegMapper
    {
        /// <summary>Creates a pyramid mapper around a base mapper. 使用基础 mapper 创建金字塔 mapper。</summary>
        public MapperPyramid(RegMapper baseMapper)
            : base(CreateHandle(baseMapper))
        {
        }

        /// <summary>Gets or sets the pyramid level count. 获取或设置金字塔层数。</summary>
        public int NumLevels
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.RegMapperPyramidGetNumLevels(NativeHandle, out int value));
                return value;
            }
            set
            {
                ThrowIfDisposed();
                ValidatePositive(value, nameof(value));
                NativeException.ThrowIfError(NativeMethods.RegMapperPyramidSetNumLevels(NativeHandle, value));
            }
        }

        /// <summary>Gets or sets the iteration count at each pyramid scale. 获取或设置每个尺度的迭代次数。</summary>
        public int NumIterationsPerScale
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.RegMapperPyramidGetNumIterationsPerScale(NativeHandle, out int value));
                return value;
            }
            set
            {
                ThrowIfDisposed();
                ValidatePositive(value, nameof(value));
                NativeException.ThrowIfError(NativeMethods.RegMapperPyramidSetNumIterationsPerScale(NativeHandle, value));
            }
        }

        private static NativeRegMapperHandle CreateHandle(RegMapper baseMapper)
        {
            ValidateNotNull(baseMapper, nameof(baseMapper));
            NativeException.ThrowIfError(NativeMethods.RegMapperPyramidCreate(baseMapper.NativeHandle, out IntPtr nativeHandle));
            return NativeRegMapperHandle.FromNativePointer(nativeHandle);
        }
    }
}
