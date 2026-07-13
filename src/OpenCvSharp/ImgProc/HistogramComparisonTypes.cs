namespace OpenCvSharp.ImgProc
{
    /// <summary>
    /// Specifies histogram comparison methods compatible with OpenCV <c>cv::HistCompMethods</c>.
    /// 指定与 OpenCV <c>cv::HistCompMethods</c> 兼容的直方图比较方法。
    /// </summary>
    public enum HistogramComparisonTypes
    {
        /// <summary>
        /// Correlation comparison.
        /// 相关性比较。
        /// </summary>
        Correl = 0,

        /// <summary>
        /// Chi-square comparison.
        /// 卡方比较。
        /// </summary>
        ChiSquare = 1,

        /// <summary>
        /// Histogram intersection comparison.
        /// 直方图交集比较。
        /// </summary>
        Intersect = 2,

        /// <summary>
        /// Bhattacharyya or Hellinger distance comparison.
        /// Bhattacharyya 或 Hellinger 距离比较。
        /// </summary>
        Bhattacharyya = 3,

        /// <summary>
        /// Hellinger distance comparison.
        /// Hellinger 距离比较。
        /// </summary>
        Hellinger = Bhattacharyya,

        /// <summary>
        /// Alternative chi-square comparison.
        /// 替代卡方比较。
        /// </summary>
        ChiSquareAlt = 4,

        /// <summary>
        /// Kullback-Leibler divergence comparison.
        /// Kullback-Leibler 散度比较。
        /// </summary>
        KlDiv = 5
    }
}
