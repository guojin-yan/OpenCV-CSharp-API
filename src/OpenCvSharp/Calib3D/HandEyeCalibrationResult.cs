using System;
using OpenCvSharp.Core;

namespace OpenCvSharp.Calib3D
{
    /// <summary>
    /// Result returned by hand-eye calibration.
    /// 手眼标定返回结果。
    /// </summary>
    public readonly struct HandEyeCalibrationResult
    {
        /// <summary>
        /// Initializes a hand-eye calibration result.
        /// 初始化手眼标定结果。
        /// </summary>
        public HandEyeCalibrationResult(Mat rCam2Gripper, Mat tCam2Gripper)
        {
            RCam2Gripper = rCam2Gripper ?? throw new ArgumentNullException(nameof(rCam2Gripper));
            TCam2Gripper = tCam2Gripper ?? throw new ArgumentNullException(nameof(tCam2Gripper));
        }

        /// <summary>Gets the camera-to-gripper rotation. 获取相机到夹爪的旋转。</summary>
        public Mat RCam2Gripper { get; }

        /// <summary>Gets the camera-to-gripper translation. 获取相机到夹爪的平移。</summary>
        public Mat TCam2Gripper { get; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "{RCam2Gripper=" + RCam2Gripper.Rows + "x" + RCam2Gripper.Cols
                + ",TCam2Gripper=" + TCam2Gripper.Rows + "x" + TCam2Gripper.Cols
                + "}";
        }
    }
}
