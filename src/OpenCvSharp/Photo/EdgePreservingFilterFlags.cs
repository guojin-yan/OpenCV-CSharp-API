namespace JYPPX.OpenCvSharp.Photo
{
    /// <summary>
    /// Edge-preserving filter modes from OpenCV photo module.
    /// OpenCV photo 模块的边缘保持滤波模式。
    /// </summary>
    public enum EdgePreservingFilterFlags
    {
        /// <summary>Recursive filtering. 递归滤波。</summary>
        RecursiveFilter = 1,

        /// <summary>Normalized convolution filtering. 归一化卷积滤波。</summary>
        NormalizedConvolutionFilter = 2
    }
}
