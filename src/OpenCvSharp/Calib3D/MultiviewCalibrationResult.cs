using System;
using System.Globalization;
using JYPPX.OpenCvSharp.Core;

namespace JYPPX.OpenCvSharp.Calib3D
{
    /// <summary>
    /// Owned result returned by multi-camera calibration.
    /// 多相机标定返回的拥有所有权的结果。
    /// </summary>
    public readonly struct MultiviewCalibrationResult
    {
        /// <summary>
        /// Initializes a multi-camera calibration result.
        /// 初始化多相机标定结果。
        /// </summary>
        public MultiviewCalibrationResult(
            double reprojectionError,
            Mat[] cameraMatrices,
            Mat[] distCoeffs,
            Mat[] rotationVectors,
            Mat[] translationVectors)
        {
            CameraMatrices = ValidateCameraMatrices(cameraMatrices, nameof(cameraMatrices));
            DistCoeffs = ValidateArray(distCoeffs, nameof(distCoeffs), CameraMatrices.Length);
            RotationVectors = ValidateArray(rotationVectors, nameof(rotationVectors), CameraMatrices.Length);
            TranslationVectors = ValidateArray(translationVectors, nameof(translationVectors), CameraMatrices.Length);

            for (int i = 0; i < CameraMatrices.Length; ++i)
            {
                ValidateShape(CameraMatrices[i], 3, 3, nameof(cameraMatrices));
                ValidateShape(RotationVectors[i], 3, 1, nameof(rotationVectors));
                ValidateShape(TranslationVectors[i], 3, 1, nameof(translationVectors));
            }

            ReprojectionError = reprojectionError;
        }

        /// <summary>Gets the overall RMS reprojection error. 获取整体 RMS 重投影误差。</summary>
        public double ReprojectionError { get; }

        /// <summary>Gets one intrinsic matrix per camera. 获取每台相机的内参矩阵。</summary>
        public Mat[] CameraMatrices { get; }

        /// <summary>Gets one distortion matrix per camera. 获取每台相机的畸变矩阵。</summary>
        public Mat[] DistCoeffs { get; }

        /// <summary>Gets camera rotation vectors relative to camera 0. 获取相对相机 0 的旋转向量。</summary>
        public Mat[] RotationVectors { get; }

        /// <summary>Gets camera translation vectors relative to camera 0. 获取相对相机 0 的平移向量。</summary>
        public Mat[] TranslationVectors { get; }

        /// <summary>Gets the camera count. 获取相机数量。</summary>
        public int CameraCount
        {
            get { return CameraMatrices.Length; }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "{ReprojectionError=" + ReprojectionError.ToString(CultureInfo.InvariantCulture)
                + ",CameraCount=" + CameraCount
                + "}";
        }

        private static Mat[] ValidateArray(Mat[] values, string parameterName, int expectedCount)
        {
            if (values == null)
            {
                throw new ArgumentNullException(parameterName);
            }
            if (values.Length != expectedCount)
            {
                throw new ArgumentException("Matrix array length is invalid.", parameterName);
            }
            for (int i = 0; i < values.Length; ++i)
            {
                if (values[i] == null)
                {
                    throw new ArgumentNullException(parameterName);
                }
            }
            return values;
        }

        private static Mat[] ValidateCameraMatrices(Mat[] values, string parameterName)
        {
            if (values == null)
            {
                throw new ArgumentNullException(parameterName);
            }
            if (values.Length < 2)
            {
                throw new ArgumentException("At least two camera matrices are required.", parameterName);
            }
            for (int i = 0; i < values.Length; ++i)
            {
                if (values[i] == null)
                {
                    throw new ArgumentNullException(parameterName);
                }
            }
            return values;
        }

        private static void ValidateShape(Mat value, int rows, int cols, string parameterName)
        {
            if (value.Rows != rows || value.Cols != cols)
            {
                throw new ArgumentException(
                    "Matrix shape must be " + rows + " x " + cols + ".",
                    parameterName);
            }
        }
    }
}
