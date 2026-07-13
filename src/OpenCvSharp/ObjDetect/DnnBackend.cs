namespace OpenCvSharp.ObjDetect
{
    /// <summary>
    /// DNN backend identifiers compatible with OpenCV <c>cv::dnn::Backend</c>.
    /// 与 OpenCV <c>cv::dnn::Backend</c> 兼容的 DNN 后端标识。
    /// </summary>
    public enum DnnBackend
    {
        /// <summary>
        /// Default OpenCV DNN backend.
        /// 默认 OpenCV DNN 后端。
        /// </summary>
        Default = 0,

        /// <summary>
        /// Intel OpenVINO backend.
        /// Intel OpenVINO 后端。
        /// </summary>
        InferenceEngine = 2,

        /// <summary>
        /// OpenCV built-in backend.
        /// OpenCV 内置后端。
        /// </summary>
        OpenCV = 3,

        /// <summary>
        /// Vulkan compute backend.
        /// Vulkan 计算后端。
        /// </summary>
        VkCom = 4,

        /// <summary>
        /// CUDA backend.
        /// CUDA 后端。
        /// </summary>
        Cuda = 5,

        /// <summary>
        /// WebNN backend.
        /// WebNN 后端。
        /// </summary>
        WebNN = 6,

        /// <summary>
        /// TIM-VX backend.
        /// TIM-VX 后端。
        /// </summary>
        TimVx = 7,

        /// <summary>
        /// CANN backend.
        /// CANN 后端。
        /// </summary>
        Cann = 8
    }
}
