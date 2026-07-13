using System;
using System.Globalization;
using System.Runtime.InteropServices;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.LineDescriptor
{
    /// <summary>
    /// Represents an OpenCV line-descriptor key line.
    /// 表示 OpenCV line_descriptor 模块中的关键线段。
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct KeyLine : IEquatable<KeyLine>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KeyLine"/> struct.
        /// 初始化 <see cref="KeyLine"/> 结构的新实例。
        /// </summary>
        public KeyLine(
            float angle,
            int classId,
            int octave,
            Point2f pt,
            float response,
            float size,
            Point2f startPoint,
            Point2f endPoint,
            Point2f startPointInOctave,
            Point2f endPointInOctave,
            float lineLength,
            int numOfPixels)
        {
            Angle = angle;
            ClassId = classId;
            Octave = octave;
            Pt = pt;
            Response = response;
            Size = size;
            StartPoint = startPoint;
            EndPoint = endPoint;
            StartPointInOctave = startPointInOctave;
            EndPointInOctave = endPointInOctave;
            LineLength = lineLength;
            NumOfPixels = numOfPixels;
        }

        /// <summary>Gets the line orientation. 获取线段方向。</summary>
        public float Angle { get; }

        /// <summary>Gets the line class id. 获取线段类别 ID。</summary>
        public int ClassId { get; }

        /// <summary>Gets the pyramid octave. 获取金字塔 octave。</summary>
        public int Octave { get; }

        /// <summary>Gets the line midpoint. 获取线段中点。</summary>
        public Point2f Pt { get; }

        /// <summary>Gets the detector response. 获取检测响应值。</summary>
        public float Response { get; }

        /// <summary>Gets the minimum area containing the line. 获取包围线段的最小面积。</summary>
        public float Size { get; }

        /// <summary>Gets the start point in the original image. 获取原图中的起点。</summary>
        public Point2f StartPoint { get; }

        /// <summary>Gets the end point in the original image. 获取原图中的终点。</summary>
        public Point2f EndPoint { get; }

        /// <summary>Gets the start point in the source octave image. 获取 octave 图像中的起点。</summary>
        public Point2f StartPointInOctave { get; }

        /// <summary>Gets the end point in the source octave image. 获取 octave 图像中的终点。</summary>
        public Point2f EndPointInOctave { get; }

        /// <summary>Gets the line length. 获取线段长度。</summary>
        public float LineLength { get; }

        /// <summary>Gets the number of covered pixels. 获取线段覆盖的像素数量。</summary>
        public int NumOfPixels { get; }

        internal static KeyLine FromNative(NativeLineDescriptorKeyLine value)
        {
            return new KeyLine(
                value.Angle,
                value.ClassId,
                value.Octave,
                new Point2f(value.PtX, value.PtY),
                value.Response,
                value.Size,
                new Point2f(value.StartPointX, value.StartPointY),
                new Point2f(value.EndPointX, value.EndPointY),
                new Point2f(value.StartPointInOctaveX, value.StartPointInOctaveY),
                new Point2f(value.EndPointInOctaveX, value.EndPointInOctaveY),
                value.LineLength,
                value.NumOfPixels);
        }

        internal NativeLineDescriptorKeyLine ToNative()
        {
            return new NativeLineDescriptorKeyLine
            {
                Angle = Angle,
                ClassId = ClassId,
                Octave = Octave,
                PtX = Pt.X,
                PtY = Pt.Y,
                Response = Response,
                Size = Size,
                StartPointX = StartPoint.X,
                StartPointY = StartPoint.Y,
                EndPointX = EndPoint.X,
                EndPointY = EndPoint.Y,
                StartPointInOctaveX = StartPointInOctave.X,
                StartPointInOctaveY = StartPointInOctave.Y,
                EndPointInOctaveX = EndPointInOctave.X,
                EndPointInOctaveY = EndPointInOctave.Y,
                LineLength = LineLength,
                NumOfPixels = NumOfPixels
            };
        }

        /// <summary>Determines whether two key lines are equal. 判断两个关键线段是否相等。</summary>
        public static bool operator ==(KeyLine left, KeyLine right)
        {
            return left.Equals(right);
        }

        /// <summary>Determines whether two key lines are different. 判断两个关键线段是否不同。</summary>
        public static bool operator !=(KeyLine left, KeyLine right)
        {
            return !left.Equals(right);
        }

        /// <summary>Returns the start point in the original image. 返回原图中的起点。</summary>
        public Point2f GetStartPoint()
        {
            return StartPoint;
        }

        /// <summary>Returns the end point in the original image. 返回原图中的终点。</summary>
        public Point2f GetEndPoint()
        {
            return EndPoint;
        }

        /// <summary>Returns the start point in the octave image. 返回 octave 图像中的起点。</summary>
        public Point2f GetStartPointInOctave()
        {
            return StartPointInOctave;
        }

        /// <summary>Returns the end point in the octave image. 返回 octave 图像中的终点。</summary>
        public Point2f GetEndPointInOctave()
        {
            return EndPointInOctave;
        }

        /// <summary>Indicates whether this key line equals another key line. 指示此关键线段是否与另一个关键线段相等。</summary>
        public bool Equals(KeyLine other)
        {
            return Angle.Equals(other.Angle)
                && ClassId == other.ClassId
                && Octave == other.Octave
                && Pt.Equals(other.Pt)
                && Response.Equals(other.Response)
                && Size.Equals(other.Size)
                && StartPoint.Equals(other.StartPoint)
                && EndPoint.Equals(other.EndPoint)
                && StartPointInOctave.Equals(other.StartPointInOctave)
                && EndPointInOctave.Equals(other.EndPointInOctave)
                && LineLength.Equals(other.LineLength)
                && NumOfPixels == other.NumOfPixels;
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is KeyLine other && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = Angle.GetHashCode();
                hash = (hash * 397) ^ ClassId;
                hash = (hash * 397) ^ Octave;
                hash = (hash * 397) ^ Pt.GetHashCode();
                hash = (hash * 397) ^ Response.GetHashCode();
                hash = (hash * 397) ^ Size.GetHashCode();
                hash = (hash * 397) ^ StartPoint.GetHashCode();
                hash = (hash * 397) ^ EndPoint.GetHashCode();
                hash = (hash * 397) ^ StartPointInOctave.GetHashCode();
                hash = (hash * 397) ^ EndPointInOctave.GetHashCode();
                hash = (hash * 397) ^ LineLength.GetHashCode();
                hash = (hash * 397) ^ NumOfPixels;
                return hash;
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "{Pt=" + Pt
                + ",StartPoint=" + StartPoint
                + ",EndPoint=" + EndPoint
                + ",Angle=" + Angle.ToString(CultureInfo.InvariantCulture)
                + ",LineLength=" + LineLength.ToString(CultureInfo.InvariantCulture) + "}";
        }
    }
}
