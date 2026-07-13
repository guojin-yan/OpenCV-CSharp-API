using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Calib3D
{
    public static partial class Cv2
    {
        /// <summary>
        /// Finds all P3P pose solutions from exactly three or four 3D-2D correspondences.
        /// 根据正好三组或四组 3D-2D 对应点查找全部 P3P 位姿解。
        /// </summary>
        /// <param name="objectPoints">The input 3D object points. 输入三维物点。</param>
        /// <param name="imagePoints">The input 2D image points. 输入二维像点。</param>
        /// <param name="cameraMatrix">The 3 x 3 camera intrinsic matrix. 3 x 3 相机内参矩阵。</param>
        /// <param name="distCoeffs">The distortion coefficients, or an empty matrix for zero distortion. 畸变系数，零畸变时可传入空矩阵。</param>
        /// <param name="rvecs">The output N x 3 CV_64FC1 rotation vectors. 输出 N x 3 CV_64FC1 旋转向量。</param>
        /// <param name="tvecs">The output N x 3 CV_64FC1 translation vectors. 输出 N x 3 CV_64FC1 平移向量。</param>
        /// <param name="flags">The P3P or AP3P method. P3P 或 AP3P 方法。</param>
        /// <returns>The number of pose solutions, from zero through four. 位姿解数量，范围为零到四。</returns>
        public static int SolveP3P(
            Mat objectPoints,
            Mat imagePoints,
            Mat cameraMatrix,
            Mat distCoeffs,
            Mat rvecs,
            Mat tvecs,
            SolvePnPFlags flags = SolvePnPFlags.P3P)
        {
            ThrowIfNull(objectPoints, nameof(objectPoints));
            ThrowIfNull(imagePoints, nameof(imagePoints));
            ThrowIfNull(cameraMatrix, nameof(cameraMatrix));
            ThrowIfNull(distCoeffs, nameof(distCoeffs));
            ThrowIfNull(rvecs, nameof(rvecs));
            ThrowIfNull(tvecs, nameof(tvecs));

            int objectPointCount = ValidateSolveP3PPointMat(objectPoints, 3, nameof(objectPoints));
            int imagePointCount = ValidateSolveP3PPointMat(imagePoints, 2, nameof(imagePoints));
            if (objectPointCount != imagePointCount)
            {
                throw new ArgumentException(
                    "Object and image point counts must match.",
                    nameof(imagePoints));
            }
            ValidateSolveP3PPointCount(objectPointCount, nameof(objectPoints));
            ValidateCameraUtilityMatrix(cameraMatrix, nameof(cameraMatrix));
            ValidatePinholeDistortionCoefficients(distCoeffs, nameof(distCoeffs));
            ValidateSolveP3PFlags(flags, nameof(flags));
            ValidateSolveP3POutputs(
                objectPoints,
                imagePoints,
                cameraMatrix,
                distCoeffs,
                rvecs,
                tvecs);

            NativeException.ThrowIfError(NativeMethods.Calib3DSolveP3P(
                objectPoints.NativeHandle,
                imagePoints.NativeHandle,
                cameraMatrix.NativeHandle,
                distCoeffs.NativeHandle,
                rvecs.NativeHandle,
                tvecs.NativeHandle,
                (int)flags,
                out int solutionCount));
            return solutionCount;
        }

        /// <summary>
        /// Finds all P3P pose solutions and returns owned output matrices.
        /// 查找全部 P3P 位姿解并返回拥有所有权的输出矩阵。
        /// </summary>
        public static SolvePnPGenericResult SolveP3P(
            Mat objectPoints,
            Mat imagePoints,
            Mat cameraMatrix,
            Mat distCoeffs,
            SolvePnPFlags flags = SolvePnPFlags.P3P)
        {
            var rvecs = new Mat();
            var tvecs = new Mat();
            try
            {
                int solutionCount = SolveP3P(
                    objectPoints,
                    imagePoints,
                    cameraMatrix,
                    distCoeffs,
                    rvecs,
                    tvecs,
                    flags);
                return new SolvePnPGenericResult(solutionCount, rvecs, tvecs, null);
            }
            catch
            {
                rvecs.Dispose();
                tvecs.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Finds all P3P pose solutions from managed point arrays.
        /// 根据托管点数组查找全部 P3P 位姿解。
        /// </summary>
        public static int SolveP3P(
            Point3f[] objectPoints,
            Point2f[] imagePoints,
            Mat cameraMatrix,
            Mat distCoeffs,
            Mat rvecs,
            Mat tvecs,
            SolvePnPFlags flags = SolvePnPFlags.P3P)
        {
            ValidateMatchingPointArrays(objectPoints, imagePoints);
            ValidateSolveP3PPointCount(objectPoints.Length, nameof(objectPoints));
            using (Mat objectPointMat = ToPointMat(objectPoints))
            using (Mat imagePointMat = ToPointMat(imagePoints))
            {
                return SolveP3P(
                    objectPointMat,
                    imagePointMat,
                    cameraMatrix,
                    distCoeffs,
                    rvecs,
                    tvecs,
                    flags);
            }
        }

        /// <summary>
        /// Finds all P3P pose solutions from managed point arrays and returns owned output matrices.
        /// 根据托管点数组查找全部 P3P 位姿解并返回拥有所有权的输出矩阵。
        /// </summary>
        public static SolvePnPGenericResult SolveP3P(
            Point3f[] objectPoints,
            Point2f[] imagePoints,
            Mat cameraMatrix,
            Mat distCoeffs,
            SolvePnPFlags flags = SolvePnPFlags.P3P)
        {
            ValidateMatchingPointArrays(objectPoints, imagePoints);
            ValidateSolveP3PPointCount(objectPoints.Length, nameof(objectPoints));
            using (Mat objectPointMat = ToPointMat(objectPoints))
            using (Mat imagePointMat = ToPointMat(imagePoints))
            {
                return SolveP3P(
                    objectPointMat,
                    imagePointMat,
                    cameraMatrix,
                    distCoeffs,
                    flags);
            }
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Finds all P3P pose solutions from point spans.
        /// 根据点 Span 查找全部 P3P 位姿解。
        /// </summary>
        public static int SolveP3P(
            ReadOnlySpan<Point3f> objectPoints,
            ReadOnlySpan<Point2f> imagePoints,
            Mat cameraMatrix,
            Mat distCoeffs,
            Mat rvecs,
            Mat tvecs,
            SolvePnPFlags flags = SolvePnPFlags.P3P)
        {
            ValidateMatchingPointSpans(objectPoints, imagePoints);
            ValidateSolveP3PPointCount(objectPoints.Length, nameof(objectPoints));
            using (Mat objectPointMat = ToPointMat(objectPoints))
            using (Mat imagePointMat = ToPointMat(imagePoints))
            {
                return SolveP3P(
                    objectPointMat,
                    imagePointMat,
                    cameraMatrix,
                    distCoeffs,
                    rvecs,
                    tvecs,
                    flags);
            }
        }

        /// <summary>
        /// Finds all P3P pose solutions from point spans and returns owned output matrices.
        /// 根据点 Span 查找全部 P3P 位姿解并返回拥有所有权的输出矩阵。
        /// </summary>
        public static SolvePnPGenericResult SolveP3P(
            ReadOnlySpan<Point3f> objectPoints,
            ReadOnlySpan<Point2f> imagePoints,
            Mat cameraMatrix,
            Mat distCoeffs,
            SolvePnPFlags flags = SolvePnPFlags.P3P)
        {
            ValidateMatchingPointSpans(objectPoints, imagePoints);
            ValidateSolveP3PPointCount(objectPoints.Length, nameof(objectPoints));
            using (Mat objectPointMat = ToPointMat(objectPoints))
            using (Mat imagePointMat = ToPointMat(imagePoints))
            {
                return SolveP3P(
                    objectPointMat,
                    imagePointMat,
                    cameraMatrix,
                    distCoeffs,
                    flags);
            }
        }
#endif

        private static int ValidateSolveP3PPointMat(
            Mat points,
            int dimensions,
            string parameterName)
        {
            if (points.Empty)
            {
                throw new ArgumentException("Point matrix cannot be empty.", parameterName);
            }
            ValidateCameraUtilityFloatingDepth(points, parameterName);
            if (!TryGetPointCount(points, dimensions, out int pointCount))
            {
                throw new ArgumentException(
                    $"Point matrix must contain {dimensions}-component points.",
                    parameterName);
            }

            return pointCount;
        }

        private static void ValidateSolveP3PPointCount(int pointCount, string parameterName)
        {
            if (pointCount != 3 && pointCount != 4)
            {
                throw new ArgumentException(
                    "SolveP3P requires exactly three or four point correspondences.",
                    parameterName);
            }
        }

        private static void ValidateSolveP3PFlags(SolvePnPFlags flags, string parameterName)
        {
            if (flags != SolvePnPFlags.P3P && flags != SolvePnPFlags.AP3P)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    "SolveP3P supports only P3P and AP3P.");
            }
        }

        private static void ValidateSolveP3POutputs(
            Mat objectPoints,
            Mat imagePoints,
            Mat cameraMatrix,
            Mat distCoeffs,
            Mat rvecs,
            Mat tvecs)
        {
            IntPtr rvecsHandle = rvecs.NativeHandle;
            IntPtr tvecsHandle = tvecs.NativeHandle;
            if (ReferenceEquals(rvecs, tvecs) || rvecsHandle == tvecsHandle)
            {
                throw new ArgumentException(
                    "Rotation and translation outputs must not alias.",
                    nameof(tvecs));
            }

            ValidateSolveP3POutputDoesNotAliasInput(
                rvecs,
                rvecsHandle,
                nameof(rvecs),
                objectPoints,
                imagePoints,
                cameraMatrix,
                distCoeffs);
            ValidateSolveP3POutputDoesNotAliasInput(
                tvecs,
                tvecsHandle,
                nameof(tvecs),
                objectPoints,
                imagePoints,
                cameraMatrix,
                distCoeffs);
        }

        private static void ValidateSolveP3POutputDoesNotAliasInput(
            Mat output,
            IntPtr outputHandle,
            string parameterName,
            Mat objectPoints,
            Mat imagePoints,
            Mat cameraMatrix,
            Mat distCoeffs)
        {
            if (SolveP3PMatsAlias(output, outputHandle, objectPoints) ||
                SolveP3PMatsAlias(output, outputHandle, imagePoints) ||
                SolveP3PMatsAlias(output, outputHandle, cameraMatrix) ||
                SolveP3PMatsAlias(output, outputHandle, distCoeffs))
            {
                throw new ArgumentException(
                    "SolveP3P outputs must not alias any input matrix.",
                    parameterName);
            }
        }

        private static bool SolveP3PMatsAlias(Mat first, IntPtr firstHandle, Mat second)
        {
            return ReferenceEquals(first, second) || firstHandle == second.NativeHandle;
        }
    }
}
