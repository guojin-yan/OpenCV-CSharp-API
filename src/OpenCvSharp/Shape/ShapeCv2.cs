using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Shape
{
    /// <summary>
    /// Provides model-free shape matching helpers.
    /// 提供无模型 shape matching 辅助函数。
    /// </summary>
    public static class ShapeCv2
    {
        /// <summary>
        /// Computes the EMD-L1 distance between two one-column floating-point signatures.
        /// 计算两个单列浮点 signature 之间的 EMD-L1 距离。
        /// </summary>
        public static float EMDL1(Mat signature1, Mat signature2)
        {
            ValidateNotNull(signature1, nameof(signature1));
            ValidateNotNull(signature2, nameof(signature2));
            ValidateEMDL1Signature(signature1, nameof(signature1));
            ValidateEMDL1Signature(signature2, nameof(signature2));
            if (signature1.Rows != signature2.Rows || signature1.Cols != signature2.Cols)
            {
                throw new ArgumentException("EMD-L1 signatures must have the same rows and columns.", nameof(signature2));
            }

            NativeException.ThrowIfError(NativeMethods.ShapeEMDL1(signature1.NativeHandle, signature2.NativeHandle, out float distance));
            return distance;
        }

        /// <summary>
        /// Creates a norm-based histogram cost extractor.
        /// 创建基于范数的直方图代价提取器。
        /// </summary>
        public static NormHistogramCostExtractor CreateNormHistogramCostExtractor(
            NormTypes flag = NormTypes.L2,
            int nDummies = 25,
            float defaultCost = 0.2F)
        {
            return NormHistogramCostExtractor.Create(flag, nDummies, defaultCost);
        }

        /// <summary>
        /// Creates an EMD histogram cost extractor.
        /// 创建 EMD 直方图代价提取器。
        /// </summary>
        public static EMDHistogramCostExtractor CreateEMDHistogramCostExtractor(
            NormTypes flag = NormTypes.L2,
            int nDummies = 25,
            float defaultCost = 0.2F)
        {
            return EMDHistogramCostExtractor.Create(flag, nDummies, defaultCost);
        }

        /// <summary>
        /// Creates a Chi histogram cost extractor.
        /// 创建 Chi 直方图代价提取器。
        /// </summary>
        public static ChiHistogramCostExtractor CreateChiHistogramCostExtractor(int nDummies = 25, float defaultCost = 0.2F)
        {
            return ChiHistogramCostExtractor.Create(nDummies, defaultCost);
        }

        /// <summary>
        /// Creates an EMD-L1 histogram cost extractor.
        /// 创建 EMD-L1 直方图代价提取器。
        /// </summary>
        public static EMDL1HistogramCostExtractor CreateEMDL1HistogramCostExtractor(int nDummies = 25, float defaultCost = 0.2F)
        {
            return EMDL1HistogramCostExtractor.Create(nDummies, defaultCost);
        }

        /// <summary>
        /// Creates a Shape Context distance extractor.
        /// 创建 Shape Context 形状距离提取器。
        /// </summary>
        public static ShapeContextDistanceExtractor CreateShapeContextDistanceExtractor(
            int angularBins = 12,
            int radialBins = 4,
            float innerRadius = 0.2F,
            float outerRadius = 2.0F,
            int iterations = 3)
        {
            return ShapeContextDistanceExtractor.Create(angularBins, radialBins, innerRadius, outerRadius, iterations);
        }

        /// <summary>
        /// Creates a Hausdorff distance extractor.
        /// 创建 Hausdorff 形状距离提取器。
        /// </summary>
        public static HausdorffDistanceExtractor CreateHausdorffDistanceExtractor(
            NormTypes distanceFlag = NormTypes.L2,
            float rankProportion = 0.6F)
        {
            return HausdorffDistanceExtractor.Create(distanceFlag, rankProportion);
        }

        private static void ValidateNotNull<T>(T value, string parameterName)
            where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }

        private static void ValidateEMDL1Signature(Mat signature, string parameterName)
        {
            if (signature.Empty)
            {
                throw new ArgumentException("EMD-L1 signature must not be empty.", parameterName);
            }
        }
    }
}
