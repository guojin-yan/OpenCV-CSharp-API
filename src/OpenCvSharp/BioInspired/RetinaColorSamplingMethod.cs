namespace OpenCvSharp.BioInspired
{
    /// <summary>
    /// Color sampling methods used by OpenCV bioinspired Retina log sampling.
    /// OpenCV bioinspired Retina 对数采样使用的颜色采样方式。
    /// </summary>
    public enum RetinaColorSamplingMethod
    {
        /// <summary>Random RGB sampling. 随机 RGB 采样。</summary>
        Random = 0,

        /// <summary>Diagonal RGB sampling. 对角 RGB 采样。</summary>
        Diagonal = 1,

        /// <summary>Bayer-like sampling. Bayer 类采样。</summary>
        Bayer = 2
    }
}
