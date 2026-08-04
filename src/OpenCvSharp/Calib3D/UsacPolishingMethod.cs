namespace JYPPX.OpenCvSharp.Calib3D
{
    /// <summary>Specifies final USAC model polishing. 指定最终 USAC 模型精修方法。</summary>
    public enum UsacPolishingMethod
    {
        /// <summary>Disables polishing. 禁用精修。</summary>
        None = 0,
        /// <summary>Least-squares polishing. 最小二乘精修。</summary>
        LeastSquares = 1,
        /// <summary>MAGSAC polishing. MAGSAC 精修。</summary>
        Magsac = 2,
        /// <summary>Covariance polishing. 协方差精修。</summary>
        Covariance = 3
    }
}
