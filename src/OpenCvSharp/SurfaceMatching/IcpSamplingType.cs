namespace OpenCvSharp.SurfaceMatching
{
    /// <summary>
    /// ICP sampling mode values used by OpenCV surface matching.
    /// OpenCV surface matching 使用的 ICP 采样模式。
    /// </summary>
    public enum IcpSamplingType
    {
        /// <summary>Uniform sampling. 均匀采样。</summary>
        Uniform = 0,

        /// <summary>Gelfand sampling value from OpenCV. OpenCV 的 Gelfand 采样值。</summary>
        Gelfand = 1
    }
}
