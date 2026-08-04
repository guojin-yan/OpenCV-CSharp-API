namespace JYPPX.OpenCvSharp.ImgProc
{
    /// <summary>Template matching comparison modes. 模板匹配比较模式。</summary>
    public enum TemplateMatchModes
    {
        /// <summary>Squared difference. 平方差。</summary>
        SqDiff = 0,
        /// <summary>Normalized squared difference. 归一化平方差。</summary>
        SqDiffNormed = 1,
        /// <summary>Cross correlation. 互相关。</summary>
        CCorr = 2,
        /// <summary>Normalized cross correlation. 归一化互相关。</summary>
        CCorrNormed = 3,
        /// <summary>Correlation coefficient. 相关系数。</summary>
        CCoeff = 4,
        /// <summary>Normalized correlation coefficient. 归一化相关系数。</summary>
        CCoeffNormed = 5
    }
}
