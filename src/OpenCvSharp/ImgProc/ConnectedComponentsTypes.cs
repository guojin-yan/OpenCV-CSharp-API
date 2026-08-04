namespace JYPPX.OpenCvSharp.ImgProc
{
    /// <summary>
    /// Specifies connected-components statistics columns compatible with OpenCV <c>cv::ConnectedComponentsTypes</c>.
    /// 指定与 OpenCV <c>cv::ConnectedComponentsTypes</c> 兼容的连通域统计列。
    /// </summary>
    public enum ConnectedComponentsTypes
    {
        /// <summary>
        /// Left coordinate of the bounding box, equivalent to <c>cv::CC_STAT_LEFT</c>.
        /// 外接矩形左侧坐标，等价于 <c>cv::CC_STAT_LEFT</c>。
        /// </summary>
        Left = 0,

        /// <summary>
        /// Top coordinate of the bounding box, equivalent to <c>cv::CC_STAT_TOP</c>.
        /// 外接矩形顶部坐标，等价于 <c>cv::CC_STAT_TOP</c>。
        /// </summary>
        Top = 1,

        /// <summary>
        /// Width of the bounding box, equivalent to <c>cv::CC_STAT_WIDTH</c>.
        /// 外接矩形宽度，等价于 <c>cv::CC_STAT_WIDTH</c>。
        /// </summary>
        Width = 2,

        /// <summary>
        /// Height of the bounding box, equivalent to <c>cv::CC_STAT_HEIGHT</c>.
        /// 外接矩形高度，等价于 <c>cv::CC_STAT_HEIGHT</c>。
        /// </summary>
        Height = 3,

        /// <summary>
        /// Area in pixels, equivalent to <c>cv::CC_STAT_AREA</c>.
        /// 像素面积，等价于 <c>cv::CC_STAT_AREA</c>。
        /// </summary>
        Area = 4,

        /// <summary>
        /// Maximum column count marker, equivalent to <c>cv::CC_STAT_MAX</c>.
        /// 最大列数标记，等价于 <c>cv::CC_STAT_MAX</c>。
        /// </summary>
        Max = 5
    }
}
