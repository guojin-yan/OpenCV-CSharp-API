namespace OpenCvSharp.Calib3D
{
    /// <summary>Specifies the USAC minimal-sample strategy. 指定 USAC 最小样本策略。</summary>
    public enum UsacSamplingMethod
    {
        /// <summary>Uniform random sampling. 均匀随机采样。</summary>
        Uniform = 0,
        /// <summary>Progressive NAPSAC sampling. 渐进式 NAPSAC 采样。</summary>
        ProgressiveNapsac = 1,
        /// <summary>NAPSAC sampling. NAPSAC 采样。</summary>
        Napsac = 2,
        /// <summary>PROSAC sampling. PROSAC 采样。</summary>
        Prosac = 3
    }
}
