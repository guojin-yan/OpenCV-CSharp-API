using System;
using System.Globalization;
using OpenCvSharp.Core;

namespace OpenCvSharp.Calib3D
{
    /// <summary>
    /// Result returned by camera calibration.
    /// 相机标定返回结果。
    /// </summary>
    public readonly struct CalibrationResult
    {
        /// <summary>
        /// Initializes a calibration result.
        /// 初始化标定结果。
        /// </summary>
        public CalibrationResult(double reprojectionError, Mat cameraMatrix, Mat distCoeffs, Mat rvecs, Mat tvecs)
        {
            ReprojectionError = reprojectionError;
            CameraMatrix = cameraMatrix ?? throw new ArgumentNullException(nameof(cameraMatrix));
            DistCoeffs = distCoeffs ?? throw new ArgumentNullException(nameof(distCoeffs));
            Rvecs = rvecs ?? throw new ArgumentNullException(nameof(rvecs));
            Tvecs = tvecs ?? throw new ArgumentNullException(nameof(tvecs));
            ValidatePairedVectorRows(Rvecs, Tvecs, nameof(tvecs));
            ValidatePoseVectorColumns(Rvecs, nameof(rvecs));
            ValidatePoseVectorColumns(Tvecs, nameof(tvecs));
        }

        /// <summary>Gets the overall RMS reprojection error. 获取整体 RMS 重投影误差。</summary>
        public double ReprojectionError { get; }

        /// <summary>Gets the calibrated camera matrix. 获取标定得到的相机矩阵。</summary>
        public Mat CameraMatrix { get; }

        /// <summary>Gets the distortion coefficients. 获取畸变系数。</summary>
        public Mat DistCoeffs { get; }

        /// <summary>Gets the packed rotation vectors as an <c>N x 3</c> matrix. 获取按 <c>N x 3</c> 打包的旋转向量。</summary>
        public Mat Rvecs { get; }

        /// <summary>Gets the packed translation vectors as an <c>N x 3</c> matrix. 获取按 <c>N x 3</c> 打包的平移向量。</summary>
        public Mat Tvecs { get; }

        /// <summary>Gets the number of calibration views represented by the packed vectors. 获取打包向量表示的标定视图数量。</summary>
        public int ViewCount
        {
            get { return Rvecs.Rows; }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "{ReprojectionError=" + ReprojectionError.ToString(CultureInfo.InvariantCulture)
                + ",CameraMatrix=" + CameraMatrix.Rows + "x" + CameraMatrix.Cols
                + ",DistCoeffs=" + DistCoeffs.Rows + "x" + DistCoeffs.Cols
                + ",Rvecs=" + Rvecs.Rows + "x" + Rvecs.Cols
                + ",Tvecs=" + Tvecs.Rows + "x" + Tvecs.Cols
                + "}";
        }

        private static void ValidatePairedVectorRows(Mat rvecs, Mat tvecs, string parameterName)
        {
            if (rvecs.Rows != tvecs.Rows)
            {
                throw new ArgumentException("Rotation and translation vector row counts must match.", parameterName);
            }
        }

        private static void ValidatePoseVectorColumns(Mat vectorMatrix, string parameterName)
        {
            if (vectorMatrix.Rows != 0 && vectorMatrix.Cols != 3)
            {
                throw new ArgumentException("Pose vector column count must be 3.", parameterName);
            }
        }
    }
}
