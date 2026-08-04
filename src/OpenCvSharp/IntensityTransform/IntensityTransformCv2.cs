using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.IntensityTransform
{
    /// <summary>
    /// Entry points for OpenCV intensity transformation functions.
    /// OpenCV intensity transformation 函数入口。
    /// </summary>
    public static class IntensityTransformCv2
    {
        /// <summary>Applies logarithmic intensity transformation. 应用对数强度变换。</summary>
        public static void LogTransform(Mat src, Mat dst)
        {
            ValidateSrcDst(src, dst);
            NativeException.ThrowIfError(NativeMethods.IntensityTransformLog(src.NativeHandle, dst.NativeHandle));
        }

        /// <summary>Applies logarithmic intensity transformation and returns a new matrix. 应用对数强度变换并返回新矩阵。</summary>
        public static Mat LogTransform(Mat src)
        {
            var dst = new Mat();
            try
            {
                LogTransform(src, dst);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        /// <summary>Applies gamma correction. 应用 gamma 校正。</summary>
        public static void GammaCorrection(Mat src, Mat dst, float gamma)
        {
            ValidateSrcDst(src, dst);
            ValidatePositiveFinite(gamma, nameof(gamma));
            NativeException.ThrowIfError(NativeMethods.IntensityTransformGammaCorrection(src.NativeHandle, dst.NativeHandle, gamma));
        }

        /// <summary>Applies gamma correction and returns a new matrix. 应用 gamma 校正并返回新矩阵。</summary>
        public static Mat GammaCorrection(Mat src, float gamma)
        {
            var dst = new Mat();
            try
            {
                GammaCorrection(src, dst, gamma);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        /// <summary>Applies intensity autoscaling. 应用强度自动缩放。</summary>
        public static void Autoscaling(Mat src, Mat dst)
        {
            ValidateSrcDst(src, dst);
            NativeException.ThrowIfError(NativeMethods.IntensityTransformAutoscaling(src.NativeHandle, dst.NativeHandle));
        }

        /// <summary>Applies intensity autoscaling and returns a new matrix. 应用强度自动缩放并返回新矩阵。</summary>
        public static Mat Autoscaling(Mat src)
        {
            var dst = new Mat();
            try
            {
                Autoscaling(src, dst);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        /// <summary>Applies linear contrast stretching. 应用线性对比度拉伸。</summary>
        public static void ContrastStretching(Mat src, Mat dst, int r1, int s1, int r2, int s2)
        {
            ValidateSrcDst(src, dst);
            ValidateByteRange(r1, nameof(r1));
            ValidateByteRange(s1, nameof(s1));
            ValidateByteRange(r2, nameof(r2));
            ValidateByteRange(s2, nameof(s2));
            if (r1 >= r2)
            {
                throw new ArgumentOutOfRangeException(nameof(r2), "r2 must be greater than r1.");
            }

            NativeException.ThrowIfError(NativeMethods.IntensityTransformContrastStretching(src.NativeHandle, dst.NativeHandle, r1, s1, r2, s2));
        }

        /// <summary>Applies linear contrast stretching and returns a new matrix. 应用线性对比度拉伸并返回新矩阵。</summary>
        public static Mat ContrastStretching(Mat src, int r1, int s1, int r2, int s2)
        {
            var dst = new Mat();
            try
            {
                ContrastStretching(src, dst, r1, s1, r2, s2);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        /// <summary>Enhances low-light images using BIMEF with automatic exposure ratio. 使用自动曝光比例通过 BIMEF 增强低照度图像。</summary>
        public static void Bimef(Mat src, Mat dst, float mu = 0.5F, float a = -0.3293F, float b = 1.1258F)
        {
            ValidateSrcDst(src, dst);
            ValidateBimefInput(src, nameof(src));
            ValidatePositiveFinite(mu, nameof(mu));
            ValidateFinite(a, nameof(a));
            ValidateFinite(b, nameof(b));
            NativeException.ThrowIfError(NativeMethods.IntensityTransformBimef(src.NativeHandle, dst.NativeHandle, mu, a, b));
        }

        /// <summary>Enhances low-light images using BIMEF with automatic exposure ratio and returns a new matrix. 使用自动曝光比例通过 BIMEF 增强低照度图像并返回新矩阵。</summary>
        public static Mat Bimef(Mat src, float mu = 0.5F, float a = -0.3293F, float b = 1.1258F)
        {
            var dst = new Mat();
            try
            {
                Bimef(src, dst, mu, a, b);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        /// <summary>Enhances low-light images using BIMEF with explicit exposure ratio. 使用显式曝光比例通过 BIMEF 增强低照度图像。</summary>
        public static void Bimef(Mat src, Mat dst, float k, float mu, float a, float b)
        {
            ValidateSrcDst(src, dst);
            ValidateBimefInput(src, nameof(src));
            ValidatePositiveFinite(k, nameof(k));
            ValidatePositiveFinite(mu, nameof(mu));
            ValidateFinite(a, nameof(a));
            ValidateFinite(b, nameof(b));
            NativeException.ThrowIfError(NativeMethods.IntensityTransformBimefWithK(src.NativeHandle, dst.NativeHandle, k, mu, a, b));
        }

        /// <summary>Enhances low-light images using BIMEF with explicit exposure ratio and returns a new matrix. 使用显式曝光比例通过 BIMEF 增强低照度图像并返回新矩阵。</summary>
        public static Mat Bimef(Mat src, float k, float mu, float a, float b)
        {
            var dst = new Mat();
            try
            {
                Bimef(src, dst, k, mu, a, b);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        private static void ValidateSrcDst(Mat src, Mat dst)
        {
            ValidateNotNull(src, nameof(src));
            ValidateNotNull(dst, nameof(dst));
        }

        private static void ValidateBimefInput(Mat src, string parameterName)
        {
            if (!src.Empty && src.Type != MatType.CV_8UC3)
            {
                throw new ArgumentException("BIMEF input image must be CV_8UC3.", parameterName);
            }
        }

        private static void ValidateNotNull<T>(T value, string parameterName)
            where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }

        private static void ValidateByteRange(int value, string parameterName)
        {
            if (value < 0 || value > 255)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Value must be in the [0, 255] range.");
            }
        }

        private static void ValidatePositiveFinite(float value, string parameterName)
        {
            ValidateFinite(value, parameterName);
            if (value <= 0.0F)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Value must be positive.");
            }
        }

        private static void ValidateFinite(float value, string parameterName)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(parameterName, "Value must be finite.");
            }
        }
    }
}
