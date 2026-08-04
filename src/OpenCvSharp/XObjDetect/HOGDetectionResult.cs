using System;
using JYPPX.OpenCvSharp.Core;

namespace JYPPX.OpenCvSharp.XObjDetect
{
    /// <summary>
    /// Result for HOG detections.
    /// HOG 检测结果。
    /// </summary>
    public sealed class HOGDetectionResult
    {
        /// <summary>Initializes a HOG detection result from points. 使用点结果初始化 HOG 检测结果。</summary>
        public HOGDetectionResult(Point[] locations, double[] weights)
        {
            Point[] normalizedLocations = Clone(locations);
            double[] normalizedWeights = Clone(weights);
            ValidateWeightCount(normalizedWeights, normalizedLocations.Length, nameof(weights));

            this.locations = normalizedLocations;
            rectangles = Array.Empty<Rect>();
            this.weights = normalizedWeights;
        }

        /// <summary>Initializes a HOG detection result from rectangles. 使用矩形结果初始化 HOG 检测结果。</summary>
        public HOGDetectionResult(Rect[] rectangles, double[] weights)
        {
            locations = Array.Empty<Point>();
            Rect[] normalizedRectangles = Clone(rectangles);
            double[] normalizedWeights = Clone(weights);
            ValidateWeightCount(normalizedWeights, normalizedRectangles.Length, nameof(weights));

            this.rectangles = normalizedRectangles;
            this.weights = normalizedWeights;
        }

        private readonly Point[] locations;
        private readonly Rect[] rectangles;
        private readonly double[] weights;

        /// <summary>Gets single-scale detection points. 获取单尺度检测点。</summary>
        public Point[] Locations
        {
            get { return Clone(locations); }
        }

        /// <summary>Gets the number of single-scale detection points. 获取单尺度检测点数量。</summary>
        public int LocationCount
        {
            get { return locations.Length; }
        }

        /// <summary>Gets multi-scale detection rectangles. 获取多尺度检测矩形。</summary>
        public Rect[] Rectangles
        {
            get { return Clone(rectangles); }
        }

        /// <summary>Gets the number of multi-scale detection rectangles. 获取多尺度检测矩形数量。</summary>
        public int RectangleCount
        {
            get { return rectangles.Length; }
        }

        /// <summary>Gets detection confidence weights. 获取检测置信权重。</summary>
        public double[] Weights
        {
            get { return Clone(weights); }
        }

        /// <summary>Gets the number of detection confidence weights. 获取检测置信权重数量。</summary>
        public int WeightCount
        {
            get { return weights.Length; }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return
                $"{nameof(HOGDetectionResult)}(" +
                $"{nameof(Locations)}={LocationCount}, " +
                $"{nameof(Rectangles)}={RectangleCount}, " +
                $"{nameof(Weights)}={WeightCount})";
        }

        private static Point[] Clone(Point[] values)
        {
            if (values == null || values.Length == 0)
            {
                return Array.Empty<Point>();
            }

            var clone = new Point[values.Length];
            Array.Copy(values, clone, clone.Length);
            return clone;
        }

        private static Rect[] Clone(Rect[] values)
        {
            if (values == null || values.Length == 0)
            {
                return Array.Empty<Rect>();
            }

            var clone = new Rect[values.Length];
            Array.Copy(values, clone, clone.Length);
            return clone;
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

        private static void ValidateWeightCount(double[] weights, int resultCount, string parameterName)
        {
            if (weights.Length != 0 && weights.Length != resultCount)
            {
                throw new ArgumentException("Weight count must be zero or match the detection count.", parameterName);
            }
        }
    }
}
