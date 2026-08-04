namespace JYPPX.OpenCvSharp.ImgCodecs
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

        /// <summary>JPEG restart interval. JPEG 重启间隔。</summary>
        JpegRestartInterval = 4,

        /// <summary>JPEG luma quality. JPEG 亮度质量。</summary>
        JpegLumaQuality = 5,

        /// <summary>JPEG chroma quality. JPEG 色度质量。</summary>
        JpegChromaQuality = 6,

        /// <summary>JPEG sampling factor. JPEG 采样因子。</summary>
        JpegSamplingFactor = 7,

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

        /// <summary>PXM binary output switch. PXM 二进制输出开关。</summary>
        PxmBinary = 32,

        /// <summary>OpenEXR storage type. OpenEXR 存储类型。</summary>
        ExrType = 48,

        /// <summary>OpenEXR compression mode. OpenEXR 压缩模式。</summary>
        ExrCompression = 49,

        /// <summary>OpenEXR DWA compression level. OpenEXR DWA 压缩级别。</summary>
        ExrDwaCompressionLevel = 50,

        /// <summary>
        /// WEBP quality from 1 to 100 for lossy mode. WEBP 有损模式质量，范围为 1 到 100。
        /// </summary>
        WebPQuality = 64,

        /// <summary>
        /// WEBP lossless mode. WEBP 无损模式。
        /// </summary>
        WebPLosslessMode = 65,

        /// <summary>Radiance HDR compression mode. Radiance HDR 压缩模式。</summary>
        HdrCompression = 80,

        /// <summary>PAM tuple type. PAM 元组类型。</summary>
        PamTupleType = 128,

        /// <summary>TIFF resolution unit. TIFF 分辨率单位。</summary>
        TiffResolutionUnit = 256,

        /// <summary>TIFF horizontal DPI. TIFF 水平 DPI。</summary>
        TiffXdpi = 257,

        /// <summary>TIFF vertical DPI. TIFF 垂直 DPI。</summary>
        TiffYdpi = 258,

        /// <summary>TIFF compression scheme. TIFF 压缩方案。</summary>
        TiffCompression = 259,

        /// <summary>TIFF rows per strip. TIFF 每条带行数。</summary>
        TiffRowsPerStrip = 278,

        /// <summary>TIFF predictor. TIFF 预测器。</summary>
        TiffPredictor = 317,

        /// <summary>JPEG 2000 target compression rate multiplied by 1000. JPEG 2000 目标压缩率乘以 1000。</summary>
        Jpeg2000CompressionX1000 = 272,

        /// <summary>AVIF quality. AVIF 质量。</summary>
        AvifQuality = 512,

        /// <summary>AVIF bit depth. AVIF 位深。</summary>
        AvifDepth = 513,

        /// <summary>AVIF encoder speed. AVIF 编码速度。</summary>
        AvifSpeed = 514,

        /// <summary>JPEG XL quality. JPEG XL 质量。</summary>
        JpegXlQuality = 640,

        /// <summary>JPEG XL encoder effort. JPEG XL 编码强度。</summary>
        JpegXlEffort = 641,

        /// <summary>JPEG XL distance. JPEG XL 距离参数。</summary>
        JpegXlDistance = 642,

        /// <summary>JPEG XL decoding speed tier. JPEG XL 解码速度级别。</summary>
        JpegXlDecodingSpeed = 643,

        /// <summary>BMP compression mode. BMP 压缩模式。</summary>
        BmpCompression = 768,

        /// <summary>Legacy GIF loop parameter, replaced by <see cref="Animation.LoopCount"/>. 旧 GIF 循环参数。</summary>
        GifLoop = 1024,

        /// <summary>Legacy GIF speed parameter, replaced by frame durations. 旧 GIF 速度参数。</summary>
        GifSpeed = 1025,

        /// <summary>GIF quantization quality. GIF 量化质量。</summary>
        GifQuality = 1026,

        /// <summary>GIF dithering level. GIF 抖动级别。</summary>
        GifDither = 1027,

        /// <summary>GIF alpha transparency threshold. GIF Alpha 透明阈值。</summary>
        GifTransparency = 1028,

        /// <summary>GIF global or local color table switch. GIF 全局或局部色表开关。</summary>
        GifColorTable = 1029
    }
}
