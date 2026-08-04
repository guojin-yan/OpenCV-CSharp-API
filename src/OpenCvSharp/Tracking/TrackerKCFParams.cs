using System;
using System.Globalization;
using System.Runtime.InteropServices;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Tracking
{
    /// <summary>
    /// Parameters for OpenCV contrib <c>TrackerKCF</c>.
    /// OpenCV contrib <c>TrackerKCF</c> 参数。
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct TrackerKCFParams : IEquatable<TrackerKCFParams>
    {
        /// <summary>Initializes KCF parameters. 初始化 KCF 参数。</summary>
        public TrackerKCFParams(
            float detectThresh,
            float sigma,
            float lambda,
            float interpFactor,
            float outputSigmaFactor,
            float pcaLearningRate,
            bool resize,
            bool splitCoeff,
            bool wrapKernel,
            bool compressFeature,
            int maxPatchSize,
            int compressedSize,
            TrackerKCFMode descPca,
            TrackerKCFMode descNpca)
        {
            ValidateTrackerKCFMode(descPca, nameof(descPca));
            ValidateTrackerKCFMode(descNpca, nameof(descNpca));

            DetectThresh = detectThresh;
            Sigma = sigma;
            Lambda = lambda;
            InterpFactor = interpFactor;
            OutputSigmaFactor = outputSigmaFactor;
            PcaLearningRate = pcaLearningRate;
            Resize = resize;
            SplitCoeff = splitCoeff;
            WrapKernel = wrapKernel;
            CompressFeature = compressFeature;
            MaxPatchSize = maxPatchSize;
            CompressedSize = compressedSize;
            DescPca = descPca;
            DescNpca = descNpca;
        }

        /// <summary>Gets the detection confidence threshold. 获取检测置信度阈值。</summary>
        public float DetectThresh { get; }

        /// <summary>Gets the Gaussian kernel bandwidth. 获取高斯核带宽。</summary>
        public float Sigma { get; }

        /// <summary>Gets the regularization value. 获取正则化值。</summary>
        public float Lambda { get; }

        /// <summary>Gets the interpolation factor. 获取插值因子。</summary>
        public float InterpFactor { get; }

        /// <summary>Gets the output sigma factor. 获取输出 sigma 因子。</summary>
        public float OutputSigmaFactor { get; }

        /// <summary>Gets the PCA learning rate. 获取 PCA 学习率。</summary>
        public float PcaLearningRate { get; }

        /// <summary>Gets whether the image is resized internally. 获取是否内部缩放图像。</summary>
        public bool Resize { get; }

        /// <summary>Gets whether coefficients are split. 获取是否拆分系数。</summary>
        public bool SplitCoeff { get; }

        /// <summary>Gets whether the kernel is wrapped. 获取是否包裹 kernel。</summary>
        public bool WrapKernel { get; }

        /// <summary>Gets whether feature compression is enabled. 获取是否启用特征压缩。</summary>
        public bool CompressFeature { get; }

        /// <summary>Gets the maximum patch size. 获取最大 patch 尺寸。</summary>
        public int MaxPatchSize { get; }

        /// <summary>Gets the compressed feature size. 获取压缩特征尺寸。</summary>
        public int CompressedSize { get; }

        /// <summary>Gets compressed descriptors. 获取压缩描述子。</summary>
        public TrackerKCFMode DescPca { get; }

        /// <summary>Gets non-compressed descriptors. 获取未压缩描述子。</summary>
        public TrackerKCFMode DescNpca { get; }

        /// <summary>Determines whether two parameter values are equal. 判断两个参数值是否相等。</summary>
        public static bool operator ==(TrackerKCFParams left, TrackerKCFParams right)
        {
            return left.Equals(right);
        }

        /// <summary>Determines whether two parameter values are different. 判断两个参数值是否不同。</summary>
        public static bool operator !=(TrackerKCFParams left, TrackerKCFParams right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Gets OpenCV 5.0.0 default KCF parameters without calling native code.
        /// 获取 OpenCV 5.0.0 KCF 默认参数，不调用 native 代码。
        /// </summary>
        public static TrackerKCFParams Default
        {
            get
            {
                return new TrackerKCFParams(
                    detectThresh: 0.5F,
                    sigma: 0.2F,
                    lambda: 0.0001F,
                    interpFactor: 0.075F,
                    outputSigmaFactor: 1.0F / 16.0F,
                    pcaLearningRate: 0.15F,
                    resize: true,
                    splitCoeff: true,
                    wrapKernel: false,
                    compressFeature: true,
                    maxPatchSize: 80 * 80,
                    compressedSize: 2,
                    descPca: TrackerKCFMode.Cn,
                    descNpca: TrackerKCFMode.Gray);
            }
        }

        /// <summary>
        /// Gets KCF defaults from the linked native OpenCV runtime.
        /// 从已链接 native OpenCV runtime 读取 KCF 默认参数。
        /// </summary>
        public static TrackerKCFParams GetDefaultFromNative()
        {
            NativeException.ThrowIfError(NativeMethods.TrackingTrackerKcfGetDefaultParams(out NativeMethods.TrackingKcfParamsNative native));
            return FromNative(native);
        }

        internal NativeMethods.TrackingKcfParamsNative ToNative()
        {
            return new NativeMethods.TrackingKcfParamsNative
            {
                DetectThresh = DetectThresh,
                Sigma = Sigma,
                LambdaValue = Lambda,
                InterpFactor = InterpFactor,
                OutputSigmaFactor = OutputSigmaFactor,
                PcaLearningRate = PcaLearningRate,
                Resize = Resize ? 1 : 0,
                SplitCoeff = SplitCoeff ? 1 : 0,
                WrapKernel = WrapKernel ? 1 : 0,
                CompressFeature = CompressFeature ? 1 : 0,
                MaxPatchSize = MaxPatchSize,
                CompressedSize = CompressedSize,
                DescPca = (int)DescPca,
                DescNpca = (int)DescNpca
            };
        }

        internal static TrackerKCFParams FromNative(NativeMethods.TrackingKcfParamsNative native)
        {
            return new TrackerKCFParams(
                native.DetectThresh,
                native.Sigma,
                native.LambdaValue,
                native.InterpFactor,
                native.OutputSigmaFactor,
                native.PcaLearningRate,
                native.Resize != 0,
                native.SplitCoeff != 0,
                native.WrapKernel != 0,
                native.CompressFeature != 0,
                native.MaxPatchSize,
                native.CompressedSize,
                (TrackerKCFMode)native.DescPca,
                (TrackerKCFMode)native.DescNpca);
        }

        private static void ValidateTrackerKCFMode(TrackerKCFMode value, string parameterName)
        {
            const TrackerKCFMode validMask = TrackerKCFMode.Gray | TrackerKCFMode.Cn | TrackerKCFMode.Custom;
            if ((value & ~validMask) != 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, value, "Unknown KCF descriptor mode bits are not supported.");
            }
        }

        /// <summary>Indicates whether this value equals another value. 指示此值是否与另一个值相等。</summary>
        public bool Equals(TrackerKCFParams other)
        {
            return DetectThresh.Equals(other.DetectThresh)
                && Sigma.Equals(other.Sigma)
                && Lambda.Equals(other.Lambda)
                && InterpFactor.Equals(other.InterpFactor)
                && OutputSigmaFactor.Equals(other.OutputSigmaFactor)
                && PcaLearningRate.Equals(other.PcaLearningRate)
                && Resize == other.Resize
                && SplitCoeff == other.SplitCoeff
                && WrapKernel == other.WrapKernel
                && CompressFeature == other.CompressFeature
                && MaxPatchSize == other.MaxPatchSize
                && CompressedSize == other.CompressedSize
                && DescPca == other.DescPca
                && DescNpca == other.DescNpca;
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is TrackerKCFParams other && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = DetectThresh.GetHashCode();
                hash = (hash * 397) ^ Sigma.GetHashCode();
                hash = (hash * 397) ^ Lambda.GetHashCode();
                hash = (hash * 397) ^ InterpFactor.GetHashCode();
                hash = (hash * 397) ^ OutputSigmaFactor.GetHashCode();
                hash = (hash * 397) ^ PcaLearningRate.GetHashCode();
                hash = (hash * 397) ^ Resize.GetHashCode();
                hash = (hash * 397) ^ SplitCoeff.GetHashCode();
                hash = (hash * 397) ^ WrapKernel.GetHashCode();
                hash = (hash * 397) ^ CompressFeature.GetHashCode();
                hash = (hash * 397) ^ MaxPatchSize;
                hash = (hash * 397) ^ CompressedSize;
                hash = (hash * 397) ^ DescPca.GetHashCode();
                hash = (hash * 397) ^ DescNpca.GetHashCode();
                return hash;
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "{DetectThresh=" + DetectThresh.ToString(CultureInfo.InvariantCulture)
                + ",Sigma=" + Sigma.ToString(CultureInfo.InvariantCulture)
                + ",Lambda=" + Lambda.ToString(CultureInfo.InvariantCulture)
                + ",InterpFactor=" + InterpFactor.ToString(CultureInfo.InvariantCulture)
                + ",OutputSigmaFactor=" + OutputSigmaFactor.ToString(CultureInfo.InvariantCulture)
                + ",PcaLearningRate=" + PcaLearningRate.ToString(CultureInfo.InvariantCulture)
                + ",Resize=" + Resize
                + ",SplitCoeff=" + SplitCoeff
                + ",WrapKernel=" + WrapKernel
                + ",CompressFeature=" + CompressFeature
                + ",MaxPatchSize=" + MaxPatchSize
                + ",CompressedSize=" + CompressedSize
                + ",DescPca=" + DescPca
                + ",DescNpca=" + DescNpca + "}";
        }
    }
}
