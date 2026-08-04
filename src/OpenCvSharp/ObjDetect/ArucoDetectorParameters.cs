using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.ObjDetect
{
    /// <summary>
    /// Parameters for <see cref="ArucoDetector"/>.
    /// <see cref="ArucoDetector"/> 使用的参数。
    /// </summary>
    public sealed class ArucoDetectorParameters
    {
        /// <summary>
        /// Initializes parameters with OpenCV default values.
        /// 使用 OpenCV 默认值初始化参数。
        /// </summary>
        public ArucoDetectorParameters()
        {
            NativeException.ThrowIfError(NativeMethods.ArucoDetectorDefaultParams(out NativeMethods.ArucoDetectorParamsNative native));
            CopyFromNative(native);
        }

        /// <summary>
        /// Initializes parameters by copying another instance.
        /// 通过复制另一个实例初始化参数。
        /// </summary>
        /// <param name="other">The parameters to copy. 要复制的参数。</param>
        public ArucoDetectorParameters(ArucoDetectorParameters other)
        {
            if (other == null)
            {
                throw new System.ArgumentNullException(nameof(other));
            }

            AdaptiveThreshWinSizeMin = other.AdaptiveThreshWinSizeMin;
            AdaptiveThreshWinSizeMax = other.AdaptiveThreshWinSizeMax;
            AdaptiveThreshWinSizeStep = other.AdaptiveThreshWinSizeStep;
            AdaptiveThreshConstant = other.AdaptiveThreshConstant;
            MinMarkerPerimeterRate = other.MinMarkerPerimeterRate;
            MaxMarkerPerimeterRate = other.MaxMarkerPerimeterRate;
            PolygonalApproxAccuracyRate = other.PolygonalApproxAccuracyRate;
            MinCornerDistanceRate = other.MinCornerDistanceRate;
            MinDistanceToBorder = other.MinDistanceToBorder;
            MinMarkerDistanceRate = other.MinMarkerDistanceRate;
            MinGroupDistance = other.MinGroupDistance;
            CornerRefinementMethod = other.CornerRefinementMethod;
            CornerRefinementWinSize = other.CornerRefinementWinSize;
            RelativeCornerRefinementWinSize = other.RelativeCornerRefinementWinSize;
            CornerRefinementMaxIterations = other.CornerRefinementMaxIterations;
            CornerRefinementMinAccuracy = other.CornerRefinementMinAccuracy;
            MarkerBorderBits = other.MarkerBorderBits;
            PerspectiveRemovePixelPerCell = other.PerspectiveRemovePixelPerCell;
            PerspectiveRemoveIgnoredMarginPerCell = other.PerspectiveRemoveIgnoredMarginPerCell;
            MaxErroneousBitsInBorderRate = other.MaxErroneousBitsInBorderRate;
            MinOtsuStdDev = other.MinOtsuStdDev;
            ErrorCorrectionRate = other.ErrorCorrectionRate;
            AprilTagQuadDecimate = other.AprilTagQuadDecimate;
            AprilTagQuadSigma = other.AprilTagQuadSigma;
            AprilTagMinClusterPixels = other.AprilTagMinClusterPixels;
            AprilTagMaxNmaxima = other.AprilTagMaxNmaxima;
            AprilTagCriticalRad = other.AprilTagCriticalRad;
            AprilTagMaxLineFitMse = other.AprilTagMaxLineFitMse;
            AprilTagMinWhiteBlackDiff = other.AprilTagMinWhiteBlackDiff;
            AprilTagDeglitch = other.AprilTagDeglitch;
            DetectInvertedMarker = other.DetectInvertedMarker;
            UseAruco3Detection = other.UseAruco3Detection;
            MinSideLengthCanonicalImg = other.MinSideLengthCanonicalImg;
            MinMarkerLengthRatioOriginalImg = other.MinMarkerLengthRatioOriginalImg;
            ValidBitIdThreshold = other.ValidBitIdThreshold;
        }

        /// <summary>Gets or sets the minimum adaptive-threshold window size. 获取或设置自适应阈值最小窗口。</summary>
        public int AdaptiveThreshWinSizeMin { get; set; }

        /// <summary>Gets or sets the maximum adaptive-threshold window size. 获取或设置自适应阈值最大窗口。</summary>
        public int AdaptiveThreshWinSizeMax { get; set; }

        /// <summary>Gets or sets adaptive-threshold window step. 获取或设置自适应阈值窗口步长。</summary>
        public int AdaptiveThreshWinSizeStep { get; set; }

        /// <summary>Gets or sets adaptive-threshold constant. 获取或设置自适应阈值常量。</summary>
        public double AdaptiveThreshConstant { get; set; }

        /// <summary>Gets or sets the minimum marker perimeter rate. 获取或设置最小 marker 周长比例。</summary>
        public double MinMarkerPerimeterRate { get; set; }

        /// <summary>Gets or sets the maximum marker perimeter rate. 获取或设置最大 marker 周长比例。</summary>
        public double MaxMarkerPerimeterRate { get; set; }

        /// <summary>Gets or sets polygonal approximation accuracy rate. 获取或设置多边形逼近精度比例。</summary>
        public double PolygonalApproxAccuracyRate { get; set; }

        /// <summary>Gets or sets the minimum corner distance rate. 获取或设置最小角点距离比例。</summary>
        public double MinCornerDistanceRate { get; set; }

        /// <summary>Gets or sets the minimum distance to image border. 获取或设置到图像边界的最小距离。</summary>
        public int MinDistanceToBorder { get; set; }

        /// <summary>Gets or sets the minimum marker distance rate. 获取或设置最小 marker 距离比例。</summary>
        public double MinMarkerDistanceRate { get; set; }

        /// <summary>Gets or sets the minimum group distance. 获取或设置最小分组距离。</summary>
        public float MinGroupDistance { get; set; }

        /// <summary>Gets or sets corner refinement method. 获取或设置角点细化方法。</summary>
        public CornerRefineMethod CornerRefinementMethod { get; set; }

        /// <summary>Gets or sets corner refinement window size. 获取或设置角点细化窗口大小。</summary>
        public int CornerRefinementWinSize { get; set; }

        /// <summary>Gets or sets relative corner refinement window size. 获取或设置相对角点细化窗口大小。</summary>
        public float RelativeCornerRefinementWinSize { get; set; }

        /// <summary>Gets or sets max corner refinement iterations. 获取或设置角点细化最大迭代次数。</summary>
        public int CornerRefinementMaxIterations { get; set; }

        /// <summary>Gets or sets minimum corner refinement accuracy. 获取或设置角点细化最小精度。</summary>
        public double CornerRefinementMinAccuracy { get; set; }

        /// <summary>Gets or sets marker border bits. 获取或设置 marker 边框 bit 数。</summary>
        public int MarkerBorderBits { get; set; }

        /// <summary>Gets or sets perspective-remove pixels per cell. 获取或设置透视校正后每个 cell 的像素数。</summary>
        public int PerspectiveRemovePixelPerCell { get; set; }

        /// <summary>Gets or sets ignored margin per cell during perspective removal. 获取或设置透视校正后每个 cell 忽略边距。</summary>
        public double PerspectiveRemoveIgnoredMarginPerCell { get; set; }

        /// <summary>Gets or sets maximum erroneous border-bit rate. 获取或设置边框错误 bit 最大比例。</summary>
        public double MaxErroneousBitsInBorderRate { get; set; }

        /// <summary>Gets or sets minimum Otsu standard deviation. 获取或设置 Otsu 最小标准差。</summary>
        public double MinOtsuStdDev { get; set; }

        /// <summary>Gets or sets dictionary error correction rate. 获取或设置字典纠错比例。</summary>
        public double ErrorCorrectionRate { get; set; }

        /// <summary>Gets or sets AprilTag quad decimation. 获取或设置 AprilTag quad 降采样。</summary>
        public float AprilTagQuadDecimate { get; set; }

        /// <summary>Gets or sets AprilTag quad sigma. 获取或设置 AprilTag quad sigma。</summary>
        public float AprilTagQuadSigma { get; set; }

        /// <summary>Gets or sets AprilTag minimum cluster pixels. 获取或设置 AprilTag 最小聚类像素数。</summary>
        public int AprilTagMinClusterPixels { get; set; }

        /// <summary>Gets or sets AprilTag maximum corner candidates. 获取或设置 AprilTag 最大角点候选数。</summary>
        public int AprilTagMaxNmaxima { get; set; }

        /// <summary>Gets or sets AprilTag critical angle in radians. 获取或设置 AprilTag 临界角度（弧度）。</summary>
        public float AprilTagCriticalRad { get; set; }

        /// <summary>Gets or sets AprilTag maximum line-fit MSE. 获取或设置 AprilTag 最大线拟合均方误差。</summary>
        public float AprilTagMaxLineFitMse { get; set; }

        /// <summary>Gets or sets AprilTag minimum white-black difference. 获取或设置 AprilTag 最小白黑差异。</summary>
        public int AprilTagMinWhiteBlackDiff { get; set; }

        /// <summary>Gets or sets AprilTag deglitch flag. 获取或设置 AprilTag 去毛刺标志。</summary>
        public int AprilTagDeglitch { get; set; }

        /// <summary>Gets or sets whether inverted markers are detected. 获取或设置是否检测反色 marker。</summary>
        public bool DetectInvertedMarker { get; set; }

        /// <summary>Gets or sets whether ArUco3 detection is used. 获取或设置是否使用 ArUco3 检测策略。</summary>
        public bool UseAruco3Detection { get; set; }

        /// <summary>Gets or sets minimum canonical side length. 获取或设置 canonical 图像最小边长。</summary>
        public int MinSideLengthCanonicalImg { get; set; }

        /// <summary>Gets or sets minimum marker-length ratio in original image. 获取或设置原图中 marker 长度最小比例。</summary>
        public float MinMarkerLengthRatioOriginalImg { get; set; }

        /// <summary>Gets or sets valid bit-id threshold. 获取或设置有效 bit id 阈值。</summary>
        public float ValidBitIdThreshold { get; set; }

        /// <summary>Creates a copy of this parameter object. 创建此参数对象副本。</summary>
        public ArucoDetectorParameters Clone()
        {
            return new ArucoDetectorParameters(this);
        }

        internal NativeMethods.ArucoDetectorParamsNative ToNative()
        {
            return new NativeMethods.ArucoDetectorParamsNative
            {
                AdaptiveThreshWinSizeMin = AdaptiveThreshWinSizeMin,
                AdaptiveThreshWinSizeMax = AdaptiveThreshWinSizeMax,
                AdaptiveThreshWinSizeStep = AdaptiveThreshWinSizeStep,
                AdaptiveThreshConstant = AdaptiveThreshConstant,
                MinMarkerPerimeterRate = MinMarkerPerimeterRate,
                MaxMarkerPerimeterRate = MaxMarkerPerimeterRate,
                PolygonalApproxAccuracyRate = PolygonalApproxAccuracyRate,
                MinCornerDistanceRate = MinCornerDistanceRate,
                MinDistanceToBorder = MinDistanceToBorder,
                MinMarkerDistanceRate = MinMarkerDistanceRate,
                MinGroupDistance = MinGroupDistance,
                CornerRefinementMethod = (int)CornerRefinementMethod,
                CornerRefinementWinSize = CornerRefinementWinSize,
                RelativeCornerRefinementWinSize = RelativeCornerRefinementWinSize,
                CornerRefinementMaxIterations = CornerRefinementMaxIterations,
                CornerRefinementMinAccuracy = CornerRefinementMinAccuracy,
                MarkerBorderBits = MarkerBorderBits,
                PerspectiveRemovePixelPerCell = PerspectiveRemovePixelPerCell,
                PerspectiveRemoveIgnoredMarginPerCell = PerspectiveRemoveIgnoredMarginPerCell,
                MaxErroneousBitsInBorderRate = MaxErroneousBitsInBorderRate,
                MinOtsuStdDev = MinOtsuStdDev,
                ErrorCorrectionRate = ErrorCorrectionRate,
                AprilTagQuadDecimate = AprilTagQuadDecimate,
                AprilTagQuadSigma = AprilTagQuadSigma,
                AprilTagMinClusterPixels = AprilTagMinClusterPixels,
                AprilTagMaxNmaxima = AprilTagMaxNmaxima,
                AprilTagCriticalRad = AprilTagCriticalRad,
                AprilTagMaxLineFitMse = AprilTagMaxLineFitMse,
                AprilTagMinWhiteBlackDiff = AprilTagMinWhiteBlackDiff,
                AprilTagDeglitch = AprilTagDeglitch,
                DetectInvertedMarker = DetectInvertedMarker ? 1 : 0,
                UseAruco3Detection = UseAruco3Detection ? 1 : 0,
                MinSideLengthCanonicalImg = MinSideLengthCanonicalImg,
                MinMarkerLengthRatioOriginalImg = MinMarkerLengthRatioOriginalImg,
                ValidBitIdThreshold = ValidBitIdThreshold
            };
        }

        internal static ArucoDetectorParameters FromNative(NativeMethods.ArucoDetectorParamsNative native)
        {
            var result = new ArucoDetectorParameters(skipNativeDefaults: true);
            result.CopyFromNative(native);
            return result;
        }

        private ArucoDetectorParameters(bool skipNativeDefaults)
        {
            _ = skipNativeDefaults;
        }

        private void CopyFromNative(NativeMethods.ArucoDetectorParamsNative native)
        {
            AdaptiveThreshWinSizeMin = native.AdaptiveThreshWinSizeMin;
            AdaptiveThreshWinSizeMax = native.AdaptiveThreshWinSizeMax;
            AdaptiveThreshWinSizeStep = native.AdaptiveThreshWinSizeStep;
            AdaptiveThreshConstant = native.AdaptiveThreshConstant;
            MinMarkerPerimeterRate = native.MinMarkerPerimeterRate;
            MaxMarkerPerimeterRate = native.MaxMarkerPerimeterRate;
            PolygonalApproxAccuracyRate = native.PolygonalApproxAccuracyRate;
            MinCornerDistanceRate = native.MinCornerDistanceRate;
            MinDistanceToBorder = native.MinDistanceToBorder;
            MinMarkerDistanceRate = native.MinMarkerDistanceRate;
            MinGroupDistance = native.MinGroupDistance;
            CornerRefinementMethod = (CornerRefineMethod)native.CornerRefinementMethod;
            CornerRefinementWinSize = native.CornerRefinementWinSize;
            RelativeCornerRefinementWinSize = native.RelativeCornerRefinementWinSize;
            CornerRefinementMaxIterations = native.CornerRefinementMaxIterations;
            CornerRefinementMinAccuracy = native.CornerRefinementMinAccuracy;
            MarkerBorderBits = native.MarkerBorderBits;
            PerspectiveRemovePixelPerCell = native.PerspectiveRemovePixelPerCell;
            PerspectiveRemoveIgnoredMarginPerCell = native.PerspectiveRemoveIgnoredMarginPerCell;
            MaxErroneousBitsInBorderRate = native.MaxErroneousBitsInBorderRate;
            MinOtsuStdDev = native.MinOtsuStdDev;
            ErrorCorrectionRate = native.ErrorCorrectionRate;
            AprilTagQuadDecimate = native.AprilTagQuadDecimate;
            AprilTagQuadSigma = native.AprilTagQuadSigma;
            AprilTagMinClusterPixels = native.AprilTagMinClusterPixels;
            AprilTagMaxNmaxima = native.AprilTagMaxNmaxima;
            AprilTagCriticalRad = native.AprilTagCriticalRad;
            AprilTagMaxLineFitMse = native.AprilTagMaxLineFitMse;
            AprilTagMinWhiteBlackDiff = native.AprilTagMinWhiteBlackDiff;
            AprilTagDeglitch = native.AprilTagDeglitch;
            DetectInvertedMarker = native.DetectInvertedMarker != 0;
            UseAruco3Detection = native.UseAruco3Detection != 0;
            MinSideLengthCanonicalImg = native.MinSideLengthCanonicalImg;
            MinMarkerLengthRatioOriginalImg = native.MinMarkerLengthRatioOriginalImg;
            ValidBitIdThreshold = native.ValidBitIdThreshold;
        }
    }
}
