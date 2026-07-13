using System;

namespace OpenCvSharp.Core
{
    /// <summary>
    /// Specifies termination criteria flags compatible with OpenCV <c>cv::TermCriteria::Type</c>.
    /// 指定与 OpenCV <c>cv::TermCriteria::Type</c> 兼容的终止条件标志。
    /// </summary>
    [Flags]
    public enum TermCriteriaTypes
    {
        /// <summary>
        /// Stop after the maximum iteration count.
        /// 达到最大迭代次数后停止。
        /// </summary>
        Count = 1,

        /// <summary>
        /// Stop after the maximum iteration count; synonym of <see cref="Count"/>.
        /// 达到最大迭代次数后停止；等同于 <see cref="Count"/>。
        /// </summary>
        MaxIter = Count,

        /// <summary>
        /// Stop after reaching the requested accuracy.
        /// 达到指定精度后停止。
        /// </summary>
        Eps = 2,

        /// <summary>
        /// Stop when either count or epsilon condition is satisfied.
        /// 当迭代次数或精度条件任一满足时停止。
        /// </summary>
        CountOrEps = Count | Eps
    }
}
