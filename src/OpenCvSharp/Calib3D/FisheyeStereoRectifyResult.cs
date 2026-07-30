using System;
using OpenCvSharp.Core;

namespace OpenCvSharp.Calib3D
{
    /// <summary>Owns all matrices returned by fisheye stereo rectification. 拥有鱼眼双目标定返回的全部矩阵。</summary>
    public readonly struct FisheyeStereoRectifyResult
    {
        /// <summary>Initializes the result. 初始化结果。</summary>
        public FisheyeStereoRectifyResult(Mat r1, Mat r2, Mat p1, Mat p2, Mat q)
        {
            R1 = r1 ?? throw new ArgumentNullException(nameof(r1));
            R2 = r2 ?? throw new ArgumentNullException(nameof(r2));
            P1 = p1 ?? throw new ArgumentNullException(nameof(p1));
            P2 = p2 ?? throw new ArgumentNullException(nameof(p2));
            Q = q ?? throw new ArgumentNullException(nameof(q));
        }

        /// <summary>Gets the first rectification transform. 获取第一个校正变换。</summary>
        public Mat R1 { get; }
        /// <summary>Gets the second rectification transform. 获取第二个校正变换。</summary>
        public Mat R2 { get; }
        /// <summary>Gets the first projection matrix. 获取第一个投影矩阵。</summary>
        public Mat P1 { get; }
        /// <summary>Gets the second projection matrix. 获取第二个投影矩阵。</summary>
        public Mat P2 { get; }
        /// <summary>Gets the disparity-to-depth matrix. 获取视差到深度矩阵。</summary>
        public Mat Q { get; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "{R1=" + R1.Rows + "x" + R1.Cols
                + ",R2=" + R2.Rows + "x" + R2.Cols
                + ",P1=" + P1.Rows + "x" + P1.Cols
                + ",P2=" + P2.Rows + "x" + P2.Cols
                + ",Q=" + Q.Rows + "x" + Q.Cols + "}";
        }
    }
}
