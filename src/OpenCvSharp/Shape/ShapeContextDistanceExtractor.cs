using System;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Shape
{
    /// <summary>
    /// Shape Context based distance extractor.
    /// 基于 Shape Context 的形状距离提取器。
    /// </summary>
    public sealed class ShapeContextDistanceExtractor : ShapeDistanceExtractor
    {
        private ShapeContextDistanceExtractor(NativeShapeDistanceExtractorHandle handle)
            : base(handle)
        {
        }

        /// <summary>
        /// Creates a Shape Context distance extractor.
        /// 创建 Shape Context 形状距离提取器。
        /// </summary>
        public static ShapeContextDistanceExtractor Create(
            int angularBins = 12,
            int radialBins = 4,
            float innerRadius = 0.2F,
            float outerRadius = 2.0F,
            int iterations = 3)
        {
            ValidatePositive(angularBins, nameof(angularBins));
            ValidatePositive(radialBins, nameof(radialBins));
            ValidatePositive(innerRadius, nameof(innerRadius));
            ValidatePositive(outerRadius, nameof(outerRadius));
            ValidatePositive(iterations, nameof(iterations));
            NativeException.ThrowIfError(NativeMethods.ShapeContextDistanceExtractorCreate(
                angularBins,
                radialBins,
                innerRadius,
                outerRadius,
                iterations,
                out IntPtr nativeHandle));
            return new ShapeContextDistanceExtractor(NativeShapeDistanceExtractorHandle.FromNativePointer(nativeHandle));
        }

        private static void ValidatePositive(int value, string parameterName)
        {
            if (value < 1)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Value must be greater than zero.");
            }
        }

        private static void ValidatePositive(float value, string parameterName)
        {
            if (float.IsNaN(value) || value <= 0.0F)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Value must be greater than 0.0.");
            }
        }
    }
}
