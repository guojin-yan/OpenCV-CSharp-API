using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Video
{
    /// <summary>
    /// Kalman filter object compatible with OpenCV <c>cv::KalmanFilter</c>.
    /// 与 OpenCV <c>cv::KalmanFilter</c> 兼容的 Kalman 滤波器对象。
    /// </summary>
    public sealed class KalmanFilter : IDisposable
    {
        private NativeKalmanFilterHandle handle;
        private bool disposed;

        /// <summary>
        /// Initializes a Kalman filter with matrix dimensions.
        /// 使用矩阵维度初始化 Kalman 滤波器。
        /// </summary>
        public KalmanFilter(int dynamParams, int measureParams, int controlParams = 0, int type = MatType.CV_32F)
        {
            NativeException.ThrowIfError(NativeMethods.KalmanFilterCreate(dynamParams, measureParams, controlParams, type, out IntPtr nativeHandle));
            handle = NativeKalmanFilterHandle.FromNativePointer(nativeHandle);
        }

        /// <summary>
        /// Gets whether this filter has been disposed.
        /// 获取滤波器是否已经释放。
        /// </summary>
        public bool IsDisposed
        {
            get { return disposed; }
        }

        internal IntPtr NativeHandle
        {
            get
            {
                ThrowIfDisposed();
                return handle.DangerousGetHandle();
            }
        }

        /// <summary>
        /// Reinitializes the filter.
        /// 重新初始化滤波器。
        /// </summary>
        public void Init(int dynamParams, int measureParams, int controlParams = 0, int type = MatType.CV_32F)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.KalmanFilterInit(NativeHandle, dynamParams, measureParams, controlParams, type));
        }

        /// <summary>
        /// Computes a predicted state.
        /// 计算预测状态。
        /// </summary>
        public void Predict(Mat prediction, Mat? control = null)
        {
            ThrowIfDisposed();
            ValidateNotNull(prediction, nameof(prediction));
            NativeException.ThrowIfError(NativeMethods.KalmanFilterPredict(NativeHandle, control == null ? IntPtr.Zero : control.NativeHandle, prediction.NativeHandle));
        }

        /// <summary>
        /// Computes and returns a predicted state matrix.
        /// 计算并返回预测状态矩阵。
        /// </summary>
        public Mat Predict(Mat? control = null)
        {
            var prediction = new Mat();
            try
            {
                Predict(prediction, control);
                return prediction;
            }
            catch
            {
                prediction.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Corrects the predicted state with a measurement.
        /// 使用测量值校正预测状态。
        /// </summary>
        public void Correct(Mat measurement, Mat corrected)
        {
            ThrowIfDisposed();
            ValidateNotNull(measurement, nameof(measurement));
            ValidateNotNull(corrected, nameof(corrected));
            NativeException.ThrowIfError(NativeMethods.KalmanFilterCorrect(NativeHandle, measurement.NativeHandle, corrected.NativeHandle));
        }

        /// <summary>
        /// Corrects and returns the corrected state matrix.
        /// 校正并返回校正后的状态矩阵。
        /// </summary>
        public Mat Correct(Mat measurement)
        {
            var corrected = new Mat();
            try
            {
                Correct(measurement, corrected);
                return corrected;
            }
            catch
            {
                corrected.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Copies a Kalman matrix into <paramref name="value"/>.
        /// 将 Kalman 矩阵复制到 <paramref name="value"/>。
        /// </summary>
        public void GetMatrix(KalmanFilterMatrix matrix, Mat value)
        {
            ThrowIfDisposed();
            ValidateNotNull(value, nameof(value));
            NativeException.ThrowIfError(NativeMethods.KalmanFilterGetMatrix(NativeHandle, (int)matrix, value.NativeHandle));
        }

        /// <summary>
        /// Returns a copy of a Kalman matrix.
        /// 返回 Kalman 矩阵副本。
        /// </summary>
        public Mat GetMatrix(KalmanFilterMatrix matrix)
        {
            var value = new Mat();
            try
            {
                GetMatrix(matrix, value);
                return value;
            }
            catch
            {
                value.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Sets a Kalman matrix from <paramref name="value"/>.
        /// 使用 <paramref name="value"/> 设置 Kalman 矩阵。
        /// </summary>
        public void SetMatrix(KalmanFilterMatrix matrix, Mat value)
        {
            ThrowIfDisposed();
            ValidateNotNull(value, nameof(value));
            NativeException.ThrowIfError(NativeMethods.KalmanFilterSetMatrix(NativeHandle, (int)matrix, value.NativeHandle));
        }

        /// <summary>Gets or sets the predicted state. 获取或设置预测状态。</summary>
        public Mat StatePre { get { return GetMatrix(KalmanFilterMatrix.StatePre); } set { SetMatrix(KalmanFilterMatrix.StatePre, value); } }

        /// <summary>Gets or sets the corrected state. 获取或设置校正状态。</summary>
        public Mat StatePost { get { return GetMatrix(KalmanFilterMatrix.StatePost); } set { SetMatrix(KalmanFilterMatrix.StatePost, value); } }

        /// <summary>Gets or sets the transition matrix. 获取或设置状态转移矩阵。</summary>
        public Mat TransitionMatrix { get { return GetMatrix(KalmanFilterMatrix.TransitionMatrix); } set { SetMatrix(KalmanFilterMatrix.TransitionMatrix, value); } }

        /// <summary>Gets or sets the control matrix. 获取或设置控制矩阵。</summary>
        public Mat ControlMatrix { get { return GetMatrix(KalmanFilterMatrix.ControlMatrix); } set { SetMatrix(KalmanFilterMatrix.ControlMatrix, value); } }

        /// <summary>Gets or sets the measurement matrix. 获取或设置测量矩阵。</summary>
        public Mat MeasurementMatrix { get { return GetMatrix(KalmanFilterMatrix.MeasurementMatrix); } set { SetMatrix(KalmanFilterMatrix.MeasurementMatrix, value); } }

        /// <summary>Gets or sets the process noise covariance. 获取或设置过程噪声协方差。</summary>
        public Mat ProcessNoiseCov { get { return GetMatrix(KalmanFilterMatrix.ProcessNoiseCov); } set { SetMatrix(KalmanFilterMatrix.ProcessNoiseCov, value); } }

        /// <summary>Gets or sets the measurement noise covariance. 获取或设置测量噪声协方差。</summary>
        public Mat MeasurementNoiseCov { get { return GetMatrix(KalmanFilterMatrix.MeasurementNoiseCov); } set { SetMatrix(KalmanFilterMatrix.MeasurementNoiseCov, value); } }

        /// <summary>Gets or sets the prior error covariance. 获取或设置先验误差协方差。</summary>
        public Mat ErrorCovPre { get { return GetMatrix(KalmanFilterMatrix.ErrorCovPre); } set { SetMatrix(KalmanFilterMatrix.ErrorCovPre, value); } }

        /// <summary>Gets or sets the Kalman gain. 获取或设置 Kalman 增益。</summary>
        public Mat Gain { get { return GetMatrix(KalmanFilterMatrix.Gain); } set { SetMatrix(KalmanFilterMatrix.Gain, value); } }

        /// <summary>Gets or sets the posterior error covariance. 获取或设置后验误差协方差。</summary>
        public Mat ErrorCovPost { get { return GetMatrix(KalmanFilterMatrix.ErrorCovPost); } set { SetMatrix(KalmanFilterMatrix.ErrorCovPost, value); } }

        /// <summary>
        /// Releases native resources.
        /// 释放 native 资源。
        /// </summary>
        public void Dispose()
        {
            if (!disposed)
            {
                handle.Dispose();
                disposed = true;
                GC.SuppressFinalize(this);
            }
        }

        private void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(KalmanFilter));
            }
        }

        private static void ValidateNotNull<T>(T value, string parameterName)
            where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }
    }
}
