using System;

namespace OpenCvSharp.Video
{
    /// <summary>
    /// Optical-flow operation flags compatible with OpenCV video tracking APIs.
    /// 与 OpenCV video tracking API 兼容的光流操作标志。
    /// </summary>
    [Flags]
    public enum OpticalFlowFlags
    {
        /// <summary>
        /// No extra flags.
        /// 不使用额外标志。
        /// </summary>
        None = 0,

        /// <summary>
        /// Use the provided initial flow estimate.
        /// 使用传入的初始光流估计。
        /// </summary>
        UseInitialFlow = 4,

        /// <summary>
        /// Use minimum eigen values in Lucas-Kanade error output.
        /// 在 Lucas-Kanade 错误输出中使用最小特征值。
        /// </summary>
        LkGetMinEigenvals = 8,

        /// <summary>
        /// Use Gaussian filtering in Farneback optical flow.
        /// 在 Farneback 光流中使用 Gaussian 滤波。
        /// </summary>
        FarnebackGaussian = 256
    }
}
