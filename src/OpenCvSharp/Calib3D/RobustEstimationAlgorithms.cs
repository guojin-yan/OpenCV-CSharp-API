namespace JYPPX.OpenCvSharp.Calib3D
{
    /// <summary>
    /// Robust estimation algorithms used by homography and essential-matrix estimation.
    /// 单应矩阵和本质矩阵估计使用的鲁棒估计算法。
    /// </summary>
    public enum RobustEstimationAlgorithms
    {
        /// <summary>
        /// Ordinary least-squares estimation using all points.
        /// 使用所有点进行普通最小二乘估计。
        /// </summary>
        LeastSquares = 0,

        /// <summary>
        /// Least-median-of-squares estimation.
        /// 最小中值平方估计。
        /// </summary>
        LMEDS = 4,

        /// <summary>
        /// RANSAC robust estimation.
        /// RANSAC 鲁棒估计。
        /// </summary>
        RANSAC = 8,

        /// <summary>
        /// RHO / PROSAC based robust estimation.
        /// 基于 RHO / PROSAC 的鲁棒估计。
        /// </summary>
        RHO = 16,

        /// <summary>
        /// Default USAC robust estimation.
        /// 默认 USAC 鲁棒估计。
        /// </summary>
        USACDefault = 32,

        /// <summary>
        /// Parallel USAC robust estimation.
        /// 并行 USAC 鲁棒估计。
        /// </summary>
        USACParallel = 33,

        /// <summary>
        /// USAC with the 8-point fundamental matrix model.
        /// 使用 8 点基础矩阵模型的 USAC。
        /// </summary>
        USACFM8Points = 34,

        /// <summary>
        /// Fast USAC settings.
        /// 快速 USAC 设置。
        /// </summary>
        USACFast = 35,

        /// <summary>
        /// Accurate USAC settings.
        /// 高精度 USAC 设置。
        /// </summary>
        USACAccurate = 36,

        /// <summary>
        /// PROSAC sampling inside USAC.
        /// USAC 内部的 PROSAC 采样。
        /// </summary>
        USACProsac = 37,

        /// <summary>
        /// MAGSAC++ scoring inside USAC.
        /// USAC 内部的 MAGSAC++ 评分。
        /// </summary>
        USACMagsac = 38
    }
}
