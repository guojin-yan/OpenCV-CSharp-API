using System;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Video
{
    /// <summary>
    /// K-nearest-neighbor background subtractor compatible with OpenCV <c>cv::BackgroundSubtractorKNN</c>.
    /// 与 OpenCV <c>cv::BackgroundSubtractorKNN</c> 兼容的 KNN 背景减除器。
    /// </summary>
    public sealed class BackgroundSubtractorKNN : BackgroundSubtractor
    {
        private const int ShadowValueProperty = 0;
        private const int KnnSamplesProperty = 1;
        private const int Dist2ThresholdProperty = 0;
        private const int ShadowThresholdProperty = 1;

        /// <summary>
        /// Creates a KNN background subtractor.
        /// 创建 KNN 背景减除器。
        /// </summary>
        public BackgroundSubtractorKNN(int history = 500, double dist2Threshold = 400.0, bool detectShadows = true)
            : base(CreateNative(history, dist2Threshold, detectShadows))
        {
        }

        /// <summary>
        /// Creates a KNN background subtractor.
        /// 创建 KNN 背景减除器。
        /// </summary>
        public static BackgroundSubtractorKNN Create(int history = 500, double dist2Threshold = 400.0, bool detectShadows = true)
        {
            return new BackgroundSubtractorKNN(history, dist2Threshold, detectShadows);
        }

        /// <summary>Gets or sets the history length. 获取或设置历史帧长度。</summary>
        public int History
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.BackgroundSubtractorKNNGetHistory(NativeHandle, out int value));
                return value;
            }
            set
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.BackgroundSubtractorKNNSetHistory(NativeHandle, value));
            }
        }

        /// <summary>Gets or sets the number of remembered samples. 获取或设置保留样本数量。</summary>
        public int NSamples
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.BackgroundSubtractorKNNGetNSamples(NativeHandle, out int value));
                return value;
            }
            set
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.BackgroundSubtractorKNNSetNSamples(NativeHandle, value));
            }
        }

        /// <summary>Gets or sets whether shadow detection is enabled. 获取或设置是否启用阴影检测。</summary>
        public bool DetectShadows
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.BackgroundSubtractorKNNGetDetectShadows(NativeHandle, out int value));
                return value != 0;
            }
            set
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.BackgroundSubtractorKNNSetDetectShadows(NativeHandle, value ? 1 : 0));
            }
        }

        /// <summary>Gets or sets the pixel value used for shadows. 获取或设置阴影像素值。</summary>
        public int ShadowValue
        {
            get { return GetIntProperty(ShadowValueProperty); }
            set { SetIntProperty(ShadowValueProperty, value); }
        }

        /// <summary>Gets or sets the number of KNN samples used for classification. 获取或设置用于分类的 KNN 样本数量。</summary>
        public int KnnSamples
        {
            get { return GetIntProperty(KnnSamplesProperty); }
            set { SetIntProperty(KnnSamplesProperty, value); }
        }

        /// <summary>Gets or sets the squared-distance threshold. 获取或设置平方距离阈值。</summary>
        public double Dist2Threshold
        {
            get { return GetDoubleProperty(Dist2ThresholdProperty); }
            set { SetDoubleProperty(Dist2ThresholdProperty, value); }
        }

        /// <summary>Gets or sets the shadow threshold. 获取或设置阴影阈值。</summary>
        public double ShadowThreshold
        {
            get { return GetDoubleProperty(ShadowThresholdProperty); }
            set { SetDoubleProperty(ShadowThresholdProperty, value); }
        }

        private static IntPtr CreateNative(int history, double dist2Threshold, bool detectShadows)
        {
            NativeException.ThrowIfError(NativeMethods.BackgroundSubtractorKNNCreate(history, dist2Threshold, detectShadows ? 1 : 0, out IntPtr nativeHandle));
            return nativeHandle;
        }

        private int GetIntProperty(int propertyId)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.BackgroundSubtractorKNNGetIntProperty(NativeHandle, propertyId, out int value));
            return value;
        }

        private void SetIntProperty(int propertyId, int value)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.BackgroundSubtractorKNNSetIntProperty(NativeHandle, propertyId, value));
        }

        private double GetDoubleProperty(int propertyId)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.BackgroundSubtractorKNNGetDoubleProperty(NativeHandle, propertyId, out double value));
            return value;
        }

        private void SetDoubleProperty(int propertyId, double value)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.BackgroundSubtractorKNNSetDoubleProperty(NativeHandle, propertyId, value));
        }
    }
}
