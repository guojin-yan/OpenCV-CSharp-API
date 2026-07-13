namespace OpenCvSharp.ImgProc
{
    /// <summary>
    /// Specifies connected-components labeling algorithms compatible with OpenCV <c>cv::ConnectedComponentsAlgorithmsTypes</c>.
    /// 指定与 OpenCV <c>cv::ConnectedComponentsAlgorithmsTypes</c> 兼容的连通域标记算法。
    /// </summary>
    public enum ConnectedComponentsAlgorithmsTypes
    {
        /// <summary>
        /// Default OpenCV algorithm, equivalent to <c>cv::CCL_DEFAULT</c>.
        /// OpenCV 默认算法，等价于 <c>cv::CCL_DEFAULT</c>。
        /// </summary>
        Default = -1,

        /// <summary>
        /// Wu SAUF algorithm, equivalent to <c>cv::CCL_WU</c>.
        /// Wu SAUF 算法，等价于 <c>cv::CCL_WU</c>。
        /// </summary>
        Wu = 0,

        /// <summary>
        /// Grana BBDT algorithm, equivalent to <c>cv::CCL_GRANA</c>.
        /// Grana BBDT 算法，等价于 <c>cv::CCL_GRANA</c>。
        /// </summary>
        Grana = 1,

        /// <summary>
        /// Bolelli Spaghetti algorithm, equivalent to <c>cv::CCL_BOLELLI</c>.
        /// Bolelli Spaghetti 算法，等价于 <c>cv::CCL_BOLELLI</c>。
        /// </summary>
        Bolelli = 2,

        /// <summary>
        /// SAUF algorithm alias, equivalent to <c>cv::CCL_SAUF</c>.
        /// SAUF 算法别名，等价于 <c>cv::CCL_SAUF</c>。
        /// </summary>
        Sauf = 3,

        /// <summary>
        /// BBDT algorithm alias, equivalent to <c>cv::CCL_BBDT</c>.
        /// BBDT 算法别名，等价于 <c>cv::CCL_BBDT</c>。
        /// </summary>
        Bbdt = 4,

        /// <summary>
        /// Spaghetti algorithm alias, equivalent to <c>cv::CCL_SPAGHETTI</c>.
        /// Spaghetti 算法别名，等价于 <c>cv::CCL_SPAGHETTI</c>。
        /// </summary>
        Spaghetti = 5
    }
}
