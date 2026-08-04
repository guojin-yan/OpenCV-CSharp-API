using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Face
{
    /// <summary>
    /// Base class for EigenFace and FisherFace recognizers.
    /// EigenFace 与 FisherFace 识别器基类。
    /// </summary>
    public class BasicFaceRecognizer : FaceRecognizer
    {
        internal BasicFaceRecognizer(IntPtr nativeHandle)
            : base(nativeHandle)
        {
        }

        /// <summary>Gets or sets the number of retained components. 获取或设置保留的分量数量。</summary>
        public int NumComponents
        {
            get
            {
                NativeException.ThrowIfError(NativeMethods.FaceBasicGetNumComponents(NativeHandle, out int value));
                return value;
            }
            set
            {
                NativeException.ThrowIfError(NativeMethods.FaceBasicSetNumComponents(NativeHandle, value));
            }
        }

        /// <summary>Gets trained labels as a matrix. 获取训练标签矩阵。</summary>
        public Mat GetLabels()
        {
            NativeException.ThrowIfError(NativeMethods.FaceBasicGetLabels(NativeHandle, out IntPtr labels));
            return new Mat(labels);
        }

        /// <summary>Gets eigen values as a matrix. 获取特征值矩阵。</summary>
        public Mat GetEigenValues()
        {
            NativeException.ThrowIfError(NativeMethods.FaceBasicGetEigenValues(NativeHandle, out IntPtr eigenValues));
            return new Mat(eigenValues);
        }

        /// <summary>Gets eigen vectors as a matrix. 获取特征向量矩阵。</summary>
        public Mat GetEigenVectors()
        {
            NativeException.ThrowIfError(NativeMethods.FaceBasicGetEigenVectors(NativeHandle, out IntPtr eigenVectors));
            return new Mat(eigenVectors);
        }

        /// <summary>Gets the mean face matrix. 获取平均人脸矩阵。</summary>
        public Mat GetMean()
        {
            NativeException.ThrowIfError(NativeMethods.FaceBasicGetMean(NativeHandle, out IntPtr mean));
            return new Mat(mean);
        }

        /// <summary>Gets projection matrices. 获取投影矩阵数组。</summary>
        public Mat[] GetProjections()
        {
            NativeException.ThrowIfError(NativeMethods.FaceBasicGetProjectionsCount(NativeHandle, out int count));
            if (count <= 0)
            {
                return Array.Empty<Mat>();
            }

            var projections = new IntPtr[count];
            NativeException.ThrowIfError(NativeMethods.FaceBasicGetProjectionsFill(NativeHandle, projections, projections.Length, out int written));
            return ToMatArray(projections, written);
        }
    }
}
