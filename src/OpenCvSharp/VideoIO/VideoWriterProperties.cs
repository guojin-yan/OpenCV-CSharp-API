namespace JYPPX.OpenCvSharp.VideoIO
{
    /// <summary>
    /// Generic properties supported by OpenCV <c>cv::VideoWriter</c>.
    /// OpenCV <c>cv::VideoWriter</c> 支持的通用属性。
    /// </summary>
    public enum VideoWriterProperties
    {
        /// <summary>Unknown or unsupported property. 未知或不支持的属性。</summary>
        Unknown = -1,

        /// <summary>Encoding quality. 编码质量。</summary>
        Quality = 1,

        /// <summary>Encoded frame byte count. 编码帧字节数。</summary>
        FrameBytes = 2,

        /// <summary>Number of stripes for parallel encoding. 并行编码条带数。</summary>
        NStripes = 3,

        /// <summary>Whether frames are color. 帧是否为彩色。</summary>
        IsColor = 4,

        /// <summary>Frame depth. 帧深度。</summary>
        Depth = 5,

        /// <summary>Hardware acceleration type. 硬件加速类型。</summary>
        HwAcceleration = 6,

        /// <summary>Hardware device index. 硬件设备索引。</summary>
        HwDevice = 7,

        /// <summary>Whether hardware acceleration uses OpenCL. 硬件加速是否使用 OpenCL。</summary>
        HwAccelerationUseOpenCL = 8,

        /// <summary>Raw video encapsulation flag. 原始视频封装标志。</summary>
        RawVideo = 9,

        /// <summary>Key-frame interval. 关键帧间隔。</summary>
        KeyInterval = 10,

        /// <summary>Key-frame flag for the next frame. 下一帧关键帧标志。</summary>
        KeyFlag = 11,

        /// <summary>Presentation timestamp. 显示时间戳。</summary>
        Pts = 12,

        /// <summary>DTS delay. DTS 延迟。</summary>
        DtsDelay = 13,

        /// <summary>Encoding color space. 编码色彩空间。</summary>
        ColorSpace = 14,

        /// <summary>Whether alpha is enabled. 是否启用 alpha。</summary>
        EnableAlpha = 15
    }
}
