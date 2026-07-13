namespace OpenCvSharp.Features2D
{
    /// <summary>
    /// Specifies AKAZE descriptor types compatible with <c>cv::xfeatures2d::AKAZE::DescriptorType</c>.
    /// 指定与 <c>cv::xfeatures2d::AKAZE::DescriptorType</c> 兼容的 AKAZE 描述子类型。
    /// </summary>
    public enum AkazeDescriptorType
    {
        /// <summary>
        /// Upright KAZE descriptor.
        /// 正立 KAZE 描述子。
        /// </summary>
        DescriptorKazeUpright = 2,

        /// <summary>
        /// Rotation-invariant KAZE descriptor.
        /// 旋转不变 KAZE 描述子。
        /// </summary>
        DescriptorKaze = 3,

        /// <summary>
        /// Upright MLDB descriptor.
        /// 正立 MLDB 描述子。
        /// </summary>
        DescriptorMldbUpright = 4,

        /// <summary>
        /// Rotation-invariant MLDB descriptor.
        /// 旋转不变 MLDB 描述子。
        /// </summary>
        DescriptorMldb = 5
    }
}
