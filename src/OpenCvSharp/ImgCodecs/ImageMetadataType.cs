namespace JYPPX.OpenCvSharp.ImgCodecs
{
    /// <summary>Identifies an image metadata chunk type. 标识图像元数据块类型。</summary>
    public enum ImageMetadataType
    {
        /// <summary>Unknown or unset metadata. 未知或未设置的元数据。</summary>
        Unknown = -1,
        /// <summary>EXIF metadata. EXIF 元数据。</summary>
        Exif = 0,
        /// <summary>XMP metadata. XMP 元数据。</summary>
        Xmp = 1,
        /// <summary>ICC profile metadata. ICC 配置文件元数据。</summary>
        Iccp = 2,
        /// <summary>cICP profile metadata. cICP 配置文件元数据。</summary>
        Cicp = 3,
        /// <summary>Highest recognized metadata value. 最高已识别元数据值。</summary>
        Max = Cicp
    }
}
