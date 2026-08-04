namespace JYPPX.OpenCvSharp.Calib3D
{
    /// <summary>Specifies the USAC local-optimization strategy. 指定 USAC 局部优化策略。</summary>
    public enum UsacLocalOptimizationMethod
    {
        /// <summary>Disables local optimization. 禁用局部优化。</summary>
        None = 0,
        /// <summary>Uses inner local optimization. 使用内部局部优化。</summary>
        Inner = 1,
        /// <summary>Uses inner and iterative local optimization. 使用内部及迭代局部优化。</summary>
        InnerAndIterative = 2,
        /// <summary>Uses graph-cut local optimization. 使用图割局部优化。</summary>
        GraphCut = 3,
        /// <summary>Uses sigma-consensus local optimization. 使用 sigma-consensus 局部优化。</summary>
        Sigma = 4
    }
}
