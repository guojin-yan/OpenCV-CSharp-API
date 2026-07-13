namespace OpenCvSharp.XStereo
{
    /// <summary>
    /// Speckle removal algorithms for OpenCV xstereo matchers.
    /// OpenCV xstereo matcher 的 speckle 去除算法。
    /// </summary>
    public enum StereoSpeckleRemovalAlgorithm
    {
        /// <summary>Default speckle removal algorithm. 默认 speckle 去除算法。</summary>
        Default = 0,

        /// <summary>Average-based speckle removal algorithm. 基于平均值的 speckle 去除算法。</summary>
        Average = 1
    }
}
