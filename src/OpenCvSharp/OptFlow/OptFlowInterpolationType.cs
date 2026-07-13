namespace OpenCvSharp.OptFlow
{
    /// <summary>
    /// RLOF dense interpolation modes.
    /// RLOF 密集插值模式。
    /// </summary>
    public enum OptFlowInterpolationType
    {
        /// <summary>Geodesic interpolation. 测地线插值。</summary>
        Geo = 0,

        /// <summary>Edge-preserving interpolation. 边缘保持插值。</summary>
        Epic = 1,

        /// <summary>Robust interpolation with superpixels. 基于超像素的鲁棒插值。</summary>
        Ric = 2
    }
}
