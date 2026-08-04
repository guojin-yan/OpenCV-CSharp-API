namespace JYPPX.OpenCvSharp.Calib3D
{
    /// <summary>
    /// Perspective-n-Point pose estimation methods.
    /// PnP 位姿估计方法。
    /// </summary>
    public enum SolvePnPFlags
    {
        /// <summary>
        /// Iterative Levenberg-Marquardt pose refinement.
        /// 迭代式 Levenberg-Marquardt 位姿优化。
        /// </summary>
        Iterative = 0,

        /// <summary>
        /// EPnP method.
        /// EPnP 方法。
        /// </summary>
        EPNP = 1,

        /// <summary>
        /// P3P method.
        /// P3P 方法。
        /// </summary>
        P3P = 2,

        /// <summary>
        /// AP3P method.
        /// AP3P 方法。
        /// </summary>
        AP3P = 3,

        /// <summary>
        /// Infinitesimal plane-based pose estimation.
        /// 基于无穷小平面的位姿估计。
        /// </summary>
        IPPE = 4,

        /// <summary>
        /// IPPE special case for square markers.
        /// 面向方形标记的 IPPE 特例。
        /// </summary>
        IPPESquare = 5,

        /// <summary>
        /// SQPnP method.
        /// SQPnP 方法。
        /// </summary>
        SQPNP = 6
    }
}
