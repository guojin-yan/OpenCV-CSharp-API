using System;
using OpenCvSharp.Core;

namespace OpenCvSharp.Video
{
    /// <summary>
    /// Result returned by <see cref="Cv2.BuildOpticalFlowPyramid"/>.
    /// <see cref="Cv2.BuildOpticalFlowPyramid"/> 返回的结果。
    /// </summary>
    public sealed class OpticalFlowPyramidResult
    {
        /// <summary>
        /// Initializes a new result.
        /// 初始化结果对象。
        /// </summary>
        public OpticalFlowPyramidResult(int levelCount, Mat[] pyramid)
        {
            if (levelCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(levelCount));
            }

            LevelCount = levelCount;
            this.pyramid = Clone(pyramid);
        }

        private readonly Mat[] pyramid;

        /// <summary>
        /// Gets the number of pyramid levels constructed by OpenCV.
        /// 获取 OpenCV 构建出的金字塔层数。
        /// </summary>
        public int LevelCount { get; }

        /// <summary>
        /// Gets the generated pyramid matrices.
        /// 获取生成的金字塔矩阵。
        /// </summary>
        public Mat[] Pyramid
        {
            get { return Clone(pyramid); }
        }

        /// <summary>
        /// Gets the number of generated pyramid matrices.
        /// 获取生成的金字塔矩阵数量。
        /// </summary>
        public int PyramidCount
        {
            get { return pyramid.Length; }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return
                $"{nameof(OpticalFlowPyramidResult)}(" +
                $"{nameof(LevelCount)}={LevelCount}, " +
                $"{nameof(Pyramid)}={PyramidCount})";
        }

        private static Mat[] Clone(Mat[] values)
        {
            if (values == null || values.Length == 0)
            {
                return Array.Empty<Mat>();
            }

            var clone = new Mat[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] == null)
                {
                    throw new ArgumentNullException(nameof(values), "Pyramid matrices cannot contain null elements.");
                }

                clone[i] = values[i];
            }

            return clone;
        }
    }
}
