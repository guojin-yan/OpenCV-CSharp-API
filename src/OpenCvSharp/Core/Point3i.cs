using System;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Core
{
    /// <summary>
    /// Represents a three-dimensional integer point compatible with OpenCV's <c>cv::Point3i</c>.
    /// 表示与 OpenCV <c>cv::Point3i</c> 对应的三维整数点。
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Point3i : IEquatable<Point3i>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Point3i"/> struct.
        /// 初始化 <see cref="Point3i"/> 结构的新实例。
        /// </summary>
        /// <param name="x">The x coordinate. X 坐标。</param>
        /// <param name="y">The y coordinate. Y 坐标。</param>
        /// <param name="z">The z coordinate. Z 坐标。</param>
        public Point3i(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>Gets the x coordinate. 获取 X 坐标。</summary>
        public int X { get; }

        /// <summary>Gets the y coordinate. 获取 Y 坐标。</summary>
        public int Y { get; }

        /// <summary>Gets the z coordinate. 获取 Z 坐标。</summary>
        public int Z { get; }

        /// <summary>Determines whether two points are equal. 判断两个点是否相等。</summary>
        public static bool operator ==(Point3i left, Point3i right)
        {
            return left.Equals(right);
        }

        /// <summary>Determines whether two points are different. 判断两个点是否不同。</summary>
        public static bool operator !=(Point3i left, Point3i right)
        {
            return !left.Equals(right);
        }

        /// <inheritdoc/>
        public bool Equals(Point3i other)
        {
            return X == other.X && Y == other.Y && Z == other.Z;
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is Point3i other && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = X;
                hash = (hash * 397) ^ Y;
                hash = (hash * 397) ^ Z;
                return hash;
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "{X=" + X + ",Y=" + Y + ",Z=" + Z + "}";
        }
    }
}
