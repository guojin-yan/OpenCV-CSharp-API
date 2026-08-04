using System;
using System.Globalization;
using JYPPX.OpenCvSharp.Core;

namespace JYPPX.OpenCvSharp.Calib3D
{
    /// <summary>
    /// Owned result returned by camera-pair registration.
    /// 相机对注册返回的拥有所有权的结果。
    /// </summary>
    public readonly struct CameraRegistrationResult
    {
        /// <summary>
        /// Initializes a camera registration result.
        /// 初始化相机注册结果。
        /// </summary>
        public CameraRegistrationResult(double reprojectionError, Mat r, Mat t, Mat e, Mat f, Mat perViewErrors)
        {
            ReprojectionError = reprojectionError;
            R = r ?? throw new ArgumentNullException(nameof(r));
            T = t ?? throw new ArgumentNullException(nameof(t));
            E = e ?? throw new ArgumentNullException(nameof(e));
            F = f ?? throw new ArgumentNullException(nameof(f));
            PerViewErrors = perViewErrors ?? throw new ArgumentNullException(nameof(perViewErrors));
            ValidateShape(R, 3, 3, nameof(r));
            ValidateShape(T, 3, 1, nameof(t));
            ValidateShape(E, 3, 3, nameof(e));
            ValidateShape(F, 3, 3, nameof(f));
            if (PerViewErrors.Rows != 0 && PerViewErrors.Cols != 2)
            {
                throw new ArgumentException("Per-view errors must have two columns.", nameof(perViewErrors));
            }
        }

        /// <summary>Gets the overall RMS reprojection error. 获取整体 RMS 重投影误差。</summary>
        public double ReprojectionError { get; }

        /// <summary>Gets the rotation from camera 1 to camera 2. 获取从相机 1 到相机 2 的旋转。</summary>
        public Mat R { get; }

        /// <summary>Gets the translation from camera 1 to camera 2. 获取从相机 1 到相机 2 的平移。</summary>
        public Mat T { get; }

        /// <summary>Gets the essential matrix. 获取本质矩阵。</summary>
        public Mat E { get; }

        /// <summary>Gets the fundamental matrix. 获取基础矩阵。</summary>
        public Mat F { get; }

        /// <summary>Gets per-view errors as an <c>N x 2</c> matrix. 获取 <c>N x 2</c> 每视图误差矩阵。</summary>
        public Mat PerViewErrors { get; }

        /// <summary>Gets the registered frame count. 获取注册帧数。</summary>
        public int ViewCount
        {
            get { return PerViewErrors.Rows; }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "{ReprojectionError=" + ReprojectionError.ToString(CultureInfo.InvariantCulture)
                + ",R=" + R.Rows + "x" + R.Cols
                + ",T=" + T.Rows + "x" + T.Cols
                + ",E=" + E.Rows + "x" + E.Cols
                + ",F=" + F.Rows + "x" + F.Cols
                + ",PerViewErrors=" + PerViewErrors.Rows + "x" + PerViewErrors.Cols
                + "}";
        }

        private static void ValidateShape(Mat value, int rows, int cols, string parameterName)
        {
            if (value.Rows != 0 && (value.Rows != rows || value.Cols != cols))
            {
                throw new ArgumentException("Matrix shape must be " + rows + " x " + cols + ".", parameterName);
            }
        }
    }
}
