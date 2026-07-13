using System;
using OpenCvSharp.Core;

namespace OpenCvSharp.Calib3D
{
    /// <summary>
    /// Extended result returned by camera calibration with uncertainty outputs.
    /// 带不确定度输出的相机标定扩展结果。
    /// </summary>
    public readonly struct CalibrationExtendedResult
    {
        /// <summary>
        /// Initializes an extended calibration result.
        /// 初始化扩展标定结果。
        /// </summary>
        public CalibrationExtendedResult(CalibrationResult calibration, Mat stdDeviationsIntrinsics, Mat stdDeviationsExtrinsics, Mat perViewErrors)
        {
            Calibration = calibration;
            StdDeviationsIntrinsics = stdDeviationsIntrinsics ?? throw new ArgumentNullException(nameof(stdDeviationsIntrinsics));
            StdDeviationsExtrinsics = stdDeviationsExtrinsics ?? throw new ArgumentNullException(nameof(stdDeviationsExtrinsics));
            PerViewErrors = perViewErrors ?? throw new ArgumentNullException(nameof(perViewErrors));
            ValidatePerViewErrorRows(Calibration.ViewCount, PerViewErrors, nameof(perViewErrors));
            ValidatePerViewErrorColumns(PerViewErrors, nameof(perViewErrors));
        }

        /// <summary>Gets the base calibration result. 获取基础标定结果。</summary>
        public CalibrationResult Calibration { get; }

        /// <summary>Gets the intrinsic parameter standard deviations. 获取内参标准差。</summary>
        public Mat StdDeviationsIntrinsics { get; }

        /// <summary>Gets the extrinsic parameter standard deviations. 获取外参标准差。</summary>
        public Mat StdDeviationsExtrinsics { get; }

        /// <summary>Gets per-view reprojection errors as an N x 1 column vector. 获取 N x 1 列向量形式的每个视图重投影误差。</summary>
        public Mat PerViewErrors { get; }

        /// <summary>Gets the number of calibration views represented by the base result. 获取基础结果表示的标定视图数量。</summary>
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
                + ",PerViewErrors=" + PerViewErrors.Rows + "x" + PerViewErrors.Cols
                + "}";
        }

        private static void ValidatePerViewErrorRows(int viewCount, Mat perViewErrors, string parameterName)
        {
            if (perViewErrors.Rows != viewCount)
            {
                throw new ArgumentException("Per-view error row count must match the calibration view count.", parameterName);
            }
        }

        private static void ValidatePerViewErrorColumns(Mat perViewErrors, string parameterName)
        {
            if (perViewErrors.Rows == 0)
            {
                return;
            }

            if (perViewErrors.Cols != 1)
            {
                throw new ArgumentException("Per-view error column count must be 1.", parameterName);
            }
        }
    }
}
