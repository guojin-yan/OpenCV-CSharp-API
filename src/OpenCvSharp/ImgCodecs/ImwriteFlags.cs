namespace OpenCvSharp.ImgCodecs
{
    /// <summary>
    /// Specifies image encoder parameter keys used by OpenCV <c>imencode</c> and <c>imwrite</c>.
    /// 指定 OpenCV <c>imencode</c> 和 <c>imwrite</c> 使用的图像编码参数键。
    /// </summary>
    public enum ImwriteFlags
    {
        /// <summary>
        /// JPEG quality from 0 to 100. JPEG 质量，范围为 0 到 100。
        /// </summary>
        JpegQuality = 1,

        /// <summary>
        /// Enables progressive JPEG output, 0 or 1. 是否启用渐进式 JPEG 输出，0 或 1。
        /// </summary>
        JpegProgressive = 2,

        /// <summary>
        /// Enables optimized JPEG Huffman tables, 0 or 1. 是否启用优化的 JPEG Huffman 表，0 或 1。
        /// </summary>
        JpegOptimize = 3,

        /// <summary>
        /// PNG compression level from 0 to 9. PNG 压缩级别，范围为 0 到 9。
        /// </summary>
        PngCompression = 16,

        /// <summary>
        /// PNG compression strategy. PNG 压缩策略。
        /// </summary>
        PngStrategy = 17,

        /// <summary>
        /// Binary level PNG flag, 0 or 1. 二值级别 PNG 标志，0 或 1。
        /// </summary>
        PngBilevel = 18,

        /// <summary>
        /// PNG filter flags. PNG 过滤器标志。
        /// </summary>
        PngFilter = 19,

        /// <summary>
        /// PNG zlib buffer size in bytes. PNG zlib 缓冲区大小，单位为字节。
        /// </summary>
        PngZlibBufferSize = 20,

        /// <summary>
        /// WEBP quality from 1 to 100 for lossy mode. WEBP 有损模式质量，范围为 1 到 100。
        /// </summary>
        WebPQuality = 64,

        /// <summary>
        /// WEBP lossless mode. WEBP 无损模式。
        /// </summary>
        WebPLosslessMode = 65
    }
}
