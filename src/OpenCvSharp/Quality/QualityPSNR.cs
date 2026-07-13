using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Quality
{
    /// <summary>
    /// Peak signal-to-noise ratio quality metric.
    /// 峰值信噪比图像质量指标。
    /// </summary>
    public sealed class QualityPSNR : QualityBase
    {
        /// <summary>The OpenCV default maximum pixel value. OpenCV 默认最大像素值。</summary>
        public const double MaxPixelValueDefault = 255.0;

        /// <summary>Creates a PSNR metric from a reference image. 使用参考图像创建 PSNR 指标。</summary>
        public QualityPSNR(Mat reference, double maxPixelValue = MaxPixelValueDefault)
            : base(CreateHandle(reference, maxPixelValue))
        {
        }

        /// <summary>Gets or sets the maximum pixel value used by PSNR. 获取或设置 PSNR 使用的最大像素值。</summary>
        public double MaxPixelValue
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.QualityPSNRGetMaxPixelValue(NativeHandle, out double value));
                return value;
            }

            set
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.QualityPSNRSetMaxPixelValue(NativeHandle, value));
            }
        }

        /// <summary>Creates a PSNR metric from a reference image. 使用参考图像创建 PSNR 指标。</summary>
        public static QualityPSNR Create(Mat reference, double maxPixelValue = MaxPixelValueDefault)
        {
            return new QualityPSNR(reference, maxPixelValue);
        }

        /// <summary>
        /// Computes PSNR between two images.
        /// 计算两幅图像之间的 PSNR。
        /// </summary>
        public static Scalar Compute(Mat reference, Mat comparison, Mat? qualityMap = null, double maxPixelValue = MaxPixelValueDefault)
        {
            ValidateNotNull(reference, nameof(reference));
            ValidateNotNull(comparison, nameof(comparison));
            var values = new double[4];
            NativeException.ThrowIfError(NativeMethods.QualityPSNRComputeStatic(
                reference.NativeHandle,
                comparison.NativeHandle,
                qualityMap == null ? IntPtr.Zero : qualityMap.NativeHandle,
                maxPixelValue,
                values,
                values.Length));
            return ToScalar(values);
        }

        private static NativeQualityHandle CreateHandle(Mat reference, double maxPixelValue)
        {
            ValidateNotNull(reference, nameof(reference));
            NativeException.ThrowIfError(NativeMethods.QualityPSNRCreate(reference.NativeHandle, maxPixelValue, out IntPtr nativeHandle));
            return NativeQualityHandle.FromNativePointer(nativeHandle);
        }
    }
}
