namespace JYPPX.OpenCvSharp.Video
{
    /// <summary>
    /// Matrix identifiers for <see cref="KalmanFilter"/>.
    /// <see cref="KalmanFilter"/> 的矩阵标识。
    /// </summary>
    public enum KalmanFilterMatrix
    {
        /// <summary>Predicted state. 预测状态。</summary>
        StatePre = 0,

        /// <summary>Corrected state. 校正状态。</summary>
        StatePost = 1,

        /// <summary>Transition matrix. 状态转移矩阵。</summary>
        TransitionMatrix = 2,

        /// <summary>Control matrix. 控制矩阵。</summary>
        ControlMatrix = 3,

        /// <summary>Measurement matrix. 测量矩阵。</summary>
        MeasurementMatrix = 4,

        /// <summary>Process noise covariance. 过程噪声协方差。</summary>
        ProcessNoiseCov = 5,

        /// <summary>Measurement noise covariance. 测量噪声协方差。</summary>
        MeasurementNoiseCov = 6,

        /// <summary>Prior error covariance. 先验误差协方差。</summary>
        ErrorCovPre = 7,

        /// <summary>Kalman gain. Kalman 增益。</summary>
        Gain = 8,

        /// <summary>Posterior error covariance. 后验误差协方差。</summary>
        ErrorCovPost = 9
    }
}
