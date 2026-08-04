using System;
using System.Globalization;
using System.Runtime.InteropServices;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.XStereo
{
    /// <summary>
    /// Propagation parameters for QuasiDenseStereo.
    /// QuasiDenseStereo 传播参数。
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct PropagationParameters : IEquatable<PropagationParameters>
    {
        /// <summary>Initializes propagation parameters. 初始化传播参数。</summary>
        public PropagationParameters(
            int corrWinSizeX,
            int corrWinSizeY,
            int borderX,
            int borderY,
            float correlationThreshold,
            float textureThreshold,
            int neighborhoodSize,
            int disparityGradient,
            int lkTemplateSize,
            int lkPyrLvl,
            int lkTermParam1,
            float lkTermParam2,
            float gftQualityThreshold,
            int gftMinSeparationDistance,
            int gftMaxNumFeatures)
        {
            CorrWinSizeX = corrWinSizeX;
            CorrWinSizeY = corrWinSizeY;
            BorderX = borderX;
            BorderY = borderY;
            CorrelationThreshold = correlationThreshold;
            TextureThreshold = textureThreshold;
            NeighborhoodSize = neighborhoodSize;
            DisparityGradient = disparityGradient;
            LkTemplateSize = lkTemplateSize;
            LkPyrLevel = lkPyrLvl;
            LkTermParam1 = lkTermParam1;
            LkTermParam2 = lkTermParam2;
            GftQualityThreshold = gftQualityThreshold;
            GftMinSeparationDistance = gftMinSeparationDistance;
            GftMaxNumFeatures = gftMaxNumFeatures;
        }

        /// <summary>Gets the correlation window width. 获取相关窗口宽度。</summary>
        public int CorrWinSizeX { get; }

        /// <summary>Gets the correlation window height. 获取相关窗口高度。</summary>
        public int CorrWinSizeY { get; }

        /// <summary>Gets ignored border width. 获取忽略边界宽度。</summary>
        public int BorderX { get; }

        /// <summary>Gets ignored border height. 获取忽略边界高度。</summary>
        public int BorderY { get; }

        /// <summary>Gets the correlation threshold. 获取相关性阈值。</summary>
        public float CorrelationThreshold { get; }

        /// <summary>Gets the texture threshold. 获取纹理阈值。</summary>
        public float TextureThreshold { get; }

        /// <summary>Gets the neighborhood size. 获取邻域大小。</summary>
        public int NeighborhoodSize { get; }

        /// <summary>Gets the disparity gradient threshold. 获取视差梯度阈值。</summary>
        public int DisparityGradient { get; }

        /// <summary>Gets LK template size. 获取 LK 模板大小。</summary>
        public int LkTemplateSize { get; }

        /// <summary>Gets LK pyramid level. 获取 LK 金字塔层级。</summary>
        public int LkPyrLevel { get; }

        /// <summary>Gets the first LK termination parameter. 获取第一个 LK 终止参数。</summary>
        public int LkTermParam1 { get; }

        /// <summary>Gets the second LK termination parameter. 获取第二个 LK 终止参数。</summary>
        public float LkTermParam2 { get; }

        /// <summary>Gets GFT quality threshold. 获取 GFT 质量阈值。</summary>
        public float GftQualityThreshold { get; }

        /// <summary>Gets GFT minimum separation distance. 获取 GFT 最小间隔距离。</summary>
        public int GftMinSeparationDistance { get; }

        /// <summary>Gets GFT maximum feature count. 获取 GFT 最大特征数量。</summary>
        public int GftMaxNumFeatures { get; }

        /// <summary>Determines whether two parameter sets are equal. 判断两组参数是否相等。</summary>
        public static bool operator ==(PropagationParameters left, PropagationParameters right)
        {
            return left.Equals(right);
        }

        /// <summary>Determines whether two parameter sets are different. 判断两组参数是否不同。</summary>
        public static bool operator !=(PropagationParameters left, PropagationParameters right)
        {
            return !left.Equals(right);
        }

        /// <summary>Indicates whether this parameter set equals another parameter set. 指示此参数组是否与另一个参数组相等。</summary>
        public bool Equals(PropagationParameters other)
        {
            return CorrWinSizeX == other.CorrWinSizeX
                && CorrWinSizeY == other.CorrWinSizeY
                && BorderX == other.BorderX
                && BorderY == other.BorderY
                && CorrelationThreshold.Equals(other.CorrelationThreshold)
                && TextureThreshold.Equals(other.TextureThreshold)
                && NeighborhoodSize == other.NeighborhoodSize
                && DisparityGradient == other.DisparityGradient
                && LkTemplateSize == other.LkTemplateSize
                && LkPyrLevel == other.LkPyrLevel
                && LkTermParam1 == other.LkTermParam1
                && LkTermParam2.Equals(other.LkTermParam2)
                && GftQualityThreshold.Equals(other.GftQualityThreshold)
                && GftMinSeparationDistance == other.GftMinSeparationDistance
                && GftMaxNumFeatures == other.GftMaxNumFeatures;
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is PropagationParameters other && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = CorrWinSizeX;
                hash = (hash * 397) ^ CorrWinSizeY;
                hash = (hash * 397) ^ BorderX;
                hash = (hash * 397) ^ BorderY;
                hash = (hash * 397) ^ CorrelationThreshold.GetHashCode();
                hash = (hash * 397) ^ TextureThreshold.GetHashCode();
                hash = (hash * 397) ^ NeighborhoodSize;
                hash = (hash * 397) ^ DisparityGradient;
                hash = (hash * 397) ^ LkTemplateSize;
                hash = (hash * 397) ^ LkPyrLevel;
                hash = (hash * 397) ^ LkTermParam1;
                hash = (hash * 397) ^ LkTermParam2.GetHashCode();
                hash = (hash * 397) ^ GftQualityThreshold.GetHashCode();
                hash = (hash * 397) ^ GftMinSeparationDistance;
                hash = (hash * 397) ^ GftMaxNumFeatures;
                return hash;
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{{CorrWinSizeX={0},CorrWinSizeY={1},BorderX={2},BorderY={3},CorrelationThreshold={4},TextureThreshold={5},NeighborhoodSize={6},DisparityGradient={7},LkTemplateSize={8},LkPyrLevel={9},LkTermParam1={10},LkTermParam2={11},GftQualityThreshold={12},GftMinSeparationDistance={13},GftMaxNumFeatures={14}}}",
                CorrWinSizeX,
                CorrWinSizeY,
                BorderX,
                BorderY,
                CorrelationThreshold,
                TextureThreshold,
                NeighborhoodSize,
                DisparityGradient,
                LkTemplateSize,
                LkPyrLevel,
                LkTermParam1,
                LkTermParam2,
                GftQualityThreshold,
                GftMinSeparationDistance,
                GftMaxNumFeatures);
        }

        internal NativeXStereoPropagationParameters ToNative()
        {
            return new NativeXStereoPropagationParameters
            {
                CorrWinSizeX = CorrWinSizeX,
                CorrWinSizeY = CorrWinSizeY,
                BorderX = BorderX,
                BorderY = BorderY,
                CorrelationThreshold = CorrelationThreshold,
                TextureThreshold = TextureThreshold,
                NeighborhoodSize = NeighborhoodSize,
                DisparityGradient = DisparityGradient,
                LkTemplateSize = LkTemplateSize,
                LkPyrLvl = LkPyrLevel,
                LkTermParam1 = LkTermParam1,
                LkTermParam2 = LkTermParam2,
                GftQualityThres = GftQualityThreshold,
                GftMinSeperationDist = GftMinSeparationDistance,
                GftMaxNumFeatures = GftMaxNumFeatures
            };
        }

        internal static PropagationParameters FromNative(NativeXStereoPropagationParameters value)
        {
            return new PropagationParameters(
                value.CorrWinSizeX,
                value.CorrWinSizeY,
                value.BorderX,
                value.BorderY,
                value.CorrelationThreshold,
                value.TextureThreshold,
                value.NeighborhoodSize,
                value.DisparityGradient,
                value.LkTemplateSize,
                value.LkPyrLvl,
                value.LkTermParam1,
                value.LkTermParam2,
                value.GftQualityThres,
                value.GftMinSeperationDist,
                value.GftMaxNumFeatures);
        }
    }
}
