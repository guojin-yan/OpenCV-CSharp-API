using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Face
{
    /// <summary>
    /// Local Binary Patterns Histograms face recognizer.
    /// 局部二值模式直方图人脸识别器。
    /// </summary>
    public sealed class LBPHFaceRecognizer : FaceRecognizer
    {
        private LBPHFaceRecognizer(IntPtr nativeHandle)
            : base(nativeHandle)
        {
        }

        /// <summary>Gets or sets LBP radius. 获取或设置 LBP 半径。</summary>
        public int Radius
        {
            get { NativeException.ThrowIfError(NativeMethods.FaceLBPHGetRadius(NativeHandle, out int value)); return value; }
            set { NativeException.ThrowIfError(NativeMethods.FaceLBPHSetRadius(NativeHandle, value)); }
        }

        /// <summary>Gets or sets neighbor count. 获取或设置邻域点数量。</summary>
        public int Neighbors
        {
            get { NativeException.ThrowIfError(NativeMethods.FaceLBPHGetNeighbors(NativeHandle, out int value)); return value; }
            set { NativeException.ThrowIfError(NativeMethods.FaceLBPHSetNeighbors(NativeHandle, value)); }
        }

        /// <summary>Gets or sets horizontal grid count. 获取或设置水平网格数量。</summary>
        public int GridX
        {
            get { NativeException.ThrowIfError(NativeMethods.FaceLBPHGetGridX(NativeHandle, out int value)); return value; }
            set { NativeException.ThrowIfError(NativeMethods.FaceLBPHSetGridX(NativeHandle, value)); }
        }

        /// <summary>Gets or sets vertical grid count. 获取或设置垂直网格数量。</summary>
        public int GridY
        {
            get { NativeException.ThrowIfError(NativeMethods.FaceLBPHGetGridY(NativeHandle, out int value)); return value; }
            set { NativeException.ThrowIfError(NativeMethods.FaceLBPHSetGridY(NativeHandle, value)); }
        }

        /// <summary>Creates an LBPH face recognizer. 创建 LBPH 人脸识别器。</summary>
        public static LBPHFaceRecognizer Create(int radius = 1, int neighbors = 8, int gridX = 8, int gridY = 8, double threshold = double.MaxValue)
        {
            NativeException.ThrowIfError(NativeMethods.FaceLBPHCreate(radius, neighbors, gridX, gridY, threshold, out IntPtr nativeHandle));
            return new LBPHFaceRecognizer(nativeHandle);
        }

        /// <summary>Gets trained labels as a matrix. 获取训练标签矩阵。</summary>
        public Mat GetLabels()
        {
            NativeException.ThrowIfError(NativeMethods.FaceLBPHGetLabels(NativeHandle, out IntPtr labels));
            return new Mat(labels);
        }

        /// <summary>Gets learned histograms. 获取已学习的直方图数组。</summary>
        public Mat[] GetHistograms()
        {
            NativeException.ThrowIfError(NativeMethods.FaceLBPHGetHistogramsCount(NativeHandle, out int count));
            if (count <= 0)
            {
                return Array.Empty<Mat>();
            }

            var histograms = new IntPtr[count];
            NativeException.ThrowIfError(NativeMethods.FaceLBPHGetHistogramsFill(NativeHandle, histograms, histograms.Length, out int written));
            return ToMatArray(histograms, written);
        }
    }
}
