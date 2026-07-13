using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Calib3D
{
    public static unsafe partial class Cv2
    {
        /// <summary>
        /// Calibrates a camera and optionally refines calibration target coordinates.
        /// 标定相机，并可选择精炼标定目标坐标。
        /// </summary>
        /// <remarks>
        /// A fixed-point index in <c>[1, objectPoints[0].Length - 2]</c> enables object-releasing calibration.
        /// A value outside that range selects standard camera calibration and leaves <paramref name="newObjectPoints"/> empty.
        /// 固定点索引位于 <c>[1, objectPoints[0].Length - 2]</c> 时启用释放物点标定；范围外值选择标准标定，并使
        /// <paramref name="newObjectPoints"/> 保持为空。
        /// </remarks>
        public static double CalibrateCameraRO(
            Point3f[][] objectPoints,
            Point2f[][] imagePoints,
            Size imageSize,
            int iFixedPoint,
            Mat cameraMatrix,
            Mat distCoeffs,
            Mat rvecs,
            Mat tvecs,
            Mat newObjectPoints,
            CalibrationFlags flags = CalibrationFlags.None,
            TermCriteria? criteria = null)
        {
            ThrowIfNull(cameraMatrix, nameof(cameraMatrix));
            ThrowIfNull(distCoeffs, nameof(distCoeffs));
            ThrowIfNull(rvecs, nameof(rvecs));
            ThrowIfNull(tvecs, nameof(tvecs));
            ThrowIfNull(newObjectPoints, nameof(newObjectPoints));
            ValidatePositiveSize(imageSize, nameof(imageSize));

            PrepareCalibrationPointGroups(
                objectPoints,
                imagePoints,
                nameof(objectPoints),
                nameof(imagePoints),
                out int[] objectOffsets,
                out NativeMethods.Calib3DPoint3fNative[] nativeObjectPoints,
                out int[] imageOffsets,
                out NativeMethods.Calib3DPoint2fNative[] nativeImagePoints);
            TermCriteria resolved = criteria ?? DefaultCalibrationCriteria;

            fixed (int* objectOffsetsPtr = objectOffsets)
            fixed (NativeMethods.Calib3DPoint3fNative* objectPointsPtr = nativeObjectPoints)
            fixed (int* imageOffsetsPtr = imageOffsets)
            fixed (NativeMethods.Calib3DPoint2fNative* imagePointsPtr = nativeImagePoints)
            {
                NativeException.ThrowIfError(NativeMethods.Calib3DCalibrateCameraRO(
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
                    iFixedPoint,
                    cameraMatrix.NativeHandle,
                    distCoeffs.NativeHandle,
                    rvecs.NativeHandle,
                    tvecs.NativeHandle,
                    newObjectPoints.NativeHandle,
                    (int)flags,
                    (int)resolved.Type,
                    resolved.MaxCount,
                    resolved.Epsilon,
                    out double reprojectionError));
                return reprojectionError;
            }
        }

        /// <summary>
        /// Calibrates a camera with optional object release and returns owned output matrices.
        /// 使用可选释放物点方法标定相机，并返回拥有所有权的输出矩阵。
        /// </summary>
        public static CalibrationROResult CalibrateCameraRO(
            Point3f[][] objectPoints,
            Point2f[][] imagePoints,
            Size imageSize,
            int iFixedPoint,
            CalibrationFlags flags = CalibrationFlags.None,
            TermCriteria? criteria = null)
        {
            var cameraMatrix = new Mat();
            var distCoeffs = new Mat();
            var rvecs = new Mat();
            var tvecs = new Mat();
            var newObjectPoints = new Mat();
            try
            {
                double reprojectionError = CalibrateCameraRO(
                    objectPoints,
                    imagePoints,
                    imageSize,
                    iFixedPoint,
                    cameraMatrix,
                    distCoeffs,
                    rvecs,
                    tvecs,
                    newObjectPoints,
                    flags,
                    criteria);
                var calibration = new CalibrationResult(reprojectionError, cameraMatrix, distCoeffs, rvecs, tvecs);
                return new CalibrationROResult(calibration, newObjectPoints);
            }
            catch
            {
                cameraMatrix.Dispose();
                distCoeffs.Dispose();
                rvecs.Dispose();
                tvecs.Dispose();
                newObjectPoints.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Calibrates a camera with object-coordinate refinement and uncertainty outputs.
        /// 使用物点坐标精炼及不确定度输出执行相机标定。
        /// </summary>
        public static double CalibrateCameraROExtended(
            Point3f[][] objectPoints,
            Point2f[][] imagePoints,
            Size imageSize,
            int iFixedPoint,
            Mat cameraMatrix,
            Mat distCoeffs,
            Mat rvecs,
            Mat tvecs,
            Mat newObjectPoints,
            Mat stdDeviationsIntrinsics,
            Mat stdDeviationsExtrinsics,
            Mat stdDeviationsObjectPoints,
            Mat perViewErrors,
            CalibrationFlags flags = CalibrationFlags.None,
            TermCriteria? criteria = null)
        {
            ThrowIfNull(cameraMatrix, nameof(cameraMatrix));
            ThrowIfNull(distCoeffs, nameof(distCoeffs));
            ThrowIfNull(rvecs, nameof(rvecs));
            ThrowIfNull(tvecs, nameof(tvecs));
            ThrowIfNull(newObjectPoints, nameof(newObjectPoints));
            ThrowIfNull(stdDeviationsIntrinsics, nameof(stdDeviationsIntrinsics));
            ThrowIfNull(stdDeviationsExtrinsics, nameof(stdDeviationsExtrinsics));
            ThrowIfNull(stdDeviationsObjectPoints, nameof(stdDeviationsObjectPoints));
            ThrowIfNull(perViewErrors, nameof(perViewErrors));
            ValidatePositiveSize(imageSize, nameof(imageSize));

            PrepareCalibrationPointGroups(
                objectPoints,
                imagePoints,
                nameof(objectPoints),
                nameof(imagePoints),
                out int[] objectOffsets,
                out NativeMethods.Calib3DPoint3fNative[] nativeObjectPoints,
                out int[] imageOffsets,
                out NativeMethods.Calib3DPoint2fNative[] nativeImagePoints);
            TermCriteria resolved = criteria ?? DefaultCalibrationCriteria;

            fixed (int* objectOffsetsPtr = objectOffsets)
            fixed (NativeMethods.Calib3DPoint3fNative* objectPointsPtr = nativeObjectPoints)
            fixed (int* imageOffsetsPtr = imageOffsets)
            fixed (NativeMethods.Calib3DPoint2fNative* imagePointsPtr = nativeImagePoints)
            {
                NativeException.ThrowIfError(NativeMethods.Calib3DCalibrateCameraROExtended(
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
                    iFixedPoint,
                    cameraMatrix.NativeHandle,
                    distCoeffs.NativeHandle,
                    rvecs.NativeHandle,
                    tvecs.NativeHandle,
                    newObjectPoints.NativeHandle,
                    stdDeviationsIntrinsics.NativeHandle,
                    stdDeviationsExtrinsics.NativeHandle,
                    stdDeviationsObjectPoints.NativeHandle,
                    perViewErrors.NativeHandle,
                    (int)flags,
                    (int)resolved.Type,
                    resolved.MaxCount,
                    resolved.Epsilon,
                    out double reprojectionError));
                return reprojectionError;
            }
        }

        /// <summary>
        /// Calibrates a camera with object-coordinate refinement and returns owned uncertainty outputs.
        /// 使用物点坐标精炼执行相机标定，并返回拥有所有权的不确定度输出。
        /// </summary>
        public static CalibrationROExtendedResult CalibrateCameraROExtended(
            Point3f[][] objectPoints,
            Point2f[][] imagePoints,
            Size imageSize,
            int iFixedPoint,
            CalibrationFlags flags = CalibrationFlags.None,
            TermCriteria? criteria = null)
        {
            var cameraMatrix = new Mat();
            var distCoeffs = new Mat();
            var rvecs = new Mat();
            var tvecs = new Mat();
            var newObjectPoints = new Mat();
            var stdDeviationsIntrinsics = new Mat();
            var stdDeviationsExtrinsics = new Mat();
            var stdDeviationsObjectPoints = new Mat();
            var perViewErrors = new Mat();
            try
            {
                double reprojectionError = CalibrateCameraROExtended(
                    objectPoints,
                    imagePoints,
                    imageSize,
                    iFixedPoint,
                    cameraMatrix,
                    distCoeffs,
                    rvecs,
                    tvecs,
                    newObjectPoints,
                    stdDeviationsIntrinsics,
                    stdDeviationsExtrinsics,
                    stdDeviationsObjectPoints,
                    perViewErrors,
                    flags,
                    criteria);
                var calibration = new CalibrationResult(reprojectionError, cameraMatrix, distCoeffs, rvecs, tvecs);
                var roCalibration = new CalibrationROResult(calibration, newObjectPoints);
                return new CalibrationROExtendedResult(
                    roCalibration,
                    stdDeviationsIntrinsics,
                    stdDeviationsExtrinsics,
                    stdDeviationsObjectPoints,
                    perViewErrors);
            }
            catch
            {
                cameraMatrix.Dispose();
                distCoeffs.Dispose();
                rvecs.Dispose();
                tvecs.Dispose();
                newObjectPoints.Dispose();
                stdDeviationsIntrinsics.Dispose();
                stdDeviationsExtrinsics.Dispose();
                stdDeviationsObjectPoints.Dispose();
                perViewErrors.Dispose();
                throw;
            }
        }
    }
}
