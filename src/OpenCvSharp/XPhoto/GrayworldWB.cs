using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.XPhoto
{
    /// <summary>
    /// Gray-world white balancer.
    /// 灰度世界白平衡器。
    /// </summary>
    public sealed class GrayworldWB : WhiteBalancer
    {
        /// <summary>Creates a gray-world white balancer. 创建灰度世界白平衡器。</summary>
        public GrayworldWB()
            : base(CreateHandle())
        {
        }

        /// <summary>Gets or sets the saturation threshold. 获取或设置饱和度阈值。</summary>
        public float SaturationThreshold
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.XPhotoGrayworldWBGetSaturationThreshold(NativeHandle, out float value));
                return value;
            }

            set
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.XPhotoGrayworldWBSetSaturationThreshold(NativeHandle, value));
            }
        }

        /// <summary>Creates a gray-world white balancer. 创建灰度世界白平衡器。</summary>
        public static GrayworldWB Create()
        {
            return new GrayworldWB();
        }

        /// <inheritdoc />
        protected override void ValidateBalanceWhiteSource(Mat src)
        {
            if (src.Empty)
            {
                throw new ArgumentException("GrayworldWB requires a non-empty source image.", nameof(src));
            }

            if (!src.IsContinuous)
            {
                throw new ArgumentException("GrayworldWB requires a continuous source image.", nameof(src));
            }

            if (src.Type != MatType.CV_8UC3 && src.Type != MatType.CV_16UC3)
            {
                throw new ArgumentException("GrayworldWB requires a CV_8UC3 or CV_16UC3 source image.", nameof(src));
            }
        }

        private static NativeWhiteBalancerHandle CreateHandle()
        {
            NativeException.ThrowIfError(NativeMethods.XPhotoGrayworldWBCreate(out IntPtr nativeHandle));
            return NativeWhiteBalancerHandle.FromNativePointer(nativeHandle);
        }
    }
}
