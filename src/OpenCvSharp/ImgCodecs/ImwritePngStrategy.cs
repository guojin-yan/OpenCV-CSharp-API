namespace JYPPX.OpenCvSharp.ImgCodecs
{
    /// <summary>
    /// Specifies PNG compression strategy values for <see cref="ImwriteFlags.PngStrategy"/>.
    /// 指定 <see cref="ImwriteFlags.PngStrategy"/> 使用的 PNG 压缩策略值。
    /// </summary>
    public enum ImwritePngStrategy
    {
        /// <summary>
        /// Uses the default zlib strategy. 使用默认 zlib 策略。
        /// </summary>
        Default = 0,

        /// <summary>
        /// Uses a strategy tuned for filtered image data. 使用适合过滤后图像数据的策略。
        /// </summary>
        Filtered = 1,

        /// <summary>
        /// Uses Huffman encoding only. 仅使用 Huffman 编码。
        /// </summary>
        HuffmanOnly = 2,

        /// <summary>
        /// Uses run-length encoding strategy. 使用游程编码策略。
        /// </summary>
        Rle = 3,

        /// <summary>
        /// Uses fixed Huffman codes. 使用固定 Huffman 编码。
        /// </summary>
        Fixed = 4
    }
}
