using System;

namespace OpenCvSharp.Calib3D
{
    /// <summary>
    /// Circle-grid calibration pattern detection flags.
    /// 圆点阵列标定图案检测标志。
    /// </summary>
    [Flags]
    public enum CirclesGridFlags
    {
        /// <summary>
        /// Uses a symmetric circle grid.
        /// 使用对称圆点阵列。
        /// </summary>
        SymmetricGrid = 1,

        /// <summary>
        /// Uses an asymmetric circle grid.
        /// 使用非对称圆点阵列。
        /// </summary>
        AsymmetricGrid = 2,

        /// <summary>
        /// Enables the clustering detector path.
        /// 启用聚类检测路径。
        /// </summary>
        Clustering = 4
    }
}
