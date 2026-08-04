using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.BgSegm
{
    /// <summary>
    /// Contrib CNT background subtractor.
    /// contrib CNT 背景减除器。
    /// </summary>
    public sealed class BackgroundSubtractorCNT : BgSegmBackgroundSubtractor
    {
        private const int IntMinPixelStability = 0;
        private const int IntMaxPixelStability = 1;
        private const int IntUseHistory = 2;
        private const int IntIsParallel = 3;

        private BackgroundSubtractorCNT(IntPtr nativeHandle)
            : base(nativeHandle)
        {
        }

        /// <summary>Gets or sets minimum pixel stability. 获取或设置最小像素稳定帧数。</summary>
        public int MinPixelStability
        {
            get { return GetInt(IntMinPixelStability); }
            set
            {
                ThrowIfDisposed();
                ValidateMinPixelStability(value, MaxPixelStability, nameof(value));
                SetInt(IntMinPixelStability, value);
            }
        }

        /// <summary>Gets or sets maximum pixel stability. 获取或设置最大像素稳定帧数。</summary>
        public int MaxPixelStability
        {
            get { return GetInt(IntMaxPixelStability); }
            set
            {
                ThrowIfDisposed();
                ValidateMaxPixelStability(value, MinPixelStability, nameof(value));
                SetInt(IntMaxPixelStability, value);
            }
        }

        /// <summary>Gets or sets whether history credit is used. 获取或设置是否使用历史稳定奖励。</summary>
        public bool UseHistory { get { return GetInt(IntUseHistory) != 0; } set { SetInt(IntUseHistory, value ? 1 : 0); } }

        /// <summary>Gets or sets whether the algorithm runs in parallel. 获取或设置是否并行运行。</summary>
        public bool IsParallel { get { return GetInt(IntIsParallel) != 0; } set { SetInt(IntIsParallel, value ? 1 : 0); } }

        /// <summary>Creates a CNT background subtractor. 创建 CNT 背景减除器。</summary>
        public static BackgroundSubtractorCNT Create(int minPixelStability = 15, bool useHistory = true, int maxPixelStability = 900, bool isParallel = true)
        {
            ValidateMinPixelStability(minPixelStability, maxPixelStability, nameof(minPixelStability));
            ValidateMaxPixelStability(maxPixelStability, minPixelStability, nameof(maxPixelStability));
            NativeException.ThrowIfError(NativeMethods.BgSegmBackgroundSubtractorCNTCreate(minPixelStability, useHistory ? 1 : 0, maxPixelStability, isParallel ? 1 : 0, out IntPtr nativeHandle));
            return new BackgroundSubtractorCNT(nativeHandle);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return IsDisposed
                ? "{Disposed=True}"
                : "{MinPixelStability=" + MinPixelStability
                    + ",MaxPixelStability=" + MaxPixelStability
                    + ",UseHistory=" + UseHistory
                    + ",IsParallel=" + IsParallel + "}";
        }

        private int GetInt(int propertyId)
        {
            NativeException.ThrowIfError(NativeMethods.BgSegmBackgroundSubtractorCNTGetInt(NativeHandle, propertyId, out int value));
            return value;
        }

        private void SetInt(int propertyId, int value)
        {
            NativeException.ThrowIfError(NativeMethods.BgSegmBackgroundSubtractorCNTSetInt(NativeHandle, propertyId, value));
        }

        /// <inheritdoc/>
        protected override void ValidateApplyImage(Mat image)
        {
            if (image.Depth != MatType.CV_8U)
            {
                throw new ArgumentException("BackgroundSubtractorCNT requires an 8-bit image.", nameof(image));
            }
        }

        private static void ValidateMinPixelStability(int value, int maxPixelStability, string parameterName)
        {
            if (value < 1 || value >= maxPixelStability)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Minimum pixel stability must be greater than zero and less than maximum pixel stability.");
            }
        }

        private static void ValidateMaxPixelStability(int value, int minPixelStability, string parameterName)
        {
            if (value <= minPixelStability)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Maximum pixel stability must be greater than minimum pixel stability.");
            }
        }
    }
}
