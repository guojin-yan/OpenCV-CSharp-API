namespace OpenCvSharp.ImgProc
{
    /// <summary>
    /// Specifies contour retrieval modes compatible with OpenCV <c>cv::RetrievalModes</c>.
    /// 指定与 OpenCV <c>cv::RetrievalModes</c> 兼容的轮廓检索模式。
    /// </summary>
    public enum RetrievalModes
    {
        /// <summary>
        /// Retrieves only the extreme outer contours, equivalent to <c>cv::RETR_EXTERNAL</c>.
        /// 仅检索最外层轮廓，等价于 <c>cv::RETR_EXTERNAL</c>。
        /// </summary>
        External = 0,

        /// <summary>
        /// Retrieves all contours without establishing hierarchical relationships, equivalent to <c>cv::RETR_LIST</c>.
        /// 检索全部轮廓但不建立层级关系，等价于 <c>cv::RETR_LIST</c>。
        /// </summary>
        List = 1,

        /// <summary>
        /// Retrieves two-level hierarchy, equivalent to <c>cv::RETR_CCOMP</c>.
        /// 检索两级层级，等价于 <c>cv::RETR_CCOMP</c>。
        /// </summary>
        CComp = 2,

        /// <summary>
        /// Retrieves full hierarchy, equivalent to <c>cv::RETR_TREE</c>.
        /// 检索完整层级，等价于 <c>cv::RETR_TREE</c>。
        /// </summary>
        Tree = 3,

        /// <summary>
        /// Flood-fill based contour retrieval, equivalent to <c>cv::RETR_FLOODFILL</c>.
        /// 基于泛洪填充的轮廓检索，等价于 <c>cv::RETR_FLOODFILL</c>。
        /// </summary>
        FloodFill = 4
    }
}
