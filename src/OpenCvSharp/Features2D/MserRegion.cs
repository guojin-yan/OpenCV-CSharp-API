using System;
using JYPPX.OpenCvSharp.Core;

namespace JYPPX.OpenCvSharp.Features2D
{
    /// <summary>
    /// Represents one MSER region and its bounding box.
    /// 表示一个 MSER 区域及其边界框。
    /// </summary>
    public sealed class MserRegion
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MserRegion"/> class.
        /// 初始化 <see cref="MserRegion"/> 类的新实例。
        /// </summary>
        /// <param name="points">The region points. 区域点集合。</param>
        /// <param name="boundingBox">The region bounding box. 区域边界框。</param>
        public MserRegion(Point[] points, Rect boundingBox)
        {
            if (points == null)
            {
                throw new ArgumentNullException(nameof(points));
            }

            this.points = Clone(points);
            BoundingBox = boundingBox;
        }

        private readonly Point[] points;

        /// <summary>
        /// Gets the region points.
        /// 获取区域点集合。
        /// </summary>
        public Point[] Points
        {
            get { return Clone(points); }
        }

        /// <summary>
        /// Gets the region bounding box.
        /// 获取区域边界框。
        /// </summary>
        public Rect BoundingBox { get; }

        /// <summary>
        /// Gets the number of points in this region.
        /// 获取此区域中的点数量。
        /// </summary>
        public int PointCount
        {
            get { return points.Length; }
        }

        /// <summary>
        /// Gets whether this region contains any points.
        /// 获取此区域是否包含任何点。
        /// </summary>
        public bool HasPoints
        {
            get { return PointCount > 0; }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "{PointCount=" + PointCount + ",BoundingBox=" + BoundingBox + "}";
        }

        private static Point[] Clone(Point[] values)
        {
            var clone = new Point[values.Length];
            Array.Copy(values, clone, clone.Length);
            return clone;
        }
    }
}
