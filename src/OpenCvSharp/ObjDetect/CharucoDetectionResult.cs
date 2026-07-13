using System;
using OpenCvSharp.Core;

namespace OpenCvSharp.ObjDetect
{
    /// <summary>
    /// Managed ChArUco detection result.
    /// 托管 ChArUco 检测结果。
    /// </summary>
    public sealed class CharucoDetectionResult
    {
        /// <summary>
        /// Initializes a ChArUco detection result.
        /// 初始化 ChArUco 检测结果。
        /// </summary>
        public CharucoDetectionResult(Point2f[] charucoCorners, int[] charucoIds, Point2f[][] markerCorners, int[] markerIds)
        {
            Point2f[] normalizedCharucoCorners = Clone(charucoCorners);
            int[] normalizedCharucoIds = Clone(charucoIds);
            Point2f[][] normalizedMarkerCorners = Clone(markerCorners);
            int[] normalizedMarkerIds = Clone(markerIds);
            ValidateMetadataCount(normalizedCharucoCorners, normalizedCharucoIds.Length, nameof(charucoCorners), "ChArUco corner count");
            ValidateMetadataCount(normalizedMarkerCorners, normalizedMarkerIds.Length, nameof(markerCorners), "Marker corner group count");

            this.charucoCorners = normalizedCharucoCorners;
            this.charucoIds = normalizedCharucoIds;
            this.markerCorners = normalizedMarkerCorners;
            this.markerIds = normalizedMarkerIds;
        }

        private readonly Point2f[] charucoCorners;
        private readonly int[] charucoIds;
        private readonly Point2f[][] markerCorners;
        private readonly int[] markerIds;

        /// <summary>Gets interpolated ChArUco corners. 获取插值得到的 ChArUco 角点。</summary>
        public Point2f[] CharucoCorners
        {
            get { return Clone(charucoCorners); }
        }

        /// <summary>Gets interpolated ChArUco corner ids. 获取插值得到的 ChArUco 角点 id。</summary>
        public int[] CharucoIds
        {
            get { return Clone(charucoIds); }
        }

        /// <summary>Gets detected or supplied marker corners. 获取检测到或传入的 marker 角点。</summary>
        public Point2f[][] MarkerCorners
        {
            get { return Clone(markerCorners); }
        }

        /// <summary>Gets detected or supplied marker ids. 获取检测到或传入的 marker id。</summary>
        public int[] MarkerIds
        {
            get { return Clone(markerIds); }
        }

        /// <summary>Gets the number of interpolated ChArUco corners. 获取插值 ChArUco 角点数量。</summary>
        public int Count
        {
            get { return charucoIds.Length; }
        }

        /// <summary>Gets the number of interpolated ChArUco corners. 获取插值 ChArUco 角点数量。</summary>
        public int CharucoCornerCount
        {
            get { return charucoCorners.Length; }
        }

        /// <summary>Gets the number of interpolated ChArUco corner ids. 获取插值 ChArUco 角点 id 数量。</summary>
        public int CharucoIdCount
        {
            get { return charucoIds.Length; }
        }

        /// <summary>Gets the number of detected or supplied marker corner groups. 获取检测到或传入的 marker 角点分组数量。</summary>
        public int MarkerCornerCount
        {
            get { return markerCorners.Length; }
        }

        /// <summary>Gets the number of detected or supplied marker ids. 获取检测到或传入的 marker id 数量。</summary>
        public int MarkerIdCount
        {
            get { return markerIds.Length; }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return
                $"{nameof(CharucoDetectionResult)}(" +
                $"{nameof(Count)}={Count}, " +
                $"{nameof(CharucoCorners)}={CharucoCornerCount}, " +
                $"{nameof(CharucoIds)}={CharucoIdCount}, " +
                $"{nameof(MarkerCorners)}={MarkerCornerCount}, " +
                $"{nameof(MarkerIds)}={MarkerIdCount})";
        }

        private static Point2f[] Clone(Point2f[] values)
        {
            if (values == null || values.Length == 0)
            {
                return Array.Empty<Point2f>();
            }

            var clone = new Point2f[values.Length];
            Array.Copy(values, clone, clone.Length);
            return clone;
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

        private static void ValidateMetadataCount(Array values, int idCount, string parameterName, string label)
        {
            if (values.Length != idCount)
            {
                throw new ArgumentException(label + " must match the id count.", parameterName);
            }
        }
    }
}
