using System;
using System.Globalization;
using JYPPX.OpenCvSharp.Core;

namespace JYPPX.OpenCvSharp.Calib3D
{
    /// <summary>
    /// Result returned by stereo calibration.
    /// 双目标定返回结果。
    /// </summary>
    public readonly struct StereoCalibrationResult
    {
        /// <summary>
        /// Initializes a stereo calibration result.
        /// 初始化双目标定结果。
        /// </summary>
        public StereoCalibrationResult(double reprojectionError, Mat cameraMatrix1, Mat distCoeffs1, Mat cameraMatrix2, Mat distCoeffs2, Mat r, Mat t, Mat e, Mat f)
        {
            ReprojectionError = reprojectionError;
            CameraMatrix1 = cameraMatrix1 ?? throw new ArgumentNullException(nameof(cameraMatrix1));
            DistCoeffs1 = distCoeffs1 ?? throw new ArgumentNullException(nameof(distCoeffs1));
            CameraMatrix2 = cameraMatrix2 ?? throw new ArgumentNullException(nameof(cameraMatrix2));
            DistCoeffs2 = distCoeffs2 ?? throw new ArgumentNullException(nameof(distCoeffs2));
            R = r ?? throw new ArgumentNullException(nameof(r));
            T = t ?? throw new ArgumentNullException(nameof(t));
            E = e ?? throw new ArgumentNullException(nameof(e));
            F = f ?? throw new ArgumentNullException(nameof(f));
        }

        /// <summary>Gets the overall RMS reprojection error. 获取整体 RMS 重投影误差。</summary>
        public double ReprojectionError { get; }

        /// <summary>Gets the first camera matrix. 获取第一个相机矩阵。</summary>
        public Mat CameraMatrix1 { get; }

        /// <summary>Gets the first distortion coefficients. 获取第一组畸变系数。</summary>
        public Mat DistCoeffs1 { get; }

        /// <summary>Gets the second camera matrix. 获取第二个相机矩阵。</summary>
        public Mat CameraMatrix2 { get; }

        /// <summary>Gets the second distortion coefficients. 获取第二组畸变系数。</summary>
        public Mat DistCoeffs2 { get; }

        /// <summary>Gets the rotation between cameras. 获取相机之间的旋转矩阵。</summary>
        public Mat R { get; }

        /// <summary>Gets the translation between cameras. 获取相机之间的平移向量。</summary>
        public Mat T { get; }

        /// <summary>Gets the essential matrix. 获取本质矩阵。</summary>
        public Mat E { get; }

        /// <summary>Gets the fundamental matrix. 获取基础矩阵。</summary>
        public Mat F { get; }

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
                + ",E=" + E.Rows + "x" + E.Cols
                + ",F=" + F.Rows + "x" + F.Cols
                + "}";
        }
    }
}
