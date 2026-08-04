using System;
using JYPPX.OpenCvSharp.Core;

namespace JYPPX.OpenCvSharp.Face
{
    /// <summary>
    /// Result returned by a facemark fitting call.
    /// Facemark 拟合调用返回的结果。
    /// </summary>
    public sealed class FacemarkFitResult
    {
        /// <summary>Initializes a new result. 初始化结果。</summary>
        public FacemarkFitResult(bool success, Point2f[][] landmarks)
        {
            if (landmarks == null)
            {
                throw new ArgumentNullException(nameof(landmarks));
            }

            Success = success;
            this.landmarks = CloneGroups(landmarks);
            this.flattenedLandmarks = Flatten(this.landmarks);
        }

        private readonly Point2f[][] landmarks;
        private readonly Point2f[] flattenedLandmarks;

        /// <summary>Gets whether OpenCV reported a successful fit. 获取 OpenCV 是否报告拟合成功。</summary>
        public bool Success { get; }

        /// <summary>Gets grouped landmarks, one group per face. 获取分组关键点，每个人脸一个分组。</summary>
        public Point2f[][] Landmarks
        {
            get { return CloneGroups(landmarks); }
        }

        /// <summary>Gets all landmarks as one flattened array. 获取所有关键点的扁平数组。</summary>
        public Point2f[] FlattenedLandmarks
        {
            get { return Clone(flattenedLandmarks); }
        }

        /// <summary>Gets the number of face groups. 获取人脸分组数量。</summary>
        public int FaceCount
        {
            get { return landmarks.Length; }
        }

        /// <summary>Gets the total landmark point count. 获取关键点总数。</summary>
        public int LandmarkCount
        {
            get { return flattenedLandmarks.Length; }
        }

        /// <summary>Gets whether any landmark points are present. 获取是否包含任何关键点。</summary>
        public bool HasLandmarks
        {
            get { return LandmarkCount > 0; }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return
                $"{nameof(FacemarkFitResult)}(" +
                $"{nameof(Success)}={Success}, " +
                $"{nameof(FaceCount)}={FaceCount}, " +
                $"{nameof(LandmarkCount)}={LandmarkCount}, " +
                $"{nameof(HasLandmarks)}={HasLandmarks})";
        }

        private static Point2f[][] CloneGroups(Point2f[][] source)
        {
            var result = new Point2f[source.Length][];
            for (int i = 0; i < source.Length; i++)
            {
                Point2f[] group = source[i];
                if (group == null)
                {
                    throw new ArgumentException("Landmark group cannot be null.", nameof(source));
                }

                var clone = new Point2f[group.Length];
                Array.Copy(group, clone, clone.Length);
                result[i] = clone;
            }

            return result;
        }

        private static Point2f[] Clone(Point2f[] source)
        {
            var result = new Point2f[source.Length];
            Array.Copy(source, result, result.Length);
            return result;
        }

        private static Point2f[] Flatten(Point2f[][] groups)
        {
            int total = 0;
            for (int i = 0; i < groups.Length; i++)
            {
                total += groups[i].Length;
            }

            var result = new Point2f[total];
            int offset = 0;
            for (int i = 0; i < groups.Length; i++)
            {
                Array.Copy(groups[i], 0, result, offset, groups[i].Length);
                offset += groups[i].Length;
            }

            return result;
        }
    }
}
