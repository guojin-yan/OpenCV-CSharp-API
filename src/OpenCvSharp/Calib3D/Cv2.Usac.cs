using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Calib3D
{
    public static unsafe partial class Cv2
    {
        /// <summary>Finds a homography with an explicit USAC configuration. 使用显式 USAC 配置估计单应矩阵。</summary>
        public static Mat FindHomography(Mat srcPoints, Mat dstPoints, Mat? mask, UsacParams parameters)
        {
            ThrowIfNull(srcPoints, nameof(srcPoints));
            ThrowIfNull(dstPoints, nameof(dstPoints));
            ThrowIfNull(parameters, nameof(parameters));
            NativeMethods.Calib3DUsacParamsNative nativeParameters = parameters.ToNative();
            NativeException.ThrowIfError(NativeMethods.Calib3DFindHomographyUsac(
                srcPoints.NativeHandle,
                dstPoints.NativeHandle,
                GetNativeHandleOrZero(mask),
                &nativeParameters,
                out IntPtr homography));
            return new Mat(homography);
        }

        /// <summary>Solves a pose with the USAC PnP RANSAC estimator. 使用 USAC PnP RANSAC 估计器求解位姿。</summary>
        public static bool SolvePnPRansac(
            Mat objectPoints,
            Mat imagePoints,
            Mat cameraMatrix,
            Mat distCoeffs,
            Mat rvec,
            Mat tvec,
            Mat inliers,
            UsacParams parameters)
        {
            ThrowIfNull(objectPoints, nameof(objectPoints));
            ThrowIfNull(imagePoints, nameof(imagePoints));
            ThrowIfNull(cameraMatrix, nameof(cameraMatrix));
            ThrowIfNull(distCoeffs, nameof(distCoeffs));
            ThrowIfNull(rvec, nameof(rvec));
            ThrowIfNull(tvec, nameof(tvec));
            ThrowIfNull(inliers, nameof(inliers));
            ThrowIfNull(parameters, nameof(parameters));
            NativeMethods.Calib3DUsacParamsNative nativeParameters = parameters.ToNative();
            NativeException.ThrowIfError(NativeMethods.Calib3DSolvePnPRansacUsac(
                objectPoints.NativeHandle,
                imagePoints.NativeHandle,
                cameraMatrix.NativeHandle,
                distCoeffs.NativeHandle,
                rvec.NativeHandle,
                tvec.NativeHandle,
                inliers.NativeHandle,
                &nativeParameters,
                out int solved));
            return solved != 0;
        }

        /// <summary>Finds a fundamental matrix with an explicit USAC configuration. 使用显式 USAC 配置估计基础矩阵。</summary>
        public static Mat FindFundamentalMat(Mat points1, Mat points2, Mat? mask, UsacParams parameters)
        {
            ThrowIfNull(points1, nameof(points1));
            ThrowIfNull(points2, nameof(points2));
            ThrowIfNull(parameters, nameof(parameters));
            NativeMethods.Calib3DUsacParamsNative nativeParameters = parameters.ToNative();
            NativeException.ThrowIfError(NativeMethods.Calib3DFindFundamentalMatUsac(
                points1.NativeHandle,
                points2.NativeHandle,
                GetNativeHandleOrZero(mask),
                &nativeParameters,
                out IntPtr fundamental));
            return new Mat(fundamental);
        }

        /// <summary>Finds an essential matrix for two calibrated cameras with an explicit USAC configuration. 使用显式 USAC 配置估计两台已标定相机的本质矩阵。</summary>
        public static Mat FindEssentialMat(
            Mat points1,
            Mat points2,
            Mat cameraMatrix1,
            Mat cameraMatrix2,
            Mat distCoeffs1,
            Mat distCoeffs2,
            Mat? mask,
            UsacParams parameters)
        {
            ThrowIfNull(points1, nameof(points1));
            ThrowIfNull(points2, nameof(points2));
            ThrowIfNull(cameraMatrix1, nameof(cameraMatrix1));
            ThrowIfNull(cameraMatrix2, nameof(cameraMatrix2));
            ThrowIfNull(distCoeffs1, nameof(distCoeffs1));
            ThrowIfNull(distCoeffs2, nameof(distCoeffs2));
            ThrowIfNull(parameters, nameof(parameters));
            NativeMethods.Calib3DUsacParamsNative nativeParameters = parameters.ToNative();
            NativeException.ThrowIfError(NativeMethods.Calib3DFindEssentialMatUsac(
                points1.NativeHandle,
                points2.NativeHandle,
                cameraMatrix1.NativeHandle,
                cameraMatrix2.NativeHandle,
                distCoeffs1.NativeHandle,
                distCoeffs2.NativeHandle,
                GetNativeHandleOrZero(mask),
                &nativeParameters,
                out IntPtr essential));
            return new Mat(essential);
        }

        /// <summary>Estimates a full 2D affine transform with an explicit USAC configuration. 使用显式 USAC 配置估计完整二维仿射变换。</summary>
        public static Mat EstimateAffine2D(Mat source, Mat destination, Mat inliers, UsacParams parameters)
        {
            ThrowIfNull(source, nameof(source));
            ThrowIfNull(destination, nameof(destination));
            ThrowIfNull(inliers, nameof(inliers));
            ThrowIfNull(parameters, nameof(parameters));
            NativeMethods.Calib3DUsacParamsNative nativeParameters = parameters.ToNative();
            var transform = new Mat();
            try
            {
                NativeException.ThrowIfError(NativeMethods.Calib3DEstimateAffine2DUsac(
                    source.NativeHandle,
                    destination.NativeHandle,
                    transform.NativeHandle,
                    inliers.NativeHandle,
                    &nativeParameters));
                return transform;
            }
            catch
            {
                transform.Dispose();
                throw;
            }
        }
    }
}
