using System;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Saliency
{
    /// <summary>
    /// Static spectral-residual saliency algorithm.
    /// 静态谱残差显著性算法。
    /// </summary>
    public sealed class StaticSaliencySpectralResidual : StaticSaliency
    {
        private StaticSaliencySpectralResidual(IntPtr nativeHandle)
            : base(nativeHandle)
        {
        }

        /// <summary>Gets or sets internal image width. 获取或设置内部图像宽度。</summary>
        public int ImageWidth
        {
            get
            {
                NativeException.ThrowIfError(NativeMethods.SaliencySpectralResidualGetImageWidth(NativeHandle, out int value));
                return value;
            }
            set
            {
                NativeException.ThrowIfError(NativeMethods.SaliencySpectralResidualSetImageWidth(NativeHandle, value));
            }
        }

        /// <summary>Gets or sets internal image height. 获取或设置内部图像高度。</summary>
        public int ImageHeight
        {
            get
            {
                NativeException.ThrowIfError(NativeMethods.SaliencySpectralResidualGetImageHeight(NativeHandle, out int value));
                return value;
            }
            set
            {
                NativeException.ThrowIfError(NativeMethods.SaliencySpectralResidualSetImageHeight(NativeHandle, value));
            }
        }

        /// <summary>Creates a spectral-residual saliency algorithm. 创建谱残差显著性算法。</summary>
        public static StaticSaliencySpectralResidual Create()
        {
            NativeException.ThrowIfError(NativeMethods.SaliencySpectralResidualCreate(out IntPtr nativeHandle));
            return new StaticSaliencySpectralResidual(nativeHandle);
        }
    }
}
