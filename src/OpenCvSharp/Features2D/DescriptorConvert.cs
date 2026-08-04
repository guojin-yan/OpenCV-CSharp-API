using System;
using JYPPX.OpenCvSharp.Core;
using CoreCv2 = JYPPX.OpenCvSharp.Core.Cv2;

namespace JYPPX.OpenCvSharp.Features2D
{
    /// <summary>
    /// Provides descriptor conversion helpers for matcher, k-means, and bag-of-words workflows.
    /// 为匹配器、k-means 与视觉词袋流程提供描述子转换辅助方法。
    /// Use these helpers when descriptor rows must be converted or normalized before matcher, <c>BOWKMeansTrainer</c>, or <c>BOWImgDescriptorExtractor</c> use.
    /// 当描述子行需要在匹配器、<c>BOWKMeansTrainer</c> 或 <c>BOWImgDescriptorExtractor</c> 使用前完成转换或归一化时，可使用这些辅助方法。
    /// </summary>
    public static class DescriptorConvert
    {
        /// <summary>
        /// Converts descriptor rows to <c>CV_32F</c> while preserving the channel count.
        /// 将描述子行转换为 <c>CV_32F</c>，并保持通道数不变。
        /// This is useful before k-means or bag-of-words training when the source extractor emits non-float descriptors.
        /// 当来源提取器输出非 float 描述子，而后续需要 k-means 或词袋训练时，这一步很有用。
        /// </summary>
        /// <param name="src">The source descriptor matrix. 源描述子矩阵。</param>
        /// <param name="dst">The destination descriptor matrix. 目标描述子矩阵。</param>
        /// <param name="alpha">The optional scale factor. 可选缩放因子。</param>
        /// <param name="beta">The optional offset added after scaling. 可选缩放后偏移。</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="src"/> or <paramref name="dst"/> is null. 当 <paramref name="src"/> 或 <paramref name="dst"/> 为空时抛出。</exception>
        public static void ConvertDescriptorsToFloat(Mat src, Mat dst, double alpha = 1.0, double beta = 0.0)
        {
            ValidateMatPair(src, dst);
            int targetType = MatType.MakeType(MatType.CV_32F, src.Channels);
            src.ConvertTo(dst, targetType, alpha, beta);
        }

        /// <summary>
        /// Converts descriptor rows to <c>CV_32F</c> and returns a new matrix.
        /// 将描述子行转换为 <c>CV_32F</c> 并返回新矩阵。
        /// This is useful before k-means or bag-of-words training when the source extractor emits non-float descriptors.
        /// 当来源提取器输出非 float 描述子，而后续需要 k-means 或词袋训练时，这一步很有用。
        /// </summary>
        /// <param name="src">The source descriptor matrix. 源描述子矩阵。</param>
        /// <param name="alpha">The optional scale factor. 可选缩放因子。</param>
        /// <param name="beta">The optional offset added after scaling. 可选缩放后偏移。</param>
        /// <returns>The converted descriptor matrix. 转换后的描述子矩阵。</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="src"/> is null. 当 <paramref name="src"/> 为空时抛出。</exception>
        public static Mat ConvertDescriptorsToFloat(Mat src, double alpha = 1.0, double beta = 0.0)
        {
            ValidateMat(src, nameof(src));
            var dst = new Mat();
            try
            {
                ConvertDescriptorsToFloat(src, dst, alpha, beta);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Normalizes descriptor rows into a destination matrix.
        /// 将描述子行归一化到目标矩阵。
        /// </summary>
        /// <param name="src">The source descriptor matrix. 源描述子矩阵。</param>
        /// <param name="dst">The destination descriptor matrix. 目标描述子矩阵。</param>
        /// <param name="normType">The OpenCV norm type. OpenCV 范数类型。</param>
        /// <param name="alpha">The normalization alpha value. 归一化 alpha 值。</param>
        /// <param name="beta">The normalization beta value. 归一化 beta 值。</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="src"/> or <paramref name="dst"/> is null. 当 <paramref name="src"/> 或 <paramref name="dst"/> 为空时抛出。</exception>
        public static void NormalizeDescriptors(Mat src, Mat dst, NormTypes normType = NormTypes.L2, double alpha = 1.0, double beta = 0.0)
        {
            ValidateMatPair(src, dst);
            CoreCv2.Normalize(src, dst, alpha, beta, normType);
        }

        /// <summary>
        /// Normalizes descriptor rows and returns a new matrix.
        /// 将描述子行归一化并返回新矩阵。
        /// </summary>
        /// <param name="src">The source descriptor matrix. 源描述子矩阵。</param>
        /// <param name="normType">The OpenCV norm type. OpenCV 范数类型。</param>
        /// <param name="alpha">The normalization alpha value. 归一化 alpha 值。</param>
        /// <param name="beta">The normalization beta value. 归一化 beta 值。</param>
        /// <returns>The normalized descriptor matrix. 归一化后的描述子矩阵。</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="src"/> is null. 当 <paramref name="src"/> 为空时抛出。</exception>
        public static Mat NormalizeDescriptors(Mat src, NormTypes normType = NormTypes.L2, double alpha = 1.0, double beta = 0.0)
        {
            ValidateMat(src, nameof(src));
            var dst = new Mat();
            try
            {
                NormalizeDescriptors(src, dst, normType, alpha, beta);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Converts descriptor rows to <c>CV_32F</c> and then normalizes them.
        /// 将描述子行转换为 <c>CV_32F</c> 后再归一化。
        /// The result can be supplied to matcher, k-means, or precomputed bag-of-words workflows when their descriptor compatibility requirements are otherwise satisfied.
        /// 当描述子兼容性要求已经满足时，结果可传给匹配器、k-means 或预计算词袋流程。
        /// </summary>
        /// <param name="src">The source descriptor matrix. 源描述子矩阵。</param>
        /// <param name="dst">The destination descriptor matrix. 目标描述子矩阵。</param>
        /// <param name="normType">The OpenCV norm type. OpenCV 范数类型。</param>
        /// <param name="alpha">The normalization alpha value. 归一化 alpha 值。</param>
        /// <param name="beta">The normalization beta value. 归一化 beta 值。</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="src"/> or <paramref name="dst"/> is null. 当 <paramref name="src"/> 或 <paramref name="dst"/> 为空时抛出。</exception>
        public static void ConvertToFloatAndNormalize(Mat src, Mat dst, NormTypes normType = NormTypes.L2, double alpha = 1.0, double beta = 0.0)
        {
            ValidateMatPair(src, dst);
            using (Mat converted = ConvertDescriptorsToFloat(src))
            {
                NormalizeDescriptors(converted, dst, normType, alpha, beta);
            }
        }

        /// <summary>
        /// Converts descriptor rows to <c>CV_32F</c>, normalizes them, and returns a new matrix.
        /// 将描述子行转换为 <c>CV_32F</c>、归一化并返回新矩阵。
        /// The result can be supplied to matcher, k-means, or precomputed bag-of-words workflows when their descriptor compatibility requirements are otherwise satisfied.
        /// 当描述子兼容性要求已经满足时，结果可传给匹配器、k-means 或预计算词袋流程。
        /// </summary>
        /// <param name="src">The source descriptor matrix. 源描述子矩阵。</param>
        /// <param name="normType">The OpenCV norm type. OpenCV 范数类型。</param>
        /// <param name="alpha">The normalization alpha value. 归一化 alpha 值。</param>
        /// <param name="beta">The normalization beta value. 归一化 beta 值。</param>
        /// <returns>The converted and normalized descriptor matrix. 转换并归一化后的描述子矩阵。</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="src"/> is null. 当 <paramref name="src"/> 为空时抛出。</exception>
        public static Mat ConvertToFloatAndNormalize(Mat src, NormTypes normType = NormTypes.L2, double alpha = 1.0, double beta = 0.0)
        {
            ValidateMat(src, nameof(src));
            var dst = new Mat();
            try
            {
                ConvertToFloatAndNormalize(src, dst, normType, alpha, beta);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        private static void ValidateMatPair(Mat src, Mat dst)
        {
            ValidateMat(src, nameof(src));
            ValidateMat(dst, nameof(dst));
        }

        private static void ValidateMat(Mat mat, string parameterName)
        {
            if (mat == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }
    }
}
