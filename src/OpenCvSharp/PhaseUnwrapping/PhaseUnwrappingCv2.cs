namespace JYPPX.OpenCvSharp.PhaseUnwrapping
{
    /// <summary>
    /// Provides OpenCV phase_unwrapping factory helpers.
    /// 提供 OpenCV phase_unwrapping 工厂辅助函数。
    /// </summary>
    public static class PhaseUnwrappingCv2
    {
        /// <summary>
        /// Creates a histogram phase unwrapping algorithm.
        /// 创建直方图相位展开算法。
        /// </summary>
        public static HistogramPhaseUnwrapping CreateHistogramPhaseUnwrapping()
        {
            return HistogramPhaseUnwrapping.Create();
        }

        /// <summary>
        /// Creates a histogram phase unwrapping algorithm.
        /// 创建直方图相位展开算法。
        /// </summary>
        public static HistogramPhaseUnwrapping CreateHistogramPhaseUnwrapping(HistogramPhaseUnwrappingParams parameters)
        {
            return HistogramPhaseUnwrapping.Create(parameters);
        }
    }
}
