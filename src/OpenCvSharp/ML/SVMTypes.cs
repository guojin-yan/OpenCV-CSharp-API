namespace OpenCvSharp.ML
{
    /// <summary>
    /// Specifies SVM formulation types.
    /// 指定 SVM 公式类型。
    /// </summary>
    public enum SVMTypes
    {
        /// <summary>C-support vector classification. C 支持向量分类。</summary>
        CSvc = 100,

        /// <summary>Nu-support vector classification. Nu 支持向量分类。</summary>
        NuSvc = 101,

        /// <summary>One-class SVM. 单类 SVM。</summary>
        OneClass = 102,

        /// <summary>Epsilon-support vector regression. Epsilon 支持向量回归。</summary>
        EpsSvr = 103,

        /// <summary>Nu-support vector regression. Nu 支持向量回归。</summary>
        NuSvr = 104
    }
}
