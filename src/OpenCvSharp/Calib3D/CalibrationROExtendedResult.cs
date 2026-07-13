using System;
using OpenCvSharp.Core;

namespace OpenCvSharp.Calib3D
{
    /// <summary>
    /// Extended result returned by object-releasing camera calibration.
    /// 释放物点相机标定扩展结果。
    /// </summary>
    public readonly struct CalibrationROExtendedResult
    {
        /// <summary>
        /// Initializes an extended object-releasing calibration result.
        /// 初始化释放物点标定扩展结果。
        /// </summary>
        public CalibrationROExtendedResult(
            CalibrationROResult calibration,
            Mat stdDeviationsIntrinsics,
            Mat stdDeviationsExtrinsics,
            Mat stdDeviationsObjectPoints,
            Mat perViewErrors)
        {
            Calibration = calibration;
            StdDeviationsIntrinsics = stdDeviationsIntrinsics ?? throw new ArgumentNullException(nameof(stdDeviationsIntrinsics));
            StdDeviationsExtrinsics = stdDeviationsExtrinsics ?? throw new ArgumentNullException(nameof(stdDeviationsExtrinsics));
            StdDeviationsObjectPoints = stdDeviationsObjectPoints ?? throw new ArgumentNullException(nameof(stdDeviationsObjectPoints));
            PerViewErrors = perViewErrors ?? throw new ArgumentNullException(nameof(perViewErrors));
            ValidateObjectPointStandardDeviations(Calibration, StdDeviationsObjectPoints, nameof(stdDeviationsObjectPoints));
            ValidatePerViewErrors(Calibration.ViewCount, PerViewErrors, nameof(perViewErrors));
        }

        /// <summary>Gets the base object-releasing calibration result. 获取基础释放物点标定结果。</summary>
        public CalibrationROResult Calibration { get; }

        /// <summary>Gets intrinsic parameter standard deviations. 获取内参标准差。</summary>
        public Mat StdDeviationsIntrinsics { get; }

        /// <summary>Gets extrinsic parameter standard deviations. 获取外参标准差。</summary>
        public Mat StdDeviationsExtrinsics { get; }

        /// <summary>
        /// Gets refined object-coordinate standard deviations as a single-channel <c>3N x 1</c> matrix.
        /// 获取单通道 <c>3N x 1</c> 矩阵形式的精炼物点坐标标准差。
        /// </summary>
        public Mat StdDeviationsObjectPoints { get; }

        /// <summary>Gets per-view reprojection errors as a single-channel <c>N x 1</c> matrix. 获取单通道 <c>N x 1</c> 矩阵形式的每视图重投影误差。</summary>
        public Mat PerViewErrors { get; }

        /// <summary>Gets the number of calibration views. 获取标定视图数量。</summary>
        public int ViewCount
        {
            get { return Calibration.ViewCount; }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "{Calibration=" + Calibration
                + ",StdDeviationsIntrinsics=" + StdDeviationsIntrinsics.Rows + "x" + StdDeviationsIntrinsics.Cols
                + ",StdDeviationsExtrinsics=" + StdDeviationsExtrinsics.Rows + "x" + StdDeviationsExtrinsics.Cols
                + ",StdDeviationsObjectPoints=" + StdDeviationsObjectPoints.Rows + "x" + StdDeviationsObjectPoints.Cols
                + ",PerViewErrors=" + PerViewErrors.Rows + "x" + PerViewErrors.Cols
                + "}";
        }

        private static void ValidateObjectPointStandardDeviations(
            CalibrationROResult calibration,
            Mat stdDeviationsObjectPoints,
            string parameterName)
        {
            if (stdDeviationsObjectPoints.Empty)
            {
                return;
            }

            int expectedRows = checked(calibration.ObjectPointCount * 3);
            if (expectedRows == 0
                || stdDeviationsObjectPoints.Rows != expectedRows
                || stdDeviationsObjectPoints.Cols != 1
                || stdDeviationsObjectPoints.Channels != 1)
            {
                throw new ArgumentException(
                    "Object-point standard deviations must be a single-channel 3N x 1 matrix matching the refined object points.",
                    parameterName);
            }
        }

        private static void ValidatePerViewErrors(int viewCount, Mat perViewErrors, string parameterName)
        {
            if (perViewErrors.Rows != viewCount
                || (perViewErrors.Rows != 0 && (perViewErrors.Cols != 1 || perViewErrors.Channels != 1)))
            {
                throw new ArgumentException(
                    "Per-view errors must be a single-channel N x 1 matrix matching the calibration view count.",
                    parameterName);
            }
        }
    }
}
