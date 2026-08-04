using System;
using System.Runtime.InteropServices;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.BioInspired
{
    /// <summary>
    /// Parameters for the Retina OPL and parvocellular channel.
    /// Retina OPL 与 parvo 通道参数。
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct RetinaParvoParameters : IEquatable<RetinaParvoParameters>
    {
        /// <summary>Initializes parvo channel parameters. 初始化 parvo 通道参数。</summary>
        public RetinaParvoParameters(
            bool colorMode = true,
            bool normaliseOutput = true,
            float photoreceptorsLocalAdaptationSensitivity = 0.75f,
            float photoreceptorsTemporalConstant = 0.9f,
            float photoreceptorsSpatialConstant = 0.53f,
            float horizontalCellsGain = 0.01f,
            float hcellsTemporalConstant = 0.5f,
            float hcellsSpatialConstant = 7.0f,
            float ganglionCellsSensitivity = 0.75f)
        {
            ColorMode = colorMode;
            NormaliseOutput = normaliseOutput;
            PhotoreceptorsLocalAdaptationSensitivity = photoreceptorsLocalAdaptationSensitivity;
            PhotoreceptorsTemporalConstant = photoreceptorsTemporalConstant;
            PhotoreceptorsSpatialConstant = photoreceptorsSpatialConstant;
            HorizontalCellsGain = horizontalCellsGain;
            HcellsTemporalConstant = hcellsTemporalConstant;
            HcellsSpatialConstant = hcellsSpatialConstant;
            GanglionCellsSensitivity = ganglionCellsSensitivity;
        }

        /// <summary>Gets whether color is processed. 获取是否处理颜色。</summary>
        public bool ColorMode { get; }

        /// <summary>Gets whether output is normalized. 获取输出是否归一化。</summary>
        public bool NormaliseOutput { get; }

        /// <summary>Gets photoreceptor local adaptation sensitivity. 获取感光器局部自适应敏感度。</summary>
        public float PhotoreceptorsLocalAdaptationSensitivity { get; }

        /// <summary>Gets photoreceptor temporal constant. 获取感光器时间常数。</summary>
        public float PhotoreceptorsTemporalConstant { get; }

        /// <summary>Gets photoreceptor spatial constant. 获取感光器空间常数。</summary>
        public float PhotoreceptorsSpatialConstant { get; }

        /// <summary>Gets horizontal cells gain. 获取水平细胞增益。</summary>
        public float HorizontalCellsGain { get; }

        /// <summary>Gets horizontal cells temporal constant. 获取水平细胞时间常数。</summary>
        public float HcellsTemporalConstant { get; }

        /// <summary>Gets horizontal cells spatial constant. 获取水平细胞空间常数。</summary>
        public float HcellsSpatialConstant { get; }

        /// <summary>Gets ganglion cells sensitivity. 获取神经节细胞敏感度。</summary>
        public float GanglionCellsSensitivity { get; }

        /// <summary>Determines whether two parameter values are equal. 判断两个参数值是否相等。</summary>
        public static bool operator ==(RetinaParvoParameters left, RetinaParvoParameters right)
        {
            return left.Equals(right);
        }

        /// <summary>Determines whether two parameter values are different. 判断两个参数值是否不同。</summary>
        public static bool operator !=(RetinaParvoParameters left, RetinaParvoParameters right)
        {
            return !left.Equals(right);
        }

        /// <summary>Creates default parameters. 创建默认参数。</summary>
        public static RetinaParvoParameters Default
        {
            get { return new RetinaParvoParameters(); }
        }

        internal NativeBioInspiredRetinaParvoParameters ToNative()
        {
            return new NativeBioInspiredRetinaParvoParameters
            {
                ColorMode = ColorMode ? 1 : 0,
                NormaliseOutput = NormaliseOutput ? 1 : 0,
                PhotoreceptorsLocalAdaptationSensitivity = PhotoreceptorsLocalAdaptationSensitivity,
                PhotoreceptorsTemporalConstant = PhotoreceptorsTemporalConstant,
                PhotoreceptorsSpatialConstant = PhotoreceptorsSpatialConstant,
                HorizontalCellsGain = HorizontalCellsGain,
                HcellsTemporalConstant = HcellsTemporalConstant,
                HcellsSpatialConstant = HcellsSpatialConstant,
                GanglionCellsSensitivity = GanglionCellsSensitivity
            };
        }

        internal static RetinaParvoParameters FromNative(NativeBioInspiredRetinaParvoParameters value)
        {
            return new RetinaParvoParameters(
                value.ColorMode != 0,
                value.NormaliseOutput != 0,
                value.PhotoreceptorsLocalAdaptationSensitivity,
                value.PhotoreceptorsTemporalConstant,
                value.PhotoreceptorsSpatialConstant,
                value.HorizontalCellsGain,
                value.HcellsTemporalConstant,
                value.HcellsSpatialConstant,
                value.GanglionCellsSensitivity);
        }

        /// <inheritdoc />
        public bool Equals(RetinaParvoParameters other)
        {
            return ColorMode == other.ColorMode &&
                NormaliseOutput == other.NormaliseOutput &&
                PhotoreceptorsLocalAdaptationSensitivity.Equals(other.PhotoreceptorsLocalAdaptationSensitivity) &&
                PhotoreceptorsTemporalConstant.Equals(other.PhotoreceptorsTemporalConstant) &&
                PhotoreceptorsSpatialConstant.Equals(other.PhotoreceptorsSpatialConstant) &&
                HorizontalCellsGain.Equals(other.HorizontalCellsGain) &&
                HcellsTemporalConstant.Equals(other.HcellsTemporalConstant) &&
                HcellsSpatialConstant.Equals(other.HcellsSpatialConstant) &&
                GanglionCellsSensitivity.Equals(other.GanglionCellsSensitivity);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is RetinaParvoParameters other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = ColorMode.GetHashCode();
                hashCode = (hashCode * 397) ^ NormaliseOutput.GetHashCode();
                hashCode = (hashCode * 397) ^ PhotoreceptorsLocalAdaptationSensitivity.GetHashCode();
                hashCode = (hashCode * 397) ^ PhotoreceptorsTemporalConstant.GetHashCode();
                hashCode = (hashCode * 397) ^ PhotoreceptorsSpatialConstant.GetHashCode();
                hashCode = (hashCode * 397) ^ HorizontalCellsGain.GetHashCode();
                hashCode = (hashCode * 397) ^ HcellsTemporalConstant.GetHashCode();
                hashCode = (hashCode * 397) ^ HcellsSpatialConstant.GetHashCode();
                hashCode = (hashCode * 397) ^ GanglionCellsSensitivity.GetHashCode();
                return hashCode;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format(
                System.Globalization.CultureInfo.InvariantCulture,
                "RetinaParvoParameters(ColorMode={0}, NormaliseOutput={1}, PhotoreceptorsLocalAdaptationSensitivity={2}, PhotoreceptorsTemporalConstant={3}, PhotoreceptorsSpatialConstant={4}, HorizontalCellsGain={5}, HcellsTemporalConstant={6}, HcellsSpatialConstant={7}, GanglionCellsSensitivity={8})",
                ColorMode,
                NormaliseOutput,
                PhotoreceptorsLocalAdaptationSensitivity,
                PhotoreceptorsTemporalConstant,
                PhotoreceptorsSpatialConstant,
                HorizontalCellsGain,
                HcellsTemporalConstant,
                HcellsSpatialConstant,
                GanglionCellsSensitivity);
        }
    }
}
