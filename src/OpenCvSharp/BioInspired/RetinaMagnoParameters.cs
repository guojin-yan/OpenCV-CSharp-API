using System;
using System.Runtime.InteropServices;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.BioInspired
{
    /// <summary>
    /// Parameters for the Retina magnocellular channel.
    /// Retina magno 通道参数。
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct RetinaMagnoParameters : IEquatable<RetinaMagnoParameters>
    {
        /// <summary>Initializes magno channel parameters. 初始化 magno 通道参数。</summary>
        public RetinaMagnoParameters(
            bool normaliseOutput = true,
            float parasolCellsBeta = 0.0f,
            float parasolCellsTau = 0.0f,
            float parasolCellsK = 7.0f,
            float amacrinCellsTemporalCutFrequency = 2.0f,
            float v0CompressionParameter = 0.95f,
            float localAdaptIntegrationTau = 0.0f,
            float localAdaptIntegrationK = 7.0f)
        {
            NormaliseOutput = normaliseOutput;
            ParasolCellsBeta = parasolCellsBeta;
            ParasolCellsTau = parasolCellsTau;
            ParasolCellsK = parasolCellsK;
            AmacrinCellsTemporalCutFrequency = amacrinCellsTemporalCutFrequency;
            V0CompressionParameter = v0CompressionParameter;
            LocalAdaptIntegrationTau = localAdaptIntegrationTau;
            LocalAdaptIntegrationK = localAdaptIntegrationK;
        }

        /// <summary>Gets whether output is normalized. 获取输出是否归一化。</summary>
        public bool NormaliseOutput { get; }

        /// <summary>Gets parasol beta. 获取 parasol beta。</summary>
        public float ParasolCellsBeta { get; }

        /// <summary>Gets parasol tau. 获取 parasol tau。</summary>
        public float ParasolCellsTau { get; }

        /// <summary>Gets parasol k. 获取 parasol k。</summary>
        public float ParasolCellsK { get; }

        /// <summary>Gets amacrine temporal cut frequency. 获取无长突细胞时间截止频率。</summary>
        public float AmacrinCellsTemporalCutFrequency { get; }

        /// <summary>Gets V0 compression parameter. 获取 V0 压缩参数。</summary>
        public float V0CompressionParameter { get; }

        /// <summary>Gets local adaptation integration tau. 获取局部自适应积分 tau。</summary>
        public float LocalAdaptIntegrationTau { get; }

        /// <summary>Gets local adaptation integration k. 获取局部自适应积分 k。</summary>
        public float LocalAdaptIntegrationK { get; }

        /// <summary>Determines whether two parameter values are equal. 判断两个参数值是否相等。</summary>
        public static bool operator ==(RetinaMagnoParameters left, RetinaMagnoParameters right)
        {
            return left.Equals(right);
        }

        /// <summary>Determines whether two parameter values are different. 判断两个参数值是否不同。</summary>
        public static bool operator !=(RetinaMagnoParameters left, RetinaMagnoParameters right)
        {
            return !left.Equals(right);
        }

        /// <summary>Creates default parameters. 创建默认参数。</summary>
        public static RetinaMagnoParameters Default
        {
            get { return new RetinaMagnoParameters(); }
        }

        internal NativeBioInspiredRetinaMagnoParameters ToNative()
        {
            return new NativeBioInspiredRetinaMagnoParameters
            {
                NormaliseOutput = NormaliseOutput ? 1 : 0,
                ParasolCellsBeta = ParasolCellsBeta,
                ParasolCellsTau = ParasolCellsTau,
                ParasolCellsK = ParasolCellsK,
                AmacrinCellsTemporalCutFrequency = AmacrinCellsTemporalCutFrequency,
                V0CompressionParameter = V0CompressionParameter,
                LocalAdaptIntegrationTau = LocalAdaptIntegrationTau,
                LocalAdaptIntegrationK = LocalAdaptIntegrationK
            };
        }

        internal static RetinaMagnoParameters FromNative(NativeBioInspiredRetinaMagnoParameters value)
        {
            return new RetinaMagnoParameters(
                value.NormaliseOutput != 0,
                value.ParasolCellsBeta,
                value.ParasolCellsTau,
                value.ParasolCellsK,
                value.AmacrinCellsTemporalCutFrequency,
                value.V0CompressionParameter,
                value.LocalAdaptIntegrationTau,
                value.LocalAdaptIntegrationK);
        }

        /// <inheritdoc />
        public bool Equals(RetinaMagnoParameters other)
        {
            return NormaliseOutput == other.NormaliseOutput &&
                ParasolCellsBeta.Equals(other.ParasolCellsBeta) &&
                ParasolCellsTau.Equals(other.ParasolCellsTau) &&
                ParasolCellsK.Equals(other.ParasolCellsK) &&
                AmacrinCellsTemporalCutFrequency.Equals(other.AmacrinCellsTemporalCutFrequency) &&
                V0CompressionParameter.Equals(other.V0CompressionParameter) &&
                LocalAdaptIntegrationTau.Equals(other.LocalAdaptIntegrationTau) &&
                LocalAdaptIntegrationK.Equals(other.LocalAdaptIntegrationK);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is RetinaMagnoParameters other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = NormaliseOutput.GetHashCode();
                hashCode = (hashCode * 397) ^ ParasolCellsBeta.GetHashCode();
                hashCode = (hashCode * 397) ^ ParasolCellsTau.GetHashCode();
                hashCode = (hashCode * 397) ^ ParasolCellsK.GetHashCode();
                hashCode = (hashCode * 397) ^ AmacrinCellsTemporalCutFrequency.GetHashCode();
                hashCode = (hashCode * 397) ^ V0CompressionParameter.GetHashCode();
                hashCode = (hashCode * 397) ^ LocalAdaptIntegrationTau.GetHashCode();
                hashCode = (hashCode * 397) ^ LocalAdaptIntegrationK.GetHashCode();
                return hashCode;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format(
                System.Globalization.CultureInfo.InvariantCulture,
                "RetinaMagnoParameters(NormaliseOutput={0}, ParasolCellsBeta={1}, ParasolCellsTau={2}, ParasolCellsK={3}, AmacrinCellsTemporalCutFrequency={4}, V0CompressionParameter={5}, LocalAdaptIntegrationTau={6}, LocalAdaptIntegrationK={7})",
                NormaliseOutput,
                ParasolCellsBeta,
                ParasolCellsTau,
                ParasolCellsK,
                AmacrinCellsTemporalCutFrequency,
                V0CompressionParameter,
                LocalAdaptIntegrationTau,
                LocalAdaptIntegrationK);
        }
    }
}
