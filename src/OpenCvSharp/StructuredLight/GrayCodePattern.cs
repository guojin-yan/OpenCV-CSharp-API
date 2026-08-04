using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.StructuredLight
{
    /// <summary>
    /// Gray-code structured-light pattern generator.
    /// Gray-code 结构光图案生成器。
    /// </summary>
    public sealed class GrayCodePattern : StructuredLightPattern
    {
        private GrayCodePattern(NativeStructuredLightPatternHandle handle)
            : base(handle)
        {
        }

        /// <summary>
        /// Gets the number of images needed by this Gray-code pattern.
        /// 获取此 Gray-code 图案所需的图像数量。
        /// </summary>
        public int NumberOfPatternImages
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.StructuredLightGrayCodePatternGetNumberOfPatternImages(
                    NativeHandle,
                    out int value));
                return value;
            }
        }

        /// <summary>
        /// Creates a Gray-code pattern with OpenCV default parameters.
        /// 使用 OpenCV 默认参数创建 Gray-code 图案。
        /// </summary>
        public static GrayCodePattern Create()
        {
            return Create(GrayCodePatternParams.Default);
        }

        /// <summary>
        /// Creates a Gray-code pattern.
        /// 创建 Gray-code 图案。
        /// </summary>
        public static GrayCodePattern Create(GrayCodePatternParams parameters)
        {
            parameters.Validate();
            NativeException.ThrowIfError(NativeMethods.StructuredLightGrayCodePatternCreate(
                parameters.Width,
                parameters.Height,
                out IntPtr nativeHandle));
            return new GrayCodePattern(NativeStructuredLightPatternHandle.FromNativePointer(nativeHandle));
        }

        /// <summary>
        /// Creates a Gray-code pattern.
        /// 创建 Gray-code 图案。
        /// </summary>
        public static GrayCodePattern Create(int width, int height)
        {
            return Create(new GrayCodePatternParams(width, height));
        }

        /// <summary>Sets the white threshold used during decoding. 设置解码时使用的白阈值。</summary>
        public void SetWhiteThreshold(int value)
        {
            ValidateNonNegativeThreshold(value, nameof(value));
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.StructuredLightGrayCodePatternSetWhiteThreshold(NativeHandle, value));
        }

        /// <summary>Sets the black threshold used during shadow-mask computation. 设置阴影掩码计算时使用的黑阈值。</summary>
        public void SetBlackThreshold(int value)
        {
            ValidateNonNegativeThreshold(value, nameof(value));
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.StructuredLightGrayCodePatternSetBlackThreshold(NativeHandle, value));
        }

        /// <summary>
        /// Generates the all-black and all-white images used for shadow masks.
        /// 生成阴影掩码所需的全黑和全白图像。
        /// </summary>
        public void GetImagesForShadowMasks(Mat blackImage, Mat whiteImage)
        {
            ThrowIfDisposed();
            ValidateNotNull(blackImage, nameof(blackImage));
            ValidateNotNull(whiteImage, nameof(whiteImage));
            NativeException.ThrowIfError(NativeMethods.StructuredLightGrayCodePatternGetImagesForShadowMasks(
                NativeHandle,
                blackImage.NativeHandle,
                whiteImage.NativeHandle));
        }

        /// <summary>
        /// Generates the all-black and all-white images used for shadow masks.
        /// 生成阴影掩码所需的全黑和全白图像。
        /// </summary>
        public void GetImagesForShadowMasks(out Mat blackImage, out Mat whiteImage)
        {
            blackImage = new Mat();
            whiteImage = new Mat();
            try
            {
                GetImagesForShadowMasks(blackImage, whiteImage);
            }
            catch
            {
                blackImage.Dispose();
                whiteImage.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Decodes a camera pixel into its projector pixel.
        /// 将相机像素解码为对应的投影仪像素。
        /// </summary>
        public unsafe bool GetProjPixel(Mat[] patternImages, int x, int y, out Point projectorPixel)
        {
            ThrowIfDisposed();
            IntPtr[] handles = ToNativeHandles(patternImages, nameof(patternImages));
            ValidatePatternImageCount(patternImages.Length, nameof(patternImages));
            fixed (IntPtr* handlesPtr = handles)
            {
                NativeException.ThrowIfError(NativeMethods.StructuredLightGrayCodePatternGetProjPixel(
                    NativeHandle,
                    handlesPtr,
                    handles.Length,
                    x,
                    y,
                    out int found,
                    out int projX,
                    out int projY));
                projectorPixel = new Point(projX, projY);
                return found != 0;
            }
        }

        private void ValidatePatternImageCount(int imageCount, string parameterName)
        {
            int expectedCount = NumberOfPatternImages;
            if (imageCount != expectedCount)
            {
                throw new ArgumentException("Pattern image count must match NumberOfPatternImages.", parameterName);
            }
        }
    }
}
