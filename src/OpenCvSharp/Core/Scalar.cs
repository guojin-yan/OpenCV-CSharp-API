using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Core
{
    /// <summary>
    /// Represents a four-element scalar compatible with OpenCV's <c>cv::Scalar</c>.
    /// 表示与 OpenCV <c>cv::Scalar</c> 对应的四元标量。
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Scalar : IEquatable<Scalar>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Scalar"/> struct.
        /// 初始化 <see cref="Scalar"/> 结构的新实例。
        /// </summary>
        /// <param name="v0">The first component. 第一个分量。</param>
        /// <param name="v1">The second component. 第二个分量。</param>
        /// <param name="v2">The third component. 第三个分量。</param>
        /// <param name="v3">The fourth component. 第四个分量。</param>
        public Scalar(double v0, double v1, double v2, double v3)
        {
            V0 = v0;
            V1 = v1;
            V2 = v2;
            V3 = v3;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Scalar"/> struct with one repeated value.
        /// 使用一个重复值初始化 <see cref="Scalar"/> 结构的新实例。
        /// </summary>
        /// <param name="value">The repeated value. 重复值。</param>
        public Scalar(double value)
            : this(value, value, value, value)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Scalar"/> struct with three components.
        /// 使用三个分量初始化 <see cref="Scalar"/> 结构的新实例。
        /// </summary>
        /// <param name="v0">The first component. 第一个分量。</param>
        /// <param name="v1">The second component. 第二个分量。</param>
        /// <param name="v2">The third component. 第三个分量。</param>
        public Scalar(double v0, double v1, double v2)
            : this(v0, v1, v2, 0)
        {
        }

        /// <summary>
        /// Gets the first component.
        /// 获取第一个分量。
        /// </summary>
        public double V0 { get; }

        /// <summary>
        /// Gets the second component.
        /// 获取第二个分量。
        /// </summary>
        public double V1 { get; }

        /// <summary>
        /// Gets the third component.
        /// 获取第三个分量。
        /// </summary>
        public double V2 { get; }

        /// <summary>
        /// Gets the fourth component.
        /// 获取第四个分量。
        /// </summary>
        public double V3 { get; }

        /// <summary>
        /// Gets the component at the specified index.
        /// 获取指定索引处的分量。
        /// </summary>
        /// <param name="index">The component index from 0 to 3. 分量索引，范围为 0 到 3。</param>
        /// <returns>The component value. 分量值。</returns>
        public double this[int index]
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
        /// Determines whether two scalars are equal.
        /// 判断两个标量是否相等。
        /// </summary>
        /// <param name="left">The first scalar. 第一个标量。</param>
        /// <param name="right">The second scalar. 第二个标量。</param>
        /// <returns><c>true</c> if all components are equal; otherwise, <c>false</c>. 如果所有分量相等则返回 <c>true</c>，否则返回 <c>false</c>。</returns>
        public static bool operator ==(Scalar left, Scalar right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two scalars are different.
        /// 判断两个标量是否不同。
        /// </summary>
        /// <param name="left">The first scalar. 第一个标量。</param>
        /// <param name="right">The second scalar. 第二个标量。</param>
        /// <returns><c>true</c> if any component differs; otherwise, <c>false</c>. 如果任一分量不同则返回 <c>true</c>，否则返回 <c>false</c>。</returns>
        public static bool operator !=(Scalar left, Scalar right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Indicates whether this scalar equals another scalar.
        /// 指示此标量是否与另一个标量相等。
        /// </summary>
        /// <param name="other">The other scalar. 另一个标量。</param>
        /// <returns><c>true</c> if all components are equal; otherwise, <c>false</c>. 如果所有分量相等则返回 <c>true</c>，否则返回 <c>false</c>。</returns>
        public bool Equals(Scalar other)
        {
            return V0.Equals(other.V0) &&
                V1.Equals(other.V1) &&
                V2.Equals(other.V2) &&
                V3.Equals(other.V3);
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is Scalar other && Equals(other);
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
