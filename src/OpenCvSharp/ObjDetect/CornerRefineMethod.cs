namespace OpenCvSharp.ObjDetect
{
    /// <summary>
    /// Corner refinement methods used by ArUco detection.
    /// ArUco 检测使用的角点细化方法。
    /// </summary>
    public enum CornerRefineMethod
    {
        /// <summary>No corner refinement. 不进行角点细化。</summary>
        None = 0,

        /// <summary>Sub-pixel corner refinement. 亚像素角点细化。</summary>
        Subpix = 1,

        /// <summary>Contour line-fitting refinement. 基于轮廓线拟合的细化。</summary>
        Contour = 2,

        /// <summary>AprilTag-style corner refinement. AprilTag 风格角点细化。</summary>
        AprilTag = 3
    }
}
