using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Quality
{
    /// <summary>
    /// Gradient magnitude similarity deviation quality metric.
    /// 梯度幅值相似性偏差图像质量指标。
    /// </summary>
    public sealed class QualityGMSD : QualityBase
    {
        /// <summary>Creates a GMSD metric from a reference image. 使用参考图像创建 GMSD 指标。</summary>
        public QualityGMSD(Mat reference)
            : base(CreateHandle(reference))
        {
        }

        /// <summary>Creates a GMSD metric from a reference image. 使用参考图像创建 GMSD 指标。</summary>
        public static QualityGMSD Create(Mat reference)
        {
            return new QualityGMSD(reference);
        }

        /// <summary>
        /// Computes GMSD between two images.
        /// 计算两幅图像之间的 GMSD。
        /// </summary>
        public static Scalar Compute(Mat reference, Mat comparison, Mat? qualityMap = null)
        {
            ValidateNotNull(reference, nameof(reference));
            ValidateNotNull(comparison, nameof(comparison));
            ValidateInputImage(reference, nameof(reference));
            ValidateInputImage(comparison, nameof(comparison));
            var values = new double[4];
            NativeException.ThrowIfError(NativeMethods.QualityGMSDComputeStatic(
                reference.NativeHandle,
                comparison.NativeHandle,
                qualityMap == null ? IntPtr.Zero : qualityMap.NativeHandle,
                values,
                values.Length));
            return ToScalar(values);
        }

        private static NativeQualityHandle CreateHandle(Mat reference)
        {
            ValidateNotNull(reference, nameof(reference));
            ValidateInputImage(reference, nameof(reference));
            NativeException.ThrowIfError(NativeMethods.QualityGMSDCreate(reference.NativeHandle, out IntPtr nativeHandle));
            return NativeQualityHandle.FromNativePointer(nativeHandle);
        }

        /// <inheritdoc />
        protected override void ValidateComputeInput(Mat comparison, string parameterName)
        {
            ValidateInputImage(comparison, parameterName);
        }

        private static void ValidateInputImage(Mat image, string parameterName)
        {
            if (image.Empty)
            {
                throw new ArgumentException("GMSD input image must be non-empty.", parameterName);
            }
        }
    }
}
