namespace OpenCvSharp.ImgProc
{
    /// <summary>Specifies quad-edge navigation relative to an edge. 指定相对于一条边的 quad-edge 导航方向。</summary>
    public enum Subdiv2DEdgeNavigation
    {
        /// <summary>Next edge around the origin. 绕起点的下一条边。</summary>
        NextAroundOrigin = 0x00,
        /// <summary>Next edge around the destination. 绕终点的下一条边。</summary>
        NextAroundDestination = 0x22,
        /// <summary>Previous edge around the origin. 绕起点的上一条边。</summary>
        PreviousAroundOrigin = 0x11,
        /// <summary>Previous edge around the destination. 绕终点的上一条边。</summary>
        PreviousAroundDestination = 0x33,
        /// <summary>Next edge around the left facet. 绕左侧面的下一条边。</summary>
        NextAroundLeft = 0x13,
        /// <summary>Next edge around the right facet. 绕右侧面的下一条边。</summary>
        NextAroundRight = 0x31,
        /// <summary>Previous edge around the left facet. 绕左侧面的上一条边。</summary>
        PreviousAroundLeft = 0x20,
        /// <summary>Previous edge around the right facet. 绕右侧面的上一条边。</summary>
        PreviousAroundRight = 0x02
    }
}
