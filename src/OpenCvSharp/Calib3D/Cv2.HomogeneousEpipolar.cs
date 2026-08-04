using System;
using System.Runtime.InteropServices;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Calib3D
{
    public static partial class Cv2
    {
        /// <summary>
        /// Converts Euclidean points to homogeneous coordinates by appending one.
        /// 通过追加一将欧氏点转换为齐次坐标。
        /// </summary>
        public static void ConvertPointsToHomogeneous(
            Mat source,
            Mat destination,
            int dtype = -1)
        {
            ThrowIfNull(source, nameof(source));
            ThrowIfNull(destination, nameof(destination));
            ValidateHomogeneousConversionSource(source, 2, 3, nameof(source));
            ValidateHomogeneousOutputDepth(dtype, nameof(dtype));

            NativeException.ThrowIfError(NativeMethods.Calib3DConvertPointsToHomogeneous(
                source.NativeHandle,
                destination.NativeHandle,
                dtype));
        }

        /// <summary>
        /// Converts Euclidean points to homogeneous coordinates and returns an owned matrix.
        /// 将欧氏点转换为齐次坐标并返回拥有所有权的矩阵。
        /// </summary>
        public static Mat ConvertPointsToHomogeneous(Mat source, int dtype = -1)
        {
            var destination = new Mat();
            try
            {
                ConvertPointsToHomogeneous(source, destination, dtype);
                return destination;
            }
            catch
            {
                destination.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Converts managed 2D points to homogeneous 3D points.
        /// 将托管二维点转换为齐次三维点。
        /// </summary>
        public static Mat ConvertPointsToHomogeneous(Point2f[] source, int dtype = -1)
        {
            ValidatePointArray(source, nameof(source));
            using (Mat sourceMat = ToPointMat(source))
            {
                return ConvertPointsToHomogeneous(sourceMat, dtype);
            }
        }

        /// <summary>
        /// Converts managed 3D points to homogeneous 4D points.
        /// 将托管三维点转换为齐次四维点。
        /// </summary>
        public static Mat ConvertPointsToHomogeneous(Point3f[] source, int dtype = -1)
        {
            ValidatePointArray(source, nameof(source));
            using (Mat sourceMat = ToPointMat(source))
            {
                return ConvertPointsToHomogeneous(sourceMat, dtype);
            }
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Converts a span of 2D points to homogeneous 3D points.
        /// 将二维点 Span 转换为齐次三维点。
        /// </summary>
        public static Mat ConvertPointsToHomogeneous(
            ReadOnlySpan<Point2f> source,
            int dtype = -1)
        {
            ValidatePointSpan(source, nameof(source));
            using (Mat sourceMat = ToPointMat(source))
            {
                return ConvertPointsToHomogeneous(sourceMat, dtype);
            }
        }

        /// <summary>
        /// Converts a span of 3D points to homogeneous 4D points.
        /// 将三维点 Span 转换为齐次四维点。
        /// </summary>
        public static Mat ConvertPointsToHomogeneous(
            ReadOnlySpan<Point3f> source,
            int dtype = -1)
        {
            ValidatePointSpan(source, nameof(source));
            using (Mat sourceMat = ToPointMat(source))
            {
                return ConvertPointsToHomogeneous(sourceMat, dtype);
            }
        }
#endif

        /// <summary>
        /// Converts homogeneous points to Euclidean coordinates by perspective division.
        /// 通过透视除法将齐次点转换为欧氏坐标。
        /// </summary>
        public static void ConvertPointsFromHomogeneous(
            Mat source,
            Mat destination,
            int dtype = -1)
        {
            ThrowIfNull(source, nameof(source));
            ThrowIfNull(destination, nameof(destination));
            ValidateHomogeneousConversionSource(source, 3, 4, nameof(source));
            ValidateHomogeneousOutputDepth(dtype, nameof(dtype));

            NativeException.ThrowIfError(NativeMethods.Calib3DConvertPointsFromHomogeneous(
                source.NativeHandle,
                destination.NativeHandle,
                dtype));
        }

        /// <summary>
        /// Converts homogeneous points to Euclidean coordinates and returns an owned matrix.
        /// 将齐次点转换为欧氏坐标并返回拥有所有权的矩阵。
        /// </summary>
        public static Mat ConvertPointsFromHomogeneous(Mat source, int dtype = -1)
        {
            var destination = new Mat();
            try
            {
                ConvertPointsFromHomogeneous(source, destination, dtype);
                return destination;
            }
            catch
            {
                destination.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Converts managed homogeneous 2D points to Euclidean 2D points.
        /// 将托管齐次二维点转换为欧氏二维点。
        /// </summary>
        public static Mat ConvertPointsFromHomogeneous(Point3f[] source, int dtype = -1)
        {
            ValidatePointArray(source, nameof(source));
            using (Mat sourceMat = ToPointMat(source))
            {
                return ConvertPointsFromHomogeneous(sourceMat, dtype);
            }
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Converts a span of homogeneous 2D points to Euclidean 2D points.
        /// 将齐次二维点 Span 转换为欧氏二维点。
        /// </summary>
        public static Mat ConvertPointsFromHomogeneous(
            ReadOnlySpan<Point3f> source,
            int dtype = -1)
        {
            ValidatePointSpan(source, nameof(source));
            using (Mat sourceMat = ToPointMat(source))
            {
                return ConvertPointsFromHomogeneous(sourceMat, dtype);
            }
        }
#endif

        /// <summary>
        /// Corrects matching image points to satisfy the epipolar constraint.
        /// 校正匹配像点，使其满足极线约束。
        /// </summary>
        public static void CorrectMatches(
            Mat fundamentalMatrix,
            Mat points1,
            Mat points2,
            Mat correctedPoints1,
            Mat correctedPoints2)
        {
            ThrowIfNull(fundamentalMatrix, nameof(fundamentalMatrix));
            ThrowIfNull(points1, nameof(points1));
            ThrowIfNull(points2, nameof(points2));
            ThrowIfNull(correctedPoints1, nameof(correctedPoints1));
            ThrowIfNull(correctedPoints2, nameof(correctedPoints2));
            ValidateFundamentalMatrix(fundamentalMatrix, nameof(fundamentalMatrix), false);
            ValidateCorrectMatchesPoints(points1, nameof(points1));
            ValidateCorrectMatchesPoints(points2, nameof(points2));
            if (points1.Rows != points2.Rows ||
                points1.Cols != points2.Cols ||
                points1.Type != points2.Type)
            {
                throw new ArgumentException(
                    "Point sets must have matching size and type.",
                    nameof(points2));
            }

            NativeException.ThrowIfError(NativeMethods.Calib3DCorrectMatches(
                fundamentalMatrix.NativeHandle,
                points1.NativeHandle,
                points2.NativeHandle,
                correctedPoints1.NativeHandle,
                correctedPoints2.NativeHandle));
        }

        /// <summary>
        /// Corrects matching image points and returns two owned matrices.
        /// 校正匹配像点并返回两个拥有所有权的矩阵。
        /// </summary>
        public static void CorrectMatches(
            Mat fundamentalMatrix,
            Mat points1,
            Mat points2,
            out Mat correctedPoints1,
            out Mat correctedPoints2)
        {
            correctedPoints1 = new Mat();
            correctedPoints2 = new Mat();
            try
            {
                CorrectMatches(
                    fundamentalMatrix,
                    points1,
                    points2,
                    correctedPoints1,
                    correctedPoints2);
            }
            catch
            {
                correctedPoints1.Dispose();
                correctedPoints2.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Corrects managed matching image points and returns owned matrices.
        /// 校正托管匹配像点并返回拥有所有权的矩阵。
        /// </summary>
        public static void CorrectMatches(
            Mat fundamentalMatrix,
            Point2f[] points1,
            Point2f[] points2,
            out Mat correctedPoints1,
            out Mat correctedPoints2)
        {
            ValidateMatchingPoint2fArrays(points1, points2);
            using (Mat points1Mat = ToPointMat(points1))
            using (Mat points2Mat = ToPointMat(points2))
            {
                CorrectMatches(
                    fundamentalMatrix,
                    points1Mat,
                    points2Mat,
                    out correctedPoints1,
                    out correctedPoints2);
            }
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Corrects matching image point spans and returns owned matrices.
        /// 校正匹配像点 Span 并返回拥有所有权的矩阵。
        /// </summary>
        public static void CorrectMatches(
            Mat fundamentalMatrix,
            ReadOnlySpan<Point2f> points1,
            ReadOnlySpan<Point2f> points2,
            out Mat correctedPoints1,
            out Mat correctedPoints2)
        {
            ValidateMatchingPoint2fSpans(points1, points2);
            using (Mat points1Mat = ToPointMat(points1))
            using (Mat points2Mat = ToPointMat(points2))
            {
                CorrectMatches(
                    fundamentalMatrix,
                    points1Mat,
                    points2Mat,
                    out correctedPoints1,
                    out correctedPoints2);
            }
        }
#endif

        /// <summary>
        /// Calculates the Sampson distance between two homogeneous image points.
        /// 计算两个齐次像点之间的 Sampson 距离。
        /// </summary>
        public static double SampsonDistance(
            Mat homogeneousPoint1,
            Mat homogeneousPoint2,
            Mat fundamentalMatrix)
        {
            ThrowIfNull(homogeneousPoint1, nameof(homogeneousPoint1));
            ThrowIfNull(homogeneousPoint2, nameof(homogeneousPoint2));
            ThrowIfNull(fundamentalMatrix, nameof(fundamentalMatrix));
            ValidateSampsonPoint(homogeneousPoint1, nameof(homogeneousPoint1));
            ValidateSampsonPoint(homogeneousPoint2, nameof(homogeneousPoint2));
            ValidateFundamentalMatrix(fundamentalMatrix, nameof(fundamentalMatrix), true);

            NativeException.ThrowIfError(NativeMethods.Calib3DSampsonDistance(
                homogeneousPoint1.NativeHandle,
                homogeneousPoint2.NativeHandle,
                fundamentalMatrix.NativeHandle,
                out double distance));
            return distance;
        }

        /// <summary>
        /// Calculates the Sampson distance between two Euclidean double-precision image points.
        /// 计算两个双精度欧氏像点之间的 Sampson 距离。
        /// </summary>
        public static double SampsonDistance(
            Point2d point1,
            Point2d point2,
            Mat fundamentalMatrix)
        {
            using (Mat homogeneousPoint1 = CreateHomogeneousPoint(point1))
            using (Mat homogeneousPoint2 = CreateHomogeneousPoint(point2))
            {
                return SampsonDistance(
                    homogeneousPoint1,
                    homogeneousPoint2,
                    fundamentalMatrix);
            }
        }

        private static void ValidateHomogeneousConversionSource(
            Mat source,
            int minimumDimensions,
            int maximumDimensions,
            string parameterName)
        {
            if (source.Empty)
            {
                throw new ArgumentException("Point matrix cannot be empty.", parameterName);
            }
            if (source.Depth != MatType.CV_32S &&
                source.Depth != MatType.CV_32F &&
                source.Depth != MatType.CV_64F)
            {
                throw new ArgumentException(
                    "Point matrix depth must be CV_32S, CV_32F, or CV_64F.",
                    parameterName);
            }

            for (int dimensions = minimumDimensions; dimensions <= maximumDimensions; ++dimensions)
            {
                if (TryGetPointCount(source, dimensions, out _))
                {
                    return;
                }
            }
            throw new ArgumentException(
                $"Point matrix must contain {minimumDimensions}- or {maximumDimensions}-component points.",
                parameterName);
        }

        private static void ValidateHomogeneousOutputDepth(int dtype, string parameterName)
        {
            if (dtype != -1 && dtype != MatType.CV_32F && dtype != MatType.CV_64F)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    "Output depth must be -1, CV_32F, or CV_64F.");
            }
        }

        private static bool TryGetPointCount(Mat points, int dimensions, out int pointCount)
        {
            if (points.Channels == dimensions && (points.Rows == 1 || points.Cols == 1))
            {
                pointCount = checked(points.Rows * points.Cols);
                return pointCount > 0;
            }
            if (points.Channels == 1 && points.Cols == dimensions)
            {
                pointCount = points.Rows;
                return pointCount > 0;
            }

            pointCount = 0;
            return false;
        }

        private static void ValidateCorrectMatchesPoints(Mat points, string parameterName)
        {
            if (points.Empty ||
                points.Channels != 2 ||
                (points.Rows != 1 && points.Cols != 1) ||
                (points.Depth != MatType.CV_32F && points.Depth != MatType.CV_64F))
            {
                throw new ArgumentException(
                    "CorrectMatches points must be a non-empty row or column vector of two-channel CV_32F or CV_64F points.",
                    parameterName);
            }
        }

        private static void ValidateFundamentalMatrix(
            Mat fundamentalMatrix,
            string parameterName,
            bool requireDouble)
        {
            bool validDepth = requireDouble
                ? fundamentalMatrix.Depth == MatType.CV_64F
                : fundamentalMatrix.Depth == MatType.CV_32F ||
                  fundamentalMatrix.Depth == MatType.CV_64F;
            if (fundamentalMatrix.Rows != 3 ||
                fundamentalMatrix.Cols != 3 ||
                fundamentalMatrix.Channels != 1 ||
                !validDepth)
            {
                string depth = requireDouble ? "CV_64F" : "CV_32F or CV_64F";
                throw new ArgumentException(
                    $"Fundamental matrix must be 3 x 3 single-channel {depth}.",
                    parameterName);
            }
        }

        private static void ValidateSampsonPoint(Mat point, string parameterName)
        {
            if (point.Rows != 3 ||
                point.Cols != 1 ||
                point.Channels != 1 ||
                point.Depth != MatType.CV_64F)
            {
                throw new ArgumentException(
                    "Sampson points must be 3 x 1 single-channel CV_64F homogeneous vectors.",
                    parameterName);
            }
        }

        private static void ValidateMatchingPoint2fArrays(Point2f[] points1, Point2f[] points2)
        {
            ValidatePointArray(points1, nameof(points1));
            ValidatePointArray(points2, nameof(points2));
            if (points1.Length != points2.Length)
            {
                throw new ArgumentException(
                    "Point counts must match.",
                    nameof(points2));
            }
        }

#if NETCOREAPP3_1_OR_GREATER
        private static void ValidateMatchingPoint2fSpans(
            ReadOnlySpan<Point2f> points1,
            ReadOnlySpan<Point2f> points2)
        {
            ValidatePointSpan(points1, nameof(points1));
            ValidatePointSpan(points2, nameof(points2));
            if (points1.Length != points2.Length)
            {
                throw new ArgumentException(
                    "Point counts must match.",
                    nameof(points2));
            }
        }
#endif

        private static Mat CreateHomogeneousPoint(Point2d point)
        {
            var result = new Mat(3, 1, MatType.CV_64FC1);
            try
            {
                Marshal.Copy(
                    new[] { point.X, point.Y, 1.0 },
                    0,
                    result.Data,
                    3);
                return result;
            }
            catch
            {
                result.Dispose();
                throw;
            }
        }
    }
}
