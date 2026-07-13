using System;
using OpenCvSharp.Core;

namespace OpenCvSharp.Calib3D
{
    /// <summary>
    /// Owned extended result returned by fisheye stereo calibration.
    /// 鱼眼双目标定返回的拥有所有权的扩展结果。
    /// </summary>
    public readonly struct FisheyeStereoCalibrationExtendedResult
    {
        /// <summary>
        /// Initializes an extended fisheye stereo calibration result.
        /// 初始化鱼眼双目标定扩展结果。
        /// </summary>
        public FisheyeStereoCalibrationExtendedResult(
            FisheyeStereoCalibrationResult calibration,
            Mat rvecs,
            Mat tvecs)
        {
            Calibration = calibration;
            Rvecs = rvecs ?? throw new ArgumentNullException(nameof(rvecs));
            Tvecs = tvecs ?? throw new ArgumentNullException(nameof(tvecs));
            if (Rvecs.Rows != Tvecs.Rows)
            {
                throw new ArgumentException("Rotation and translation vector row counts must match.", nameof(tvecs));
            }
            if (Rvecs.Rows != 0 && Rvecs.Cols != 3)
            {
                throw new ArgumentException("Rotation vectors must have three columns.", nameof(rvecs));
            }
            if (Tvecs.Rows != 0 && Tvecs.Cols != 3)
            {
                throw new ArgumentException("Translation vectors must have three columns.", nameof(tvecs));
            }
        }

        /// <summary>Gets the compact fisheye stereo calibration result. 获取基础鱼眼双目标定结果。</summary>
        public FisheyeStereoCalibrationResult Calibration { get; }

        /// <summary>
        /// Gets packed board rotation vectors in the first camera coordinate system as <c>N x 3</c>.
        /// 获取第一相机坐标系中的 <c>N x 3</c> 标定板旋转向量。
        /// </summary>
        public Mat Rvecs { get; }

        /// <summary>
        /// Gets packed board translation vectors in the first camera coordinate system as <c>N x 3</c>.
        /// 获取第一相机坐标系中的 <c>N x 3</c> 标定板平移向量。
        /// </summary>
        public Mat Tvecs { get; }

        /// <summary>Gets the number of calibration views. 获取标定视图数量。</summary>
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
                + "}";
        }
    }
}
