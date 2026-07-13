using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.ObjDetect
{
    /// <summary>
    /// Result returned by ArUco dictionary identification.
    /// ArUco 字典识别返回的结果。
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct ArucoIdentificationResult : IEquatable<ArucoIdentificationResult>
    {
        /// <summary>
        /// Initializes an identification result.
        /// 初始化识别结果。
        /// </summary>
        public ArucoIdentificationResult(bool identified, int index, int rotation)
        {
            if (identified)
            {
                if (index < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                if (rotation < 0 || rotation > 3)
                {
                    throw new ArgumentOutOfRangeException(nameof(rotation));
                }
            }

            Identified = identified;
            Index = index;
            Rotation = rotation;
        }

        /// <summary>Gets whether a marker was identified. 获取是否识别到 marker。</summary>
        public bool Identified { get; }

        /// <summary>Gets the marker index in the dictionary. 获取字典中的 marker 索引。</summary>
        public int Index { get; }

        /// <summary>Gets the selected marker rotation. 获取选中的 marker 旋转。</summary>
        public int Rotation { get; }

        /// <summary>
        /// Determines whether two values are equal.
        /// 判断两个值是否相等。
        /// </summary>
        public static bool operator ==(ArucoIdentificationResult left, ArucoIdentificationResult right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two values are not equal.
        /// 判断两个值是否不相等。
        /// </summary>
        public static bool operator !=(ArucoIdentificationResult left, ArucoIdentificationResult right)
        {
            return !left.Equals(right);
        }

        /// <inheritdoc/>
        public bool Equals(ArucoIdentificationResult other)
        {
            return Identified == other.Identified &&
                Index == other.Index &&
                Rotation == other.Rotation;
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is ArucoIdentificationResult other && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hash = Identified.GetHashCode();
            hash = (hash * 397) ^ Index;
            hash = (hash * 397) ^ Rotation;
            return hash;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "{Identified=" + Identified + ",Index=" + Index + ",Rotation=" + Rotation + "}";
        }
    }
}
