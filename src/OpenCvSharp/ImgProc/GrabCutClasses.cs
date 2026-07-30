namespace OpenCvSharp.ImgProc
{
    /// <summary>GrabCut mask labels. GrabCut 掩码标签。</summary>
    public enum GrabCutClasses : byte
    {
        /// <summary>Definite background. 确定背景。</summary>
        Background = 0,
        /// <summary>Definite foreground. 确定前景。</summary>
        Foreground = 1,
        /// <summary>Probable background. 可能背景。</summary>
        ProbableBackground = 2,
        /// <summary>Probable foreground. 可能前景。</summary>
        ProbableForeground = 3
    }
}
