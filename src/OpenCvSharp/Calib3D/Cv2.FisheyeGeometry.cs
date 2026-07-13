using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Calib3D
{
    public static partial class Cv2
    {
        private static readonly TermCriteria DefaultFisheyeGeometryCriteria =
            TermCriteria.ByCountAndEpsilon(10, 1.0e-8);

        /// <summary>
        /// Projects 3D points using the fisheye camera model.
        /// 使用鱼眼相机模型投影三维点。
        /// </summary>
        public static void FisheyeProjectPoints(
            Mat objectPoints,
            Mat rvec,
            Mat tvec,
            Mat cameraMatrix,
            Mat distCoeffs,
            Mat imagePoints,
            double alpha = 0.0,
            Mat? jacobian = null)
        {
            ThrowIfNull(objectPoints, nameof(objectPoints));
            ThrowIfNull(rvec, nameof(rvec));
            ThrowIfNull(tvec, nameof(tvec));
            ThrowIfNull(cameraMatrix, nameof(cameraMatrix));
            ThrowIfNull(distCoeffs, nameof(distCoeffs));
            ThrowIfNull(imagePoints, nameof(imagePoints));
            ValidateFisheyePointMat(objectPoints, 3, nameof(objectPoints));
            ValidateThreeVector(rvec, nameof(rvec), false);
            ValidateThreeVector(tvec, nameof(tvec), false);
            ValidateFisheyeGeometryIntrinsics(cameraMatrix, distCoeffs);
            ValidateFinite(alpha, nameof(alpha));
            ValidateFisheyeProjectPointsOutputs(
                new[] { objectPoints, rvec, tvec, cameraMatrix, distCoeffs },
                imagePoints,
                jacobian);

            NativeException.ThrowIfError(NativeMethods.Calib3DFisheyeProjectPoints(
                objectPoints.NativeHandle,
                rvec.NativeHandle,
                tvec.NativeHandle,
                cameraMatrix.NativeHandle,
                distCoeffs.NativeHandle,
                imagePoints.NativeHandle,
                alpha,
                GetNativeHandleOrZero(jacobian)));
        }

        /// <summary>
        /// Projects 3D points using the fisheye camera model and returns an owned point matrix.
        /// 使用鱼眼相机模型投影三维点并返回拥有所有权的点矩阵。
        /// </summary>
        public static Mat FisheyeProjectPoints(
            Mat objectPoints,
            Mat rvec,
            Mat tvec,
            Mat cameraMatrix,
            Mat distCoeffs,
            double alpha = 0.0)
        {
            var imagePoints = new Mat();
            try
            {
                FisheyeProjectPoints(
                    objectPoints,
                    rvec,
                    tvec,
                    cameraMatrix,
                    distCoeffs,
                    imagePoints,
                    alpha);
                return imagePoints;
            }
            catch
            {
                imagePoints.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Projects managed 3D points using the fisheye camera model.
        /// 使用鱼眼相机模型投影托管三维点。
        /// </summary>
        public static Mat FisheyeProjectPoints(
            Point3f[] objectPoints,
            Mat rvec,
            Mat tvec,
            Mat cameraMatrix,
            Mat distCoeffs,
            double alpha = 0.0)
        {
            ValidatePointArray(objectPoints, nameof(objectPoints));
            using (Mat objectPointMat = ToPointMat(objectPoints))
            {
                return FisheyeProjectPoints(
                    objectPointMat,
                    rvec,
                    tvec,
                    cameraMatrix,
                    distCoeffs,
                    alpha);
            }
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Projects a span of 3D points using the fisheye camera model.
        /// 使用鱼眼相机模型投影三维点 Span。
        /// </summary>
        public static Mat FisheyeProjectPoints(
            ReadOnlySpan<Point3f> objectPoints,
            Mat rvec,
            Mat tvec,
            Mat cameraMatrix,
            Mat distCoeffs,
            double alpha = 0.0)
        {
            ValidatePointSpan(objectPoints, nameof(objectPoints));
            using (Mat objectPointMat = ToPointMat(objectPoints))
            {
                return FisheyeProjectPoints(
                    objectPointMat,
                    rvec,
                    tvec,
                    cameraMatrix,
                    distCoeffs,
                    alpha);
            }
        }
#endif

        /// <summary>
        /// Distorts normalized 2D points using the fisheye camera model.
        /// 使用鱼眼相机模型对归一化二维点施加畸变。
        /// </summary>
        /// <remarks>
        /// Input points use identity-camera normalized coordinates. 输入点使用单位相机矩阵的归一化坐标。
        /// </remarks>
        public static void FisheyeDistortPoints(
            Mat undistorted,
            Mat distorted,
            Mat cameraMatrix,
            Mat distCoeffs,
            double alpha = 0.0)
        {
            ThrowIfNull(undistorted, nameof(undistorted));
            ThrowIfNull(distorted, nameof(distorted));
            ThrowIfNull(cameraMatrix, nameof(cameraMatrix));
            ThrowIfNull(distCoeffs, nameof(distCoeffs));
            ValidateFisheyePointMat(undistorted, 2, nameof(undistorted));
            ValidateFisheyeGeometryIntrinsics(cameraMatrix, distCoeffs);
            ValidateFinite(alpha, nameof(alpha));

            NativeException.ThrowIfError(NativeMethods.Calib3DFisheyeDistortPoints(
                undistorted.NativeHandle,
                distorted.NativeHandle,
                cameraMatrix.NativeHandle,
                distCoeffs.NativeHandle,
                alpha));
        }

        /// <summary>
        /// Distorts normalized 2D points and returns an owned point matrix.
        /// 对归一化二维点施加畸变并返回拥有所有权的点矩阵。
        /// </summary>
        public static Mat FisheyeDistortPoints(
            Mat undistorted,
            Mat cameraMatrix,
            Mat distCoeffs,
            double alpha = 0.0)
        {
            var distorted = new Mat();
            try
            {
                FisheyeDistortPoints(undistorted, distorted, cameraMatrix, distCoeffs, alpha);
                return distorted;
            }
            catch
            {
                distorted.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Distorts 2D points expressed with a non-identity undistorted camera matrix.
        /// 对使用非单位去畸变相机矩阵表示的二维点施加畸变。
        /// </summary>
        public static void FisheyeDistortPointsWithCameraMatrix(
            Mat undistorted,
            Mat distorted,
            Mat undistortedCameraMatrix,
            Mat cameraMatrix,
            Mat distCoeffs,
            double alpha = 0.0)
        {
            ThrowIfNull(undistorted, nameof(undistorted));
            ThrowIfNull(distorted, nameof(distorted));
            ThrowIfNull(undistortedCameraMatrix, nameof(undistortedCameraMatrix));
            ThrowIfNull(cameraMatrix, nameof(cameraMatrix));
            ThrowIfNull(distCoeffs, nameof(distCoeffs));
            ValidateFisheyePointMat(undistorted, 2, nameof(undistorted));
            ValidateCameraMatrix(undistortedCameraMatrix, nameof(undistortedCameraMatrix));
            ValidateFisheyeGeometryIntrinsics(cameraMatrix, distCoeffs);
            ValidateFinite(alpha, nameof(alpha));

            NativeException.ThrowIfError(
                NativeMethods.Calib3DFisheyeDistortPointsWithCameraMatrix(
                    undistorted.NativeHandle,
                    distorted.NativeHandle,
                    undistortedCameraMatrix.NativeHandle,
                    cameraMatrix.NativeHandle,
                    distCoeffs.NativeHandle,
                    alpha));
        }

        /// <summary>
        /// Distorts 2D points expressed with a non-identity camera matrix and returns an owned matrix.
        /// 对使用非单位相机矩阵表示的二维点施加畸变并返回拥有所有权的矩阵。
        /// </summary>
        public static Mat FisheyeDistortPointsWithCameraMatrix(
            Mat undistorted,
            Mat undistortedCameraMatrix,
            Mat cameraMatrix,
            Mat distCoeffs,
            double alpha = 0.0)
        {
            var distorted = new Mat();
            try
            {
                FisheyeDistortPointsWithCameraMatrix(
                    undistorted,
                    distorted,
                    undistortedCameraMatrix,
                    cameraMatrix,
                    distCoeffs,
                    alpha);
                return distorted;
            }
            catch
            {
                distorted.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Distorts managed normalized 2D points using the fisheye camera model.
        /// 使用鱼眼相机模型对托管归一化二维点施加畸变。
        /// </summary>
        public static Mat FisheyeDistortPoints(
            Point2f[] undistorted,
            Mat cameraMatrix,
            Mat distCoeffs,
            double alpha = 0.0)
        {
            ValidatePointArray(undistorted, nameof(undistorted));
            using (Mat undistortedMat = ToPointMat(undistorted))
            {
                return FisheyeDistortPoints(undistortedMat, cameraMatrix, distCoeffs, alpha);
            }
        }

        /// <summary>
        /// Distorts managed 2D points expressed with a non-identity camera matrix.
        /// 对使用非单位相机矩阵表示的托管二维点施加畸变。
        /// </summary>
        public static Mat FisheyeDistortPointsWithCameraMatrix(
            Point2f[] undistorted,
            Mat undistortedCameraMatrix,
            Mat cameraMatrix,
            Mat distCoeffs,
            double alpha = 0.0)
        {
            ValidatePointArray(undistorted, nameof(undistorted));
            using (Mat undistortedMat = ToPointMat(undistorted))
            {
                return FisheyeDistortPointsWithCameraMatrix(
                    undistortedMat,
                    undistortedCameraMatrix,
                    cameraMatrix,
                    distCoeffs,
                    alpha);
            }
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Distorts a span of normalized 2D points using the fisheye camera model.
        /// 使用鱼眼相机模型对归一化二维点 Span 施加畸变。
        /// </summary>
        public static Mat FisheyeDistortPoints(
            ReadOnlySpan<Point2f> undistorted,
            Mat cameraMatrix,
            Mat distCoeffs,
            double alpha = 0.0)
        {
            ValidatePointSpan(undistorted, nameof(undistorted));
            using (Mat undistortedMat = ToPointMat(undistorted))
            {
                return FisheyeDistortPoints(undistortedMat, cameraMatrix, distCoeffs, alpha);
            }
        }

        /// <summary>
        /// Distorts a span of 2D points expressed with a non-identity camera matrix.
        /// 对使用非单位相机矩阵表示的二维点 Span 施加畸变。
        /// </summary>
        public static Mat FisheyeDistortPointsWithCameraMatrix(
            ReadOnlySpan<Point2f> undistorted,
            Mat undistortedCameraMatrix,
            Mat cameraMatrix,
            Mat distCoeffs,
            double alpha = 0.0)
        {
            ValidatePointSpan(undistorted, nameof(undistorted));
            using (Mat undistortedMat = ToPointMat(undistorted))
            {
                return FisheyeDistortPointsWithCameraMatrix(
                    undistortedMat,
                    undistortedCameraMatrix,
                    cameraMatrix,
                    distCoeffs,
                    alpha);
            }
        }
#endif

        /// <summary>
        /// Undistorts fisheye image points with optional rectification and projection matrices.
        /// 使用可选校正与投影矩阵对鱼眼像点去畸变。
        /// </summary>
        /// <remarks>
        /// Without <paramref name="p"/>, output uses normalized coordinates. Supplying
        /// <paramref name="p"/> produces coordinates in that projected image plane.
        /// 未提供 <paramref name="p"/> 时输出归一化坐标；提供后输出位于对应投影图像平面。
        /// </remarks>
        public static void FisheyeUndistortPoints(
            Mat distorted,
            Mat undistorted,
            Mat cameraMatrix,
            Mat distCoeffs,
            Mat? r = null,
            Mat? p = null,
            TermCriteria? criteria = null)
        {
            ThrowIfNull(distorted, nameof(distorted));
            ThrowIfNull(undistorted, nameof(undistorted));
            ThrowIfNull(cameraMatrix, nameof(cameraMatrix));
            ThrowIfNull(distCoeffs, nameof(distCoeffs));
            ValidateFisheyePointMat(distorted, 2, nameof(distorted));
            ValidateFisheyeGeometryIntrinsics(cameraMatrix, distCoeffs);
            ValidateOptionalRectification(r, nameof(r));
            ValidateOptionalProjection(p, nameof(p));
            TermCriteria resolved = criteria ?? DefaultFisheyeGeometryCriteria;
            ValidateRegistrationCriteria(resolved, nameof(criteria));

            NativeException.ThrowIfError(NativeMethods.Calib3DFisheyeUndistortPoints(
                distorted.NativeHandle,
                undistorted.NativeHandle,
                cameraMatrix.NativeHandle,
                distCoeffs.NativeHandle,
                GetNativeHandleOrZero(r),
                GetNativeHandleOrZero(p),
                (int)resolved.Type,
                resolved.MaxCount,
                resolved.Epsilon));
        }

        /// <summary>
        /// Undistorts fisheye image points and returns an owned point matrix.
        /// 对鱼眼像点去畸变并返回拥有所有权的点矩阵。
        /// </summary>
        public static Mat FisheyeUndistortPoints(
            Mat distorted,
            Mat cameraMatrix,
            Mat distCoeffs,
            Mat? r = null,
            Mat? p = null,
            TermCriteria? criteria = null)
        {
            var undistorted = new Mat();
            try
            {
                FisheyeUndistortPoints(
                    distorted,
                    undistorted,
                    cameraMatrix,
                    distCoeffs,
                    r,
                    p,
                    criteria);
                return undistorted;
            }
            catch
            {
                undistorted.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Undistorts managed fisheye image points.
        /// 对托管鱼眼像点去畸变。
        /// </summary>
        public static Mat FisheyeUndistortPoints(
            Point2f[] distorted,
            Mat cameraMatrix,
            Mat distCoeffs,
            Mat? r = null,
            Mat? p = null,
            TermCriteria? criteria = null)
        {
            ValidatePointArray(distorted, nameof(distorted));
            using (Mat distortedMat = ToPointMat(distorted))
            {
                return FisheyeUndistortPoints(
                    distortedMat,
                    cameraMatrix,
                    distCoeffs,
                    r,
                    p,
                    criteria);
            }
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Undistorts a span of fisheye image points.
        /// 对鱼眼像点 Span 去畸变。
        /// </summary>
        public static Mat FisheyeUndistortPoints(
            ReadOnlySpan<Point2f> distorted,
            Mat cameraMatrix,
            Mat distCoeffs,
            Mat? r = null,
            Mat? p = null,
            TermCriteria? criteria = null)
        {
            ValidatePointSpan(distorted, nameof(distorted));
            using (Mat distortedMat = ToPointMat(distorted))
            {
                return FisheyeUndistortPoints(
                    distortedMat,
                    cameraMatrix,
                    distCoeffs,
                    r,
                    p,
                    criteria);
            }
        }
#endif

        /// <summary>
        /// Estimates a new camera matrix for fisheye undistortion or rectification.
        /// 估计用于鱼眼去畸变或校正的新相机矩阵。
        /// </summary>
        public static void FisheyeEstimateNewCameraMatrixForUndistortRectify(
            Mat cameraMatrix,
            Mat distCoeffs,
            Size imageSize,
            Mat newCameraMatrix,
            Mat? r = null,
            double balance = 0.0,
            Size? newSize = null,
            double fovScale = 1.0)
        {
            ThrowIfNull(cameraMatrix, nameof(cameraMatrix));
            ThrowIfNull(distCoeffs, nameof(distCoeffs));
            ThrowIfNull(newCameraMatrix, nameof(newCameraMatrix));
            ValidateFisheyeGeometryIntrinsics(cameraMatrix, distCoeffs);
            ValidatePositiveSize(imageSize, nameof(imageSize));
            ValidateOptionalRectification(r, nameof(r));
            ValidateFinite(balance, nameof(balance));
            if (balance < 0.0 || balance > 1.0)
            {
                throw new ArgumentOutOfRangeException(nameof(balance), "Balance must be in [0, 1].");
            }
            ValidateFinite(fovScale, nameof(fovScale));
            if (fovScale <= 0.0)
            {
                throw new ArgumentOutOfRangeException(nameof(fovScale), "FOV scale must be positive.");
            }

            Size resolvedNewSize = newSize ?? default;
            ValidateOptionalNewSize(resolvedNewSize, nameof(newSize));
            NativeException.ThrowIfError(NativeMethods.Calib3DFisheyeEstimateNewCameraMatrix(
                cameraMatrix.NativeHandle,
                distCoeffs.NativeHandle,
                imageSize.Width,
                imageSize.Height,
                GetNativeHandleOrZero(r),
                newCameraMatrix.NativeHandle,
                balance,
                resolvedNewSize.Width,
                resolvedNewSize.Height,
                fovScale));
        }

        /// <summary>
        /// Estimates and returns an owned camera matrix for fisheye undistortion or rectification.
        /// 估计并返回用于鱼眼去畸变或校正的拥有所有权的新相机矩阵。
        /// </summary>
        public static Mat FisheyeEstimateNewCameraMatrixForUndistortRectify(
            Mat cameraMatrix,
            Mat distCoeffs,
            Size imageSize,
            Mat? r = null,
            double balance = 0.0,
            Size? newSize = null,
            double fovScale = 1.0)
        {
            var newCameraMatrix = new Mat();
            try
            {
                FisheyeEstimateNewCameraMatrixForUndistortRectify(
                    cameraMatrix,
                    distCoeffs,
                    imageSize,
                    newCameraMatrix,
                    r,
                    balance,
                    newSize,
                    fovScale);
                return newCameraMatrix;
            }
            catch
            {
                newCameraMatrix.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Solves a pose from 3D-2D correspondences using the fisheye camera model.
        /// 使用鱼眼相机模型根据 3D-2D 对应点求解位姿。
        /// </summary>
        public static bool FisheyeSolvePnP(
            Mat objectPoints,
            Mat imagePoints,
            Mat cameraMatrix,
            Mat distCoeffs,
            Mat rvec,
            Mat tvec,
            bool useExtrinsicGuess = false,
            SolvePnPFlags flags = SolvePnPFlags.Iterative,
            TermCriteria? criteria = null)
        {
            ThrowIfNull(objectPoints, nameof(objectPoints));
            ThrowIfNull(imagePoints, nameof(imagePoints));
            ThrowIfNull(cameraMatrix, nameof(cameraMatrix));
            ThrowIfNull(distCoeffs, nameof(distCoeffs));
            ThrowIfNull(rvec, nameof(rvec));
            ThrowIfNull(tvec, nameof(tvec));
            int objectPointCount = ValidateFisheyePointMat(objectPoints, 3, nameof(objectPoints));
            int imagePointCount = ValidateFisheyePointMat(imagePoints, 2, nameof(imagePoints));
            if (objectPointCount != imagePointCount)
            {
                throw new ArgumentException("Object and image point counts must match.", nameof(imagePoints));
            }
            ValidateFisheyeGeometryIntrinsics(cameraMatrix, distCoeffs);
            if (useExtrinsicGuess)
            {
                ValidateThreeVector(rvec, nameof(rvec), false);
                ValidateThreeVector(tvec, nameof(tvec), false);
            }
            ValidateSolvePnPFlags(flags);
            TermCriteria resolved = criteria ?? DefaultFisheyeGeometryCriteria;
            ValidateRegistrationCriteria(resolved, nameof(criteria));

            NativeException.ThrowIfError(NativeMethods.Calib3DFisheyeSolvePnP(
                objectPoints.NativeHandle,
                imagePoints.NativeHandle,
                cameraMatrix.NativeHandle,
                distCoeffs.NativeHandle,
                rvec.NativeHandle,
                tvec.NativeHandle,
                useExtrinsicGuess ? 1 : 0,
                (int)flags,
                (int)resolved.Type,
                resolved.MaxCount,
                resolved.Epsilon,
                out int solved));
            return solved != 0;
        }

        /// <summary>
        /// Solves a fisheye pose from managed point arrays.
        /// 使用托管点数组求解鱼眼位姿。
        /// </summary>
        public static bool FisheyeSolvePnP(
            Point3f[] objectPoints,
            Point2f[] imagePoints,
            Mat cameraMatrix,
            Mat distCoeffs,
            Mat rvec,
            Mat tvec,
            bool useExtrinsicGuess = false,
            SolvePnPFlags flags = SolvePnPFlags.Iterative,
            TermCriteria? criteria = null)
        {
            ValidateMatchingPointArrays(objectPoints, imagePoints);
            using (Mat objectPointMat = ToPointMat(objectPoints))
            using (Mat imagePointMat = ToPointMat(imagePoints))
            {
                return FisheyeSolvePnP(
                    objectPointMat,
                    imagePointMat,
                    cameraMatrix,
                    distCoeffs,
                    rvec,
                    tvec,
                    useExtrinsicGuess,
                    flags,
                    criteria);
            }
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Solves a fisheye pose from point spans.
        /// 使用点 Span 求解鱼眼位姿。
        /// </summary>
        public static bool FisheyeSolvePnP(
            ReadOnlySpan<Point3f> objectPoints,
            ReadOnlySpan<Point2f> imagePoints,
            Mat cameraMatrix,
            Mat distCoeffs,
            Mat rvec,
            Mat tvec,
            bool useExtrinsicGuess = false,
            SolvePnPFlags flags = SolvePnPFlags.Iterative,
            TermCriteria? criteria = null)
        {
            ValidateMatchingPointSpans(objectPoints, imagePoints);
            using (Mat objectPointMat = ToPointMat(objectPoints))
            using (Mat imagePointMat = ToPointMat(imagePoints))
            {
                return FisheyeSolvePnP(
                    objectPointMat,
                    imagePointMat,
                    cameraMatrix,
                    distCoeffs,
                    rvec,
                    tvec,
                    useExtrinsicGuess,
                    flags,
                    criteria);
            }
        }
#endif

        /// <summary>
        /// Solves a fisheye pose using RANSAC.
        /// 使用 RANSAC 求解鱼眼位姿。
        /// </summary>
        public static bool FisheyeSolvePnPRansac(
            Mat objectPoints,
            Mat imagePoints,
            Mat cameraMatrix,
            Mat distCoeffs,
            Mat rvec,
            Mat tvec,
            bool useExtrinsicGuess = false,
            int iterationsCount = 100,
            float reprojectionError = 8.0F,
            double confidence = 0.99,
            Mat? inliers = null,
            SolvePnPFlags flags = SolvePnPFlags.Iterative,
            TermCriteria? criteria = null)
        {
            ThrowIfNull(objectPoints, nameof(objectPoints));
            ThrowIfNull(imagePoints, nameof(imagePoints));
            ThrowIfNull(cameraMatrix, nameof(cameraMatrix));
            ThrowIfNull(distCoeffs, nameof(distCoeffs));
            ThrowIfNull(rvec, nameof(rvec));
            ThrowIfNull(tvec, nameof(tvec));
            int objectPointCount = ValidateFisheyePointMat(objectPoints, 3, nameof(objectPoints));
            int imagePointCount = ValidateFisheyePointMat(imagePoints, 2, nameof(imagePoints));
            if (objectPointCount != imagePointCount)
            {
                throw new ArgumentException("Object and image point counts must match.", nameof(imagePoints));
            }
            ValidateFisheyeGeometryIntrinsics(cameraMatrix, distCoeffs);
            if (useExtrinsicGuess)
            {
                ValidateThreeVector(rvec, nameof(rvec), false);
                ValidateThreeVector(tvec, nameof(tvec), false);
            }
            if (iterationsCount <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(iterationsCount),
                    "Iteration count must be positive.");
            }
            ValidateFinite(reprojectionError, nameof(reprojectionError));
            if (reprojectionError <= 0.0F)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(reprojectionError),
                    "Reprojection error threshold must be positive.");
            }
            ValidateFinite(confidence, nameof(confidence));
            if (confidence <= 0.0 || confidence >= 1.0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(confidence),
                    "Confidence must be strictly between zero and one.");
            }
            ValidateSolvePnPFlags(flags);
            TermCriteria resolved = criteria ?? DefaultFisheyeGeometryCriteria;
            ValidateRegistrationCriteria(resolved, nameof(criteria));

            NativeException.ThrowIfError(NativeMethods.Calib3DFisheyeSolvePnPRansac(
                objectPoints.NativeHandle,
                imagePoints.NativeHandle,
                cameraMatrix.NativeHandle,
                distCoeffs.NativeHandle,
                rvec.NativeHandle,
                tvec.NativeHandle,
                useExtrinsicGuess ? 1 : 0,
                iterationsCount,
                reprojectionError,
                confidence,
                GetNativeHandleOrZero(inliers),
                (int)flags,
                (int)resolved.Type,
                resolved.MaxCount,
                resolved.Epsilon,
                out int solved));
            return solved != 0;
        }

        /// <summary>
        /// Solves a fisheye pose with RANSAC from managed point arrays.
        /// 使用 RANSAC 根据托管点数组求解鱼眼位姿。
        /// </summary>
        public static bool FisheyeSolvePnPRansac(
            Point3f[] objectPoints,
            Point2f[] imagePoints,
            Mat cameraMatrix,
            Mat distCoeffs,
            Mat rvec,
            Mat tvec,
            bool useExtrinsicGuess = false,
            int iterationsCount = 100,
            float reprojectionError = 8.0F,
            double confidence = 0.99,
            Mat? inliers = null,
            SolvePnPFlags flags = SolvePnPFlags.Iterative,
            TermCriteria? criteria = null)
        {
            ValidateMatchingPointArrays(objectPoints, imagePoints);
            using (Mat objectPointMat = ToPointMat(objectPoints))
            using (Mat imagePointMat = ToPointMat(imagePoints))
            {
                return FisheyeSolvePnPRansac(
                    objectPointMat,
                    imagePointMat,
                    cameraMatrix,
                    distCoeffs,
                    rvec,
                    tvec,
                    useExtrinsicGuess,
                    iterationsCount,
                    reprojectionError,
                    confidence,
                    inliers,
                    flags,
                    criteria);
            }
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Solves a fisheye pose with RANSAC from point spans.
        /// 使用 RANSAC 根据点 Span 求解鱼眼位姿。
        /// </summary>
        public static bool FisheyeSolvePnPRansac(
            ReadOnlySpan<Point3f> objectPoints,
            ReadOnlySpan<Point2f> imagePoints,
            Mat cameraMatrix,
            Mat distCoeffs,
            Mat rvec,
            Mat tvec,
            bool useExtrinsicGuess = false,
            int iterationsCount = 100,
            float reprojectionError = 8.0F,
            double confidence = 0.99,
            Mat? inliers = null,
            SolvePnPFlags flags = SolvePnPFlags.Iterative,
            TermCriteria? criteria = null)
        {
            ValidateMatchingPointSpans(objectPoints, imagePoints);
            using (Mat objectPointMat = ToPointMat(objectPoints))
            using (Mat imagePointMat = ToPointMat(imagePoints))
            {
                return FisheyeSolvePnPRansac(
                    objectPointMat,
                    imagePointMat,
                    cameraMatrix,
                    distCoeffs,
                    rvec,
                    tvec,
                    useExtrinsicGuess,
                    iterationsCount,
                    reprojectionError,
                    confidence,
                    inliers,
                    flags,
                    criteria);
            }
        }
#endif

        private static void ValidateFisheyeGeometryIntrinsics(Mat cameraMatrix, Mat distCoeffs)
        {
            ValidateCameraMatrix(cameraMatrix, nameof(cameraMatrix));
            ValidateCameraUtilityFloatingDepth(cameraMatrix, nameof(cameraMatrix));
            ValidateCameraUtilityFloatingDepth(distCoeffs, nameof(distCoeffs));
            if (distCoeffs.Channels != 1 ||
                !((distCoeffs.Rows == 4 && distCoeffs.Cols == 1) ||
                  (distCoeffs.Rows == 1 && distCoeffs.Cols == 4)))
            {
                throw new ArgumentException(
                    "Fisheye distortion coefficients must contain exactly four scalar values.",
                    nameof(distCoeffs));
            }
        }

        private static void ValidateCameraMatrix(Mat cameraMatrix, string parameterName)
        {
            if (cameraMatrix.Rows != 3 || cameraMatrix.Cols != 3 || cameraMatrix.Channels != 1)
            {
                throw new ArgumentException("Camera matrix must be 3 x 3 and single-channel.", parameterName);
            }
        }

        private static int ValidateFisheyePointMat(Mat points, int dimensions, string parameterName)
        {
            if (points.Empty || points.Rows <= 0 || points.Cols <= 0)
            {
                throw new ArgumentException("Point matrix cannot be empty.", parameterName);
            }
            ValidateCameraUtilityFloatingDepth(points, parameterName);
            if (points.Channels == dimensions && (points.Rows == 1 || points.Cols == 1))
            {
                return checked(points.Rows * points.Cols);
            }
            if (points.Channels == 1 && points.Cols == dimensions)
            {
                return points.Rows;
            }
            if (points.Channels == 1 && points.Rows == dimensions)
            {
                return points.Cols;
            }

            throw new ArgumentException(
                $"Point matrix must contain {dimensions}-component points.",
                parameterName);
        }

        private static void ValidateThreeVector(Mat value, string parameterName, bool allowEmpty)
        {
            if (value.Empty)
            {
                if (allowEmpty)
                {
                    return;
                }
                throw new ArgumentException("Vector cannot be empty.", parameterName);
            }
            ValidateCameraUtilityFloatingDepth(value, parameterName);
            bool scalarVector = value.Channels == 1 && value.Rows * value.Cols == 3;
            bool channelVector = value.Channels == 3 && value.Rows * value.Cols == 1;
            if (!scalarVector && !channelVector)
            {
                throw new ArgumentException("Vector must contain exactly three values.", parameterName);
            }
        }

        private static void ValidateFisheyeProjectPointsOutputs(
            Mat[] inputs,
            Mat imagePoints,
            Mat? jacobian)
        {
            IntPtr imagePointsHandle = imagePoints.NativeHandle;
            foreach (Mat input in inputs)
            {
                if (FisheyeGeometryMatsAlias(imagePoints, imagePointsHandle, input))
                {
                    throw new ArgumentException(
                        "Fisheye image output must not alias any input matrix.",
                        nameof(imagePoints));
                }
            }

            if (jacobian is null)
            {
                return;
            }

            IntPtr jacobianHandle = jacobian.NativeHandle;
            foreach (Mat input in inputs)
            {
                if (FisheyeGeometryMatsAlias(jacobian, jacobianHandle, input))
                {
                    throw new ArgumentException(
                        "Fisheye jacobian output must not alias any input matrix.",
                        nameof(jacobian));
                }
            }

            if (FisheyeGeometryMatsAlias(jacobian, jacobianHandle, imagePoints))
            {
                throw new ArgumentException(
                    "Fisheye projection outputs must not alias each other.",
                    nameof(jacobian));
            }
        }

        private static bool FisheyeGeometryMatsAlias(Mat first, IntPtr firstHandle, Mat second)
        {
            return ReferenceEquals(first, second) || firstHandle == second.NativeHandle;
        }

        private static void ValidateOptionalRectification(Mat? value, string parameterName)
        {
            if (value == null || value.Empty)
            {
                return;
            }
            bool matrix = value.Channels == 1 && value.Rows == 3 && value.Cols == 3;
            bool scalarVector = value.Channels == 1 && value.Rows * value.Cols == 3;
            bool channelVector = value.Channels == 3 && value.Rows * value.Cols == 1;
            if (!matrix && !scalarVector && !channelVector)
            {
                throw new ArgumentException(
                    "Rectification must be a 3 x 3 matrix or a three-value vector.",
                    parameterName);
            }
        }

        private static void ValidateOptionalProjection(Mat? value, string parameterName)
        {
            if (value == null || value.Empty)
            {
                return;
            }
            if (value.Channels != 1 ||
                value.Rows != 3 ||
                (value.Cols != 3 && value.Cols != 4))
            {
                throw new ArgumentException("Projection matrix must be 3 x 3 or 3 x 4.", parameterName);
            }
        }

        private static void ValidateOptionalNewSize(Size size, string parameterName)
        {
            if (size.Width < 0 || size.Height < 0 ||
                ((size.Width == 0) != (size.Height == 0)))
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    "New size must be empty or have positive width and height.");
            }
        }

        private static void ValidateSolvePnPFlags(SolvePnPFlags flags)
        {
            if (flags < SolvePnPFlags.Iterative || flags > SolvePnPFlags.SQPNP)
            {
                throw new ArgumentOutOfRangeException(nameof(flags), "Unsupported SolvePnP method.");
            }
        }

        private static void ValidateFinite(double value, string parameterName)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(parameterName, "Value must be finite.");
            }
        }

        private static void ValidateFinite(float value, string parameterName)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(parameterName, "Value must be finite.");
            }
        }

        private static void ValidatePointArray<T>(T[] points, string parameterName)
        {
            ThrowIfNull(points, parameterName);
            if (points.Length == 0)
            {
                throw new ArgumentException("Point array cannot be empty.", parameterName);
            }
        }

        private static void ValidateMatchingPointArrays(Point3f[] objectPoints, Point2f[] imagePoints)
        {
            ValidatePointArray(objectPoints, nameof(objectPoints));
            ValidatePointArray(imagePoints, nameof(imagePoints));
            if (objectPoints.Length != imagePoints.Length)
            {
                throw new ArgumentException(
                    "Object and image point counts must match.",
                    nameof(imagePoints));
            }
        }

#if NETCOREAPP3_1_OR_GREATER
        private static void ValidatePointSpan<T>(ReadOnlySpan<T> points, string parameterName)
        {
            if (points.IsEmpty)
            {
                throw new ArgumentException("Point span cannot be empty.", parameterName);
            }
        }

        private static void ValidateMatchingPointSpans(
            ReadOnlySpan<Point3f> objectPoints,
            ReadOnlySpan<Point2f> imagePoints)
        {
            ValidatePointSpan(objectPoints, nameof(objectPoints));
            ValidatePointSpan(imagePoints, nameof(imagePoints));
            if (objectPoints.Length != imagePoints.Length)
            {
                throw new ArgumentException(
                    "Object and image point counts must match.",
                    nameof(imagePoints));
            }
        }
#endif
    }
}
