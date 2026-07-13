namespace OpenCvSharp.Calib3D
{
    /// <summary>
    /// Hand-eye calibration algorithms.
    /// 手眼标定算法。
    /// </summary>
    public enum HandEyeCalibrationMethod
    {
        /// <summary>Tsai-Lenz method. Tsai-Lenz 方法。</summary>
        Tsai = 0,

        /// <summary>Park-Martin method. Park-Martin 方法。</summary>
        Park = 1,

        /// <summary>Horaud-Dornaika method. Horaud-Dornaika 方法。</summary>
        Horaud = 2,

        /// <summary>Andreff online method. Andreff 在线方法。</summary>
        Andreff = 3,

        /// <summary>Daniilidis dual-quaternion method. Daniilidis 对偶四元数方法。</summary>
        Daniilidis = 4
    }
}
