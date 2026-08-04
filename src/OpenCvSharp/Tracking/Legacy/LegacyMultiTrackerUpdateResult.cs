using System;
using System.Linq;
using JYPPX.OpenCvSharp.Core;

namespace JYPPX.OpenCvSharp.Tracking.Legacy
{
    /// <summary>
    /// Result of an OpenCV legacy MultiTracker update.
    /// OpenCV legacy MultiTracker 更新结果。
    /// </summary>
    public readonly struct LegacyMultiTrackerUpdateResult : IEquatable<LegacyMultiTrackerUpdateResult>
    {
        /// <summary>Initializes a new update result. 初始化更新结果。</summary>
        public LegacyMultiTrackerUpdateResult(bool success, Rect2d[] boundingBoxes)
        {
            Success = success;
            this.boundingBoxes = Clone(boundingBoxes);
        }

        private readonly Rect2d[] boundingBoxes;

        /// <summary>Gets whether all trackers updated successfully. 获取是否所有跟踪器更新成功。</summary>
        public bool Success { get; }

        /// <summary>Gets updated bounding boxes. 获取更新后的边界框数组。</summary>
        public Rect2d[] BoundingBoxes
        {
            get { return Clone(boundingBoxes); }
        }

        /// <summary>Gets the number of updated bounding boxes. 获取更新后的边界框数量。</summary>
        public int BoundingBoxCount
        {
            get { return GetBoundingBoxes(this).Length; }
        }

        /// <summary>Compares two update results for value equality. 比较两个更新结果是否值相等。</summary>
        public static bool operator ==(LegacyMultiTrackerUpdateResult left, LegacyMultiTrackerUpdateResult right)
        {
            return left.Equals(right);
        }

        /// <summary>Compares two update results for value inequality. 比较两个更新结果是否值不相等。</summary>
        public static bool operator !=(LegacyMultiTrackerUpdateResult left, LegacyMultiTrackerUpdateResult right)
        {
            return !left.Equals(right);
        }

        /// <inheritdoc/>
        public bool Equals(LegacyMultiTrackerUpdateResult other)
        {
            return Success == other.Success && GetBoundingBoxes(this).SequenceEqual(GetBoundingBoxes(other));
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is LegacyMultiTrackerUpdateResult other && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = Success.GetHashCode();
                foreach (Rect2d boundingBox in GetBoundingBoxes(this))
                {
                    hash = (hash * 397) ^ boundingBox.GetHashCode();
                }

                return hash;
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "{Success=" + Success + ",BoundingBoxes=" + BoundingBoxCount + "}";
        }

        private static Rect2d[] GetBoundingBoxes(LegacyMultiTrackerUpdateResult result)
        {
            return result.boundingBoxes ?? Array.Empty<Rect2d>();
        }

        private static Rect2d[] Clone(Rect2d[] values)
        {
            if (values == null || values.Length == 0)
            {
                return Array.Empty<Rect2d>();
            }

            var clone = new Rect2d[values.Length];
            Array.Copy(values, clone, clone.Length);
            return clone;
        }
    }
}
