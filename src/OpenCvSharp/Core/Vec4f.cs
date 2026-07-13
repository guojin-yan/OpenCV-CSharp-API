using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Core
{
    /// <summary>
    /// Represents a four-element single-precision vector compatible with OpenCV's <c>cv::Vec4f</c>.
    /// 表示与 OpenCV <c>cv::Vec4f</c> 对应的四元素单精度浮点向量。
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Vec4f : IEquatable<Vec4f>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Vec4f"/> struct.
        /// 初始化 <see cref="Vec4f"/> 结构的新实例。
        /// </summary>
        /// <param name="v0">The first value. 第一个值。</param>
        /// <param name="v1">The second value. 第二个值。</param>
        /// <param name="v2">The third value. 第三个值。</param>
        /// <param name="v3">The fourth value. 第四个值。</param>
        public Vec4f(float v0, float v1, float v2, float v3)
        {
            V0 = v0;
            V1 = v1;
            V2 = v2;
            V3 = v3;
        }

        /// <summary>
        /// Gets the first value.
        /// 获取第一个值。
        /// </summary>
        public float V0 { get; }

        /// <summary>
        /// Gets the second value.
        /// 获取第二个值。
        /// </summary>
        public float V1 { get; }

        /// <summary>
        /// Gets the third value.
        /// 获取第三个值。
        /// </summary>
        public float V2 { get; }

        /// <summary>
        /// Gets the fourth value.
        /// 获取第四个值。
        /// </summary>
        public float V3 { get; }

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
                        return V0;
                    case 1:
                        return V1;
                    case 2:
                        return V2;
                    case 3:
                        return V3;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
        }

        /// <summary>
        /// Determines whether two vectors are equal.
        /// 判断两个向量是否相等。
        /// </summary>
        /// <param name="left">The first vector. 第一个向量。</param>
        /// <param name="right">The second vector. 第二个向量。</param>
        /// <returns><c>true</c> if all values are equal; otherwise, <c>false</c>. 如果所有值都相等则返回 <c>true</c>，否则返回 <c>false</c>。</returns>
        public static bool operator ==(Vec4f left, Vec4f right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two vectors are different.
        /// 判断两个向量是否不同。
        /// </summary>
        /// <param name="left">The first vector. 第一个向量。</param>
        /// <param name="right">The second vector. 第二个向量。</param>
        /// <returns><c>true</c> if any value differs; otherwise, <c>false</c>. 如果任一值不同则返回 <c>true</c>，否则返回 <c>false</c>。</returns>
        public static bool operator !=(Vec4f left, Vec4f right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Indicates whether this vector equals another vector.
        /// 指示此向量是否与另一个向量相等。
        /// </summary>
        /// <param name="other">The other vector. 另一个向量。</param>
        /// <returns><c>true</c> if all values are equal; otherwise, <c>false</c>. 如果所有值都相等则返回 <c>true</c>，否则返回 <c>false</c>。</returns>
        public bool Equals(Vec4f other)
        {
            return V0.Equals(other.V0) &&
                V1.Equals(other.V1) &&
                V2.Equals(other.V2) &&
                V3.Equals(other.V3);
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is Vec4f other && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = V0.GetHashCode();
                hash = (hash * 397) ^ V1.GetHashCode();
                hash = (hash * 397) ^ V2.GetHashCode();
                hash = (hash * 397) ^ V3.GetHashCode();
                return hash;
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "{V0=" + V0.ToString(CultureInfo.InvariantCulture) +
                ",V1=" + V1.ToString(CultureInfo.InvariantCulture) +
                ",V2=" + V2.ToString(CultureInfo.InvariantCulture) +
                ",V3=" + V3.ToString(CultureInfo.InvariantCulture) + "}";
        }
    }
}
