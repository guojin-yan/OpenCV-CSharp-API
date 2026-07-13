namespace OpenCvSharp.Geometry
{
    /// <summary>
    /// Specifies the intersection type between two rotated rectangles compatible with OpenCV's <c>cv::RectanglesIntersectTypes</c>.
    /// 指定与 OpenCV <c>cv::RectanglesIntersectTypes</c> 兼容的两个旋转矩形之间的相交类型。
    /// </summary>
    public enum RectanglesIntersectTypes
    {
        /// <summary>
        /// No intersection.
        /// 无交集。
        /// </summary>
        IntersectNone = 0,

        /// <summary>
        /// Partial intersection.
        /// 部分相交。
        /// </summary>
        IntersectPartial = 1,

        /// <summary>
        /// One rectangle fully contains the other.
        /// 一个矩形完全包含另一个矩形。
        /// </summary>
        IntersectFull = 2
    }
}
