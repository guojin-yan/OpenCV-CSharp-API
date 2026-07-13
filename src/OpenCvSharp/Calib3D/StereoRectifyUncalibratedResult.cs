using System;
using OpenCvSharp.Core;

namespace OpenCvSharp.Calib3D
{
    /// <summary>
    /// Result returned by the owned <see cref="Cv2.StereoRectifyUncalibrated(Mat, Mat, Mat, Size, double)"/> overload.
    /// owned <see cref="Cv2.StereoRectifyUncalibrated(Mat, Mat, Mat, Size, double)"/> 重载返回的结果。
    /// </summary>
    public readonly struct StereoRectifyUncalibratedResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StereoRectifyUncalibratedResult"/> struct.
        /// 初始化 <see cref="StereoRectifyUncalibratedResult"/> 结构的新实例。
        /// </summary>
        public StereoRectifyUncalibratedResult(bool success, Mat h1, Mat h2)
        {
            Success = success;
            H1 = h1 ?? throw new ArgumentNullException(nameof(h1));
            H2 = h2 ?? throw new ArgumentNullException(nameof(h2));
        }

        /// <summary>
        /// Gets a value indicating whether rectification succeeded.
        /// 获取校正是否成功。
        /// </summary>
        public bool Success { get; }

        /// <summary>
        /// Gets the first owned rectification homography.
        /// 获取第一个拥有所有权的校正单应矩阵。
        /// </summary>
        public Mat H1 { get; }

        /// <summary>
        /// Gets the second owned rectification homography.
        /// 获取第二个拥有所有权的校正单应矩阵。
        /// </summary>
        public Mat H2 { get; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "{Success=" + Success
                + ",H1=" + H1.Rows + "x" + H1.Cols
                + ",H2=" + H2.Rows + "x" + H2.Cols
                + "}";
        }
    }
}
