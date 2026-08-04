using System;
using JYPPX.OpenCvSharp.Core;

namespace JYPPX.OpenCvSharp.Calib3D
{
    /// <summary>
    /// Result returned by robot-world/hand-eye calibration.
    /// 机器人世界坐标系/手眼标定返回结果。
    /// </summary>
    public readonly struct RobotWorldHandEyeCalibrationResult
    {
        /// <summary>
        /// Initializes a robot-world/hand-eye calibration result.
        /// 初始化机器人世界坐标系/手眼标定结果。
        /// </summary>
        public RobotWorldHandEyeCalibrationResult(
            Mat rBase2World,
            Mat tBase2World,
            Mat rGripper2Cam,
            Mat tGripper2Cam)
        {
            RBase2World = rBase2World ?? throw new ArgumentNullException(nameof(rBase2World));
            TBase2World = tBase2World ?? throw new ArgumentNullException(nameof(tBase2World));
            RGripper2Cam = rGripper2Cam ?? throw new ArgumentNullException(nameof(rGripper2Cam));
            TGripper2Cam = tGripper2Cam ?? throw new ArgumentNullException(nameof(tGripper2Cam));
        }

        /// <summary>Gets the base-to-world rotation. 获取基座到世界坐标系的旋转。</summary>
        public Mat RBase2World { get; }

        /// <summary>Gets the base-to-world translation. 获取基座到世界坐标系的平移。</summary>
        public Mat TBase2World { get; }

        /// <summary>Gets the gripper-to-camera rotation. 获取夹爪到相机的旋转。</summary>
        public Mat RGripper2Cam { get; }

        /// <summary>Gets the gripper-to-camera translation. 获取夹爪到相机的平移。</summary>
        public Mat TGripper2Cam { get; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "{RBase2World=" + RBase2World.Rows + "x" + RBase2World.Cols
                + ",TBase2World=" + TBase2World.Rows + "x" + TBase2World.Cols
                + ",RGripper2Cam=" + RGripper2Cam.Rows + "x" + RGripper2Cam.Cols
                + ",TGripper2Cam=" + TGripper2Cam.Rows + "x" + TGripper2Cam.Cols
                + "}";
        }
    }
}
