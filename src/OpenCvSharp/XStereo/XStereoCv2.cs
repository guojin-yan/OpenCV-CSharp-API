using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.XStereo
{
    /// <summary>
    /// Static helpers for the OpenCV xstereo module.
    /// OpenCV xstereo 模块的静态辅助方法。
    /// </summary>
    public static class XStereoCv2
    {
        /// <summary>Runs a census transform. 运行 census transform。</summary>
        public static void CensusTransform(Mat image, int kernelSize, Mat dist, CensusTransformType type = CensusTransformType.Dense)
        {
            ValidateNotNull(image, nameof(image));
            ValidatePositive(kernelSize, nameof(kernelSize));
            ValidateNotNull(dist, nameof(dist));
            ValidateCensusTransformType(type, nameof(type));
            NativeException.ThrowIfError(NativeMethods.XStereoCensusTransform(image.NativeHandle, kernelSize, dist.NativeHandle, (int)type));
        }

        /// <summary>Runs a census transform and returns a new matrix. 运行 census transform 并返回新矩阵。</summary>
        public static Mat CensusTransform(Mat image, int kernelSize, CensusTransformType type = CensusTransformType.Dense)
        {
            return Transform(image, kernelSize, type, CensusTransform);
        }

        /// <summary>Runs a census transform on a pair. 对图像对运行 census transform。</summary>
        public static void CensusTransform(Mat image1, Mat image2, int kernelSize, Mat dist1, Mat dist2, CensusTransformType type = CensusTransformType.Dense)
        {
            ValidatePairArguments(image1, image2, kernelSize, dist1, dist2);
            ValidateCensusTransformType(type, nameof(type));
            NativeException.ThrowIfError(NativeMethods.XStereoCensusTransformPair(image1.NativeHandle, image2.NativeHandle, kernelSize, dist1.NativeHandle, dist2.NativeHandle, (int)type));
        }

        /// <summary>Runs a modified census transform. 运行 modified census transform。</summary>
        public static void ModifiedCensusTransform(Mat image, int kernelSize, Mat dist, CensusTransformType type = CensusTransformType.Modified, int tolerance = 0, Mat? integralImage = null)
        {
            ValidateNotNull(image, nameof(image));
            ValidatePositive(kernelSize, nameof(kernelSize));
            ValidateNotNull(dist, nameof(dist));
            ValidateCensusTransformType(type, nameof(type));
            NativeException.ThrowIfError(NativeMethods.XStereoModifiedCensusTransform(
                image.NativeHandle,
                kernelSize,
                dist.NativeHandle,
                (int)type,
                tolerance,
                OptionalHandle(integralImage)));
        }

        /// <summary>Runs a modified census transform and returns a new matrix. 运行 modified census transform 并返回新矩阵。</summary>
        public static Mat ModifiedCensusTransform(Mat image, int kernelSize, CensusTransformType type = CensusTransformType.Modified, int tolerance = 0, Mat? integralImage = null)
        {
            var dist = new Mat();
            try
            {
                ModifiedCensusTransform(image, kernelSize, dist, type, tolerance, integralImage);
                return dist;
            }
            catch
            {
                dist.Dispose();
                throw;
            }
        }

        /// <summary>Runs a modified census transform on a pair. 对图像对运行 modified census transform。</summary>
        public static void ModifiedCensusTransform(Mat image1, Mat image2, int kernelSize, Mat dist1, Mat dist2, CensusTransformType type = CensusTransformType.Modified, int tolerance = 0, Mat? integralImage1 = null, Mat? integralImage2 = null)
        {
            ValidatePairArguments(image1, image2, kernelSize, dist1, dist2);
            ValidateCensusTransformType(type, nameof(type));
            NativeException.ThrowIfError(NativeMethods.XStereoModifiedCensusTransformPair(
                image1.NativeHandle,
                image2.NativeHandle,
                kernelSize,
                dist1.NativeHandle,
                dist2.NativeHandle,
                (int)type,
                tolerance,
                OptionalHandle(integralImage1),
                OptionalHandle(integralImage2)));
        }

        /// <summary>Runs a symmetric census transform. 运行 symmetric census transform。</summary>
        public static void SymmetricCensusTransform(Mat image, int kernelSize, Mat dist, CensusTransformType type = CensusTransformType.CenterSymmetric)
        {
            ValidateNotNull(image, nameof(image));
            ValidatePositive(kernelSize, nameof(kernelSize));
            ValidateNotNull(dist, nameof(dist));
            ValidateCensusTransformType(type, nameof(type));
            NativeException.ThrowIfError(NativeMethods.XStereoSymetricCensusTransform(image.NativeHandle, kernelSize, dist.NativeHandle, (int)type));
        }

        /// <summary>Runs a symmetric census transform and returns a new matrix. 运行 symmetric census transform 并返回新矩阵。</summary>
        public static Mat SymmetricCensusTransform(Mat image, int kernelSize, CensusTransformType type = CensusTransformType.CenterSymmetric)
        {
            return Transform(image, kernelSize, type, SymmetricCensusTransform);
        }

        /// <summary>Runs a symmetric census transform on a pair. 对图像对运行 symmetric census transform。</summary>
        public static void SymmetricCensusTransform(Mat image1, Mat image2, int kernelSize, Mat dist1, Mat dist2, CensusTransformType type = CensusTransformType.CenterSymmetric)
        {
            ValidatePairArguments(image1, image2, kernelSize, dist1, dist2);
            ValidateCensusTransformType(type, nameof(type));
            NativeException.ThrowIfError(NativeMethods.XStereoSymetricCensusTransformPair(image1.NativeHandle, image2.NativeHandle, kernelSize, dist1.NativeHandle, dist2.NativeHandle, (int)type));
        }

        /// <summary>Runs a star census transform. 运行 star census transform。</summary>
        public static void StarCensusTransform(Mat image, int kernelSize, Mat dist)
        {
            ValidateNotNull(image, nameof(image));
            ValidatePositive(kernelSize, nameof(kernelSize));
            ValidateNotNull(dist, nameof(dist));
            NativeException.ThrowIfError(NativeMethods.XStereoStarCensusTransform(image.NativeHandle, kernelSize, dist.NativeHandle));
        }

        /// <summary>Runs a star census transform and returns a new matrix. 运行 star census transform 并返回新矩阵。</summary>
        public static Mat StarCensusTransform(Mat image, int kernelSize)
        {
            var dist = new Mat();
            try
            {
                StarCensusTransform(image, kernelSize, dist);
                return dist;
            }
            catch
            {
                dist.Dispose();
                throw;
            }
        }

        /// <summary>Runs a star census transform on a pair. 对图像对运行 star census transform。</summary>
        public static void StarCensusTransform(Mat image1, Mat image2, int kernelSize, Mat dist1, Mat dist2)
        {
            ValidatePairArguments(image1, image2, kernelSize, dist1, dist2);
            NativeException.ThrowIfError(NativeMethods.XStereoStarCensusTransformPair(image1.NativeHandle, image2.NativeHandle, kernelSize, dist1.NativeHandle, dist2.NativeHandle));
        }

        internal static void ValidateNotNull<T>(T value, string parameterName)
            where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }

        internal static void ValidatePositive(int value, string parameterName)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Value must be positive.");
            }
        }

        internal static void ValidatePositiveSize(Size size, string parameterName)
        {
            if (size.Width <= 0 || size.Height <= 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Size dimensions must be positive.");
            }
        }

        internal static void ValidateCensusTransformType(CensusTransformType value, string parameterName)
        {
            if (value != CensusTransformType.Dense
                && value != CensusTransformType.Sparse
                && value != CensusTransformType.CenterSymmetric
                && value != CensusTransformType.ModifiedCenterSymmetric
                && value != CensusTransformType.Modified
                && value != CensusTransformType.MeanVariation
                && value != CensusTransformType.StarKernel)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Census transform type must be a defined value.");
            }
        }

        internal static void ValidateStereoBinaryBMPreFilterType(StereoBinaryBMPreFilterType value, string parameterName)
        {
            if (value != StereoBinaryBMPreFilterType.NormalizedResponse
                && value != StereoBinaryBMPreFilterType.XSobel)
            {
                throw new ArgumentOutOfRangeException(parameterName, "StereoBinaryBM pre-filter type must be a defined value.");
            }
        }

        internal static void ValidateStereoSpeckleRemovalAlgorithm(StereoSpeckleRemovalAlgorithm value, string parameterName)
        {
            if (value != StereoSpeckleRemovalAlgorithm.Default
                && value != StereoSpeckleRemovalAlgorithm.Average)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Stereo speckle removal algorithm must be a defined value.");
            }
        }

        internal static void ValidateStereoBinarySGBMMode(StereoBinarySGBMMode value, string parameterName)
        {
            if (value != StereoBinarySGBMMode.Sgbm
                && value != StereoBinarySGBMMode.HH)
            {
                throw new ArgumentOutOfRangeException(parameterName, "StereoBinarySGBM mode must be a defined value.");
            }
        }

        internal static void ValidateStereoSubPixelInterpolationMethod(StereoSubPixelInterpolationMethod value, string parameterName)
        {
            if (value != StereoSubPixelInterpolationMethod.Quadratic
                && value != StereoSubPixelInterpolationMethod.Symmetric)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Stereo sub-pixel interpolation method must be a defined value.");
            }
        }

        private static Mat Transform(Mat image, int kernelSize, CensusTransformType type, TransformMethod transform)
        {
            var dist = new Mat();
            try
            {
                transform(image, kernelSize, dist, type);
                return dist;
            }
            catch
            {
                dist.Dispose();
                throw;
            }
        }

        private static void ValidatePairArguments(Mat image1, Mat image2, int kernelSize, Mat dist1, Mat dist2)
        {
            ValidateNotNull(image1, nameof(image1));
            ValidateNotNull(image2, nameof(image2));
            ValidatePositive(kernelSize, nameof(kernelSize));
            ValidateNotNull(dist1, nameof(dist1));
            ValidateNotNull(dist2, nameof(dist2));
        }

        private static IntPtr OptionalHandle(Mat? mat)
        {
            return mat == null ? IntPtr.Zero : mat.NativeHandle;
        }

        private delegate void TransformMethod(Mat image, int kernelSize, Mat dist, CensusTransformType type);
    }
}
