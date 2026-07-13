namespace OpenCvSharp.ObjDetect
{
    /// <summary>
    /// DNN target identifiers compatible with OpenCV <c>cv::dnn::Target</c>.
    /// 与 OpenCV <c>cv::dnn::Target</c> 兼容的 DNN 目标设备标识。
    /// </summary>
    public enum DnnTarget
    {
        /// <summary>
        /// CPU target.
        /// CPU 目标。
        /// </summary>
        Cpu = 0,

        /// <summary>
        /// OpenCL target.
        /// OpenCL 目标。
        /// </summary>
        OpenCL = 1,

        /// <summary>
        /// OpenCL FP16 target.
        /// OpenCL FP16 目标。
        /// </summary>
        OpenCLFp16 = 2,

        /// <summary>
        /// Intel Movidius Myriad target.
        /// Intel Movidius Myriad 目标。
        /// </summary>
        Myriad = 3,

        /// <summary>
        /// Vulkan target.
        /// Vulkan 目标。
        /// </summary>
        Vulkan = 4,

        /// <summary>
        /// FPGA target.
        /// FPGA 目标。
        /// </summary>
        Fpga = 5,

        /// <summary>
        /// CUDA target.
        /// CUDA 目标。
        /// </summary>
        Cuda = 6,

        /// <summary>
        /// CUDA FP16 target.
        /// CUDA FP16 目标。
        /// </summary>
        CudaFp16 = 7,

        /// <summary>
        /// HDDL target.
        /// HDDL 目标。
        /// </summary>
        Hddl = 8,

        /// <summary>
        /// NPU target.
        /// NPU 目标。
        /// </summary>
        Npu = 9,

        /// <summary>
        /// CPU FP16 target.
        /// CPU FP16 目标。
        /// </summary>
        CpuFp16 = 10
    }
}
