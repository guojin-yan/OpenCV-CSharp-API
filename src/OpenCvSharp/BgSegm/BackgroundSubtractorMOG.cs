using System;
using System.Globalization;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.BgSegm
{
    /// <summary>
    /// Contrib MOG background subtractor.
    /// contrib MOG 背景减除器。
    /// </summary>
    public sealed class BackgroundSubtractorMOG : BgSegmBackgroundSubtractor
    {
        private const int IntHistory = 0;
        private const int IntNMixtures = 1;
        private const int DoubleBackgroundRatio = 0;
        private const int DoubleNoiseSigma = 1;

        private BackgroundSubtractorMOG(IntPtr nativeHandle)
            : base(nativeHandle)
        {
        }

        /// <summary>Gets or sets history length. 获取或设置历史长度。</summary>
        public int History { get { return GetInt(IntHistory); } set { SetInt(IntHistory, value); } }

        /// <summary>Gets or sets mixture count. 获取或设置混合数量。</summary>
        public int NMixtures { get { return GetInt(IntNMixtures); } set { SetInt(IntNMixtures, value); } }

        /// <summary>Gets or sets background ratio. 获取或设置背景比例。</summary>
        public double BackgroundRatio { get { return GetDouble(DoubleBackgroundRatio); } set { SetDouble(DoubleBackgroundRatio, value); } }

        /// <summary>Gets or sets noise sigma. 获取或设置噪声 sigma。</summary>
        public double NoiseSigma { get { return GetDouble(DoubleNoiseSigma); } set { SetDouble(DoubleNoiseSigma, value); } }

        /// <summary>Creates a MOG background subtractor. 创建 MOG 背景减除器。</summary>
        public static BackgroundSubtractorMOG Create(int history = 200, int nmixtures = 5, double backgroundRatio = 0.7, double noiseSigma = 0.0)
        {
            NativeException.ThrowIfError(NativeMethods.BgSegmBackgroundSubtractorMOGCreate(history, nmixtures, backgroundRatio, noiseSigma, out IntPtr nativeHandle));
            return new BackgroundSubtractorMOG(nativeHandle);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return IsDisposed
                ? "{Disposed=True}"
                : "{History=" + History
                    + ",NMixtures=" + NMixtures
                    + ",BackgroundRatio=" + BackgroundRatio.ToString(CultureInfo.InvariantCulture)
                    + ",NoiseSigma=" + NoiseSigma.ToString(CultureInfo.InvariantCulture) + "}";
        }

        private int GetInt(int propertyId)
        {
            NativeException.ThrowIfError(NativeMethods.BgSegmBackgroundSubtractorMOGGetInt(NativeHandle, propertyId, out int value));
            return value;
        }

        private void SetInt(int propertyId, int value)
        {
            NativeException.ThrowIfError(NativeMethods.BgSegmBackgroundSubtractorMOGSetInt(NativeHandle, propertyId, value));
        }

        private double GetDouble(int propertyId)
        {
            NativeException.ThrowIfError(NativeMethods.BgSegmBackgroundSubtractorMOGGetDouble(NativeHandle, propertyId, out double value));
            return value;
        }

        private void SetDouble(int propertyId, double value)
        {
            NativeException.ThrowIfError(NativeMethods.BgSegmBackgroundSubtractorMOGSetDouble(NativeHandle, propertyId, value));
        }

        /// <inheritdoc/>
        protected override void ValidateApplyImage(Mat image)
        {
            int channels = image.Channels;
            if (image.Depth != MatType.CV_8U || (channels != 1 && channels != 3))
            {
                throw new ArgumentException("BackgroundSubtractorMOG requires an 8-bit single-channel or three-channel image.", nameof(image));
            }
        }
    }
}
