using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Calib3D
{
    public static unsafe partial class Cv2
    {
        /// <summary>
        /// Registers two calibrated cameras and writes the relative transform and per-view errors.
        /// 注册两个已标定相机，并写入相对变换和每视图误差。
        /// </summary>
        public static double RegisterCameras(
            Point3f[][] objectPoints1,
            Point3f[][] objectPoints2,
            Point2f[][] imagePoints1,
            Point2f[][] imagePoints2,
            Mat cameraMatrix1,
            Mat distCoeffs1,
            CameraModel cameraModel1,
            Mat cameraMatrix2,
            Mat distCoeffs2,
            CameraModel cameraModel2,
            Mat r,
            Mat t,
            Mat e,
            Mat f,
            Mat perViewErrors,
            CalibrationFlags flags = CalibrationFlags.None,
            TermCriteria? criteria = null)
        {
            ValidateRegistrationMats(cameraMatrix1, distCoeffs1, cameraMatrix2, distCoeffs2, r, t, e, f, perViewErrors);
            ValidateCameraModel(cameraModel1, nameof(cameraModel1));
            ValidateCameraModel(cameraModel2, nameof(cameraModel2));
            TermCriteria resolved = criteria ?? DefaultStereoCalibrationCriteria;
            ValidateRegistrationCriteria(resolved, nameof(criteria));

            PrepareCameraRegistrationPointGroups(
                objectPoints1,
                objectPoints2,
                imagePoints1,
                imagePoints2,
                out int[] object1Offsets,
                out NativeMethods.Calib3DPoint3fNative[] nativeObjectPoints1,
                out int[] object2Offsets,
                out NativeMethods.Calib3DPoint3fNative[] nativeObjectPoints2,
                out int[] image1Offsets,
                out NativeMethods.Calib3DPoint2fNative[] nativeImagePoints1,
                out int[] image2Offsets,
                out NativeMethods.Calib3DPoint2fNative[] nativeImagePoints2);

            fixed (int* object1OffsetsPtr = object1Offsets)
            fixed (NativeMethods.Calib3DPoint3fNative* objectPoints1Ptr = nativeObjectPoints1)
            fixed (int* object2OffsetsPtr = object2Offsets)
            fixed (NativeMethods.Calib3DPoint3fNative* objectPoints2Ptr = nativeObjectPoints2)
            fixed (int* image1OffsetsPtr = image1Offsets)
            fixed (NativeMethods.Calib3DPoint2fNative* imagePoints1Ptr = nativeImagePoints1)
            fixed (int* image2OffsetsPtr = image2Offsets)
            fixed (NativeMethods.Calib3DPoint2fNative* imagePoints2Ptr = nativeImagePoints2)
            {
                NativeException.ThrowIfError(NativeMethods.Calib3DRegisterCameras(
                    object1OffsetsPtr,
                    objectPoints1.Length,
                    objectPoints1Ptr,
                    nativeObjectPoints1.Length,
                    object2OffsetsPtr,
                    objectPoints2.Length,
                    objectPoints2Ptr,
                    nativeObjectPoints2.Length,
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
                    (int)cameraModel1,
                    cameraMatrix2.NativeHandle,
                    distCoeffs2.NativeHandle,
                    (int)cameraModel2,
                    r.NativeHandle,
                    t.NativeHandle,
                    e.NativeHandle,
                    f.NativeHandle,
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
        /// Registers two calibrated cameras and returns owned output matrices.
        /// 注册两个已标定相机并返回拥有所有权的输出矩阵。
        /// </summary>
        /// <remarks>The caller must dispose every matrix in the returned result. 调用方必须释放返回结果中的每个矩阵。</remarks>
        public static CameraRegistrationResult RegisterCameras(
            Point3f[][] objectPoints1,
            Point3f[][] objectPoints2,
            Point2f[][] imagePoints1,
            Point2f[][] imagePoints2,
            Mat cameraMatrix1,
            Mat distCoeffs1,
            CameraModel cameraModel1,
            Mat cameraMatrix2,
            Mat distCoeffs2,
            CameraModel cameraModel2,
            CalibrationFlags flags = CalibrationFlags.None,
            TermCriteria? criteria = null)
        {
            RejectOwnedExtrinsicGuess(flags);
            var r = new Mat();
            var t = new Mat();
            var e = new Mat();
            var f = new Mat();
            var perViewErrors = new Mat();
            try
            {
                double reprojectionError = RegisterCameras(
                    objectPoints1,
                    objectPoints2,
                    imagePoints1,
                    imagePoints2,
                    cameraMatrix1,
                    distCoeffs1,
                    cameraModel1,
                    cameraMatrix2,
                    distCoeffs2,
                    cameraModel2,
                    r,
                    t,
                    e,
                    f,
                    perViewErrors,
                    flags,
                    criteria);
                return new CameraRegistrationResult(reprojectionError, r, t, e, f, perViewErrors);
            }
            catch
            {
                r.Dispose();
                t.Dispose();
                e.Dispose();
                f.Dispose();
                perViewErrors.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Registers two calibrated cameras and writes per-view poses and errors.
        /// 注册两个已标定相机，并写入每视图位姿和误差。
        /// </summary>
        public static double RegisterCamerasExtended(
            Point3f[][] objectPoints1,
            Point3f[][] objectPoints2,
            Point2f[][] imagePoints1,
            Point2f[][] imagePoints2,
            Mat cameraMatrix1,
            Mat distCoeffs1,
            CameraModel cameraModel1,
            Mat cameraMatrix2,
            Mat distCoeffs2,
            CameraModel cameraModel2,
            Mat r,
            Mat t,
            Mat e,
            Mat f,
            Mat rvecs,
            Mat tvecs,
            Mat perViewErrors,
            CalibrationFlags flags = CalibrationFlags.None,
            TermCriteria? criteria = null)
        {
            ValidateRegistrationMats(cameraMatrix1, distCoeffs1, cameraMatrix2, distCoeffs2, r, t, e, f, perViewErrors);
            ThrowIfNull(rvecs, nameof(rvecs));
            ThrowIfNull(tvecs, nameof(tvecs));
            ValidateCameraModel(cameraModel1, nameof(cameraModel1));
            ValidateCameraModel(cameraModel2, nameof(cameraModel2));
            TermCriteria resolved = criteria ?? DefaultStereoCalibrationCriteria;
            ValidateRegistrationCriteria(resolved, nameof(criteria));

            PrepareCameraRegistrationPointGroups(
                objectPoints1,
                objectPoints2,
                imagePoints1,
                imagePoints2,
                out int[] object1Offsets,
                out NativeMethods.Calib3DPoint3fNative[] nativeObjectPoints1,
                out int[] object2Offsets,
                out NativeMethods.Calib3DPoint3fNative[] nativeObjectPoints2,
                out int[] image1Offsets,
                out NativeMethods.Calib3DPoint2fNative[] nativeImagePoints1,
                out int[] image2Offsets,
                out NativeMethods.Calib3DPoint2fNative[] nativeImagePoints2);

            fixed (int* object1OffsetsPtr = object1Offsets)
            fixed (NativeMethods.Calib3DPoint3fNative* objectPoints1Ptr = nativeObjectPoints1)
            fixed (int* object2OffsetsPtr = object2Offsets)
            fixed (NativeMethods.Calib3DPoint3fNative* objectPoints2Ptr = nativeObjectPoints2)
            fixed (int* image1OffsetsPtr = image1Offsets)
            fixed (NativeMethods.Calib3DPoint2fNative* imagePoints1Ptr = nativeImagePoints1)
            fixed (int* image2OffsetsPtr = image2Offsets)
            fixed (NativeMethods.Calib3DPoint2fNative* imagePoints2Ptr = nativeImagePoints2)
            {
                NativeException.ThrowIfError(NativeMethods.Calib3DRegisterCamerasExtended(
                    object1OffsetsPtr,
                    objectPoints1.Length,
                    objectPoints1Ptr,
                    nativeObjectPoints1.Length,
                    object2OffsetsPtr,
                    objectPoints2.Length,
                    objectPoints2Ptr,
                    nativeObjectPoints2.Length,
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
                    (int)cameraModel1,
                    cameraMatrix2.NativeHandle,
                    distCoeffs2.NativeHandle,
                    (int)cameraModel2,
                    r.NativeHandle,
                    t.NativeHandle,
                    e.NativeHandle,
                    f.NativeHandle,
                    rvecs.NativeHandle,
                    tvecs.NativeHandle,
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
        /// Registers two calibrated cameras and returns owned extended outputs.
        /// 注册两个已标定相机并返回拥有所有权的扩展输出。
        /// </summary>
        /// <remarks>The caller must dispose every matrix in the returned result. 调用方必须释放返回结果中的每个矩阵。</remarks>
        public static CameraRegistrationExtendedResult RegisterCamerasExtended(
            Point3f[][] objectPoints1,
            Point3f[][] objectPoints2,
            Point2f[][] imagePoints1,
            Point2f[][] imagePoints2,
            Mat cameraMatrix1,
            Mat distCoeffs1,
            CameraModel cameraModel1,
            Mat cameraMatrix2,
            Mat distCoeffs2,
            CameraModel cameraModel2,
            CalibrationFlags flags = CalibrationFlags.None,
            TermCriteria? criteria = null)
        {
            RejectOwnedExtrinsicGuess(flags);
            var r = new Mat();
            var t = new Mat();
            var e = new Mat();
            var f = new Mat();
            var rvecs = new Mat();
            var tvecs = new Mat();
            var perViewErrors = new Mat();
            try
            {
                double reprojectionError = RegisterCamerasExtended(
                    objectPoints1,
                    objectPoints2,
                    imagePoints1,
                    imagePoints2,
                    cameraMatrix1,
                    distCoeffs1,
                    cameraModel1,
                    cameraMatrix2,
                    distCoeffs2,
                    cameraModel2,
                    r,
                    t,
                    e,
                    f,
                    rvecs,
                    tvecs,
                    perViewErrors,
                    flags,
                    criteria);
                var registration = new CameraRegistrationResult(reprojectionError, r, t, e, f, perViewErrors);
                return new CameraRegistrationExtendedResult(registration, rvecs, tvecs);
            }
            catch
            {
                r.Dispose();
                t.Dispose();
                e.Dispose();
                f.Dispose();
                rvecs.Dispose();
                tvecs.Dispose();
                perViewErrors.Dispose();
                throw;
            }
        }

        private static void PrepareCameraRegistrationPointGroups(
            Point3f[][] objectPoints1,
            Point3f[][] objectPoints2,
            Point2f[][] imagePoints1,
            Point2f[][] imagePoints2,
            out int[] object1Offsets,
            out NativeMethods.Calib3DPoint3fNative[] nativeObjectPoints1,
            out int[] object2Offsets,
            out NativeMethods.Calib3DPoint3fNative[] nativeObjectPoints2,
            out int[] image1Offsets,
            out NativeMethods.Calib3DPoint2fNative[] nativeImagePoints1,
            out int[] image2Offsets,
            out NativeMethods.Calib3DPoint2fNative[] nativeImagePoints2)
        {
            PrepareCalibrationPointGroups(
                objectPoints1,
                imagePoints1,
                nameof(objectPoints1),
                nameof(imagePoints1),
                out object1Offsets,
                out nativeObjectPoints1,
                out image1Offsets,
                out nativeImagePoints1);
            PrepareCalibrationPointGroups(
                objectPoints2,
                imagePoints2,
                nameof(objectPoints2),
                nameof(imagePoints2),
                out object2Offsets,
                out nativeObjectPoints2,
                out image2Offsets,
                out nativeImagePoints2);
            if (objectPoints2.Length != objectPoints1.Length)
            {
                throw new ArgumentException("Both cameras must contain the same number of frames.", nameof(objectPoints2));
            }
        }

        private static void ValidateRegistrationMats(
            Mat cameraMatrix1,
            Mat distCoeffs1,
            Mat cameraMatrix2,
            Mat distCoeffs2,
            Mat r,
            Mat t,
            Mat e,
            Mat f,
            Mat perViewErrors)
        {
            ThrowIfNull(cameraMatrix1, nameof(cameraMatrix1));
            ThrowIfNull(distCoeffs1, nameof(distCoeffs1));
            ThrowIfNull(cameraMatrix2, nameof(cameraMatrix2));
            ThrowIfNull(distCoeffs2, nameof(distCoeffs2));
            ThrowIfNull(r, nameof(r));
            ThrowIfNull(t, nameof(t));
            ThrowIfNull(e, nameof(e));
            ThrowIfNull(f, nameof(f));
            ThrowIfNull(perViewErrors, nameof(perViewErrors));
        }

        private static void ValidateCameraModel(CameraModel model, string parameterName)
        {
            if (model != CameraModel.Pinhole && model != CameraModel.Fisheye)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Camera model must be Pinhole or Fisheye.");
            }
        }

        private static void ValidateRegistrationCriteria(TermCriteria criteria, string parameterName)
        {
            int type = (int)criteria.Type;
            const int supportedTypes = 3;
            if (type == 0 || (type & ~supportedTypes) != 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Termination criteria type is invalid.");
            }
            if ((type & 1) != 0 && criteria.MaxCount <= 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Termination criteria count must be positive.");
            }
            if ((type & 2) != 0 &&
                (!(criteria.Epsilon > 0.0) || double.IsNaN(criteria.Epsilon) || double.IsInfinity(criteria.Epsilon)))
            {
                throw new ArgumentOutOfRangeException(parameterName, "Termination criteria epsilon must be finite and positive.");
            }
        }

        private static void RejectOwnedExtrinsicGuess(CalibrationFlags flags)
        {
            if ((flags & CalibrationFlags.UseExtrinsicGuess) != 0)
            {
                throw new ArgumentException(
                    "Use the caller-owned overload to supply initial R and T values with UseExtrinsicGuess.",
                    nameof(flags));
            }
        }
    }
}
