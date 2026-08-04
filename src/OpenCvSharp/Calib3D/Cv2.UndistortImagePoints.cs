using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Calib3D
{
    public static partial class Cv2
    {
        private static readonly TermCriteria DefaultUndistortImagePointsCriteria =
            new TermCriteria(TermCriteriaTypes.Count, 5, 0.01);

        /// <summary>
        /// Computes undistorted image-point positions and writes them into a caller-owned Mat.
        /// 计算无畸变像点位置并写入调用方拥有的 Mat。
        /// </summary>
        /// <param name="src">Observed distorted image points in pixel coordinates. 以像素坐标表示的畸变像点。</param>
        /// <param name="dst">The caller-owned output point matrix. 调用方拥有的输出点矩阵。</param>
        /// <param name="cameraMatrix">The 3 x 3 camera intrinsic matrix. 3 x 3 相机内参矩阵。</param>
        /// <param name="distCoeffs">
        /// Distortion coefficients, or an empty Mat for zero distortion. 畸变系数，空 Mat 表示零畸变。
        /// </param>
        /// <param name="criteria">The iterative undistortion termination criteria. 迭代去畸变终止条件。</param>
        /// <remarks>
        /// Unlike <see cref="UndistortPoints(Mat, Mat, Mat, Mat, Mat?, Mat?, TermCriteria?)"/>,
        /// this method always returns pixel coordinates by applying the original camera matrix
        /// after undistortion. The output preserves the source point depth.
        /// 与 <see cref="UndistortPoints(Mat, Mat, Mat, Mat, Mat?, Mat?, TermCriteria?)"/> 不同，
        /// 本方法在去畸变后固定应用原相机矩阵，因此始终返回像素坐标，并保留源点深度。
        /// </remarks>
        public static void UndistortImagePoints(
            Mat src,
            Mat dst,
            Mat cameraMatrix,
            Mat distCoeffs,
            TermCriteria? criteria = null)
        {
            ThrowIfNull(src, nameof(src));
            ThrowIfNull(dst, nameof(dst));
            ThrowIfNull(cameraMatrix, nameof(cameraMatrix));
            ThrowIfNull(distCoeffs, nameof(distCoeffs));
            ValidateUndistortImagePointMat(src, nameof(src));
            ValidateCameraUtilityMatrix(cameraMatrix, nameof(cameraMatrix));
            ValidatePinholeDistortionCoefficients(distCoeffs, nameof(distCoeffs));

            TermCriteria resolved = criteria ?? DefaultUndistortImagePointsCriteria;
            ValidateRegistrationCriteria(resolved, nameof(criteria));

            NativeException.ThrowIfError(NativeMethods.Calib3DUndistortImagePoints(
                src.NativeHandle,
                dst.NativeHandle,
                cameraMatrix.NativeHandle,
                distCoeffs.NativeHandle,
                (int)resolved.Type,
                resolved.MaxCount,
                resolved.Epsilon));
        }

        /// <summary>
        /// Computes undistorted image-point positions and returns an owned point matrix.
        /// 计算无畸变像点位置并返回拥有所有权的点矩阵。
        /// </summary>
        public static Mat UndistortImagePoints(
            Mat src,
            Mat cameraMatrix,
            Mat distCoeffs,
            TermCriteria? criteria = null)
        {
            var result = new Mat();
            try
            {
                UndistortImagePoints(src, result, cameraMatrix, distCoeffs, criteria);
                return result;
            }
            catch
            {
                result.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Computes undistorted positions for managed single-precision image points.
        /// 计算托管单精度像点的无畸变位置。
        /// </summary>
        public static Mat UndistortImagePoints(
            Point2f[] src,
            Mat cameraMatrix,
            Mat distCoeffs,
            TermCriteria? criteria = null)
        {
            ValidatePointArray(src, nameof(src));
            using (Mat srcMat = ToPointMat(src))
            {
                return UndistortImagePoints(srcMat, cameraMatrix, distCoeffs, criteria);
            }
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Computes undistorted positions for a span of single-precision image points.
        /// 计算单精度像点 Span 的无畸变位置。
        /// </summary>
        public static Mat UndistortImagePoints(
            ReadOnlySpan<Point2f> src,
            Mat cameraMatrix,
            Mat distCoeffs,
            TermCriteria? criteria = null)
        {
            ValidatePointSpan(src, nameof(src));
            using (Mat srcMat = ToPointMat(src))
            {
                return UndistortImagePoints(srcMat, cameraMatrix, distCoeffs, criteria);
            }
        }
#endif

        private static void ValidateUndistortImagePointMat(Mat value, string parameterName)
        {
            if (value.Empty)
            {
                throw new ArgumentException("Point matrix cannot be empty.", parameterName);
            }
            if (value.Depth != MatType.CV_32F && value.Depth != MatType.CV_64F)
            {
                throw new ArgumentException(
                    "Point matrix depth must be CV_32F or CV_64F.",
                    parameterName);
            }

            bool channelVector =
                value.Channels == 2 &&
                (value.Rows == 1 || value.Cols == 1);
            bool scalarMatrix =
                value.Channels == 1 &&
                (value.Rows == 2 || value.Cols == 2);
            if (!channelVector && !scalarMatrix)
            {
                throw new ArgumentException(
                    "Point matrix must be 2 x N or N x 2 single-channel, or a two-channel vector.",
                    parameterName);
            }
        }
    }
}
