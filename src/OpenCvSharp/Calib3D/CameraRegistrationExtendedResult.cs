using System;
using JYPPX.OpenCvSharp.Core;

namespace JYPPX.OpenCvSharp.Calib3D
{
    /// <summary>
    /// Owned extended result returned by camera-pair registration.
    /// 相机对注册返回的拥有所有权的扩展结果。
    /// </summary>
    public readonly struct CameraRegistrationExtendedResult
    {
        /// <summary>
        /// Initializes an extended camera registration result.
        /// 初始化相机注册扩展结果。
        /// </summary>
        public CameraRegistrationExtendedResult(CameraRegistrationResult registration, Mat rvecs, Mat tvecs)
        {
            Registration = registration;
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
            if (Registration.PerViewErrors.Rows != Rvecs.Rows)
            {
                throw new ArgumentException("Pose vector rows must match the per-view error rows.", nameof(rvecs));
            }
        }

        /// <summary>Gets the compact camera registration result. 获取基础相机注册结果。</summary>
        public CameraRegistrationResult Registration { get; }

        /// <summary>Gets packed per-view rotation vectors as <c>N x 3</c>. 获取 <c>N x 3</c> 每视图旋转向量。</summary>
        public Mat Rvecs { get; }

        /// <summary>Gets packed per-view translation vectors as <c>N x 3</c>. 获取 <c>N x 3</c> 每视图平移向量。</summary>
        public Mat Tvecs { get; }

        /// <summary>Gets the registered frame count. 获取注册帧数。</summary>
        public int ViewCount
        {
            get { return Rvecs.Rows; }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "{Registration=" + Registration
                + ",Rvecs=" + Rvecs.Rows + "x" + Rvecs.Cols
                + ",Tvecs=" + Tvecs.Rows + "x" + Tvecs.Cols
                + "}";
        }
    }
}
