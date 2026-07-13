namespace OpenCvSharp.PtCloud
{
    /// <summary>
    /// Normal-estimation methods for <see cref="RgbdNormals"/>.
    /// <see cref="RgbdNormals"/> 的法线估计方法。
    /// </summary>
    public enum RgbdNormalsMethod
    {
        /// <summary>Fast and accurate local surface normals. 快速准确的局部表面法线方法。</summary>
        Fals = 0,

        /// <summary>LINEMOD-style bilateral depth gradients. LINEMOD 风格的双边深度梯度方法。</summary>
        Linemod = 1,

        /// <summary>SRI normal estimation. SRI 法线估计方法。</summary>
        Sri = 2,

        /// <summary>Cross-product normal estimation. 叉积法线估计方法。</summary>
        CrossProduct = 3
    }
}
