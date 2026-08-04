namespace JYPPX.OpenCvSharp.OptFlow
{
    /// <summary>
    /// RLOF iterative solver types.
    /// RLOF 迭代求解器类型。
    /// </summary>
    public enum OptFlowSolverType
    {
        /// <summary>OpenCV constant <c>ST_STANDART</c>; spelling follows the upstream header. OpenCV 常量 <c>ST_STANDART</c>，拼写保持与上游一致。</summary>
        Standart = 0,

        /// <summary>Bilinear optimized solver. 双线性优化求解器。</summary>
        Bilinear = 1
    }
}
