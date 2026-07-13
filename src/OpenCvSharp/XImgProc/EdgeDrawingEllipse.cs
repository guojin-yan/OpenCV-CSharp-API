using System;
using System.Globalization;
using System.Runtime.InteropServices;
using OpenCvSharp.Core;

namespace OpenCvSharp.XImgProc
{
    /// <summary>
    /// Represents an ellipse candidate returned by EdgeDrawing.
    /// 表示 EdgeDrawing 返回的椭圆候选。
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct EdgeDrawingEllipse : IEquatable<EdgeDrawingEllipse>
    {
        /// <summary>Initializes an ellipse candidate. 初始化椭圆候选。</summary>
        public EdgeDrawingEllipse(double centerX, double centerY, double axisA, double axisB, double angle, double score)
        {
            Center = new Point2f((float)centerX, (float)centerY);
            AxisA = axisA;
            AxisB = axisB;
            Angle = angle;
            Score = score;
        }

        /// <summary>Gets the center point. 获取中心点。</summary>
        public Point2f Center { get; }

        /// <summary>Gets the first axis or radius-like value. 获取第一轴或半径类值。</summary>
        public double AxisA { get; }

        /// <summary>Gets the second axis or perimeter-like value. 获取第二轴或周长类值。</summary>
        public double AxisB { get; }

        /// <summary>Gets the ellipse angle. 获取椭圆角度。</summary>
        public double Angle { get; }

        /// <summary>Gets the validation or auxiliary score. 获取校验或辅助分数。</summary>
        public double Score { get; }

        /// <summary>Gets whether this row represents a circle output. 获取此输出行是否表示圆。</summary>
        public bool IsCircle
        {
            get { return AxisA != 0.0; }
        }

        /// <summary>Gets whether this row represents an ellipse output. 获取此输出行是否表示椭圆。</summary>
        public bool IsEllipse
        {
            get { return AxisA == 0.0; }
        }

        /// <summary>Gets the circle radius for circle rows, or zero for ellipse rows. 获取圆行半径；椭圆行为零。</summary>
        public double Radius
        {
            get { return AxisA; }
        }

        /// <summary>Gets the first ellipse axis for ellipse rows. 获取椭圆行的第一轴。</summary>
        public double EllipseAxisA
        {
            get { return AxisB; }
        }

        /// <summary>Gets the second ellipse axis for ellipse rows. 获取椭圆行的第二轴。</summary>
        public double EllipseAxisB
        {
            get { return Angle; }
        }

        /// <summary>Gets the ellipse rotation angle for ellipse rows. 获取椭圆行的旋转角度。</summary>
        public double EllipseAngle
        {
            get { return Score; }
        }

        /// <summary>Determines whether two ellipses are equal. 判断两个椭圆候选是否相等。</summary>
        public static bool operator ==(EdgeDrawingEllipse left, EdgeDrawingEllipse right)
        {
            return left.Equals(right);
        }

        /// <summary>Determines whether two ellipses are different. 判断两个椭圆候选是否不同。</summary>
        public static bool operator !=(EdgeDrawingEllipse left, EdgeDrawingEllipse right)
        {
            return !left.Equals(right);
        }

        /// <summary>Indicates whether this ellipse equals another ellipse. 指示此椭圆候选是否与另一个椭圆候选相等。</summary>
        public bool Equals(EdgeDrawingEllipse other)
        {
            return Center.Equals(other.Center)
                && AxisA.Equals(other.AxisA)
                && AxisB.Equals(other.AxisB)
                && Angle.Equals(other.Angle)
                && Score.Equals(other.Score);
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is EdgeDrawingEllipse other && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = Center.GetHashCode();
                hash = (hash * 397) ^ AxisA.GetHashCode();
                hash = (hash * 397) ^ AxisB.GetHashCode();
                hash = (hash * 397) ^ Angle.GetHashCode();
                hash = (hash * 397) ^ Score.GetHashCode();
                return hash;
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{{Center={{X={0},Y={1}}},AxisA={2},AxisB={3},Angle={4},Score={5}}}",
                Center.X,
                Center.Y,
                AxisA,
                AxisB,
                Angle,
                Score);
        }
    }
}
