namespace JYPPX.OpenCvSharp.Calib3D
{
    /// <summary>
    /// StereoSGBM dynamic-programming mode.
    /// StereoSGBM 动态规划模式。
    /// </summary>
    public enum StereoSGBMMode
    {
        /// <summary>Five-direction single-pass SGBM. 五方向单遍 SGBM。</summary>
        SGBM = 0,

        /// <summary>Full eight-direction Hirschmuller mode. 完整八方向 Hirschmuller 模式。</summary>
        HH = 1,

        /// <summary>Three-way SGBM mode. 三路 SGBM 模式。</summary>
        SGBM3Way = 2,

        /// <summary>Four-path full-scale mode. 四路径全尺度模式。</summary>
        HH4 = 3
    }
}
