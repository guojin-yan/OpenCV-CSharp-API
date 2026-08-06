using System;
using System.Globalization;
using System.Runtime.InteropServices;

#pragma warning disable CS1591

namespace JYPPX.OpenCvSharp.Core
{
    /// <summary>Represents two unsigned byte values compatible with <c>cv::Vec2b</c>.</summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Vec2b : IEquatable<Vec2b>
    {
        public Vec2b(byte v0, byte v1) { V0 = v0; V1 = v1; }
        public byte V0 { get; }
        public byte V1 { get; }
        public byte this[int index] { get { return index == 0 ? V0 : index == 1 ? V1 : throw new IndexOutOfRangeException(); } }
        public void Deconstruct(out byte v0, out byte v1) { v0 = V0; v1 = V1; }
        public bool Equals(Vec2b other) { return V0 == other.V0 && V1 == other.V1; }
        public override bool Equals(object? obj) { return obj is Vec2b other && Equals(other); }
        public override int GetHashCode() { return (V0 * 397) ^ V1; }
        public override string ToString() { return "{V0=" + V0 + ",V1=" + V1 + "}"; }
        public static bool operator ==(Vec2b left, Vec2b right) { return left.Equals(right); }
        public static bool operator !=(Vec2b left, Vec2b right) { return !left.Equals(right); }
    }

    /// <summary>Represents a BGR-compatible three-byte pixel or <c>cv::Vec3b</c> value.</summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Vec3b : IEquatable<Vec3b>
    {
        public Vec3b(byte v0, byte v1, byte v2) { V0 = v0; V1 = v1; V2 = v2; }
        public byte V0 { get; }
        public byte V1 { get; }
        public byte V2 { get; }
        public byte this[int index]
        {
            get
            {
                switch (index) { case 0: return V0; case 1: return V1; case 2: return V2; default: throw new IndexOutOfRangeException(); }
            }
        }
        public void Deconstruct(out byte v0, out byte v1, out byte v2) { v0 = V0; v1 = V1; v2 = V2; }
        public bool Equals(Vec3b other) { return V0 == other.V0 && V1 == other.V1 && V2 == other.V2; }
        public override bool Equals(object? obj) { return obj is Vec3b other && Equals(other); }
        public override int GetHashCode() { unchecked { return ((V0 * 397) ^ V1) * 397 ^ V2; } }
        public override string ToString() { return "{V0=" + V0 + ",V1=" + V1 + ",V2=" + V2 + "}"; }
        public static bool operator ==(Vec3b left, Vec3b right) { return left.Equals(right); }
        public static bool operator !=(Vec3b left, Vec3b right) { return !left.Equals(right); }
    }

    /// <summary>Represents a BGRA-compatible four-byte pixel or <c>cv::Vec4b</c> value.</summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Vec4b : IEquatable<Vec4b>
    {
        public Vec4b(byte v0, byte v1, byte v2, byte v3) { V0 = v0; V1 = v1; V2 = v2; V3 = v3; }
        public byte V0 { get; }
        public byte V1 { get; }
        public byte V2 { get; }
        public byte V3 { get; }
        public byte this[int index]
        {
            get
            {
                switch (index) { case 0: return V0; case 1: return V1; case 2: return V2; case 3: return V3; default: throw new IndexOutOfRangeException(); }
            }
        }
        public void Deconstruct(out byte v0, out byte v1, out byte v2, out byte v3) { v0 = V0; v1 = V1; v2 = V2; v3 = V3; }
        public bool Equals(Vec4b other) { return V0 == other.V0 && V1 == other.V1 && V2 == other.V2 && V3 == other.V3; }
        public override bool Equals(object? obj) { return obj is Vec4b other && Equals(other); }
        public override int GetHashCode() { unchecked { int hash = V0; hash = (hash * 397) ^ V1; hash = (hash * 397) ^ V2; return (hash * 397) ^ V3; } }
        public override string ToString() { return "{V0=" + V0 + ",V1=" + V1 + ",V2=" + V2 + ",V3=" + V3 + "}"; }
        public static bool operator ==(Vec4b left, Vec4b right) { return left.Equals(right); }
        public static bool operator !=(Vec4b left, Vec4b right) { return !left.Equals(right); }
    }

    /// <summary>Represents two single-precision values compatible with <c>cv::Vec2f</c>.</summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Vec2f : IEquatable<Vec2f>
    {
        public Vec2f(float v0, float v1) { V0 = v0; V1 = v1; }
        public float V0 { get; }
        public float V1 { get; }
        public float this[int index] { get { return index == 0 ? V0 : index == 1 ? V1 : throw new IndexOutOfRangeException(); } }
        public void Deconstruct(out float v0, out float v1) { v0 = V0; v1 = V1; }
        public bool Equals(Vec2f other) { return V0.Equals(other.V0) && V1.Equals(other.V1); }
        public override bool Equals(object? obj) { return obj is Vec2f other && Equals(other); }
        public override int GetHashCode() { unchecked { return (V0.GetHashCode() * 397) ^ V1.GetHashCode(); } }
        public override string ToString() { return Format("Vec2f", V0, V1); }
        public static bool operator ==(Vec2f left, Vec2f right) { return left.Equals(right); }
        public static bool operator !=(Vec2f left, Vec2f right) { return !left.Equals(right); }
        private static string Format(string name, float v0, float v1) { return name + "(" + v0.ToString(CultureInfo.InvariantCulture) + "," + v1.ToString(CultureInfo.InvariantCulture) + ")"; }
    }

    /// <summary>Represents three single-precision values compatible with <c>cv::Vec3f</c>.</summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Vec3f : IEquatable<Vec3f>
    {
        public Vec3f(float v0, float v1, float v2) { V0 = v0; V1 = v1; V2 = v2; }
        public float V0 { get; }
        public float V1 { get; }
        public float V2 { get; }
        public float this[int index]
        {
            get { switch (index) { case 0: return V0; case 1: return V1; case 2: return V2; default: throw new IndexOutOfRangeException(); } }
        }
        public void Deconstruct(out float v0, out float v1, out float v2) { v0 = V0; v1 = V1; v2 = V2; }
        public bool Equals(Vec3f other) { return V0.Equals(other.V0) && V1.Equals(other.V1) && V2.Equals(other.V2); }
        public override bool Equals(object? obj) { return obj is Vec3f other && Equals(other); }
        public override int GetHashCode() { unchecked { return ((V0.GetHashCode() * 397) ^ V1.GetHashCode()) * 397 ^ V2.GetHashCode(); } }
        public override string ToString() { return "Vec3f(" + V0.ToString(CultureInfo.InvariantCulture) + "," + V1.ToString(CultureInfo.InvariantCulture) + "," + V2.ToString(CultureInfo.InvariantCulture) + ")"; }
        public static bool operator ==(Vec3f left, Vec3f right) { return left.Equals(right); }
        public static bool operator !=(Vec3f left, Vec3f right) { return !left.Equals(right); }
    }

    /// <summary>Represents two double-precision values compatible with <c>cv::Vec2d</c>.</summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Vec2d : IEquatable<Vec2d>
    {
        public Vec2d(double v0, double v1) { V0 = v0; V1 = v1; }
        public double V0 { get; }
        public double V1 { get; }
        public double this[int index] { get { return index == 0 ? V0 : index == 1 ? V1 : throw new IndexOutOfRangeException(); } }
        public void Deconstruct(out double v0, out double v1) { v0 = V0; v1 = V1; }
        public bool Equals(Vec2d other) { return V0.Equals(other.V0) && V1.Equals(other.V1); }
        public override bool Equals(object? obj) { return obj is Vec2d other && Equals(other); }
        public override int GetHashCode() { unchecked { return (V0.GetHashCode() * 397) ^ V1.GetHashCode(); } }
        public override string ToString() { return "Vec2d(" + V0.ToString(CultureInfo.InvariantCulture) + "," + V1.ToString(CultureInfo.InvariantCulture) + ")"; }
        public static bool operator ==(Vec2d left, Vec2d right) { return left.Equals(right); }
        public static bool operator !=(Vec2d left, Vec2d right) { return !left.Equals(right); }
    }

    /// <summary>Represents three double-precision values compatible with <c>cv::Vec3d</c>.</summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Vec3d : IEquatable<Vec3d>
    {
        public Vec3d(double v0, double v1, double v2) { V0 = v0; V1 = v1; V2 = v2; }
        public double V0 { get; }
        public double V1 { get; }
        public double V2 { get; }
        public double this[int index]
        {
            get { switch (index) { case 0: return V0; case 1: return V1; case 2: return V2; default: throw new IndexOutOfRangeException(); } }
        }
        public void Deconstruct(out double v0, out double v1, out double v2) { v0 = V0; v1 = V1; v2 = V2; }
        public bool Equals(Vec3d other) { return V0.Equals(other.V0) && V1.Equals(other.V1) && V2.Equals(other.V2); }
        public override bool Equals(object? obj) { return obj is Vec3d other && Equals(other); }
        public override int GetHashCode() { unchecked { return ((V0.GetHashCode() * 397) ^ V1.GetHashCode()) * 397 ^ V2.GetHashCode(); } }
        public override string ToString() { return "Vec3d(" + V0.ToString(CultureInfo.InvariantCulture) + "," + V1.ToString(CultureInfo.InvariantCulture) + "," + V2.ToString(CultureInfo.InvariantCulture) + ")"; }
        public static bool operator ==(Vec3d left, Vec3d right) { return left.Equals(right); }
        public static bool operator !=(Vec3d left, Vec3d right) { return !left.Equals(right); }
    }

    /// <summary>Represents four double-precision values compatible with <c>cv::Vec4d</c>.</summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Vec4d : IEquatable<Vec4d>
    {
        public Vec4d(double v0, double v1, double v2, double v3) { V0 = v0; V1 = v1; V2 = v2; V3 = v3; }
        public double V0 { get; }
        public double V1 { get; }
        public double V2 { get; }
        public double V3 { get; }
        public double this[int index]
        {
            get { switch (index) { case 0: return V0; case 1: return V1; case 2: return V2; case 3: return V3; default: throw new IndexOutOfRangeException(); } }
        }
        public void Deconstruct(out double v0, out double v1, out double v2, out double v3) { v0 = V0; v1 = V1; v2 = V2; v3 = V3; }
        public bool Equals(Vec4d other) { return V0.Equals(other.V0) && V1.Equals(other.V1) && V2.Equals(other.V2) && V3.Equals(other.V3); }
        public override bool Equals(object? obj) { return obj is Vec4d other && Equals(other); }
        public override int GetHashCode() { unchecked { int hash = V0.GetHashCode(); hash = (hash * 397) ^ V1.GetHashCode(); hash = (hash * 397) ^ V2.GetHashCode(); return (hash * 397) ^ V3.GetHashCode(); } }
        public override string ToString() { return "Vec4d(" + V0.ToString(CultureInfo.InvariantCulture) + "," + V1.ToString(CultureInfo.InvariantCulture) + "," + V2.ToString(CultureInfo.InvariantCulture) + "," + V3.ToString(CultureInfo.InvariantCulture) + ")"; }
        public static bool operator ==(Vec4d left, Vec4d right) { return left.Equals(right); }
        public static bool operator !=(Vec4d left, Vec4d right) { return !left.Equals(right); }
    }

    /// <summary>Represents two signed 32-bit values compatible with <c>cv::Vec2i</c>.</summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Vec2i : IEquatable<Vec2i>
    {
        public Vec2i(int v0, int v1) { V0 = v0; V1 = v1; }
        public int V0 { get; }
        public int V1 { get; }
        public int this[int index] { get { return index == 0 ? V0 : index == 1 ? V1 : throw new IndexOutOfRangeException(); } }
        public void Deconstruct(out int v0, out int v1) { v0 = V0; v1 = V1; }
        public bool Equals(Vec2i other) { return V0 == other.V0 && V1 == other.V1; }
        public override bool Equals(object? obj) { return obj is Vec2i other && Equals(other); }
        public override int GetHashCode() { unchecked { return (V0 * 397) ^ V1; } }
        public override string ToString() { return "{V0=" + V0 + ",V1=" + V1 + "}"; }
        public static bool operator ==(Vec2i left, Vec2i right) { return left.Equals(right); }
        public static bool operator !=(Vec2i left, Vec2i right) { return !left.Equals(right); }
    }

    /// <summary>Represents three signed 32-bit values compatible with <c>cv::Vec3i</c>.</summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Vec3i : IEquatable<Vec3i>
    {
        public Vec3i(int v0, int v1, int v2) { V0 = v0; V1 = v1; V2 = v2; }
        public int V0 { get; }
        public int V1 { get; }
        public int V2 { get; }
        public int this[int index]
        {
            get { switch (index) { case 0: return V0; case 1: return V1; case 2: return V2; default: throw new IndexOutOfRangeException(); } }
        }
        public void Deconstruct(out int v0, out int v1, out int v2) { v0 = V0; v1 = V1; v2 = V2; }
        public bool Equals(Vec3i other) { return V0 == other.V0 && V1 == other.V1 && V2 == other.V2; }
        public override bool Equals(object? obj) { return obj is Vec3i other && Equals(other); }
        public override int GetHashCode() { unchecked { return ((V0 * 397) ^ V1) * 397 ^ V2; } }
        public override string ToString() { return "{V0=" + V0 + ",V1=" + V1 + ",V2=" + V2 + "}"; }
        public static bool operator ==(Vec3i left, Vec3i right) { return left.Equals(right); }
        public static bool operator !=(Vec3i left, Vec3i right) { return !left.Equals(right); }
    }

    /// <summary>Represents two signed 16-bit values compatible with <c>cv::Vec2s</c>.</summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Vec2s : IEquatable<Vec2s>
    {
        public Vec2s(short v0, short v1) { V0 = v0; V1 = v1; }
        public short V0 { get; }
        public short V1 { get; }
        public short this[int index] { get { return index == 0 ? V0 : index == 1 ? V1 : throw new IndexOutOfRangeException(); } }
        public void Deconstruct(out short v0, out short v1) { v0 = V0; v1 = V1; }
        public bool Equals(Vec2s other) { return V0 == other.V0 && V1 == other.V1; }
        public override bool Equals(object? obj) { return obj is Vec2s other && Equals(other); }
        public override int GetHashCode() { return (V0 * 397) ^ V1; }
        public override string ToString() { return "{V0=" + V0 + ",V1=" + V1 + "}"; }
        public static bool operator ==(Vec2s left, Vec2s right) { return left.Equals(right); }
        public static bool operator !=(Vec2s left, Vec2s right) { return !left.Equals(right); }
    }

    /// <summary>Represents three signed 16-bit values compatible with <c>cv::Vec3s</c>.</summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Vec3s : IEquatable<Vec3s>
    {
        public Vec3s(short v0, short v1, short v2) { V0 = v0; V1 = v1; V2 = v2; }
        public short V0 { get; }
        public short V1 { get; }
        public short V2 { get; }
        public short this[int index] { get { switch (index) { case 0: return V0; case 1: return V1; case 2: return V2; default: throw new IndexOutOfRangeException(); } } }
        public void Deconstruct(out short v0, out short v1, out short v2) { v0 = V0; v1 = V1; v2 = V2; }
        public bool Equals(Vec3s other) { return V0 == other.V0 && V1 == other.V1 && V2 == other.V2; }
        public override bool Equals(object? obj) { return obj is Vec3s other && Equals(other); }
        public override int GetHashCode() { unchecked { return ((V0 * 397) ^ V1) * 397 ^ V2; } }
        public override string ToString() { return "{V0=" + V0 + ",V1=" + V1 + ",V2=" + V2 + "}"; }
        public static bool operator ==(Vec3s left, Vec3s right) { return left.Equals(right); }
        public static bool operator !=(Vec3s left, Vec3s right) { return !left.Equals(right); }
    }

    /// <summary>Represents four signed 16-bit values compatible with <c>cv::Vec4s</c>.</summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Vec4s : IEquatable<Vec4s>
    {
        public Vec4s(short v0, short v1, short v2, short v3) { V0 = v0; V1 = v1; V2 = v2; V3 = v3; }
        public short V0 { get; }
        public short V1 { get; }
        public short V2 { get; }
        public short V3 { get; }
        public short this[int index] { get { switch (index) { case 0: return V0; case 1: return V1; case 2: return V2; case 3: return V3; default: throw new IndexOutOfRangeException(); } } }
        public void Deconstruct(out short v0, out short v1, out short v2, out short v3) { v0 = V0; v1 = V1; v2 = V2; v3 = V3; }
        public bool Equals(Vec4s other) { return V0 == other.V0 && V1 == other.V1 && V2 == other.V2 && V3 == other.V3; }
        public override bool Equals(object? obj) { return obj is Vec4s other && Equals(other); }
        public override int GetHashCode() { unchecked { int hash = V0; hash = (hash * 397) ^ V1; hash = (hash * 397) ^ V2; return (hash * 397) ^ V3; } }
        public override string ToString() { return "{V0=" + V0 + ",V1=" + V1 + ",V2=" + V2 + ",V3=" + V3 + "}"; }
        public static bool operator ==(Vec4s left, Vec4s right) { return left.Equals(right); }
        public static bool operator !=(Vec4s left, Vec4s right) { return !left.Equals(right); }
    }

    /// <summary>Represents two unsigned 16-bit values compatible with <c>cv::Vec2w</c>.</summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Vec2w : IEquatable<Vec2w>
    {
        public Vec2w(ushort v0, ushort v1) { V0 = v0; V1 = v1; }
        public ushort V0 { get; }
        public ushort V1 { get; }
        public ushort this[int index] { get { return index == 0 ? V0 : index == 1 ? V1 : throw new IndexOutOfRangeException(); } }
        public void Deconstruct(out ushort v0, out ushort v1) { v0 = V0; v1 = V1; }
        public bool Equals(Vec2w other) { return V0 == other.V0 && V1 == other.V1; }
        public override bool Equals(object? obj) { return obj is Vec2w other && Equals(other); }
        public override int GetHashCode() { return (V0 * 397) ^ V1; }
        public override string ToString() { return "{V0=" + V0 + ",V1=" + V1 + "}"; }
        public static bool operator ==(Vec2w left, Vec2w right) { return left.Equals(right); }
        public static bool operator !=(Vec2w left, Vec2w right) { return !left.Equals(right); }
    }

    /// <summary>Represents three unsigned 16-bit values compatible with <c>cv::Vec3w</c>.</summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Vec3w : IEquatable<Vec3w>
    {
        public Vec3w(ushort v0, ushort v1, ushort v2) { V0 = v0; V1 = v1; V2 = v2; }
        public ushort V0 { get; }
        public ushort V1 { get; }
        public ushort V2 { get; }
        public ushort this[int index] { get { switch (index) { case 0: return V0; case 1: return V1; case 2: return V2; default: throw new IndexOutOfRangeException(); } } }
        public void Deconstruct(out ushort v0, out ushort v1, out ushort v2) { v0 = V0; v1 = V1; v2 = V2; }
        public bool Equals(Vec3w other) { return V0 == other.V0 && V1 == other.V1 && V2 == other.V2; }
        public override bool Equals(object? obj) { return obj is Vec3w other && Equals(other); }
        public override int GetHashCode() { unchecked { return ((V0 * 397) ^ V1) * 397 ^ V2; } }
        public override string ToString() { return "{V0=" + V0 + ",V1=" + V1 + ",V2=" + V2 + "}"; }
        public static bool operator ==(Vec3w left, Vec3w right) { return left.Equals(right); }
        public static bool operator !=(Vec3w left, Vec3w right) { return !left.Equals(right); }
    }

    /// <summary>Represents four unsigned 16-bit values compatible with <c>cv::Vec4w</c>.</summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Vec4w : IEquatable<Vec4w>
    {
        public Vec4w(ushort v0, ushort v1, ushort v2, ushort v3) { V0 = v0; V1 = v1; V2 = v2; V3 = v3; }
        public ushort V0 { get; }
        public ushort V1 { get; }
        public ushort V2 { get; }
        public ushort V3 { get; }
        public ushort this[int index] { get { switch (index) { case 0: return V0; case 1: return V1; case 2: return V2; case 3: return V3; default: throw new IndexOutOfRangeException(); } } }
        public void Deconstruct(out ushort v0, out ushort v1, out ushort v2, out ushort v3) { v0 = V0; v1 = V1; v2 = V2; v3 = V3; }
        public bool Equals(Vec4w other) { return V0 == other.V0 && V1 == other.V1 && V2 == other.V2 && V3 == other.V3; }
        public override bool Equals(object? obj) { return obj is Vec4w other && Equals(other); }
        public override int GetHashCode() { unchecked { int hash = V0; hash = (hash * 397) ^ V1; hash = (hash * 397) ^ V2; return (hash * 397) ^ V3; } }
        public override string ToString() { return "{V0=" + V0 + ",V1=" + V1 + ",V2=" + V2 + ",V3=" + V3 + "}"; }
        public static bool operator ==(Vec4w left, Vec4w right) { return left.Equals(right); }
        public static bool operator !=(Vec4w left, Vec4w right) { return !left.Equals(right); }
    }
}

#pragma warning restore CS1591
