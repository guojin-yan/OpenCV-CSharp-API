using System;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Core
{
    public static partial class Cv2
    {
        /// <summary>
        /// Horizontally concatenates matrices.
        /// 横向拼接多个矩阵。
        /// </summary>
        /// <param name="src">The source matrices. 源矩阵集合。</param>
        /// <param name="dst">The destination matrix. 目标矩阵。</param>
        public static void HConcat(Mat[] src, Mat dst)
        {
            ValidateNonEmpty(src, nameof(src));
            ValidateNotNull(dst, nameof(dst));

#if NETCOREAPP3_1_OR_GREATER
            HConcat(src.AsSpan(), dst);
#else
            IntPtr[] handles = new IntPtr[src.Length];
            Mat first = src[0];
            if (first == null)
            {
                throw new ArgumentNullException(nameof(src));
            }

            ValidateHConcatSource(first, first, nameof(src));
            handles[0] = first.NativeHandle;

            for (int i = 1; i < src.Length; i++)
            {
                if (src[i] == null)
                {
                    throw new ArgumentNullException(nameof(src));
                }

                ValidateHConcatSource(src[i], first, nameof(src));
                handles[i] = src[i].NativeHandle;
            }

            NativeException.ThrowIfError(NativeMethods.CoreHConcat(handles, handles.Length, dst.NativeHandle));
#endif
        }

        /// <summary>
        /// Horizontally concatenates matrices and returns a new matrix.
        /// 横向拼接多个矩阵并返回新矩阵。
        /// </summary>
        /// <param name="src">The source matrices. 源矩阵集合。</param>
        /// <returns>The concatenated matrix. 拼接后的矩阵。</returns>
        public static Mat HConcat(Mat[] src)
        {
            var dst = new Mat();
            try
            {
                HConcat(src, dst);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Horizontally concatenates matrices from a span-backed collection.
        /// 从 Span 支持的集合横向拼接矩阵。
        /// </summary>
        /// <param name="src">The source matrices. 源矩阵集合。</param>
        /// <param name="dst">The destination matrix. 目标矩阵。</param>
        public static unsafe void HConcat(ReadOnlySpan<Mat> src, Mat dst)
        {
            ValidateNonEmpty(src, nameof(src));
            ValidateNotNull(dst, nameof(dst));
            ValidateHConcatInputs(src, nameof(src));

            IntPtr[] handles = new IntPtr[src.Length];
            for (int i = 0; i < src.Length; i++)
            {
                if (src[i] == null)
                {
                    throw new ArgumentNullException(nameof(src));
                }

                handles[i] = src[i].NativeHandle;
            }

            fixed (IntPtr* handlesPtr = handles)
            {
                NativeException.ThrowIfError(NativeMethods.CoreHConcatPtr(handlesPtr, handles.Length, dst.NativeHandle));
            }
        }

        /// <summary>
        /// Horizontally concatenates matrices from a span-backed collection and returns a new matrix.
        /// 从 Span 支持的集合横向拼接矩阵并返回新矩阵。
        /// </summary>
        /// <param name="src">The source matrices. 源矩阵集合。</param>
        /// <returns>The concatenated matrix. 拼接后的矩阵。</returns>
        public static Mat HConcat(ReadOnlySpan<Mat> src)
        {
            var dst = new Mat();
            try
            {
                HConcat(src, dst);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        private static void ValidateHConcatInputs(ReadOnlySpan<Mat> src, string parameterName)
        {
            Mat first = src[0];
            if (first == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            ValidateHConcatSource(first, first, parameterName);
            for (int i = 1; i < src.Length; i++)
            {
                if (src[i] == null)
                {
                    throw new ArgumentNullException(parameterName);
                }

                ValidateHConcatSource(src[i], first, parameterName);
            }
        }
#endif

        private static void ValidateHConcatSource(Mat mat, Mat first, string parameterName)
        {
            if (mat.Dims > 2)
            {
                throw new ArgumentException("HConcat source matrices must have at most two dimensions.", parameterName);
            }

            if (mat.Rows != first.Rows)
            {
                throw new ArgumentException("HConcat source matrices must have the same number of rows.", parameterName);
            }

            if (mat.Type != first.Type)
            {
                throw new ArgumentException("HConcat source matrices must have the same type.", parameterName);
            }
        }

        /// <summary>
        /// Vertically concatenates matrices.
        /// 纵向拼接多个矩阵。
        /// </summary>
        /// <param name="src">The source matrices. 源矩阵集合。</param>
        /// <param name="dst">The destination matrix. 目标矩阵。</param>
        public static void VConcat(Mat[] src, Mat dst)
        {
            ValidateNonEmpty(src, nameof(src));
            ValidateNotNull(dst, nameof(dst));

#if NETCOREAPP3_1_OR_GREATER
            VConcat(src.AsSpan(), dst);
#else
            IntPtr[] handles = new IntPtr[src.Length];
            Mat first = src[0];
            if (first == null)
            {
                throw new ArgumentNullException(nameof(src));
            }

            ValidateVConcatSource(first, first, nameof(src));
            handles[0] = first.NativeHandle;

            for (int i = 1; i < src.Length; i++)
            {
                if (src[i] == null)
                {
                    throw new ArgumentNullException(nameof(src));
                }

                ValidateVConcatSource(src[i], first, nameof(src));
                handles[i] = src[i].NativeHandle;
            }

            NativeException.ThrowIfError(NativeMethods.CoreVConcat(handles, handles.Length, dst.NativeHandle));
#endif
        }

        /// <summary>
        /// Vertically concatenates matrices and returns a new matrix.
        /// 纵向拼接多个矩阵并返回新矩阵。
        /// </summary>
        /// <param name="src">The source matrices. 源矩阵集合。</param>
        /// <returns>The concatenated matrix. 拼接后的矩阵。</returns>
        public static Mat VConcat(Mat[] src)
        {
            var dst = new Mat();
            try
            {
                VConcat(src, dst);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Vertically concatenates matrices from a span-backed collection.
        /// 从 Span 支持的集合纵向拼接矩阵。
        /// </summary>
        /// <param name="src">The source matrices. 源矩阵集合。</param>
        /// <param name="dst">The destination matrix. 目标矩阵。</param>
        public static unsafe void VConcat(ReadOnlySpan<Mat> src, Mat dst)
        {
            ValidateNonEmpty(src, nameof(src));
            ValidateNotNull(dst, nameof(dst));
            ValidateVConcatInputs(src, nameof(src));

            IntPtr[] handles = new IntPtr[src.Length];
            for (int i = 0; i < src.Length; i++)
            {
                if (src[i] == null)
                {
                    throw new ArgumentNullException(nameof(src));
                }

                handles[i] = src[i].NativeHandle;
            }

            fixed (IntPtr* handlesPtr = handles)
            {
                NativeException.ThrowIfError(NativeMethods.CoreVConcatPtr(handlesPtr, handles.Length, dst.NativeHandle));
            }
        }

        /// <summary>
        /// Vertically concatenates matrices from a span-backed collection and returns a new matrix.
        /// 从 Span 支持的集合纵向拼接矩阵并返回新矩阵。
        /// </summary>
        /// <param name="src">The source matrices. 源矩阵集合。</param>
        /// <returns>The concatenated matrix. 拼接后的矩阵。</returns>
        public static Mat VConcat(ReadOnlySpan<Mat> src)
        {
            var dst = new Mat();
            try
            {
                VConcat(src, dst);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        private static void ValidateVConcatInputs(ReadOnlySpan<Mat> src, string parameterName)
        {
            Mat first = src[0];
            if (first == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            ValidateVConcatSource(first, first, parameterName);
            for (int i = 1; i < src.Length; i++)
            {
                if (src[i] == null)
                {
                    throw new ArgumentNullException(parameterName);
                }

                ValidateVConcatSource(src[i], first, parameterName);
            }
        }
#endif

        private static void ValidateVConcatSource(Mat mat, Mat first, string parameterName)
        {
            if (mat.Dims > 2)
            {
                throw new ArgumentException("VConcat source matrices must have at most two dimensions.", parameterName);
            }

            if (mat.Cols != first.Cols)
            {
                throw new ArgumentException("VConcat source matrices must have the same number of columns.", parameterName);
            }

            if (mat.Type != first.Type)
            {
                throw new ArgumentException("VConcat source matrices must have the same type.", parameterName);
            }
        }

        /// <summary>
        /// Finds cluster centers and labels using OpenCV <c>cv::kmeans</c>.
        /// 使用 OpenCV <c>cv::kmeans</c> 查找聚类中心和标签。
        /// </summary>
        /// <param name="data">The input samples, one sample per row. 输入样本，每行一个样本。</param>
        /// <param name="k">The number of clusters. 聚类数量。</param>
        /// <param name="bestLabels">The output or initial labels matrix. 输出或初始标签矩阵。</param>
        /// <param name="criteria">The termination criteria. 终止条件。</param>
        /// <param name="attempts">The number of independent attempts. 独立尝试次数。</param>
        /// <param name="flags">The center initialization flags. 中心初始化标志。</param>
        /// <param name="centers">The output cluster centers. 输出聚类中心。</param>
        /// <returns>The compactness value returned by OpenCV. OpenCV 返回的紧凑度值。</returns>
        public static double KMeans(Mat data, int k, Mat bestLabels, TermCriteria criteria, int attempts, KMeansFlags flags, Mat centers)
        {
            ValidateNotNull(data, nameof(data));
            ValidateNotNull(bestLabels, nameof(bestLabels));
            ValidateNotNull(centers, nameof(centers));
            ValidatePositive(k, nameof(k));
            ValidatePositive(attempts, nameof(attempts));
            ValidateKMeansFlags(flags, nameof(flags));
            ValidateKMeansInput(data, k);

            NativeException.ThrowIfError(NativeMethods.CoreKMeans(
                data.NativeHandle,
                k,
                bestLabels.NativeHandle,
                (int)criteria.Type,
                criteria.MaxCount,
                criteria.Epsilon,
                attempts,
                (int)flags,
                centers.NativeHandle,
                out double compactness));
            return compactness;
        }

        /// <summary>
        /// Finds cluster centers and labels using default count-and-epsilon termination criteria.
        /// 使用默认次数与精度终止条件查找聚类中心和标签。
        /// </summary>
        /// <param name="data">The input samples, one sample per row. 输入样本，每行一个样本。</param>
        /// <param name="k">The number of clusters. 聚类数量。</param>
        /// <param name="bestLabels">The output labels matrix. 输出标签矩阵。</param>
        /// <param name="centers">The output cluster centers. 输出聚类中心。</param>
        /// <returns>The compactness value returned by OpenCV. OpenCV 返回的紧凑度值。</returns>
        public static double KMeans(Mat data, int k, Mat bestLabels, Mat centers)
        {
            return KMeans(data, k, bestLabels, TermCriteria.ByCountAndEpsilon(10, 1.0), 3, KMeansFlags.PpCenters, centers);
        }

        private static void ValidateKMeansInput(Mat data, int k)
        {
            if (data.Dims > 2)
            {
                throw new ArgumentException("KMeans data matrix must have at most two dimensions.", nameof(data));
            }

            if (data.Depth != MatType.CV_32F)
            {
                throw new ArgumentException("KMeans data matrix must have CV_32F depth.", nameof(data));
            }

            int sampleCount = data.Rows == 1 ? data.Cols : data.Rows;
            if (sampleCount < k)
            {
                throw new ArgumentException("KMeans cluster count cannot exceed the number of samples.", nameof(k));
            }
        }

        private static void ValidateKMeansFlags(KMeansFlags value, string parameterName)
        {
            switch (value)
            {
                case KMeansFlags.RandomCenters:
                case KMeansFlags.UseInitialLabels:
                case KMeansFlags.PpCenters:
                    return;
                default:
                    throw new ArgumentOutOfRangeException(parameterName, "Unsupported k-means initialization flag.");
            }
        }

#if NETCOREAPP3_1_OR_GREATER
        private static void ValidateNonEmpty<T>(ReadOnlySpan<T> values, string parameterName)
            where T : class
        {
            if (values.IsEmpty)
            {
                throw new ArgumentException("Span cannot be empty.", parameterName);
            }
        }
#endif
    }
}
