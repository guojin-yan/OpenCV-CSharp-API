namespace JYPPX.OpenCvSharp.Core
{
    /// <summary>
    /// Specifies center initialization flags for OpenCV <c>cv::kmeans</c>.
    /// 指定 OpenCV <c>cv::kmeans</c> 的聚类中心初始化标志。
    /// </summary>
    public enum KMeansFlags
    {
        /// <summary>
        /// Select random initial centers for each attempt.
        /// 每次尝试随机选择初始中心。
        /// </summary>
        RandomCenters = 0,

        /// <summary>
        /// Use the supplied best-labels matrix as initial labels.
        /// 使用传入的 best-labels 矩阵作为初始标签。
        /// </summary>
        UseInitialLabels = 1,

        /// <summary>
        /// Use k-means++ center initialization.
        /// 使用 k-means++ 初始化聚类中心。
        /// </summary>
        PpCenters = 2
    }
}
