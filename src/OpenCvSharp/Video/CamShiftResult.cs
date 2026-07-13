using System;
using System.Runtime.InteropServices;
using OpenCvSharp.Core;

namespace OpenCvSharp.Video
{
    /// <summary>
    /// Result returned by CamShift tracking.
    /// CamShift 跟踪返回的结果。
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct CamShiftResult : IEquatable<CamShiftResult>
    {
        /// <summary>
        /// Initializes a new result.
        /// 初始化结果。
        /// </summary>
        public CamShiftResult(Rect window, RotatedRect box)
        {
            Window = window;
            Box = box;
        }

        /// <summary>
        /// Gets the updated search window.
        /// 获取更新后的搜索窗口。
        /// </summary>
        public Rect Window { get; }

        /// <summary>
        /// Gets the rotated tracking box.
        /// 获取旋转跟踪框。
        /// </summary>
        public RotatedRect Box { get; }

        /// <summary>
        /// Determines whether two results are equal.
        /// 判断两个结果是否相等。
        /// </summary>
        public static bool operator ==(CamShiftResult left, CamShiftResult right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two results are different.
        /// 判断两个结果是否不同。
        /// </summary>
        public static bool operator !=(CamShiftResult left, CamShiftResult right)
        {
            return !left.Equals(right);
        }

        /// <inheritdoc/>
        public bool Equals(CamShiftResult other)
        {
            return Window.Equals(other.Window) &&
                Box.Equals(other.Box);
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is CamShiftResult other && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                return (Window.GetHashCode() * 397) ^ Box.GetHashCode();
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "{Window=" + Window + ",Box=" + Box + "}";
        }
    }
}
