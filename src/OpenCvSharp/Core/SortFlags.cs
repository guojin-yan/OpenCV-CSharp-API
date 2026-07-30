using System;

namespace OpenCvSharp.Core
{
    /// <summary>
    /// Specifies the axis and direction used by Core sorting operations.
    /// 指定 Core 排序操作使用的轴和方向。
    /// </summary>
    [Flags]
    public enum SortFlags
    {
        /// <summary>Sorts each row independently.</summary>
        EveryRow = 0,
        /// <summary>Sorts each column independently.</summary>
        EveryColumn = 1,
        /// <summary>Sorts values in ascending order.</summary>
        Ascending = 0,
        /// <summary>Sorts values in descending order.</summary>
        Descending = 16
    }
}
