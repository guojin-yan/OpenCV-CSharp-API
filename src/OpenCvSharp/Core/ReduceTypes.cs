namespace OpenCvSharp.Core
{
    /// <summary>
    /// Specifies reduction operations compatible with OpenCV <c>cv::ReduceTypes</c>.
    /// 指定与 OpenCV <c>cv::ReduceTypes</c> 兼容的降维归约操作。
    /// </summary>
    public enum ReduceTypes
    {
        /// <summary>
        /// Sums values along the selected dimension.
        /// 沿指定维度求和。
        /// </summary>
        Sum = 0,

        /// <summary>
        /// Averages values along the selected dimension.
        /// 沿指定维度求平均值。
        /// </summary>
        Avg = 1,

        /// <summary>
        /// Finds the maximum value along the selected dimension.
        /// 沿指定维度查找最大值。
        /// </summary>
        Max = 2,

        /// <summary>
        /// Finds the minimum value along the selected dimension.
        /// 沿指定维度查找最小值。
        /// </summary>
        Min = 3
    }
}
