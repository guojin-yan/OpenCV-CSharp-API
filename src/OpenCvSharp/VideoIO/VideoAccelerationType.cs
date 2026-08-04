namespace JYPPX.OpenCvSharp.VideoIO
{
    /// <summary>
    /// Hardware acceleration type for video decoding and encoding.
    /// 视频解码与编码硬件加速类型。
    /// </summary>
    public enum VideoAccelerationType
    {
        /// <summary>No requested hardware acceleration. 不要求硬件加速。</summary>
        None = 0,

        /// <summary>Prefer any available hardware acceleration. 优先使用任意可用硬件加速。</summary>
        Any = 1,

        /// <summary>Direct3D 11 acceleration. Direct3D 11 加速。</summary>
        D3D11 = 2,

        /// <summary>VAAPI acceleration. VAAPI 加速。</summary>
        VAAPI = 3,

        /// <summary>Intel MediaSDK / oneVPL acceleration. Intel MediaSDK / oneVPL 加速。</summary>
        Mfx = 4,

        /// <summary>DRM acceleration. DRM 加速。</summary>
        DRM = 5
    }
}
