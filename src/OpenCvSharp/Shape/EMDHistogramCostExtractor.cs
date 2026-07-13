using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Shape
{
    /// <summary>
    /// Earth Mover's Distance histogram cost extractor.
    /// Earth Mover's Distance 直方图代价提取器。
    /// </summary>
    public sealed class EMDHistogramCostExtractor : NormHistogramCostExtractorBase
    {
        private EMDHistogramCostExtractor(NativeShapeHistogramCostExtractorHandle handle)
            : base(handle)
        {
        }

        /// <summary>
        /// Creates an EMD histogram cost extractor.
        /// 创建 EMD 直方图代价提取器。
        /// </summary>
        public static EMDHistogramCostExtractor Create(NormTypes flag = NormTypes.L2, int nDummies = 25, float defaultCost = 0.2F)
        {
            NativeException.ThrowIfError(NativeMethods.ShapeEMDHistogramCostExtractorCreate((int)flag, nDummies, defaultCost, out IntPtr nativeHandle));
            return new EMDHistogramCostExtractor(NativeShapeHistogramCostExtractorHandle.FromNativePointer(nativeHandle));
        }
    }
}
