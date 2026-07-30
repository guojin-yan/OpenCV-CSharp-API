namespace OpenCvSharp.Calib3D
{
    /// <summary>Specifies the USAC neighborhood search. 指定 USAC 邻域搜索方法。</summary>
    public enum UsacNeighborSearchMethod
    {
        /// <summary>FLANN k-nearest-neighbor search. FLANN k 近邻搜索。</summary>
        FlannKNearest = 0,
        /// <summary>Grid search. 网格搜索。</summary>
        Grid = 1,
        /// <summary>FLANN radius search. FLANN 半径搜索。</summary>
        FlannRadius = 2
    }
}
