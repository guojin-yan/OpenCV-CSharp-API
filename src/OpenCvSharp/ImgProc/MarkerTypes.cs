namespace OpenCvSharp.ImgProc
{
    /// <summary>Specifies marker shapes used by <see cref="Cv2.DrawMarker"/>. 指定绘制标记使用的形状。</summary>
    public enum MarkerTypes
    {
        /// <summary>Cross marker.</summary>
        Cross = 0,
        /// <summary>Forty-five degree cross marker.</summary>
        TiltedCross = 1,
        /// <summary>Star marker.</summary>
        Star = 2,
        /// <summary>Diamond marker.</summary>
        Diamond = 3,
        /// <summary>Square marker.</summary>
        Square = 4,
        /// <summary>Upward triangle marker.</summary>
        TriangleUp = 5,
        /// <summary>Downward triangle marker.</summary>
        TriangleDown = 6
    }
}
