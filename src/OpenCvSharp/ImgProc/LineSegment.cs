using System;
using System.Globalization;
using System.Runtime.InteropServices;
using OpenCvSharp.Core;

namespace OpenCvSharp.ImgProc
{
    /// <summary>
    /// Represents a floating-point line segment encoded as start and end points.
    /// 表示由起点和终点编码的浮点线段。
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct LineSegment : IEquatable<LineSegment>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LineSegment"/> struct.
        /// 初始化 <see cref="LineSegment"/> 结构的新实例。
        /// </summary>
        /// <param name="x1">The start x coordinate. 起点 X 坐标。</param>
        /// <param name="y1">The start y coordinate. 起点 Y 坐标。</param>
        /// <param name="x2">The end x coordinate. 终点 X 坐标。</param>
        /// <param name="y2">The end y coordinate. 终点 Y 坐标。</param>
        public LineSegment(float x1, float y1, float x2, float y2)
        {
            P1 = new Point2f(x1, y1);
            P2 = new Point2f(x2, y2);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LineSegment"/> struct.
        /// 初始化 <see cref="LineSegment"/> 结构的新实例。
        /// </summary>
        /// <param name="p1">The start point. 起点。</param>
        /// <param name="p2">The end point. 终点。</param>
        public LineSegment(Point2f p1, Point2f p2)
        {
            P1 = p1;
            P2 = p2;
        }

        /// <summary>
        /// Gets the start point.
        /// 获取起点。
        /// </summary>
        public Point2f P1 { get; }

        /// <summary>
        /// Gets the end point.
        /// 获取终点。
        /// </summary>
        public Point2f P2 { get; }

        /// <summary>
        /// Gets the start x coordinate.
        /// 获取起点 X 坐标。
        /// </summary>
        public float X1
        {
            get { return P1.X; }
        }

        /// <summary>
        /// Gets the start y coordinate.
        /// 获取起点 Y 坐标。
        /// </summary>
        public float Y1
        {
            get { return P1.Y; }
        }

        /// <summary>
        /// Gets the end x coordinate.
        /// 获取终点 X 坐标。
        /// </summary>
        public float X2
        {
            get { return P2.X; }
        }

        /// <summary>
        /// Gets the end y coordinate.
        /// 获取终点 Y 坐标。
        /// </summary>
        public float Y2
        {
            get { return P2.Y; }
        }

        /// <summary>
        /// Gets the squared segment length.
        /// 获取线段长度平方。
        /// </summary>
        public float LengthSquared
        {
            get
            {
                float dx = X2 - X1;
                float dy = Y2 - Y1;
                return (dx * dx) + (dy * dy);
            }
        }

        /// <summary>
        /// Gets the value at the specified index.
        /// 获取指定索引处的值。
        /// </summary>
        /// <param name="index">The value index from 0 to 3. 值索引，范围为 0 到 3。</param>
        /// <returns>The indexed value. 索引处的值。</returns>
        public float this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return X1;
                    case 1:
                        return Y1;
                    case 2:
                        return X2;
                    case 3:
                        return Y2;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
        }

        /// <summary>
        /// Determines whether two segments are equal.
        /// 判断两个线段是否相等。
        /// </summary>
        public static bool operator ==(LineSegment left, LineSegment right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two segments are different.
        /// 判断两个线段是否不同。
        /// </summary>
        public static bool operator !=(LineSegment left, LineSegment right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Indicates whether this segment equals another segment.
        /// 指示此线段是否与另一个线段相等。
        /// </summary>
        public bool Equals(LineSegment other)
        {
            return P1.Equals(other.P1) && P2.Equals(other.P2);
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is LineSegment other && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                return (P1.GetHashCode() * 397) ^ P2.GetHashCode();
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{{P1={{X={0},Y={1}}},P2={{X={2},Y={3}}}}}",
                X1,
                Y1,
                X2,
                Y2);
        }
    }
}
