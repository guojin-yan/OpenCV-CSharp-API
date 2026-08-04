using System;

namespace JYPPX.OpenCvSharp.ImgProc
{
    /// <summary>Flags for OpenCV FontFace text rendering. OpenCV FontFace 文本渲染标志。</summary>
    [Flags]
    public enum PutTextFlags
    {
        /// <summary>Left alignment. 左对齐。</summary>
        AlignLeft = 0,
        /// <summary>Center alignment. 居中对齐。</summary>
        AlignCenter = 1,
        /// <summary>Right alignment. 右对齐。</summary>
        AlignRight = 2,
        /// <summary>Alignment bit mask. 对齐位掩码。</summary>
        AlignMask = 3,
        /// <summary>Top-left origin. 左上原点。</summary>
        OriginTopLeft = 0,
        /// <summary>Bottom-left origin. 左下原点。</summary>
        OriginBottomLeft = 32,
        /// <summary>Enables wrapping. 启用换行。</summary>
        Wrap = 128
    }
}
