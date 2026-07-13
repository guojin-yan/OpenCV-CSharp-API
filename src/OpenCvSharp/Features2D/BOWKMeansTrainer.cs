using System;
using System.Collections.Generic;
using OpenCvSharp.Core;
using CoreCv2 = OpenCvSharp.Core.Cv2;

#if NET5_0_OR_GREATER
using System.Runtime.InteropServices;
#endif

namespace OpenCvSharp.Features2D
{
    /// <summary>
    /// Trains a bag-of-visual-words vocabulary with OpenCV <c>cv::kmeans</c>.
    /// 使用 OpenCV <c>cv::kmeans</c> 训练视觉词袋词典。
    /// Input descriptor matrices must contain one <c>CV_32F</c> descriptor per row and remain column/type compatible within a collection.
    /// 输入描述子矩阵必须每行包含一个 <c>CV_32F</c> 描述子，且同一集合内列数和类型保持兼容。
    /// </summary>
    public sealed class BOWKMeansTrainer : IDisposable
    {
        private readonly List<Mat> descriptors = new List<Mat>();
        private int descriptorsCount;
        private bool disposed;

        /// <summary>
        /// Initializes a new vocabulary trainer.
        /// 初始化新的视觉词袋词典训练器。
        /// </summary>
        /// <param name="clusterCount">The number of vocabulary clusters. 词典聚类中心数量。</param>
        /// <param name="termCriteria">The k-means termination criteria, or default criteria when null. k-means 终止条件；为空时使用默认条件。</param>
        /// <param name="attempts">The number of independent k-means attempts. k-means 独立尝试次数。</param>
        /// <param name="flags">The k-means center initialization flags. k-means 聚类中心初始化标志。</param>
        public BOWKMeansTrainer(
            int clusterCount,
            TermCriteria? termCriteria = null,
            int attempts = 3,
            KMeansFlags flags = KMeansFlags.PpCenters)
        {
            if (clusterCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(clusterCount), "Cluster count must be positive.");
            }

            if (attempts <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(attempts), "Attempts must be positive.");
            }

            ClusterCount = clusterCount;
            TermCriteria = termCriteria ?? TermCriteria.ByCountAndEpsilon(100, 0.001);
            Attempts = attempts;
            Flags = flags;
        }

        /// <summary>
        /// Gets the requested vocabulary cluster count.
        /// 获取请求的词典聚类中心数量。
        /// </summary>
        public int ClusterCount { get; }

        /// <summary>
        /// Gets the k-means termination criteria.
        /// 获取 k-means 终止条件。
        /// </summary>
        public TermCriteria TermCriteria { get; }

        /// <summary>
        /// Gets the number of independent k-means attempts.
        /// 获取 k-means 独立尝试次数。
        /// </summary>
        public int Attempts { get; }

        /// <summary>
        /// Gets the k-means center initialization flags.
        /// 获取 k-means 聚类中心初始化标志。
        /// </summary>
        public KMeansFlags Flags { get; }

        /// <summary>
        /// Gets the total number of descriptor rows currently stored by the trainer.
        /// 获取训练器当前保存的描述子总行数。
        /// </summary>
        public int DescriptorsCount
        {
            get
            {
                ThrowIfDisposed();
                return descriptorsCount;
            }
        }

        /// <summary>
        /// Gets whether descriptor rows have been added to the trainer.
        /// 获取训练器是否已经添加描述子行。
        /// </summary>
        public bool HasDescriptors
        {
            get { return DescriptorsCount > 0; }
        }

        /// <summary>
        /// Gets a value indicating whether this trainer has been disposed.
        /// 获取训练器是否已经释放。
        /// </summary>
        public bool IsDisposed
        {
            get { return disposed; }
        }

        /// <summary>
        /// Adds descriptor rows to the training set.
        /// 向训练集合添加描述子行。
        /// </summary>
        /// <param name="descriptorRows">The non-empty <c>CV_32F</c> descriptor matrix, with one descriptor per row. 非空 <c>CV_32F</c> 描述子矩阵，每行一个描述子。</param>
        public void Add(Mat descriptorRows)
        {
            ThrowIfDisposed();
            ValidateDescriptorRows(descriptorRows, nameof(descriptorRows));
            ValidateDescriptorCompatibility(descriptorRows, nameof(descriptorRows));

            descriptors.Add(descriptorRows.Clone());
            descriptorsCount += descriptorRows.Rows;
        }

        /// <summary>
        /// Adds multiple descriptor matrices to the training set.
        /// 向训练集合添加多个描述子矩阵。
        /// </summary>
        /// <param name="descriptorRows">The compatible <c>CV_32F</c> descriptor matrices, with one descriptor per row. 兼容的 <c>CV_32F</c> 描述子矩阵集合，每行一个描述子。</param>
        public void Add(Mat[] descriptorRows)
        {
            if (descriptorRows == null)
            {
                throw new ArgumentNullException(nameof(descriptorRows));
            }

            ThrowIfDisposed();
            for (int i = 0; i < descriptorRows.Length; i++)
            {
                Add(descriptorRows[i]);
            }
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Adds descriptor matrices from a span-backed collection.
        /// 从 Span 支持的集合向训练集合添加描述子矩阵。
        /// </summary>
        /// <param name="descriptorRows">The compatible <c>CV_32F</c> descriptor matrices, with one descriptor per row. 兼容的 <c>CV_32F</c> 描述子矩阵集合，每行一个描述子。</param>
        public void Add(ReadOnlySpan<Mat> descriptorRows)
        {
            ThrowIfDisposed();
            for (int i = 0; i < descriptorRows.Length; i++)
            {
                Add(descriptorRows[i]);
            }
        }
#endif

        /// <summary>
        /// Gets cloned descriptor matrices currently stored by the trainer.
        /// 获取训练器当前保存的描述子矩阵克隆集合。
        /// </summary>
        /// <returns>The descriptor matrices. 描述子矩阵集合。</returns>
        public Mat[] GetDescriptors()
        {
            ThrowIfDisposed();

            if (descriptors.Count == 0)
            {
                return Array.Empty<Mat>();
            }

            var result = new Mat[descriptors.Count];
            for (int i = 0; i < descriptors.Count; i++)
            {
                result[i] = descriptors[i].Clone();
            }

            return result;
        }

        /// <summary>
        /// Clears all stored descriptor matrices.
        /// 清除所有已保存的描述子矩阵。
        /// </summary>
        public void Clear()
        {
            ThrowIfDisposed();
            ClearDescriptors();
        }

        /// <summary>
        /// Clusters the stored descriptor rows and returns the vocabulary centers.
        /// 对已保存的描述子行聚类，并返回词典中心。
        /// </summary>
        /// <returns>The vocabulary matrix, one visual word per row. 词典矩阵，每行一个视觉词。</returns>
        public Mat Cluster()
        {
            ThrowIfDisposed();
            if (descriptors.Count == 0)
            {
                throw new InvalidOperationException("No descriptor rows have been added.");
            }

            if (ClusterCount > descriptorsCount)
            {
                throw new InvalidOperationException("Cluster count cannot exceed the number of descriptor rows.");
            }

#if NET5_0_OR_GREATER
            using (Mat merged = CoreCv2.VConcat(CollectionsMarshal.AsSpan(descriptors)))
#else
            using (Mat merged = CoreCv2.VConcat(descriptors.ToArray()))
#endif
            {
                return Cluster(merged);
            }
        }

        /// <summary>
        /// Clusters the supplied descriptor rows and returns the vocabulary centers.
        /// 对传入的描述子行聚类，并返回词典中心。
        /// </summary>
        /// <param name="descriptorRows">The non-empty <c>CV_32F</c> descriptor matrix, with one descriptor per row. 非空 <c>CV_32F</c> 描述子矩阵，每行一个描述子。</param>
        /// <returns>The vocabulary matrix, one visual word per row. 词典矩阵，每行一个视觉词。</returns>
        public Mat Cluster(Mat descriptorRows)
        {
            ThrowIfDisposed();
            ValidateDescriptorRows(descriptorRows, nameof(descriptorRows));

            if (ClusterCount > descriptorRows.Rows)
            {
                throw new ArgumentException("Cluster count cannot exceed the number of descriptor rows.", nameof(descriptorRows));
            }

            using (Mat labels = new Mat())
            {
                var centers = new Mat();
                try
                {
                    CoreCv2.KMeans(descriptorRows, ClusterCount, labels, TermCriteria, Attempts, Flags, centers);
                    return centers;
                }
                catch
                {
                    centers.Dispose();
                    throw;
                }
            }
        }

        /// <summary>
        /// Clusters the supplied descriptor matrices and returns the vocabulary centers.
        /// 对传入的描述子矩阵集合聚类，并返回词典中心。
        /// </summary>
        /// <param name="descriptorRows">The compatible non-empty <c>CV_32F</c> descriptor matrices, with one descriptor per row. 兼容且非空的 <c>CV_32F</c> 描述子矩阵集合，每行一个描述子。</param>
        /// <returns>The vocabulary matrix, one visual word per row. 词典矩阵，每行一个视觉词。</returns>
        public Mat Cluster(Mat[] descriptorRows)
        {
            if (descriptorRows == null)
            {
                throw new ArgumentNullException(nameof(descriptorRows));
            }

            ThrowIfDisposed();
            if (descriptorRows.Length == 0)
            {
                throw new ArgumentException("Descriptor matrix collection cannot be empty.", nameof(descriptorRows));
            }

            ValidateDescriptorCollection(descriptorRows, nameof(descriptorRows));
            using (Mat merged = CoreCv2.VConcat(descriptorRows))
            {
                return Cluster(merged);
            }
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Clusters the supplied span-backed descriptor matrices and returns the vocabulary centers.
        /// 对传入的 Span 支持描述子矩阵集合聚类，并返回词典中心。
        /// </summary>
        /// <param name="descriptorRows">The compatible non-empty <c>CV_32F</c> descriptor matrices, with one descriptor per row. 兼容且非空的 <c>CV_32F</c> 描述子矩阵集合，每行一个描述子。</param>
        /// <returns>The vocabulary matrix, one visual word per row. 词典矩阵，每行一个视觉词。</returns>
        public Mat Cluster(ReadOnlySpan<Mat> descriptorRows)
        {
            ThrowIfDisposed();
            if (descriptorRows.IsEmpty)
            {
                throw new ArgumentException("Descriptor matrix collection cannot be empty.", nameof(descriptorRows));
            }

            ValidateDescriptorCollection(descriptorRows, nameof(descriptorRows));
            using (Mat merged = CoreCv2.VConcat(descriptorRows))
            {
                return Cluster(merged);
            }
        }
#endif

        /// <summary>
        /// Releases cloned descriptor matrices owned by this trainer.
        /// 释放训练器拥有的描述子矩阵克隆。
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
                : "{ClusterCount=" + ClusterCount + ",DescriptorsCount=" + DescriptorsCount + ",Attempts=" + Attempts + "}";
        }

        private static void ValidateDescriptorRows(Mat descriptorRows, string parameterName)
        {
            if (descriptorRows == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            if (descriptorRows.Empty || descriptorRows.Rows <= 0)
            {
                throw new ArgumentException("Descriptor matrix must contain at least one row.", parameterName);
            }

            if (descriptorRows.Cols <= 0)
            {
                throw new ArgumentException("Descriptor matrix must contain at least one column.", parameterName);
            }

            if (MatType.Depth(descriptorRows.Type) != MatType.CV_32F)
            {
                throw new ArgumentException("BOW k-means training requires CV_32F descriptor rows.", parameterName);
            }
        }

        private static void ValidateDescriptorCollection(Mat[] descriptorRows, string parameterName)
        {
            for (int i = 0; i < descriptorRows.Length; i++)
            {
                ValidateDescriptorRows(descriptorRows[i], parameterName);
                if (i > 0)
                {
                    ValidateDescriptorCompatibility(descriptorRows[0], descriptorRows[i], parameterName);
                }
            }
        }

#if NETCOREAPP3_1_OR_GREATER
        private static void ValidateDescriptorCollection(ReadOnlySpan<Mat> descriptorRows, string parameterName)
        {
            for (int i = 0; i < descriptorRows.Length; i++)
            {
                ValidateDescriptorRows(descriptorRows[i], parameterName);
                if (i > 0)
                {
                    ValidateDescriptorCompatibility(descriptorRows[0], descriptorRows[i], parameterName);
                }
            }
        }
#endif

        private void ValidateDescriptorCompatibility(Mat descriptorRows, string parameterName)
        {
            if (descriptors.Count == 0)
            {
                return;
            }

            ValidateDescriptorCompatibility(descriptors[0], descriptorRows, parameterName);
        }

        private static void ValidateDescriptorCompatibility(Mat reference, Mat descriptorRows, string parameterName)
        {
            if (descriptorRows.Cols != reference.Cols)
            {
                throw new ArgumentException("Descriptor column count must match previous descriptors.", parameterName);
            }

            if (descriptorRows.Type != reference.Type)
            {
                throw new ArgumentException("Descriptor matrix type must match previous descriptors.", parameterName);
            }
        }

        private void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                ClearDescriptors();
            }

            disposed = true;
        }

        private void ClearDescriptors()
        {
            for (int i = 0; i < descriptors.Count; i++)
            {
                descriptors[i].Dispose();
            }

            descriptors.Clear();
            descriptorsCount = 0;
        }

        private void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(BOWKMeansTrainer));
            }
        }
    }
}
