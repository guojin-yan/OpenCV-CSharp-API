using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Saliency
{
    /// <summary>
    /// Motion saliency algorithm by Bin Wang and Dudek 2014.
    /// Bin Wang 与 Dudek 2014 运动显著性算法。
    /// </summary>
    public sealed class MotionSaliencyBinWangApr2014 : Saliency
    {
        private MotionSaliencyBinWangApr2014(IntPtr nativeHandle)
            : base(nativeHandle)
        {
        }

        /// <summary>Gets or sets image width. 获取或设置图像宽度。</summary>
        public int ImageWidth
        {
            get
            {
                NativeException.ThrowIfError(NativeMethods.SaliencyMotionBinWangGetImageWidth(NativeHandle, out int value));
                return value;
            }
            set
            {
                NativeException.ThrowIfError(NativeMethods.SaliencyMotionBinWangSetImageWidth(NativeHandle, value));
            }
        }

        /// <summary>Gets or sets image height. 获取或设置图像高度。</summary>
        public int ImageHeight
        {
            get
            {
                NativeException.ThrowIfError(NativeMethods.SaliencyMotionBinWangGetImageHeight(NativeHandle, out int value));
                return value;
            }
            set
            {
                NativeException.ThrowIfError(NativeMethods.SaliencyMotionBinWangSetImageHeight(NativeHandle, value));
            }
        }

        /// <summary>Creates a motion saliency algorithm. 创建运动显著性算法。</summary>
        public static MotionSaliencyBinWangApr2014 Create()
        {
            NativeException.ThrowIfError(NativeMethods.SaliencyMotionBinWangCreate(out IntPtr nativeHandle));
            return new MotionSaliencyBinWangApr2014(nativeHandle);
        }

        /// <summary>Sets image size. 设置图像尺寸。</summary>
        public void SetImageSize(int width, int height)
        {
            NativeException.ThrowIfError(NativeMethods.SaliencyMotionBinWangSetImageSize(NativeHandle, width, height));
        }

        /// <summary>Initializes internal state. 初始化内部状态。</summary>
        public bool Init()
        {
            NativeException.ThrowIfError(NativeMethods.SaliencyMotionBinWangInit(NativeHandle, out int result));
            return result != 0;
        }

        internal override void ValidateComputeSaliencyImage(Mat image, string parameterName)
        {
            if (image.Channels != 1)
            {
                throw new ArgumentException("Motion saliency requires a single-channel image.", parameterName);
            }
        }
    }
}
