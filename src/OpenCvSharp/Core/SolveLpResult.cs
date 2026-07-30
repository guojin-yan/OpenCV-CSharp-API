namespace OpenCvSharp.Core
{
    /// <summary>
    /// Specifies the result of OpenCV linear programming compatible with <c>cv::SolveLPResult</c>.
    /// 指定与 OpenCV <c>cv::SolveLPResult</c> 兼容的线性规划结果。
    /// </summary>
    public enum SolveLpResult
    {
        /// <summary>The computed solution failed the final constraint check. 计算结果未通过最终约束检查。</summary>
        Lost = -3,

        /// <summary>The objective is unbounded. 目标函数无界。</summary>
        Unbounded = -2,

        /// <summary>The constraints are infeasible. 约束不可行。</summary>
        Unfeasible = -1,

        /// <summary>A unique optimal solution was found. 找到唯一最优解。</summary>
        Single = 0,

        /// <summary>Multiple optimal solutions exist. 存在多个最优解。</summary>
        Multiple = 1
    }
}
