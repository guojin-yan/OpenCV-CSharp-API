using System;
using System.Linq;

namespace OpenCvSharp.Dnn
{
    /// <summary>
    /// DNN performance profile returned by <see cref="Net.GetPerfProfile"/>.
    /// <see cref="Net.GetPerfProfile"/> 返回的 DNN 性能剖析结果。
    /// </summary>
    public readonly struct DnnPerfProfile : IEquatable<DnnPerfProfile>
    {
        /// <summary>
        /// Initializes a new profile result.
        /// 初始化新的剖析结果。
        /// </summary>
        public DnnPerfProfile(long tickCount, double[] layerTimings)
        {
            if (tickCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(tickCount));
            }

            this.layerTimings = Clone(layerTimings);
            TickCount = tickCount;
        }

        private readonly double[] layerTimings;

        /// <summary>
        /// Gets the total tick count reported by OpenCV.
        /// 获取 OpenCV 报告的总 tick 数。
        /// </summary>
        public long TickCount { get; }

        /// <summary>
        /// Gets per-layer timings in ticks.
        /// 获取每层耗时 tick。
        /// </summary>
        public double[] LayerTimings
        {
            get { return Clone(TimingsForRead); }
        }

        /// <summary>
        /// Gets the number of per-layer timing entries.
        /// 获取每层耗时条目数量。
        /// </summary>
        public int LayerCount
        {
            get { return TimingsForRead.Length; }
        }

        private double[] TimingsForRead
        {
            get { return layerTimings ?? Array.Empty<double>(); }
        }

        /// <summary>
        /// Determines whether two values are equal.
        /// 判断两个值是否相等。
        /// </summary>
        public static bool operator ==(DnnPerfProfile left, DnnPerfProfile right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two values are not equal.
        /// 判断两个值是否不相等。
        /// </summary>
        public static bool operator !=(DnnPerfProfile left, DnnPerfProfile right)
        {
            return !left.Equals(right);
        }

        /// <inheritdoc/>
        public bool Equals(DnnPerfProfile other)
        {
            return TickCount == other.TickCount &&
                TimingsForRead.SequenceEqual(other.TimingsForRead);
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is DnnPerfProfile other && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hash = TickCount.GetHashCode();
            foreach (double timing in TimingsForRead)
            {
                hash = (hash * 397) ^ timing.GetHashCode();
            }

            return hash;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "{TickCount=" + TickCount + ",LayerTimings=" + LayerCount + "}";
        }

        private static double[] Clone(double[] values)
        {
            if (values == null || values.Length == 0)
            {
                return Array.Empty<double>();
            }

            var clone = new double[values.Length];
            Array.Copy(values, clone, clone.Length);
            return clone;
        }
    }
}
