using System;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Video
{
    /// <summary>
    /// Gaussian-mixture background subtractor compatible with OpenCV <c>cv::BackgroundSubtractorMOG2</c>.
    /// 与 OpenCV <c>cv::BackgroundSubtractorMOG2</c> 兼容的高斯混合背景减除器。
    /// </summary>
    public sealed class BackgroundSubtractorMOG2 : BackgroundSubtractor
    {
        private const int ShadowValueProperty = 0;
        private const int BackgroundRatioProperty = 0;
        private const int VarThresholdProperty = 1;
        private const int VarThresholdGenProperty = 2;
        private const int VarInitProperty = 3;
        private const int VarMinProperty = 4;
        private const int VarMaxProperty = 5;
        private const int ComplexityReductionThresholdProperty = 6;
        private const int ShadowThresholdProperty = 7;

        /// <summary>
        /// Creates a MOG2 background subtractor.
        /// 创建 MOG2 背景减除器。
        /// </summary>
        public BackgroundSubtractorMOG2(int history = 500, double varThreshold = 16.0, bool detectShadows = true)
            : base(CreateNative(history, varThreshold, detectShadows))
        {
        }

        /// <summary>
        /// Creates a MOG2 background subtractor.
        /// 创建 MOG2 背景减除器。
        /// </summary>
        public static BackgroundSubtractorMOG2 Create(int history = 500, double varThreshold = 16.0, bool detectShadows = true)
        {
            return new BackgroundSubtractorMOG2(history, varThreshold, detectShadows);
        }

        /// <summary>Gets or sets the history length. 获取或设置历史帧长度。</summary>
        public int History
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.BackgroundSubtractorMOG2GetHistory(NativeHandle, out int value));
                return value;
            }
            set
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.BackgroundSubtractorMOG2SetHistory(NativeHandle, value));
            }
        }

        /// <summary>Gets or sets the number of Gaussian mixtures. 获取或设置高斯混合数量。</summary>
        public int NMixtures
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.BackgroundSubtractorMOG2GetNMixtures(NativeHandle, out int value));
                return value;
            }
            set
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.BackgroundSubtractorMOG2SetNMixtures(NativeHandle, value));
            }
        }

        /// <summary>Gets or sets whether shadow detection is enabled. 获取或设置是否启用阴影检测。</summary>
        public bool DetectShadows
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.BackgroundSubtractorMOG2GetDetectShadows(NativeHandle, out int value));
                return value != 0;
            }
            set
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.BackgroundSubtractorMOG2SetDetectShadows(NativeHandle, value ? 1 : 0));
            }
        }

        /// <summary>Gets or sets the pixel value used for shadows. 获取或设置阴影像素值。</summary>
        public int ShadowValue
        {
            get { return GetIntProperty(ShadowValueProperty); }
            set { SetIntProperty(ShadowValueProperty, value); }
        }

        /// <summary>Gets or sets the background ratio threshold. 获取或设置背景比例阈值。</summary>
        public double BackgroundRatio
        {
            get { return GetDoubleProperty(BackgroundRatioProperty); }
            set { SetDoubleProperty(BackgroundRatioProperty, value); }
        }

        /// <summary>Gets or sets the main variance threshold. 获取或设置主方差阈值。</summary>
        public double VarThreshold
        {
            get { return GetDoubleProperty(VarThresholdProperty); }
            set { SetDoubleProperty(VarThresholdProperty, value); }
        }

        /// <summary>Gets or sets the variance threshold for new component generation. 获取或设置生成新分量的方差阈值。</summary>
        public double VarThresholdGen
        {
            get { return GetDoubleProperty(VarThresholdGenProperty); }
            set { SetDoubleProperty(VarThresholdGenProperty, value); }
        }

        /// <summary>Gets or sets the initial variance for new components. 获取或设置新分量的初始方差。</summary>
        public double VarInit
        {
            get { return GetDoubleProperty(VarInitProperty); }
            set { SetDoubleProperty(VarInitProperty, value); }
        }

        /// <summary>Gets or sets the minimum component variance. 获取或设置分量最小方差。</summary>
        public double VarMin
        {
            get { return GetDoubleProperty(VarMinProperty); }
            set { SetDoubleProperty(VarMinProperty, value); }
        }

        /// <summary>Gets or sets the maximum component variance. 获取或设置分量最大方差。</summary>
        public double VarMax
        {
            get { return GetDoubleProperty(VarMaxProperty); }
            set { SetDoubleProperty(VarMaxProperty, value); }
        }

        /// <summary>Gets or sets the complexity reduction threshold. 获取或设置复杂度削减阈值。</summary>
        public double ComplexityReductionThreshold
        {
            get { return GetDoubleProperty(ComplexityReductionThresholdProperty); }
            set { SetDoubleProperty(ComplexityReductionThresholdProperty, value); }
        }

        /// <summary>Gets or sets the shadow threshold. 获取或设置阴影阈值。</summary>
        public double ShadowThreshold
        {
            get { return GetDoubleProperty(ShadowThresholdProperty); }
            set { SetDoubleProperty(ShadowThresholdProperty, value); }
        }

        private static IntPtr CreateNative(int history, double varThreshold, bool detectShadows)
        {
            NativeException.ThrowIfError(NativeMethods.BackgroundSubtractorMOG2Create(history, varThreshold, detectShadows ? 1 : 0, out IntPtr nativeHandle));
            return nativeHandle;
        }

        private int GetIntProperty(int propertyId)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.BackgroundSubtractorMOG2GetIntProperty(NativeHandle, propertyId, out int value));
            return value;
        }

        private void SetIntProperty(int propertyId, int value)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.BackgroundSubtractorMOG2SetIntProperty(NativeHandle, propertyId, value));
        }

        private double GetDoubleProperty(int propertyId)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.BackgroundSubtractorMOG2GetDoubleProperty(NativeHandle, propertyId, out double value));
            return value;
        }

        private void SetDoubleProperty(int propertyId, double value)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.BackgroundSubtractorMOG2SetDoubleProperty(NativeHandle, propertyId, value));
        }
    }
}
