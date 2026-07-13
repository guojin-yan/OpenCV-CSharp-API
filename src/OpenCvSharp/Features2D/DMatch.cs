using System;
using System.Globalization;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Features2D
{
    /// <summary>
    /// Represents an OpenCV descriptor match compatible with <c>cv::DMatch</c>.
    /// 表示与 OpenCV <c>cv::DMatch</c> 兼容的描述子匹配结果。
    /// </summary>
    public readonly struct DMatch : IEquatable<DMatch>, IComparable<DMatch>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DMatch"/> struct.
        /// 初始化 <see cref="DMatch"/> 结构的新实例。
        /// </summary>
        public DMatch(int queryIdx, int trainIdx, int imgIdx, float distance)
        {
            QueryIdx = queryIdx;
            TrainIdx = trainIdx;
            ImgIdx = imgIdx;
            Distance = distance;
        }

        /// <summary>
        /// Initializes a new instance with image index zero.
        /// 使用图像索引 0 初始化新实例。
        /// </summary>
        public DMatch(int queryIdx, int trainIdx, float distance)
            : this(queryIdx, trainIdx, 0, distance)
        {
        }

        /// <summary>
        /// Gets the query descriptor index.
        /// 获取查询描述子索引。
        /// </summary>
        public int QueryIdx { get; }

        /// <summary>
        /// Gets the train descriptor index.
        /// 获取训练描述子索引。
        /// </summary>
        public int TrainIdx { get; }

        /// <summary>
        /// Gets the train image index.
        /// 获取训练图像索引。
        /// </summary>
        public int ImgIdx { get; }

        /// <summary>
        /// Gets the descriptor distance.
        /// 获取描述子距离。
        /// </summary>
        public float Distance { get; }

        /// <summary>
        /// Gets the value at the specified OpenCV field index.
        /// 获取指定 OpenCV 字段索引处的值。
        /// </summary>
        public float this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return QueryIdx;
                    case 1:
                        return TrainIdx;
                    case 2:
                        return ImgIdx;
                    case 3:
                        return Distance;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
        }

        /// <summary>
        /// Determines whether two matches are equal.
        /// 判断两个匹配结果是否相等。
        /// </summary>
        public static bool operator ==(DMatch left, DMatch right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two matches are different.
        /// 判断两个匹配结果是否不同。
        /// </summary>
        public static bool operator !=(DMatch left, DMatch right)
        {
            return !left.Equals(right);
        }

        internal static DMatch FromNative(NativeDMatch value)
        {
            return new DMatch(value.QueryIdx, value.TrainIdx, value.ImgIdx, value.Distance);
        }

        internal NativeDMatch ToNative()
        {
            return new NativeDMatch
            {
                QueryIdx = QueryIdx,
                TrainIdx = TrainIdx,
                ImgIdx = ImgIdx,
                Distance = Distance
            };
        }

        /// <summary>
        /// Compares matches by distance.
        /// 按距离比较匹配结果。
        /// </summary>
        public int CompareTo(DMatch other)
        {
            return Distance.CompareTo(other.Distance);
        }

        /// <summary>
        /// Indicates whether this match equals another match.
        /// 指示此匹配结果是否与另一个匹配结果相等。
        /// </summary>
        public bool Equals(DMatch other)
        {
            return QueryIdx == other.QueryIdx
                && TrainIdx == other.TrainIdx
                && ImgIdx == other.ImgIdx
                && Distance.Equals(other.Distance);
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is DMatch other && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = QueryIdx;
                hash = (hash * 397) ^ TrainIdx;
                hash = (hash * 397) ^ ImgIdx;
                hash = (hash * 397) ^ Distance.GetHashCode();
                return hash;
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "{QueryIdx=" + QueryIdx + ",TrainIdx=" + TrainIdx + ",ImgIdx=" + ImgIdx + ",Distance=" + Distance.ToString(CultureInfo.InvariantCulture) + "}";
        }
    }
}
