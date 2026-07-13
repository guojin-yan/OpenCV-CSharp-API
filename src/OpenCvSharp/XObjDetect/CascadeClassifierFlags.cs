using System;

namespace OpenCvSharp.XObjDetect
{
    /// <summary>
    /// Flags for legacy cascade classifier detection.
    /// 旧式级联分类器检测标志。
    /// </summary>
    [Flags]
    public enum CascadeClassifierFlags
    {
        /// <summary>No flags. 无标志。</summary>
        None = 0,

        /// <summary>Enable Canny pruning. 启用 Canny 裁剪。</summary>
        DoCannyPruning = 1,

        /// <summary>Scale the image. 缩放图像。</summary>
        ScaleImage = 2,

        /// <summary>Find only the biggest object. 只查找最大目标。</summary>
        FindBiggestObject = 4,

        /// <summary>Use rough search. 使用粗略搜索。</summary>
        DoRoughSearch = 8
    }
}
