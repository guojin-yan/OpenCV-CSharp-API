using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.XPhoto
{
    /// <summary>
    /// Simple percentile-based white balancer.
    /// 基于百分位的简单白平衡器。
    /// </summary>
    public sealed class SimpleWB : WhiteBalancer
    {
        private const int PropertyInputMin = 0;
        private const int PropertyInputMax = 1;
        private const int PropertyOutputMin = 2;
        private const int PropertyOutputMax = 3;
        private const int PropertyP = 4;

        /// <summary>Creates a simple white balancer. 创建简单白平衡器。</summary>
        public SimpleWB()
            : base(CreateHandle())
        {
        }

        /// <summary>Gets or sets the input minimum value. 获取或设置输入最小值。</summary>
        public float InputMin
        {
            get { return GetProperty(PropertyInputMin); }
            set { SetProperty(PropertyInputMin, value); }
        }

        /// <summary>Gets or sets the input maximum value. 获取或设置输入最大值。</summary>
        public float InputMax
        {
            get { return GetProperty(PropertyInputMax); }
            set { SetProperty(PropertyInputMax, value); }
        }

        /// <summary>Gets or sets the output minimum value. 获取或设置输出最小值。</summary>
        public float OutputMin
        {
            get { return GetProperty(PropertyOutputMin); }
            set { SetProperty(PropertyOutputMin, value); }
        }

        /// <summary>Gets or sets the output maximum value. 获取或设置输出最大值。</summary>
        public float OutputMax
        {
            get { return GetProperty(PropertyOutputMax); }
            set { SetProperty(PropertyOutputMax, value); }
        }

        /// <summary>Gets or sets the percentage of pixels to discard. 获取或设置丢弃像素百分比。</summary>
        public float P
        {
            get { return GetProperty(PropertyP); }
            set { SetProperty(PropertyP, value); }
        }

        /// <summary>Creates a simple white balancer. 创建简单白平衡器。</summary>
        public static SimpleWB Create()
        {
            return new SimpleWB();
        }

        /// <inheritdoc />
        protected override void ValidateBalanceWhiteSource(Mat src)
        {
            if (src.Empty)
            {
                throw new ArgumentException("SimpleWB requires a non-empty source image.", nameof(src));
            }

            int depth = MatType.Depth(src.Type);
            if (depth != MatType.CV_8U &&
                depth != MatType.CV_16S &&
                depth != MatType.CV_32S &&
                depth != MatType.CV_32F)
            {
                throw new ArgumentException("SimpleWB requires a CV_8U, CV_16S, CV_32S, or CV_32F source image depth.", nameof(src));
            }
        }

        private float GetProperty(int propertyId)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.XPhotoSimpleWBGetProperty(NativeHandle, propertyId, out float value));
            return value;
        }

        private void SetProperty(int propertyId, float value)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.XPhotoSimpleWBSetProperty(NativeHandle, propertyId, value));
        }

        private static NativeWhiteBalancerHandle CreateHandle()
        {
            NativeException.ThrowIfError(NativeMethods.XPhotoSimpleWBCreate(out IntPtr nativeHandle));
            return NativeWhiteBalancerHandle.FromNativePointer(nativeHandle);
        }
    }
}
