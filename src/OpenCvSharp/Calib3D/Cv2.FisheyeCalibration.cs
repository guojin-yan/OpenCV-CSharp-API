using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Calib3D
{
    public static unsafe partial class Cv2
    {
        private static readonly TermCriteria DefaultFisheyeCalibrationCriteria =
            TermCriteria.ByCountAndEpsilon(100, 2.2204460492503131E-16);

        /// <summary>
        /// Calibrates one fisheye camera and writes intrinsics and per-view board poses.
        /// 标定单个鱼眼相机，并写入内参和每视图标定板位姿。
        /// </summary>
        /// <remarks>
        /// In the fisheye model, <c>FixK1</c> through <c>FixK4</c> set the corresponding distortion
        /// coefficients to zero and keep them fixed; they do not preserve caller-provided values.
        /// 在鱼眼模型中，<c>FixK1</c> 至 <c>FixK4</c> 会将对应畸变系数设为零并固定，而不是保留调用方输入值。
        /// </remarks>
        public static double FisheyeCalibrate(
            Point3f[][] objectPoints,
            Point2f[][] imagePoints,
            Size imageSize,
            Mat cameraMatrix,
            Mat distCoeffs,
            Mat rvecs,
            Mat tvecs,
            CalibrationFlags flags = CalibrationFlags.None,
            TermCriteria? criteria = null)
        {
            ValidateFisheyeSingleMats(cameraMatrix, distCoeffs, rvecs, tvecs);
            ValidatePositiveSize(imageSize, nameof(imageSize));
            ValidateFisheyeFlags(flags, false);
            ValidateFisheyeInitialIntrinsics(cameraMatrix, distCoeffs, flags, nameof(cameraMatrix));
            TermCriteria resolved = criteria ?? DefaultFisheyeCalibrationCriteria;
            ValidateRegistrationCriteria(resolved, nameof(criteria));

            PrepareCalibrationPointGroups(
                objectPoints,
                imagePoints,
                nameof(objectPoints),
                nameof(imagePoints),
                out int[] objectOffsets,
                out NativeMethods.Calib3DPoint3fNative[] nativeObjectPoints,
                out int[] imageOffsets,
                out NativeMethods.Calib3DPoint2fNative[] nativeImagePoints);

            fixed (int* objectOffsetsPtr = objectOffsets)
            fixed (NativeMethods.Calib3DPoint3fNative* objectPointsPtr = nativeObjectPoints)
            fixed (int* imageOffsetsPtr = imageOffsets)
            fixed (NativeMethods.Calib3DPoint2fNative* imagePointsPtr = nativeImagePoints)
            {
                NativeException.ThrowIfError(NativeMethods.Calib3DFisheyeCalibrate(
                    objectOffsetsPtr,
                    objectPoints.Length,
                    objectPointsPtr,
                    nativeObjectPoints.Length,
                    imageOffsetsPtr,
                    imagePoints.Length,
                    imagePointsPtr,
                    nativeImagePoints.Length,
                    imageSize.Width,
                    imageSize.Height,
                    cameraMatrix.NativeHandle,
                    distCoeffs.NativeHandle,
                    rvecs.NativeHandle,
                    tvecs.NativeHandle,
                    (int)flags,
                    (int)resolved.Type,
                    resolved.MaxCount,
                    resolved.Epsilon,
                    out double reprojectionError));
                return reprojectionError;
            }
        }

        /// <summary>
        /// Calibrates one fisheye camera and returns owned output matrices.
        /// 标定单个鱼眼相机并返回拥有所有权的输出矩阵。
        /// </summary>
        /// <remarks>
        /// The caller must dispose every matrix in the result. In the fisheye model, <c>FixK1</c>
        /// through <c>FixK4</c> set the corresponding distortion coefficients to zero and keep them fixed.
        /// 调用方必须释放结果中的每个矩阵。在鱼眼模型中，<c>FixK1</c> 至 <c>FixK4</c>
        /// 会将对应畸变系数设为零并固定。
        /// </remarks>
        public static CalibrationResult FisheyeCalibrate(
            Point3f[][] objectPoints,
            Point2f[][] imagePoints,
            Size imageSize,
            CalibrationFlags flags = CalibrationFlags.None,
            TermCriteria? criteria = null)
        {
            RejectOwnedFisheyeIntrinsicFlags(flags, false);
            var cameraMatrix = new Mat();
            var distCoeffs = new Mat();
            var rvecs = new Mat();
            var tvecs = new Mat();
            try
            {
                double reprojectionError = FisheyeCalibrate(
                    objectPoints,
                    imagePoints,
                    imageSize,
                    cameraMatrix,
                    distCoeffs,
                    rvecs,
                    tvecs,
                    flags,
                    criteria);
                return new CalibrationResult(reprojectionError, cameraMatrix, distCoeffs, rvecs, tvecs);
            }
            catch
            {
                cameraMatrix.Dispose();
                distCoeffs.Dispose();
                rvecs.Dispose();
                tvecs.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Calibrates a fisheye stereo pair and writes intrinsics and the camera-to-camera transform.
        /// 标定鱼眼双目相机，并写入内参和相机间变换。
        /// </summary>
        /// <remarks>
        /// In the fisheye model, <c>FixK1</c> through <c>FixK4</c> set the corresponding distortion
        /// coefficients to zero and keep them fixed; they do not preserve caller-provided values.
        /// 在鱼眼模型中，<c>FixK1</c> 至 <c>FixK4</c> 会将对应畸变系数设为零并固定，而不是保留调用方输入值。
        /// </remarks>
        public static double FisheyeStereoCalibrate(
            Point3f[][] objectPoints,
            Point2f[][] imagePoints1,
            Point2f[][] imagePoints2,
            Mat cameraMatrix1,
            Mat distCoeffs1,
            Mat cameraMatrix2,
            Mat distCoeffs2,
            Size imageSize,
            Mat r,
            Mat t,
            CalibrationFlags flags = CalibrationFlags.FixIntrinsic,
            TermCriteria? criteria = null)
        {
            ValidateFisheyeStereoMats(cameraMatrix1, distCoeffs1, cameraMatrix2, distCoeffs2, r, t);
            ValidatePositiveSize(imageSize, nameof(imageSize));
            ValidateFisheyeFlags(flags, true);
            ValidateFisheyeInitialIntrinsics(cameraMatrix1, distCoeffs1, flags, nameof(cameraMatrix1));
            ValidateFisheyeInitialIntrinsics(cameraMatrix2, distCoeffs2, flags, nameof(cameraMatrix2));
            TermCriteria resolved = criteria ?? DefaultFisheyeCalibrationCriteria;
            ValidateRegistrationCriteria(resolved, nameof(criteria));

            PrepareStereoCalibrationPointGroups(
                objectPoints,
                imagePoints1,
                imagePoints2,
                out int[] objectOffsets,
                out NativeMethods.Calib3DPoint3fNative[] nativeObjectPoints,
                out int[] image1Offsets,
                out NativeMethods.Calib3DPoint2fNative[] nativeImagePoints1,
                out int[] image2Offsets,
                out NativeMethods.Calib3DPoint2fNative[] nativeImagePoints2);

            fixed (int* objectOffsetsPtr = objectOffsets)
            fixed (NativeMethods.Calib3DPoint3fNative* objectPointsPtr = nativeObjectPoints)
            fixed (int* image1OffsetsPtr = image1Offsets)
            fixed (NativeMethods.Calib3DPoint2fNative* imagePoints1Ptr = nativeImagePoints1)
            fixed (int* image2OffsetsPtr = image2Offsets)
            fixed (NativeMethods.Calib3DPoint2fNative* imagePoints2Ptr = nativeImagePoints2)
            {
                NativeException.ThrowIfError(NativeMethods.Calib3DFisheyeStereoCalibrate(
                    objectOffsetsPtr,
                    objectPoints.Length,
                    objectPointsPtr,
                    nativeObjectPoints.Length,
                    image1OffsetsPtr,
                    imagePoints1.Length,
                    imagePoints1Ptr,
                    nativeImagePoints1.Length,
                    image2OffsetsPtr,
                    imagePoints2.Length,
                    imagePoints2Ptr,
                    nativeImagePoints2.Length,
                    cameraMatrix1.NativeHandle,
                    distCoeffs1.NativeHandle,
                    cameraMatrix2.NativeHandle,
                    distCoeffs2.NativeHandle,
                    imageSize.Width,
                    imageSize.Height,
                    r.NativeHandle,
                    t.NativeHandle,
                    (int)flags,
                    (int)resolved.Type,
                    resolved.MaxCount,
                    resolved.Epsilon,
                    out double reprojectionError));
                return reprojectionError;
            }
        }

        /// <summary>
        /// Calibrates a fisheye stereo pair and returns owned output matrices.
        /// 标定鱼眼双目相机并返回拥有所有权的输出矩阵。
        /// </summary>
        /// <remarks>
        /// The caller must dispose every matrix in the result. In the fisheye model, <c>FixK1</c>
        /// through <c>FixK4</c> set the corresponding distortion coefficients to zero and keep them fixed.
        /// 调用方必须释放结果中的每个矩阵。在鱼眼模型中，<c>FixK1</c> 至 <c>FixK4</c>
        /// 会将对应畸变系数设为零并固定。
        /// </remarks>
        public static FisheyeStereoCalibrationResult FisheyeStereoCalibrate(
            Point3f[][] objectPoints,
            Point2f[][] imagePoints1,
            Point2f[][] imagePoints2,
            Size imageSize,
            CalibrationFlags flags = CalibrationFlags.None,
            TermCriteria? criteria = null)
        {
            RejectOwnedFisheyeIntrinsicFlags(flags, true);
            var cameraMatrix1 = new Mat();
            var distCoeffs1 = new Mat();
            var cameraMatrix2 = new Mat();
            var distCoeffs2 = new Mat();
            var r = new Mat();
            var t = new Mat();
            try
            {
                double reprojectionError = FisheyeStereoCalibrate(
                    objectPoints,
                    imagePoints1,
                    imagePoints2,
                    cameraMatrix1,
                    distCoeffs1,
                    cameraMatrix2,
                    distCoeffs2,
                    imageSize,
                    r,
                    t,
                    flags,
                    criteria);
                return new FisheyeStereoCalibrationResult(
                    reprojectionError,
                    cameraMatrix1,
                    distCoeffs1,
                    cameraMatrix2,
                    distCoeffs2,
                    r,
                    t);
            }
            catch
            {
                cameraMatrix1.Dispose();
                distCoeffs1.Dispose();
                cameraMatrix2.Dispose();
                distCoeffs2.Dispose();
                r.Dispose();
                t.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Calibrates a fisheye stereo pair and writes per-view board poses in camera 1 coordinates.
        /// 标定鱼眼双目相机，并写入第一相机坐标系中的每视图标定板位姿。
        /// </summary>
        /// <remarks>
        /// In the fisheye model, <c>FixK1</c> through <c>FixK4</c> set the corresponding distortion
        /// coefficients to zero and keep them fixed; they do not preserve caller-provided values.
        /// 在鱼眼模型中，<c>FixK1</c> 至 <c>FixK4</c> 会将对应畸变系数设为零并固定，而不是保留调用方输入值。
        /// </remarks>
        public static double FisheyeStereoCalibrateExtended(
            Point3f[][] objectPoints,
            Point2f[][] imagePoints1,
            Point2f[][] imagePoints2,
            Mat cameraMatrix1,
            Mat distCoeffs1,
            Mat cameraMatrix2,
            Mat distCoeffs2,
            Size imageSize,
            Mat r,
            Mat t,
            Mat rvecs,
            Mat tvecs,
            CalibrationFlags flags = CalibrationFlags.FixIntrinsic,
            TermCriteria? criteria = null)
        {
            ValidateFisheyeStereoMats(cameraMatrix1, distCoeffs1, cameraMatrix2, distCoeffs2, r, t);
            ThrowIfNull(rvecs, nameof(rvecs));
            ThrowIfNull(tvecs, nameof(tvecs));
            ValidatePositiveSize(imageSize, nameof(imageSize));
            ValidateFisheyeFlags(flags, true);
            ValidateFisheyeInitialIntrinsics(cameraMatrix1, distCoeffs1, flags, nameof(cameraMatrix1));
            ValidateFisheyeInitialIntrinsics(cameraMatrix2, distCoeffs2, flags, nameof(cameraMatrix2));
            TermCriteria resolved = criteria ?? DefaultFisheyeCalibrationCriteria;
            ValidateRegistrationCriteria(resolved, nameof(criteria));

            PrepareStereoCalibrationPointGroups(
                objectPoints,
                imagePoints1,
                imagePoints2,
                out int[] objectOffsets,
                out NativeMethods.Calib3DPoint3fNative[] nativeObjectPoints,
                out int[] image1Offsets,
                out NativeMethods.Calib3DPoint2fNative[] nativeImagePoints1,
                out int[] image2Offsets,
                out NativeMethods.Calib3DPoint2fNative[] nativeImagePoints2);

            fixed (int* objectOffsetsPtr = objectOffsets)
            fixed (NativeMethods.Calib3DPoint3fNative* objectPointsPtr = nativeObjectPoints)
            fixed (int* image1OffsetsPtr = image1Offsets)
            fixed (NativeMethods.Calib3DPoint2fNative* imagePoints1Ptr = nativeImagePoints1)
            fixed (int* image2OffsetsPtr = image2Offsets)
            fixed (NativeMethods.Calib3DPoint2fNative* imagePoints2Ptr = nativeImagePoints2)
            {
                NativeException.ThrowIfError(NativeMethods.Calib3DFisheyeStereoCalibrateExtended(
                    objectOffsetsPtr,
                    objectPoints.Length,
                    objectPointsPtr,
                    nativeObjectPoints.Length,
                    image1OffsetsPtr,
                    imagePoints1.Length,
                    imagePoints1Ptr,
                    nativeImagePoints1.Length,
                    image2OffsetsPtr,
                    imagePoints2.Length,
                    imagePoints2Ptr,
                    nativeImagePoints2.Length,
                    cameraMatrix1.NativeHandle,
                    distCoeffs1.NativeHandle,
                    cameraMatrix2.NativeHandle,
                    distCoeffs2.NativeHandle,
                    imageSize.Width,
                    imageSize.Height,
                    r.NativeHandle,
                    t.NativeHandle,
                    rvecs.NativeHandle,
                    tvecs.NativeHandle,
                    (int)flags,
                    (int)resolved.Type,
                    resolved.MaxCount,
                    resolved.Epsilon,
                    out double reprojectionError));
                return reprojectionError;
            }
        }

        /// <summary>
        /// Calibrates a fisheye stereo pair and returns owned per-view board poses.
        /// 标定鱼眼双目相机并返回拥有所有权的每视图标定板位姿。
        /// </summary>
        /// <remarks>
        /// The caller must dispose every matrix in the result. In the fisheye model, <c>FixK1</c>
        /// through <c>FixK4</c> set the corresponding distortion coefficients to zero and keep them fixed.
        /// 调用方必须释放结果中的每个矩阵。在鱼眼模型中，<c>FixK1</c> 至 <c>FixK4</c>
        /// 会将对应畸变系数设为零并固定。
        /// </remarks>
        public static FisheyeStereoCalibrationExtendedResult FisheyeStereoCalibrateExtended(
            Point3f[][] objectPoints,
            Point2f[][] imagePoints1,
            Point2f[][] imagePoints2,
            Size imageSize,
            CalibrationFlags flags = CalibrationFlags.None,
            TermCriteria? criteria = null)
        {
            RejectOwnedFisheyeIntrinsicFlags(flags, true);
            var cameraMatrix1 = new Mat();
            var distCoeffs1 = new Mat();
            var cameraMatrix2 = new Mat();
            var distCoeffs2 = new Mat();
            var r = new Mat();
            var t = new Mat();
            var rvecs = new Mat();
            var tvecs = new Mat();
            try
            {
                double reprojectionError = FisheyeStereoCalibrateExtended(
                    objectPoints,
                    imagePoints1,
                    imagePoints2,
                    cameraMatrix1,
                    distCoeffs1,
                    cameraMatrix2,
                    distCoeffs2,
                    imageSize,
                    r,
                    t,
                    rvecs,
                    tvecs,
                    flags,
                    criteria);
                var calibration = new FisheyeStereoCalibrationResult(
                    reprojectionError,
                    cameraMatrix1,
                    distCoeffs1,
                    cameraMatrix2,
                    distCoeffs2,
                    r,
                    t);
                return new FisheyeStereoCalibrationExtendedResult(calibration, rvecs, tvecs);
            }
            catch
            {
                cameraMatrix1.Dispose();
                distCoeffs1.Dispose();
                cameraMatrix2.Dispose();
                distCoeffs2.Dispose();
                r.Dispose();
                t.Dispose();
                rvecs.Dispose();
                tvecs.Dispose();
                throw;
            }
        }

        private static void ValidateFisheyeSingleMats(Mat cameraMatrix, Mat distCoeffs, Mat rvecs, Mat tvecs)
        {
            ThrowIfNull(cameraMatrix, nameof(cameraMatrix));
            ThrowIfNull(distCoeffs, nameof(distCoeffs));
            ThrowIfNull(rvecs, nameof(rvecs));
            ThrowIfNull(tvecs, nameof(tvecs));
        }

        private static void ValidateFisheyeStereoMats(
            Mat cameraMatrix1,
            Mat distCoeffs1,
            Mat cameraMatrix2,
            Mat distCoeffs2,
            Mat r,
            Mat t)
        {
            ThrowIfNull(cameraMatrix1, nameof(cameraMatrix1));
            ThrowIfNull(distCoeffs1, nameof(distCoeffs1));
            ThrowIfNull(cameraMatrix2, nameof(cameraMatrix2));
            ThrowIfNull(distCoeffs2, nameof(distCoeffs2));
            ThrowIfNull(r, nameof(r));
            ThrowIfNull(t, nameof(t));
        }

        private static void ValidateFisheyeFlags(CalibrationFlags flags, bool stereo)
        {
            const CalibrationFlags singleSupported =
                CalibrationFlags.UseIntrinsicGuess |
                CalibrationFlags.FixPrincipalPoint |
                CalibrationFlags.FixFocalLength |
                CalibrationFlags.FixK1 |
                CalibrationFlags.FixK2 |
                CalibrationFlags.FixK3 |
                CalibrationFlags.FixK4 |
                CalibrationFlags.RecomputeExtrinsic |
                CalibrationFlags.CheckCond |
                CalibrationFlags.FixSkew;
            CalibrationFlags supported = stereo
                ? singleSupported | CalibrationFlags.FixIntrinsic
                : singleSupported;
            if ((flags & ~supported) != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(flags), "Unsupported fisheye calibration flag.");
            }
        }

        private static void ValidateFisheyeInitialIntrinsics(
            Mat cameraMatrix,
            Mat distCoeffs,
            CalibrationFlags flags,
            string parameterName)
        {
            if ((flags & (CalibrationFlags.UseIntrinsicGuess | CalibrationFlags.FixIntrinsic)) == 0)
            {
                return;
            }

            if (cameraMatrix.Rows != 3 || cameraMatrix.Cols != 3)
            {
                throw new ArgumentException("Initial fisheye camera matrix must be 3 x 3.", parameterName);
            }
            bool validDistortion =
                (distCoeffs.Rows == 4 && distCoeffs.Cols == 1) ||
                (distCoeffs.Rows == 1 && distCoeffs.Cols == 4);
            if (!validDistortion)
            {
                throw new ArgumentException("Initial fisheye distortion coefficients must contain exactly four values.", parameterName);
            }
        }

        private static void RejectOwnedFisheyeIntrinsicFlags(CalibrationFlags flags, bool stereo)
        {
            CalibrationFlags unsupported = stereo
                ? CalibrationFlags.UseIntrinsicGuess | CalibrationFlags.FixIntrinsic
                : CalibrationFlags.UseIntrinsicGuess;
            if ((flags & unsupported) != 0)
            {
                throw new ArgumentException(
                    "Use the caller-owned overload to provide initial fisheye intrinsics.",
                    nameof(flags));
            }
        }
    }
}
