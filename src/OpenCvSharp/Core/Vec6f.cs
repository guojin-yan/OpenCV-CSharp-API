using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Core
{
    /// <summary>
    /// Represents a six-element single-precision vector compatible with OpenCV's <c>cv::Vec6f</c>.
    /// 表示与 OpenCV <c>cv::Vec6f</c> 对应的六元素单精度浮点向量。
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Vec6f : IEquatable<Vec6f>
    {
        /// <summary>Initializes the vector. 初始化向量。</summary>
        public Vec6f(float v0, float v1, float v2, float v3, float v4, float v5)
        {
            V0 = v0;
            V1 = v1;
            V2 = v2;
            V3 = v3;
            V4 = v4;
            V5 = v5;
        }

        /// <summary>Gets the first value. 获取第一个值。</summary>
        public float V0 { get; }
        /// <summary>Gets the second value. 获取第二个值。</summary>
        public float V1 { get; }
        /// <summary>Gets the third value. 获取第三个值。</summary>
        public float V2 { get; }
        /// <summary>Gets the fourth value. 获取第四个值。</summary>
        public float V3 { get; }
        /// <summary>Gets the fifth value. 获取第五个值。</summary>
        public float V4 { get; }
        /// <summary>Gets the sixth value. 获取第六个值。</summary>
        public float V5 { get; }

        /// <summary>Gets a value by index. 按索引获取值。</summary>
        public float this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return V0;
                    case 1: return V1;
                    case 2: return V2;
                    case 3: return V3;
                    case 4: return V4;
                    case 5: return V5;
                    default: throw new IndexOutOfRangeException();
                }
            }
        }

        /// <inheritdoc/>
        public bool Equals(Vec6f other)
        {
            return V0.Equals(other.V0) && V1.Equals(other.V1) && V2.Equals(other.V2) &&
                V3.Equals(other.V3) && V4.Equals(other.V4) && V5.Equals(other.V5);
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj) { return obj is Vec6f other && Equals(other); }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = V0.GetHashCode();
                hash = (hash * 397) ^ V1.GetHashCode();
                hash = (hash * 397) ^ V2.GetHashCode();
                hash = (hash * 397) ^ V3.GetHashCode();
                hash = (hash * 397) ^ V4.GetHashCode();
                return (hash * 397) ^ V5.GetHashCode();
            }
        }

        /// <summary>Determines whether two vectors are equal. 判断两个向量是否相等。</summary>
        public static bool operator ==(Vec6f left, Vec6f right) { return left.Equals(right); }

        /// <summary>Determines whether two vectors differ. 判断两个向量是否不同。</summary>
        public static bool operator !=(Vec6f left, Vec6f right) { return !left.Equals(right); }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "{V0=" + V0.ToString(CultureInfo.InvariantCulture)
                + ",V1=" + V1.ToString(CultureInfo.InvariantCulture)
                + ",V2=" + V2.ToString(CultureInfo.InvariantCulture)
                + ",V3=" + V3.ToString(CultureInfo.InvariantCulture)
                + ",V4=" + V4.ToString(CultureInfo.InvariantCulture)
                + ",V5=" + V5.ToString(CultureInfo.InvariantCulture) + "}";
        }
    }
}
