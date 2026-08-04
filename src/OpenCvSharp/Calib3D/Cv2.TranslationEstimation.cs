using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Calib3D
{
    public static partial class Cv2
    {
        /// <summary>
        /// Estimates a pure 3D translation with RANSAC.
        /// 使用 RANSAC 估计纯三维平移。
        /// </summary>
        /// <param name="source">The source 3D points. 源三维点。</param>
        /// <param name="destination">The destination 3D points. 目标三维点。</param>
        /// <param name="translation">The caller-owned <c>1 x 3 CV_64F</c> translation output. 调用方持有的 <c>1 x 3 CV_64F</c> 平移输出。</param>
        /// <param name="inliers">The optional caller-owned <c>N x 1 CV_8U</c> inlier mask. 可选的调用方持有 <c>N x 1 CV_8U</c> 内点掩码。</param>
        /// <param name="ransacThreshold">The positive RANSAC residual threshold. 正的 RANSAC 残差阈值。</param>
        /// <param name="confidence">The confidence strictly between zero and one. 严格位于零和一之间的置信度。</param>
        /// <returns><c>true</c> if a translation was found; otherwise, <c>false</c>. 如果找到平移则返回 <c>true</c>，否则返回 <c>false</c>。</returns>
        public static bool EstimateTranslation3D(
            Mat source,
            Mat destination,
            Mat translation,
            Mat? inliers = null,
            double ransacThreshold = 3.0,
            double confidence = 0.99)
        {
            ThrowIfNull(source, nameof(source));
            ThrowIfNull(destination, nameof(destination));
            ThrowIfNull(translation, nameof(translation));
            int sourceCount = ValidateTranslationPointSet(
                source,
                3,
                4,
                nameof(source));
            int destinationCount = ValidateTranslationPointSet(
                destination,
                3,
                4,
                nameof(destination));
            ValidateMatchingTranslationPointCounts(
                sourceCount,
                destinationCount,
                nameof(destination));
            ValidateTranslationThreshold(ransacThreshold, nameof(ransacThreshold));
            ValidateTranslationConfidence(confidence, nameof(confidence));

            NativeException.ThrowIfError(NativeMethods.Calib3DEstimateTranslation3D(
                source.NativeHandle,
                destination.NativeHandle,
                translation.NativeHandle,
                GetNativeHandleOrZero(inliers),
                ransacThreshold,
                confidence,
                out int found));
            return found != 0;
        }

        /// <summary>
        /// Estimates a pure 3D translation and returns owned output matrices.
        /// 估计纯三维平移并返回拥有所有权的输出矩阵。
        /// </summary>
        public static bool EstimateTranslation3D(
            Mat source,
            Mat destination,
            out Mat translation,
            out Mat inliers,
            double ransacThreshold = 3.0,
            double confidence = 0.99)
        {
            translation = new Mat();
            inliers = new Mat();
            try
            {
                return EstimateTranslation3D(
                    source,
                    destination,
                    translation,
                    inliers,
                    ransacThreshold,
                    confidence);
            }
            catch
            {
                translation.Dispose();
                inliers.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Estimates a pure 3D translation from managed point arrays.
        /// 根据托管点数组估计纯三维平移。
        /// </summary>
        public static bool EstimateTranslation3D(
            Point3f[] source,
            Point3f[] destination,
            Mat translation,
            Mat? inliers = null,
            double ransacThreshold = 3.0,
            double confidence = 0.99)
        {
            ValidateMatchingTranslationArrays(
                source,
                destination,
                4);
            using (Mat sourceMat = ToPointMat(source))
            using (Mat destinationMat = ToPointMat(destination))
            {
                return EstimateTranslation3D(
                    sourceMat,
                    destinationMat,
                    translation,
                    inliers,
                    ransacThreshold,
                    confidence);
            }
        }

        /// <summary>
        /// Estimates a pure 3D translation from managed point arrays and returns owned outputs.
        /// 根据托管点数组估计纯三维平移并返回拥有所有权的输出。
        /// </summary>
        public static bool EstimateTranslation3D(
            Point3f[] source,
            Point3f[] destination,
            out Mat translation,
            out Mat inliers,
            double ransacThreshold = 3.0,
            double confidence = 0.99)
        {
            ValidateMatchingTranslationArrays(
                source,
                destination,
                4);
            using (Mat sourceMat = ToPointMat(source))
            using (Mat destinationMat = ToPointMat(destination))
            {
                return EstimateTranslation3D(
                    sourceMat,
                    destinationMat,
                    out translation,
                    out inliers,
                    ransacThreshold,
                    confidence);
            }
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Estimates a pure 3D translation from point spans.
        /// 根据点 Span 估计纯三维平移。
        /// </summary>
        public static bool EstimateTranslation3D(
            ReadOnlySpan<Point3f> source,
            ReadOnlySpan<Point3f> destination,
            Mat translation,
            Mat? inliers = null,
            double ransacThreshold = 3.0,
            double confidence = 0.99)
        {
            ValidateMatchingTranslationSpans(
                source,
                destination,
                4);
            using (Mat sourceMat = ToPointMat(source))
            using (Mat destinationMat = ToPointMat(destination))
            {
                return EstimateTranslation3D(
                    sourceMat,
                    destinationMat,
                    translation,
                    inliers,
                    ransacThreshold,
                    confidence);
            }
        }

        /// <summary>
        /// Estimates a pure 3D translation from point spans and returns owned outputs.
        /// 根据点 Span 估计纯三维平移并返回拥有所有权的输出。
        /// </summary>
        public static bool EstimateTranslation3D(
            ReadOnlySpan<Point3f> source,
            ReadOnlySpan<Point3f> destination,
            out Mat translation,
            out Mat inliers,
            double ransacThreshold = 3.0,
            double confidence = 0.99)
        {
            ValidateMatchingTranslationSpans(
                source,
                destination,
                4);
            using (Mat sourceMat = ToPointMat(source))
            using (Mat destinationMat = ToPointMat(destination))
            {
                return EstimateTranslation3D(
                    sourceMat,
                    destinationMat,
                    out translation,
                    out inliers,
                    ransacThreshold,
                    confidence);
            }
        }
#endif

        /// <summary>
        /// Estimates a pure 2D translation with RANSAC or LMEDS.
        /// 使用 RANSAC 或 LMEDS 估计纯二维平移。
        /// </summary>
        /// <param name="source">The source 2D points. 源二维点。</param>
        /// <param name="destination">The destination 2D points. 目标二维点。</param>
        /// <param name="inliers">The optional caller-owned <c>N x 1 CV_8U</c> inlier mask. 可选的调用方持有 <c>N x 1 CV_8U</c> 内点掩码。</param>
        /// <param name="method">RANSAC or LMEDS. RANSAC 或 LMEDS。</param>
        /// <param name="ransacReprojThreshold">The positive RANSAC reprojection threshold. 正的 RANSAC 重投影阈值。</param>
        /// <param name="maxIters">The positive maximum iteration count. 正的最大迭代次数。</param>
        /// <param name="confidence">The confidence strictly between zero and one. 严格位于零和一之间的置信度。</param>
        /// <param name="refineIters">The non-negative refinement iteration count. 非负的细化迭代次数。</param>
        /// <returns>The double-precision translation, or NaN components if estimation fails. 双精度平移；估计失败时两个分量均为 NaN。</returns>
        public static Point2d EstimateTranslation2D(
            Mat source,
            Mat destination,
            Mat? inliers = null,
            RobustEstimationAlgorithms method = RobustEstimationAlgorithms.RANSAC,
            double ransacReprojThreshold = 3.0,
            int maxIters = 2000,
            double confidence = 0.99,
            int refineIters = 0)
        {
            ThrowIfNull(source, nameof(source));
            ThrowIfNull(destination, nameof(destination));
            int sourceCount = ValidateTranslationPointSet(
                source,
                2,
                1,
                nameof(source));
            int destinationCount = ValidateTranslationPointSet(
                destination,
                2,
                1,
                nameof(destination));
            ValidateMatchingTranslationPointCounts(
                sourceCount,
                destinationCount,
                nameof(destination));
            ValidateTranslationMethod(method, nameof(method));
            ValidateTranslationThreshold(
                ransacReprojThreshold,
                nameof(ransacReprojThreshold));
            ValidateTranslationIterations(
                maxIters,
                refineIters);
            ValidateTranslationConfidence(confidence, nameof(confidence));

            NativeException.ThrowIfError(NativeMethods.Calib3DEstimateTranslation2D(
                source.NativeHandle,
                destination.NativeHandle,
                GetNativeHandleOrZero(inliers),
                (int)method,
                ransacReprojThreshold,
                maxIters,
                confidence,
                refineIters,
                out double translationX,
                out double translationY));
            return new Point2d(translationX, translationY);
        }

        /// <summary>
        /// Estimates a pure 2D translation from managed point arrays.
        /// 根据托管点数组估计纯二维平移。
        /// </summary>
        public static Point2d EstimateTranslation2D(
            Point2f[] source,
            Point2f[] destination,
            Mat? inliers = null,
            RobustEstimationAlgorithms method = RobustEstimationAlgorithms.RANSAC,
            double ransacReprojThreshold = 3.0,
            int maxIters = 2000,
            double confidence = 0.99,
            int refineIters = 0)
        {
            ValidateMatchingTranslationArrays(
                source,
                destination,
                1);
            using (Mat sourceMat = ToPointMat(source))
            using (Mat destinationMat = ToPointMat(destination))
            {
                return EstimateTranslation2D(
                    sourceMat,
                    destinationMat,
                    inliers,
                    method,
                    ransacReprojThreshold,
                    maxIters,
                    confidence,
                    refineIters);
            }
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Estimates a pure 2D translation from point spans.
        /// 根据点 Span 估计纯二维平移。
        /// </summary>
        public static Point2d EstimateTranslation2D(
            ReadOnlySpan<Point2f> source,
            ReadOnlySpan<Point2f> destination,
            Mat? inliers = null,
            RobustEstimationAlgorithms method = RobustEstimationAlgorithms.RANSAC,
            double ransacReprojThreshold = 3.0,
            int maxIters = 2000,
            double confidence = 0.99,
            int refineIters = 0)
        {
            ValidateMatchingTranslationSpans(
                source,
                destination,
                1);
            using (Mat sourceMat = ToPointMat(source))
            using (Mat destinationMat = ToPointMat(destination))
            {
                return EstimateTranslation2D(
                    sourceMat,
                    destinationMat,
                    inliers,
                    method,
                    ransacReprojThreshold,
                    maxIters,
                    confidence,
                    refineIters);
            }
        }
#endif

        private static int ValidateTranslationPointSet(
            Mat points,
            int dimensions,
            int minimumPointCount,
            string parameterName)
        {
            if (points.Empty)
            {
                throw new ArgumentException("Point matrix cannot be empty.", parameterName);
            }
            if (points.Depth < MatType.CV_8U ||
                points.Depth > MatType.CV_16F)
            {
                throw new ArgumentException(
                    "Point matrix depth is not supported.",
                    parameterName);
            }
            if (!TryGetPointCount(points, dimensions, out int pointCount))
            {
                throw new ArgumentException(
                    $"Point matrix must contain {dimensions}-component points.",
                    parameterName);
            }
            if (pointCount < minimumPointCount)
            {
                throw new ArgumentException(
                    $"At least {minimumPointCount} point matches are required.",
                    parameterName);
            }

            return pointCount;
        }

        private static void ValidateMatchingTranslationPointCounts(
            int sourceCount,
            int destinationCount,
            string parameterName)
        {
            if (sourceCount != destinationCount)
            {
                throw new ArgumentException(
                    "Source and destination point counts must match.",
                    parameterName);
            }
        }

        private static void ValidateTranslationThreshold(
            double value,
            string parameterName)
        {
            ValidateFinite(value, parameterName);
            if (!(value > 0.0))
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    "Threshold must be positive.");
            }
        }

        private static void ValidateTranslationConfidence(
            double value,
            string parameterName)
        {
            ValidateFinite(value, parameterName);
            if (!(value > 0.0 && value < 1.0))
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    "Confidence must be strictly between zero and one.");
            }
        }

        private static void ValidateTranslationMethod(
            RobustEstimationAlgorithms method,
            string parameterName)
        {
            if (method != RobustEstimationAlgorithms.RANSAC &&
                method != RobustEstimationAlgorithms.LMEDS)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    "Translation estimation supports only RANSAC and LMEDS.");
            }
        }

        private static void ValidateTranslationIterations(
            int maxIters,
            int refineIters)
        {
            if (maxIters <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maxIters),
                    "Maximum iteration count must be positive.");
            }
            if (refineIters < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(refineIters),
                    "Refinement iteration count cannot be negative.");
            }
        }

        private static void ValidateMatchingTranslationArrays<T>(
            T[] source,
            T[] destination,
            int minimumPointCount)
        {
            ValidatePointArray(source, nameof(source));
            ValidatePointArray(destination, nameof(destination));
            if (source.Length != destination.Length)
            {
                throw new ArgumentException(
                    "Source and destination point counts must match.",
                    nameof(destination));
            }
            if (source.Length < minimumPointCount)
            {
                throw new ArgumentException(
                    $"At least {minimumPointCount} point matches are required.",
                    nameof(source));
            }
        }

#if NETCOREAPP3_1_OR_GREATER
        private static void ValidateMatchingTranslationSpans<T>(
            ReadOnlySpan<T> source,
            ReadOnlySpan<T> destination,
            int minimumPointCount)
        {
            ValidatePointSpan(source, nameof(source));
            ValidatePointSpan(destination, nameof(destination));
            if (source.Length != destination.Length)
            {
                throw new ArgumentException(
                    "Source and destination point counts must match.",
                    nameof(destination));
            }
            if (source.Length < minimumPointCount)
            {
                throw new ArgumentException(
                    $"At least {minimumPointCount} point matches are required.",
                    nameof(source));
            }
        }
#endif
    }
}
