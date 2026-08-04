using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using JYPPX.OpenCvSharp.Core;

namespace JYPPX.OpenCvSharp.Features2D
{
    /// <summary>
    /// Computes bag-of-visual-words image descriptors from local feature descriptors.
    /// 根据局部特征描述子计算视觉词袋图像描述子。
    /// Vocabulary rows are matcher training descriptors and must stay compatible with the descriptors produced by the extractor or supplied to precomputed compute overloads.
    /// 词典行是匹配器训练描述子，必须与提取器生成或预计算重载传入的描述子保持兼容。
    /// </summary>
    public sealed class BOWImgDescriptorExtractor : IDisposable
    {
        private readonly Feature2D? descriptorExtractor;
        private readonly DescriptorMatcher descriptorMatcher;
        private Mat? vocabulary;
        private bool disposed;

        /// <summary>
        /// Initializes a bag-of-visual-words descriptor extractor.
        /// 初始化视觉词袋描述子提取器。
        /// </summary>
        /// <param name="descriptorExtractor">The feature descriptor extractor used to compute keypoint descriptors. 用于计算关键点描述子的特征描述子提取器。</param>
        /// <param name="descriptorMatcher">The matcher used to assign descriptors to compatible vocabulary rows. 用于将描述子分配到兼容词典行的匹配器。</param>
        public BOWImgDescriptorExtractor(Feature2D descriptorExtractor, DescriptorMatcher descriptorMatcher)
        {
            if (descriptorExtractor == null)
            {
                throw new ArgumentNullException(nameof(descriptorExtractor));
            }

            if (descriptorMatcher == null)
            {
                throw new ArgumentNullException(nameof(descriptorMatcher));
            }

            this.descriptorExtractor = descriptorExtractor;
            this.descriptorMatcher = descriptorMatcher;
        }

        /// <summary>
        /// Initializes a bag-of-visual-words descriptor extractor that accepts precomputed keypoint descriptors.
        /// 初始化接收预计算关键点描述子的视觉词袋描述子提取器。
        /// </summary>
        /// <param name="descriptorMatcher">The matcher used to assign descriptors to compatible vocabulary rows. 用于将描述子分配到兼容词典行的匹配器。</param>
        public BOWImgDescriptorExtractor(DescriptorMatcher descriptorMatcher)
        {
            if (descriptorMatcher == null)
            {
                throw new ArgumentNullException(nameof(descriptorMatcher));
            }

            this.descriptorMatcher = descriptorMatcher;
        }

        /// <summary>
        /// Gets the feature descriptor extractor supplied to the constructor, if any.
        /// 获取构造时传入的特征描述子提取器；没有传入时为空。
        /// </summary>
        public Feature2D? DescriptorExtractor
        {
            get
            {
                ThrowIfDisposed();
                return descriptorExtractor;
            }
        }

        /// <summary>
        /// Gets the descriptor matcher used for vocabulary assignment.
        /// 获取用于词典分配的描述子匹配器。
        /// </summary>
        public DescriptorMatcher DescriptorMatcher
        {
            get
            {
                ThrowIfDisposed();
                return descriptorMatcher;
            }
        }

        /// <summary>
        /// Gets the bag-of-words descriptor size, equal to the number of vocabulary rows.
        /// 获取词袋描述子尺寸，等于词典行数。
        /// </summary>
        public int DescriptorSize
        {
            get
            {
                ThrowIfDisposed();
                return vocabulary == null || vocabulary.Empty ? 0 : vocabulary.Rows;
            }
        }

        /// <summary>
        /// Gets the bag-of-words descriptor matrix type, always <c>CV_32FC1</c>.
        /// 获取词袋描述子的矩阵类型，始终为 <c>CV_32FC1</c>。
        /// </summary>
        public int DescriptorType
        {
            get
            {
                ThrowIfDisposed();
                return MatType.CV_32FC1;
            }
        }

        /// <summary>
        /// Gets a cloned vocabulary snapshot owned by the caller.
        /// 获取由调用方拥有的词典快照克隆。
        /// </summary>
        public Mat Vocabulary
        {
            get { return GetVocabulary(); }
        }

        /// <summary>
        /// Gets a value indicating whether this extractor has been disposed.
        /// 获取提取器是否已经释放。
        /// </summary>
        public bool IsDisposed
        {
            get { return disposed; }
        }

        /// <summary>
        /// Sets the visual vocabulary and trains the matcher against it.
        /// 设置视觉词典，并使用该词典训练匹配器。
        /// </summary>
        /// <param name="vocabulary">The vocabulary matrix, one visual word per row, compatible with later keypoint descriptor rows and the configured matcher norm. 词典矩阵，每行一个视觉词，并与后续关键点描述子行及已配置的匹配器范数兼容。</param>
        public void SetVocabulary(Mat vocabulary)
        {
            ValidateVocabulary(vocabulary, nameof(vocabulary));
            ThrowIfDisposed();

            Mat newVocabulary = vocabulary.Clone();
            try
            {
                descriptorMatcher.Clear();
                descriptorMatcher.Add(new[] { newVocabulary });
                descriptorMatcher.Train();
            }
            catch
            {
                descriptorMatcher.Clear();
                newVocabulary.Dispose();
                throw;
            }

            if (this.vocabulary != null)
            {
                this.vocabulary.Dispose();
            }

            this.vocabulary = newVocabulary;
        }

        /// <summary>
        /// Gets a cloned vocabulary matrix.
        /// 获取词典矩阵的克隆。
        /// </summary>
        /// <returns>The vocabulary clone, or an empty matrix when no vocabulary is set. 词典克隆；未设置词典时返回空矩阵。</returns>
        public Mat GetVocabulary()
        {
            ThrowIfDisposed();
            return vocabulary == null ? new Mat() : vocabulary.Clone();
        }

        /// <summary>
        /// Clears the vocabulary and matcher training data.
        /// 清除词典和匹配器训练数据。
        /// </summary>
        public void Clear()
        {
            ThrowIfDisposed();
            descriptorMatcher.Clear();
            if (vocabulary != null)
            {
                vocabulary.Dispose();
                vocabulary = null;
            }
        }

        /// <summary>
        /// Computes a bag-of-words descriptor from an image and keypoints.
        /// 根据图像和关键点计算词袋描述子。
        /// </summary>
        /// <param name="image">The input image. 输入图像。</param>
        /// <param name="keypoints">The input keypoints. 输入关键点。</param>
        /// <param name="imgDescriptor">The output 1 x vocabulary-size histogram descriptor. 输出的 1 x 词典尺寸直方图描述子。</param>
        /// <param name="descriptors">Optional output of raw keypoint descriptors. 可选输出原始关键点描述子。</param>
        /// <returns>The keypoints returned by the descriptor extractor. 描述子提取器返回的关键点。</returns>
        public KeyPoint[] Compute(Mat image, KeyPoint[] keypoints, Mat imgDescriptor, Mat? descriptors = null)
        {
            if (keypoints == null)
            {
                throw new ArgumentNullException(nameof(keypoints));
            }

            return ComputeCore(image, keypoints, imgDescriptor, descriptors);
        }

        /// <summary>
        /// Computes a bag-of-words descriptor from an image and returns descriptor indexes grouped by vocabulary row.
        /// 根据图像计算词袋描述子，并返回按词典行分组的描述子索引。
        /// </summary>
        /// <param name="image">The input image. 输入图像。</param>
        /// <param name="keypoints">The input keypoints. 输入关键点。</param>
        /// <param name="imgDescriptor">The output 1 x vocabulary-size histogram descriptor. 输出的 1 x 词典尺寸直方图描述子。</param>
        /// <param name="pointIdxsOfClusters">Descriptor indexes grouped by assigned vocabulary row. 按分配到的词典行分组的描述子索引。</param>
        /// <param name="descriptors">Optional output of raw keypoint descriptors. 可选输出原始关键点描述子。</param>
        /// <returns>The keypoints returned by the descriptor extractor. 描述子提取器返回的关键点。</returns>
        public KeyPoint[] Compute(Mat image, KeyPoint[] keypoints, Mat imgDescriptor, out int[][] pointIdxsOfClusters, Mat? descriptors = null)
        {
            if (keypoints == null)
            {
                throw new ArgumentNullException(nameof(keypoints));
            }

            return ComputeCore(image, keypoints, imgDescriptor, out pointIdxsOfClusters, descriptors);
        }

        /// <summary>
        /// Computes a bag-of-words descriptor from an image and updates the keypoint array.
        /// 根据图像计算词袋描述子，并更新关键点数组。
        /// </summary>
        /// <param name="image">The input image. 输入图像。</param>
        /// <param name="keypoints">The input and output keypoints. 输入和输出关键点。</param>
        /// <param name="imgDescriptor">The output 1 x vocabulary-size histogram descriptor. 输出的 1 x 词典尺寸直方图描述子。</param>
        /// <param name="descriptors">Optional output of raw keypoint descriptors. 可选输出原始关键点描述子。</param>
        public void Compute(Mat image, ref KeyPoint[] keypoints, Mat imgDescriptor, Mat? descriptors = null)
        {
            if (keypoints == null)
            {
                throw new ArgumentNullException(nameof(keypoints));
            }

            keypoints = Compute(image, keypoints, imgDescriptor, descriptors);
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Computes a bag-of-words descriptor from an image and span-backed keypoints.
        /// 根据图像和 Span 支持的关键点集合计算词袋描述子。
        /// </summary>
        /// <param name="image">The input image. 输入图像。</param>
        /// <param name="keypoints">The input keypoints. 输入关键点。</param>
        /// <param name="imgDescriptor">The output 1 x vocabulary-size histogram descriptor. 输出的 1 x 词典尺寸直方图描述子。</param>
        /// <param name="descriptors">Optional output of raw keypoint descriptors. 可选输出原始关键点描述子。</param>
        /// <returns>The keypoints returned by the descriptor extractor. 描述子提取器返回的关键点。</returns>
        public KeyPoint[] Compute(Mat image, ReadOnlySpan<KeyPoint> keypoints, Mat imgDescriptor, Mat? descriptors = null)
        {
            return ComputeCore(image, keypoints, imgDescriptor, descriptors);
        }

        /// <summary>
        /// Computes a bag-of-words descriptor from an image and span-backed keypoints, returning descriptor indexes grouped by vocabulary row.
        /// 根据图像和 Span 支持的关键点集合计算词袋描述子，并返回按词典行分组的描述子索引。
        /// </summary>
        /// <param name="image">The input image. 输入图像。</param>
        /// <param name="keypoints">The input keypoints. 输入关键点。</param>
        /// <param name="imgDescriptor">The output 1 x vocabulary-size histogram descriptor. 输出的 1 x 词典尺寸直方图描述子。</param>
        /// <param name="pointIdxsOfClusters">Descriptor indexes grouped by assigned vocabulary row. 按分配到的词典行分组的描述子索引。</param>
        /// <param name="descriptors">Optional output of raw keypoint descriptors. 可选输出原始关键点描述子。</param>
        /// <returns>The keypoints returned by the descriptor extractor. 描述子提取器返回的关键点。</returns>
        public KeyPoint[] Compute(Mat image, ReadOnlySpan<KeyPoint> keypoints, Mat imgDescriptor, out int[][] pointIdxsOfClusters, Mat? descriptors = null)
        {
            return ComputeCore(image, keypoints, imgDescriptor, out pointIdxsOfClusters, descriptors);
        }
#endif

        /// <summary>
        /// Computes a bag-of-words descriptor from precomputed keypoint descriptors.
        /// 根据预计算关键点描述子计算词袋描述子。
        /// </summary>
        /// <param name="keypointDescriptors">The keypoint descriptors, one descriptor per row, compatible with the trained vocabulary and matcher. 关键点描述子矩阵，每行一个描述子，并与已训练词典和匹配器兼容。</param>
        /// <param name="imgDescriptor">The output 1 x vocabulary-size histogram descriptor. 输出的 1 x 词典尺寸直方图描述子。</param>
        public void Compute(Mat keypointDescriptors, Mat imgDescriptor)
        {
            int[][] unused;
            Compute(keypointDescriptors, imgDescriptor, out unused);
        }

        /// <summary>
        /// Computes a bag-of-words descriptor from precomputed keypoint descriptors.
        /// 根据预计算关键点描述子计算词袋描述子。
        /// </summary>
        /// <param name="keypointDescriptors">The keypoint descriptors, one descriptor per row, compatible with the trained vocabulary and matcher. 关键点描述子矩阵，每行一个描述子，并与已训练词典和匹配器兼容。</param>
        /// <param name="imgDescriptor">The output 1 x vocabulary-size histogram descriptor. 输出的 1 x 词典尺寸直方图描述子。</param>
        /// <param name="normalize">Whether to normalize the histogram by descriptor count. 是否按描述子数量归一化直方图。</param>
        public void Compute(Mat keypointDescriptors, Mat imgDescriptor, bool normalize)
        {
            int[][] unused;
            Compute(keypointDescriptors, imgDescriptor, out unused, normalize);
        }

        /// <summary>
        /// Computes a bag-of-words descriptor and returns descriptor indexes grouped by vocabulary row.
        /// 计算词袋描述子，并返回按词典行分组的描述子索引。
        /// </summary>
        /// <param name="keypointDescriptors">The keypoint descriptors, one descriptor per row, compatible with the trained vocabulary and matcher. 关键点描述子矩阵，每行一个描述子，并与已训练词典和匹配器兼容。</param>
        /// <param name="imgDescriptor">The output 1 x vocabulary-size histogram descriptor. 输出的 1 x 词典尺寸直方图描述子。</param>
        /// <param name="pointIdxsOfClusters">Descriptor indexes grouped by assigned vocabulary row. 按分配到的词典行分组的描述子索引。</param>
        public void Compute(Mat keypointDescriptors, Mat imgDescriptor, out int[][] pointIdxsOfClusters)
        {
            Compute(keypointDescriptors, imgDescriptor, out pointIdxsOfClusters, normalize: true);
        }

        /// <summary>
        /// Computes a bag-of-words descriptor and returns descriptor indexes grouped by vocabulary row.
        /// 计算词袋描述子，并返回按词典行分组的描述子索引。
        /// </summary>
        /// <param name="keypointDescriptors">The keypoint descriptors, one descriptor per row, compatible with the trained vocabulary and matcher. 关键点描述子矩阵，每行一个描述子，并与已训练词典和匹配器兼容。</param>
        /// <param name="imgDescriptor">The output 1 x vocabulary-size histogram descriptor. 输出的 1 x 词典尺寸直方图描述子。</param>
        /// <param name="pointIdxsOfClusters">Descriptor indexes grouped by assigned vocabulary row. 按分配到的词典行分组的描述子索引。</param>
        /// <param name="normalize">Whether to normalize the histogram by descriptor count. 是否按描述子数量归一化直方图。</param>
        public void Compute(Mat keypointDescriptors, Mat imgDescriptor, out int[][] pointIdxsOfClusters, bool normalize)
        {
            ValidateDescriptorMatrix(keypointDescriptors, nameof(keypointDescriptors));
            ValidateOutput(imgDescriptor, nameof(imgDescriptor));
            Mat currentVocabulary = EnsureVocabulary();

            DMatch[] matches = descriptorMatcher.Match(keypointDescriptors);
            WriteHistogram(matches, keypointDescriptors.Rows, currentVocabulary.Rows, imgDescriptor, normalize, out pointIdxsOfClusters);
        }

        /// <summary>
        /// Releases the vocabulary clone owned by this extractor.
        /// 释放提取器拥有的词典克隆。
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return disposed
                ? "{Disposed=True}"
                : "{DescriptorSize=" + DescriptorSize + ",DescriptorType=" + DescriptorType + "}";
        }

        private static void ValidateVocabulary(Mat vocabulary, string parameterName)
        {
            if (vocabulary == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            if (vocabulary.Empty || vocabulary.Rows <= 0)
            {
                throw new ArgumentException("Vocabulary must contain at least one row.", parameterName);
            }
        }

        private static void ValidateDescriptorMatrix(Mat descriptors, string parameterName)
        {
            if (descriptors == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            if (descriptors.Empty || descriptors.Rows <= 0)
            {
                throw new ArgumentException("Descriptor matrix must contain at least one row.", parameterName);
            }
        }

        private static void ValidateOutput(Mat output, string parameterName)
        {
            if (output == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }

        private static void CreateEmptyOutput(Mat imgDescriptor, Mat? descriptors, Feature2D extractor)
        {
            imgDescriptor.Create(0, 0, MatType.CV_32FC1);
            if (descriptors != null)
            {
                descriptors.Create(0, Math.Max(extractor.DescriptorSize, 0), extractor.DescriptorType);
            }
        }

        private static KeyPoint[] ComputeDescriptors(Feature2D extractor, Mat image, KeyPoint[] keypoints, Mat descriptors)
        {
            if (extractor is ORB orb)
            {
                return orb.Compute(image, keypoints, descriptors);
            }

            if (extractor is SIFT sift)
            {
                return sift.Compute(image, keypoints, descriptors);
            }

            if (extractor is BRISK brisk)
            {
                return brisk.Compute(image, keypoints, descriptors);
            }

            if (extractor is KAZE kaze)
            {
                return kaze.Compute(image, keypoints, descriptors);
            }

            if (extractor is AKAZE akaze)
            {
                return akaze.Compute(image, keypoints, descriptors);
            }

            throw new NotSupportedException("The supplied Feature2D type does not expose descriptor computation yet.");
        }

#if NETCOREAPP3_1_OR_GREATER
        private static KeyPoint[] ComputeDescriptors(Feature2D extractor, Mat image, ReadOnlySpan<KeyPoint> keypoints, Mat descriptors)
        {
            if (extractor is ORB orb)
            {
                return orb.Compute(image, keypoints, descriptors);
            }

            if (extractor is SIFT sift)
            {
                return sift.Compute(image, keypoints, descriptors);
            }

            if (extractor is BRISK brisk)
            {
                return brisk.Compute(image, keypoints, descriptors);
            }

            if (extractor is KAZE kaze)
            {
                return kaze.Compute(image, keypoints, descriptors);
            }

            if (extractor is AKAZE akaze)
            {
                return akaze.Compute(image, keypoints, descriptors);
            }

            throw new NotSupportedException("The supplied Feature2D type does not expose descriptor computation yet.");
        }
#endif

        private KeyPoint[] ComputeCore(Mat image, KeyPoint[] keypoints, Mat imgDescriptor, Mat? descriptors)
        {
            ValidateImage(image);
            ValidateOutput(imgDescriptor, nameof(imgDescriptor));
            Feature2D extractor = EnsureDescriptorExtractor();

            if (keypoints.Length == 0)
            {
                CreateEmptyOutput(imgDescriptor, descriptors, extractor);
                return Array.Empty<KeyPoint>();
            }

            EnsureVocabulary();
            using (Mat keypointDescriptors = new Mat())
            {
                KeyPoint[] result = ComputeDescriptors(extractor, image, keypoints, keypointDescriptors);
                FinishImageCompute(keypointDescriptors, imgDescriptor, out int[][] unused, descriptors, extractor);
                return result;
            }
        }

        private KeyPoint[] ComputeCore(Mat image, KeyPoint[] keypoints, Mat imgDescriptor, out int[][] pointIdxsOfClusters, Mat? descriptors)
        {
            ValidateImage(image);
            ValidateOutput(imgDescriptor, nameof(imgDescriptor));
            Feature2D extractor = EnsureDescriptorExtractor();

            if (keypoints.Length == 0)
            {
                CreateEmptyOutput(imgDescriptor, descriptors, extractor);
                pointIdxsOfClusters = Array.Empty<int[]>();
                return Array.Empty<KeyPoint>();
            }

            EnsureVocabulary();
            using (Mat keypointDescriptors = new Mat())
            {
                KeyPoint[] result = ComputeDescriptors(extractor, image, keypoints, keypointDescriptors);
                FinishImageCompute(keypointDescriptors, imgDescriptor, out pointIdxsOfClusters, descriptors, extractor);
                return result;
            }
        }

#if NETCOREAPP3_1_OR_GREATER
        private KeyPoint[] ComputeCore(Mat image, ReadOnlySpan<KeyPoint> keypoints, Mat imgDescriptor, Mat? descriptors)
        {
            ValidateImage(image);
            ValidateOutput(imgDescriptor, nameof(imgDescriptor));
            Feature2D extractor = EnsureDescriptorExtractor();

            if (keypoints.IsEmpty)
            {
                CreateEmptyOutput(imgDescriptor, descriptors, extractor);
                return Array.Empty<KeyPoint>();
            }

            EnsureVocabulary();
            using (Mat keypointDescriptors = new Mat())
            {
                KeyPoint[] result = ComputeDescriptors(extractor, image, keypoints, keypointDescriptors);
                FinishImageCompute(keypointDescriptors, imgDescriptor, out int[][] unused, descriptors, extractor);
                return result;
            }
        }

        private KeyPoint[] ComputeCore(Mat image, ReadOnlySpan<KeyPoint> keypoints, Mat imgDescriptor, out int[][] pointIdxsOfClusters, Mat? descriptors)
        {
            ValidateImage(image);
            ValidateOutput(imgDescriptor, nameof(imgDescriptor));
            Feature2D extractor = EnsureDescriptorExtractor();

            if (keypoints.IsEmpty)
            {
                CreateEmptyOutput(imgDescriptor, descriptors, extractor);
                pointIdxsOfClusters = Array.Empty<int[]>();
                return Array.Empty<KeyPoint>();
            }

            EnsureVocabulary();
            using (Mat keypointDescriptors = new Mat())
            {
                KeyPoint[] result = ComputeDescriptors(extractor, image, keypoints, keypointDescriptors);
                FinishImageCompute(keypointDescriptors, imgDescriptor, out pointIdxsOfClusters, descriptors, extractor);
                return result;
            }
        }
#endif

        private void FinishImageCompute(Mat keypointDescriptors, Mat imgDescriptor, out int[][] pointIdxsOfClusters, Mat? descriptors, Feature2D extractor)
        {
            if (keypointDescriptors.Empty || keypointDescriptors.Rows == 0)
            {
                CreateEmptyOutput(imgDescriptor, descriptors, extractor);
                pointIdxsOfClusters = Array.Empty<int[]>();
                return;
            }

            Compute(keypointDescriptors, imgDescriptor, out pointIdxsOfClusters);
            if (descriptors != null)
            {
                keypointDescriptors.CopyTo(descriptors);
            }
        }

        private static void ValidateImage(Mat image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }
        }

        private Feature2D EnsureDescriptorExtractor()
        {
            ThrowIfDisposed();
            if (descriptorExtractor == null)
            {
                throw new InvalidOperationException("This instance was created without a descriptor extractor.");
            }

            return descriptorExtractor;
        }

        private Mat EnsureVocabulary()
        {
            ThrowIfDisposed();
            if (vocabulary == null || vocabulary.Empty)
            {
                throw new InvalidOperationException("Vocabulary has not been set.");
            }

            return vocabulary;
        }

        private void WriteHistogram(DMatch[] matches, int descriptorRowCount, int clusterCount, Mat imgDescriptor, bool normalize, out int[][] pointIdxsOfClusters)
        {
            imgDescriptor.Create(1, clusterCount, MatType.CV_32FC1);
            imgDescriptor.SetTo(new Scalar(0));

            var clusterIndexes = new List<int>[clusterCount];
            for (int i = 0; i < clusterIndexes.Length; i++)
            {
                clusterIndexes[i] = new List<int>();
            }

#if NETCOREAPP3_1_OR_GREATER
            Span<float> histogram = imgDescriptor.AsSpan<float>();
            AccumulateHistogram(matches, descriptorRowCount, histogram, clusterIndexes, normalize);
#else
            var histogram = new float[clusterCount];
            AccumulateHistogram(matches, descriptorRowCount, histogram, clusterIndexes, normalize);
            Marshal.Copy(histogram, 0, imgDescriptor.Data, histogram.Length);
#endif

            pointIdxsOfClusters = new int[clusterIndexes.Length][];
            for (int i = 0; i < clusterIndexes.Length; i++)
            {
                pointIdxsOfClusters[i] = clusterIndexes[i].ToArray();
            }
        }

#if NETCOREAPP3_1_OR_GREATER
        private static void AccumulateHistogram(DMatch[] matches, int descriptorRowCount, Span<float> histogram, List<int>[] pointIdxsOfClusters, bool normalize)
        {
            for (int i = 0; i < matches.Length; i++)
            {
                DMatch match = matches[i];
                if (match.TrainIdx < 0 || match.TrainIdx >= histogram.Length)
                {
                    throw new OpenCvException("Matcher returned a train index outside the vocabulary range.");
                }

                histogram[match.TrainIdx] += 1.0F;
                pointIdxsOfClusters[match.TrainIdx].Add(match.QueryIdx);
            }

            if (!normalize)
            {
                return;
            }

            float scale = descriptorRowCount <= 0 ? 0.0F : 1.0F / descriptorRowCount;
            for (int i = 0; i < histogram.Length; i++)
            {
                histogram[i] *= scale;
            }
        }
#else
        private static void AccumulateHistogram(DMatch[] matches, int descriptorRowCount, float[] histogram, List<int>[] pointIdxsOfClusters, bool normalize)
        {
            for (int i = 0; i < matches.Length; i++)
            {
                DMatch match = matches[i];
                if (match.TrainIdx < 0 || match.TrainIdx >= histogram.Length)
                {
                    throw new OpenCvException("Matcher returned a train index outside the vocabulary range.");
                }

                histogram[match.TrainIdx] += 1.0F;
                pointIdxsOfClusters[match.TrainIdx].Add(match.QueryIdx);
            }

            if (!normalize)
            {
                return;
            }

            float scale = descriptorRowCount <= 0 ? 0.0F : 1.0F / descriptorRowCount;
            for (int i = 0; i < histogram.Length; i++)
            {
                histogram[i] *= scale;
            }
        }
#endif

        private void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing && vocabulary != null)
            {
                vocabulary.Dispose();
                vocabulary = null;
            }

            disposed = true;
        }

        private void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(BOWImgDescriptorExtractor));
            }
        }
    }
}
