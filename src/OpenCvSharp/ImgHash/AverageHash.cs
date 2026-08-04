using System;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.ImgHash
{
    /// <summary>Average image hash. Average 图像哈希。</summary>
    public sealed class AverageHash : ImgHashBase
    {
        private AverageHash(IntPtr nativeHandle)
            : base(nativeHandle)
        {
        }

        /// <summary>Creates an AverageHash object. 创建 AverageHash 对象。</summary>
        public static AverageHash Create()
        {
            NativeException.ThrowIfError(NativeMethods.ImgHashAverageCreate(out IntPtr nativeHandle));
            return new AverageHash(nativeHandle);
        }
    }
}
