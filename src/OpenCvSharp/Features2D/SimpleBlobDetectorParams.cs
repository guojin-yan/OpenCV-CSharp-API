using System;
using System.Globalization;
using System.Runtime.InteropServices;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Features2D
{
    /// <summary>
    /// Provides parameters for <see cref="SimpleBlobDetector"/>.
    /// 提供 <see cref="SimpleBlobDetector"/> 使用的参数。
    /// </summary>
    public sealed class SimpleBlobDetectorParams
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleBlobDetectorParams"/> class with OpenCV default values.
        /// 使用 OpenCV 默认值初始化 <see cref="SimpleBlobDetectorParams"/> 类的新实例。
        /// </summary>
        public SimpleBlobDetectorParams()
        {
            ThresholdStep = 10.0F;
            MinThreshold = 50.0F;
            MaxThreshold = 220.0F;
            MinRepeatability = 2;
            MinDistBetweenBlobs = 10.0F;
            FilterByColor = true;
            BlobColor = 0;
            FilterByArea = true;
            MinArea = 25.0F;
            MaxArea = 5000.0F;
            FilterByCircularity = false;
            MinCircularity = 0.8F;
            MaxCircularity = float.MaxValue;
            FilterByInertia = true;
            MinInertiaRatio = 0.1F;
            MaxInertiaRatio = float.MaxValue;
            FilterByConvexity = true;
            MinConvexity = 0.95F;
            MaxConvexity = float.MaxValue;
            CollectContours = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleBlobDetectorParams"/> class with explicit values.
        /// 使用显式值初始化 <see cref="SimpleBlobDetectorParams"/> 类的新实例。
        /// </summary>
        /// <param name="thresholdStep">The threshold step. 阈值步长。</param>
        /// <param name="minThreshold">The minimum threshold. 最小阈值。</param>
        /// <param name="maxThreshold">The maximum threshold. 最大阈值。</param>
        /// <param name="minRepeatability">The minimum repeatability. 最小重复次数。</param>
        /// <param name="minDistBetweenBlobs">The minimum distance between blob centers. 斑点中心之间的最小距离。</param>
        /// <param name="filterByColor">Whether blob color filtering is enabled. 是否启用斑点颜色过滤。</param>
        /// <param name="blobColor">The blob color. 斑点颜色。</param>
        /// <param name="filterByArea">Whether area filtering is enabled. 是否启用面积过滤。</param>
        /// <param name="minArea">The minimum blob area. 最小斑点面积。</param>
        /// <param name="maxArea">The maximum blob area. 最大斑点面积。</param>
        /// <param name="filterByCircularity">Whether circularity filtering is enabled. 是否启用圆度过滤。</param>
        /// <param name="minCircularity">The minimum circularity. 最小圆度。</param>
        /// <param name="maxCircularity">The maximum circularity. 最大圆度。</param>
        /// <param name="filterByInertia">Whether inertia filtering is enabled. 是否启用惯性比过滤。</param>
        /// <param name="minInertiaRatio">The minimum inertia ratio. 最小惯性比。</param>
        /// <param name="maxInertiaRatio">The maximum inertia ratio. 最大惯性比。</param>
        /// <param name="filterByConvexity">Whether convexity filtering is enabled. 是否启用凸性过滤。</param>
        /// <param name="minConvexity">The minimum convexity. 最小凸性。</param>
        /// <param name="maxConvexity">The maximum convexity. 最大凸性。</param>
        /// <param name="collectContours">Whether OpenCV should collect blob contours. OpenCV 是否收集斑点轮廓。</param>
        public SimpleBlobDetectorParams(
            float thresholdStep,
            float minThreshold,
            float maxThreshold,
            int minRepeatability,
            float minDistBetweenBlobs,
            bool filterByColor,
            byte blobColor,
            bool filterByArea,
            float minArea,
            float maxArea,
            bool filterByCircularity,
            float minCircularity,
            float maxCircularity,
            bool filterByInertia,
            float minInertiaRatio,
            float maxInertiaRatio,
            bool filterByConvexity,
            float minConvexity,
            float maxConvexity,
            bool collectContours)
        {
            ThresholdStep = thresholdStep;
            MinThreshold = minThreshold;
            MaxThreshold = maxThreshold;
            MinRepeatability = minRepeatability;
            MinDistBetweenBlobs = minDistBetweenBlobs;
            FilterByColor = filterByColor;
            BlobColor = blobColor;
            FilterByArea = filterByArea;
            MinArea = minArea;
            MaxArea = maxArea;
            FilterByCircularity = filterByCircularity;
            MinCircularity = minCircularity;
            MaxCircularity = maxCircularity;
            FilterByInertia = filterByInertia;
            MinInertiaRatio = minInertiaRatio;
            MaxInertiaRatio = maxInertiaRatio;
            FilterByConvexity = filterByConvexity;
            MinConvexity = minConvexity;
            MaxConvexity = maxConvexity;
            CollectContours = collectContours;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleBlobDetectorParams"/> class by copying another instance.
        /// 通过复制另一个实例初始化 <see cref="SimpleBlobDetectorParams"/> 类的新实例。
        /// </summary>
        /// <param name="other">The parameters to copy. 要复制的参数。</param>
        public SimpleBlobDetectorParams(SimpleBlobDetectorParams other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            ThresholdStep = other.ThresholdStep;
            MinThreshold = other.MinThreshold;
            MaxThreshold = other.MaxThreshold;
            MinRepeatability = other.MinRepeatability;
            MinDistBetweenBlobs = other.MinDistBetweenBlobs;
            FilterByColor = other.FilterByColor;
            BlobColor = other.BlobColor;
            FilterByArea = other.FilterByArea;
            MinArea = other.MinArea;
            MaxArea = other.MaxArea;
            FilterByCircularity = other.FilterByCircularity;
            MinCircularity = other.MinCircularity;
            MaxCircularity = other.MaxCircularity;
            FilterByInertia = other.FilterByInertia;
            MinInertiaRatio = other.MinInertiaRatio;
            MaxInertiaRatio = other.MaxInertiaRatio;
            FilterByConvexity = other.FilterByConvexity;
            MinConvexity = other.MinConvexity;
            MaxConvexity = other.MaxConvexity;
            CollectContours = other.CollectContours;
        }

        /// <summary>
        /// Gets or sets the threshold step used between <see cref="MinThreshold"/> and <see cref="MaxThreshold"/>.
        /// 获取或设置在 <see cref="MinThreshold"/> 和 <see cref="MaxThreshold"/> 之间使用的阈值步长。
        /// </summary>
        public float ThresholdStep { get; set; }

        /// <summary>
        /// Gets or sets the minimum threshold.
        /// 获取或设置最小阈值。
        /// </summary>
        public float MinThreshold { get; set; }

        /// <summary>
        /// Gets or sets the maximum threshold.
        /// 获取或设置最大阈值。
        /// </summary>
        public float MaxThreshold { get; set; }

        /// <summary>
        /// Gets or sets the minimum repeatability across threshold levels.
        /// 获取或设置跨阈值层级的最小重复次数。
        /// </summary>
        public int MinRepeatability { get; set; }

        /// <summary>
        /// Gets or sets the minimum distance between blob centers.
        /// 获取或设置斑点中心之间的最小距离。
        /// </summary>
        public float MinDistBetweenBlobs { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether blob color filtering is enabled.
        /// 获取或设置是否启用斑点颜色过滤。
        /// </summary>
        public bool FilterByColor { get; set; }

        /// <summary>
        /// Gets or sets the blob color, usually 0 for dark blobs or 255 for bright blobs.
        /// 获取或设置斑点颜色，通常 0 表示暗斑点，255 表示亮斑点。
        /// </summary>
        public byte BlobColor { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether area filtering is enabled.
        /// 获取或设置是否启用面积过滤。
        /// </summary>
        public bool FilterByArea { get; set; }

        /// <summary>
        /// Gets or sets the minimum blob area.
        /// 获取或设置最小斑点面积。
        /// </summary>
        public float MinArea { get; set; }

        /// <summary>
        /// Gets or sets the maximum blob area.
        /// 获取或设置最大斑点面积。
        /// </summary>
        public float MaxArea { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether circularity filtering is enabled.
        /// 获取或设置是否启用圆度过滤。
        /// </summary>
        public bool FilterByCircularity { get; set; }

        /// <summary>
        /// Gets or sets the minimum blob circularity.
        /// 获取或设置最小斑点圆度。
        /// </summary>
        public float MinCircularity { get; set; }

        /// <summary>
        /// Gets or sets the maximum blob circularity.
        /// 获取或设置最大斑点圆度。
        /// </summary>
        public float MaxCircularity { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether inertia filtering is enabled.
        /// 获取或设置是否启用惯性比过滤。
        /// </summary>
        public bool FilterByInertia { get; set; }

        /// <summary>
        /// Gets or sets the minimum inertia ratio.
        /// 获取或设置最小惯性比。
        /// </summary>
        public float MinInertiaRatio { get; set; }

        /// <summary>
        /// Gets or sets the maximum inertia ratio.
        /// 获取或设置最大惯性比。
        /// </summary>
        public float MaxInertiaRatio { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether convexity filtering is enabled.
        /// 获取或设置是否启用凸性过滤。
        /// </summary>
        public bool FilterByConvexity { get; set; }

        /// <summary>
        /// Gets or sets the minimum blob convexity.
        /// 获取或设置最小斑点凸性。
        /// </summary>
        public float MinConvexity { get; set; }

        /// <summary>
        /// Gets or sets the maximum blob convexity.
        /// 获取或设置最大斑点凸性。
        /// </summary>
        public float MaxConvexity { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether OpenCV should collect blob contours.
        /// 获取或设置 OpenCV 是否收集斑点轮廓。
        /// </summary>
        public bool CollectContours { get; set; }

        /// <summary>
        /// Creates a copy of this parameter object.
        /// 创建此参数对象的副本。
        /// </summary>
        /// <returns>The copied parameters. 复制后的参数。</returns>
        public SimpleBlobDetectorParams Clone()
        {
            return new SimpleBlobDetectorParams(this);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "{ThresholdStep=" + ThresholdStep.ToString(CultureInfo.InvariantCulture)
                + ",MinThreshold=" + MinThreshold.ToString(CultureInfo.InvariantCulture)
                + ",MaxThreshold=" + MaxThreshold.ToString(CultureInfo.InvariantCulture)
                + ",MinArea=" + MinArea.ToString(CultureInfo.InvariantCulture)
                + ",MaxArea=" + MaxArea.ToString(CultureInfo.InvariantCulture)
                + ",BlobColor=" + BlobColor
                + "}";
        }

        internal NativeSimpleBlobParams ToNative()
        {
            return new NativeSimpleBlobParams
            {
                Size = Marshal.SizeOf(typeof(NativeSimpleBlobParams)),
                ThresholdStep = ThresholdStep,
                MinThreshold = MinThreshold,
                MaxThreshold = MaxThreshold,
                MinRepeatability = MinRepeatability,
                MinDistBetweenBlobs = MinDistBetweenBlobs,
                FilterByColor = FilterByColor ? 1 : 0,
                BlobColor = BlobColor,
                FilterByArea = FilterByArea ? 1 : 0,
                MinArea = MinArea,
                MaxArea = MaxArea,
                FilterByCircularity = FilterByCircularity ? 1 : 0,
                MinCircularity = MinCircularity,
                MaxCircularity = MaxCircularity,
                FilterByInertia = FilterByInertia ? 1 : 0,
                MinInertiaRatio = MinInertiaRatio,
                MaxInertiaRatio = MaxInertiaRatio,
                FilterByConvexity = FilterByConvexity ? 1 : 0,
                MinConvexity = MinConvexity,
                MaxConvexity = MaxConvexity,
                CollectContours = CollectContours ? 1 : 0
            };
        }

        internal static SimpleBlobDetectorParams FromNative(NativeSimpleBlobParams native)
        {
            return new SimpleBlobDetectorParams(
                native.ThresholdStep,
                native.MinThreshold,
                native.MaxThreshold,
                native.MinRepeatability,
                native.MinDistBetweenBlobs,
                native.FilterByColor != 0,
                (byte)native.BlobColor,
                native.FilterByArea != 0,
                native.MinArea,
                native.MaxArea,
                native.FilterByCircularity != 0,
                native.MinCircularity,
                native.MaxCircularity,
                native.FilterByInertia != 0,
                native.MinInertiaRatio,
                native.MaxInertiaRatio,
                native.FilterByConvexity != 0,
                native.MinConvexity,
                native.MaxConvexity,
                native.CollectContours != 0);
        }
    }
}
