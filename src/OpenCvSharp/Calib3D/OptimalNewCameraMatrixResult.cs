using System;
using JYPPX.OpenCvSharp.Core;

namespace JYPPX.OpenCvSharp.Calib3D
{
    /// <summary>
    /// Result returned by <see cref="Cv2.GetOptimalNewCameraMatrix"/>.
    /// <see cref="Cv2.GetOptimalNewCameraMatrix"/> 返回的结果。
    /// </summary>
    public readonly struct OptimalNewCameraMatrixResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OptimalNewCameraMatrixResult"/> struct.
        /// 初始化 <see cref="OptimalNewCameraMatrixResult"/> 结构的新实例。
        /// </summary>
        /// <param name="cameraMatrix">The computed camera matrix. 计算得到的相机矩阵。</param>
        /// <param name="validPixROI">The valid-pixel region of interest. 有效像素 ROI。</param>
        public OptimalNewCameraMatrixResult(Mat cameraMatrix, Rect validPixROI)
        {
            CameraMatrix = cameraMatrix ?? throw new ArgumentNullException(nameof(cameraMatrix));
            ValidPixROI = validPixROI;
        }

        /// <summary>
        /// Gets the computed camera matrix.
        /// 获取计算得到的相机矩阵。
        /// </summary>
        public Mat CameraMatrix { get; }

        /// <summary>
        /// Gets the number of rows in the computed camera matrix.
        /// 获取计算得到的相机矩阵行数。
        /// </summary>
        public int CameraMatrixRows
        {
            get { return CameraMatrix.Rows; }
        }

        /// <summary>
        /// Gets the number of columns in the computed camera matrix.
        /// 获取计算得到的相机矩阵列数。
        /// </summary>
        public int CameraMatrixCols
        {
            get { return CameraMatrix.Cols; }
        }

        /// <summary>
        /// Gets the valid-pixel region of interest.
        /// 获取有效像素 ROI。
        /// </summary>
        public Rect ValidPixROI { get; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "{CameraMatrix=" + CameraMatrixRows + "x" + CameraMatrixCols + ",ValidPixROI=" + ValidPixROI + "}";
        }
    }
}
