namespace OpenCvSharp.Fuzzy
{
    /// <summary>
    /// Fuzzy inpainting algorithm mode.
    /// 模糊 inpainting 算法模式。
    /// </summary>
    public enum FuzzyInpaintAlgorithm
    {
        /// <summary>One-step algorithm. 单步算法。</summary>
        OneStep = 1,

        /// <summary>Multi-step algorithm. 多步算法。</summary>
        MultiStep = 2,

        /// <summary>Iterative algorithm. 迭代算法。</summary>
        Iterative = 3
    }
}
