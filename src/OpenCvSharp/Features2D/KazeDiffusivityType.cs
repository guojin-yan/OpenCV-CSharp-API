namespace JYPPX.OpenCvSharp.Features2D
{
    /// <summary>
    /// Specifies KAZE diffusivity types compatible with <c>cv::xfeatures2d::KAZE::DiffusivityType</c>.
    /// 指定与 <c>cv::xfeatures2d::KAZE::DiffusivityType</c> 兼容的 KAZE 扩散类型。
    /// </summary>
    public enum KazeDiffusivityType
    {
        /// <summary>
        /// Perona-Malik diffusion 1.
        /// Perona-Malik 扩散 1。
        /// </summary>
        DiffPmG1 = 0,

        /// <summary>
        /// Perona-Malik diffusion 2.
        /// Perona-Malik 扩散 2。
        /// </summary>
        DiffPmG2 = 1,

        /// <summary>
        /// Weickert diffusivity.
        /// Weickert 扩散类型。
        /// </summary>
        DiffWeickert = 2,

        /// <summary>
        /// Charbonnier diffusivity.
        /// Charbonnier 扩散类型。
        /// </summary>
        DiffCharbonnier = 3
    }
}
