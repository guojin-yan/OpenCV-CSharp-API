using System;
using OpenCvSharp.Core;

namespace OpenCvSharp.XObjDetect
{
    /// <summary>
    /// Result for cascade classifier detections.
    /// 级联分类器检测结果。
    /// </summary>
    public sealed class CascadeDetectionResult
    {
        /// <summary>Initializes a cascade detection result. 初始化级联检测结果。</summary>
        public CascadeDetectionResult(Rect[] rectangles, int[] rejectLevels, double[] levelWeights)
        {
            Rect[] normalizedRectangles = Clone(rectangles);
            int[] normalizedRejectLevels = Clone(rejectLevels);
            double[] normalizedLevelWeights = Clone(levelWeights);
            ValidateMetadataCount(normalizedRejectLevels, normalizedRectangles.Length, nameof(rejectLevels), "Reject-level count");
            ValidateMetadataCount(normalizedLevelWeights, normalizedRectangles.Length, nameof(levelWeights), "Level-weight count");

            this.rectangles = normalizedRectangles;
            this.rejectLevels = normalizedRejectLevels;
            this.levelWeights = normalizedLevelWeights;
        }

        private readonly Rect[] rectangles;
        private readonly int[] rejectLevels;
        private readonly double[] levelWeights;

        /// <summary>Gets detected rectangles. 获取检测矩形。</summary>
        public Rect[] Rectangles
        {
            get { return Clone(rectangles); }
        }

        /// <summary>Gets the number of detected rectangles. 获取检测矩形数量。</summary>
        public int RectangleCount
        {
            get { return rectangles.Length; }
        }

        /// <summary>Gets reject levels or number-of-detection values depending on the API. 获取 reject levels 或检测次数。</summary>
        public int[] RejectLevels
        {
            get { return Clone(rejectLevels); }
        }

        /// <summary>Gets the number of reject-level entries. 获取 reject-level 条目数量。</summary>
        public int RejectLevelCount
        {
            get { return rejectLevels.Length; }
        }

        /// <summary>Gets final stage weights. 获取最终阶段权重。</summary>
        public double[] LevelWeights
        {
            get { return Clone(levelWeights); }
        }

        /// <summary>Gets the number of final stage weight entries. 获取最终阶段权重条目数量。</summary>
        public int LevelWeightCount
        {
            get { return levelWeights.Length; }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return
                $"{nameof(CascadeDetectionResult)}(" +
                $"{nameof(Rectangles)}={RectangleCount}, " +
                $"{nameof(RejectLevels)}={RejectLevelCount}, " +
                $"{nameof(LevelWeights)}={LevelWeightCount})";
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

        private static int[] Clone(int[] values)
        {
            if (values == null || values.Length == 0)
            {
                return Array.Empty<int>();
            }

            var clone = new int[values.Length];
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

        private static void ValidateMetadataCount(Array values, int rectangleCount, string parameterName, string label)
        {
            if (values.Length != 0 && values.Length != rectangleCount)
            {
                throw new ArgumentException(label + " must be zero or match the rectangle count.", parameterName);
            }
        }
    }
}
