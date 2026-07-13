using System;
using System.Globalization;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.ObjDetect
{
    /// <summary>
    /// Parameters for MCC color checker detection.
    /// MCC 色卡检测参数。
    /// </summary>
    public sealed class DetectorParametersMCC
    {
        /// <summary>
        /// Initializes parameters with OpenCV defaults.
        /// 使用 OpenCV 默认值初始化参数。
        /// </summary>
        public DetectorParametersMCC()
        {
            NativeException.ThrowIfError(NativeMethods.MccDetectorDefaultParams(out NativeMethods.MccDetectorParamsNative native));
            CopyFromNative(native);
        }

        /// <summary>
        /// Initializes parameters with scalar values.
        /// 使用标量值初始化参数。
        /// </summary>
        public DetectorParametersMCC(
            int adaptiveThreshWinSizeMin,
            int adaptiveThreshWinSizeMax,
            int adaptiveThreshWinSizeStep,
            double adaptiveThreshConstant,
            double minContoursAreaRate,
            double minContoursArea,
            double confidenceThreshold,
            double minContourSolidity,
            double findCandidatesApproxPolyDPEpsMultiplier,
            int borderWidth,
            float b0Factor,
            float maxError,
            int minContourPointsAllowed,
            int minContourLengthAllowed,
            int minInterContourDistance,
            int minInterCheckerDistance,
            int minImageSize,
            int minGroupSize)
        {
            AdaptiveThreshWinSizeMin = adaptiveThreshWinSizeMin;
            AdaptiveThreshWinSizeMax = adaptiveThreshWinSizeMax;
            AdaptiveThreshWinSizeStep = adaptiveThreshWinSizeStep;
            AdaptiveThreshConstant = adaptiveThreshConstant;
            MinContoursAreaRate = minContoursAreaRate;
            MinContoursArea = minContoursArea;
            ConfidenceThreshold = confidenceThreshold;
            MinContourSolidity = minContourSolidity;
            FindCandidatesApproxPolyDPEpsMultiplier = findCandidatesApproxPolyDPEpsMultiplier;
            BorderWidth = borderWidth;
            B0Factor = b0Factor;
            MaxError = maxError;
            MinContourPointsAllowed = minContourPointsAllowed;
            MinContourLengthAllowed = minContourLengthAllowed;
            MinInterContourDistance = minInterContourDistance;
            MinInterCheckerDistance = minInterCheckerDistance;
            MinImageSize = minImageSize;
            MinGroupSize = minGroupSize;
        }

        /// <summary>
        /// Initializes parameters by copying another instance.
        /// 通过复制另一个实例初始化参数。
        /// </summary>
        /// <param name="other">The parameters to copy. 要复制的参数。</param>
        public DetectorParametersMCC(DetectorParametersMCC other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            AdaptiveThreshWinSizeMin = other.AdaptiveThreshWinSizeMin;
            AdaptiveThreshWinSizeMax = other.AdaptiveThreshWinSizeMax;
            AdaptiveThreshWinSizeStep = other.AdaptiveThreshWinSizeStep;
            AdaptiveThreshConstant = other.AdaptiveThreshConstant;
            MinContoursAreaRate = other.MinContoursAreaRate;
            MinContoursArea = other.MinContoursArea;
            ConfidenceThreshold = other.ConfidenceThreshold;
            MinContourSolidity = other.MinContourSolidity;
            FindCandidatesApproxPolyDPEpsMultiplier = other.FindCandidatesApproxPolyDPEpsMultiplier;
            BorderWidth = other.BorderWidth;
            B0Factor = other.B0Factor;
            MaxError = other.MaxError;
            MinContourPointsAllowed = other.MinContourPointsAllowed;
            MinContourLengthAllowed = other.MinContourLengthAllowed;
            MinInterContourDistance = other.MinInterContourDistance;
            MinInterCheckerDistance = other.MinInterCheckerDistance;
            MinImageSize = other.MinImageSize;
            MinGroupSize = other.MinGroupSize;
        }

        /// <summary>Gets or sets minimum adaptive threshold window size. 获取或设置自适应阈值最小窗口。</summary>
        public int AdaptiveThreshWinSizeMin { get; set; }

        /// <summary>Gets or sets maximum adaptive threshold window size. 获取或设置自适应阈值最大窗口。</summary>
        public int AdaptiveThreshWinSizeMax { get; set; }

        /// <summary>Gets or sets adaptive threshold window step. 获取或设置自适应阈值窗口步长。</summary>
        public int AdaptiveThreshWinSizeStep { get; set; }

        /// <summary>Gets or sets adaptive threshold constant. 获取或设置自适应阈值常量。</summary>
        public double AdaptiveThreshConstant { get; set; }

        /// <summary>Gets or sets minimum contour area rate. 获取或设置最小轮廓面积比例。</summary>
        public double MinContoursAreaRate { get; set; }

        /// <summary>Gets or sets minimum contour area. 获取或设置最小轮廓面积。</summary>
        public double MinContoursArea { get; set; }

        /// <summary>Gets or sets confidence threshold. 获取或设置信心阈值。</summary>
        public double ConfidenceThreshold { get; set; }

        /// <summary>Gets or sets minimum contour solidity. 获取或设置最小轮廓实心度。</summary>
        public double MinContourSolidity { get; set; }

        /// <summary>Gets or sets polygon approximation epsilon multiplier. 获取或设置多边形逼近 epsilon 乘子。</summary>
        public double FindCandidatesApproxPolyDPEpsMultiplier { get; set; }

        /// <summary>Gets or sets border width. 获取或设置边框宽度。</summary>
        public int BorderWidth { get; set; }

        /// <summary>Gets or sets B0 factor. 获取或设置 B0 因子。</summary>
        public float B0Factor { get; set; }

        /// <summary>Gets or sets maximum detection error. 获取或设置最大检测误差。</summary>
        public float MaxError { get; set; }

        /// <summary>Gets or sets minimum contour point count. 获取或设置最小轮廓点数。</summary>
        public int MinContourPointsAllowed { get; set; }

        /// <summary>Gets or sets minimum contour length. 获取或设置最小轮廓长度。</summary>
        public int MinContourLengthAllowed { get; set; }

        /// <summary>Gets or sets minimum inter-contour distance. 获取或设置轮廓间最小距离。</summary>
        public int MinInterContourDistance { get; set; }

        /// <summary>Gets or sets minimum inter-checker distance. 获取或设置色卡间最小距离。</summary>
        public int MinInterCheckerDistance { get; set; }

        /// <summary>Gets or sets minimum image size. 获取或设置最小图像尺寸。</summary>
        public int MinImageSize { get; set; }

        /// <summary>Gets or sets minimum group size. 获取或设置最小分组大小。</summary>
        public int MinGroupSize { get; set; }

        /// <summary>
        /// Creates a copy of this parameter object.
        /// 创建此参数对象的副本。
        /// </summary>
        public DetectorParametersMCC Clone()
        {
            return new DetectorParametersMCC(this);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return
                $"{nameof(DetectorParametersMCC)}(" +
                $"{nameof(AdaptiveThreshWinSizeMin)}={AdaptiveThreshWinSizeMin}, " +
                $"{nameof(AdaptiveThreshWinSizeMax)}={AdaptiveThreshWinSizeMax}, " +
                $"{nameof(AdaptiveThreshWinSizeStep)}={AdaptiveThreshWinSizeStep}, " +
                $"{nameof(AdaptiveThreshConstant)}={AdaptiveThreshConstant.ToString(CultureInfo.InvariantCulture)}, " +
                $"{nameof(MinContoursAreaRate)}={MinContoursAreaRate.ToString(CultureInfo.InvariantCulture)}, " +
                $"{nameof(MinContoursArea)}={MinContoursArea.ToString(CultureInfo.InvariantCulture)}, " +
                $"{nameof(ConfidenceThreshold)}={ConfidenceThreshold.ToString(CultureInfo.InvariantCulture)}, " +
                $"{nameof(MinContourSolidity)}={MinContourSolidity.ToString(CultureInfo.InvariantCulture)}, " +
                $"{nameof(FindCandidatesApproxPolyDPEpsMultiplier)}={FindCandidatesApproxPolyDPEpsMultiplier.ToString(CultureInfo.InvariantCulture)}, " +
                $"{nameof(BorderWidth)}={BorderWidth}, " +
                $"{nameof(B0Factor)}={B0Factor.ToString(CultureInfo.InvariantCulture)}, " +
                $"{nameof(MaxError)}={MaxError.ToString(CultureInfo.InvariantCulture)}, " +
                $"{nameof(MinContourPointsAllowed)}={MinContourPointsAllowed}, " +
                $"{nameof(MinContourLengthAllowed)}={MinContourLengthAllowed}, " +
                $"{nameof(MinInterContourDistance)}={MinInterContourDistance}, " +
                $"{nameof(MinInterCheckerDistance)}={MinInterCheckerDistance}, " +
                $"{nameof(MinImageSize)}={MinImageSize}, " +
                $"{nameof(MinGroupSize)}={MinGroupSize})";
        }

        internal static DetectorParametersMCC FromNative(NativeMethods.MccDetectorParamsNative native)
        {
            var result = new DetectorParametersMCC(skipNativeDefaults: true);
            result.CopyFromNative(native);
            return result;
        }

        internal NativeMethods.MccDetectorParamsNative ToNative()
        {
            return new NativeMethods.MccDetectorParamsNative
            {
                AdaptiveThreshWinSizeMin = AdaptiveThreshWinSizeMin,
                AdaptiveThreshWinSizeMax = AdaptiveThreshWinSizeMax,
                AdaptiveThreshWinSizeStep = AdaptiveThreshWinSizeStep,
                AdaptiveThreshConstant = AdaptiveThreshConstant,
                MinContoursAreaRate = MinContoursAreaRate,
                MinContoursArea = MinContoursArea,
                ConfidenceThreshold = ConfidenceThreshold,
                MinContourSolidity = MinContourSolidity,
                FindCandidatesApproxPolyDPEpsMultiplier = FindCandidatesApproxPolyDPEpsMultiplier,
                BorderWidth = BorderWidth,
                B0Factor = B0Factor,
                MaxError = MaxError,
                MinContourPointsAllowed = MinContourPointsAllowed,
                MinContourLengthAllowed = MinContourLengthAllowed,
                MinInterContourDistance = MinInterContourDistance,
                MinInterCheckerDistance = MinInterCheckerDistance,
                MinImageSize = MinImageSize,
                MinGroupSize = MinGroupSize
            };
        }

        private DetectorParametersMCC(bool skipNativeDefaults)
        {
            _ = skipNativeDefaults;
        }

        private void CopyFromNative(NativeMethods.MccDetectorParamsNative native)
        {
            AdaptiveThreshWinSizeMin = native.AdaptiveThreshWinSizeMin;
            AdaptiveThreshWinSizeMax = native.AdaptiveThreshWinSizeMax;
            AdaptiveThreshWinSizeStep = native.AdaptiveThreshWinSizeStep;
            AdaptiveThreshConstant = native.AdaptiveThreshConstant;
            MinContoursAreaRate = native.MinContoursAreaRate;
            MinContoursArea = native.MinContoursArea;
            ConfidenceThreshold = native.ConfidenceThreshold;
            MinContourSolidity = native.MinContourSolidity;
            FindCandidatesApproxPolyDPEpsMultiplier = native.FindCandidatesApproxPolyDPEpsMultiplier;
            BorderWidth = native.BorderWidth;
            B0Factor = native.B0Factor;
            MaxError = native.MaxError;
            MinContourPointsAllowed = native.MinContourPointsAllowed;
            MinContourLengthAllowed = native.MinContourLengthAllowed;
            MinInterContourDistance = native.MinInterContourDistance;
            MinInterCheckerDistance = native.MinInterCheckerDistance;
            MinImageSize = native.MinImageSize;
            MinGroupSize = native.MinGroupSize;
        }
    }
}
