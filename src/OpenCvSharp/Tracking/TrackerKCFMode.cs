using System;

namespace JYPPX.OpenCvSharp.Tracking
{
    /// <summary>
    /// Feature descriptor flags for KCF trackers.
    /// KCF 跟踪器特征描述子标志。
    /// </summary>
    [Flags]
    public enum TrackerKCFMode
    {
        /// <summary>Use grayscale values. 使用灰度值。</summary>
        Gray = 1,

        /// <summary>Use color-names features. 使用颜色名称特征。</summary>
        Cn = 2,

        /// <summary>Use a custom extractor. 使用自定义特征提取器。</summary>
        Custom = 4
    }
}
