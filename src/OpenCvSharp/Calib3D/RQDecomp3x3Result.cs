using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Calib3D
{
    /// <summary>
    /// Euler angle result returned by <see cref="Cv2.RQDecomp3x3"/>.
    /// <see cref="Cv2.RQDecomp3x3"/> 返回的欧拉角结果。
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct RQDecomp3x3Result : IEquatable<RQDecomp3x3Result>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RQDecomp3x3Result"/> struct.
        /// 初始化 <see cref="RQDecomp3x3Result"/> 结构的新实例。
        /// </summary>
        /// <param name="x">The X-axis Euler angle in degrees. X 轴欧拉角，单位为度。</param>
        /// <param name="y">The Y-axis Euler angle in degrees. Y 轴欧拉角，单位为度。</param>
        /// <param name="z">The Z-axis Euler angle in degrees. Z 轴欧拉角，单位为度。</param>
        public RQDecomp3x3Result(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>
        /// Gets the X-axis Euler angle in degrees.
        /// 获取 X 轴欧拉角，单位为度。
        /// </summary>
        public double X { get; }

        /// <summary>
        /// Gets the Y-axis Euler angle in degrees.
        /// 获取 Y 轴欧拉角，单位为度。
        /// </summary>
        public double Y { get; }

        /// <summary>
        /// Gets the Z-axis Euler angle in degrees.
        /// 获取 Z 轴欧拉角，单位为度。
        /// </summary>
        public double Z { get; }

        /// <summary>Returns whether two RQ decomposition results are equal. 返回两个 RQ 分解结果是否相等。</summary>
        public static bool operator ==(RQDecomp3x3Result left, RQDecomp3x3Result right)
        {
            return left.Equals(right);
        }

        /// <summary>Returns whether two RQ decomposition results are not equal. 返回两个 RQ 分解结果是否不相等。</summary>
        public static bool operator !=(RQDecomp3x3Result left, RQDecomp3x3Result right)
        {
            return !left.Equals(right);
        }

        /// <inheritdoc/>
        public bool Equals(RQDecomp3x3Result other)
        {
            return X.Equals(other.X) &&
                Y.Equals(other.Y) &&
                Z.Equals(other.Z);
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is RQDecomp3x3Result other && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 17;
                hashCode = (hashCode * 31) + X.GetHashCode();
                hashCode = (hashCode * 31) + Y.GetHashCode();
                hashCode = (hashCode * 31) + Z.GetHashCode();
                return hashCode;
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "{X=" + X.ToString(CultureInfo.InvariantCulture) +
                ",Y=" + Y.ToString(CultureInfo.InvariantCulture) +
                ",Z=" + Z.ToString(CultureInfo.InvariantCulture) + "}";
        }
    }
}
