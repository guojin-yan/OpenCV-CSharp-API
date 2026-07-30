namespace OpenCvSharp.Calib3D
{
    /// <summary>Specifies the USAC model score. 指定 USAC 模型评分方法。</summary>
    public enum UsacScoreMethod
    {
        /// <summary>RANSAC score. RANSAC 评分。</summary>
        Ransac = 0,
        /// <summary>MSAC score. MSAC 评分。</summary>
        Msac = 1,
        /// <summary>MAGSAC score. MAGSAC 评分。</summary>
        Magsac = 2,
        /// <summary>Least-median score. 最小中值评分。</summary>
        Lmeds = 3
    }
}
