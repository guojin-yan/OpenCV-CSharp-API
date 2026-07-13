using System;

namespace OpenCvSharp.LineDescriptor
{
    /// <summary>
    /// Drawing flags for line-descriptor keylines and matches.
    /// line_descriptor 关键线段与匹配结果绘制标志。
    /// </summary>
    [Flags]
    public enum DrawLinesMatchesFlags
    {
        /// <summary>Creates or clears the output image before drawing. 绘制前创建或清空输出图像。</summary>
        Default = 0,

        /// <summary>Draws over an existing output image. 在已有输出图像上绘制。</summary>
        DrawOverOutImg = 1,

        /// <summary>Does not draw single unmatched lines. 不绘制未匹配的单独线段。</summary>
        NotDrawSingleLines = 2
    }
}
