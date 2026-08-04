namespace JYPPX.OpenCvSharp.ImgCodecs
{
    /// <summary>Specifies JPEG chroma sampling factors. 指定 JPEG 色度采样因子。</summary>
    public enum ImwriteJpegSamplingFactor
    {
        /// <summary>4:1:1 sampling. 4:1:1 采样。</summary>
        Sampling411 = 0x411111,
        /// <summary>4:2:0 sampling. 4:2:0 采样。</summary>
        Sampling420 = 0x221111,
        /// <summary>4:2:2 sampling. 4:2:2 采样。</summary>
        Sampling422 = 0x211111,
        /// <summary>4:4:0 sampling. 4:4:0 采样。</summary>
        Sampling440 = 0x121111,
        /// <summary>4:4:4 sampling. 4:4:4 采样。</summary>
        Sampling444 = 0x111111
    }

    /// <summary>Specifies TIFF compression tag values. 指定 TIFF 压缩标签值。</summary>
    public enum ImwriteTiffCompression
    {
        /// <summary>No compression. 不压缩。</summary>
        None = 1,
        /// <summary>CCITT modified Huffman RLE. CCITT 修改 Huffman RLE。</summary>
        CcittRle = 2,
        /// <summary>CCITT Group 3 fax. CCITT Group 3 传真。</summary>
        CcittFax3 = 3,
        /// <summary>Alias for CCITT T.4. CCITT T.4 别名。</summary>
        CcittT4 = CcittFax3,
        /// <summary>CCITT Group 4 fax. CCITT Group 4 传真。</summary>
        CcittFax4 = 4,
        /// <summary>Alias for CCITT T.6. CCITT T.6 别名。</summary>
        CcittT6 = CcittFax4,
        /// <summary>LZW compression. LZW 压缩。</summary>
        Lzw = 5,
        /// <summary>Old JPEG compression. 旧 JPEG 压缩。</summary>
        Ojpeg = 6,
        /// <summary>JPEG compression. JPEG 压缩。</summary>
        Jpeg = 7,
        /// <summary>Adobe deflate compression. Adobe Deflate 压缩。</summary>
        AdobeDeflate = 8,
        /// <summary>T.85 JBIG compression. T.85 JBIG 压缩。</summary>
        T85 = 9,
        /// <summary>T.43 layered JBIG compression. T.43 分层 JBIG 压缩。</summary>
        T43 = 10,
        /// <summary>NeXT 2-bit RLE. NeXT 2 位 RLE。</summary>
        Next = 32766,
        /// <summary>Word-aligned CCITT RLE. 字对齐 CCITT RLE。</summary>
        CcittRleW = 32771,
        /// <summary>PackBits compression. PackBits 压缩。</summary>
        PackBits = 32773,
        /// <summary>ThunderScan compression. ThunderScan 压缩。</summary>
        ThunderScan = 32809,
        /// <summary>IT8 CT padded compression. IT8 CT 填充压缩。</summary>
        It8CtPad = 32895,
        /// <summary>IT8 linework compression. IT8 线稿压缩。</summary>
        It8Lw = 32896,
        /// <summary>IT8 monochrome picture compression. IT8 单色图片压缩。</summary>
        It8Mp = 32897,
        /// <summary>IT8 binary line art compression. IT8 二值线稿压缩。</summary>
        It8Bl = 32898,
        /// <summary>Pixar film compression. Pixar Film 压缩。</summary>
        PixarFilm = 32908,
        /// <summary>Pixar log compression. Pixar Log 压缩。</summary>
        PixarLog = 32909,
        /// <summary>Legacy deflate tag. 旧 Deflate 标签。</summary>
        Deflate = 32946,
        /// <summary>Kodak DCS compression. Kodak DCS 压缩。</summary>
        Dcs = 32947,
        /// <summary>ISO JBIG compression. ISO JBIG 压缩。</summary>
        Jbig = 34661,
        /// <summary>SGI Log compression. SGI Log 压缩。</summary>
        SgiLog = 34676,
        /// <summary>SGI Log 24-bit compression. SGI Log 24 位压缩。</summary>
        SgiLog24 = 34677,
        /// <summary>JPEG 2000 compression. JPEG 2000 压缩。</summary>
        Jpeg2000 = 34712,
        /// <summary>LERC compression. LERC 压缩。</summary>
        Lerc = 34887,
        /// <summary>LZMA compression. LZMA 压缩。</summary>
        Lzma = 34925,
        /// <summary>Zstandard compression. Zstandard 压缩。</summary>
        Zstd = 50000,
        /// <summary>WebP compression. WebP 压缩。</summary>
        WebP = 50001,
        /// <summary>JPEG XL compression. JPEG XL 压缩。</summary>
        JpegXl = 50002
    }

    /// <summary>Specifies TIFF predictor values. 指定 TIFF 预测器值。</summary>
    public enum ImwriteTiffPredictor
    {
        /// <summary>No predictor. 无预测器。</summary>
        None = 1,
        /// <summary>Horizontal differencing. 水平差分。</summary>
        Horizontal = 2,
        /// <summary>Floating-point predictor. 浮点预测器。</summary>
        FloatingPoint = 3
    }

    /// <summary>Specifies TIFF resolution units. 指定 TIFF 分辨率单位。</summary>
    public enum ImwriteTiffResolutionUnit
    {
        /// <summary>No absolute unit. 无绝对单位。</summary>
        None = 1,
        /// <summary>Inches. 英寸。</summary>
        Inch = 2,
        /// <summary>Centimeters. 厘米。</summary>
        Centimeter = 3
    }

    /// <summary>Specifies OpenEXR pixel storage types. 指定 OpenEXR 像素存储类型。</summary>
    public enum ImwriteExrType
    {
        /// <summary>16-bit half float. 16 位半精度浮点。</summary>
        Half = 1,
        /// <summary>32-bit float. 32 位浮点。</summary>
        Float = 2
    }

    /// <summary>Specifies OpenEXR compression modes. 指定 OpenEXR 压缩模式。</summary>
    public enum ImwriteExrCompression
    {
        /// <summary>No compression. 不压缩。</summary>
        None = 0,
        /// <summary>Run-length encoding. 游程编码。</summary>
        Rle = 1,
        /// <summary>Zlib per scanline. 每扫描线 Zlib。</summary>
        Zips = 2,
        /// <summary>Zlib in 16-line blocks. 16 行块 Zlib。</summary>
        Zip = 3,
        /// <summary>PIZ wavelet compression. PIZ 小波压缩。</summary>
        Piz = 4,
        /// <summary>Lossy 24-bit float compression. 有损 24 位浮点压缩。</summary>
        Pxr24 = 5,
        /// <summary>B44 block compression. B44 块压缩。</summary>
        B44 = 6,
        /// <summary>B44A block compression. B44A 块压缩。</summary>
        B44A = 7,
        /// <summary>DWAA compression. DWAA 压缩。</summary>
        Dwaa = 8,
        /// <summary>DWAB compression. DWAB 压缩。</summary>
        Dwab = 9
    }

    /// <summary>Specifies PAM tuple formats. 指定 PAM 元组格式。</summary>
    public enum ImwritePamFormat
    {
        /// <summary>Unspecified format. 未指定格式。</summary>
        Null = 0,
        /// <summary>Black and white. 黑白。</summary>
        BlackAndWhite = 1,
        /// <summary>Grayscale. 灰度。</summary>
        Grayscale = 2,
        /// <summary>Grayscale with alpha. 带 Alpha 的灰度。</summary>
        GrayscaleAlpha = 3,
        /// <summary>RGB. RGB。</summary>
        Rgb = 4,
        /// <summary>RGB with alpha. 带 Alpha 的 RGB。</summary>
        RgbAlpha = 5
    }

    /// <summary>Specifies Radiance HDR compression. 指定 Radiance HDR 压缩。</summary>
    public enum ImwriteHdrCompression
    {
        /// <summary>No compression. 不压缩。</summary>
        None = 0,
        /// <summary>Run-length encoding. 游程编码。</summary>
        Rle = 1
    }

    /// <summary>Specifies BMP compression. 指定 BMP 压缩。</summary>
    public enum ImwriteBmpCompression
    {
        /// <summary>BI_RGB. BI_RGB。</summary>
        Rgb = 0,
        /// <summary>BI_BITFIELDS. BI_BITFIELDS。</summary>
        BitFields = 3
    }

    /// <summary>Specifies GIF quantization and color-table sizes. 指定 GIF 量化及色表大小。</summary>
    public enum ImwriteGifCompression
    {
        /// <summary>Fast quantization without dithering. 快速无抖动量化。</summary>
        FastNoDither = 1,
        /// <summary>Fast Floyd-Steinberg dithering. 快速 Floyd-Steinberg 抖动。</summary>
        FastFloydDither = 2,
        /// <summary>8-entry color table. 8 项色表。</summary>
        ColorTableSize8 = 3,
        /// <summary>16-entry color table. 16 项色表。</summary>
        ColorTableSize16 = 4,
        /// <summary>32-entry color table. 32 项色表。</summary>
        ColorTableSize32 = 5,
        /// <summary>64-entry color table. 64 项色表。</summary>
        ColorTableSize64 = 6,
        /// <summary>128-entry color table. 128 项色表。</summary>
        ColorTableSize128 = 7,
        /// <summary>256-entry color table. 256 项色表。</summary>
        ColorTableSize256 = 8
    }
}
