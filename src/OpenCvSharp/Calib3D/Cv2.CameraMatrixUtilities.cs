using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Calib3D
{
    public static partial class Cv2
    {
        /// <summary>
        /// Builds the default new camera matrix and writes it into a caller-owned Mat.
        /// 构建默认新相机矩阵并写入调用方拥有的 Mat。
        /// </summary>
        /// <param name="cameraMatrix">The input 3 x 3 camera matrix. 输入 3 x 3 相机矩阵。</param>
        /// <param name="newCameraMatrix">The caller-owned output matrix. 调用方拥有的输出矩阵。</param>
        /// <param name="imageSize">
        /// The image size used when centering the principal point. 仅在主点居中时使用的图像尺寸。
        /// </param>
        /// <param name="centerPrincipalPoint">
        /// Whether to move the principal point to the image center. 是否将主点移动到图像中心。
        /// </param>
        /// <remarks>
        /// The output is always <c>CV_64FC1</c>. The input and output Mats must not alias.
        /// 输出始终为 <c>CV_64FC1</c>，且输入和输出 Mat 不得别名。
        /// </remarks>
        public static void GetDefaultNewCameraMatrix(
            Mat cameraMatrix,
            Mat newCameraMatrix,
            Size imageSize = default,
            bool centerPrincipalPoint = false)
        {
            ThrowIfNull(cameraMatrix, nameof(cameraMatrix));
            ThrowIfNull(newCameraMatrix, nameof(newCameraMatrix));
            ValidateCameraUtilityMatrix(cameraMatrix, nameof(cameraMatrix));

            IntPtr cameraHandle = cameraMatrix.NativeHandle;
            IntPtr outputHandle = newCameraMatrix.NativeHandle;
            if (ReferenceEquals(cameraMatrix, newCameraMatrix) || cameraHandle == outputHandle)
            {
                throw new ArgumentException(
                    "The input and output camera matrices must not alias.",
                    nameof(newCameraMatrix));
            }

            if (centerPrincipalPoint)
            {
                ValidatePositiveCameraUtilitySize(imageSize, nameof(imageSize));
            }

            NativeException.ThrowIfError(NativeMethods.Calib3DGetDefaultNewCameraMatrix(
                cameraHandle,
                imageSize.Width,
                imageSize.Height,
                centerPrincipalPoint ? 1 : 0,
                outputHandle));
        }

        /// <summary>
        /// Builds and returns an independently owned default new camera matrix.
        /// 构建并返回独立拥有的默认新相机矩阵。
        /// </summary>
        /// <param name="cameraMatrix">The input 3 x 3 camera matrix. 输入 3 x 3 相机矩阵。</param>
        /// <param name="imageSize">
        /// The image size used when centering the principal point. 仅在主点居中时使用的图像尺寸。
        /// </param>
        /// <param name="centerPrincipalPoint">
        /// Whether to move the principal point to the image center. 是否将主点移动到图像中心。
        /// </param>
        /// <returns>An independently owned <c>3 x 3 CV_64FC1</c> matrix. 独立拥有的 <c>3 x 3 CV_64FC1</c> 矩阵。</returns>
        public static Mat GetDefaultNewCameraMatrix(
            Mat cameraMatrix,
            Size imageSize = default,
            bool centerPrincipalPoint = false)
        {
            var result = new Mat();
            try
            {
                GetDefaultNewCameraMatrix(
                    cameraMatrix,
                    result,
                    imageSize,
                    centerPrincipalPoint);
                return result;
            }
            catch
            {
                result.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Computes maximal-inscribed and minimal-bounding rectangles for an undistorted image plane.
        /// 计算无畸变图像平面的最大内接矩形和最小外接矩形。
        /// </summary>
        /// <param name="cameraMatrix">The input 3 x 3 camera matrix. 输入 3 x 3 相机矩阵。</param>
        /// <param name="distCoeffs">
        /// Distortion coefficients, or an empty Mat for zero distortion. 畸变系数，空 Mat 表示零畸变。
        /// </param>
        /// <param name="imageSize">The positive source image size. 正的源图像尺寸。</param>
        /// <param name="inner">The maximal inscribed rectangle. 最大内接矩形。</param>
        /// <param name="outer">The minimal bounding rectangle. 最小外接矩形。</param>
        /// <param name="r">Optional 3 x 3 rectification matrix. 可选 3 x 3 校正矩阵。</param>
        /// <param name="newCameraMatrix">
        /// Optional 3 x 3 camera matrix or 3 x 4 projection matrix. 可选 3 x 3 相机矩阵或 3 x 4 投影矩阵。
        /// </param>
        /// <remarks>
        /// Without <paramref name="newCameraMatrix"/>, rectangles use normalized undistorted
        /// coordinates. Supplying a camera or projection matrix returns coordinates in that
        /// projected image plane.
        /// 未提供 <paramref name="newCameraMatrix"/> 时返回归一化无畸变坐标；提供相机或投影矩阵时返回对应投影平面的坐标。
        /// </remarks>
        public static void GetUndistortRectangles(
            Mat cameraMatrix,
            Mat distCoeffs,
            Size imageSize,
            out Rect2d inner,
            out Rect2d outer,
            Mat? r = null,
            Mat? newCameraMatrix = null)
        {
            ThrowIfNull(cameraMatrix, nameof(cameraMatrix));
            ThrowIfNull(distCoeffs, nameof(distCoeffs));
            ValidateCameraUtilityMatrix(cameraMatrix, nameof(cameraMatrix));
            ValidatePinholeDistortionCoefficients(distCoeffs, nameof(distCoeffs));
            ValidatePositiveCameraUtilitySize(imageSize, nameof(imageSize));
            ValidateCameraUtilityRectification(r, nameof(r));
            ValidateCameraUtilityProjection(newCameraMatrix, nameof(newCameraMatrix));

            NativeException.ThrowIfError(NativeMethods.Calib3DGetUndistortRectangles(
                cameraMatrix.NativeHandle,
                distCoeffs.NativeHandle,
                GetNativeHandleOrZero(r),
                GetNativeHandleOrZero(newCameraMatrix),
                imageSize.Width,
                imageSize.Height,
                out double innerX,
                out double innerY,
                out double innerWidth,
                out double innerHeight,
                out double outerX,
                out double outerY,
                out double outerWidth,
                out double outerHeight));

            inner = new Rect2d(innerX, innerY, innerWidth, innerHeight);
            outer = new Rect2d(outerX, outerY, outerWidth, outerHeight);
        }

        private static void ValidateCameraUtilityMatrix(Mat value, string parameterName)
        {
            if (value.Empty)
            {
                throw new ArgumentException("Camera matrix cannot be empty.", parameterName);
            }
            if (value.Rows != 3 || value.Cols != 3 || value.Channels != 1)
            {
                throw new ArgumentException(
                    "Camera matrix must be 3 x 3 and single-channel.",
                    parameterName);
            }
            ValidateCameraUtilityFloatingDepth(value, parameterName);
        }

        private static void ValidatePinholeDistortionCoefficients(Mat value, string parameterName)
        {
            if (value.Empty)
            {
                return;
            }
            if (value.Channels != 1 || (value.Rows != 1 && value.Cols != 1))
            {
                throw new ArgumentException(
                    "Distortion coefficients must be a single-channel vector.",
                    parameterName);
            }
            ValidateCameraUtilityFloatingDepth(value, parameterName);

            int count = checked(value.Rows * value.Cols);
            if (count != 4 && count != 5 && count != 8 && count != 12 && count != 14)
            {
                throw new ArgumentException(
                    "Distortion coefficients must contain 4, 5, 8, 12, or 14 values.",
                    parameterName);
            }
        }

        private static void ValidateCameraUtilityRectification(Mat? value, string parameterName)
        {
            if (value == null || value.Empty)
            {
                return;
            }
            if (value.Rows != 3 || value.Cols != 3 || value.Channels != 1)
            {
                throw new ArgumentException(
                    "Rectification matrix must be 3 x 3 and single-channel.",
                    parameterName);
            }
            ValidateCameraUtilityFloatingDepth(value, parameterName);
        }

        private static void ValidateCameraUtilityProjection(Mat? value, string parameterName)
        {
            if (value == null || value.Empty)
            {
                return;
            }
            if (value.Rows != 3 ||
                (value.Cols != 3 && value.Cols != 4) ||
                value.Channels != 1)
            {
                throw new ArgumentException(
                    "New camera or projection matrix must be 3 x 3 or 3 x 4 and single-channel.",
                    parameterName);
            }
            ValidateCameraUtilityFloatingDepth(value, parameterName);
        }

        private static void ValidateCameraUtilityFloatingDepth(Mat value, string parameterName)
        {
            if (value.Depth != MatType.CV_32F && value.Depth != MatType.CV_64F)
            {
                throw new ArgumentException(
                    "Matrix depth must be CV_32F or CV_64F.",
                    parameterName);
            }
        }

        private static void ValidatePositiveCameraUtilitySize(Size value, string parameterName)
        {
            if (value.Width <= 0 || value.Height <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    "Image size must have positive width and height.");
            }
        }
    }
}
