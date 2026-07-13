namespace OpenCvSharp.ImgCodecs
{
    /// <summary>
    /// Specifies how an image buffer should be decoded by OpenCV.
    /// 指定 OpenCV 解码图像缓冲区的方式。
    /// </summary>
    public enum ImreadModes
    {
        /// <summary>
        /// Returns the loaded image as-is, including alpha channel when present.
        /// 按原样返回加载的图像，存在 Alpha 通道时保留。
        /// </summary>
        Unchanged = -1,

        /// <summary>
        /// Converts the image to a single-channel grayscale image.
        /// 将图像转换为单通道灰度图像。
        /// </summary>
        Grayscale = 0,

        /// <summary>
        /// Converts the image to a three-channel BGR color image.
        /// 将图像转换为三通道 BGR 彩色图像。
        /// </summary>
        Color = 1,

        /// <summary>
        /// Returns 16-bit or 32-bit image data when the input contains it.
        /// 输入包含 16 位或 32 位数据时按对应深度返回。
        /// </summary>
        AnyDepth = 2,

        /// <summary>
        /// Reads the image in any possible color format.
        /// 以任何可能的颜色格式读取图像。
        /// </summary>
        AnyColor = 4,

        /// <summary>
        /// Uses the GDAL driver for loading the image when OpenCV was built with GDAL.
        /// 当 OpenCV 构建启用 GDAL 时，使用 GDAL 驱动加载图像。
        /// </summary>
        LoadGdal = 8,

        /// <summary>
        /// Converts to grayscale and reduces the image size by 1/2.
        /// 转换为灰度图，并将图像尺寸缩小到 1/2。
        /// </summary>
        ReducedGrayscale2 = 16,

        /// <summary>
        /// Converts to BGR color and reduces the image size by 1/2.
        /// 转换为 BGR 彩色图，并将图像尺寸缩小到 1/2。
        /// </summary>
        ReducedColor2 = 17,

        /// <summary>
        /// Converts to grayscale and reduces the image size by 1/4.
        /// 转换为灰度图，并将图像尺寸缩小到 1/4。
        /// </summary>
        ReducedGrayscale4 = 32,

        /// <summary>
        /// Converts to BGR color and reduces the image size by 1/4.
        /// 转换为 BGR 彩色图，并将图像尺寸缩小到 1/4。
        /// </summary>
        ReducedColor4 = 33,

        /// <summary>
        /// Converts to grayscale and reduces the image size by 1/8.
        /// 转换为灰度图，并将图像尺寸缩小到 1/8。
        /// </summary>
        ReducedGrayscale8 = 64,

        /// <summary>
        /// Converts to BGR color and reduces the image size by 1/8.
        /// 转换为 BGR 彩色图，并将图像尺寸缩小到 1/8。
        /// </summary>
        ReducedColor8 = 65,

        /// <summary>
        /// Ignores EXIF orientation metadata.
        /// 忽略 EXIF 方向元数据。
        /// </summary>
        IgnoreOrientation = 128,

        /// <summary>
        /// Converts the image to a three-channel RGB color image.
        /// 将图像转换为三通道 RGB 彩色图像。
        /// </summary>
        ColorRgb = 256
    }
}
