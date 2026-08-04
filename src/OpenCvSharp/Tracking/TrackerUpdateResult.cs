using System;
using System.Runtime.InteropServices;
using JYPPX.OpenCvSharp.Core;

namespace JYPPX.OpenCvSharp.Tracking
{
    /// <summary>
    /// Result of a modern tracker update.
    /// 现代跟踪器更新结果。
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct TrackerUpdateResult : IEquatable<TrackerUpdateResult>
    {
        /// <summary>Initializes a new update result. 初始化更新结果。</summary>
        public TrackerUpdateResult(bool success, Rect boundingBox)
        {
            Success = success;
            BoundingBox = boundingBox;
        }

        /// <summary>Gets whether the tracker updated successfully. 获取跟踪器是否更新成功。</summary>
        public bool Success { get; }

        /// <summary>Gets the updated bounding box. 获取更新后的边界框。</summary>
        public Rect BoundingBox { get; }

        /// <summary>Returns whether two update results are equal. 返回两个更新结果是否相等。</summary>
        public static bool operator ==(TrackerUpdateResult left, TrackerUpdateResult right)
        {
            return left.Equals(right);
        }

        /// <summary>Returns whether two update results are not equal. 返回两个更新结果是否不相等。</summary>
        public static bool operator !=(TrackerUpdateResult left, TrackerUpdateResult right)
        {
            return !left.Equals(right);
        }

        /// <inheritdoc />
        public bool Equals(TrackerUpdateResult other)
        {
            return Success == other.Success &&
                BoundingBox.Equals(other.BoundingBox);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is TrackerUpdateResult other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 17;
                hashCode = (hashCode * 31) + Success.GetHashCode();
                hashCode = (hashCode * 31) + BoundingBox.GetHashCode();
                return hashCode;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return "{Success=" + Success + ",BoundingBox=" + BoundingBox + "}";
        }
    }
}
