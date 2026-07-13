using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Tracking
{
    /// <summary>
    /// Parameters for OpenCV contrib <c>TrackerCSRT</c>.
    /// OpenCV contrib <c>TrackerCSRT</c> 参数。
    /// </summary>
    public readonly struct TrackerCSRTParams : IEquatable<TrackerCSRTParams>
    {
        /// <summary>Initializes CSRT parameters. 初始化 CSRT 参数。</summary>
        public TrackerCSRTParams(
            bool useHog,
            bool useColorNames,
            bool useGray,
            bool useRgb,
            bool useChannelWeights,
            bool useSegmentation,
            string windowFunction,
            float kaiserAlpha,
            float chebAttenuation,
            float templateSize,
            float gslSigma,
            float hogOrientations,
            float hogClip,
            float padding,
            float filterLr,
            float weightsLr,
            int numHogChannelsUsed,
            int admmIterations,
            int histogramBins,
            float histogramLr,
            int backgroundRatio,
            int numberOfScales,
            float scaleSigmaFactor,
            float scaleModelMaxArea,
            float scaleLr,
            float scaleStep,
            float psrThreshold)
        {
            UseHog = useHog;
            UseColorNames = useColorNames;
            UseGray = useGray;
            UseRgb = useRgb;
            UseChannelWeights = useChannelWeights;
            UseSegmentation = useSegmentation;
            WindowFunction = windowFunction ?? throw new ArgumentNullException(nameof(windowFunction));
            KaiserAlpha = kaiserAlpha;
            ChebAttenuation = chebAttenuation;
            TemplateSize = templateSize;
            GslSigma = gslSigma;
            HogOrientations = hogOrientations;
            HogClip = hogClip;
            Padding = padding;
            FilterLr = filterLr;
            WeightsLr = weightsLr;
            NumHogChannelsUsed = numHogChannelsUsed;
            AdmmIterations = admmIterations;
            HistogramBins = histogramBins;
            HistogramLr = histogramLr;
            BackgroundRatio = backgroundRatio;
            NumberOfScales = numberOfScales;
            ScaleSigmaFactor = scaleSigmaFactor;
            ScaleModelMaxArea = scaleModelMaxArea;
            ScaleLr = scaleLr;
            ScaleStep = scaleStep;
            PsrThreshold = psrThreshold;
        }

        /// <summary>Gets whether HOG features are used. 获取是否使用 HOG 特征。</summary>
        public bool UseHog { get; }

        /// <summary>Gets whether color names are used. 获取是否使用颜色名称。</summary>
        public bool UseColorNames { get; }

        /// <summary>Gets whether grayscale features are used. 获取是否使用灰度特征。</summary>
        public bool UseGray { get; }

        /// <summary>Gets whether RGB features are used. 获取是否使用 RGB 特征。</summary>
        public bool UseRgb { get; }

        /// <summary>Gets whether channel weights are used. 获取是否使用通道权重。</summary>
        public bool UseChannelWeights { get; }

        /// <summary>Gets whether segmentation is used. 获取是否使用分割。</summary>
        public bool UseSegmentation { get; }

        /// <summary>Gets the window function name. 获取窗口函数名称。</summary>
        public string WindowFunction { get; }

        /// <summary>Gets the Kaiser alpha value. 获取 Kaiser alpha 值。</summary>
        public float KaiserAlpha { get; }

        /// <summary>Gets the Chebyshev attenuation. 获取 Chebyshev 衰减值。</summary>
        public float ChebAttenuation { get; }

        /// <summary>Gets the template size. 获取模板尺寸。</summary>
        public float TemplateSize { get; }

        /// <summary>Gets the Gaussian spatial label sigma. 获取空间标签 sigma。</summary>
        public float GslSigma { get; }

        /// <summary>Gets HOG orientation count. 获取 HOG 方向数量。</summary>
        public float HogOrientations { get; }

        /// <summary>Gets HOG clipping value. 获取 HOG 裁剪值。</summary>
        public float HogClip { get; }

        /// <summary>Gets padding. 获取 padding。</summary>
        public float Padding { get; }

        /// <summary>Gets filter learning rate. 获取滤波器学习率。</summary>
        public float FilterLr { get; }

        /// <summary>Gets channel-weight learning rate. 获取通道权重学习率。</summary>
        public float WeightsLr { get; }

        /// <summary>Gets number of HOG channels used. 获取使用的 HOG 通道数量。</summary>
        public int NumHogChannelsUsed { get; }

        /// <summary>Gets ADMM iteration count. 获取 ADMM 迭代次数。</summary>
        public int AdmmIterations { get; }

        /// <summary>Gets histogram bin count. 获取直方图 bin 数量。</summary>
        public int HistogramBins { get; }

        /// <summary>Gets histogram learning rate. 获取直方图学习率。</summary>
        public float HistogramLr { get; }

        /// <summary>Gets background ratio. 获取背景比例。</summary>
        public int BackgroundRatio { get; }

        /// <summary>Gets number of scales. 获取尺度数量。</summary>
        public int NumberOfScales { get; }

        /// <summary>Gets scale sigma factor. 获取尺度 sigma 因子。</summary>
        public float ScaleSigmaFactor { get; }

        /// <summary>Gets scale model max area. 获取尺度模型最大面积。</summary>
        public float ScaleModelMaxArea { get; }

        /// <summary>Gets scale learning rate. 获取尺度学习率。</summary>
        public float ScaleLr { get; }

        /// <summary>Gets scale step. 获取尺度步长。</summary>
        public float ScaleStep { get; }

        /// <summary>Gets PSR threshold. 获取 PSR 阈值。</summary>
        public float PsrThreshold { get; }

        /// <summary>Determines whether two parameter values are equal. 判断两个参数值是否相等。</summary>
        public static bool operator ==(TrackerCSRTParams left, TrackerCSRTParams right)
        {
            return left.Equals(right);
        }

        /// <summary>Determines whether two parameter values are different. 判断两个参数值是否不同。</summary>
        public static bool operator !=(TrackerCSRTParams left, TrackerCSRTParams right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Gets OpenCV 5.0.0 default CSRT parameters without calling native code.
        /// 获取 OpenCV 5.0.0 CSRT 默认参数，不调用 native 代码。
        /// </summary>
        public static TrackerCSRTParams Default
        {
            get
            {
                return new TrackerCSRTParams(
                    useHog: true,
                    useColorNames: true,
                    useGray: true,
                    useRgb: false,
                    useChannelWeights: true,
                    useSegmentation: true,
                    windowFunction: "hann",
                    kaiserAlpha: 3.75F,
                    chebAttenuation: 45.0F,
                    templateSize: 200.0F,
                    gslSigma: 1.0F,
                    hogOrientations: 9.0F,
                    hogClip: 0.2F,
                    padding: 3.0F,
                    filterLr: 0.02F,
                    weightsLr: 0.02F,
                    numHogChannelsUsed: 18,
                    admmIterations: 4,
                    histogramBins: 16,
                    histogramLr: 0.04F,
                    backgroundRatio: 2,
                    numberOfScales: 33,
                    scaleSigmaFactor: 0.25F,
                    scaleModelMaxArea: 512.0F,
                    scaleLr: 0.025F,
                    scaleStep: 1.02F,
                    psrThreshold: 0.035F);
            }
        }

        /// <summary>
        /// Gets CSRT defaults from the linked native OpenCV runtime.
        /// 从已链接 native OpenCV runtime 读取 CSRT 默认参数。
        /// </summary>
        public static TrackerCSRTParams GetDefaultFromNative()
        {
            NativeException.ThrowIfError(NativeMethods.TrackingTrackerCsrtGetDefaultParams(out NativeMethods.TrackingCsrtParamsNative native));
            return FromNative(native, Default.WindowFunction);
        }

        internal static TrackerCSRTParams FromNative(NativeMethods.TrackingCsrtParamsNative native, string windowFunction)
        {
            return new TrackerCSRTParams(
                native.UseHog != 0,
                native.UseColorNames != 0,
                native.UseGray != 0,
                native.UseRgb != 0,
                native.UseChannelWeights != 0,
                native.UseSegmentation != 0,
                windowFunction,
                native.KaiserAlpha,
                native.ChebAttenuation,
                native.TemplateSize,
                native.GslSigma,
                native.HogOrientations,
                native.HogClip,
                native.Padding,
                native.FilterLr,
                native.WeightsLr,
                native.NumHogChannelsUsed,
                native.AdmmIterations,
                native.HistogramBins,
                native.HistogramLr,
                native.BackgroundRatio,
                native.NumberOfScales,
                native.ScaleSigmaFactor,
                native.ScaleModelMaxArea,
                native.ScaleLr,
                native.ScaleStep,
                native.PsrThreshold);
        }

        internal NativeMethods.TrackingCsrtParamsNative ToNative(IntPtr windowFunctionPtr)
        {
            return new NativeMethods.TrackingCsrtParamsNative
            {
                UseHog = UseHog ? 1 : 0,
                UseColorNames = UseColorNames ? 1 : 0,
                UseGray = UseGray ? 1 : 0,
                UseRgb = UseRgb ? 1 : 0,
                UseChannelWeights = UseChannelWeights ? 1 : 0,
                UseSegmentation = UseSegmentation ? 1 : 0,
                WindowFunction = windowFunctionPtr,
                KaiserAlpha = KaiserAlpha,
                ChebAttenuation = ChebAttenuation,
                TemplateSize = TemplateSize,
                GslSigma = GslSigma,
                HogOrientations = HogOrientations,
                HogClip = HogClip,
                Padding = Padding,
                FilterLr = FilterLr,
                WeightsLr = WeightsLr,
                NumHogChannelsUsed = NumHogChannelsUsed,
                AdmmIterations = AdmmIterations,
                HistogramBins = HistogramBins,
                HistogramLr = HistogramLr,
                BackgroundRatio = BackgroundRatio,
                NumberOfScales = NumberOfScales,
                ScaleSigmaFactor = ScaleSigmaFactor,
                ScaleModelMaxArea = ScaleModelMaxArea,
                ScaleLr = ScaleLr,
                ScaleStep = ScaleStep,
                PsrThreshold = PsrThreshold
            };
        }

        internal static byte[] ToNullTerminatedUtf8(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            int byteCount = Encoding.UTF8.GetByteCount(value);
            var buffer = new byte[byteCount + 1];
            Encoding.UTF8.GetBytes(value, 0, value.Length, buffer, 0);
            return buffer;
        }

        /// <summary>Indicates whether this value equals another value. 指示此值是否与另一个值相等。</summary>
        public bool Equals(TrackerCSRTParams other)
        {
            return UseHog == other.UseHog
                && UseColorNames == other.UseColorNames
                && UseGray == other.UseGray
                && UseRgb == other.UseRgb
                && UseChannelWeights == other.UseChannelWeights
                && UseSegmentation == other.UseSegmentation
                && string.Equals(WindowFunction, other.WindowFunction, StringComparison.Ordinal)
                && KaiserAlpha.Equals(other.KaiserAlpha)
                && ChebAttenuation.Equals(other.ChebAttenuation)
                && TemplateSize.Equals(other.TemplateSize)
                && GslSigma.Equals(other.GslSigma)
                && HogOrientations.Equals(other.HogOrientations)
                && HogClip.Equals(other.HogClip)
                && Padding.Equals(other.Padding)
                && FilterLr.Equals(other.FilterLr)
                && WeightsLr.Equals(other.WeightsLr)
                && NumHogChannelsUsed == other.NumHogChannelsUsed
                && AdmmIterations == other.AdmmIterations
                && HistogramBins == other.HistogramBins
                && HistogramLr.Equals(other.HistogramLr)
                && BackgroundRatio == other.BackgroundRatio
                && NumberOfScales == other.NumberOfScales
                && ScaleSigmaFactor.Equals(other.ScaleSigmaFactor)
                && ScaleModelMaxArea.Equals(other.ScaleModelMaxArea)
                && ScaleLr.Equals(other.ScaleLr)
                && ScaleStep.Equals(other.ScaleStep)
                && PsrThreshold.Equals(other.PsrThreshold);
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is TrackerCSRTParams other && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = UseHog.GetHashCode();
                hash = (hash * 397) ^ UseColorNames.GetHashCode();
                hash = (hash * 397) ^ UseGray.GetHashCode();
                hash = (hash * 397) ^ UseRgb.GetHashCode();
                hash = (hash * 397) ^ UseChannelWeights.GetHashCode();
                hash = (hash * 397) ^ UseSegmentation.GetHashCode();
                hash = (hash * 397) ^ (WindowFunction == null ? 0 : WindowFunction.GetHashCode());
                hash = (hash * 397) ^ KaiserAlpha.GetHashCode();
                hash = (hash * 397) ^ ChebAttenuation.GetHashCode();
                hash = (hash * 397) ^ TemplateSize.GetHashCode();
                hash = (hash * 397) ^ GslSigma.GetHashCode();
                hash = (hash * 397) ^ HogOrientations.GetHashCode();
                hash = (hash * 397) ^ HogClip.GetHashCode();
                hash = (hash * 397) ^ Padding.GetHashCode();
                hash = (hash * 397) ^ FilterLr.GetHashCode();
                hash = (hash * 397) ^ WeightsLr.GetHashCode();
                hash = (hash * 397) ^ NumHogChannelsUsed;
                hash = (hash * 397) ^ AdmmIterations;
                hash = (hash * 397) ^ HistogramBins;
                hash = (hash * 397) ^ HistogramLr.GetHashCode();
                hash = (hash * 397) ^ BackgroundRatio;
                hash = (hash * 397) ^ NumberOfScales;
                hash = (hash * 397) ^ ScaleSigmaFactor.GetHashCode();
                hash = (hash * 397) ^ ScaleModelMaxArea.GetHashCode();
                hash = (hash * 397) ^ ScaleLr.GetHashCode();
                hash = (hash * 397) ^ ScaleStep.GetHashCode();
                hash = (hash * 397) ^ PsrThreshold.GetHashCode();
                return hash;
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "{UseHog=" + UseHog
                + ",UseColorNames=" + UseColorNames
                + ",UseGray=" + UseGray
                + ",UseRgb=" + UseRgb
                + ",UseChannelWeights=" + UseChannelWeights
                + ",UseSegmentation=" + UseSegmentation
                + ",WindowFunction=" + WindowFunction
                + ",KaiserAlpha=" + KaiserAlpha.ToString(CultureInfo.InvariantCulture)
                + ",ChebAttenuation=" + ChebAttenuation.ToString(CultureInfo.InvariantCulture)
                + ",TemplateSize=" + TemplateSize.ToString(CultureInfo.InvariantCulture)
                + ",GslSigma=" + GslSigma.ToString(CultureInfo.InvariantCulture)
                + ",HogOrientations=" + HogOrientations.ToString(CultureInfo.InvariantCulture)
                + ",HogClip=" + HogClip.ToString(CultureInfo.InvariantCulture)
                + ",Padding=" + Padding.ToString(CultureInfo.InvariantCulture)
                + ",FilterLr=" + FilterLr.ToString(CultureInfo.InvariantCulture)
                + ",WeightsLr=" + WeightsLr.ToString(CultureInfo.InvariantCulture)
                + ",NumHogChannelsUsed=" + NumHogChannelsUsed
                + ",AdmmIterations=" + AdmmIterations
                + ",HistogramBins=" + HistogramBins
                + ",HistogramLr=" + HistogramLr.ToString(CultureInfo.InvariantCulture)
                + ",BackgroundRatio=" + BackgroundRatio
                + ",NumberOfScales=" + NumberOfScales
                + ",ScaleSigmaFactor=" + ScaleSigmaFactor.ToString(CultureInfo.InvariantCulture)
                + ",ScaleModelMaxArea=" + ScaleModelMaxArea.ToString(CultureInfo.InvariantCulture)
                + ",ScaleLr=" + ScaleLr.ToString(CultureInfo.InvariantCulture)
                + ",ScaleStep=" + ScaleStep.ToString(CultureInfo.InvariantCulture)
                + ",PsrThreshold=" + PsrThreshold.ToString(CultureInfo.InvariantCulture) + "}";
        }
    }
}
