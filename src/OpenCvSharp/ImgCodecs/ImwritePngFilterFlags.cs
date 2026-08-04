using System;

namespace JYPPX.OpenCvSharp.ImgCodecs
{
    /// <summary>
    /// Specifies PNG filter values for <see cref="ImwriteFlags.PngFilter"/>.
    /// 指定 <see cref="ImwriteFlags.PngFilter"/> 使用的 PNG 过滤器值。
    /// </summary>
    [Flags]
    public enum ImwritePngFilterFlags
    {
        /// <summary>
        /// Applies no PNG filter. 不应用 PNG 过滤器。
        /// </summary>
        None = 8,

        /// <summary>
        /// Applies the sub filter. 应用 sub 过滤器。
        /// </summary>
        Sub = 16,

        /// <summary>
        /// Applies the up filter. 应用 up 过滤器。
        /// </summary>
        Up = 32,

        /// <summary>
        /// Applies the average filter. 应用 average 过滤器。
        /// </summary>
        Avg = 64,

        /// <summary>
        /// Applies the Paeth filter. 应用 Paeth 过滤器。
        /// </summary>
        Paeth = 128,

        /// <summary>
        /// Uses the fast PNG filters. 使用快速 PNG 过滤器组合。
        /// </summary>
        FastFilters = None | Sub | Up,

        /// <summary>
        /// Uses all PNG filters. 使用所有 PNG 过滤器组合。
        /// </summary>
        AllFilters = FastFilters | Avg | Paeth
    }
}
