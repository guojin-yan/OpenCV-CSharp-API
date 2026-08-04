namespace JYPPX.OpenCvSharp.XStereo
{
    /// <summary>
    /// Census descriptor kernel types from OpenCV xstereo.
    /// OpenCV xstereo census 描述子核类型。
    /// </summary>
    public enum CensusTransformType
    {
        /// <summary>Dense census. 稠密 census。</summary>
        Dense = 0,

        /// <summary>Sparse census. 稀疏 census。</summary>
        Sparse = 1,

        /// <summary>Center-symmetric census. 中心对称 census。</summary>
        CenterSymmetric = 2,

        /// <summary>Modified center-symmetric census. 修改的中心对称 census。</summary>
        ModifiedCenterSymmetric = 3,

        /// <summary>Modified census transform. 修改的 census transform。</summary>
        Modified = 4,

        /// <summary>Mean variation census. 均值变化 census。</summary>
        MeanVariation = 5,

        /// <summary>Star kernel census. star kernel census。</summary>
        StarKernel = 6
    }
}
