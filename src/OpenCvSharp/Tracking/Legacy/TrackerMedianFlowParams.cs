using System;
using System.Globalization;
using System.Runtime.InteropServices;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Tracking.Legacy
{
    /// <summary>
    /// Parameters for OpenCV legacy MedianFlow tracker.
    /// OpenCV legacy MedianFlow 跟踪器参数。
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct TrackerMedianFlowParams : IEquatable<TrackerMedianFlowParams>
    {
        /// <summary>Initializes MedianFlow parameters. 初始化 MedianFlow 参数。</summary>
        public TrackerMedianFlowParams(
            int pointsInGrid,
            Size winSize,
            int maxLevel,
            TermCriteria termCriteria,
            Size winSizeNcc,
            double maxMedianLengthOfDisplacementDifference)
        {
            PointsInGrid = pointsInGrid;
            WinSize = winSize;
            MaxLevel = maxLevel;
            TermCriteria = termCriteria;
            WinSizeNcc = winSizeNcc;
            MaxMedianLengthOfDisplacementDifference = maxMedianLengthOfDisplacementDifference;
        }

        /// <summary>Gets the grid point count root. 获取关键点网格边长。</summary>
        public int PointsInGrid { get; }

        /// <summary>Gets Lucas-Kanade window size. 获取 Lucas-Kanade 窗口尺寸。</summary>
        public Size WinSize { get; }

        /// <summary>Gets maximal pyramid level. 获取最大金字塔层数。</summary>
        public int MaxLevel { get; }

        /// <summary>Gets termination criteria. 获取终止条件。</summary>
        public TermCriteria TermCriteria { get; }

        /// <summary>Gets NCC window size. 获取 NCC 窗口尺寸。</summary>
        public Size WinSizeNcc { get; }

        /// <summary>Gets object-loss criterion. 获取目标丢失判据。</summary>
        public double MaxMedianLengthOfDisplacementDifference { get; }

        /// <summary>Determines whether two parameter values are equal. 判断两个参数值是否相等。</summary>
        public static bool operator ==(TrackerMedianFlowParams left, TrackerMedianFlowParams right)
        {
            return left.Equals(right);
        }

        /// <summary>Determines whether two parameter values are different. 判断两个参数值是否不同。</summary>
        public static bool operator !=(TrackerMedianFlowParams left, TrackerMedianFlowParams right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Gets OpenCV 5.0.0 default MedianFlow parameters without calling native code.
        /// 获取 OpenCV 5.0.0 MedianFlow 默认参数，不调用 native 代码。
        /// </summary>
        public static TrackerMedianFlowParams Default
        {
            get
            {
                return new TrackerMedianFlowParams(
                    10,
                    new Size(3, 3),
                    5,
                    TermCriteria.ByCountAndEpsilon(20, 0.3),
                    new Size(30, 30),
                    10.0);
            }
        }

        /// <summary>
        /// Gets MedianFlow defaults from the linked native OpenCV runtime.
        /// 从已链接 native OpenCV runtime 读取 MedianFlow 默认参数。
        /// </summary>
        public static TrackerMedianFlowParams GetDefaultFromNative()
        {
            NativeException.ThrowIfError(NativeMethods.TrackingLegacyTrackerMedianFlowGetDefaultParams(out NativeMethods.TrackingMedianFlowParamsNative native));
            return FromNative(native);
        }

        internal NativeMethods.TrackingMedianFlowParamsNative ToNative()
        {
            return new NativeMethods.TrackingMedianFlowParamsNative
            {
                PointsInGrid = PointsInGrid,
                WinWidth = WinSize.Width,
                WinHeight = WinSize.Height,
                MaxLevel = MaxLevel,
                CriteriaType = (int)TermCriteria.Type,
                CriteriaMaxCount = TermCriteria.MaxCount,
                CriteriaEpsilon = TermCriteria.Epsilon,
                WinWidthNcc = WinSizeNcc.Width,
                WinHeightNcc = WinSizeNcc.Height,
                MaxMedianLengthOfDisplacementDifference = MaxMedianLengthOfDisplacementDifference
            };
        }

        internal static TrackerMedianFlowParams FromNative(NativeMethods.TrackingMedianFlowParamsNative native)
        {
            return new TrackerMedianFlowParams(
                native.PointsInGrid,
                new Size(native.WinWidth, native.WinHeight),
                native.MaxLevel,
                new TermCriteria((TermCriteriaTypes)native.CriteriaType, native.CriteriaMaxCount, native.CriteriaEpsilon),
                new Size(native.WinWidthNcc, native.WinHeightNcc),
                native.MaxMedianLengthOfDisplacementDifference);
        }

        /// <summary>Indicates whether this value equals another value. 指示此值是否与另一个值相等。</summary>
        public bool Equals(TrackerMedianFlowParams other)
        {
            return PointsInGrid == other.PointsInGrid
                && WinSize.Equals(other.WinSize)
                && MaxLevel == other.MaxLevel
                && TermCriteria.Equals(other.TermCriteria)
                && WinSizeNcc.Equals(other.WinSizeNcc)
                && MaxMedianLengthOfDisplacementDifference.Equals(other.MaxMedianLengthOfDisplacementDifference);
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is TrackerMedianFlowParams other && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = PointsInGrid;
                hash = (hash * 397) ^ WinSize.GetHashCode();
                hash = (hash * 397) ^ MaxLevel;
                hash = (hash * 397) ^ TermCriteria.GetHashCode();
                hash = (hash * 397) ^ WinSizeNcc.GetHashCode();
                hash = (hash * 397) ^ MaxMedianLengthOfDisplacementDifference.GetHashCode();
                return hash;
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "{PointsInGrid=" + PointsInGrid
                + ",WinSize=" + WinSize
                + ",MaxLevel=" + MaxLevel
                + ",TermCriteria=" + TermCriteria
                + ",WinSizeNcc=" + WinSizeNcc
                + ",MaxMedianLengthOfDisplacementDifference=" + MaxMedianLengthOfDisplacementDifference.ToString(CultureInfo.InvariantCulture) + "}";
        }
    }
}
