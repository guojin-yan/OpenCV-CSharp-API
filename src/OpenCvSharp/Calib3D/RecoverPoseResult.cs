using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Calib3D
{
    /// <summary>
    /// Result metadata returned by recover-pose operations.
    /// recover-pose 操作返回的结果元数据。
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct RecoverPoseResult : IEquatable<RecoverPoseResult>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RecoverPoseResult"/> struct.
        /// 初始化 <see cref="RecoverPoseResult"/> 结构的新实例。
        /// </summary>
        /// <param name="inlierCount">The number of inliers accepted by the chirality check. 通过深度一致性检查的内点数量。</param>
        public RecoverPoseResult(int inlierCount)
        {
            if (inlierCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(inlierCount), "Inlier count cannot be negative.");
            }

            InlierCount = inlierCount;
        }

        /// <summary>
        /// Gets the number of inliers accepted by the chirality check.
        /// 获取通过深度一致性检查的内点数量。
        /// </summary>
        public int InlierCount { get; }

        /// <summary>
        /// Gets whether any inliers were accepted by the chirality check.
        /// 获取是否有内点通过深度一致性检查。
        /// </summary>
        public bool HasInliers
        {
            get { return InlierCount > 0; }
        }

        /// <summary>Returns whether two recover-pose results are equal. 返回两个 recover-pose 结果是否相等。</summary>
        public static bool operator ==(RecoverPoseResult left, RecoverPoseResult right)
        {
            return left.Equals(right);
        }

        /// <summary>Returns whether two recover-pose results are not equal. 返回两个 recover-pose 结果是否不相等。</summary>
        public static bool operator !=(RecoverPoseResult left, RecoverPoseResult right)
        {
            return !left.Equals(right);
        }

        /// <inheritdoc/>
        public bool Equals(RecoverPoseResult other)
        {
            return InlierCount == other.InlierCount;
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is RecoverPoseResult other && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return InlierCount.GetHashCode();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "{InlierCount=" + InlierCount + "}";
        }
    }
}
