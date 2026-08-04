using System;
using System.Runtime.InteropServices;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.XImgProc
{
    /// <summary>
    /// Parameters for ximgproc EdgeDrawing.
    /// ximgproc EdgeDrawing 参数。
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct EdgeDrawingParams : IEquatable<EdgeDrawingParams>
    {
        /// <summary>Initializes EdgeDrawing parameters. 初始化 EdgeDrawing 参数。</summary>
        public EdgeDrawingParams(
            bool pfMode,
            EdgeDrawingGradientOperator edgeDetectionOperator,
            int gradientThresholdValue,
            int anchorThresholdValue,
            int scanInterval,
            int minPathLength,
            float sigma,
            bool sumFlag,
            bool nfaValidation,
            int minLineLength,
            double maxDistanceBetweenTwoLines,
            double lineFitErrorThreshold,
            double maxErrorThreshold)
        {
            PFMode = pfMode;
            EdgeDetectionOperator = edgeDetectionOperator;
            GradientThresholdValue = gradientThresholdValue;
            AnchorThresholdValue = anchorThresholdValue;
            ScanInterval = scanInterval;
            MinPathLength = minPathLength;
            Sigma = sigma;
            SumFlag = sumFlag;
            NFAValidation = nfaValidation;
            MinLineLength = minLineLength;
            MaxDistanceBetweenTwoLines = maxDistanceBetweenTwoLines;
            LineFitErrorThreshold = lineFitErrorThreshold;
            MaxErrorThreshold = maxErrorThreshold;
        }

        /// <summary>Gets or sets whether parameter-free mode is enabled. 获取或设置是否启用无参数模式。</summary>
        public bool PFMode { get; set; }

        /// <summary>Gets or sets the gradient operator. 获取或设置梯度算子。</summary>
        public EdgeDrawingGradientOperator EdgeDetectionOperator { get; set; }

        /// <summary>Gets or sets the gradient threshold. 获取或设置梯度阈值。</summary>
        public int GradientThresholdValue { get; set; }

        /// <summary>Gets or sets the anchor threshold. 获取或设置锚点阈值。</summary>
        public int AnchorThresholdValue { get; set; }

        /// <summary>Gets or sets the scan interval. 获取或设置扫描间隔。</summary>
        public int ScanInterval { get; set; }

        /// <summary>Gets or sets the minimum edge path length. 获取或设置最小边缘路径长度。</summary>
        public int MinPathLength { get; set; }

        /// <summary>Gets or sets Gaussian sigma. 获取或设置高斯 sigma。</summary>
        public float Sigma { get; set; }

        /// <summary>Gets or sets the sum flag. 获取或设置 sum 标志。</summary>
        public bool SumFlag { get; set; }

        /// <summary>Gets or sets whether NFA validation is enabled. 获取或设置是否启用 NFA 校验。</summary>
        public bool NFAValidation { get; set; }

        /// <summary>Gets or sets the minimum line length. 获取或设置最小线段长度。</summary>
        public int MinLineLength { get; set; }

        /// <summary>Gets or sets the maximum distance between two lines. 获取或设置两条线之间最大距离。</summary>
        public double MaxDistanceBetweenTwoLines { get; set; }

        /// <summary>Gets or sets line fit error threshold. 获取或设置线拟合误差阈值。</summary>
        public double LineFitErrorThreshold { get; set; }

        /// <summary>Gets or sets the maximum error threshold. 获取或设置最大误差阈值。</summary>
        public double MaxErrorThreshold { get; set; }

        internal NativeMethods.XImgProcEdgeDrawingParamsNative ToNative()
        {
            return new NativeMethods.XImgProcEdgeDrawingParamsNative
            {
                PfMode = PFMode ? 1 : 0,
                EdgeDetectionOperator = (int)EdgeDetectionOperator,
                GradientThresholdValue = GradientThresholdValue,
                AnchorThresholdValue = AnchorThresholdValue,
                ScanInterval = ScanInterval,
                MinPathLength = MinPathLength,
                Sigma = Sigma,
                SumFlag = SumFlag ? 1 : 0,
                NfaValidation = NFAValidation ? 1 : 0,
                MinLineLength = MinLineLength,
                MaxDistanceBetweenTwoLines = MaxDistanceBetweenTwoLines,
                LineFitErrorThreshold = LineFitErrorThreshold,
                MaxErrorThreshold = MaxErrorThreshold
            };
        }

        internal static EdgeDrawingParams FromNative(NativeMethods.XImgProcEdgeDrawingParamsNative native)
        {
            return new EdgeDrawingParams(
                native.PfMode != 0,
                (EdgeDrawingGradientOperator)native.EdgeDetectionOperator,
                native.GradientThresholdValue,
                native.AnchorThresholdValue,
                native.ScanInterval,
                native.MinPathLength,
                native.Sigma,
                native.SumFlag != 0,
                native.NfaValidation != 0,
                native.MinLineLength,
                native.MaxDistanceBetweenTwoLines,
                native.LineFitErrorThreshold,
                native.MaxErrorThreshold);
        }

        /// <summary>Determines whether two parameter values are equal. 判断两个参数值是否相等。</summary>
        public static bool operator ==(EdgeDrawingParams left, EdgeDrawingParams right)
        {
            return left.Equals(right);
        }

        /// <summary>Determines whether two parameter values are different. 判断两个参数值是否不同。</summary>
        public static bool operator !=(EdgeDrawingParams left, EdgeDrawingParams right)
        {
            return !left.Equals(right);
        }

        /// <inheritdoc/>
        public bool Equals(EdgeDrawingParams other)
        {
            return PFMode == other.PFMode
                && EdgeDetectionOperator == other.EdgeDetectionOperator
                && GradientThresholdValue == other.GradientThresholdValue
                && AnchorThresholdValue == other.AnchorThresholdValue
                && ScanInterval == other.ScanInterval
                && MinPathLength == other.MinPathLength
                && Sigma.Equals(other.Sigma)
                && SumFlag == other.SumFlag
                && NFAValidation == other.NFAValidation
                && MinLineLength == other.MinLineLength
                && MaxDistanceBetweenTwoLines.Equals(other.MaxDistanceBetweenTwoLines)
                && LineFitErrorThreshold.Equals(other.LineFitErrorThreshold)
                && MaxErrorThreshold.Equals(other.MaxErrorThreshold);
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is EdgeDrawingParams other && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = PFMode.GetHashCode();
                hash = (hash * 397) ^ EdgeDetectionOperator.GetHashCode();
                hash = (hash * 397) ^ GradientThresholdValue;
                hash = (hash * 397) ^ AnchorThresholdValue;
                hash = (hash * 397) ^ ScanInterval;
                hash = (hash * 397) ^ MinPathLength;
                hash = (hash * 397) ^ Sigma.GetHashCode();
                hash = (hash * 397) ^ SumFlag.GetHashCode();
                hash = (hash * 397) ^ NFAValidation.GetHashCode();
                hash = (hash * 397) ^ MinLineLength;
                hash = (hash * 397) ^ MaxDistanceBetweenTwoLines.GetHashCode();
                hash = (hash * 397) ^ LineFitErrorThreshold.GetHashCode();
                hash = (hash * 397) ^ MaxErrorThreshold.GetHashCode();
                return hash;
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Format(
                System.Globalization.CultureInfo.InvariantCulture,
                "EdgeDrawingParams(PFMode={0}, EdgeDetectionOperator={1}, GradientThresholdValue={2}, AnchorThresholdValue={3}, ScanInterval={4}, MinPathLength={5}, Sigma={6}, SumFlag={7}, NFAValidation={8}, MinLineLength={9}, MaxDistanceBetweenTwoLines={10}, LineFitErrorThreshold={11}, MaxErrorThreshold={12})",
                PFMode,
                EdgeDetectionOperator,
                GradientThresholdValue,
                AnchorThresholdValue,
                ScanInterval,
                MinPathLength,
                Sigma,
                SumFlag,
                NFAValidation,
                MinLineLength,
                MaxDistanceBetweenTwoLines,
                LineFitErrorThreshold,
                MaxErrorThreshold);
        }
    }
}
