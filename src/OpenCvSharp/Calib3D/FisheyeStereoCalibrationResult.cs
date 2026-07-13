using System;
using System.Globalization;
using OpenCvSharp.Core;

namespace OpenCvSharp.Calib3D
{
    /// <summary>
    /// Owned result returned by fisheye stereo calibration.
    /// 鱼眼双目标定返回的拥有所有权的结果。
    /// </summary>
    public readonly struct FisheyeStereoCalibrationResult
    {
        /// <summary>
        /// Initializes a fisheye stereo calibration result.
        /// 初始化鱼眼双目标定结果。
        /// </summary>
        public FisheyeStereoCalibrationResult(
            double reprojectionError,
            Mat cameraMatrix1,
            Mat distCoeffs1,
            Mat cameraMatrix2,
            Mat distCoeffs2,
            Mat r,
            Mat t)
        {
            ReprojectionError = reprojectionError;
            CameraMatrix1 = cameraMatrix1 ?? throw new ArgumentNullException(nameof(cameraMatrix1));
            DistCoeffs1 = distCoeffs1 ?? throw new ArgumentNullException(nameof(distCoeffs1));
            CameraMatrix2 = cameraMatrix2 ?? throw new ArgumentNullException(nameof(cameraMatrix2));
            DistCoeffs2 = distCoeffs2 ?? throw new ArgumentNullException(nameof(distCoeffs2));
            R = r ?? throw new ArgumentNullException(nameof(r));
            T = t ?? throw new ArgumentNullException(nameof(t));
            ValidateShape(CameraMatrix1, 3, 3, nameof(cameraMatrix1));
            ValidateDistortionShape(DistCoeffs1, nameof(distCoeffs1));
            ValidateShape(CameraMatrix2, 3, 3, nameof(cameraMatrix2));
            ValidateDistortionShape(DistCoeffs2, nameof(distCoeffs2));
            ValidateShape(R, 3, 3, nameof(r));
            ValidateShape(T, 3, 1, nameof(t));
        }

        /// <summary>Gets the overall RMS reprojection error. 获取整体 RMS 重投影误差。</summary>
        public double ReprojectionError { get; }

        /// <summary>Gets the first fisheye camera matrix. 获取第一个鱼眼相机矩阵。</summary>
        public Mat CameraMatrix1 { get; }

        /// <summary>Gets the first four fisheye distortion coefficients. 获取第一组四个鱼眼畸变系数。</summary>
        public Mat DistCoeffs1 { get; }

        /// <summary>Gets the second fisheye camera matrix. 获取第二个鱼眼相机矩阵。</summary>
        public Mat CameraMatrix2 { get; }

        /// <summary>Gets the second four fisheye distortion coefficients. 获取第二组四个鱼眼畸变系数。</summary>
        public Mat DistCoeffs2 { get; }

        /// <summary>Gets the rotation from camera 1 to camera 2. 获取从相机 1 到相机 2 的旋转。</summary>
        public Mat R { get; }

        /// <summary>Gets the translation from camera 1 to camera 2. 获取从相机 1 到相机 2 的平移。</summary>
        public Mat T { get; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "{ReprojectionError=" + ReprojectionError.ToString(CultureInfo.InvariantCulture)
                + ",CameraMatrix1=" + CameraMatrix1.Rows + "x" + CameraMatrix1.Cols
                + ",DistCoeffs1=" + DistCoeffs1.Rows + "x" + DistCoeffs1.Cols
                + ",CameraMatrix2=" + CameraMatrix2.Rows + "x" + CameraMatrix2.Cols
                + ",DistCoeffs2=" + DistCoeffs2.Rows + "x" + DistCoeffs2.Cols
                + ",R=" + R.Rows + "x" + R.Cols
                + ",T=" + T.Rows + "x" + T.Cols
                + "}";
        }

        private static void ValidateShape(Mat value, int rows, int cols, string parameterName)
        {
            if (value.Rows != 0 && (value.Rows != rows || value.Cols != cols))
            {
                throw new ArgumentException("Matrix shape must be " + rows + " x " + cols + ".", parameterName);
            }
        }

        private static void ValidateDistortionShape(Mat value, string parameterName)
        {
            if (value.Rows == 0)
            {
                return;
            }

            if (!((value.Rows == 4 && value.Cols == 1) || (value.Rows == 1 && value.Cols == 4)))
            {
                throw new ArgumentException("Fisheye distortion coefficients must contain exactly four values.", parameterName);
            }
        }
    }
}
