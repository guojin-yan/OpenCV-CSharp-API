namespace JYPPX.OpenCvSharp.ML
{
    /// <summary>
    /// Specifies how samples are laid out in a matrix.
    /// 指定样本在矩阵中的布局方式。
    /// </summary>
    public enum SampleTypes
    {
        /// <summary>Each sample is stored in one row. 每个样本占一行。</summary>
        RowSample = 0,

        /// <summary>Each sample is stored in one column. 每个样本占一列。</summary>
        ColSample = 1
    }
}
