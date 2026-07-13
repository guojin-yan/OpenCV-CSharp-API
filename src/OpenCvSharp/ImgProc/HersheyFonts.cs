namespace OpenCvSharp.ImgProc
{
    /// <summary>
    /// Specifies Hershey font faces compatible with OpenCV <c>cv::HersheyFonts</c>.
    /// 指定与 OpenCV <c>cv::HersheyFonts</c> 兼容的 Hershey 字体。
    /// </summary>
    public enum HersheyFonts
    {
        /// <summary>
        /// Normal size sans-serif font, equivalent to <c>cv::FONT_HERSHEY_SIMPLEX</c>.
        /// 普通大小的无衬线字体，等价于 <c>cv::FONT_HERSHEY_SIMPLEX</c>。
        /// </summary>
        HersheySimplex = 0,

        /// <summary>
        /// Small size sans-serif font, equivalent to <c>cv::FONT_HERSHEY_PLAIN</c>.
        /// 小号无衬线字体，等价于 <c>cv::FONT_HERSHEY_PLAIN</c>。
        /// </summary>
        HersheyPlain = 1,

        /// <summary>
        /// Normal size sans-serif font with extra complexity, equivalent to <c>cv::FONT_HERSHEY_DUPLEX</c>.
        /// 较复杂的普通大小无衬线字体，等价于 <c>cv::FONT_HERSHEY_DUPLEX</c>。
        /// </summary>
        HersheyDuplex = 2,

        /// <summary>
        /// Normal size serif font, equivalent to <c>cv::FONT_HERSHEY_COMPLEX</c>.
        /// 普通大小的衬线字体，等价于 <c>cv::FONT_HERSHEY_COMPLEX</c>。
        /// </summary>
        HersheyComplex = 3,

        /// <summary>
        /// Normal size serif font with extra complexity, equivalent to <c>cv::FONT_HERSHEY_TRIPLEX</c>.
        /// 较复杂的普通大小衬线字体，等价于 <c>cv::FONT_HERSHEY_TRIPLEX</c>。
        /// </summary>
        HersheyTriplex = 4,

        /// <summary>
        /// Smaller version of the complex serif font, equivalent to <c>cv::FONT_HERSHEY_COMPLEX_SMALL</c>.
        /// 较小版本的复杂衬线字体，等价于 <c>cv::FONT_HERSHEY_COMPLEX_SMALL</c>。
        /// </summary>
        HersheyComplexSmall = 5,

        /// <summary>
        /// Hand-writing style font, equivalent to <c>cv::FONT_HERSHEY_SCRIPT_SIMPLEX</c>.
        /// 手写风格字体，等价于 <c>cv::FONT_HERSHEY_SCRIPT_SIMPLEX</c>。
        /// </summary>
        HersheyScriptSimplex = 6,

        /// <summary>
        /// More complex hand-writing style font, equivalent to <c>cv::FONT_HERSHEY_SCRIPT_COMPLEX</c>.
        /// 更复杂的手写风格字体，等价于 <c>cv::FONT_HERSHEY_SCRIPT_COMPLEX</c>。
        /// </summary>
        HersheyScriptComplex = 7,

        /// <summary>
        /// Italic font flag, equivalent to <c>cv::FONT_ITALIC</c>.
        /// 斜体字体标志，等价于 <c>cv::FONT_ITALIC</c>。
        /// </summary>
        Italic = 16
    }
}
