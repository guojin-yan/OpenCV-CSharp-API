using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.BioInspired
{
    /// <summary>
    /// Factory helpers for the OpenCV bioinspired module.
    /// OpenCV bioinspired 模块的工厂辅助方法。
    /// </summary>
    public static class BioInspiredCv2
    {
        /// <summary>
        /// Creates a Retina model.
        /// 创建 Retina 模型。
        /// </summary>
        public static Retina CreateRetina(
            Size inputSize,
            bool colorMode = true,
            RetinaColorSamplingMethod colorSamplingMethod = RetinaColorSamplingMethod.Bayer,
            bool useRetinaLogSampling = false,
            float reductionFactor = 1.0f,
            float samplingStrength = 10.0f)
        {
            ValidatePositiveSize(inputSize, nameof(inputSize));
            ValidatePositive(reductionFactor, nameof(reductionFactor));
            ValidatePositive(samplingStrength, nameof(samplingStrength));
            NativeException.ThrowIfError(NativeMethods.BioInspiredRetinaCreate(
                inputSize.Width,
                inputSize.Height,
                colorMode ? 1 : 0,
                (int)colorSamplingMethod,
                useRetinaLogSampling ? 1 : 0,
                reductionFactor,
                samplingStrength,
                out IntPtr nativeHandle));
            return new Retina(nativeHandle);
        }

        /// <summary>
        /// Creates a fast tone-mapping model.
        /// 创建 fast tone mapping 模型。
        /// </summary>
        public static RetinaFastToneMapping CreateRetinaFastToneMapping(Size inputSize)
        {
            ValidatePositiveSize(inputSize, nameof(inputSize));
            NativeException.ThrowIfError(NativeMethods.BioInspiredRetinaFastToneMappingCreate(inputSize.Width, inputSize.Height, out IntPtr nativeHandle));
            return new RetinaFastToneMapping(nativeHandle);
        }

        /// <summary>
        /// Creates a transient areas segmentation module.
        /// 创建 transient areas 分割模块。
        /// </summary>
        public static TransientAreasSegmentationModule CreateTransientAreasSegmentationModule(Size inputSize)
        {
            ValidatePositiveSize(inputSize, nameof(inputSize));
            NativeException.ThrowIfError(NativeMethods.BioInspiredTransientAreasCreate(inputSize.Width, inputSize.Height, out IntPtr nativeHandle));
            return new TransientAreasSegmentationModule(nativeHandle);
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

        internal static void ValidatePositive(float value, string parameterName)
        {
            if (value <= 0.0f)
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
    }
}
