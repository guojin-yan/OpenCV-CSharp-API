using System;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Shape
{
    /// <summary>
    /// EMD-L1 histogram cost extractor.
    /// EMD-L1 直方图代价提取器。
    /// </summary>
    public sealed class EMDL1HistogramCostExtractor : HistogramCostExtractor
    {
        private EMDL1HistogramCostExtractor(NativeShapeHistogramCostExtractorHandle handle)
            : base(handle)
        {
        }

        /// <summary>
        /// Creates an EMD-L1 histogram cost extractor.
        /// 创建 EMD-L1 直方图代价提取器。
        /// </summary>
        public static EMDL1HistogramCostExtractor Create(int nDummies = 25, float defaultCost = 0.2F)
        {
            NativeException.ThrowIfError(NativeMethods.ShapeEMDL1HistogramCostExtractorCreate(nDummies, defaultCost, out IntPtr nativeHandle));
            return new EMDL1HistogramCostExtractor(NativeShapeHistogramCostExtractorHandle.FromNativePointer(nativeHandle));
        }
    }
}
