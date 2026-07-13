using System;
using System.Globalization;
using System.Runtime.InteropServices;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.ObjDetect
{
    /// <summary>
    /// Parameters for ArUco-based QR detection.
    /// 基于 ArUco 的二维码检测参数。
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct QRCodeDetectorArucoParams : IEquatable<QRCodeDetectorArucoParams>
    {
        /// <summary>
        /// Initializes ArUco QR detector parameters.
        /// 初始化 ArUco 二维码检测参数。
        /// </summary>
        public QRCodeDetectorArucoParams(
            float minModuleSizeInPyramid,
            float maxRotation,
            float maxModuleSizeMismatch,
            float maxTimingPatternMismatch,
            float maxPenalties,
            float maxColorsMismatch,
            float scaleTimingPatternScore)
        {
            MinModuleSizeInPyramid = minModuleSizeInPyramid;
            MaxRotation = maxRotation;
            MaxModuleSizeMismatch = maxModuleSizeMismatch;
            MaxTimingPatternMismatch = maxTimingPatternMismatch;
            MaxPenalties = maxPenalties;
            MaxColorsMismatch = maxColorsMismatch;
            ScaleTimingPatternScore = scaleTimingPatternScore;
        }

        /// <summary>Gets the minimum QR module size in the image pyramid. 获取图像金字塔中的最小二维码模块尺寸。</summary>
        public float MinModuleSizeInPyramid { get; }

        /// <summary>Gets the maximum relative finder-pattern rotation. 获取定位图案最大相对旋转。</summary>
        public float MaxRotation { get; }

        /// <summary>Gets the maximum finder-pattern module-size mismatch. 获取定位图案模块尺寸最大不匹配值。</summary>
        public float MaxModuleSizeMismatch { get; }

        /// <summary>Gets the maximum timing-pattern mismatch. 获取 timing pattern 最大不匹配值。</summary>
        public float MaxTimingPatternMismatch { get; }

        /// <summary>Gets the maximum penalty ratio. 获取最大惩罚比例。</summary>
        public float MaxPenalties { get; }

        /// <summary>Gets the maximum timing-pattern color mismatch. 获取 timing pattern 最大颜色不匹配值。</summary>
        public float MaxColorsMismatch { get; }

        /// <summary>Gets the timing-pattern score scale. 获取 timing pattern 分数缩放值。</summary>
        public float ScaleTimingPatternScore { get; }

        /// <summary>
        /// Determines whether two values are equal.
        /// 判断两个值是否相等。
        /// </summary>
        public static bool operator ==(QRCodeDetectorArucoParams left, QRCodeDetectorArucoParams right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two values are not equal.
        /// 判断两个值是否不相等。
        /// </summary>
        public static bool operator !=(QRCodeDetectorArucoParams left, QRCodeDetectorArucoParams right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Gets default OpenCV ArUco QR detector parameters.
        /// 获取 OpenCV 默认 ArUco 二维码检测参数。
        /// </summary>
        public static QRCodeDetectorArucoParams Default
        {
            get
            {
                NativeException.ThrowIfError(NativeMethods.QRCodeDetectorArucoDefaultParams(out NativeMethods.QRCodeDetectorArucoParamsNative native));
                return FromNative(native);
            }
        }

        internal NativeMethods.QRCodeDetectorArucoParamsNative ToNative()
        {
            return new NativeMethods.QRCodeDetectorArucoParamsNative
            {
                MinModuleSizeInPyramid = MinModuleSizeInPyramid,
                MaxRotation = MaxRotation,
                MaxModuleSizeMismatch = MaxModuleSizeMismatch,
                MaxTimingPatternMismatch = MaxTimingPatternMismatch,
                MaxPenalties = MaxPenalties,
                MaxColorsMismatch = MaxColorsMismatch,
                ScaleTimingPatternScore = ScaleTimingPatternScore
            };
        }

        internal static QRCodeDetectorArucoParams FromNative(NativeMethods.QRCodeDetectorArucoParamsNative native)
        {
            return new QRCodeDetectorArucoParams(
                native.MinModuleSizeInPyramid,
                native.MaxRotation,
                native.MaxModuleSizeMismatch,
                native.MaxTimingPatternMismatch,
                native.MaxPenalties,
                native.MaxColorsMismatch,
                native.ScaleTimingPatternScore);
        }

        /// <inheritdoc/>
        public bool Equals(QRCodeDetectorArucoParams other)
        {
            return MinModuleSizeInPyramid.Equals(other.MinModuleSizeInPyramid) &&
                MaxRotation.Equals(other.MaxRotation) &&
                MaxModuleSizeMismatch.Equals(other.MaxModuleSizeMismatch) &&
                MaxTimingPatternMismatch.Equals(other.MaxTimingPatternMismatch) &&
                MaxPenalties.Equals(other.MaxPenalties) &&
                MaxColorsMismatch.Equals(other.MaxColorsMismatch) &&
                ScaleTimingPatternScore.Equals(other.ScaleTimingPatternScore);
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is QRCodeDetectorArucoParams other && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hash = MinModuleSizeInPyramid.GetHashCode();
            hash = (hash * 397) ^ MaxRotation.GetHashCode();
            hash = (hash * 397) ^ MaxModuleSizeMismatch.GetHashCode();
            hash = (hash * 397) ^ MaxTimingPatternMismatch.GetHashCode();
            hash = (hash * 397) ^ MaxPenalties.GetHashCode();
            hash = (hash * 397) ^ MaxColorsMismatch.GetHashCode();
            hash = (hash * 397) ^ ScaleTimingPatternScore.GetHashCode();
            return hash;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{{MinModuleSizeInPyramid={0},MaxRotation={1},MaxModuleSizeMismatch={2},MaxTimingPatternMismatch={3},MaxPenalties={4},MaxColorsMismatch={5},ScaleTimingPatternScore={6}}}",
                MinModuleSizeInPyramid,
                MaxRotation,
                MaxModuleSizeMismatch,
                MaxTimingPatternMismatch,
                MaxPenalties,
                MaxColorsMismatch,
                ScaleTimingPatternScore);
        }
    }
}
