using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Calib3D
{
    public static partial class Cv2
    {
        /// <summary>Removes lens distortion from an image. 去除图像镜头畸变。</summary>
        public static void Undistort(Mat src, Mat dst, Mat cameraMatrix, Mat distCoeffs, Mat? newCameraMatrix = null)
        {
            ThrowIfNull(src, nameof(src));
            ThrowIfNull(dst, nameof(dst));
            ThrowIfNull(cameraMatrix, nameof(cameraMatrix));
            ThrowIfNull(distCoeffs, nameof(distCoeffs));
            NativeException.ThrowIfError(NativeMethods.ImgProcUndistort(
                src.NativeHandle,
                dst.NativeHandle,
                cameraMatrix.NativeHandle,
                distCoeffs.NativeHandle,
                GetNativeHandleOrZero(newCameraMatrix)));
        }

        /// <summary>Removes lens distortion and returns an owned image. 去除镜头畸变并返回拥有所有权的图像。</summary>
        public static Mat Undistort(Mat src, Mat cameraMatrix, Mat distCoeffs, Mat? newCameraMatrix = null)
        {
            var dst = new Mat();
            try
            {
                Undistort(src, dst, cameraMatrix, distCoeffs, newCameraMatrix);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        /// <summary>Builds inverse rectification maps for projector-camera pairs. 为投影仪-相机组合构建逆校正映射。</summary>
        public static void InitInverseRectificationMap(
            Mat cameraMatrix,
            Mat distCoeffs,
            Mat r,
            Mat newCameraMatrix,
            Size size,
            int m1type,
            Mat map1,
            Mat map2)
        {
            ThrowIfNull(cameraMatrix, nameof(cameraMatrix));
            ThrowIfNull(distCoeffs, nameof(distCoeffs));
            ThrowIfNull(r, nameof(r));
            ThrowIfNull(newCameraMatrix, nameof(newCameraMatrix));
            ThrowIfNull(map1, nameof(map1));
            ThrowIfNull(map2, nameof(map2));
            ValidatePositiveImageSize(size, nameof(size));
            NativeException.ThrowIfError(NativeMethods.ImgProcInitInverseRectificationMap(
                cameraMatrix.NativeHandle,
                distCoeffs.NativeHandle,
                r.NativeHandle,
                newCameraMatrix.NativeHandle,
                size.Width,
                size.Height,
                m1type,
                map1.NativeHandle,
                map2.NativeHandle));
        }

        /// <summary>Builds and returns owned inverse rectification maps. 构建并返回拥有所有权的逆校正映射。</summary>
        public static UndistortRectifyMapResult InitInverseRectificationMap(
            Mat cameraMatrix,
            Mat distCoeffs,
            Mat r,
            Mat newCameraMatrix,
            Size size,
            int m1type)
        {
            var map1 = new Mat();
            var map2 = new Mat();
            try
            {
                InitInverseRectificationMap(cameraMatrix, distCoeffs, r, newCameraMatrix, size, m1type, map1, map2);
                return new UndistortRectifyMapResult(map1, map2);
            }
            catch
            {
                map1.Dispose();
                map2.Dispose();
                throw;
            }
        }

        /// <summary>Undistorts an image using the fisheye camera model. 使用鱼眼相机模型校正图像。</summary>
        public static void FisheyeUndistortImage(
            Mat distorted,
            Mat undistorted,
            Mat cameraMatrix,
            Mat distCoeffs,
            Mat? newCameraMatrix = null,
            Size? newSize = null)
        {
            ThrowIfNull(distorted, nameof(distorted));
            ThrowIfNull(undistorted, nameof(undistorted));
            ThrowIfNull(cameraMatrix, nameof(cameraMatrix));
            ThrowIfNull(distCoeffs, nameof(distCoeffs));
            Size size = newSize ?? new Size();
            if (size.Width < 0 || size.Height < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(newSize), "Image dimensions cannot be negative.");
            }

            NativeException.ThrowIfError(NativeMethods.ImgProcFisheyeUndistortImage(
                distorted.NativeHandle,
                undistorted.NativeHandle,
                cameraMatrix.NativeHandle,
                distCoeffs.NativeHandle,
                GetNativeHandleOrZero(newCameraMatrix),
                size.Width,
                size.Height));
        }

        /// <summary>Undistorts a fisheye image and returns an owned result. 校正鱼眼图像并返回拥有所有权的结果。</summary>
        public static Mat FisheyeUndistortImage(
            Mat distorted,
            Mat cameraMatrix,
            Mat distCoeffs,
            Mat? newCameraMatrix = null,
            Size? newSize = null)
        {
            var undistorted = new Mat();
            try
            {
                FisheyeUndistortImage(distorted, undistorted, cameraMatrix, distCoeffs, newCameraMatrix, newSize);
                return undistorted;
            }
            catch
            {
                undistorted.Dispose();
                throw;
            }
        }

        /// <summary>Draws pose-estimation coordinate axes on an image. 在图像上绘制位姿估计坐标轴。</summary>
        public static void DrawFrameAxes(
            Mat image,
            Mat cameraMatrix,
            Mat distCoeffs,
            Mat rotationVector,
            Mat translationVector,
            float length,
            int thickness = 3)
        {
            ThrowIfNull(image, nameof(image));
            ThrowIfNull(cameraMatrix, nameof(cameraMatrix));
            ThrowIfNull(distCoeffs, nameof(distCoeffs));
            ThrowIfNull(rotationVector, nameof(rotationVector));
            ThrowIfNull(translationVector, nameof(translationVector));
            if (!(length > 0.0F) || float.IsNaN(length) || float.IsInfinity(length))
            {
                throw new ArgumentOutOfRangeException(nameof(length), "Length must be finite and positive.");
            }

            if (thickness <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(thickness), "Thickness must be positive.");
            }

            NativeException.ThrowIfError(NativeMethods.ImgProcDrawFrameAxes(
                image.NativeHandle,
                cameraMatrix.NativeHandle,
                distCoeffs.NativeHandle,
                rotationVector.NativeHandle,
                translationVector.NativeHandle,
                length,
                thickness));
        }

        private static void ValidatePositiveImageSize(Size size, string parameterName)
        {
            if (size.Width <= 0 || size.Height <= 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Image dimensions must be positive.");
            }
        }
    }
}
