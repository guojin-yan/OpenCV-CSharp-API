namespace OpenCvSharp.XStereo
{
    /// <summary>
    /// Sub-pixel interpolation methods for xstereo disparities.
    /// xstereo 视差的亚像素插值方式。
    /// </summary>
    public enum StereoSubPixelInterpolationMethod
    {
        /// <summary>Quadratic interpolation. 二次插值。</summary>
        Quadratic = 0,

        /// <summary>Symmetric interpolation. 对称插值。</summary>
        Symmetric = 1
    }
}
