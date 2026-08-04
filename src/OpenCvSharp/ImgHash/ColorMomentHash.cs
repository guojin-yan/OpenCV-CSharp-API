using System;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.ImgHash
{
    /// <summary>Color moment image hash. Color moment 图像哈希。</summary>
    public sealed class ColorMomentHash : ImgHashBase
    {
        private ColorMomentHash(IntPtr nativeHandle)
            : base(nativeHandle)
        {
        }

        /// <summary>Creates a ColorMomentHash object. 创建 ColorMomentHash 对象。</summary>
        public static ColorMomentHash Create()
        {
            NativeException.ThrowIfError(NativeMethods.ImgHashColorMomentCreate(out IntPtr nativeHandle));
            return new ColorMomentHash(nativeHandle);
        }
    }
}
