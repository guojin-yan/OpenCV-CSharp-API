using System;
using System.Runtime.InteropServices;
using JYPPX.OpenCvSharp.Core;

namespace JYPPX.OpenCvSharp.Tracking.Legacy
{
    /// <summary>
    /// Result of an OpenCV legacy tracker update.
    /// OpenCV legacy 跟踪器更新结果。
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct LegacyTrackerUpdateResult : IEquatable<LegacyTrackerUpdateResult>
    {
        /// <summary>Initializes a new update result. 初始化更新结果。</summary>
        public LegacyTrackerUpdateResult(bool success, Rect2d boundingBox)
        {
            Success = success;
            BoundingBox = boundingBox;
        }

        /// <summary>Gets whether the update succeeded. 获取更新是否成功。</summary>
        public bool Success { get; }

        /// <summary>Gets the updated bounding box. 获取更新后的边界框。</summary>
        public Rect2d BoundingBox { get; }

        /// <summary>Returns whether two update results are equal. 返回两个更新结果是否相等。</summary>
        public static bool operator ==(LegacyTrackerUpdateResult left, LegacyTrackerUpdateResult right)
        {
            return left.Equals(right);
        }

        /// <summary>Returns whether two update results are not equal. 返回两个更新结果是否不相等。</summary>
        public static bool operator !=(LegacyTrackerUpdateResult left, LegacyTrackerUpdateResult right)
        {
            return !left.Equals(right);
        }

        /// <inheritdoc />
        public bool Equals(LegacyTrackerUpdateResult other)
        {
            return Success == other.Success &&
                BoundingBox.Equals(other.BoundingBox);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is LegacyTrackerUpdateResult other && Equals(other);
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
