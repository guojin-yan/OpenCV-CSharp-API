namespace OpenCvSharp.Core
{
    /// <summary>
    /// Describes whether every matrix value lies in a half-open numeric range and where the first failure occurred.
    /// 描述矩阵中的每个值是否位于半开数值区间内，以及第一个失败位置。
    /// </summary>
    public readonly struct CheckRangeResult
    {
        /// <summary>Initializes a range-check result.</summary>
        public CheckRangeResult(bool isValid, Point position)
        {
            IsValid = isValid;
            Position = position;
        }

        /// <summary>Gets whether every value was finite and inside the requested range.</summary>
        public bool IsValid { get; }

        /// <summary>
        /// Gets the first invalid element position, or (-1,-1) when the matrix is valid.
        /// 获取第一个无效元素的位置；矩阵有效时为 (-1,-1)。
        /// </summary>
        public Point Position { get; }
    }
}
