using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Quality
{
    /// <summary>
    /// Structural similarity quality metric.
    /// 结构相似性图像质量指标。
    /// </summary>
    public sealed class QualitySSIM : QualityBase
    {
        /// <summary>Creates an SSIM metric from a reference image. 使用参考图像创建 SSIM 指标。</summary>
        public QualitySSIM(Mat reference)
            : base(CreateHandle(reference))
        {
        }

        /// <summary>Creates an SSIM metric from a reference image. 使用参考图像创建 SSIM 指标。</summary>
        public static QualitySSIM Create(Mat reference)
        {
            return new QualitySSIM(reference);
        }

        /// <summary>
        /// Computes SSIM between two images.
        /// 计算两幅图像之间的 SSIM。
        /// </summary>
        public static Scalar Compute(Mat reference, Mat comparison, Mat? qualityMap = null)
        {
            ValidateNotNull(reference, nameof(reference));
            ValidateNotNull(comparison, nameof(comparison));
            var values = new double[4];
            NativeException.ThrowIfError(NativeMethods.QualitySSIMComputeStatic(
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
            NativeException.ThrowIfError(NativeMethods.QualitySSIMCreate(reference.NativeHandle, out IntPtr nativeHandle));
            return NativeQualityHandle.FromNativePointer(nativeHandle);
        }
    }
}
