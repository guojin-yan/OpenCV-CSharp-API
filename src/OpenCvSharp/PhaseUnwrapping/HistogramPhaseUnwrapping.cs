using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.PhaseUnwrapping
{
    /// <summary>
    /// Two-dimensional quality-guided histogram phase unwrapping.
    /// 二维质量引导的直方图相位展开算法。
    /// </summary>
    public sealed class HistogramPhaseUnwrapping : PhaseUnwrappingObject
    {
        private HistogramPhaseUnwrapping(NativePhaseUnwrappingHandle handle)
            : base(handle)
        {
        }

        /// <summary>
        /// Creates a histogram phase unwrapping algorithm with OpenCV default parameters.
        /// 使用 OpenCV 默认参数创建直方图相位展开算法。
        /// </summary>
        public static HistogramPhaseUnwrapping Create()
        {
            return Create(HistogramPhaseUnwrappingParams.Default);
        }

        /// <summary>
        /// Creates a histogram phase unwrapping algorithm.
        /// 创建直方图相位展开算法。
        /// </summary>
        public static HistogramPhaseUnwrapping Create(HistogramPhaseUnwrappingParams parameters)
        {
            parameters.Validate();
            NativeException.ThrowIfError(NativeMethods.PhaseUnwrappingHistogramCreate(
                parameters.Width,
                parameters.Height,
                parameters.HistThresh,
                parameters.NbrOfSmallBins,
                parameters.NbrOfLargeBins,
                out IntPtr nativeHandle));
            return new HistogramPhaseUnwrapping(NativePhaseUnwrappingHandle.FromNativePointer(nativeHandle));
        }

        /// <summary>
        /// Creates a histogram phase unwrapping algorithm.
        /// 创建直方图相位展开算法。
        /// </summary>
        public static HistogramPhaseUnwrapping Create(
            int width,
            int height,
            float histThresh,
            int nbrOfSmallBins = 10,
            int nbrOfLargeBins = 5)
        {
            return Create(new HistogramPhaseUnwrappingParams(width, height, histThresh, nbrOfSmallBins, nbrOfLargeBins));
        }

        /// <summary>
        /// Gets the inverse reliability map computed by the previous unwrap call.
        /// 获取上一次展开调用计算得到的反可靠性图。
        /// </summary>
        public void GetInverseReliabilityMap(Mat reliabilityMap)
        {
            ThrowIfDisposed();
            ValidateNotNull(reliabilityMap, nameof(reliabilityMap));
            NativeException.ThrowIfError(NativeMethods.PhaseUnwrappingHistogramGetInverseReliabilityMap(
                NativeHandle,
                reliabilityMap.NativeHandle));
        }

        /// <summary>
        /// Gets the inverse reliability map computed by the previous unwrap call.
        /// 获取上一次展开调用计算得到的反可靠性图。
        /// </summary>
        public Mat GetInverseReliabilityMap()
        {
            var result = new Mat();
            try
            {
                GetInverseReliabilityMap(result);
                return result;
            }
            catch
            {
                result.Dispose();
                throw;
            }
        }
    }
}
