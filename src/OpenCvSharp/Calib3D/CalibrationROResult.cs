using System;
using JYPPX.OpenCvSharp.Core;

namespace JYPPX.OpenCvSharp.Calib3D
{
    /// <summary>
    /// Result returned by object-releasing camera calibration.
    /// 释放物点相机标定返回结果。
    /// </summary>
    public readonly struct CalibrationROResult
    {
        /// <summary>
        /// Initializes an object-releasing calibration result.
        /// 初始化释放物点标定结果。
        /// </summary>
        public CalibrationROResult(CalibrationResult calibration, Mat newObjectPoints)
        {
            Calibration = calibration;
            NewObjectPoints = newObjectPoints ?? throw new ArgumentNullException(nameof(newObjectPoints));
            ValidateNewObjectPoints(NewObjectPoints, nameof(newObjectPoints));
        }

        /// <summary>Gets the base camera calibration result. 获取基础相机标定结果。</summary>
        public CalibrationResult Calibration { get; }

        /// <summary>
        /// Gets refined calibration target points as a single-channel <c>N x 3</c> matrix.
        /// 获取单通道 <c>N x 3</c> 矩阵形式的精炼标定目标物点。
        /// </summary>
        public Mat NewObjectPoints { get; }

        /// <summary>Gets the number of calibration views. 获取标定视图数量。</summary>
        public int ViewCount
        {
            get { return Calibration.ViewCount; }
        }

        /// <summary>Gets the number of refined target points, or zero when standard calibration was selected. 获取精炼目标点数量；选择标准标定时为零。</summary>
        public int ObjectPointCount
        {
            get { return NewObjectPoints.Empty ? 0 : NewObjectPoints.Rows; }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "{Calibration=" + Calibration
                + ",NewObjectPoints=" + NewObjectPoints.Rows + "x" + NewObjectPoints.Cols
                + "}";
        }

        private static void ValidateNewObjectPoints(Mat newObjectPoints, string parameterName)
        {
            if (newObjectPoints.Empty)
            {
                return;
            }

            if (newObjectPoints.Cols != 3 || newObjectPoints.Channels != 1)
            {
                throw new ArgumentException("Refined object points must be a single-channel N x 3 matrix.", parameterName);
            }
        }
    }
}
