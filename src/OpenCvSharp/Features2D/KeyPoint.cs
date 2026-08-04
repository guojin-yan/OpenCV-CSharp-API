using System;
using System.Globalization;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Features2D
{
    /// <summary>
    /// Represents an OpenCV feature keypoint compatible with <c>cv::KeyPoint</c>.
    /// 表示与 OpenCV <c>cv::KeyPoint</c> 兼容的特征关键点。
    /// </summary>
    public readonly struct KeyPoint : IEquatable<KeyPoint>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KeyPoint"/> struct.
        /// 初始化 <see cref="KeyPoint"/> 结构的新实例。
        /// </summary>
        public KeyPoint(Point2f pt, float size, float angle = -1.0F, float response = 0.0F, int octave = 0, int classId = -1)
        {
            Pt = pt;
            Size = size;
            Angle = angle;
            Response = response;
            Octave = octave;
            ClassId = classId;
        }

        /// <summary>
        /// Initializes a new instance from separate coordinates.
        /// 使用独立坐标初始化新实例。
        /// </summary>
        public KeyPoint(float x, float y, float size, float angle = -1.0F, float response = 0.0F, int octave = 0, int classId = -1)
            : this(new Point2f(x, y), size, angle, response, octave, classId)
        {
        }

        /// <summary>
        /// Gets the keypoint location.
        /// 获取关键点位置。
        /// </summary>
        public Point2f Pt { get; }

        /// <summary>
        /// Gets the x coordinate.
        /// 获取 X 坐标。
        /// </summary>
        public float X
        {
            get { return Pt.X; }
        }

        /// <summary>
        /// Gets the y coordinate.
        /// 获取 Y 坐标。
        /// </summary>
        public float Y
        {
            get { return Pt.Y; }
        }

        /// <summary>
        /// Gets the keypoint diameter.
        /// 获取关键点直径。
        /// </summary>
        public float Size { get; }

        /// <summary>
        /// Gets the keypoint orientation in degrees.
        /// 获取关键点方向，单位为度。
        /// </summary>
        public float Angle { get; }

        /// <summary>
        /// Gets the detector response.
        /// 获取检测器响应值。
        /// </summary>
        public float Response { get; }

        /// <summary>
        /// Gets the pyramid octave.
        /// 获取金字塔 octave。
        /// </summary>
        public int Octave { get; }

        /// <summary>
        /// Gets the class id.
        /// 获取类别 ID。
        /// </summary>
        public int ClassId { get; }

        /// <summary>
        /// Determines whether two keypoints are equal.
        /// 判断两个关键点是否相等。
        /// </summary>
        public static bool operator ==(KeyPoint left, KeyPoint right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two keypoints are different.
        /// 判断两个关键点是否不同。
        /// </summary>
        public static bool operator !=(KeyPoint left, KeyPoint right)
        {
            return !left.Equals(right);
        }

        internal static KeyPoint FromNative(NativeKeyPoint value)
        {
            return new KeyPoint(value.X, value.Y, value.Size, value.Angle, value.Response, value.Octave, value.ClassId);
        }

        internal NativeKeyPoint ToNative()
        {
            return new NativeKeyPoint
            {
                X = X,
                Y = Y,
                Size = Size,
                Angle = Angle,
                Response = Response,
                Octave = Octave,
                ClassId = ClassId
            };
        }

        /// <summary>
        /// Indicates whether this keypoint equals another keypoint.
        /// 指示此关键点是否与另一个关键点相等。
        /// </summary>
        public bool Equals(KeyPoint other)
        {
            return Pt.Equals(other.Pt)
                && Size.Equals(other.Size)
                && Angle.Equals(other.Angle)
                && Response.Equals(other.Response)
                && Octave == other.Octave
                && ClassId == other.ClassId;
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is KeyPoint other && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = Pt.GetHashCode();
                hash = (hash * 397) ^ Size.GetHashCode();
                hash = (hash * 397) ^ Angle.GetHashCode();
                hash = (hash * 397) ^ Response.GetHashCode();
                hash = (hash * 397) ^ Octave;
                hash = (hash * 397) ^ ClassId;
                return hash;
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "{Pt=" + Pt
                + ",Size=" + Size.ToString(CultureInfo.InvariantCulture)
                + ",Angle=" + Angle.ToString(CultureInfo.InvariantCulture)
                + ",Response=" + Response.ToString(CultureInfo.InvariantCulture)
                + ",Octave=" + Octave
                + ",ClassId=" + ClassId + "}";
        }
    }
}
