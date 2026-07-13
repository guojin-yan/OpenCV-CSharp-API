using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Calib3D
{
    public static partial class Cv2
    {
        /// <summary>
        /// Estimates a robust 3D affine transform with RANSAC.
        /// 使用 RANSAC 估计鲁棒三维仿射变换。
        /// </summary>
        public static bool EstimateAffine3D(
            Mat source,
            Mat destination,
            Mat transform,
            Mat? inliers = null,
            double ransacThreshold = 3.0,
            double confidence = 0.99)
        {
            ThrowIfNull(source, nameof(source));
            ThrowIfNull(destination, nameof(destination));
            ThrowIfNull(transform, nameof(transform));
            int sourceCount = ValidateTranslationPointSet(source, 3, 4, nameof(source));
            int destinationCount = ValidateTranslationPointSet(destination, 3, 4, nameof(destination));
            ValidateMatchingTranslationPointCounts(sourceCount, destinationCount, nameof(destination));
            ValidateTranslationThreshold(ransacThreshold, nameof(ransacThreshold));
            ValidateTranslationConfidence(confidence, nameof(confidence));
            ValidateAffineOutputs(source, destination, transform, inliers);

            NativeException.ThrowIfError(NativeMethods.Calib3DEstimateAffine3DRansac(
                source.NativeHandle,
                destination.NativeHandle,
                transform.NativeHandle,
                GetNativeHandleOrZero(inliers),
                ransacThreshold,
                confidence,
                out int found));
            return found != 0;
        }

        /// <summary>
        /// Estimates a robust 3D affine transform and returns owned outputs.
        /// 估计鲁棒三维仿射变换并返回拥有所有权的输出。
        /// </summary>
        public static bool EstimateAffine3D(
            Mat source,
            Mat destination,
            out Mat transform,
            out Mat inliers,
            double ransacThreshold = 3.0,
            double confidence = 0.99)
        {
            transform = new Mat();
            inliers = new Mat();
            try
            {
                return EstimateAffine3D(
                    source,
                    destination,
                    transform,
                    inliers,
                    ransacThreshold,
                    confidence);
            }
            catch
            {
                transform.Dispose();
                inliers.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Estimates a robust 3D affine transform from managed point arrays.
        /// 根据托管点数组估计鲁棒三维仿射变换。
        /// </summary>
        public static bool EstimateAffine3D(
            Point3f[] source,
            Point3f[] destination,
            Mat transform,
            Mat? inliers = null,
            double ransacThreshold = 3.0,
            double confidence = 0.99)
        {
            ValidateMatchingTranslationArrays(source, destination, 4);
            using (Mat sourceMat = ToPointMat(source))
            using (Mat destinationMat = ToPointMat(destination))
            {
                return EstimateAffine3D(
                    sourceMat,
                    destinationMat,
                    transform,
                    inliers,
                    ransacThreshold,
                    confidence);
            }
        }

        /// <summary>
        /// Estimates a robust 3D affine transform from managed arrays and returns owned outputs.
        /// 根据托管点数组估计鲁棒三维仿射变换并返回拥有所有权的输出。
        /// </summary>
        public static bool EstimateAffine3D(
            Point3f[] source,
            Point3f[] destination,
            out Mat transform,
            out Mat inliers,
            double ransacThreshold = 3.0,
            double confidence = 0.99)
        {
            ValidateMatchingTranslationArrays(source, destination, 4);
            using (Mat sourceMat = ToPointMat(source))
            using (Mat destinationMat = ToPointMat(destination))
            {
                return EstimateAffine3D(
                    sourceMat,
                    destinationMat,
                    out transform,
                    out inliers,
                    ransacThreshold,
                    confidence);
            }
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Estimates a robust 3D affine transform from point spans.
        /// 根据点 Span 估计鲁棒三维仿射变换。
        /// </summary>
        public static bool EstimateAffine3D(
            ReadOnlySpan<Point3f> source,
            ReadOnlySpan<Point3f> destination,
            Mat transform,
            Mat? inliers = null,
            double ransacThreshold = 3.0,
            double confidence = 0.99)
        {
            ValidateMatchingTranslationSpans(source, destination, 4);
            using (Mat sourceMat = ToPointMat(source))
            using (Mat destinationMat = ToPointMat(destination))
            {
                return EstimateAffine3D(
                    sourceMat,
                    destinationMat,
                    transform,
                    inliers,
                    ransacThreshold,
                    confidence);
            }
        }

        /// <summary>
        /// Estimates a robust 3D affine transform from spans and returns owned outputs.
        /// 根据点 Span 估计鲁棒三维仿射变换并返回拥有所有权的输出。
        /// </summary>
        public static bool EstimateAffine3D(
            ReadOnlySpan<Point3f> source,
            ReadOnlySpan<Point3f> destination,
            out Mat transform,
            out Mat inliers,
            double ransacThreshold = 3.0,
            double confidence = 0.99)
        {
            ValidateMatchingTranslationSpans(source, destination, 4);
            using (Mat sourceMat = ToPointMat(source))
            using (Mat destinationMat = ToPointMat(destination))
            {
                return EstimateAffine3D(
                    sourceMat,
                    destinationMat,
                    out transform,
                    out inliers,
                    ransacThreshold,
                    confidence);
            }
        }
#endif

        /// <summary>
        /// Estimates a 3D similarity transform with the Umeyama algorithm.
        /// 使用 Umeyama 算法估计三维相似变换。
        /// </summary>
        public static Mat EstimateAffine3D(
            Mat source,
            Mat destination,
            out double scale,
            bool forceRotation = true)
        {
            ThrowIfNull(source, nameof(source));
            ThrowIfNull(destination, nameof(destination));
            int sourceCount = ValidateTranslationPointSet(source, 3, 3, nameof(source));
            int destinationCount = ValidateTranslationPointSet(destination, 3, 3, nameof(destination));
            ValidateMatchingTranslationPointCounts(sourceCount, destinationCount, nameof(destination));

            var transform = new Mat();
            try
            {
                NativeException.ThrowIfError(NativeMethods.Calib3DEstimateAffine3DUmeyama(
                    source.NativeHandle,
                    destination.NativeHandle,
                    transform.NativeHandle,
                    forceRotation ? 1 : 0,
                    out scale));
                return transform;
            }
            catch
            {
                transform.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Estimates a 3D similarity transform from managed point arrays.
        /// 根据托管点数组估计三维相似变换。
        /// </summary>
        public static Mat EstimateAffine3D(
            Point3f[] source,
            Point3f[] destination,
            out double scale,
            bool forceRotation = true)
        {
            ValidateMatchingTranslationArrays(source, destination, 3);
            using (Mat sourceMat = ToPointMat(source))
            using (Mat destinationMat = ToPointMat(destination))
            {
                return EstimateAffine3D(sourceMat, destinationMat, out scale, forceRotation);
            }
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Estimates a 3D similarity transform from point spans.
        /// 根据点 Span 估计三维相似变换。
        /// </summary>
        public static Mat EstimateAffine3D(
            ReadOnlySpan<Point3f> source,
            ReadOnlySpan<Point3f> destination,
            out double scale,
            bool forceRotation = true)
        {
            ValidateMatchingTranslationSpans(source, destination, 3);
            using (Mat sourceMat = ToPointMat(source))
            using (Mat destinationMat = ToPointMat(destination))
            {
                return EstimateAffine3D(sourceMat, destinationMat, out scale, forceRotation);
            }
        }
#endif

        /// <summary>
        /// Estimates a full 2D affine transform with RANSAC or LMEDS.
        /// 使用 RANSAC 或 LMEDS 估计完整二维仿射变换。
        /// </summary>
        public static Mat EstimateAffine2D(
            Mat source,
            Mat destination,
            Mat? inliers = null,
            RobustEstimationAlgorithms method = RobustEstimationAlgorithms.RANSAC,
            double ransacReprojThreshold = 3.0,
            int maxIters = 2000,
            double confidence = 0.99,
            int refineIters = 10)
        {
            ValidateAffine2DInputs(
                source,
                destination,
                inliers,
                3,
                method,
                ransacReprojThreshold,
                maxIters,
                confidence,
                refineIters);

            var transform = new Mat();
            try
            {
                NativeException.ThrowIfError(NativeMethods.Calib3DEstimateAffine2D(
                    source.NativeHandle,
                    destination.NativeHandle,
                    transform.NativeHandle,
                    GetNativeHandleOrZero(inliers),
                    (int)method,
                    ransacReprojThreshold,
                    maxIters,
                    confidence,
                    refineIters));
                return transform;
            }
            catch
            {
                transform.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Estimates a full 2D affine transform from managed point arrays.
        /// 根据托管点数组估计完整二维仿射变换。
        /// </summary>
        public static Mat EstimateAffine2D(
            Point2f[] source,
            Point2f[] destination,
            Mat? inliers = null,
            RobustEstimationAlgorithms method = RobustEstimationAlgorithms.RANSAC,
            double ransacReprojThreshold = 3.0,
            int maxIters = 2000,
            double confidence = 0.99,
            int refineIters = 10)
        {
            ValidateMatchingTranslationArrays(source, destination, 3);
            using (Mat sourceMat = ToPointMat(source))
            using (Mat destinationMat = ToPointMat(destination))
            {
                return EstimateAffine2D(
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
        /// Estimates a full 2D affine transform from point spans.
        /// 根据点 Span 估计完整二维仿射变换。
        /// </summary>
        public static Mat EstimateAffine2D(
            ReadOnlySpan<Point2f> source,
            ReadOnlySpan<Point2f> destination,
            Mat? inliers = null,
            RobustEstimationAlgorithms method = RobustEstimationAlgorithms.RANSAC,
            double ransacReprojThreshold = 3.0,
            int maxIters = 2000,
            double confidence = 0.99,
            int refineIters = 10)
        {
            ValidateMatchingTranslationSpans(source, destination, 3);
            using (Mat sourceMat = ToPointMat(source))
            using (Mat destinationMat = ToPointMat(destination))
            {
                return EstimateAffine2D(
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

        /// <summary>
        /// Estimates a partial 2D affine transform with uniform scale and rotation.
        /// 估计包含统一缩放与旋转的部分二维仿射变换。
        /// </summary>
        public static Mat EstimateAffinePartial2D(
            Mat source,
            Mat destination,
            Mat? inliers = null,
            RobustEstimationAlgorithms method = RobustEstimationAlgorithms.RANSAC,
            double ransacReprojThreshold = 3.0,
            int maxIters = 2000,
            double confidence = 0.99,
            int refineIters = 10)
        {
            ValidateAffine2DInputs(
                source,
                destination,
                inliers,
                2,
                method,
                ransacReprojThreshold,
                maxIters,
                confidence,
                refineIters);

            var transform = new Mat();
            try
            {
                NativeException.ThrowIfError(NativeMethods.Calib3DEstimateAffinePartial2D(
                    source.NativeHandle,
                    destination.NativeHandle,
                    transform.NativeHandle,
                    GetNativeHandleOrZero(inliers),
                    (int)method,
                    ransacReprojThreshold,
                    maxIters,
                    confidence,
                    refineIters));
                return transform;
            }
            catch
            {
                transform.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Estimates a partial 2D affine transform from managed point arrays.
        /// 根据托管点数组估计部分二维仿射变换。
        /// </summary>
        public static Mat EstimateAffinePartial2D(
            Point2f[] source,
            Point2f[] destination,
            Mat? inliers = null,
            RobustEstimationAlgorithms method = RobustEstimationAlgorithms.RANSAC,
            double ransacReprojThreshold = 3.0,
            int maxIters = 2000,
            double confidence = 0.99,
            int refineIters = 10)
        {
            ValidateMatchingTranslationArrays(source, destination, 2);
            using (Mat sourceMat = ToPointMat(source))
            using (Mat destinationMat = ToPointMat(destination))
            {
                return EstimateAffinePartial2D(
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
        /// Estimates a partial 2D affine transform from point spans.
        /// 根据点 Span 估计部分二维仿射变换。
        /// </summary>
        public static Mat EstimateAffinePartial2D(
            ReadOnlySpan<Point2f> source,
            ReadOnlySpan<Point2f> destination,
            Mat? inliers = null,
            RobustEstimationAlgorithms method = RobustEstimationAlgorithms.RANSAC,
            double ransacReprojThreshold = 3.0,
            int maxIters = 2000,
            double confidence = 0.99,
            int refineIters = 10)
        {
            ValidateMatchingTranslationSpans(source, destination, 2);
            using (Mat sourceMat = ToPointMat(source))
            using (Mat destinationMat = ToPointMat(destination))
            {
                return EstimateAffinePartial2D(
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

        private static void ValidateAffine2DInputs(
            Mat source,
            Mat destination,
            Mat? inliers,
            int minimumPointCount,
            RobustEstimationAlgorithms method,
            double ransacReprojThreshold,
            int maxIters,
            double confidence,
            int refineIters)
        {
            ThrowIfNull(source, nameof(source));
            ThrowIfNull(destination, nameof(destination));
            int sourceCount = ValidateTranslationPointSet(
                source,
                2,
                minimumPointCount,
                nameof(source));
            int destinationCount = ValidateTranslationPointSet(
                destination,
                2,
                minimumPointCount,
                nameof(destination));
            ValidateMatchingTranslationPointCounts(
                sourceCount,
                destinationCount,
                nameof(destination));
            ValidateAffineMethod(method, nameof(method));
            ValidateTranslationThreshold(
                ransacReprojThreshold,
                nameof(ransacReprojThreshold));
            ValidateTranslationIterations(maxIters, refineIters);
            ValidateTranslationConfidence(confidence, nameof(confidence));

            if (inliers != null)
            {
                IntPtr inliersHandle = inliers.NativeHandle;
                if (ReferenceEquals(source, inliers) || source.NativeHandle == inliersHandle)
                {
                    throw new ArgumentException(
                        "The source points and inlier mask must not alias.",
                        nameof(inliers));
                }
                if (ReferenceEquals(destination, inliers) || destination.NativeHandle == inliersHandle)
                {
                    throw new ArgumentException(
                        "The destination points and inlier mask must not alias.",
                        nameof(inliers));
                }
            }
        }

        private static void ValidateAffineOutputs(
            Mat source,
            Mat destination,
            Mat transform,
            Mat? inliers)
        {
            IntPtr sourceHandle = source.NativeHandle;
            IntPtr destinationHandle = destination.NativeHandle;
            IntPtr transformHandle = transform.NativeHandle;
            if (ReferenceEquals(source, transform) || sourceHandle == transformHandle)
            {
                throw new ArgumentException(
                    "The source points and transform output must not alias.",
                    nameof(transform));
            }
            if (ReferenceEquals(destination, transform) || destinationHandle == transformHandle)
            {
                throw new ArgumentException(
                    "The destination points and transform output must not alias.",
                    nameof(transform));
            }

            if (inliers != null)
            {
                IntPtr inliersHandle = inliers.NativeHandle;
                if (ReferenceEquals(source, inliers) || sourceHandle == inliersHandle)
                {
                    throw new ArgumentException(
                        "The source points and inlier mask must not alias.",
                        nameof(inliers));
                }
                if (ReferenceEquals(destination, inliers) || destinationHandle == inliersHandle)
                {
                    throw new ArgumentException(
                        "The destination points and inlier mask must not alias.",
                        nameof(inliers));
                }
                if (ReferenceEquals(transform, inliers) || transformHandle == inliersHandle)
                {
                    throw new ArgumentException(
                        "The transform output and inlier mask must not alias.",
                        nameof(inliers));
                }
            }
        }

        private static void ValidateAffineMethod(
            RobustEstimationAlgorithms method,
            string parameterName)
        {
            if (method != RobustEstimationAlgorithms.RANSAC &&
                method != RobustEstimationAlgorithms.LMEDS)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    "Affine estimation supports only RANSAC and LMEDS.");
            }
        }
    }
}
