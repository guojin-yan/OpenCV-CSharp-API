using System;
using OpenCvSharp.Core;

namespace OpenCvSharp.Calib3D
{
    /// <summary>
    /// Owned extended result returned by multi-camera calibration.
    /// 多相机标定返回的拥有所有权的扩展结果。
    /// </summary>
    public readonly struct MultiviewCalibrationExtendedResult
    {
        /// <summary>
        /// Initializes an extended multi-camera calibration result.
        /// 初始化多相机标定扩展结果。
        /// </summary>
        public MultiviewCalibrationExtendedResult(
            MultiviewCalibrationResult calibration,
            Mat initializationPairs,
            Mat[] rvecs0,
            Mat[] tvecs0,
            Mat perFrameErrors)
        {
            Calibration = calibration;
            InitializationPairs = initializationPairs ?? throw new ArgumentNullException(nameof(initializationPairs));
            Rvecs0 = ValidateFrameVectors(rvecs0, nameof(rvecs0));
            Tvecs0 = ValidateFrameVectors(tvecs0, nameof(tvecs0));
            PerFrameErrors = perFrameErrors ?? throw new ArgumentNullException(nameof(perFrameErrors));

            if (Rvecs0.Length != Tvecs0.Length)
            {
                throw new ArgumentException("Frame-pose arrays must have the same length.", nameof(tvecs0));
            }
            if (InitializationPairs.Rows != Calibration.CameraCount - 1 || InitializationPairs.Cols != 2)
            {
                throw new ArgumentException(
                    "Initialization pairs must have shape (cameraCount - 1) x 2.",
                    nameof(initializationPairs));
            }
            if (PerFrameErrors.Rows != Calibration.CameraCount || PerFrameErrors.Cols != Rvecs0.Length)
            {
                throw new ArgumentException(
                    "Per-frame errors must have shape cameraCount x frameCount.",
                    nameof(perFrameErrors));
            }
        }

        /// <summary>Gets the compact calibration result. 获取基础标定结果。</summary>
        public MultiviewCalibrationResult Calibration { get; }

        /// <summary>Gets initialization camera pairs as a <c>(C - 1) x 2</c> matrix. 获取 <c>(C - 1) x 2</c> 初始化相机对。</summary>
        public Mat InitializationPairs { get; }

        /// <summary>Gets one camera-0 rotation vector per frame. 获取每帧相机 0 的旋转向量。</summary>
        public Mat[] Rvecs0 { get; }

        /// <summary>Gets one camera-0 translation vector per frame. 获取每帧相机 0 的平移向量。</summary>
        public Mat[] Tvecs0 { get; }

        /// <summary>Gets camera-by-frame reprojection errors. 获取相机乘帧的重投影误差。</summary>
        public Mat PerFrameErrors { get; }

        /// <summary>Gets the frame count. 获取帧数。</summary>
        public int FrameCount
        {
            get { return Rvecs0.Length; }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "{Calibration=" + Calibration
                + ",FrameCount=" + FrameCount
                + ",InitializationPairs=" + InitializationPairs.Rows + "x" + InitializationPairs.Cols
                + ",PerFrameErrors=" + PerFrameErrors.Rows + "x" + PerFrameErrors.Cols
                + "}";
        }

        private static Mat[] ValidateFrameVectors(Mat[] values, string parameterName)
        {
            if (values == null)
            {
                throw new ArgumentNullException(parameterName);
            }
            if (values.Length == 0)
            {
                throw new ArgumentException("Frame-pose arrays cannot be empty.", parameterName);
            }
            for (int i = 0; i < values.Length; ++i)
            {
                if (values[i] == null)
                {
                    throw new ArgumentNullException(parameterName);
                }
                if (!values[i].Empty && (values[i].Rows != 3 || values[i].Cols != 1))
                {
                    throw new ArgumentException(
                        "Frame-pose matrices must be empty or 3 x 1.",
                        parameterName);
                }
            }
            return values;
        }
    }
}
