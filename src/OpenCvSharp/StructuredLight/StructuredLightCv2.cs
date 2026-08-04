namespace JYPPX.OpenCvSharp.StructuredLight
{
    /// <summary>
    /// Provides OpenCV structured_light factory helpers.
    /// 提供 OpenCV structured_light 工厂辅助函数。
    /// </summary>
    public static class StructuredLightCv2
    {
        /// <summary>
        /// Creates a Gray-code structured-light pattern.
        /// 创建 Gray-code 结构光图案。
        /// </summary>
        public static GrayCodePattern CreateGrayCodePattern()
        {
            return GrayCodePattern.Create();
        }

        /// <summary>
        /// Creates a Gray-code structured-light pattern.
        /// 创建 Gray-code 结构光图案。
        /// </summary>
        public static GrayCodePattern CreateGrayCodePattern(GrayCodePatternParams parameters)
        {
            return GrayCodePattern.Create(parameters);
        }

        /// <summary>
        /// Creates a sinusoidal structured-light pattern.
        /// 创建正弦结构光图案。
        /// </summary>
        public static SinusoidalPattern CreateSinusoidalPattern()
        {
            return SinusoidalPattern.Create();
        }

        /// <summary>
        /// Creates a sinusoidal structured-light pattern.
        /// 创建正弦结构光图案。
        /// </summary>
        public static SinusoidalPattern CreateSinusoidalPattern(SinusoidalPatternParams parameters)
        {
            return SinusoidalPattern.Create(parameters);
        }
    }
}
