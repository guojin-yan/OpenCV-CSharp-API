using System;
using OpenCvSharp.Core;

namespace OpenCvSharp.ObjDetect
{
    /// <summary>
    /// Managed ArUco marker detection result.
    /// 托管 ArUco marker 检测结果。
    /// </summary>
    public sealed class ArucoDetectionResult
    {
        /// <summary>
        /// Initializes a detection result.
        /// 初始化检测结果。
        /// </summary>
        public ArucoDetectionResult(Point2f[][] corners, int[] ids, Point2f[][] rejectedCandidates, float[]? confidence = null)
        {
            Point2f[][] normalizedCorners = Clone(corners);
            int[] normalizedIds = Clone(ids);
            Point2f[][] normalizedRejectedCandidates = Clone(rejectedCandidates);
            float[] normalizedConfidence = Clone(confidence);
            ValidateMarkerCornerCount(normalizedCorners, normalizedIds.Length);
            ValidateOptionalMarkerMetadataCount(normalizedConfidence, normalizedIds.Length, nameof(confidence), "Confidence count");

            this.corners = normalizedCorners;
            this.ids = normalizedIds;
            this.rejectedCandidates = normalizedRejectedCandidates;
            this.confidence = normalizedConfidence;
        }

        private readonly Point2f[][] corners;
        private readonly int[] ids;
        private readonly Point2f[][] rejectedCandidates;
        private readonly float[] confidence;

        /// <summary>Gets marker corner groups. 获取 marker 角点分组。</summary>
        public Point2f[][] Corners
        {
            get { return Clone(corners); }
        }

        /// <summary>Gets marker identifiers. 获取 marker 标识符。</summary>
        public int[] Ids
        {
            get { return Clone(ids); }
        }

        /// <summary>Gets rejected marker candidates. 获取被拒绝的 marker 候选。</summary>
        public Point2f[][] RejectedCandidates
        {
            get { return Clone(rejectedCandidates); }
        }

        /// <summary>Gets marker confidence values when requested. 获取请求置信度检测时的 marker 置信度。</summary>
        public float[] Confidence
        {
            get { return Clone(confidence); }
        }

        /// <summary>Gets the number of detected markers. 获取检测到的 marker 数量。</summary>
        public int Count
        {
            get { return ids.Length; }
        }

        /// <summary>Gets the number of marker corner groups. 获取 marker 角点分组数量。</summary>
        public int CornerCount
        {
            get { return corners.Length; }
        }

        /// <summary>Gets the number of marker identifiers. 获取 marker 标识符数量。</summary>
        public int IdCount
        {
            get { return ids.Length; }
        }

        /// <summary>Gets the number of rejected marker candidates. 获取被拒绝 marker 候选数量。</summary>
        public int RejectedCandidateCount
        {
            get { return rejectedCandidates.Length; }
        }

        /// <summary>Gets the number of marker confidence values. 获取 marker 置信度数量。</summary>
        public int ConfidenceCount
        {
            get { return confidence.Length; }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return
                $"{nameof(ArucoDetectionResult)}(" +
                $"{nameof(Count)}={Count}, " +
                $"{nameof(Corners)}={CornerCount}, " +
                $"{nameof(Ids)}={IdCount}, " +
                $"{nameof(RejectedCandidates)}={RejectedCandidateCount}, " +
                $"{nameof(Confidence)}={ConfidenceCount})";
        }

        private static Point2f[][] Clone(Point2f[][] values)
        {
            if (values == null || values.Length == 0)
            {
                return Array.Empty<Point2f[]>();
            }

            var clone = new Point2f[values.Length][];
            for (int i = 0; i < values.Length; i++)
            {
                Point2f[] group = values[i];
                if (group == null || group.Length == 0)
                {
                    clone[i] = Array.Empty<Point2f>();
                    continue;
                }

                clone[i] = new Point2f[group.Length];
                Array.Copy(group, clone[i], clone[i].Length);
            }

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

        private static float[] Clone(float[]? values)
        {
            if (values == null || values.Length == 0)
            {
                return Array.Empty<float>();
            }

            var clone = new float[values.Length];
            Array.Copy(values, clone, clone.Length);
            return clone;
        }

        private static void ValidateMarkerCornerCount(Point2f[][] corners, int markerCount)
        {
            if (corners.Length != markerCount)
            {
                throw new ArgumentException("Corner group count must match the marker id count.", nameof(corners));
            }
        }

        private static void ValidateOptionalMarkerMetadataCount(Array values, int markerCount, string parameterName, string label)
        {
            if (values.Length != 0 && values.Length != markerCount)
            {
                throw new ArgumentException(label + " must be zero or match the marker id count.", parameterName);
            }
        }
    }
}
