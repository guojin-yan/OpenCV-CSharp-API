using System;
using OpenCvSharp.Core;

namespace OpenCvSharp.Calib3D
{
    /// <summary>
    /// Extended result returned by stereo calibration.
    /// 双目标定扩展结果。
    /// </summary>
    public readonly struct StereoCalibrationExtendedResult
    {
        /// <summary>
        /// Initializes an extended stereo calibration result.
        /// 初始化双目标定扩展结果。
        /// </summary>
        public StereoCalibrationExtendedResult(StereoCalibrationResult calibration, Mat rvecs, Mat tvecs, Mat perViewErrors)
        {
            Calibration = calibration;
            Rvecs = rvecs ?? throw new ArgumentNullException(nameof(rvecs));
            Tvecs = tvecs ?? throw new ArgumentNullException(nameof(tvecs));
            PerViewErrors = perViewErrors ?? throw new ArgumentNullException(nameof(perViewErrors));
            ValidatePairedVectorRows(Rvecs, Tvecs, nameof(tvecs));
            ValidatePoseVectorColumns(Rvecs, nameof(rvecs));
            ValidatePoseVectorColumns(Tvecs, nameof(tvecs));
            ValidatePerViewErrorRows(Rvecs, PerViewErrors, nameof(perViewErrors));
            ValidatePerViewErrorColumns(PerViewErrors, nameof(perViewErrors));
        }

        /// <summary>Gets the base stereo calibration result. 获取基础双目标定结果。</summary>
        public StereoCalibrationResult Calibration { get; }

        /// <summary>Gets packed per-view rotation vectors as an <c>N x 3</c> matrix. 获取按 <c>N x 3</c> 打包的每视图旋转向量。</summary>
        public Mat Rvecs { get; }

        /// <summary>Gets packed per-view translation vectors as an <c>N x 3</c> matrix. 获取按 <c>N x 3</c> 打包的每视图平移向量。</summary>
        public Mat Tvecs { get; }

        /// <summary>Gets per-view reprojection errors as an <c>N x 2</c> matrix. 获取 <c>N x 2</c> 矩阵形式的每个视图重投影误差。</summary>
        public Mat PerViewErrors { get; }

        /// <summary>Gets the number of stereo calibration views represented by the packed vectors. 获取打包向量表示的双目标定视图数量。</summary>
        public int ViewCount
        {
            get { return Rvecs.Rows; }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "{Calibration=" + Calibration
                + ",Rvecs=" + Rvecs.Rows + "x" + Rvecs.Cols
                + ",Tvecs=" + Tvecs.Rows + "x" + Tvecs.Cols
                + ",PerViewErrors=" + PerViewErrors.Rows + "x" + PerViewErrors.Cols
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

        private static void ValidatePerViewErrorRows(Mat rvecs, Mat perViewErrors, string parameterName)
        {
            if (perViewErrors.Rows != rvecs.Rows)
            {
                throw new ArgumentException("Per-view error row count must match the stereo calibration view count.", parameterName);
            }
        }

        private static void ValidatePerViewErrorColumns(Mat perViewErrors, string parameterName)
        {
            if (perViewErrors.Rows == 0)
            {
                return;
            }

            if (perViewErrors.Cols != 2)
            {
                throw new ArgumentException("Per-view error column count must be 2.", parameterName);
            }
        }
    }
}
