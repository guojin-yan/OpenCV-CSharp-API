namespace JYPPX.OpenCvSharp.Calib3D
{
    /// <summary>
    /// Pre-filtering modes for <see cref="StereoBM"/>.
    /// <see cref="StereoBM"/> 的预滤波模式。
    /// </summary>
    public enum StereoBMPreFilterType
    {
        /// <summary>
        /// Normalized response pre-filter.
        /// 归一化响应预滤波。
        /// </summary>
        NormalizedResponse = 0,

        /// <summary>
        /// X-Sobel pre-filter.
        /// X 方向 Sobel 预滤波。
        /// </summary>
        XSobel = 1
    }
}
