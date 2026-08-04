using System;
using JYPPX.OpenCvSharp.Core;

namespace JYPPX.OpenCvSharp.Calib3D
{
    /// <summary>
    /// Result returned by the owned <see cref="Cv2.StereoRectify(Mat, Mat, Mat, Mat, Size, Mat, Mat, StereoRectifyFlags, double, Size)"/> overload.
    /// owned <see cref="Cv2.StereoRectify(Mat, Mat, Mat, Mat, Size, Mat, Mat, StereoRectifyFlags, double, Size)"/> 重载返回的结果。
    /// </summary>
    public readonly struct StereoRectifyResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StereoRectifyResult"/> struct.
        /// 初始化 <see cref="StereoRectifyResult"/> 结构的新实例。
        /// </summary>
        public StereoRectifyResult(Mat r1, Mat r2, Mat p1, Mat p2, Mat q, Rect validPixROI1, Rect validPixROI2)
        {
            R1 = r1 ?? throw new ArgumentNullException(nameof(r1));
            R2 = r2 ?? throw new ArgumentNullException(nameof(r2));
            P1 = p1 ?? throw new ArgumentNullException(nameof(p1));
            P2 = p2 ?? throw new ArgumentNullException(nameof(p2));
            Q = q ?? throw new ArgumentNullException(nameof(q));
            ValidPixROI1 = validPixROI1;
            ValidPixROI2 = validPixROI2;
        }

        /// <summary>Gets the first rectification transform. 获取第一个校正变换。</summary>
        public Mat R1 { get; }

        /// <summary>Gets the second rectification transform. 获取第二个校正变换。</summary>
        public Mat R2 { get; }

        /// <summary>Gets the first projection matrix. 获取第一个投影矩阵。</summary>
        public Mat P1 { get; }

        /// <summary>Gets the second projection matrix. 获取第二个投影矩阵。</summary>
        public Mat P2 { get; }

        /// <summary>Gets the disparity-to-depth mapping matrix. 获取视差到深度映射矩阵。</summary>
        public Mat Q { get; }

        /// <summary>Gets the first valid-pixel ROI. 获取第一个有效像素 ROI。</summary>
        public Rect ValidPixROI1 { get; }

        /// <summary>Gets the second valid-pixel ROI. 获取第二个有效像素 ROI。</summary>
        public Rect ValidPixROI2 { get; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "{R1=" + R1.Rows + "x" + R1.Cols
                + ",R2=" + R2.Rows + "x" + R2.Cols
                + ",P1=" + P1.Rows + "x" + P1.Cols
                + ",P2=" + P2.Rows + "x" + P2.Cols
                + ",Q=" + Q.Rows + "x" + Q.Cols
                + ",ValidPixROI1=" + ValidPixROI1
                + ",ValidPixROI2=" + ValidPixROI2
                + "}";
        }
    }
}
