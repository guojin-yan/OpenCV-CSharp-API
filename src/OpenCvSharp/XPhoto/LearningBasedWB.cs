using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.XPhoto
{
    /// <summary>
    /// Learning-based white balancer.
    /// 基于学习的白平衡器。
    /// </summary>
    public sealed class LearningBasedWB : WhiteBalancer
    {
        private const int PropertyRangeMaxVal = 0;
        private const int PropertyHistBinNum = 1;

        /// <summary>Creates a learning-based white balancer. 创建基于学习的白平衡器。</summary>
        public LearningBasedWB(string? modelPath = null)
            : base(CreateHandle(modelPath))
        {
        }

        /// <summary>Gets or sets the maximum input range value. 获取或设置输入范围最大值。</summary>
        public int RangeMaxVal
        {
            get { return GetIntProperty(PropertyRangeMaxVal); }
            set { SetIntProperty(PropertyRangeMaxVal, value); }
        }

        /// <summary>Gets or sets the histogram bin count. 获取或设置直方图 bin 数。</summary>
        public int HistBinNum
        {
            get { return GetIntProperty(PropertyHistBinNum); }
            set { SetIntProperty(PropertyHistBinNum, value); }
        }

        /// <summary>Gets or sets the saturation threshold. 获取或设置饱和度阈值。</summary>
        public float SaturationThreshold
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.XPhotoLearningBasedWBGetSaturationThreshold(NativeHandle, out float value));
                return value;
            }

            set
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.XPhotoLearningBasedWBSetSaturationThreshold(NativeHandle, value));
            }
        }

        /// <summary>Creates a learning-based white balancer. 创建基于学习的白平衡器。</summary>
        public static LearningBasedWB Create(string? modelPath = null)
        {
            return new LearningBasedWB(modelPath);
        }

        /// <summary>
        /// Extracts simple learning-based white-balance features.
        /// 提取基于学习白平衡的简单特征。
        /// </summary>
        public void ExtractSimpleFeatures(Mat src, Mat dst)
        {
            ThrowIfDisposed();
            ValidateNotNull(src, nameof(src));
            ValidateNotNull(dst, nameof(dst));
            ValidateColorSource(src, nameof(src), "LearningBasedWB simple feature extraction");
            NativeException.ThrowIfError(NativeMethods.XPhotoLearningBasedWBExtractSimpleFeatures(NativeHandle, src.NativeHandle, dst.NativeHandle));
        }

        /// <summary>
        /// Extracts simple features and returns a new matrix.
        /// 提取简单特征并返回新矩阵。
        /// </summary>
        public Mat ExtractSimpleFeatures(Mat src)
        {
            var dst = new Mat();
            try
            {
                ExtractSimpleFeatures(src, dst);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        private int GetIntProperty(int propertyId)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.XPhotoLearningBasedWBGetIntProperty(NativeHandle, propertyId, out int value));
            return value;
        }

        private void SetIntProperty(int propertyId, int value)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.XPhotoLearningBasedWBSetIntProperty(NativeHandle, propertyId, value));
        }

        /// <inheritdoc />
        protected override void ValidateBalanceWhiteSource(Mat src)
        {
            ValidateColorSource(src, nameof(src), "LearningBasedWB");
        }

        private static void ValidateColorSource(Mat src, string parameterName, string operationName)
        {
            if (src.Empty)
            {
                throw new ArgumentException(operationName + " requires a non-empty source image.", parameterName);
            }

            if (!src.IsContinuous)
            {
                throw new ArgumentException(operationName + " requires a continuous source image.", parameterName);
            }

            if (src.Type != MatType.CV_8UC3 && src.Type != MatType.CV_16UC3)
            {
                throw new ArgumentException(operationName + " requires a CV_8UC3 or CV_16UC3 source image.", parameterName);
            }
        }

        private static NativeWhiteBalancerHandle CreateHandle(string? modelPath)
        {
            byte[] path = XPhotoStringConvert.ToOptionalNullTerminatedUtf8(modelPath);
            NativeException.ThrowIfError(NativeMethods.XPhotoLearningBasedWBCreate(path, out IntPtr nativeHandle));
            return NativeWhiteBalancerHandle.FromNativePointer(nativeHandle);
        }
    }
}
