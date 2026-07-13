using System;
using OpenCvSharp.Core;

namespace OpenCvSharp.Features2D
{
    /// <summary>
    /// Provides a base type for OpenCV feature detectors and descriptor extractors.
    /// 提供 OpenCV 特征检测器与描述子提取器的基类型。
    /// </summary>
    public abstract class Feature2D : IDisposable
    {
        /// <summary>
        /// Gets a value indicating whether this object has been disposed.
        /// 获取此对象是否已经释放。
        /// </summary>
        public abstract bool IsDisposed { get; }

        /// <summary>
        /// Gets a value indicating whether the native algorithm is empty.
        /// 获取 native 算法对象是否为空。
        /// </summary>
        public abstract bool Empty { get; }

        /// <summary>
        /// Gets the descriptor size in bytes.
        /// 获取描述子字节尺寸。
        /// </summary>
        public abstract int DescriptorSize { get; }

        /// <summary>
        /// Gets the descriptor OpenCV matrix type.
        /// 获取描述子的 OpenCV 矩阵类型。
        /// </summary>
        public abstract int DescriptorType { get; }

        /// <summary>
        /// Gets the default norm type used for descriptor matching.
        /// 获取描述子匹配默认使用的范数类型。
        /// </summary>
        public abstract NormTypes DefaultNorm { get; }

        /// <summary>
        /// Gets OpenCV's default algorithm name for serialization and diagnostics.
        /// 获取 OpenCV 用于序列化和诊断的默认算法名称。
        /// </summary>
        public abstract string DefaultName { get; }

        /// <summary>
        /// Clears the native algorithm state.
        /// 清除 native 算法状态。
        /// </summary>
        public abstract void Clear();

        /// <summary>
        /// Detects keypoints in an image.
        /// 检测图像中的关键点。
        /// </summary>
        /// <param name="image">The input image. 输入图像。</param>
        /// <param name="mask">The optional mask. 可选掩码。</param>
        /// <returns>The detected keypoints. 检测到的关键点。</returns>
        public abstract KeyPoint[] Detect(Mat image, Mat? mask = null);

        /// <summary>
        /// Detects keypoints in a batch of images.
        /// 批量检测多张图像中的关键点。
        /// </summary>
        /// <param name="images">The input images. 输入图像集合。</param>
        /// <returns>The detected keypoints for each image. 每张图像对应的关键点集合。</returns>
        public KeyPoint[][] Detect(Mat[] images)
        {
            return Detect(images, null);
        }

        /// <summary>
        /// Detects keypoints in a batch of images with optional masks.
        /// 使用可选掩码批量检测多张图像中的关键点。
        /// </summary>
        /// <param name="images">The input images. 输入图像集合。</param>
        /// <param name="masks">The optional masks; when provided, the length must match <paramref name="images"/>. 可选掩码；提供时长度必须与 <paramref name="images"/> 一致。</param>
        /// <returns>The detected keypoints for each image. 每张图像对应的关键点集合。</returns>
        public KeyPoint[][] Detect(Mat[] images, Mat[]? masks)
        {
            ValidateImageArray(images);
            ValidateMaskArray(images.Length, masks);

            var result = new KeyPoint[images.Length][];
            for (int i = 0; i < images.Length; i++)
            {
                result[i] = Detect(images[i], masks == null || masks.Length == 0 ? null : masks[i]);
            }

            return result;
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Detects keypoints in a span-backed batch of images.
        /// 使用 Span 支持的图像集合批量检测关键点。
        /// </summary>
        /// <param name="images">The input images. 输入图像集合。</param>
        /// <returns>The detected keypoints for each image. 每张图像对应的关键点集合。</returns>
        public KeyPoint[][] Detect(ReadOnlySpan<Mat> images)
        {
            return Detect(images, ReadOnlySpan<Mat?>.Empty);
        }

        /// <summary>
        /// Detects keypoints in a span-backed batch of images with optional masks.
        /// 使用 Span 支持的图像集合和可选掩码批量检测关键点。
        /// </summary>
        /// <param name="images">The input images. 输入图像集合。</param>
        /// <param name="masks">The optional masks; an empty span means no masks. 可选掩码；空 Span 表示不使用掩码。</param>
        /// <returns>The detected keypoints for each image. 每张图像对应的关键点集合。</returns>
        public KeyPoint[][] Detect(ReadOnlySpan<Mat> images, ReadOnlySpan<Mat?> masks)
        {
            ValidateImageSpan(images);
            if (!masks.IsEmpty && masks.Length != images.Length)
            {
                throw new ArgumentException("Mask count must be zero or match the image count.", nameof(masks));
            }

            var result = new KeyPoint[images.Length][];
            for (int i = 0; i < images.Length; i++)
            {
                result[i] = Detect(images[i], masks.IsEmpty ? null : masks[i]);
            }

            return result;
        }
#endif

        /// <summary>
        /// Releases the native object.
        /// 释放 native 对象。
        /// </summary>
        public abstract void Dispose();

        private static void ValidateImageArray(Mat[] images)
        {
            if (images == null)
            {
                throw new ArgumentNullException(nameof(images));
            }

            if (images.Length == 0)
            {
                throw new ArgumentException("Image collection must contain at least one image.", nameof(images));
            }

            for (int i = 0; i < images.Length; i++)
            {
                if (images[i] == null)
                {
                    throw new ArgumentNullException(nameof(images));
                }
            }
        }

        private static void ValidateMaskArray(int imageCount, Mat[]? masks)
        {
            if (masks == null)
            {
                return;
            }

            if (masks.Length != 0 && masks.Length != imageCount)
            {
                throw new ArgumentException("Mask count must be zero or match the image count.", nameof(masks));
            }
        }

#if NETCOREAPP3_1_OR_GREATER
        private static void ValidateImageSpan(ReadOnlySpan<Mat> images)
        {
            if (images.IsEmpty)
            {
                throw new ArgumentException("Image collection must contain at least one image.", nameof(images));
            }

            for (int i = 0; i < images.Length; i++)
            {
                if (images[i] == null)
                {
                    throw new ArgumentNullException(nameof(images));
                }
            }
        }
#endif
    }
}
