using System;
using OpenCvSharp.Core;

namespace OpenCvSharp.ObjDetect
{
    /// <summary>
    /// Managed result returned by ArUco marker refinement.
    /// ArUco marker 细化返回的托管结果。
    /// </summary>
    public sealed class ArucoRefineResult
    {
        /// <summary>
        /// Initializes an ArUco refinement result.
        /// 初始化 ArUco 细化结果。
        /// </summary>
        public ArucoRefineResult(Point2f[][] corners, int[] ids, Point2f[][] rejectedCandidates, int[] recoveredIndices)
        {
            Point2f[][] normalizedCorners = Clone(corners);
            int[] normalizedIds = Clone(ids);
            Point2f[][] normalizedRejectedCandidates = Clone(rejectedCandidates);
            int[] normalizedRecoveredIndices = Clone(recoveredIndices);
            ValidateCornerCount(normalizedCorners, normalizedIds.Length);

            this.corners = normalizedCorners;
            this.ids = normalizedIds;
            this.rejectedCandidates = normalizedRejectedCandidates;
            this.recoveredIndices = normalizedRecoveredIndices;
        }

        private readonly Point2f[][] corners;
        private readonly int[] ids;
        private readonly Point2f[][] rejectedCandidates;
        private readonly int[] recoveredIndices;

        /// <summary>Gets refined marker corners. 获取细化后的 marker 角点。</summary>
        public Point2f[][] Corners
        {
            get { return Clone(corners); }
        }

        /// <summary>Gets refined marker ids. 获取细化后的 marker id。</summary>
        public int[] Ids
        {
            get { return Clone(ids); }
        }

        /// <summary>Gets remaining rejected candidates. 获取剩余被拒绝候选。</summary>
        public Point2f[][] RejectedCandidates
        {
            get { return Clone(rejectedCandidates); }
        }

        /// <summary>Gets indices recovered from the original rejected candidates. 获取从原始 rejected candidates 中恢复的索引。</summary>
        public int[] RecoveredIndices
        {
            get { return Clone(recoveredIndices); }
        }

        /// <summary>Gets the number of refined markers. 获取细化后的 marker 数量。</summary>
        public int Count
        {
            get { return ids.Length; }
        }

        /// <summary>Gets the number of refined marker corner groups. 获取细化后的 marker 角点分组数量。</summary>
        public int CornerCount
        {
            get { return corners.Length; }
        }

        /// <summary>Gets the number of refined marker identifiers. 获取细化后的 marker id 数量。</summary>
        public int IdCount
        {
            get { return ids.Length; }
        }

        /// <summary>Gets the number of remaining rejected candidates. 获取剩余被拒绝候选数量。</summary>
        public int RejectedCandidateCount
        {
            get { return rejectedCandidates.Length; }
        }

        /// <summary>Gets the number of recovered candidate indices. 获取恢复候选索引数量。</summary>
        public int RecoveredIndexCount
        {
            get { return recoveredIndices.Length; }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return
                $"{nameof(ArucoRefineResult)}(" +
                $"{nameof(Count)}={Count}, " +
                $"{nameof(Corners)}={CornerCount}, " +
                $"{nameof(Ids)}={IdCount}, " +
                $"{nameof(RejectedCandidates)}={RejectedCandidateCount}, " +
                $"{nameof(RecoveredIndices)}={RecoveredIndexCount})";
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

        private static void ValidateCornerCount(Point2f[][] corners, int markerCount)
        {
            if (corners.Length != markerCount)
            {
                throw new ArgumentException("Corner group count must match the marker id count.", nameof(corners));
            }
        }
    }
}
