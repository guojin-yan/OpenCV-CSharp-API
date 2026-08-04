namespace JYPPX.OpenCvSharp.Stitching
{
    /// <summary>
    /// Wave correction kind compatible with OpenCV <c>cv::detail::WaveCorrectKind</c>.
    /// 与 OpenCV <c>cv::detail::WaveCorrectKind</c> 兼容的波形校正类型。
    /// </summary>
    public enum WaveCorrectKind
    {
        /// <summary>Horizontal wave correction. 水平方向波形校正。</summary>
        Horizontal = 0,
        /// <summary>Vertical wave correction. 垂直方向波形校正。</summary>
        Vertical = 1,
        /// <summary>Automatically detect correction direction. 自动检测校正方向。</summary>
        Auto = 2
    }
}
