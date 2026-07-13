using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Calib3D
{
    /// <summary>
    /// Block-matching stereo correspondence algorithm compatible with OpenCV <c>cv::StereoBM</c>.
    /// 与 OpenCV <c>cv::StereoBM</c> 兼容的块匹配双目视差算法。
    /// </summary>
    public sealed class StereoBM : IDisposable
    {
        /// <summary>
        /// Number of fractional bits used by OpenCV fixed-point disparity maps.
        /// OpenCV 定点视差图使用的小数位数。
        /// </summary>
        public const int DispShift = 4;

        /// <summary>
        /// Scale factor used by OpenCV fixed-point disparity maps.
        /// OpenCV 定点视差图使用的缩放因子。
        /// </summary>
        public const int DispScale = 1 << DispShift;

        private NativeStereoBMHandle handle;
        private bool disposed;

        private StereoBM(IntPtr nativeHandle)
        {
            handle = NativeStereoBMHandle.FromNativePointer(nativeHandle);
        }

        /// <summary>
        /// Gets a value indicating whether this object has been disposed.
        /// 获取此对象是否已经释放。
        /// </summary>
        public bool IsDisposed
        {
            get { return disposed; }
        }

        /// <summary>
        /// Gets or sets the minimum possible disparity value.
        /// 获取或设置最小可能视差值。
        /// </summary>
        public int MinDisparity
        {
            get { return GetInt(NativeMethods.StereoBMGetMinDisparity); }
            set { SetInt(NativeMethods.StereoBMSetMinDisparity, value); }
        }

        /// <summary>
        /// Gets or sets the number of disparity levels.
        /// 获取或设置视差级数。
        /// </summary>
        public int NumDisparities
        {
            get { return GetInt(NativeMethods.StereoBMGetNumDisparities); }
            set { SetInt(NativeMethods.StereoBMSetNumDisparities, value); }
        }

        /// <summary>
        /// Gets or sets the matched block size.
        /// 获取或设置匹配块大小。
        /// </summary>
        public int BlockSize
        {
            get { return GetInt(NativeMethods.StereoBMGetBlockSize); }
            set { SetInt(NativeMethods.StereoBMSetBlockSize, value); }
        }

        /// <summary>
        /// Gets or sets the speckle filtering window size.
        /// 获取或设置斑点过滤窗口大小。
        /// </summary>
        public int SpeckleWindowSize
        {
            get { return GetInt(NativeMethods.StereoBMGetSpeckleWindowSize); }
            set { SetInt(NativeMethods.StereoBMSetSpeckleWindowSize, value); }
        }

        /// <summary>
        /// Gets or sets the maximum disparity variation within a speckle component.
        /// 获取或设置斑点连通域内允许的最大视差变化。
        /// </summary>
        public int SpeckleRange
        {
            get { return GetInt(NativeMethods.StereoBMGetSpeckleRange); }
            set { SetInt(NativeMethods.StereoBMSetSpeckleRange, value); }
        }

        /// <summary>
        /// Gets or sets the maximum allowed left-right disparity difference.
        /// 获取或设置左右一致性检查允许的最大视差差异。
        /// </summary>
        public int Disp12MaxDiff
        {
            get { return GetInt(NativeMethods.StereoBMGetDisp12MaxDiff); }
            set { SetInt(NativeMethods.StereoBMSetDisp12MaxDiff, value); }
        }

        /// <summary>
        /// Gets or sets the pre-filter type.
        /// 获取或设置预滤波类型。
        /// </summary>
        public StereoBMPreFilterType PreFilterType
        {
            get { return (StereoBMPreFilterType)GetInt(NativeMethods.StereoBMGetPreFilterType); }
            set { SetInt(NativeMethods.StereoBMSetPreFilterType, (int)value); }
        }

        /// <summary>
        /// Gets or sets the pre-filter averaging window size.
        /// 获取或设置预滤波平均窗口大小。
        /// </summary>
        public int PreFilterSize
        {
            get { return GetInt(NativeMethods.StereoBMGetPreFilterSize); }
            set { SetInt(NativeMethods.StereoBMSetPreFilterSize, value); }
        }

        /// <summary>
        /// Gets or sets the pre-filter clipping cap.
        /// 获取或设置预滤波裁剪上限。
        /// </summary>
        public int PreFilterCap
        {
            get { return GetInt(NativeMethods.StereoBMGetPreFilterCap); }
            set { SetInt(NativeMethods.StereoBMSetPreFilterCap, value); }
        }

        /// <summary>
        /// Gets or sets the texture threshold.
        /// 获取或设置纹理阈值。
        /// </summary>
        public int TextureThreshold
        {
            get { return GetInt(NativeMethods.StereoBMGetTextureThreshold); }
            set { SetInt(NativeMethods.StereoBMSetTextureThreshold, value); }
        }

        /// <summary>
        /// Gets or sets the uniqueness ratio.
        /// 获取或设置唯一性比例。
        /// </summary>
        public int UniquenessRatio
        {
            get { return GetInt(NativeMethods.StereoBMGetUniquenessRatio); }
            set { SetInt(NativeMethods.StereoBMSetUniquenessRatio, value); }
        }

        /// <summary>
        /// Gets or sets the smaller block size used by the algorithm.
        /// 获取或设置算法使用的较小块大小。
        /// </summary>
        public int SmallerBlockSize
        {
            get { return GetInt(NativeMethods.StereoBMGetSmallerBlockSize); }
            set { SetInt(NativeMethods.StereoBMSetSmallerBlockSize, value); }
        }

        /// <summary>
        /// Gets or sets the valid ROI in the first rectified image.
        /// 获取或设置第一张校正图像中的有效 ROI。
        /// </summary>
        public Rect ROI1
        {
            get { return GetRect(NativeMethods.StereoBMGetROI1); }
            set { SetRect(NativeMethods.StereoBMSetROI1, value); }
        }

        /// <summary>
        /// Gets or sets the valid ROI in the second rectified image.
        /// 获取或设置第二张校正图像中的有效 ROI。
        /// </summary>
        public Rect ROI2
        {
            get { return GetRect(NativeMethods.StereoBMGetROI2); }
            set { SetRect(NativeMethods.StereoBMSetROI2, value); }
        }

        internal IntPtr NativeHandle
        {
            get
            {
                ThrowIfDisposed();
                return handle.DangerousGetHandle();
            }
        }

        /// <summary>
        /// Creates a StereoBM object.
        /// 创建 StereoBM 对象。
        /// </summary>
        /// <param name="numDisparities">The number of disparity levels. 视差级数。</param>
        /// <param name="blockSize">The matched block size. 匹配块大小。</param>
        /// <returns>The created StereoBM object. 创建的 StereoBM 对象。</returns>
        public static StereoBM Create(int numDisparities = 0, int blockSize = 21)
        {
            NativeException.ThrowIfError(NativeMethods.StereoBMCreate(numDisparities, blockSize, out IntPtr nativeHandle));
            return new StereoBM(nativeHandle);
        }

        /// <summary>
        /// Computes a disparity map from a rectified stereo pair.
        /// 根据校正后的双目图像对计算视差图。
        /// </summary>
        /// <param name="left">The left 8-bit single-channel image. 左侧 8 位单通道图像。</param>
        /// <param name="right">The right 8-bit single-channel image. 右侧 8 位单通道图像。</param>
        /// <param name="disparity">The output disparity map. 输出视差图。</param>
        public void Compute(Mat left, Mat right, Mat disparity)
        {
            ThrowIfDisposed();
            ValidateNotNull(left, nameof(left));
            ValidateNotNull(right, nameof(right));
            ValidateNotNull(disparity, nameof(disparity));

            NativeException.ThrowIfError(NativeMethods.StereoBMCompute(NativeHandle, left.NativeHandle, right.NativeHandle, disparity.NativeHandle));
        }

        /// <summary>
        /// Computes and returns a disparity map from a rectified stereo pair.
        /// 根据校正后的双目图像对计算并返回视差图。
        /// </summary>
        /// <param name="left">The left 8-bit single-channel image. 左侧 8 位单通道图像。</param>
        /// <param name="right">The right 8-bit single-channel image. 右侧 8 位单通道图像。</param>
        /// <returns>The output disparity map. 输出视差图。</returns>
        public Mat Compute(Mat left, Mat right)
        {
            var disparity = new Mat();
            try
            {
                Compute(left, right, disparity);
                return disparity;
            }
            catch
            {
                disparity.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Releases the native StereoBM object.
        /// 释放 native StereoBM 对象。
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing && handle != null)
                {
                    handle.Dispose();
                }

                disposed = true;
            }
        }

        private int GetInt(IntGetter getter)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(getter(NativeHandle, out int value));
            return value;
        }

        private void SetInt(Func<IntPtr, int, int> setter, int value)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(setter(NativeHandle, value));
        }

        private Rect GetRect(RectGetter getter)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(getter(NativeHandle, out int x, out int y, out int width, out int height));
            return new Rect(x, y, width, height);
        }

        private void SetRect(RectSetter setter, Rect value)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(setter(NativeHandle, value.X, value.Y, value.Width, value.Height));
        }

        private static void ValidateNotNull<T>(T value, string parameterName)
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }

        private void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        private delegate int RectGetter(IntPtr handle, out int x, out int y, out int width, out int height);

        private delegate int RectSetter(IntPtr handle, int x, int y, int width, int height);

        private delegate int IntGetter(IntPtr handle, out int value);
    }
}
