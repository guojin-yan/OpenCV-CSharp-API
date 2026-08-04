namespace JYPPX.OpenCvSharp.ObjDetect
{
    /// <summary>
    /// QR code encoder mode.
    /// 二维码编码模式。
    /// </summary>
    public enum QRCodeEncoderEncodeMode
    {
        /// <summary>Let OpenCV select a mode. 由 OpenCV 自动选择模式。</summary>
        Auto = -1,

        /// <summary>Numeric mode. 数字模式。</summary>
        Numeric = 1,

        /// <summary>Alphanumeric mode. 字母数字模式。</summary>
        Alphanumeric = 2,

        /// <summary>Structured append mode. 结构化追加模式。</summary>
        StructuredAppend = 3,

        /// <summary>Byte mode. 字节模式。</summary>
        Byte = 4,

        /// <summary>ECI mode. ECI 模式。</summary>
        Eci = 7,

        /// <summary>Kanji mode. Kanji 模式。</summary>
        Kanji = 8
    }
}
