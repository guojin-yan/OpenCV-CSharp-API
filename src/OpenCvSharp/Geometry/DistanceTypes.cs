namespace OpenCvSharp.Geometry
{
    /// <summary>
    /// Specifies distance metrics compatible with OpenCV's <c>cv::DistanceTypes</c>.
    /// 指定与 OpenCV <c>cv::DistanceTypes</c> 兼容的距离度量。
    /// </summary>
    public enum DistanceTypes
    {
        /// <summary>
        /// User defined distance.
        /// 用户自定义距离。
        /// </summary>
        User = -1,

        /// <summary>
        /// Manhattan distance.
        /// 曼哈顿距离。
        /// </summary>
        L1 = 1,

        /// <summary>
        /// Euclidean distance.
        /// 欧几里得距离。
        /// </summary>
        L2 = 2,

        /// <summary>
        /// Chebyshev distance.
        /// 切比雪夫距离。
        /// </summary>
        C = 3,

        /// <summary>
        /// L1-L2 metric.
        /// L1-L2 度量。
        /// </summary>
        L12 = 4,

        /// <summary>
        /// Fair distance.
        /// Fair 距离。
        /// </summary>
        Fair = 5,

        /// <summary>
        /// Welsch distance.
        /// Welsch 距离。
        /// </summary>
        Welsch = 6,

        /// <summary>
        /// Huber distance.
        /// Huber 距离。
        /// </summary>
        Huber = 7
    }
}
