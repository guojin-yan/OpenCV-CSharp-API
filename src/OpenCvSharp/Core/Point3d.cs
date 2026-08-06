using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Core
{
    /// <summary>Represents a three-dimensional double-precision point compatible with <c>cv::Point3d</c>.</summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Point3d : IEquatable<Point3d>
    {
        /// <summary>Creates a three-dimensional point.</summary>
        public Point3d(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>Gets the x coordinate.</summary>
        public double X { get; }
        /// <summary>Gets the y coordinate.</summary>
        public double Y { get; }
        /// <summary>Gets the z coordinate.</summary>
        public double Z { get; }

        /// <inheritdoc/>
        public bool Equals(Point3d other) { return X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z); }
        /// <inheritdoc/>
        public override bool Equals(object? obj) { return obj is Point3d other && Equals(other); }
        /// <inheritdoc/>
        public override int GetHashCode() { unchecked { return ((X.GetHashCode() * 397) ^ Y.GetHashCode()) * 397 ^ Z.GetHashCode(); } }
        /// <inheritdoc/>
        public override string ToString() { return string.Format(CultureInfo.InvariantCulture, "{{X={0},Y={1},Z={2}}}", X, Y, Z); }
        /// <summary>Determines whether two points are equal.</summary>
        public static bool operator ==(Point3d left, Point3d right) { return left.Equals(right); }
        /// <summary>Determines whether two points are different.</summary>
        public static bool operator !=(Point3d left, Point3d right) { return !left.Equals(right); }
    }
}
