using System;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.ImgHash
{
    /// <summary>Block mean image hash. Block mean 图像哈希。</summary>
    public sealed class BlockMeanHash : ImgHashBase
    {
        private BlockMeanHash(IntPtr nativeHandle)
            : base(nativeHandle)
        {
        }

        /// <summary>Creates a BlockMeanHash object. 创建 BlockMeanHash 对象。</summary>
        public static BlockMeanHash Create(BlockMeanHashMode mode = BlockMeanHashMode.Mode0)
        {
            ValidateMode(mode, nameof(mode));
            NativeException.ThrowIfError(NativeMethods.ImgHashBlockMeanCreate((int)mode, out IntPtr nativeHandle));
            return new BlockMeanHash(nativeHandle);
        }

        /// <summary>Sets the mode. 设置模式。</summary>
        public void SetMode(BlockMeanHashMode mode)
        {
            ValidateMode(mode, nameof(mode));
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.ImgHashBlockMeanSetMode(NativeHandle, (int)mode));
        }

        /// <summary>Gets internal mean values. 获取内部均值。</summary>
        public double[] GetMean()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.ImgHashBlockMeanGetMeanCount(NativeHandle, out int count));
            if (count <= 0)
            {
                return Array.Empty<double>();
            }

            var values = new double[count];
            NativeException.ThrowIfError(NativeMethods.ImgHashBlockMeanGetMeanFill(NativeHandle, values, values.Length, out int written));
            if (written == values.Length)
            {
                return values;
            }

            var trimmed = new double[Math.Max(0, Math.Min(written, values.Length))];
            Array.Copy(values, trimmed, trimmed.Length);
            return trimmed;
        }

        internal static void ValidateMode(BlockMeanHashMode mode, string parameterName)
        {
            if (mode != BlockMeanHashMode.Mode0 && mode != BlockMeanHashMode.Mode1)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Block mean hash mode must be Mode0 or Mode1.");
            }
        }
    }
}
