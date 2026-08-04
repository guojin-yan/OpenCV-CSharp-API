namespace JYPPX.OpenCvSharp.VideoIO
{
    /// <summary>
    /// Video capture and writer API backend identifiers.
    /// 视频捕获与写入 API 后端标识。
    /// </summary>
    public enum VideoCaptureAPIs
    {
        /// <summary>Auto-detect backend. 自动检测后端。</summary>
        Any = 0,

        /// <summary>Video4Linux backend. Video4Linux 后端。</summary>
        V4L = 200,

        /// <summary>Video4Linux2 backend. Video4Linux2 后端。</summary>
        V4L2 = V4L,

        /// <summary>IEEE 1394 FireWire backend. IEEE 1394 FireWire 后端。</summary>
        FireWire = 300,

        /// <summary>Alternative spelling for FireWire. FireWire 的兼容拼写。</summary>
        Fireware = FireWire,

        /// <summary>IEEE 1394 backend alias. IEEE 1394 后端别名。</summary>
        IEEE1394 = FireWire,

        /// <summary>DC1394 backend alias. DC1394 后端别名。</summary>
        DC1394 = FireWire,

        /// <summary>CMU1394 backend alias. CMU1394 后端别名。</summary>
        CMU1394 = FireWire,

        /// <summary>DirectShow backend. DirectShow 后端。</summary>
        DShow = 700,

        /// <summary>Prosilica GigE SDK backend. Prosilica GigE SDK 后端。</summary>
        PvApi = 800,

        /// <summary>Android MediaNDK and NDK camera backend. Android MediaNDK 与 NDK Camera 后端。</summary>
        Android = 1000,

        /// <summary>XIMEA camera backend. XIMEA 相机后端。</summary>
        XiApi = 1100,

        /// <summary>AVFoundation backend. AVFoundation 后端。</summary>
        AVFoundation = 1200,

        /// <summary>Microsoft Media Foundation backend. Microsoft Media Foundation 后端。</summary>
        MSMF = 1400,

        /// <summary>Windows Runtime backend. Windows Runtime 后端。</summary>
        WinRT = 1410,

        /// <summary>Intel Perceptual Computing / RealSense backend. Intel Perceptual Computing / RealSense 后端。</summary>
        IntelPerc = 1500,

        /// <summary>RealSense backend alias. RealSense 后端别名。</summary>
        RealSense = IntelPerc,

        /// <summary>OpenNI2 backend. OpenNI2 后端。</summary>
        OpenNI2 = 1600,

        /// <summary>OpenNI2 ASUS backend. OpenNI2 ASUS 后端。</summary>
        OpenNI2Asus = 1610,

        /// <summary>OpenNI2 Astra backend. OpenNI2 Astra 后端。</summary>
        OpenNI2Astra = 1620,

        /// <summary>gPhoto2 backend. gPhoto2 后端。</summary>
        GPhoto2 = 1700,

        /// <summary>GStreamer backend. GStreamer 后端。</summary>
        GStreamer = 1800,

        /// <summary>FFmpeg backend. FFmpeg 后端。</summary>
        FFmpeg = 1900,

        /// <summary>OpenCV image sequence backend. OpenCV 图像序列后端。</summary>
        Images = 2000,

        /// <summary>Aravis SDK backend. Aravis SDK 后端。</summary>
        Aravis = 2100,

        /// <summary>Built-in OpenCV MotionJPEG backend. OpenCV 内置 MotionJPEG 后端。</summary>
        OpenCVMjpeg = 2200,

        /// <summary>Intel MediaSDK backend. Intel MediaSDK 后端。</summary>
        IntelMfx = 2300,

        /// <summary>Xine backend. Xine 后端。</summary>
        Xine = 2400,

        /// <summary>uEye camera backend. uEye 相机后端。</summary>
        UEye = 2500,

        /// <summary>Orbbec 3D sensor backend. Orbbec 3D 传感器后端。</summary>
        OBSensor = 2600
    }
}
