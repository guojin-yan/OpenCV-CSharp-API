using System;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.ImgHash
{
    /// <summary>Perceptual image hash. 感知图像哈希。</summary>
    public sealed class PHash : ImgHashBase
    {
        private PHash(IntPtr nativeHandle)
            : base(nativeHandle)
        {
        }

        /// <summary>Creates a PHash object. 创建 PHash 对象。</summary>
        public static PHash Create()
        {
            NativeException.ThrowIfError(NativeMethods.ImgHashPHashCreate(out IntPtr nativeHandle));
            return new PHash(nativeHandle);
        }
    }
}
