using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace OpenCvSharp.ImgProc
{
    /// <summary>
    /// Represents spatial, central, and normalized central moments compatible with OpenCV <c>cv::Moments</c>.
    /// 表示与 OpenCV <c>cv::Moments</c> 兼容的空间矩、中心矩和归一化中心矩。
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Moments : IEquatable<Moments>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Moments"/> struct.
        /// 初始化 <see cref="Moments"/> 结构的新实例。
        /// </summary>
        public Moments(
            double m00,
            double m10,
            double m01,
            double m20,
            double m11,
            double m02,
            double m30,
            double m21,
            double m12,
            double m03,
            double mu20,
            double mu11,
            double mu02,
            double mu30,
            double mu21,
            double mu12,
            double mu03,
            double nu20,
            double nu11,
            double nu02,
            double nu30,
            double nu21,
            double nu12,
            double nu03)
        {
            M00 = m00;
            M10 = m10;
            M01 = m01;
            M20 = m20;
            M11 = m11;
            M02 = m02;
            M30 = m30;
            M21 = m21;
            M12 = m12;
            M03 = m03;
            Mu20 = mu20;
            Mu11 = mu11;
            Mu02 = mu02;
            Mu30 = mu30;
            Mu21 = mu21;
            Mu12 = mu12;
            Mu03 = mu03;
            Nu20 = nu20;
            Nu11 = nu11;
            Nu02 = nu02;
            Nu30 = nu30;
            Nu21 = nu21;
            Nu12 = nu12;
            Nu03 = nu03;
        }

        /// <summary>
        /// Gets the zero-order spatial moment.
        /// 获取零阶空间矩。
        /// </summary>
        public double M00 { get; }

        /// <summary>
        /// Gets the first-order spatial moment along x.
        /// 获取 x 方向一阶空间矩。
        /// </summary>
        public double M10 { get; }

        /// <summary>
        /// Gets the first-order spatial moment along y.
        /// 获取 y 方向一阶空间矩。
        /// </summary>
        public double M01 { get; }

        /// <summary>
        /// Gets the second-order spatial moment along x.
        /// 获取 x 方向二阶空间矩。
        /// </summary>
        public double M20 { get; }

        /// <summary>
        /// Gets the mixed second-order spatial moment.
        /// 获取二阶混合空间矩。
        /// </summary>
        public double M11 { get; }

        /// <summary>
        /// Gets the second-order spatial moment along y.
        /// 获取 y 方向二阶空间矩。
        /// </summary>
        public double M02 { get; }

        /// <summary>
        /// Gets the third-order spatial moment along x.
        /// 获取 x 方向三阶空间矩。
        /// </summary>
        public double M30 { get; }

        /// <summary>
        /// Gets the x2-y mixed third-order spatial moment.
        /// 获取 x2-y 混合三阶空间矩。
        /// </summary>
        public double M21 { get; }

        /// <summary>
        /// Gets the x-y2 mixed third-order spatial moment.
        /// 获取 x-y2 混合三阶空间矩。
        /// </summary>
        public double M12 { get; }

        /// <summary>
        /// Gets the third-order spatial moment along y.
        /// 获取 y 方向三阶空间矩。
        /// </summary>
        public double M03 { get; }

        /// <summary>
        /// Gets the second-order central moment along x.
        /// 获取 x 方向二阶中心矩。
        /// </summary>
        public double Mu20 { get; }

        /// <summary>
        /// Gets the mixed second-order central moment.
        /// 获取二阶混合中心矩。
        /// </summary>
        public double Mu11 { get; }

        /// <summary>
        /// Gets the second-order central moment along y.
        /// 获取 y 方向二阶中心矩。
        /// </summary>
        public double Mu02 { get; }

        /// <summary>
        /// Gets the third-order central moment along x.
        /// 获取 x 方向三阶中心矩。
        /// </summary>
        public double Mu30 { get; }

        /// <summary>
        /// Gets the x2-y mixed third-order central moment.
        /// 获取 x2-y 混合三阶中心矩。
        /// </summary>
        public double Mu21 { get; }

        /// <summary>
        /// Gets the x-y2 mixed third-order central moment.
        /// 获取 x-y2 混合三阶中心矩。
        /// </summary>
        public double Mu12 { get; }

        /// <summary>
        /// Gets the third-order central moment along y.
        /// 获取 y 方向三阶中心矩。
        /// </summary>
        public double Mu03 { get; }

        /// <summary>
        /// Gets the normalized second-order central moment along x.
        /// 获取 x 方向归一化二阶中心矩。
        /// </summary>
        public double Nu20 { get; }

        /// <summary>
        /// Gets the normalized mixed second-order central moment.
        /// 获取归一化二阶混合中心矩。
        /// </summary>
        public double Nu11 { get; }

        /// <summary>
        /// Gets the normalized second-order central moment along y.
        /// 获取 y 方向归一化二阶中心矩。
        /// </summary>
        public double Nu02 { get; }

        /// <summary>
        /// Gets the normalized third-order central moment along x.
        /// 获取 x 方向归一化三阶中心矩。
        /// </summary>
        public double Nu30 { get; }

        /// <summary>
        /// Gets the normalized x2-y mixed third-order central moment.
        /// 获取归一化 x2-y 混合三阶中心矩。
        /// </summary>
        public double Nu21 { get; }

        /// <summary>
        /// Gets the normalized x-y2 mixed third-order central moment.
        /// 获取归一化 x-y2 混合三阶中心矩。
        /// </summary>
        public double Nu12 { get; }

        /// <summary>
        /// Gets the normalized third-order central moment along y.
        /// 获取 y 方向归一化三阶中心矩。
        /// </summary>
        public double Nu03 { get; }

        /// <summary>
        /// Gets the moment value at the OpenCV field order index from 0 to 23.
        /// 按 OpenCV 字段顺序获取索引 0 到 23 的矩值。
        /// </summary>
        public double this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return M00;
                    case 1:
                        return M10;
                    case 2:
                        return M01;
                    case 3:
                        return M20;
                    case 4:
                        return M11;
                    case 5:
                        return M02;
                    case 6:
                        return M30;
                    case 7:
                        return M21;
                    case 8:
                        return M12;
                    case 9:
                        return M03;
                    case 10:
                        return Mu20;
                    case 11:
                        return Mu11;
                    case 12:
                        return Mu02;
                    case 13:
                        return Mu30;
                    case 14:
                        return Mu21;
                    case 15:
                        return Mu12;
                    case 16:
                        return Mu03;
                    case 17:
                        return Nu20;
                    case 18:
                        return Nu11;
                    case 19:
                        return Nu02;
                    case 20:
                        return Nu30;
                    case 21:
                        return Nu21;
                    case 22:
                        return Nu12;
                    case 23:
                        return Nu03;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
        }

        /// <summary>
        /// Returns all moment values in OpenCV field order.
        /// 按 OpenCV 字段顺序返回全部矩值。
        /// </summary>
        /// <returns>A 24-element array containing the moment values. 包含矩值的 24 元素数组。</returns>
        public double[] ToArray()
        {
            return new double[]
            {
                M00, M10, M01, M20, M11, M02, M30, M21, M12, M03,
                Mu20, Mu11, Mu02, Mu30, Mu21, Mu12, Mu03,
                Nu20, Nu11, Nu02, Nu30, Nu21, Nu12, Nu03
            };
        }

        /// <summary>
        /// Determines whether two moment values are equal.
        /// 判断两个矩值是否相等。
        /// </summary>
        public static bool operator ==(Moments left, Moments right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two moment values are different.
        /// 判断两个矩值是否不同。
        /// </summary>
        public static bool operator !=(Moments left, Moments right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Indicates whether this instance equals another moments value.
        /// 指示此实例是否与另一个矩值相等。
        /// </summary>
        public bool Equals(Moments other)
        {
            for (int i = 0; i < 24; i++)
            {
                if (this[i] != other[i])
                {
                    return false;
                }
            }

            return true;
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is Moments other && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = M00.GetHashCode();
                for (int i = 1; i < 24; i++)
                {
                    hash = (hash * 397) ^ this[i].GetHashCode();
                }

                return hash;
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "{M00=" + M00.ToString(CultureInfo.InvariantCulture) +
                ",M10=" + M10.ToString(CultureInfo.InvariantCulture) +
                ",M01=" + M01.ToString(CultureInfo.InvariantCulture) + "}";
        }
    }
}
