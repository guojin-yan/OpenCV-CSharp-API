using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Reg
{
    /// <summary>
    /// Represents a 3x3 projective transform used by OpenCV reg maps.
    /// 表示 OpenCV reg map 使用的 3x3 投影变换。
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct ProjectiveTransform2D : IEquatable<ProjectiveTransform2D>
    {
        /// <summary>Initializes the transform. 初始化变换。</summary>
        public ProjectiveTransform2D(
            double m00,
            double m01,
            double m02,
            double m10,
            double m11,
            double m12,
            double m20,
            double m21,
            double m22)
        {
            M00 = m00;
            M01 = m01;
            M02 = m02;
            M10 = m10;
            M11 = m11;
            M12 = m12;
            M20 = m20;
            M21 = m21;
            M22 = m22;
        }

        /// <summary>Gets the identity projective transform. 获取单位投影变换。</summary>
        public static ProjectiveTransform2D Identity
        {
            get { return new ProjectiveTransform2D(1.0, 0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 1.0); }
        }

        /// <summary>Gets the matrix coefficient. 获取矩阵系数。</summary>
        public double M00 { get; }

        /// <summary>Gets the matrix coefficient. 获取矩阵系数。</summary>
        public double M01 { get; }

        /// <summary>Gets the matrix coefficient. 获取矩阵系数。</summary>
        public double M02 { get; }

        /// <summary>Gets the matrix coefficient. 获取矩阵系数。</summary>
        public double M10 { get; }

        /// <summary>Gets the matrix coefficient. 获取矩阵系数。</summary>
        public double M11 { get; }

        /// <summary>Gets the matrix coefficient. 获取矩阵系数。</summary>
        public double M12 { get; }

        /// <summary>Gets the matrix coefficient. 获取矩阵系数。</summary>
        public double M20 { get; }

        /// <summary>Gets the matrix coefficient. 获取矩阵系数。</summary>
        public double M21 { get; }

        /// <summary>Gets the matrix coefficient. 获取矩阵系数。</summary>
        public double M22 { get; }

        /// <summary>Returns the transform as a row-major nine-value array. 以行优先九元素数组返回变换。</summary>
        public double[] ToArray()
        {
            return new[] { M00, M01, M02, M10, M11, M12, M20, M21, M22 };
        }

        /// <summary>Creates a projective transform from a row-major nine-value array. 从行优先九元素数组创建投影变换。</summary>
        public static ProjectiveTransform2D FromArray(double[] values)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            if (values.Length != 9)
            {
                throw new ArgumentException("Array must contain exactly nine values.", nameof(values));
            }

            return new ProjectiveTransform2D(
                values[0],
                values[1],
                values[2],
                values[3],
                values[4],
                values[5],
                values[6],
                values[7],
                values[8]);
        }

        /// <summary>Returns whether two transforms are equal. 返回两个变换是否相等。</summary>
        public static bool operator ==(ProjectiveTransform2D left, ProjectiveTransform2D right)
        {
            return left.Equals(right);
        }

        /// <summary>Returns whether two transforms are not equal. 返回两个变换是否不相等。</summary>
        public static bool operator !=(ProjectiveTransform2D left, ProjectiveTransform2D right)
        {
            return !left.Equals(right);
        }

        /// <summary>Determines whether two transforms are equal. 判断两个变换是否相等。</summary>
        public bool Equals(ProjectiveTransform2D other)
        {
            return M00.Equals(other.M00) &&
                M01.Equals(other.M01) &&
                M02.Equals(other.M02) &&
                M10.Equals(other.M10) &&
                M11.Equals(other.M11) &&
                M12.Equals(other.M12) &&
                M20.Equals(other.M20) &&
                M21.Equals(other.M21) &&
                M22.Equals(other.M22);
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is ProjectiveTransform2D other && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = M00.GetHashCode();
                hash = (hash * 397) ^ M01.GetHashCode();
                hash = (hash * 397) ^ M02.GetHashCode();
                hash = (hash * 397) ^ M10.GetHashCode();
                hash = (hash * 397) ^ M11.GetHashCode();
                hash = (hash * 397) ^ M12.GetHashCode();
                hash = (hash * 397) ^ M20.GetHashCode();
                hash = (hash * 397) ^ M21.GetHashCode();
                hash = (hash * 397) ^ M22.GetHashCode();
                return hash;
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{{M00={0},M01={1},M02={2},M10={3},M11={4},M12={5},M20={6},M21={7},M22={8}}}",
                M00,
                M01,
                M02,
                M10,
                M11,
                M12,
                M20,
                M21,
                M22);
        }
    }
}
