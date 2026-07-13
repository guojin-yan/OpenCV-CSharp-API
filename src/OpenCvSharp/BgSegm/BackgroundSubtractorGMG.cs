using System;
using System.Globalization;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.BgSegm
{
    /// <summary>
    /// Contrib GMG background subtractor.
    /// contrib GMG 背景减除器。
    /// </summary>
    public sealed class BackgroundSubtractorGMG : BgSegmBackgroundSubtractor
    {
        private const int IntMaxFeatures = 0;
        private const int IntNumFrames = 1;
        private const int IntQuantizationLevels = 2;
        private const int IntSmoothingRadius = 3;
        private const int IntUpdateBackgroundModel = 4;

        private const int DoubleDefaultLearningRate = 0;
        private const int DoubleBackgroundPrior = 1;
        private const int DoubleDecisionThreshold = 2;
        private const int DoubleMinVal = 3;
        private const int DoubleMaxVal = 4;

        private BackgroundSubtractorGMG(IntPtr nativeHandle)
            : base(nativeHandle)
        {
        }

        /// <summary>Gets or sets max features. 获取或设置最大特征数。</summary>
        public int MaxFeatures
        {
            get { return GetInt(IntMaxFeatures); }
            set
            {
                ThrowIfDisposed();
                ValidatePositive(value, nameof(value));
                SetInt(IntMaxFeatures, value);
            }
        }

        /// <summary>Gets or sets default learning rate. 获取或设置默认学习率。</summary>
        public double DefaultLearningRate
        {
            get { return GetDouble(DoubleDefaultLearningRate); }
            set
            {
                ThrowIfDisposed();
                ValidateUnitInterval(value, nameof(value));
                SetDouble(DoubleDefaultLearningRate, value);
            }
        }

        /// <summary>Gets or sets initialization frame count. 获取或设置初始化帧数。</summary>
        public int NumFrames
        {
            get { return GetInt(IntNumFrames); }
            set
            {
                ThrowIfDisposed();
                ValidatePositive(value, nameof(value));
                SetInt(IntNumFrames, value);
            }
        }

        /// <summary>Gets or sets quantization levels. 获取或设置量化层级。</summary>
        public int QuantizationLevels
        {
            get { return GetInt(IntQuantizationLevels); }
            set
            {
                ThrowIfDisposed();
                ValidateQuantizationLevels(value, nameof(value));
                SetInt(IntQuantizationLevels, value);
            }
        }

        /// <summary>Gets or sets background prior. 获取或设置背景先验概率。</summary>
        public double BackgroundPrior
        {
            get { return GetDouble(DoubleBackgroundPrior); }
            set
            {
                ThrowIfDisposed();
                ValidateUnitInterval(value, nameof(value));
                SetDouble(DoubleBackgroundPrior, value);
            }
        }

        /// <summary>Gets or sets smoothing radius. 获取或设置平滑半径。</summary>
        public int SmoothingRadius { get { return GetInt(IntSmoothingRadius); } set { SetInt(IntSmoothingRadius, value); } }

        /// <summary>Gets or sets decision threshold. 获取或设置决策阈值。</summary>
        public double DecisionThreshold { get { return GetDouble(DoubleDecisionThreshold); } set { SetDouble(DoubleDecisionThreshold, value); } }

        /// <summary>Gets or sets whether the background model is updated. 获取或设置是否更新背景模型。</summary>
        public bool UpdateBackgroundModel { get { return GetInt(IntUpdateBackgroundModel) != 0; } set { SetInt(IntUpdateBackgroundModel, value ? 1 : 0); } }

        /// <summary>Gets or sets min value. 获取或设置最小值。</summary>
        public double MinVal { get { return GetDouble(DoubleMinVal); } set { SetDouble(DoubleMinVal, value); } }

        /// <summary>Gets or sets max value. 获取或设置最大值。</summary>
        public double MaxVal { get { return GetDouble(DoubleMaxVal); } set { SetDouble(DoubleMaxVal, value); } }

        /// <summary>Creates a GMG background subtractor. 创建 GMG 背景减除器。</summary>
        public static BackgroundSubtractorGMG Create(int initializationFrames = 120, double decisionThreshold = 0.8)
        {
            ValidatePositive(initializationFrames, nameof(initializationFrames));
            NativeException.ThrowIfError(NativeMethods.BgSegmBackgroundSubtractorGMGCreate(initializationFrames, decisionThreshold, out IntPtr nativeHandle));
            return new BackgroundSubtractorGMG(nativeHandle);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return IsDisposed
                ? "{Disposed=True}"
                : "{MaxFeatures=" + MaxFeatures
                    + ",NumFrames=" + NumFrames
                    + ",BackgroundPrior=" + BackgroundPrior.ToString(CultureInfo.InvariantCulture)
                    + ",DecisionThreshold=" + DecisionThreshold.ToString(CultureInfo.InvariantCulture)
                    + ",MinVal=" + MinVal.ToString(CultureInfo.InvariantCulture)
                    + ",MaxVal=" + MaxVal.ToString(CultureInfo.InvariantCulture)
                    + ",UpdateBackgroundModel=" + UpdateBackgroundModel + "}";
        }

        private int GetInt(int propertyId)
        {
            NativeException.ThrowIfError(NativeMethods.BgSegmBackgroundSubtractorGMGGetInt(NativeHandle, propertyId, out int value));
            return value;
        }

        private void SetInt(int propertyId, int value)
        {
            NativeException.ThrowIfError(NativeMethods.BgSegmBackgroundSubtractorGMGSetInt(NativeHandle, propertyId, value));
        }

        private double GetDouble(int propertyId)
        {
            NativeException.ThrowIfError(NativeMethods.BgSegmBackgroundSubtractorGMGGetDouble(NativeHandle, propertyId, out double value));
            return value;
        }

        private void SetDouble(int propertyId, double value)
        {
            NativeException.ThrowIfError(NativeMethods.BgSegmBackgroundSubtractorGMGSetDouble(NativeHandle, propertyId, value));
        }

        private static void ValidatePositive(int value, string parameterName)
        {
            if (value < 1)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Value must be greater than zero.");
            }
        }

        private static void ValidateQuantizationLevels(int value, string parameterName)
        {
            if (value < 1 || value > 255)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Quantization levels must be between 1 and 255.");
            }
        }

        private static void ValidateUnitInterval(double value, string parameterName)
        {
            if (double.IsNaN(value) || value < 0.0 || value > 1.0)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Value must be between 0.0 and 1.0.");
            }
        }
    }
}
