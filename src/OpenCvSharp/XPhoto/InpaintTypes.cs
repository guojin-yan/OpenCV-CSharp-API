namespace JYPPX.OpenCvSharp.XPhoto
{
    /// <summary>
    /// Inpainting algorithms supported by xphoto.
    /// xphoto 支持的图像修复算法。
    /// </summary>
    public enum InpaintTypes
    {
        /// <summary>Patch-based Shiftmap reconstruction. 基于图像块的 Shiftmap 重建。</summary>
        Shiftmap = 0,

        /// <summary>Frequency Selective Reconstruction, best quality. 频率选择性重建，最高质量。</summary>
        FsrBest = 1,

        /// <summary>Frequency Selective Reconstruction, faster profile. 频率选择性重建，快速模式。</summary>
        FsrFast = 2
    }
}
