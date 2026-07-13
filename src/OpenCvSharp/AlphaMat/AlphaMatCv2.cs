using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.AlphaMat
{
    /// <summary>
    /// Static helpers for OpenCV contrib alpha matting.
    /// OpenCV contrib alpha matting 静态辅助方法。
    /// </summary>
    public static class AlphaMatCv2
    {
        /// <summary>
        /// Computes an alpha matte from an RGB/BGR image and a grayscale trimap.
        /// 根据 RGB/BGR 图像和灰度 trimap 计算 alpha matte。
        /// </summary>
        /// <param name="image">The input color image. 输入彩色图像。</param>
        /// <param name="trimap">The grayscale trimap. 灰度 trimap。</param>
        /// <param name="result">The caller-owned output alpha matte. 调用方持有的输出 alpha matte。</param>
        public static void InfoFlow(Mat image, Mat trimap, Mat result)
        {
            ValidateNotNull(image, nameof(image));
            ValidateNotNull(trimap, nameof(trimap));
            ValidateNotNull(result, nameof(result));
            ValidateInfoFlowInputs(image, trimap);
            NativeException.ThrowIfError(NativeMethods.AlphaMatInfoFlow(image.NativeHandle, trimap.NativeHandle, result.NativeHandle));
        }

        /// <summary>
        /// Computes and returns an alpha matte from an RGB/BGR image and a grayscale trimap.
        /// 根据 RGB/BGR 图像和灰度 trimap 计算并返回 alpha matte。
        /// </summary>
        /// <param name="image">The input color image. 输入彩色图像。</param>
        /// <param name="trimap">The grayscale trimap. 灰度 trimap。</param>
        /// <returns>The computed alpha matte. 计算得到的 alpha matte。</returns>
        public static Mat InfoFlow(Mat image, Mat trimap)
        {
            var result = new Mat();
            try
            {
                InfoFlow(image, trimap, result);
                return result;
            }
            catch
            {
                result.Dispose();
                throw;
            }
        }

        internal static void ValidateNotNull<T>(T value, string parameterName)
            where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }

        private static void ValidateInfoFlowInputs(Mat image, Mat trimap)
        {
            if (image.Type != MatType.CV_8UC3)
            {
                throw new ArgumentException("AlphaMat InfoFlow requires a CV_8UC3 image.", nameof(image));
            }

            if (trimap.Type != MatType.CV_8UC1)
            {
                throw new ArgumentException("AlphaMat InfoFlow requires a CV_8UC1 trimap.", nameof(trimap));
            }

            if (image.Rows != trimap.Rows || image.Cols != trimap.Cols)
            {
                throw new ArgumentException("AlphaMat InfoFlow requires image and trimap to have the same size.", nameof(trimap));
            }
        }
    }
}
