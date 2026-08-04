namespace JYPPX.OpenCvSharp.ObjDetect
{
    /// <summary>
    /// QR code ECI encoding identifiers compatible with OpenCV <c>cv::QRCodeEncoder::ECIEncodings</c>.
    /// 与 OpenCV <c>cv::QRCodeEncoder::ECIEncodings</c> 兼容的二维码 ECI 编码标识。
    /// </summary>
    public enum QRCodeEncoderECIEncodings
    {
        /// <summary>
        /// Shift JIS encoding.
        /// Shift JIS 编码。
        /// </summary>
        ShiftJis = 20,

        /// <summary>
        /// UTF-8 encoding.
        /// UTF-8 编码。
        /// </summary>
        Utf8 = 26
    }
}
