using System;
using OpenCvSharp.Core;

namespace OpenCvSharp.Calib3D
{
    /// <summary>
    /// Owned projected points and six separated ProjectPoints Jacobian blocks.
    /// 拥有所有权的投影像点和六个分离 ProjectPoints Jacobian 块。
    /// </summary>
    public readonly struct ProjectPointsDerivativesResult
    {
        /// <summary>
        /// Initializes a separated ProjectPoints derivative result.
        /// 初始化分离 ProjectPoints 导数结果。
        /// </summary>
        public ProjectPointsDerivativesResult(
            Mat imagePoints,
            Mat dpdr,
            Mat dpdt,
            Mat dpdf,
            Mat dpdc,
            Mat dpdk,
            Mat dpdo)
        {
            ImagePoints = imagePoints ?? throw new ArgumentNullException(nameof(imagePoints));
            DpDr = dpdr ?? throw new ArgumentNullException(nameof(dpdr));
            DpDt = dpdt ?? throw new ArgumentNullException(nameof(dpdt));
            DpDf = dpdf ?? throw new ArgumentNullException(nameof(dpdf));
            DpDc = dpdc ?? throw new ArgumentNullException(nameof(dpdc));
            DpDk = dpdk ?? throw new ArgumentNullException(nameof(dpdk));
            DpDo = dpdo ?? throw new ArgumentNullException(nameof(dpdo));

            ValidateImagePoints(ImagePoints, nameof(imagePoints));
            int pointCount = ImagePoints.Rows;
            int derivativeRows = checked(pointCount * 2);
            ValidateDerivative(DpDr, derivativeRows, 3, nameof(dpdr));
            ValidateDerivative(DpDt, derivativeRows, 3, nameof(dpdt));
            ValidateDerivative(DpDf, derivativeRows, 2, nameof(dpdf));
            ValidateDerivative(DpDc, derivativeRows, 2, nameof(dpdc));
            ValidateDistortionDerivative(DpDk, derivativeRows, nameof(dpdk));
            ValidateDerivative(DpDo, derivativeRows, checked(pointCount * 3), nameof(dpdo));
        }

        /// <summary>Gets the projected image points. 获取投影像点。</summary>
        public Mat ImagePoints { get; }

        /// <summary>Gets derivatives with respect to rotation. 获取旋转导数。</summary>
        public Mat DpDr { get; }

        /// <summary>Gets derivatives with respect to translation. 获取平移导数。</summary>
        public Mat DpDt { get; }

        /// <summary>Gets derivatives with respect to focal lengths. 获取焦距导数。</summary>
        public Mat DpDf { get; }

        /// <summary>Gets derivatives with respect to the principal point. 获取主点导数。</summary>
        public Mat DpDc { get; }

        /// <summary>Gets derivatives with respect to distortion coefficients. 获取畸变系数导数。</summary>
        public Mat DpDk { get; }

        /// <summary>Gets derivatives with respect to object-point coordinates. 获取物点坐标导数。</summary>
        public Mat DpDo { get; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "{ImagePoints=" + ImagePoints.Rows + "x" + ImagePoints.Cols
                + "x" + ImagePoints.Channels
                + ",DerivativeRows=" + DpDr.Rows
                + ",DistortionColumns=" + DpDk.Cols
                + ",ObjectColumns=" + DpDo.Cols + "}";
        }

        private static void ValidateImagePoints(Mat imagePoints, string parameterName)
        {
            if (imagePoints.Empty ||
                imagePoints.Rows <= 0 ||
                imagePoints.Cols != 1 ||
                imagePoints.Channels != 2 ||
                (imagePoints.Depth != MatType.CV_32F &&
                 imagePoints.Depth != MatType.CV_64F))
            {
                throw new ArgumentException(
                    "Image points must be a non-empty N x 1 two-channel CV_32F or CV_64F matrix.",
                    parameterName);
            }
        }

        private static void ValidateDerivative(
            Mat derivative,
            int expectedRows,
            int expectedColumns,
            string parameterName)
        {
            if (derivative.Rows != expectedRows ||
                derivative.Cols != expectedColumns ||
                derivative.Type != MatType.CV_64FC1)
            {
                throw new ArgumentException(
                    "ProjectPoints derivative matrix has an invalid shape or type.",
                    parameterName);
            }
        }

        private static void ValidateDistortionDerivative(
            Mat derivative,
            int expectedRows,
            string parameterName)
        {
            if (derivative.Rows != expectedRows ||
                derivative.Type != MatType.CV_64FC1 ||
                (derivative.Cols != 4 &&
                 derivative.Cols != 5 &&
                 derivative.Cols != 8 &&
                 derivative.Cols != 12 &&
                 derivative.Cols != 14))
            {
                throw new ArgumentException(
                    "Distortion derivative must be CV_64FC1 with 4, 5, 8, 12, or 14 columns.",
                    parameterName);
            }
        }
    }
}
