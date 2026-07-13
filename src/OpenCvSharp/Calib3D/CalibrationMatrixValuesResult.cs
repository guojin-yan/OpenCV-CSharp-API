using System;
using System.Globalization;
using System.Runtime.InteropServices;
using OpenCvSharp.Core;

namespace OpenCvSharp.Calib3D
{
    /// <summary>
    /// Camera characteristics computed from an intrinsic calibration matrix.
    /// 从相机内参矩阵计算出的相机特性。
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct CalibrationMatrixValuesResult : IEquatable<CalibrationMatrixValuesResult>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CalibrationMatrixValuesResult"/> struct.
        /// 初始化 <see cref="CalibrationMatrixValuesResult"/> 结构的新实例。
        /// </summary>
        /// <param name="fovX">The horizontal field of view in degrees. 水平视场角，单位为度。</param>
        /// <param name="fovY">The vertical field of view in degrees. 垂直视场角，单位为度。</param>
        /// <param name="focalLength">The focal length in the aperture unit. 按孔径单位表示的焦距。</param>
        /// <param name="principalPoint">The principal point in the aperture unit. 按孔径单位表示的主点。</param>
        /// <param name="aspectRatio">The focal aspect ratio. 焦距宽高比。</param>
        public CalibrationMatrixValuesResult(double fovX, double fovY, double focalLength, Point2d principalPoint, double aspectRatio)
        {
            FovX = fovX;
            FovY = fovY;
            FocalLength = focalLength;
            PrincipalPoint = principalPoint;
            AspectRatio = aspectRatio;
        }

        /// <summary>
        /// Gets the horizontal field of view in degrees.
        /// 获取水平视场角，单位为度。
        /// </summary>
        public double FovX { get; }

        /// <summary>
        /// Gets the vertical field of view in degrees.
        /// 获取垂直视场角，单位为度。
        /// </summary>
        public double FovY { get; }

        /// <summary>
        /// Gets the focal length in the aperture unit.
        /// 获取按孔径单位表示的焦距。
        /// </summary>
        public double FocalLength { get; }

        /// <summary>
        /// Gets the principal point in the aperture unit.
        /// 获取按孔径单位表示的主点。
        /// </summary>
        public Point2d PrincipalPoint { get; }

        /// <summary>
        /// Gets the focal aspect ratio.
        /// 获取焦距宽高比。
        /// </summary>
        public double AspectRatio { get; }

        /// <summary>Returns whether two calibration matrix value results are equal. 返回两个相机标定矩阵值结果是否相等。</summary>
        public static bool operator ==(CalibrationMatrixValuesResult left, CalibrationMatrixValuesResult right)
        {
            return left.Equals(right);
        }

        /// <summary>Returns whether two calibration matrix value results are not equal. 返回两个相机标定矩阵值结果是否不相等。</summary>
        public static bool operator !=(CalibrationMatrixValuesResult left, CalibrationMatrixValuesResult right)
        {
            return !left.Equals(right);
        }

        /// <inheritdoc/>
        public bool Equals(CalibrationMatrixValuesResult other)
        {
            return FovX.Equals(other.FovX) &&
                FovY.Equals(other.FovY) &&
                FocalLength.Equals(other.FocalLength) &&
                PrincipalPoint.Equals(other.PrincipalPoint) &&
                AspectRatio.Equals(other.AspectRatio);
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is CalibrationMatrixValuesResult other && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 17;
                hashCode = (hashCode * 31) + FovX.GetHashCode();
                hashCode = (hashCode * 31) + FovY.GetHashCode();
                hashCode = (hashCode * 31) + FocalLength.GetHashCode();
                hashCode = (hashCode * 31) + PrincipalPoint.GetHashCode();
                hashCode = (hashCode * 31) + AspectRatio.GetHashCode();
                return hashCode;
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "{FovX=" + FovX.ToString(CultureInfo.InvariantCulture) +
                ",FovY=" + FovY.ToString(CultureInfo.InvariantCulture) +
                ",FocalLength=" + FocalLength.ToString(CultureInfo.InvariantCulture) +
                ",PrincipalPoint=" + PrincipalPoint +
                ",AspectRatio=" + AspectRatio.ToString(CultureInfo.InvariantCulture) + "}";
        }
    }
}
