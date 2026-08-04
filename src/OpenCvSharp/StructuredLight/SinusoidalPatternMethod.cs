namespace JYPPX.OpenCvSharp.StructuredLight
{
    /// <summary>
    /// Sinusoidal structured-light profilometry method.
    /// 正弦结构光轮廓测量方法。
    /// </summary>
    public enum SinusoidalPatternMethod
    {
        /// <summary>Fourier transform profilometry. 傅里叶变换轮廓测量。</summary>
        Ftp = 0,

        /// <summary>Phase-shifting profilometry. 相移轮廓测量。</summary>
        Psp = 1,

        /// <summary>Fourier-assisted phase-shifting profilometry. 傅里叶辅助相移轮廓测量。</summary>
        Faps = 2
    }
}
