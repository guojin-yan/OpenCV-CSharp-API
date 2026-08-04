using System;
using System.Runtime.InteropServices;
using JYPPX.OpenCvSharp.Core;

namespace JYPPX.OpenCvSharp.Saliency
{
    /// <summary>
    /// Bounding box returned by OpenCV contrib ObjectnessBING as min/max coordinates.
    /// OpenCV contrib ObjectnessBING 返回的 min/max 坐标候选框。
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct ObjectnessBINGBox : IEquatable<ObjectnessBINGBox>
    {
        /// <summary>Initializes a box from min/max coordinates. 使用 min/max 坐标初始化候选框。</summary>
        public ObjectnessBINGBox(int minX, int minY, int maxX, int maxY)
        {
            MinX = minX;
            MinY = minY;
            MaxX = maxX;
            MaxY = maxY;
        }

        /// <summary>Gets the minimum X coordinate. 获取最小 X 坐标。</summary>
        public int MinX { get; }

        /// <summary>Gets the minimum Y coordinate. 获取最小 Y 坐标。</summary>
        public int MinY { get; }

        /// <summary>Gets the maximum X coordinate. 获取最大 X 坐标。</summary>
        public int MaxX { get; }

        /// <summary>Gets the maximum Y coordinate. 获取最大 Y 坐标。</summary>
        public int MaxY { get; }

        /// <summary>Gets the box width. 获取候选框宽度。</summary>
        public int Width
        {
            get { return MaxX - MinX; }
        }

        /// <summary>Gets the box height. 获取候选框高度。</summary>
        public int Height
        {
            get { return MaxY - MinY; }
        }

        /// <summary>Converts this min/max box to a <see cref="Rect"/>. 将 min/max 候选框转换为 <see cref="Rect"/>。</summary>
        public Rect ToRect()
        {
            return new Rect(MinX, MinY, Width, Height);
        }

        /// <summary>Determines whether two boxes are equal. 判断两个候选框是否相等。</summary>
        public static bool operator ==(ObjectnessBINGBox left, ObjectnessBINGBox right)
        {
            return left.Equals(right);
        }

        /// <summary>Determines whether two boxes are different. 判断两个候选框是否不同。</summary>
        public static bool operator !=(ObjectnessBINGBox left, ObjectnessBINGBox right)
        {
            return !left.Equals(right);
        }

        /// <summary>Indicates whether this box equals another box. 指示此候选框是否与另一个候选框相等。</summary>
        public bool Equals(ObjectnessBINGBox other)
        {
            return MinX == other.MinX &&
                MinY == other.MinY &&
                MaxX == other.MaxX &&
                MaxY == other.MaxY;
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is ObjectnessBINGBox other && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = MinX;
                hash = (hash * 397) ^ MinY;
                hash = (hash * 397) ^ MaxX;
                hash = (hash * 397) ^ MaxY;
                return hash;
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "{MinX=" + MinX + ",MinY=" + MinY + ",MaxX=" + MaxX + ",MaxY=" + MaxY + "}";
        }
    }
}
