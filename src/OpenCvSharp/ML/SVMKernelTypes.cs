namespace JYPPX.OpenCvSharp.ML
{
    /// <summary>
    /// Specifies SVM kernel types.
    /// 指定 SVM 核函数类型。
    /// </summary>
    public enum SVMKernelTypes
    {
        /// <summary>Custom kernel. 自定义核函数。</summary>
        Custom = -1,

        /// <summary>Linear kernel. 线性核。</summary>
        Linear = 0,

        /// <summary>Polynomial kernel. 多项式核。</summary>
        Poly = 1,

        /// <summary>Radial basis function kernel. 径向基函数核。</summary>
        Rbf = 2,

        /// <summary>Sigmoid kernel. Sigmoid 核。</summary>
        Sigmoid = 3,

        /// <summary>Chi-square kernel. 卡方核。</summary>
        Chi2 = 4,

        /// <summary>Histogram intersection kernel. 直方图交叉核。</summary>
        Inter = 5
    }
}
