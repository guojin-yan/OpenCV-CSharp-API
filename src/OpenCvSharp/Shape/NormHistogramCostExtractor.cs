using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Shape
{
    /// <summary>
    /// Norm-based histogram cost extractor.
    /// 基于范数的直方图代价提取器。
    /// </summary>
    public sealed class NormHistogramCostExtractor : NormHistogramCostExtractorBase
    {
        private NormHistogramCostExtractor(NativeShapeHistogramCostExtractorHandle handle)
            : base(handle)
        {
        }

        /// <summary>
        /// Creates a norm-based histogram cost extractor.
        /// 创建基于范数的直方图代价提取器。
        /// </summary>
        public static NormHistogramCostExtractor Create(NormTypes flag = NormTypes.L2, int nDummies = 25, float defaultCost = 0.2F)
        {
            NativeException.ThrowIfError(NativeMethods.ShapeNormHistogramCostExtractorCreate((int)flag, nDummies, defaultCost, out IntPtr nativeHandle));
            return new NormHistogramCostExtractor(NativeShapeHistogramCostExtractorHandle.FromNativePointer(nativeHandle));
        }
    }
}
