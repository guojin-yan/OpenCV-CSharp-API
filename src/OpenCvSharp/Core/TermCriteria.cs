using System;
using System.Globalization;

namespace JYPPX.OpenCvSharp.Core
{
    /// <summary>
    /// Represents OpenCV iteration termination criteria compatible with <c>cv::TermCriteria</c>.
    /// 表示与 OpenCV <c>cv::TermCriteria</c> 兼容的迭代终止条件。
    /// </summary>
    public readonly struct TermCriteria : IEquatable<TermCriteria>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TermCriteria"/> struct.
        /// 初始化 <see cref="TermCriteria"/> 结构的新实例。
        /// </summary>
        /// <param name="type">The termination criteria type flags. 终止条件类型标志。</param>
        /// <param name="maxCount">The maximum iteration count. 最大迭代次数。</param>
        /// <param name="epsilon">The requested accuracy. 请求精度。</param>
        public TermCriteria(TermCriteriaTypes type, int maxCount, double epsilon)
        {
            Type = type;
            MaxCount = maxCount;
            Epsilon = epsilon;
        }

        /// <summary>
        /// Gets the termination criteria type flags.
        /// 获取终止条件类型标志。
        /// </summary>
        public TermCriteriaTypes Type { get; }

        /// <summary>
        /// Gets the maximum iteration count.
        /// 获取最大迭代次数。
        /// </summary>
        public int MaxCount { get; }

        /// <summary>
        /// Gets the requested accuracy.
        /// 获取请求精度。
        /// </summary>
        public double Epsilon { get; }

        /// <summary>
        /// Creates a criteria that stops after a fixed number of iterations.
        /// 创建按固定迭代次数停止的条件。
        /// </summary>
        /// <param name="maxCount">The maximum iteration count. 最大迭代次数。</param>
        /// <returns>The created criteria. 创建的条件。</returns>
        public static TermCriteria ByCount(int maxCount)
        {
            return new TermCriteria(TermCriteriaTypes.Count, maxCount, 0.0);
        }

        /// <summary>
        /// Creates a criteria that stops after reaching an accuracy threshold.
        /// 创建达到精度阈值后停止的条件。
        /// </summary>
        /// <param name="epsilon">The requested accuracy. 请求精度。</param>
        /// <returns>The created criteria. 创建的条件。</returns>
        public static TermCriteria ByEpsilon(double epsilon)
        {
            return new TermCriteria(TermCriteriaTypes.Eps, 0, epsilon);
        }

        /// <summary>
        /// Creates a criteria that stops by count or accuracy.
        /// 创建按次数或精度停止的条件。
        /// </summary>
        /// <param name="maxCount">The maximum iteration count. 最大迭代次数。</param>
        /// <param name="epsilon">The requested accuracy. 请求精度。</param>
        /// <returns>The created criteria. 创建的条件。</returns>
        public static TermCriteria ByCountAndEpsilon(int maxCount, double epsilon)
        {
            return new TermCriteria(TermCriteriaTypes.CountOrEps, maxCount, epsilon);
        }

        /// <summary>
        /// Determines whether two criteria are equal.
        /// 判断两个终止条件是否相等。
        /// </summary>
        /// <param name="left">The first criteria. 第一个条件。</param>
        /// <param name="right">The second criteria. 第二个条件。</param>
        /// <returns><c>true</c> if both criteria are equal; otherwise, <c>false</c>. 如果两个条件相等则返回 <c>true</c>，否则返回 <c>false</c>。</returns>
        public static bool operator ==(TermCriteria left, TermCriteria right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two criteria are different.
        /// 判断两个终止条件是否不同。
        /// </summary>
        /// <param name="left">The first criteria. 第一个条件。</param>
        /// <param name="right">The second criteria. 第二个条件。</param>
        /// <returns><c>true</c> if any value differs; otherwise, <c>false</c>. 如果任一值不同则返回 <c>true</c>，否则返回 <c>false</c>。</returns>
        public static bool operator !=(TermCriteria left, TermCriteria right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Indicates whether this criteria equals another criteria.
        /// 指示此条件是否与另一个条件相等。
        /// </summary>
        /// <param name="other">The other criteria. 另一个条件。</param>
        /// <returns><c>true</c> if all values are equal; otherwise, <c>false</c>. 如果所有值相等则返回 <c>true</c>，否则返回 <c>false</c>。</returns>
        public bool Equals(TermCriteria other)
        {
            return Type == other.Type &&
                MaxCount == other.MaxCount &&
                Epsilon.Equals(other.Epsilon);
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is TermCriteria other && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = (int)Type;
                hash = (hash * 397) ^ MaxCount;
                hash = (hash * 397) ^ Epsilon.GetHashCode();
                return hash;
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "{Type=" + Type +
                ",MaxCount=" + MaxCount +
                ",Epsilon=" + Epsilon.ToString(CultureInfo.InvariantCulture) + "}";
        }
    }
}
