namespace OpenCvSharp.Photo
{
    /// <summary>
    /// Seamless clone modes from OpenCV photo module.
    /// OpenCV photo 模块的 seamless clone 模式。
    /// </summary>
    public enum SeamlessCloneFlags
    {
        /// <summary>Normal seamless cloning. 普通 seamless cloning。</summary>
        NormalClone = 1,

        /// <summary>Mixed seamless cloning. 混合 seamless cloning。</summary>
        MixedClone = 2,

        /// <summary>Monochrome transfer cloning. 单色迁移 cloning。</summary>
        MonochromeTransfer = 3,

        /// <summary>Wide-region normal seamless cloning. 宽区域普通 seamless cloning。</summary>
        NormalCloneWide = 9,

        /// <summary>Wide-region mixed seamless cloning. 宽区域混合 seamless cloning。</summary>
        MixedCloneWide = 10,

        /// <summary>Wide-region monochrome transfer cloning. 宽区域单色迁移 cloning。</summary>
        MonochromeTransferWide = 11
    }
}
