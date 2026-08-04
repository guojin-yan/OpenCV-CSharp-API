using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Quality
{
    /// <summary>
    /// Mean squared error quality metric.
    /// 均方误差图像质量指标。
    /// </summary>
    public sealed class QualityMSE : QualityBase
    {
        /// <summary>Creates an MSE metric from a reference image. 使用参考图像创建 MSE 指标。</summary>
        public QualityMSE(Mat reference)
            : base(CreateHandle(reference))
        {
        }

        /// <summary>Creates an MSE metric from a reference image. 使用参考图像创建 MSE 指标。</summary>
        public static QualityMSE Create(Mat reference)
        {
            return new QualityMSE(reference);
        }

        /// <summary>
        /// Computes MSE between two images.
        /// 计算两幅图像之间的 MSE。
        /// </summary>
        public static Scalar Compute(Mat reference, Mat comparison, Mat? qualityMap = null)
        {
            ValidateNotNull(reference, nameof(reference));
            ValidateNotNull(comparison, nameof(comparison));
            var values = new double[4];
            NativeException.ThrowIfError(NativeMethods.QualityMSEComputeStatic(
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
            NativeException.ThrowIfError(NativeMethods.QualityMSECreate(reference.NativeHandle, out IntPtr nativeHandle));
            return NativeQualityHandle.FromNativePointer(nativeHandle);
        }
    }
}
