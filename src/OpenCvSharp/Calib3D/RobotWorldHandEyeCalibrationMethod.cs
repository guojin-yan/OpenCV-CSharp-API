namespace JYPPX.OpenCvSharp.Calib3D
{
    /// <summary>
    /// Robot-world/hand-eye calibration algorithms.
    /// 机器人世界坐标系/手眼标定算法。
    /// </summary>
    public enum RobotWorldHandEyeCalibrationMethod
    {
        /// <summary>Shah Kronecker-product method. Shah Kronecker 积方法。</summary>
        Shah = 0,

        /// <summary>Li simultaneous dual-quaternion method. Li 同步对偶四元数方法。</summary>
        Li = 1
    }
}
