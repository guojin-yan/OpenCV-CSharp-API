using System;
using System.Globalization;
using System.Runtime.InteropServices;
using OpenCvSharp.Core;

namespace OpenCvSharp.Calib3D
{
    /// <summary>
    /// Result returned by three-camera collinear rectification.
    /// 三相机共线校正返回结果。
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Rectify3CollinearResult : IEquatable<Rectify3CollinearResult>
    {
        /// <summary>
        /// Initializes a three-camera rectification result.
        /// 初始化三相机校正结果。
        /// </summary>
        public Rectify3CollinearResult(float scale, Rect validPixROI1, Rect validPixROI2)
        {
            Scale = scale;
            ValidPixROI1 = validPixROI1;
            ValidPixROI2 = validPixROI2;
        }

        /// <summary>Gets the returned rectification scale. 获取返回的校正比例。</summary>
        public float Scale { get; }

        /// <summary>Gets the first valid-pixel ROI. 获取第一个有效像素 ROI。</summary>
        public Rect ValidPixROI1 { get; }

        /// <summary>Gets the second valid-pixel ROI. 获取第二个有效像素 ROI。</summary>
        public Rect ValidPixROI2 { get; }

        /// <summary>
        /// Determines whether two values are equal.
        /// 判断两个值是否相等。
        /// </summary>
        public static bool operator ==(Rectify3CollinearResult left, Rectify3CollinearResult right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two values are not equal.
        /// 判断两个值是否不相等。
        /// </summary>
        public static bool operator !=(Rectify3CollinearResult left, Rectify3CollinearResult right)
        {
            return !left.Equals(right);
        }

        /// <inheritdoc/>
        public bool Equals(Rectify3CollinearResult other)
        {
            return Scale.Equals(other.Scale) &&
                ValidPixROI1.Equals(other.ValidPixROI1) &&
                ValidPixROI2.Equals(other.ValidPixROI2);
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is Rectify3CollinearResult other && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hash = Scale.GetHashCode();
            hash = (hash * 397) ^ ValidPixROI1.GetHashCode();
            hash = (hash * 397) ^ ValidPixROI2.GetHashCode();
            return hash;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "{Scale=" + Scale.ToString(CultureInfo.InvariantCulture) +
                ",ValidPixROI1=" + ValidPixROI1 +
                ",ValidPixROI2=" + ValidPixROI2 + "}";
        }
    }
}
