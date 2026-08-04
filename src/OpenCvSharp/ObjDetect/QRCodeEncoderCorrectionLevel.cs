namespace JYPPX.OpenCvSharp.ObjDetect
{
    /// <summary>
    /// QR code error correction level.
    /// 二维码纠错级别。
    /// </summary>
    public enum QRCodeEncoderCorrectionLevel
    {
        /// <summary>Low correction level. 低纠错级别。</summary>
        L = 0,

        /// <summary>Medium correction level. 中等纠错级别。</summary>
        M = 1,

        /// <summary>Quartile correction level. 四分位纠错级别。</summary>
        Q = 2,

        /// <summary>High correction level. 高纠错级别。</summary>
        H = 3
    }
}
