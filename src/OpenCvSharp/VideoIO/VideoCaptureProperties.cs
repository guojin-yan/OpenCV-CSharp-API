namespace JYPPX.OpenCvSharp.VideoIO
{
    /// <summary>
    /// Generic properties supported by OpenCV <c>cv::VideoCapture</c>.
    /// OpenCV <c>cv::VideoCapture</c> 支持的通用属性。
    /// </summary>
    public enum VideoCaptureProperties
    {
        /// <summary>Unknown or unsupported property. 未知或不支持的属性。</summary>
        Unknown = -1,

        /// <summary>Current position in milliseconds. 当前毫秒位置。</summary>
        PosMsec = 0,

        /// <summary>0-based index of the next frame. 下一帧的 0 基索引。</summary>
        PosFrames = 1,

        /// <summary>Relative position of the video file. 视频文件相对位置。</summary>
        PosAviRatio = 2,

        /// <summary>Frame width. 帧宽。</summary>
        FrameWidth = 3,

        /// <summary>Frame height. 帧高。</summary>
        FrameHeight = 4,

        /// <summary>Frame rate. 帧率。</summary>
        Fps = 5,

        /// <summary>Codec FourCC. 编解码器 FourCC。</summary>
        FourCC = 6,

        /// <summary>Number of frames in the file. 文件中的帧数。</summary>
        FrameCount = 7,

        /// <summary>Mat format returned by retrieval. 检索返回的 Mat 格式。</summary>
        Format = 8,

        /// <summary>Backend-specific capture mode. 后端特定捕获模式。</summary>
        Mode = 9,

        /// <summary>Brightness. 亮度。</summary>
        Brightness = 10,

        /// <summary>Contrast. 对比度。</summary>
        Contrast = 11,

        /// <summary>Saturation. 饱和度。</summary>
        Saturation = 12,

        /// <summary>Hue. 色调。</summary>
        Hue = 13,

        /// <summary>Gain. 增益。</summary>
        Gain = 14,

        /// <summary>Exposure. 曝光。</summary>
        Exposure = 15,

        /// <summary>Whether frames are converted to BGR. 是否将帧转换为 BGR。</summary>
        ConvertRgb = 16,

        /// <summary>Blue/U white balance. 蓝色/U 白平衡。</summary>
        WhiteBalanceBlueU = 17,

        /// <summary>Rectification flag. 校正标志。</summary>
        Rectification = 18,

        /// <summary>Monochrome mode. 单色模式。</summary>
        Monochrome = 19,

        /// <summary>Sharpness. 锐度。</summary>
        Sharpness = 20,

        /// <summary>Auto exposure. 自动曝光。</summary>
        AutoExposure = 21,

        /// <summary>Gamma. 伽马。</summary>
        Gamma = 22,

        /// <summary>Temperature. 色温。</summary>
        Temperature = 23,

        /// <summary>Trigger. 触发。</summary>
        Trigger = 24,

        /// <summary>Trigger delay. 触发延迟。</summary>
        TriggerDelay = 25,

        /// <summary>Red/V white balance. 红色/V 白平衡。</summary>
        WhiteBalanceRedV = 26,

        /// <summary>Zoom. 缩放。</summary>
        Zoom = 27,

        /// <summary>Focus. 焦点。</summary>
        Focus = 28,

        /// <summary>Device GUID. 设备 GUID。</summary>
        Guid = 29,

        /// <summary>ISO speed. ISO 速度。</summary>
        IsoSpeed = 30,

        /// <summary>Backlight compensation. 背光补偿。</summary>
        Backlight = 32,

        /// <summary>Pan. 平移。</summary>
        Pan = 33,

        /// <summary>Tilt. 俯仰。</summary>
        Tilt = 34,

        /// <summary>Roll. 横滚。</summary>
        Roll = 35,

        /// <summary>Iris. 光圈。</summary>
        Iris = 36,

        /// <summary>Backend settings dialog. 后端设置对话框。</summary>
        Settings = 37,

        /// <summary>Buffer size. 缓冲区大小。</summary>
        BufferSize = 38,

        /// <summary>Autofocus. 自动对焦。</summary>
        AutoFocus = 39,

        /// <summary>Sample aspect ratio numerator. 采样宽高比分子。</summary>
        SarNum = 40,

        /// <summary>Sample aspect ratio denominator. 采样宽高比分母。</summary>
        SarDen = 41,

        /// <summary>Current backend identifier. 当前后端标识。</summary>
        Backend = 42,

        /// <summary>Video input channel. 视频输入通道。</summary>
        Channel = 43,

        /// <summary>Auto white balance. 自动白平衡。</summary>
        AutoWb = 44,

        /// <summary>White-balance color temperature. 白平衡色温。</summary>
        WbTemperature = 45,

        /// <summary>Codec pixel format. 编解码器像素格式。</summary>
        CodecPixelFormat = 46,

        /// <summary>Video bitrate. 视频码率。</summary>
        Bitrate = 47,

        /// <summary>Frame orientation metadata. 帧方向元数据。</summary>
        OrientationMeta = 48,

        /// <summary>Whether orientation metadata is applied automatically. 是否自动应用方向元数据。</summary>
        OrientationAuto = 49,

        /// <summary>Hardware acceleration type. 硬件加速类型。</summary>
        HwAcceleration = 50,

        /// <summary>Hardware device index. 硬件设备索引。</summary>
        HwDevice = 51,

        /// <summary>Whether hardware acceleration uses OpenCL. 硬件加速是否使用 OpenCL。</summary>
        HwAccelerationUseOpenCL = 52,

        /// <summary>Open timeout in milliseconds. 打开超时毫秒数。</summary>
        OpenTimeoutMsec = 53,

        /// <summary>Read timeout in milliseconds. 读取超时毫秒数。</summary>
        ReadTimeoutMsec = 54,

        /// <summary>Stream open time in microseconds. 流打开时间，单位微秒。</summary>
        StreamOpenTimeUsec = 55,

        /// <summary>Total number of video channels. 视频通道总数。</summary>
        VideoTotalChannels = 56,

        /// <summary>Selected video stream. 选择的视频流。</summary>
        VideoStream = 57,

        /// <summary>Selected audio stream. 选择的音频流。</summary>
        AudioStream = 58,

        /// <summary>Audio position. 音频位置。</summary>
        AudioPos = 59,

        /// <summary>Audio shift in nanoseconds. 音频偏移，单位纳秒。</summary>
        AudioShiftNsec = 60,

        /// <summary>Audio data depth. 音频数据深度。</summary>
        AudioDataDepth = 61,

        /// <summary>Audio samples per second. 音频采样率。</summary>
        AudioSamplesPerSecond = 62,

        /// <summary>Base index of audio channels. 音频通道起始索引。</summary>
        AudioBaseIndex = 63,

        /// <summary>Total audio channels. 音频通道总数。</summary>
        AudioTotalChannels = 64,

        /// <summary>Total audio streams. 音频流总数。</summary>
        AudioTotalStreams = 65,

        /// <summary>Audio synchronization flag. 音频同步标志。</summary>
        AudioSynchronize = 66,

        /// <summary>Whether the last raw frame has a key frame. 最后的原始帧是否包含关键帧。</summary>
        LrfHasKeyFrame = 67,

        /// <summary>Codec extra-data retrieval index. 编解码器额外数据检索索引。</summary>
        CodecExtradataIndex = 68,

        /// <summary>Most recent frame type. 最近帧类型。</summary>
        FrameType = 69,

        /// <summary>Maximum thread count. 最大线程数。</summary>
        NThreads = 70,

        /// <summary>Presentation timestamp. 显示时间戳。</summary>
        Pts = 71,

        /// <summary>DTS delay. DTS 延迟。</summary>
        DtsDelay = 72,

        /// <summary>Initial frame number for image sequences. 图像序列起始帧号。</summary>
        ImageSequenceStart = 73
    }
}
