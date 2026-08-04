namespace JYPPX.OpenCvSharp.Features2D
{
    /// <summary>
    /// Specifies OpenCV descriptor matcher factory types compatible with <c>cv::DescriptorMatcher::MatcherType</c>.
    /// 指定与 OpenCV <c>cv::DescriptorMatcher::MatcherType</c> 兼容的描述子匹配器工厂类型。
    /// </summary>
    public enum DescriptorMatcherType
    {
        /// <summary>
        /// FLANN based matcher.
        /// 基于 FLANN 的匹配器。
        /// </summary>
        FlannBased = 1,

        /// <summary>
        /// Brute-force matcher with L2 norm.
        /// 使用 L2 范数的暴力匹配器。
        /// </summary>
        BruteForce = 2,

        /// <summary>
        /// Brute-force matcher with L1 norm.
        /// 使用 L1 范数的暴力匹配器。
        /// </summary>
        BruteForceL1 = 3,

        /// <summary>
        /// Brute-force matcher with Hamming norm.
        /// 使用 Hamming 范数的暴力匹配器。
        /// </summary>
        BruteForceHamming = 4,

        /// <summary>
        /// Brute-force matcher with lookup-table Hamming norm.
        /// 使用查找表 Hamming 范数的暴力匹配器。
        /// </summary>
        BruteForceHammingLut = 5,

        /// <summary>
        /// Brute-force matcher with squared L2 norm.
        /// 使用平方 L2 范数的暴力匹配器。
        /// </summary>
        BruteForceSL2 = 6
    }
}
