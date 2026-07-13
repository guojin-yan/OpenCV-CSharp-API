using System;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Shape
{
    /// <summary>
    /// Chi histogram cost extractor.
    /// Chi 直方图代价提取器。
    /// </summary>
    public sealed class ChiHistogramCostExtractor : HistogramCostExtractor
    {
        private ChiHistogramCostExtractor(NativeShapeHistogramCostExtractorHandle handle)
            : base(handle)
        {
        }

        /// <summary>
        /// Creates a Chi histogram cost extractor.
        /// 创建 Chi 直方图代价提取器。
        /// </summary>
        public static ChiHistogramCostExtractor Create(int nDummies = 25, float defaultCost = 0.2F)
        {
            NativeException.ThrowIfError(NativeMethods.ShapeChiHistogramCostExtractorCreate(nDummies, defaultCost, out IntPtr nativeHandle));
            return new ChiHistogramCostExtractor(NativeShapeHistogramCostExtractorHandle.FromNativePointer(nativeHandle));
        }
    }
}
