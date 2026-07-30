namespace OpenCvSharp.ImgProc
{
    /// <summary>Describes where a point lies in a Delaunay subdivision. 描述点在 Delaunay 细分中的位置。</summary>
    public enum Subdiv2DPointLocation
    {
        /// <summary>The location operation failed. 定位操作失败。</summary>
        Error = -2,
        /// <summary>The point is outside the reference rectangle. 点位于参考矩形之外。</summary>
        OutsideRect = -1,
        /// <summary>The point is inside a facet. 点位于面内部。</summary>
        Inside = 0,
        /// <summary>The point coincides with a vertex. 点与顶点重合。</summary>
        Vertex = 1,
        /// <summary>The point lies on an edge. 点位于边上。</summary>
        OnEdge = 2
    }
}
