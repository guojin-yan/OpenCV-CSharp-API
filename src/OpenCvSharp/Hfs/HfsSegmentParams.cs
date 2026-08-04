using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Hfs
{
    /// <summary>
    /// Parameters for OpenCV HFS image segmentation.
    /// OpenCV HFS 图像分割参数。
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct HfsSegmentParams : IEquatable<HfsSegmentParams>
    {
        /// <summary>
        /// Initializes a new HFS parameter set.
        /// 初始化一组 HFS 参数。
        /// </summary>
        public HfsSegmentParams(
            int height,
            int width,
            float segEgbThresholdI,
            int minRegionSizeI,
            float segEgbThresholdII,
            int minRegionSizeII,
            float spatialWeight,
            int slicSpixelSize,
            int numSlicIter)
        {
            Height = height;
            Width = width;
            SegEgbThresholdI = segEgbThresholdI;
            MinRegionSizeI = minRegionSizeI;
            SegEgbThresholdII = segEgbThresholdII;
            MinRegionSizeII = minRegionSizeII;
            SpatialWeight = spatialWeight;
            SlicSpixelSize = slicSpixelSize;
            NumSlicIter = numSlicIter;
        }

        /// <summary>Creates parameters with OpenCV default HFS algorithm values for the specified image size. 使用指定图像尺寸和 OpenCV 默认 HFS 算法值创建参数。</summary>
        public static HfsSegmentParams Default(int height, int width)
        {
            return new HfsSegmentParams(height, width, 0.08F, 100, 0.28F, 200, 0.6F, 8, 5);
        }

        /// <summary>Gets image height. 获取图像高度。</summary>
        public int Height { get; }

        /// <summary>Gets image width. 获取图像宽度。</summary>
        public int Width { get; }

        /// <summary>Gets first-stage EGB segmentation threshold. 获取第一阶段 EGB 分割阈值。</summary>
        public float SegEgbThresholdI { get; }

        /// <summary>Gets first-stage minimum region size. 获取第一阶段最小区域尺寸。</summary>
        public int MinRegionSizeI { get; }

        /// <summary>Gets second-stage EGB segmentation threshold. 获取第二阶段 EGB 分割阈值。</summary>
        public float SegEgbThresholdII { get; }

        /// <summary>Gets second-stage minimum region size. 获取第二阶段最小区域尺寸。</summary>
        public int MinRegionSizeII { get; }

        /// <summary>Gets SLIC spatial weight. 获取 SLIC 空间权重。</summary>
        public float SpatialWeight { get; }

        /// <summary>Gets SLIC superpixel size. 获取 SLIC 超像素尺寸。</summary>
        public int SlicSpixelSize { get; }

        /// <summary>Gets SLIC iteration count. 获取 SLIC 迭代次数。</summary>
        public int NumSlicIter { get; }

        /// <summary>Determines whether two parameter values are equal. 判断两个参数值是否相等。</summary>
        public static bool operator ==(HfsSegmentParams left, HfsSegmentParams right)
        {
            return left.Equals(right);
        }

        /// <summary>Determines whether two parameter values are different. 判断两个参数值是否不同。</summary>
        public static bool operator !=(HfsSegmentParams left, HfsSegmentParams right)
        {
            return !left.Equals(right);
        }

        /// <summary>Validates parameter ranges. 验证参数范围。</summary>
        public void Validate()
        {
            ValidatePositive(Height, nameof(Height));
            ValidatePositive(Width, nameof(Width));
            ValidatePositiveFinite(SegEgbThresholdI, nameof(SegEgbThresholdI));
            ValidatePositive(MinRegionSizeI, nameof(MinRegionSizeI));
            ValidatePositiveFinite(SegEgbThresholdII, nameof(SegEgbThresholdII));
            ValidatePositive(MinRegionSizeII, nameof(MinRegionSizeII));
            ValidatePositiveFinite(SpatialWeight, nameof(SpatialWeight));
            ValidatePositive(SlicSpixelSize, nameof(SlicSpixelSize));
            ValidatePositive(NumSlicIter, nameof(NumSlicIter));
        }

        private static void ValidatePositive(int value, string parameterName)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Value must be positive.");
            }
        }

        private static void ValidatePositiveFinite(float value, string parameterName)
        {
            if (value <= 0.0F || float.IsNaN(value) || float.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(parameterName, "Value must be a finite positive value.");
            }
        }

        /// <summary>Indicates whether this value equals another value. 指示此值是否与另一个值相等。</summary>
        public bool Equals(HfsSegmentParams other)
        {
            return Height == other.Height
                && Width == other.Width
                && SegEgbThresholdI.Equals(other.SegEgbThresholdI)
                && MinRegionSizeI == other.MinRegionSizeI
                && SegEgbThresholdII.Equals(other.SegEgbThresholdII)
                && MinRegionSizeII == other.MinRegionSizeII
                && SpatialWeight.Equals(other.SpatialWeight)
                && SlicSpixelSize == other.SlicSpixelSize
                && NumSlicIter == other.NumSlicIter;
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is HfsSegmentParams other && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = Height;
                hash = (hash * 397) ^ Width;
                hash = (hash * 397) ^ SegEgbThresholdI.GetHashCode();
                hash = (hash * 397) ^ MinRegionSizeI;
                hash = (hash * 397) ^ SegEgbThresholdII.GetHashCode();
                hash = (hash * 397) ^ MinRegionSizeII;
                hash = (hash * 397) ^ SpatialWeight.GetHashCode();
                hash = (hash * 397) ^ SlicSpixelSize;
                hash = (hash * 397) ^ NumSlicIter;
                return hash;
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{{Height={0},Width={1},SegEgbThresholdI={2},MinRegionSizeI={3},SegEgbThresholdII={4},MinRegionSizeII={5},SpatialWeight={6},SlicSpixelSize={7},NumSlicIter={8}}}",
                Height,
                Width,
                SegEgbThresholdI,
                MinRegionSizeI,
                SegEgbThresholdII,
                MinRegionSizeII,
                SpatialWeight,
                SlicSpixelSize,
                NumSlicIter);
        }
    }
}
