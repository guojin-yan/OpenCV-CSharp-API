namespace JYPPX.OpenCvSharp.ImgCodecs
{
    /// <summary>
    /// Specifies WEBP lossless mode values for <see cref="ImwriteFlags.WebPLosslessMode"/>.
    /// 指定 <see cref="ImwriteFlags.WebPLosslessMode"/> 使用的 WEBP 无损模式值。
    /// </summary>
    public enum ImwriteWebPLosslessMode
    {
        /// <summary>
        /// Uses lossy WEBP compression. 使用有损 WEBP 压缩。
        /// </summary>
        Off = 0,

        /// <summary>
        /// Uses standard lossless WEBP compression. 使用标准无损 WEBP 压缩。
        /// </summary>
        On = 1,

        /// <summary>
        /// Uses exact lossless WEBP compression. 使用精确无损 WEBP 压缩。
        /// </summary>
        PreserveColor = 2
    }
}
