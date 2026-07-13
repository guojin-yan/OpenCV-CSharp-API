using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Reg
{
    /// <summary>
    /// Represents a 2D affine transform used by OpenCV reg maps.
    /// 表示 OpenCV reg map 使用的二维仿射变换。
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct AffineTransform2D : IEquatable<AffineTransform2D>
    {
        /// <summary>Initializes the transform. 初始化变换。</summary>
        public AffineTransform2D(double m00, double m01, double m10, double m11, double shiftX, double shiftY)
        {
            M00 = m00;
            M01 = m01;
            M10 = m10;
            M11 = m11;
            ShiftX = shiftX;
            ShiftY = shiftY;
        }

        /// <summary>Gets the identity affine transform. 获取单位仿射变换。</summary>
        public static AffineTransform2D Identity
        {
            get { return new AffineTransform2D(1.0, 0.0, 0.0, 1.0, 0.0, 0.0); }
        }

        /// <summary>Gets the first row, first column coefficient. 获取第一行第一列系数。</summary>
        public double M00 { get; }

        /// <summary>Gets the first row, second column coefficient. 获取第一行第二列系数。</summary>
        public double M01 { get; }

        /// <summary>Gets the second row, first column coefficient. 获取第二行第一列系数。</summary>
        public double M10 { get; }

        /// <summary>Gets the second row, second column coefficient. 获取第二行第二列系数。</summary>
        public double M11 { get; }

        /// <summary>Gets the x translation. 获取 X 平移量。</summary>
        public double ShiftX { get; }

        /// <summary>Gets the y translation. 获取 Y 平移量。</summary>
        public double ShiftY { get; }

        /// <summary>Returns the transform as a six-value array. 以六元素数组返回变换。</summary>
        public double[] ToArray()
        {
            return new[] { M00, M01, M10, M11, ShiftX, ShiftY };
        }

        /// <summary>Creates an affine transform from a six-value array. 从六元素数组创建仿射变换。</summary>
        public static AffineTransform2D FromArray(double[] values)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            if (values.Length != 6)
            {
                throw new ArgumentException("Array must contain exactly six values.", nameof(values));
            }

            return new AffineTransform2D(values[0], values[1], values[2], values[3], values[4], values[5]);
        }

        /// <summary>Returns whether two transforms are equal. 返回两个变换是否相等。</summary>
        public static bool operator ==(AffineTransform2D left, AffineTransform2D right)
        {
            return left.Equals(right);
        }

        /// <summary>Returns whether two transforms are not equal. 返回两个变换是否不相等。</summary>
        public static bool operator !=(AffineTransform2D left, AffineTransform2D right)
        {
            return !left.Equals(right);
        }

        /// <summary>Determines whether two transforms are equal. 判断两个变换是否相等。</summary>
        public bool Equals(AffineTransform2D other)
        {
            return M00.Equals(other.M00) &&
                M01.Equals(other.M01) &&
                M10.Equals(other.M10) &&
                M11.Equals(other.M11) &&
                ShiftX.Equals(other.ShiftX) &&
                ShiftY.Equals(other.ShiftY);
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is AffineTransform2D other && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = M00.GetHashCode();
                hash = (hash * 397) ^ M01.GetHashCode();
                hash = (hash * 397) ^ M10.GetHashCode();
                hash = (hash * 397) ^ M11.GetHashCode();
                hash = (hash * 397) ^ ShiftX.GetHashCode();
                hash = (hash * 397) ^ ShiftY.GetHashCode();
                return hash;
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{{M00={0},M01={1},M10={2},M11={3},ShiftX={4},ShiftY={5}}}",
                M00,
                M01,
                M10,
                M11,
                ShiftX,
                ShiftY);
        }
    }
}
