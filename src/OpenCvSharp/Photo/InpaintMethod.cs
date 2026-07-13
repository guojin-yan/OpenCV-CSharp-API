namespace OpenCvSharp.Photo
{
    /// <summary>
    /// Inpainting algorithms from OpenCV photo module.
    /// OpenCV photo 模块的图像修复算法。
    /// </summary>
    public enum InpaintMethod
    {
        /// <summary>Navier-Stokes based inpainting. 基于 Navier-Stokes 的图像修复。</summary>
        Ns = 0,

        /// <summary>Telea inpainting algorithm. Telea 图像修复算法。</summary>
        Telea = 1
    }
}
